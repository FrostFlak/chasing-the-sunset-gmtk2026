using System.Collections.Generic;
using Helpers;
using UnityEngine;

namespace Gameplay {
    public class GameFlowService : MonoBehaviour {

        [SerializeField] private List<Level> _levels;
        [SerializeField] private LevelContext _levelContext;
        private int _currentLevelIdx;
        private Level CurrentLevel => _levels[_currentLevelIdx];
        
        private void Start() => StartLevel(0);

        private void Update() {
            if (_currentLevelIdx >= _levels.Count)
                return;
            
            // if (CurrentLevel.IsCompleted)
            //     CompleteLevel();
        }

        private void StartLevel(int index) {
            _currentLevelIdx = index;
            Log.Debug($"Started Level [{_currentLevelIdx}]");
            CurrentLevel.Start(_levelContext);
        }

        private void CompleteLevel() {
            Log.Debug($"Completed Level: [{_currentLevelIdx}]");
            _currentLevelIdx++;
            
            if(_currentLevelIdx >= _levels.Count) {
                Log.Debug("Completed All Levels");
                return;
            }

            StartLevel(_currentLevelIdx);
        }
    }
}