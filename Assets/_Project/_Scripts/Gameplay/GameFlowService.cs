using System;
using System.Collections.Generic;  
using Helpers;  
using UnityEngine;

namespace Gameplay {
    public class GameFlowService : MonoBehaviour {
        
        [SerializeField] private List<Level> _levels;
        [SerializeField] private LevelContext _levelContext;
        [SerializeField] private int _firstLevelIDX;

        private int _currentLevelIdx;
        public event Action<int, bool> OnLevelStateChanged; 
        
        private void Start() {
            AudioService.Instance.PlayMusic();
            AudioService.Instance.PlayAmbient();
            
            StartLevel(_firstLevelIDX);
        }

        private void StartLevel(int index) {
            _currentLevelIdx = index;
            
            Level level = _levels[_currentLevelIdx];
            level.OnCompleted += OnLevelCompleted;
            level.OnFailed += OnLevelFailed;
            
            Log.Debug($"Started Level [{_currentLevelIdx}]");
            
            level.Start(_levelContext);
        }

        private void OnLevelCompleted(Level level) => FinishCurrentLvl(true);

        private void OnGameCompleted() {
            Log.Debug("Completed All Levels");
        }

        private void OnLevelFailed(Level level) => FinishCurrentLvl(false);

        private void FinishCurrentLvl(bool completed) {
            Level level = _levels[_currentLevelIdx];
            level.OnCompleted -= OnLevelCompleted;
            level.OnFailed -= OnLevelFailed;
            
            Log.Debug($"{(completed ? "Completed" : "Failed")} Level [{_currentLevelIdx}]");
            OnLevelStateChanged?.Invoke(_currentLevelIdx, completed);
            
            _currentLevelIdx++;
            if (_currentLevelIdx >= _levels.Count) {
                OnGameCompleted();
                return;
            }

            StartLevel(_currentLevelIdx);
        }
    }
}