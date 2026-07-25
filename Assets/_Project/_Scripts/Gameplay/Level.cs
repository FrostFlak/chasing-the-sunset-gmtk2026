using System; using System.Collections.Generic; using Helpers; using UnityEngine;

namespace Gameplay {
    [Serializable] public class Level {
        [SerializeReference] private List<ILevelStep> _steps = new();
        private int _currentStep;
        public event Action<Level> OnCompleted;
        public event Action<Level> OnFailed;

        public void Start(LevelContext levelContext) {
            _currentStep = 0;
            foreach (var step in _steps) {
                step.Initialize(levelContext);
                step.OnStepResult += OnStepResult;
            }

            if (_steps.Count == 0) {
                OnCompleted?.Invoke(this);
                return;
            }

            StartNextStep();
        }

        private void StartNextStep() {
            if (_currentStep >= _steps.Count) return;
            Log.Debug($"Starting step [{_currentStep}] - [{_steps[_currentStep]}]");
            _steps[_currentStep].Enter();
        }

        private void OnStepResult(ILevelStep step, StepResult result) {
            if (step != _steps[_currentStep]) return;
            step.OnStepResult -= OnStepResult;
            Log.Debug($"Step [{_currentStep}] result: {result}");
            if (result == StepResult.Success) {
                _currentStep++;
                if (_currentStep >= _steps.Count) {
                    OnCompleted?.Invoke(this);
                    return;
                }

                StartNextStep();
            }
            else if (result == StepResult.Failure) {
                OnFailed?.Invoke(this);
            }
        }
    }
}