﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

using Munq.DI;

namespace Munq.MVC
{
    public class MunqControllerFactory : IControllerFactory
    {
        public Container IOC { get; private set; }

        public MunqControllerFactory(Container container)
        {
            IOC = container;
        }

        #region IControllerFactory Members

        public IController CreateController(RequestContext requestContext, string controllerName)
        {
            try
            {
                return IOC.Resolve<IController>(controllerName);
            }
            catch
            {
                return null;
            }
        }

        public void ReleaseController(IController controller)
        {
            var disposable = controller as IDisposable;

            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        #endregion
    }
}
