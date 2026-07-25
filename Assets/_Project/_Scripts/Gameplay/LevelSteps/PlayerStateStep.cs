using System;
using UnityEngine;

namespace Gameplay.LevelSteps {
    [Serializable]
    public class PlayerStateStep : ILevelStep {

        [SerializeField] private bool _active;
        private LevelContext _levelContext;

        public event Action<ILevelStep, StepResult> OnStepResult;

        public void Initialize(LevelContext levelContext) => _levelContext = levelContext;

        public void Enter() {
            _levelContext.Player.SetState(_active);
            OnStepResult?.Invoke(this, StepResult.Success);
        }
    }
}