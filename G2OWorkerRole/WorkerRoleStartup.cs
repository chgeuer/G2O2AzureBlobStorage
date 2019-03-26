namespace G2OWorkerRole
{
    using G2OAzureStorage;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Owin;
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;

    public class WorkerRoleStartup
    {
        [Export("GetConfiguration")]
        public Func<string, string> GetConfiguration { get { return RoleEnvironment.GetConfigurationSettingValue; } }

        public void Configuration(IAppBuilder app)
        {
            // A CompositionContainer object that can be accessed from multiple threads must set the isThreadSafe parameter to true. 
            // Performance will be slightly slower when isThreadSafe is true, so we recommend that you set this parameter to false in single-threaded scenarios. The default is false.
            const bool willBeAccessedFromMultipleThreads = true;
            var compositionContainer = new CompositionContainer(catalog: new AggregateCatalog(
                    new AssemblyCatalog(this.GetType().Assembly),
                    new AssemblyCatalog(typeof(G2OAzureStorage.G2OProxyConfiguration).Assembly)),
                    isThreadSafe: willBeAccessedFromMultipleThreads);

            new MEFG2OProxyStartup().Configuration(app, compositionContainer);
        }
    }
}
