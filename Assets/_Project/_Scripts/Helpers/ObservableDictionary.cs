using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Helpers {
    public class ObservableDictionary<TKey, TValue> {
        private readonly ConcurrentDictionary<TKey, TValue> _dictionary;
        
        public event Action<TKey, TValue, TValue> OnUpdate;

        public ObservableDictionary(ConcurrentDictionary<TKey, TValue> dictionary) {
            _dictionary = dictionary;
        }
        
        public ICollection<TValue> Values() {
            return _dictionary.Values;
        }

        public bool Set(TKey key, TValue value) {
            bool saved;

            if (_dictionary.TryGetValue(key, out var oldValue)) {
                saved = _dictionary.TryUpdate(key, value, oldValue);
            } else {
                saved = _dictionary.TryAdd(key, value);
            }

            if (saved) { OnUpdate?.Invoke(key, oldValue, value); }

            return saved;
        }

        public bool Get(TKey key, out TValue value) {
            return _dictionary.TryGetValue(key, out value);
        }

        public bool Remove(TKey key) {
            bool removed = false;

            if (_dictionary.TryGetValue(key, out var oldValue)) {
                removed = _dictionary.TryRemove(key, out _); 
            }

            if (removed) { OnUpdate?.Invoke(key, oldValue, oldValue); }

            return removed;
        }
    }
}