﻿using Axon.Client.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Client.Meta;

public class MetaEvent : IEvent
{
    public MetaEvent(Type type)
    {
        Type = type;
    }

    public Type Type { get; set; }

    public T GetAttribute<T>() where T : Attribute
    {
        return Type.GetCustomAttribute<T>();
    }

    public T[] GetAttributes<T>() where T : Attribute
    {
        return Type.GetCustomAttributes<T>().ToArray();
    }

    public bool HasAttribute<T>() where T : Attribute
    {
        return GetAttribute<T> != null;
    }

    public bool Is<T>()
    {
        return typeof(T).IsAssignableFrom(Type);
    }

    public Object Create()
    {
        return Activator.CreateInstance(Type);
    }

    public T CreateAs<T>()
    {
        return (T)Activator.CreateInstance(Type);
    }
}
