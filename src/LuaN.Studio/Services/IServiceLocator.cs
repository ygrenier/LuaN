using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.Studio.Services
{

    /// <summary>
    /// Service locator
    /// </summary>
    public interface IServiceLocator
    {
        /// <summary>
        /// Get a service
        /// </summary>
        T GetService<T>() where T : class;

        /// <summary>
        /// Get services based on a type
        /// </summary>
        IEnumerable<T> GetServices<T>();

    }

}
