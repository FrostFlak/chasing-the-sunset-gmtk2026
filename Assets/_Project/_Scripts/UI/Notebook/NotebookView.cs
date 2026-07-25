using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Notebook {
    public class NotebookView : MonoBehaviour {
        
        [SerializeField] private Button _closeButton;

        public event Action OnShow;
        public event Action OnHide;
        private void OnEnable() => _closeButton.onClick.AddListener(Hide);
        private void OnDisable() => _closeButton.onClick.RemoveAllListeners();

        public void Show(int page) {
            gameObject.SetActive(true);
            OnShow?.Invoke();
        }

        public void Hide() {
            gameObject.SetActive(false);
            OnHide?.Invoke();
        }
    }
}