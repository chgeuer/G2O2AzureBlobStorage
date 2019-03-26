namespace G2OWorkerRole
{
    using Microsoft.Owin.Hosting;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private IDisposable _app = null;

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 12;

            bool result = base.OnStart();
            Trace.TraceInformation("G2OWorkerRole has been started");

            var endpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["http"];
            string listenUrl = string.Format("{0}://{1}/", endpoint.Protocol, endpoint.IPEndpoint); //  "http://+:80/";
            _app = WebApp.Start<WorkerRoleStartup>(new StartOptions(listenUrl) { });
            Trace.TraceInformation("Server running on {0}", listenUrl);

            return result;
        }

        public override void Run()
        {
            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }
        
        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }


        public override void OnStop()
        {
            Trace.TraceInformation("G2OWorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            if (_app != null)
            {
                _app.Dispose();
            }

            Trace.TraceInformation("G2OWorkerRole has stopped");
        }
    }
}