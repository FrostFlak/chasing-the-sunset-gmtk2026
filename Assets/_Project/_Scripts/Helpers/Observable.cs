using System;

namespace Helpers {
    public class Observable<T> {
        private T _value;
        
        public event Action<T, T> OnUpdate;

        public Observable(T value) {
            _value = value;
        }
        
        public T Value() {
            return _value;
        }
        
        public T Set(T newVal) {
            var oldVal = _value;
            if (oldVal.Equals(newVal)) { return oldVal; }

            lock (_value) {
                _value = newVal;
            }

            OnUpdate?.Invoke(oldVal, _value);

            return _value;
        }

        public T Set(Func<T, T> setter) {
            var oldVal = _value;

            lock (_value) {
                _value = setter(_value);
            }

            if (!oldVal.Equals(_value)) {
                OnUpdate?.Invoke(oldVal, _value);
            }

            return _value;
        }
    }
}