using System;
using UnityEngine;

namespace Gameplay.LevelSteps {
    [Serializable]
    public class PlayerInputStep : ILevelStep {

        [SerializeField] private bool _active;
        private LevelContext _levelContext;

        public event Action<ILevelStep> OnCompleted;
        
        public void Initialize(LevelContext levelContext) => _levelContext = levelContext;

        public void Enter() {
            _levelContext.Player.SetInputState(_active);
            OnCompleted?.Invoke(this);
        }
    }
}