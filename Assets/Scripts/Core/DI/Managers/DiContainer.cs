using System;
using System.Collections.Generic;
using System.Reflection;
using Core.DI.Attributes;

namespace Core.DI.Managers
{
    public static class DiContainer
    {
        private const BindingFlags FieldScope = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly Dictionary<Type, object> Registry = new();

        public static void Register<T>(T instance) => Registry[typeof(T)] = instance;

        public static T Resolve<T>()
        {
            if (Registry.TryGetValue(typeof(T), out var instance)) return (T)instance;
            throw new Exception($"Service {typeof(T)} not registered.");
        }

        public static void Inject(object target)
        {
            var type = target.GetType();
            foreach (var field in type.GetFields(FieldScope))
            {
                if (!Attribute.IsDefined(field, typeof(InjectAttribute))) continue;
                if (!Registry.TryGetValue(field.FieldType, out var service))
                    throw new Exception($"Dependency missing: {field.FieldType} in {type.Name}");
                field.SetValue(target, service);
            }
        }

        public static void Clear() => Registry.Clear();
    }
}
