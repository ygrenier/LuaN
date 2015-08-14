using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.Studio
{

    /// <summary>
    /// Application context
    /// </summary>
    public class AppContext
    {

        /// <summary>
        /// Context
        /// </summary>
        public AppContext(Services.IServiceLocator serviceLocator)
        {
            if (serviceLocator == null)
                throw new ArgumentNullException("serviceLocator");
            this.ServiceLocator = serviceLocator;
        }

        /// <summary>
        /// Define the current context
        /// </summary>
        public static void DefineCurrent(Services.IServiceLocator serviceLocator)
        {
            Current = new AppContext(serviceLocator);
        }

        /// <summary>
        /// Current context
        /// </summary>
        public static AppContext Current { get; private set; }

        /// <summary>
        /// The service locator
        /// </summary>
        public Services.IServiceLocator ServiceLocator { get; private set; }

        /// <summary>
        /// Current service
        /// </summary>
        public Services.IAppService AppService
        {
            get { return _AppService ?? (_AppService = ServiceLocator.GetService<Services.IAppService>()); }
        }
        private Services.IAppService _AppService;

    }

}
