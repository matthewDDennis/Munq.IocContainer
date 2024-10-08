﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Munq.DI
{
	public interface IRegistrationKey
	{
	    Type GetInstanceType();
		bool Equals(object obj);
		int GetHashCode();
	}
	
    public interface IRegistration
    {
        string Id { get; }
        string Name { get; }
        IRegistration WithLifetimeManager(ILifetimeManager manager);
    }
}
