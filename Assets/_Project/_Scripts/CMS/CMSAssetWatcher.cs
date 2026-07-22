#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;

namespace CMSResources {
    /// <summary>
    /// Editor window for all CMS content.
    /// Open via: CMS &gt; Editor
    /// </summary>
    /// <summary>
    /// Watches for file changes in the CMS folder and triggers events.
    /// </summary>
    internal class CMSAssetWatcher : AssetPostprocessor {
        public static event Action OnAssetsChanged;
        private static bool _isProcessing;

        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom) {
            if (_isProcessing) return;

            bool hasCMSChange = imported.Any(p => p.Contains("/Resources/CMS/")) ||
                                deleted.Any(p => p.Contains("/Resources/CMS/")) ||
                                moved.Any(p => p.Contains("/Resources/CMS/")) ||
                                movedFrom.Any(p => p.Contains("/Resources/CMS/"));

            if (hasCMSChange) {
                _isProcessing = true;
                EditorApplication.delayCall += () => {
                    OnAssetsChanged?.Invoke();
                    _isProcessing = false;
                };
            }
        }
    }
}
#endif