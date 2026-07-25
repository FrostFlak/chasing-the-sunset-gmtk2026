using System;
using System.Collections.Generic;  
using Helpers;  
using UnityEngine;

namespace Gameplay {
    public class GameFlowService : MonoBehaviour {
        
        [SerializeField] private List<Level> _levels;
        [SerializeField] private LevelContext _levelContext;
        
        private int _currentLevelIdx;
        private bool _lastLevelFailed;
        public event Action<bool> OnLevelStatusChanged;

        private void Start() => StartLevel(0);

        private void StartLevel(int index) {
            _currentLevelIdx = index;
            
            Level level = _levels[_currentLevelIdx];
            level.OnCompleted += OnLevelCompleted;
            level.OnFailed += OnLevelFailed;
            
            OnLevelStatusChanged?.Invoke(_lastLevelFailed);
            Log.Debug($"Started Level [{_currentLevelIdx}]");
            
            level.Start(_levelContext);
        }

        private void OnLevelCompleted(Level level) {
            level.OnCompleted -= OnLevelCompleted;
            level.OnFailed -= OnLevelFailed;
            
            Log.Debug($"Completed Level [{_currentLevelIdx}]");
            _lastLevelFailed = false;
            _currentLevelIdx++;
            if (_currentLevelIdx >= _levels.Count) {
                OnGameCompleted();
                return;
            }

            StartLevel(_currentLevelIdx);
        }

        private void OnGameCompleted() {
            Log.Debug("Completed All Levels");
        }

        private void OnLevelFailed(Level level) {
            level.OnCompleted -= OnLevelCompleted;
            level.OnFailed -= OnLevelFailed;
            
            Log.Debug($"Failed Level [{_currentLevelIdx}]");
            
            _lastLevelFailed = true;
            _currentLevelIdx++;
            if (_currentLevelIdx >= _levels.Count) {
                OnGameCompleted();
                return;
            }

            StartLevel(_currentLevelIdx);
        }
    }
}