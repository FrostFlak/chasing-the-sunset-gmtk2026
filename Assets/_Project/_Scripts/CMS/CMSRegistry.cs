using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CMSResources {

    #region Interface
    /// <summary>Base interface for JSON config data. Id is auto-assigned from filename.</summary>
    public interface IDataEntity {
        string ID { get; set; }
    }
    #endregion

    #region Registry (Internal)
    internal static class Registry {
        public struct Category {
            public string FolderName;
            public Type DataType;
            public Category(string folder, Type type) { FolderName = folder; DataType = type; }
        }

        private static readonly List<Category> _categories = new();
        private static readonly Dictionary<string, IDataEntity> _dataCache = new();
        private static readonly Dictionary<string, Object> _assetCache = new();
        private static bool _isLoaded;
        private static readonly object _lock = new object();

        public static IReadOnlyList<Category> Categories => _categories.AsReadOnly();

        public static void BuildCategories() {
            if (_categories.Count > 0) return;
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                .Where(t => typeof(IDataEntity).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            foreach (var type in types)
                RegisterCategory(type.Name, type);
        }

        public static void RegisterCategory(string folder, Type type) {
            if (!_categories.Exists(c => c.FolderName == folder && c.DataType == type))
                _categories.Add(new Category(folder, type));
        }

        public static void SetDefaultStrings(object obj) {
            var type = obj.GetType();
            foreach (var p in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                if (p.PropertyType == typeof(string) && p.CanWrite && p.GetValue(obj) == null)
                    p.SetValue(obj, string.Empty);
            foreach (var f in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                if (f.FieldType == typeof(string) && f.GetValue(obj) == null)
                    f.SetValue(obj, string.Empty);
        }

        private static void AssignId(IDataEntity entry, string filename) {
            var prop = entry.GetType().GetProperty("ID");
            if (prop != null && prop.PropertyType == typeof(string) && prop.CanWrite)
                prop.SetValue(entry, filename);
        }

        public static void LoadAll() {
            lock (_lock) {
                if (_isLoaded) return;
                _isLoaded = true;
                BuildCategories();

                foreach (var cat in Categories) {
                    var textAssets = Resources.LoadAll<TextAsset>($"{CMSConstants.CMS_ROOT}/Configs/{cat.FolderName}");
                    foreach (var ta in textAssets) {
                        try {
                            if (JsonConvert.DeserializeObject(ta.text, cat.DataType) is IDataEntity entry) {
                                AssignId(entry, ta.name);
                                _dataCache[entry.ID] = entry;
                            }
                        } catch (Exception ex) {
                            Debug.LogError($"[CMS] Failed to parse {ta.name}: {ex.Message}");
                        }
                    }
                }

                // Load assets from CMS root (recursively includes subfolders)
                foreach (var asset in Resources.LoadAll<Object>(CMSConstants.CMS_ROOT)) {
                    if (asset is TextAsset) continue;
                    if (!_assetCache.ContainsKey(asset.name))
                        _assetCache[asset.name] = asset;
                }

                // Also load assets from each category's Configs folder
                // (in case assets are placed alongside configs)
                foreach (var cat in Categories) {
                    var configAssets = Resources.LoadAll<Object>($"{CMSConstants.CMS_ROOT}/Configs/{cat.FolderName}");
                    foreach (var asset in configAssets) {
                        if (asset is TextAsset) continue;
                        if (!_assetCache.ContainsKey(asset.name))
                            _assetCache[asset.name] = asset;
                    }
                }
            }
        }

        public static void Reload() {
            Clear();
#if UNITY_EDITOR
            if (System.IO.Directory.Exists(CMSConstants.LocalCMSPath))
                UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceUpdate);
#endif
            LoadAll();
        }

        public static T Get<T>(string id) where T : class {
            LoadAll();
            if (typeof(IDataEntity).IsAssignableFrom(typeof(T))) {
                if (_dataCache.TryGetValue(id, out var d)) return d as T;
                Log.Error($"[CMS] Entry not found: '{id}' (type: {typeof(T).Name}). Verify the JSON file exists in Resources/CMS/Configs/{typeof(T).Name}/.");
                return null;
            }
            if (typeof(Object).IsAssignableFrom(typeof(T))) {
                // Check cache first
                if (_assetCache.TryGetValue(id, out var a)) {
                    if (a is T t) 
                        return t;
                    if (a is GameObject go) {
                        var c = go.GetComponent(typeof(T)) as T;
                        if (c != null) return c;
                    }
                }

                // Fallback: try loading directly from each category folder
                foreach (var cat in Categories) {
                    var direct = Resources.Load<Object>($"{CMSConstants.CMS_ROOT}/{cat.FolderName}/{id}") as T;
                    if (direct != null) return direct;
                }

                // Fallback: try loading from CMS root directly
                {
                    var rootDirect = Resources.Load<Object>($"{CMSConstants.CMS_ROOT}/{id}") as T;
                    if (rootDirect != null) return rootDirect;
                }

                // If T is a component (not GameObject), try loading the GameObject and GetComponent
                if (typeof(T) != typeof(GameObject) && typeof(Component).IsAssignableFrom(typeof(T))) {
                    foreach (var cat in Categories) {
                        var go = Resources.Load<GameObject>($"{CMSConstants.CMS_ROOT}/{cat.FolderName}/{id}");
                        if (go != null) {
                            var comp = go.GetComponent(typeof(T)) as T;
                            if (comp != null) return comp;
                        }
                    }
                    var rootGo = Resources.Load<GameObject>($"{CMSConstants.CMS_ROOT}/{id}");
                    if (rootGo != null) {
                        var comp = rootGo.GetComponent(typeof(T)) as T;
                        if (comp != null) return comp;
                    }
                }
            }
            return null;
        }

        public static IReadOnlyDictionary<string, T> GetAll<T>() {
            LoadAll();
            var result = new Dictionary<string, T>();
            if (typeof(IDataEntity).IsAssignableFrom(typeof(T))) {
                foreach (var kv in _dataCache) if (kv.Value is T t) result[kv.Key] = t;
            } else if (typeof(Object).IsAssignableFrom(typeof(T))) {
                foreach (var kv in _assetCache) if (kv.Value is T t) result[kv.Key] = t;
            }
            return result;
        }

        public static void Clear() {
            _dataCache.Clear();
            _assetCache.Clear();
            _isLoaded = false;
        }
    }
    #endregion
}
