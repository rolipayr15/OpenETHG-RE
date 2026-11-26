using System;
using System.Collections.Generic;

public class LocatorKit
{
	private static Dictionary<Type, Type> _singletons;

	private static Dictionary<Type, object> _singletonInstances;

	public static void reset()
	{
	}

	public static void registerSingleton<TConcrete>()
	{
	}

	public static void registerSingleton<TInterface, TConcrete>()
	{
	}

	public static void registerSingleton<TConcrete>(TConcrete instance)
	{
	}

	public static T resolve<T>() where T : class
	{
		return null;
	}
}
