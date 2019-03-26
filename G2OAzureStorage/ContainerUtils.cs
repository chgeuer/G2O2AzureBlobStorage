namespace G2OAzureStorage
{
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public static class ContainerUtils
    {
        internal static Uri GetSASAdHoc(CloudBlobContainer container, string path, TimeSpan validity)
        {
            var blob = container.GetBlockBlobReference(path);

            var sas = blob.GetSharedAccessSignature(
                new SharedAccessBlobPolicy
                {
                    Permissions = SharedAccessBlobPermissions.Read,
                    SharedAccessExpiryTime = DateTimeOffset.UtcNow.Add(validity)
                });

            return new Uri(blob.Uri.AbsoluteUri + sas);
        }

        internal static Uri GetSASUsingPolicy(CloudBlobContainer container, string path, TimeSpan validity)
        {
            var blob = container.GetBlockBlobReference(path);
            var sas = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy { SharedAccessExpiryTime = DateTimeOffset.UtcNow.Add(validity) }, groupPolicyIdentifier: PolicyName);
            return new Uri(blob.Uri.AbsoluteUri + sas);
        }

        internal const string PolicyName = "g2o";

        internal static void SetSASPolicies(G2OProxyConfiguration config)
        {
            Task[] tasks = config.Storage
                .Select(_ => new { CloudBlobClient = _.CloudStorageAccount.CreateCloudBlobClient(), ContainerNames = _.Containers })
                .SelectMany(_ => _.ContainerNames.Select(_.CloudBlobClient.GetContainerReference))
                .Select(cloudBlobContaine => ExecuteUntilSuccessAsync(
                            action: () => SetPermissionAsync(cloudBlobContaine),
                            exceptionHandler: (e) => Trace.TraceInformation(e.Message)
                        ))
                .ToArray();

            Task.WaitAll(tasks.ToArray());
        }

        private static async Task SetPermissionAsync(CloudBlobContainer container)
        {
            try
            {
                if (!await container.ExistsAsync())
                {
                    Trace.TraceError("Container {0} does not exist", container.Uri.AbsoluteUri);
                    return;
                }

                var permissions = await container.GetPermissionsAsync();
                if (!permissions.SharedAccessPolicies.Any(_ => _.Key == PolicyName))
                {
                    Trace.TraceInformation(string.Format("Setting policy for {0}", container.Uri.AbsoluteUri));
                    permissions.SharedAccessPolicies[PolicyName] = new SharedAccessBlobPolicy { Permissions = SharedAccessBlobPermissions.Read, SharedAccessStartTime = DateTimeOffset.UtcNow };
                    await container.SetPermissionsAsync(permissions);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error while setting policy for container {0}: {1}", container.Uri.AbsoluteUri, e.Message);
                throw;
            }
        }

        public static async Task ExecuteUntilSuccessAsync(Func<Task> action, Action<Exception> exceptionHandler = null)
        {
            bool success = false;
            while (!success)
            {

                try
                {
                    await action();
                    success = true;
                }
                catch (Exception ex)
                {
                    if (exceptionHandler != null) { exceptionHandler(ex); }
                }
            }
        }

        internal static Task ForEachAsync<T>(this IEnumerable<T> source, int parallelUploads, Func<T, Task> body)
        {
            return Task.WhenAll(
                Partitioner
                .Create(source)
                .GetPartitions(parallelUploads)
                .Select(partition => Task.Run(async () =>
                {
                    using (partition)
                    {
                        while (partition.MoveNext())
                        {
                            await body(partition.Current);
                        }
                    }
                })));
        }
    }
}
