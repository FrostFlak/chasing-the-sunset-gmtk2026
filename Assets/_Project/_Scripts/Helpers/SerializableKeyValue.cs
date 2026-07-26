using System;

namespace Helpers {
    [Serializable]
    public class SerializableKeyValue<TKey, TValue> {
        public TKey Key;
        public TValue Value;
    }
}