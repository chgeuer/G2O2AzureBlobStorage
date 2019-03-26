namespace G2O
{
    using Microsoft.Owin.Security;
    using System;

    public class G2OAuthenticationOptions : AuthenticationOptions
    {
        public const string Scheme = "G2O";

        public Func<string, string> Credentials;

        public G2OAuthenticationOptions(Func<string, string> credentials)
            : base(authenticationType: Scheme)
        {
            this.Credentials = credentials;
            // this.AuthenticationMode = AuthenticationMode.Active;
        }
    }
}
