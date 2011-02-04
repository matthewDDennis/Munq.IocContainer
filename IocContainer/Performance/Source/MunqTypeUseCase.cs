﻿using System;
using Domain;
using Munq;
using Munq.LifetimeManagers;

namespace Performance
{
	[System.ComponentModel.Description("MunqType")]
	public class MunqTypeUseCase : UseCase
	{
		IIocContainer container;
		ILifetimeManager containerlifetime = new ContainerLifetime();
		ILifetimeManager lifetime = null; // new AlwaysNewLifetime();

		public MunqTypeUseCase()
		{
			container = new Container().UsesDefaultLifetimeManagerOf(lifetime);

			container.Register<IWebService, WebService>();

			container.Register<IAuthenticator, Authenticator>();

			container.Register<IStockQuote, StockQuote>();

			container.Register<IDatabase, Database>();

			container.Register<IErrorHandler, ErrorHandler>();

			container.Register<ILogger,Logger>().WithLifetimeManager(containerlifetime);
		}

		public override void Run()
		{
			var webApp = container.Resolve<IWebService>();
			webApp.Execute();
		}
	}
}