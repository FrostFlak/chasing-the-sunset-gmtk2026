using System;
using System.Collections.Generic;
using Helpers;
using UnityEngine;

namespace Gameplay {
    [Serializable]
    public class Level {
        
        [SerializeReference] private List<ILevelStep> _steps;
        private int _currentStep;
        
        public event Action<Level> OnCompleted;

        public void Start(LevelContext levelContext) {
            foreach (var step in _steps) {
                step.Initialize(levelContext);
                step.OnCompleted += OnStepCompleted;
            } 

            if (_steps.Count <= 0) 
                return;
            
            StartNextStep();
        }

        private void StartNextStep() {
            if (_currentStep < _steps.Count)
                _steps[_currentStep].Enter();
        }

        private void OnStepCompleted(ILevelStep step) {
            if (_currentStep == _steps.Count)
                return;
            
            Log.Debug($"Completed step [{_currentStep}] - [{_steps[_currentStep]}]");
            StartNextStep();
        }
    }
}