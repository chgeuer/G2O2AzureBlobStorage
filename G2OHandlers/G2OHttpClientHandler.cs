namespace G2O
{
    using Owin;
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class G2OHttpClientHandler : HttpClientHandler
    {
        private int Version;
        private string EdgeIP;
        private string ClientIP;
        private string Nonce;
        private string NonceValue;

        public G2OHttpClientHandler(int version, string edgeIP, string clientIP, string nonce, string nonceValue)
        {
            this.Version = version;
            this.EdgeIP = edgeIP;
            this.ClientIP = clientIP;
            this.Nonce = nonce;
            this.NonceValue = nonceValue;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var signature = G2OData.CreateSignature(
                version: this.Version,
                edgeIP: this.EdgeIP,
                clientIP: this.ClientIP,
                time: DateTimeOffset.UtcNow,
                uniqueId: Guid.NewGuid().ToString(),
                nonce: this.Nonce,
                nonceValue: this.NonceValue,
                path: request.RequestUri.LocalPath);

            request.SetG2OSignatureHeaders(signature);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
