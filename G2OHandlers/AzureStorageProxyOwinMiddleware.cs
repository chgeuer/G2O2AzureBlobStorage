namespace G2O
{
    using Microsoft.Owin;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading.Tasks;

    public class AzureStorageProxyOwinMiddleware : OwinMiddleware
    {
        public AzureStorageProxyOwinMiddleware(OwinMiddleware next) : base(next) { }

        public async override Task Invoke(IOwinContext context)
        {
            //if (context.Request.User == null || context.Request.User.Identity.Name != "Akamai")
            //{
            //    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            //    context.Response.ContentType = "text/plain";
            //    await context.Response.WriteAsync("401 Unauthorized: " + context.Request.Uri.AbsoluteUri);
            //    return;
            //}

            Trace.TraceInformation("Begin Request " + context.Request.Path.Value);

            //context.Response.ContentType = "text/plain";
            //await context.Response.WriteAsync("Hello, World! " + context.Request.Uri.AbsoluteUri);
            //if (context.Request.Uri.AbsolutePath.Contains("close"))
            //{
            //    await context.Response.WriteAsync(" close");
            //    await context.Response.Body.FlushAsync();
            //    context.Response.Body.Close();
            //}

            await Next.Invoke(context);

            Trace.TraceInformation("End Request " + context.Request.Path.Value);
        }
    }
}
