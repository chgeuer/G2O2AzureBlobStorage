namespace G2O
{
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Infrastructure;
    using Owin;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class G2OAuthenticationHandler : AuthenticationHandler<G2OAuthenticationOptions>
    {
        private const string ProblemContextName = "G2OAuthProblem";
        private void SetProblem(string problem) { Context.Set<string>(ProblemContextName, problem); }
        private string GetProblem() { return Context.Get<string>(ProblemContextName); }

        private readonly G2OAuthenticationOptions options;
        public G2OAuthenticationHandler(G2OAuthenticationOptions options)
        {
            this.options = options;
        }

        private void Error(string message)
        {
            Trace.TraceError(string.Format("Unauthorized: {0}", message));
        }

        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            var requestPath = Request.Path.Value;
            Trace.TraceInformation("Checking access for " + requestPath);

            var g2oSignature = Request.GetG2OSignatureHeaders();
            if (g2oSignature == null) 
            { 
                Error(string.Format("Missing {0} or {1} header", G2OExtensions.DATAHEADER, G2OExtensions.SIGNATUREHEADER)); 
                return EmptyTicket(); 
            }

            Trace.TraceInformation("DATA: " + g2oSignature.Data);
            Trace.TraceInformation("SIGN: " + g2oSignature.Value);

            if (!g2oSignature.IsValidDataStructure) 
            { 
                Error(string.Format("{0} header contents invalid", G2OExtensions.DATAHEADER)); 
                return EmptyTicket(); 
            }
            Trace.TraceInformation("Header is valid");
            
            var isLocalHost = IPAddress.IsLoopback(IPAddress.Parse(Request.RemoteIpAddress));
            if (!isLocalHost && Request.RemoteIpAddress != g2oSignature.Fields.EdgeIP.ToString()) 
            { 
                Error("G2O EdgeIP is wrong"); return EmptyTicket(); 
            }

            var timeDelta = Math.Abs(DateTimeOffset.UtcNow.Subtract(g2oSignature.Fields.Time).TotalSeconds);
            if (timeDelta > TimeSpan.FromSeconds(30).TotalSeconds)
            {
                Error("Time out of range"); return EmptyTicket();
            }

            if (!g2oSignature.Validate(
                path: Request.Path.Value, 
                keyResolver: options.Credentials))
            {
                Error("signature is cryprographically invalid"); return EmptyTicket();
            }
            Trace.TraceInformation("Signature is valid");

            var id = new ClaimsIdentity(
                claims: new[] 
                { 
                    new Claim(ClaimTypes.Name, "Akamai"),
                    new Claim(G2OClaimTypes.DataHeader, g2oSignature.Data),
                    new Claim(G2OClaimTypes.SignatureHeader, g2oSignature.Value),
                    new Claim(G2OClaimTypes.RequestPath, requestPath),
                    new Claim(G2OClaimTypes.Version, g2oSignature.Fields.Version.ToString()),
                    new Claim(G2OClaimTypes.Nonce, g2oSignature.Fields.Nonce),
                    new Claim(G2OClaimTypes.EdgeIP, g2oSignature.Fields.EdgeIP),
                    new Claim(G2OClaimTypes.ClientIP, g2oSignature.Fields.ClientIP),
                    new Claim(G2OClaimTypes.UniqueID, g2oSignature.Fields.UniqueID)
                },
                authenticationType: options.AuthenticationType);

            return new AuthenticationTicket(id, new AuthenticationProperties());
        }

        private static AuthenticationTicket EmptyTicket()
        {
            return new AuthenticationTicket(null, (AuthenticationProperties)null);
        }
    }
}