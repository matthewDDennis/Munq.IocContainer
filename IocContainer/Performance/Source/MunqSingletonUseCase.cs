﻿using System;
using Domain;
using Munq;

namespace Performance
{
	[System.ComponentModel.Description("MunqSingleton")]
	public class MunqSingletonUseCase : UseCase
	{
		static Container container;
		static ILifetimeManager singleton;

		static MunqSingletonUseCase()
		{
			container = new Container();
			singleton = new Munq.LifetimeManagers.ContainerLifetime();

			container.Register<IWebService>(
				c => new WebService(
					c.Resolve<IAuthenticator>(),
					c.Resolve<IStockQuote>()));

			container.Register<IAuthenticator>(
				c => new Authenticator(
					c.Resolve<ILogger>(),
					c.Resolve<IErrorHandler>(),
					c.Resolve<IDatabase>()))
					.WithLifetimeManager(singleton);

			container.Register<IStockQuote>(
				c => new StockQuote(
					c.Resolve<ILogger>(),
					c.Resolve<IErrorHandler>(),
					c.Resolve<IDatabase>()))
					.WithLifetimeManager(singleton);

			container.Register<IDatabase>(
				c => new Database(
					c.Resolve<ILogger>(),
					c.Resolve<IErrorHandler>()))
					.WithLifetimeManager(singleton);

			container.Register<IErrorHandler>(
				c => new ErrorHandler(c.Resolve<ILogger>()))
					.WithLifetimeManager(singleton);

			container.RegisterInstance<ILogger>(new Logger())
					.WithLifetimeManager(singleton);
		}

		public override void Run()
		{
			var webApp = container.Resolve<IWebService>();
			webApp.Execute();
		}
	}
}
