using System;
using Helpers.Tick;
using UnityEngine;

namespace Helpers {
    public class TickableTimer : ITick {

        #region PrivateFields
        private readonly bool _repeatable;
        private float _duration;
        private float _elapsed;
        private Action _onStart;
        private Action<float> _onProgress;
        private Action _onComplete;
        #endregion

        #region Properties
        public bool IsRunning { get; private set; }
        public float Remaining { get; private set; }
        #endregion

        #region Constructor
        public TickableTimer(bool repeatable = false) => _repeatable = repeatable;
        #endregion

        #region Timer
        public TickableTimer Start(
            float duration,
            Action onStart = null,
            Action<float> onProgress = null,
            Action onComplete = null) {
            _onStart    = onStart;
            _onProgress = onProgress;
            _onComplete = onComplete;

            if (duration <= 0f) {
                _onStart?.Invoke();
                _onProgress?.Invoke(1f);
                _onComplete?.Invoke();
                IsRunning = false;
                Remaining = 0f;
                return this;
            }

            _duration = duration;
            _elapsed  = 0f;
            Remaining = duration;
            IsRunning = true;
            _onStart?.Invoke();
            
            return this;
        }

        public TickableTimer Stop() {
            IsRunning = false;
            Remaining = 0f;
            
            return this;
        }

        public void Tick(float dt) {
            if (!IsRunning)
                return;

            _elapsed += dt;

            if (_elapsed >= _duration) {
                Remaining = 0f;
                _onProgress?.Invoke(1f);
                _onComplete?.Invoke();

                if (_repeatable) {
                    _elapsed = 0f;
                    _onStart?.Invoke();
                } 
                else {
                    IsRunning = false;
                }
            } 
            else {
                Remaining = _duration - _elapsed;
                _onProgress?.Invoke(Mathf.Clamp01(_elapsed / _duration));
            }
        }
        #endregion
    }
}
