using System;
using UnityEngine;

namespace Gameplay.LevelSteps {
    [Serializable]
    public class IncreaseAgeStep : ILevelStep {
        
        [SerializeField] private int _age;
        private LevelContext _levelContext;
        
        public event Action<ILevelStep, StepResult> OnStepResult;
        
        public void Initialize(LevelContext levelContext) => _levelContext = levelContext;

        public void Enter() {
            _levelContext.Player.Age = _age;
            OnStepResult?.Invoke(this, StepResult.Success);
        }
    }
}