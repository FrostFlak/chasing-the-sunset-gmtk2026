using UnityEngine;

namespace Helpers.Tick {
    public abstract class TickableBehaviour : MonoBehaviour, ITick {
        protected virtual void OnEnable() => TickSystem.Instance.Register(this);
        protected virtual void OnDisable() => TickSystem.Instance.Unregister(this);
        public abstract void Tick(float dt);
    }
}