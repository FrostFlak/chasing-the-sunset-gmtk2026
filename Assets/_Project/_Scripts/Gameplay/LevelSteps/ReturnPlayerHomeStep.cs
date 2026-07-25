using System;
using UnityEngine;

namespace Gameplay.LevelSteps {
    [Serializable]
    public class ReturnPlayerHomeStep : ILevelStep {
        
        public event Action<ILevelStep, StepResult> OnStepResult;
        private LevelContext _levelContext;
        
        public void Initialize(LevelContext levelContext) => _levelContext = levelContext;

        public void Enter() {
            _levelContext.Player.GetComponent<Rigidbody>().position = _levelContext.SpawnPosition.position;
            OnStepResult?.Invoke(this, StepResult.Success);
        }
    }
}