using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bonsai.Code.Infrastructure;

/// <summary>
/// Helper utilities for configuring the runtime.
/// </summary>
public static class RuntimeHelper
{
    static RuntimeHelper()
    {
        var rootAsm = Assembly.GetEntryAssembly();
        var namePrefix = rootAsm.FullName.Split([", "], StringSplitOptions.None)[0];
        ForceLoadReferences(rootAsm, namePrefix);
        OwnAssemblies = AppDomain.CurrentDomain
                                 .GetAssemblies()
                                 .Where(x => x.FullName?.StartsWith(namePrefix) == true).ToList();
    }

    /// <summary>
    /// List of all assemblies defined in the current project.
    /// </summary>
    public static readonly IReadOnlyList<Assembly> OwnAssemblies;

    /// <summary>
    /// List of types defined in own this project.
    /// </summary>
    public static IEnumerable<Type> OwnTypes => OwnAssemblies.SelectMany(x => x.GetTypes());

    /// <summary>
    /// Instantiates and returns instances of all matching types in all own assemblies.
    /// </summary>
    public static IEnumerable<T> GetAllInstances<T>()
    {
        var targetType = typeof(T);

        foreach (var type in OwnTypes)
            if (type.IsConcrete() && targetType.IsAssignableFrom(type))
                yield return (T)Activator.CreateInstance(type);
    }

    /// <summary>
    /// Checks if the type can be instantiated via CreateInstance.
    /// </summary>
    public static bool IsConcrete(this Type type)
    {
        return !type.IsAbstract
               && !type.IsInterface
               && !type.IsGenericTypeDefinition;
    }

    /// <summary>
    /// Ensures that all recursive references are loaded.
    /// </summary>
    private static void ForceLoadReferences(Assembly asm, string namePrefix)
    {
        var loaded = AppDomain.CurrentDomain.GetAssemblies().ToDictionary(x => x.FullName, x => x);
        LoadRecursively(asm);

        void LoadRecursively(Assembly currAsm)
        {
            var refs = currAsm.GetReferencedAssemblies();
            foreach (var r in refs)
            {
                if (r.FullName?.StartsWith(namePrefix) != true)
                    continue;

                if (!loaded.ContainsKey(r.FullName))
                    loaded[r.FullName] = Assembly.Load(r);

                LoadRecursively(loaded[r.FullName]);
            }
        }
    }
}