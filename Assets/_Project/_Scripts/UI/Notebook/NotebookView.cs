using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gameplay;
using Helpers;
using Helpers.ExtMethods;
using TMPEffects.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Notebook {
    public class NotebookView : MonoBehaviour {

        [SerializeField] private GameFlowService _gameFlowService;
        [SerializeField] private Button _closeButton;
        [SerializeField] private List<SerializableKeyValue<int, PageData>> _pages;

        public event Action OnShow;
        public event Action OnHide;

        [Serializable]
        public class PageData {
            public int LevelIndex;
            public GameObject CompletedPage;
            public GameObject FailedPage;
            public Button Button;
            public bool IsOpen;
            public bool IsCompleted;
        }

        private void Start() => _gameFlowService.OnLevelStateChanged += OnLevelStateChanged;

        private void OnEnable() {
            _closeButton.onClick.AddListener(Hide);
            _pages.ForEach(pb => pb.Value.Button.onClick.AddListener(() => Show(pb.Key)));
        }
        
        private void OnDisable() {
            _closeButton.onClick.RemoveAllListeners();
            _pages.ForEach(pb => pb.Value.Button.onClick.RemoveAllListeners());
        }
        
        private void OnDestroy() => _gameFlowService.OnLevelStateChanged -= OnLevelStateChanged;

        private void OpenPage(int levelIdx, bool completed) {
            var pageData = _pages[levelIdx + 1].Value;
            pageData.IsOpen = true;
            pageData.IsCompleted = completed;
            pageData.Button.gameObject.SetActive(true);
        }

        public void Show(int pageIdx) {
            foreach (var pg in _pages) {
                if (pg.Key != pageIdx) {
                    if (pg.Value.CompletedPage != null)
                        pg.Value.CompletedPage.SetActive(false);

                    if (pg.Value.FailedPage != null)
                        pg.Value.FailedPage.SetActive(false);
                    
                    continue;
                }

                if (!pg.Value.IsOpen)
                    continue;
                
                if (pg.Value.CompletedPage != null)
                    pg.Value.CompletedPage.SetActive(pg.Value.IsCompleted);
                
                if (pg.Value.FailedPage != null)
                    pg.Value.FailedPage.SetActive(!pg.Value.IsCompleted);
            }
            
            gameObject.SetActive(true);
            OnShow?.Invoke();
        }

        private void Hide() {
            gameObject.SetActive(false);
            OnHide?.Invoke();
        }

        private void OnLevelStateChanged(int levelIdx, bool completed) => OpenPage(levelIdx, completed);
    }
}