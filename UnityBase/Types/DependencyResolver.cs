using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityBase.Types
{
    /// <summary>
    /// The dependency resolver allows classes to register themselves as the instance to be used for a given interface.
    /// Other classes can then query the dependency resolver to get the instance to use.
    /// This is essentially doing dependency injection.
    /// </summary>
    public static class DependencyResolver
    {
        internal static readonly Dictionary<Type, object> typeToObjectInstance = new Dictionary<Type, object>();

        static DependencyResolver()
        {
        }

        /// <summary>
        /// Register a class with the dependency resolver as the class implementing interface T.
        /// </summary>
        /// <typeparam name="T">Type of interface to register. This must be an interface.</typeparam>
        /// <param name="instance">Instance of the class that implements the interface.</param>
        public static void RegisterInstance<T>(T instance) where T : class
        {
            var typeToRegister = typeof(T);

            if (instance == null)
            {
                throw new ArgumentNullException("instance", string.Format("Tried to register a null instance for type {0}", typeToRegister.Name));
            }

            // Make sure we are registering an interface
            if (!typeToRegister.GetTypeInfo().IsInterface)
            {
                throw new InvalidOperationException(
                    string.Format("Trying to register object {0} as type {1}, but the type isn't an interface.",
                        instance.GetType().Name, typeToRegister.Name));
            }

            // Prevent registering the same interface twice
            if (typeToObjectInstance.ContainsKey(typeToRegister))
            {
                throw new InvalidOperationException(string.Format("Trying to register object {0} as type {1}, but object {2} is already registered.",
                    instance.GetType().Name,
                    typeToRegister.Name,
                    typeToObjectInstance[typeToRegister]));
            }

            typeToObjectInstance.Add(typeof(T), instance);
        }

        /// <summary>
        /// Unregisters the instance for the specific type.
        /// </summary>
        /// <typeparam name="T">Type of interface to unregister.</typeparam>
        public static void UnregisterInstance<T>() where T : class
        {
            var typeToUnregister = typeof(T);
            typeToObjectInstance.Remove(typeToUnregister);
        }

        /// <summary>
        /// Retrieves a class that implements instance T.
        /// </summary>
        /// <typeparam name="T">Type of interface for which a class should be retrieved.</typeparam>
        /// <returns>Class that was registered for the requested interface.</returns>
        public static T Get<T>() where T : class
        {
            T resolvedDependency = TryGet<T>();

            if (resolvedDependency == null)
            {
                Type typeToRetrieve = typeof(T);
                throw new InvalidOperationException(string.Format("Tried to retrieve a class of interface {0}, but none is registered.", typeToRetrieve.Name));
            }

            return resolvedDependency;
        }

        /// <summary>
        /// Retrieves a class that implements instance T.
        /// If no class is registered, this will return null instead of throwing. 
        /// </summary>
        /// <typeparam name="T">Type of interface for which a class should be retrieved.</typeparam>
        /// <returns>Class that was registered for the requested interface. Null if none is registered.</returns>
        public static T TryGet<T>() where T: class
        {
            Type typeToRetrieve = typeof(T);

            // Make sure we are retrieving an interface
            if (!typeToRetrieve.GetTypeInfo().IsInterface)
            {
                // This still throws, because this condition being true means an incorrect usage of the DependencyResolver
                throw new InvalidOperationException(string.Format("Trying to get an object of type {0}, but the type isn't an interface.", typeToRetrieve.Name));
            }

            // Validate that a class is registered for that interface
            object resolvedDependency;
            if (!typeToObjectInstance.TryGetValue(typeof(T), out resolvedDependency))
            {
                return null;
            }

            return resolvedDependency as T;
        }
    }
}

