namespace G2O
{
    using Microsoft.Owin;
    using Microsoft.Owin.Security.Infrastructure;

    public class G2OAuthenticationMiddleware : AuthenticationMiddleware<G2OAuthenticationOptions>
    {
        public G2OAuthenticationMiddleware(OwinMiddleware next, G2OAuthenticationOptions options) : base(next, options) { }

        protected override AuthenticationHandler<G2OAuthenticationOptions> CreateHandler()
        {
            return new G2OAuthenticationHandler(Options);
        }
    }
}
