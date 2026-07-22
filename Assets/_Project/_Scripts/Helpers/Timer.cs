using System;
using System.Collections;
using UnityEngine;

namespace Helpers {
    public class Timer {

        #region PrivateFields
        private readonly MonoBehaviour _monoBehaviour;
        private readonly bool _repeatable;
        
        private float _duration;
        private Coroutine _timerCoroutine;
        #endregion

        #region Properties
        public bool IsRunning { get; private set; }
        public float Remaining { get; private set; }
        #endregion

        #region Constructor
        public Timer(MonoBehaviour monoBehaviour, bool repeatable = false) {
            _monoBehaviour = monoBehaviour;
            _repeatable = repeatable;
        }

        #endregion

        #region Timer
        public Timer Start(
            float duration,
            Action onStart = null,
            Action<float> onProgress = null,
            Action onComplete = null
        ) {
            if (_timerCoroutine != null)
                _monoBehaviour.StopCoroutine(_timerCoroutine);

            _duration = duration;
            Remaining = _duration;
            
            _timerCoroutine = _repeatable
                ? _monoBehaviour.StartCoroutine(RepeatableTimerCoroutine(onStart, onProgress, onComplete))
                : _monoBehaviour.StartCoroutine(TimerCoroutine(onStart, onProgress, onComplete));
            
            return this;
        }

        public Timer Stop() {
            if (_timerCoroutine == null) 
                return this;

            IsRunning = false;
            _monoBehaviour.StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
            Remaining = 0f;
            
            return this;
        }

        private IEnumerator TimerCoroutine(
            Action onStart,
            Action<float> onProgress,
            Action onComplete
        ) {
            onStart?.Invoke();
            IsRunning = true;
            
            float elapsed = 0f;
            
            while (elapsed < _duration) {
                elapsed += Time.deltaTime;
                Remaining = Mathf.Max(0, _duration - elapsed);
                onProgress?.Invoke(Mathf.Clamp01(elapsed / _duration));
                
                yield return null;
            }

            onProgress?.Invoke(1f);
            onComplete?.Invoke();
            
            Remaining = 0f;
            IsRunning = false;
        }
        
        private IEnumerator RepeatableTimerCoroutine(
            Action onStart,
            Action<float> onProgress,
            Action onComplete
        ) {
            while (_repeatable) {
                yield return TimerCoroutine(onStart, onProgress, onComplete);
                
                yield return null;
            }
        }
        #endregion
    }
}