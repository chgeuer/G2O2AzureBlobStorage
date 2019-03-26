namespace Owin
{
    using G2O;
    using Microsoft.Owin;
    using System;
    using System.Net.Http;

    public static class G2OExtensions
    {
        public static void EnforeG2OAuthentication(this IAppBuilder app, Func<string, string> keyResolver)
        {
            app.Use(typeof(G2OAuthenticationMiddleware), new G2OAuthenticationOptions(keyResolver));
        }

        internal const string DATAHEADER = "X-Akamai-G2O-Auth-Data";
        internal const string SIGNATUREHEADER = "X-Akamai-G2O-Auth-Sign";

        public static void SetG2OSignatureHeaders(this HttpRequestMessage request, G2OData.Signature signature)
        {
            request.Headers.Add(DATAHEADER, signature.Data);
            request.Headers.Add(SIGNATUREHEADER, signature.Value);
        }

        public static G2OData.Signature GetG2OSignatureHeaders(this IOwinRequest request)
        {
            var g2oDataHeader = request.Headers.Get(DATAHEADER);
            if (string.IsNullOrEmpty(g2oDataHeader)) { return null; }

            var g2oSignatureHeader = request.Headers.Get(SIGNATUREHEADER);
            if (string.IsNullOrEmpty(g2oSignatureHeader)) { return null; }

            return new G2OData.Signature(data: g2oDataHeader, value: g2oSignatureHeader);
        }
    }
}