using System;
using System.Collections.Generic;

public static class ServiceLocator
{
    static readonly Dictionary<Type, object> _services = new();

    public static void Register<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
    }

    public static T Get<T>() where T : class
    {
        _services.TryGetValue(typeof(T), out var service);
        return service as T;
    }

    public static void Clear() => _services.Clear();
}
