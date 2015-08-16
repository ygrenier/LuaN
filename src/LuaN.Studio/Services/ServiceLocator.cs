using Autofac;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LuaN.Studio.Services
{
    /// <summary>
    /// Service locator
    /// </summary>
    class ServiceLocator : IServiceLocator
    {
        /// <summary>
        /// Create the service locator
        /// </summary>
        public ServiceLocator()
        {
            BuildContainer();
        }

        /// <summary>
        /// Build the container
        /// </summary>
        void BuildContainer()
        {
            var builder = new ContainerBuilder();

            // Reference all services
            builder.RegisterAssemblyTypes(typeof(ServiceLocator).Assembly)
                .Where(tp => tp.Name.EndsWith("Service"))
                .AsImplementedInterfaces()
                .SingleInstance()
                ;
            
            // Reference all ViewModels
            builder.RegisterAssemblyTypes(typeof(ViewModels.ViewModel).Assembly)
                .Where(tp => tp.Name.EndsWith("ViewModel") && !tp.Name.StartsWith("Dt"))
                .AsSelf()
                .InstancePerDependency()
                ;
            // ShellViewModel is an instance
            builder.RegisterType<ViewModels.ShellViewModel>()
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance()
                ;

            // Reference the service locator
            builder.RegisterInstance<IServiceLocator>(this);
            
            // Create the container
            Container = builder.Build();
        }

        /// <summary>
        /// Get a service
        /// </summary>
        public T GetService<T>() where T : class
        {
            return Container.Resolve<T>();
        }

        /// <summary>
        /// Get services based on a type
        /// </summary>
        public IEnumerable<T> GetServices<T>()
        {
            return Container.ComponentRegistry
                .Registrations
                .Where(r => r.Activator.LimitType.IsSubclassOf(typeof(T)) || typeof(T).IsAssignableFrom(r.Activator.LimitType))
                .Select(r => Container.Resolve(r.Activator.LimitType))
                .OfType<T>()
                ;
        }

        /// <summary>
        /// Container
        /// </summary>
        public IContainer Container { get; private set; }

    }
}
