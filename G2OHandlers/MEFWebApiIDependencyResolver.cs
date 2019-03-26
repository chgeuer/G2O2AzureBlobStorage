namespace G2O
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Web.Http.Dependencies;

    public class MEFWebApiIDependencyResolver : IDependencyResolver
    {
        private readonly CompositionContainer _container;

        public MEFWebApiIDependencyResolver(CompositionContainer container)
        {
            _container = container;
        }

        IDependencyScope IDependencyResolver.BeginScope()
        {
            return this;
        }

        object IDependencyScope.GetService(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            var name = AttributedModelServices.GetContractName(serviceType);
            var export = _container.GetExportedValueOrDefault<object>(name);
            return export;
        }

        IEnumerable<object> IDependencyScope.GetServices(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            var exports = _container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
            return exports;
        }

        void IDisposable.Dispose() { }
    }
}
