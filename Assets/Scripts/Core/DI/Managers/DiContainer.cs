using System;
using System.Collections.Generic;
using System.Reflection;
using Core.DI.Attributes;
using UnityEngine;

namespace Core.DI.Managers
{
    public static class DiContainer
    {
        private static readonly Dictionary<Type, object> Registry = new();
        public static void Register<T>(T instance)
        {
            var type = typeof(T);
            if (Registry.TryAdd(type, instance)) return;
            Registry[type] = instance;
        }
        
        public static T Resolve<T>()
        {
            var type = typeof(T);
            if (Registry.TryGetValue(type, out var instance))
            {
                return (T)instance;
            }
            throw new Exception($"Service {type} not registered.");
        }

        public static void Inject(object target)
        {
            var type = target.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (!Attribute.IsDefined(field, typeof(InjectAttribute))) continue;
                var fieldType = field.FieldType;
                if (Registry.TryGetValue(fieldType, out var service))
                {
                    field.SetValue(target, service);
                }
                else
                {
                    Debug.LogError($"Dependency missing: {fieldType} in {type.Name}");
                }
            }
        }
        public static void Clear()
        {
            Registry.Clear();
        }
    }
}