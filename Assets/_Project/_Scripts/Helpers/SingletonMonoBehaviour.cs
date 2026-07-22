using UnityEngine;

namespace Helpers {
    [DefaultExecutionOrder(-9999)]
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour {

        #region SerializedFields
        [SerializeField] private bool _DDOL;
        #endregion
        
        #region Instance
        public static T Instance { get; private set; }
        #endregion

        #region MonoBehavior
        protected virtual void Awake() => SetInstance();
        protected virtual void OnDestroy() => Destroy(this);
        protected virtual void OnDisable() => Destroy(this);
        #endregion

        #region Instance
        private void SetInstance() {
            if (Instance != null && Instance != this)
                Destroy(this);
            else {
                Instance = this as T;
                if (_DDOL)
                    DontDestroyOnLoad(this);
            }
        }
        #endregion
    }
}