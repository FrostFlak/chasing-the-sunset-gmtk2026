using System;
using System.Collections.Generic;
using UnityEngine;

namespace Helpers.Animations {
    public class AnimationEventReceiver : MonoBehaviour {
        
        private readonly Dictionary<AnimationEvents, Action> _listeners = new();

        public void Subscribe(AnimationEvents animationEvent, Action callback) {
            if (!_listeners.TryAdd(animationEvent, callback))
                _listeners[animationEvent] += callback;
        }

        public void Unsubscribe(AnimationEvents animationEvent, Action callback) {
            if (!_listeners.ContainsKey(animationEvent)) 
                return;
            
            _listeners[animationEvent] -= callback;

            if (_listeners[animationEvent] == null)
                _listeners.Remove(animationEvent);
        }

        public void TriggerEvent(AnimationEvents animationEvent) {
            if (_listeners.TryGetValue(animationEvent, out var callback)) 
                callback?.Invoke();
        }
    }
}