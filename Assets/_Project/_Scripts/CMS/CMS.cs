using System.Collections.Generic;

namespace CMSResources {
    public static class CMS {
        /// <summary>Get content by ID. Works for both JSON data and Unity assets. Auto-loads on first call.</summary>
        public static T Get<T>(string id) where T : class => Registry.Get<T>(id);

        /// <summary>Get all loaded entries of type T. Auto-loads on first call.</summary>
        public static IReadOnlyDictionary<string, T> GetAll<T>() => Registry.GetAll<T>();

        /// <summary>Load all content. Subsequent calls are no-ops unless Clear() was called.</summary>
        public static void LoadAll() => Registry.LoadAll();

        /// <summary>Force reload all content.</summary>
        public static void Reload() => Registry.Reload();

        /// <summary>Clear all loaded data and assets.</summary>
        public static void ClearAll() => Registry.Clear();
    }
}