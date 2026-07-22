using UnityEngine;

namespace Helpers.Animations {
    public class AnimationEventStateBehaviour : StateMachineBehaviour {
        
        [SerializeField] private AnimationEvents _eventName;
        [field: SerializeField, Range(0f, 1f)] public float TriggerTime { get; private set; }
        
        private AnimationEventReceiver _receiver;
        private bool _hasTriggered;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _receiver = animator.GetComponent<AnimationEventReceiver>();
            _hasTriggered = false;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            float currentTime = stateInfo.normalizedTime % 1f;

            if (!_hasTriggered && currentTime >= TriggerTime) {
                _receiver?.TriggerEvent(_eventName);
                _hasTriggered = true;
            }
        }
    }
}