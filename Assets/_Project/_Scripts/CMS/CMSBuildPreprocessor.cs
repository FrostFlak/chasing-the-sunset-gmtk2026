#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace CMSResources {
    public class CMSBuildPreprocessor : IPreprocessBuildWithReport {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report) {
            CMSIdGenerator.Generate();
        }
    }
}
#endif
