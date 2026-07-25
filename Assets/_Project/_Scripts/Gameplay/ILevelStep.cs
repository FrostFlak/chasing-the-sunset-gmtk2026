using System;

namespace Gameplay {
    public interface ILevelStep {
        event Action<ILevelStep, StepResult> OnStepResult;
        void Initialize(LevelContext levelContext);
        void Enter();
    }
}