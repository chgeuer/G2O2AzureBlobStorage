namespace G2OAzureStorage
{
    using G2O;
    using Owin;
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Hosting;
    using System.Net.Http;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;
    using System;

    public class MEFG2OProxyStartup
    {
        public MEFG2OProxyStartup() { }

        public void Configuration(IAppBuilder app, CompositionContainer compositionContainer)
        {
            // app.UseWelcomePage();
            // app.UseErrorPage();
            // app.UseErrorPage(new ErrorPageOptions
            // {
            //     ShowEnvironment = true,
            //     ShowCookies = true,
            //     ShowExceptionDetails = true,
            //     ShowHeaders = true,
            //     ShowQuery = true,
            //     ShowSourceCode = true
            // });
            // app.Use<AzureStorageProxyOwinMiddleware>();

            var proxyConfig = compositionContainer.GetExportedValue<G2OProxyConfiguration>();
            ContainerUtils.SetSASPolicies(proxyConfig);
            app.EnforeG2OAuthentication(keyResolver: proxyConfig.GetKey);
            this.ConfigureWebApi(app, compositionContainer);
        }

        private void ConfigureWebApi(IAppBuilder app, CompositionContainer compositionContainer)
        {
            var config = new HttpConfiguration
            {
                IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always,
                DependencyResolver = new MEFWebApiIDependencyResolver(compositionContainer)
            };

            config.MapHttpAttributeRoutes();
            config.Routes.Clear();
            config.Routes.MapHttpRoute( name: "DefaultRoute", routeTemplate: "{*segments}", defaults: new { controller = typeof(CdnController).Name.Replace("Controller", string.Empty) });
           // config.Services.Replace(typeof(IHttpControllerSelector), new AlwaysTheSameControllerSelector(config, typeof(CdnController)));

            app.UseWebApi(config);
        }
    }

    //public class AlwaysTheSameControllerSelector : DefaultHttpControllerSelector
    //{
    //    private readonly HttpConfiguration _configuration;
    //    private readonly Type _controllerType;
    //    public AlwaysTheSameControllerSelector(HttpConfiguration configuration, Type controllerType) : base(configuration)
    //    {
    //        this._configuration = configuration;
    //        this._controllerType = controllerType;
    //    }
    //    public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
    //    {
    //        return new HttpControllerDescriptor(_configuration, _controllerType.Name, _controllerType);
    //    }
    //}
}
