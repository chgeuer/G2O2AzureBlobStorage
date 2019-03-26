namespace G2OAzureStorage
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.WindowsAzure.Storage;
    using G2O;

    [Export]
    [Authorize]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CdnController : ApiController
    {
        [Import]
        public G2OProxyConfiguration G2OConfiguration { get; set; }


        /// <summary>
        /// Returns a 302 Redirect into Azure Blob Storage.
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        [HttpGet] 
        public IHttpActionResult Get(string segments) 
        { 
            return GetRedirect(segments); 
        }

        ///// <summary>
        ///// Fetches the data from Blob Storage and returns a Stream. 
        ///// </summary>
        ///// <param name="segments"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public Task<HttpResponseMessage> GetAsync(string segments) 
        //{ 
        //    return GetDataAsync(segments); 
        //}

        #region Implementation

        private IHttpActionResult GetRedirect(string segments)
        {
            string[] pathSegments = segments.Split(new[] { "/" }, StringSplitOptions.None);
            string alias = pathSegments[0];
            string containerName = pathSegments[1];
            string path = string.Join("/", pathSegments.Skip(2));

            // var userId = this.User.Identity.Name; // Akamai

            var nonce = ((ClaimsIdentity)this.User.Identity).Claims.First(claim => claim.Type == G2OClaimTypes.Nonce);
            Trace.TraceInformation(string.Format("Logged in via nonce {0}", nonce));

            var storage = this.G2OConfiguration.Storage.FirstOrDefault(_ => _.Alias == alias);
            if (storage == null) { return NotFound(); }
            if (!storage.Containers.Any(_ => _ == containerName)) { return NotFound(); }

            var blobClient = storage.CloudStorageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);

            // var sasAdHocUri = ContainerUtils.GetSASAdHoc(container, path, validity: TimeSpan.FromMinutes(55));
            var sasPolicyUri = ContainerUtils.GetSASUsingPolicy(container, path, validity: TimeSpan.FromDays(31));


            bool enforceProtocolEquality = storage.CloudStorageAccount != CloudStorageAccount.DevelopmentStorageAccount;
            var redirectUri = enforceProtocolEquality
                 ? new UriBuilder(sasPolicyUri) { Scheme = this.Request.RequestUri.Scheme, Port = -1 }.Uri // redirect scheme must be equal to inbound protocol
                 : sasPolicyUri;

            Trace.TraceInformation(string.Format("Redirecting to {0}", redirectUri.AbsoluteUri));

            return Redirect(redirectUri);
        }

        private async Task<HttpResponseMessage> GetDataAsync(string segments)
        {
            string[] pathSegments = segments.Split(new[] { "/" }, StringSplitOptions.None);
            string alias = pathSegments[0];
            string containerName = pathSegments[1];
            string path = string.Join("/", pathSegments.Skip(2));

            var storage = this.G2OConfiguration.Storage.FirstOrDefault(_ => _.Alias == alias);
            if (storage == null) { return new HttpResponseMessage(HttpStatusCode.NotFound); }
            if (!storage.Containers.Any(_ => _ == containerName)) { return new HttpResponseMessage(HttpStatusCode.NotFound); }

            var blobClient = storage.CloudStorageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            var blob = container.GetBlockBlobReference(path);
            if (!await blob.ExistsAsync()) { return new HttpResponseMessage(HttpStatusCode.NotFound); }

            Stream blobStream = await blob.OpenReadAsync();
            var message = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(blobStream)
            };
            message.Content.Headers.ContentLength = blob.Properties.Length;
            if (!string.IsNullOrEmpty(blob.Properties.ContentType)) { message.Content.Headers.ContentType = new MediaTypeHeaderValue(blob.Properties.ContentType); }
            if (!string.IsNullOrEmpty(blob.Properties.ContentEncoding)) { message.Content.Headers.ContentEncoding.Add(blob.Properties.ContentEncoding); }
            if (!string.IsNullOrEmpty(blob.Properties.CacheControl)) { message.Headers.CacheControl = CacheControlHeaderValue.Parse(blob.Properties.CacheControl); }
            return message;
        }

        #endregion
    }
}