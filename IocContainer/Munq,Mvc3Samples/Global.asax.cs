﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Munq.Mvc3Samples.Models;
using Munq;
using Munq.Mvc3Samples.Controllers;
using System.Web.Security;
using Munq.MVC3;

namespace Munq.Mvc3Samples
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);

		}

		protected void Application_Start()
		{
			DependencyResolver.SetResolver(new MunqDependencyResolver());
			var ioc = MunqDependencyResolver.Container;
			ioc.Register<IMembershipService, AccountMembershipService>();
			ioc.Register<IFormsAuthenticationService, FormsAuthenticationService>();
			ioc.RegisterInstance<MembershipProvider>(Membership.Provider);
			//ioc.Register<AccountController, AccountController>();

			AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);
		}
	}
}