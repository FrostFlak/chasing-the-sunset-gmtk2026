using System.Collections.Generic;
using UnityEngine;

namespace Helpers {
    public class ObjectPool<T> where T : MonoBehaviour {

        private readonly T _prefab;
        private readonly Queue<T> _pool = new();

        public ObjectPool(T prefab, int initialSize, Transform parent = null) {
            _prefab = prefab;
            
            for (int i = 0; i < initialSize; i++) {
                T obj = Object.Instantiate(_prefab, parent);
                obj.gameObject.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        /// <summary>
        /// Get an object from the pool
        /// </summary>
        public T Get(Vector3 position, Quaternion rotation, Transform parent = null) {
            T obj;

            if (_pool.Count > 0)
                obj = _pool.Dequeue();
            else
                obj = Object.Instantiate(_prefab, position, rotation);

            obj.gameObject.transform.position = position;
            obj.gameObject.transform.rotation = rotation;
            obj.transform.SetParent(parent);
            obj.gameObject.SetActive(true);
            return obj;
        }

        /// <summary>
        /// Return an object to the pool
        /// </summary>
        public void Return(T obj) {
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
}