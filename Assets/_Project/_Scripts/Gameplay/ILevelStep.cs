using System;

namespace Gameplay {
    public interface ILevelStep {
        event Action<ILevelStep> OnCompleted;
        void Initialize(LevelContext levelContext);
        void Enter();
    }
}