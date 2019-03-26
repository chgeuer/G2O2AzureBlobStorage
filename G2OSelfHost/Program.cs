namespace G2OSelfHost
{
    using G2OAzureStorage;
    using Microsoft.Owin.Hosting;
    using Owin;
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Configuration;
    using System.Diagnostics;

    public class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "G2O SelfHost";

            Trace.Listeners.Clear();
            Trace.Listeners.Add(new ConsoleTraceListener());

            var listenUrl = "http://+:82/";
            using (WebApp.Start<SelfhostStartup>(new StartOptions(listenUrl) { }))
            {
                Console.WriteLine("Server running on {0}", listenUrl);
                Console.ReadLine();
            }
        }
    }

    public class SelfhostStartup
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
                    new AssemblyCatalog(typeof(G2OProxyConfiguration).Assembly)),
                    isThreadSafe: willBeAccessedFromMultipleThreads);

            new MEFG2OProxyStartup().Configuration(app, compositionContainer);
        }
    }
}