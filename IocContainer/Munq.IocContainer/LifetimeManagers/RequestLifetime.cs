﻿using System.Web;

namespace Munq.LifetimeManagers
{
	/// <summary>
	/// A lifetime manager that scopes the lifetime of created instances to the duration of the
	/// current HttpRequest.
	/// </summary>
	public class RequestLifetime : ILifetimeManager
	{
		private HttpContextBase testContext;
		private object _lock = new object();

		/// <summary>
		/// Return the HttpContext if running in a web application, the test 
		/// context otherwise.
		/// </summary>
		private HttpContextBase Context
		{
			get
			{
				HttpContextBase context = (HttpContext.Current != null)
								? new HttpContextWrapper(HttpContext.Current)
								: testContext;
				return context;
			}
		}

		#region ILifetimeManage Members
		/// <summary>
		/// Gets the instance from the Request Items, if available, otherwise creates a new
		/// instance and stores in the Request Items.
		/// </summary>
		/// <param name="registration">The creator (registration) to create a new instance.</param>
		/// <returns>The instance.</returns>
		public object GetInstance(IRegistration registration)
		{
			object instance = Context.Items[registration.Key];
			if (instance == null)
			{
				lock (_lock)
				{
					instance = Context.Items[registration.Key];
					if (instance == null)
					{
						instance                   = registration.CreateInstance();
						Context.Items[registration.Key] = instance;
					}
				}
			}

			return instance;
		}
		/// <summary>
		/// Invalidates the cached value.
		/// </summary>
		/// <param name="registration">The Registration which is having its value invalidated</param>
		public void InvalidateInstanceCache(IRegistration registration)
		{
			Context.Items.Remove(registration.Key);
		}

		#endregion

		/// <summary>
		/// Only used for testing.  Has no effect when in web application
		/// </summary>
		/// <param name="context"></param>
		public void SetContext(HttpContextBase context)
		{
			testContext = context;
		}
	}
}
