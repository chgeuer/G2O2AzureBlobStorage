
[assembly: Microsoft.Owin.OwinStartup(startupType: typeof(G2OWebSite.Startup))]

namespace G2OWebSite
{
    using Owin;
    using G2OAzureStorage;
    using System;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition;
    using System.Configuration;
    using System.Web.Http.Dispatcher;

    public class Startup
    {
        [Export("GetConfiguration")]
        public Func<string, string> GetConfiguration { get { return _ => ConfigurationManager.AppSettings[_]; } }

        public void Configuration(IAppBuilder app)
        {
            // A CompositionContainer object that can be accessed from multiple threads must set the isThreadSafe parameter to true. 
            // Performance will be slightly slower when isThreadSafe is true, so we recommend that you set this parameter to false in single-threaded scenarios. The default is false.
            const bool willBeAccessedFromMultipleThreads = true;
            var compositionContainer = new CompositionContainer(catalog: new AggregateCatalog(
                    new AssemblyCatalog(this.GetType().Assembly),
                    new AssemblyCatalog(typeof(G2OAzureStorage.G2OProxyConfiguration).Assembly)),
                    isThreadSafe: willBeAccessedFromMultipleThreads);

            throw new NotSupportedException("Currently does not run on web sites");

            new MEFG2OProxyStartup().Configuration(app, compositionContainer);
        }
    }
}