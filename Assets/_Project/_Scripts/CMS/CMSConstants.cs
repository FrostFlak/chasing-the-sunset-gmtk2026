using UnityEngine;

namespace CMSResources {
    internal static class CMSConstants {
        public const string CMS_ROOT = "CMS";
#if UNITY_EDITOR
        private const string CONFIGS_SUB = "Configs";
        private static string ProjectRoot => System.IO.Path.GetDirectoryName(Application.dataPath);
        public static string ConfigsPath => System.IO.Path.Combine(Application.dataPath, "Resources", CMS_ROOT, CONFIGS_SUB);
        public static string LocalConfigsPath => System.IO.Path.Combine(ProjectRoot, "Assets", "Resources", CMS_ROOT, CONFIGS_SUB);
        public static string LocalCMSPath => System.IO.Path.Combine(ProjectRoot, "Assets", "Resources", CMS_ROOT);
#endif
    }
}
