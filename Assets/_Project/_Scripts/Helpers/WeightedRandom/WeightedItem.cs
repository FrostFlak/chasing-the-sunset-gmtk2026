using System;
using UnityEngine;

namespace Helpers.WeightedRandom {
    [Serializable]
    public class WeightedItem<T> {
        public T Item;
        [Min(0f)] public float Weight;

        public WeightedItem(T item, float weight) {
            Item = item;
            Weight = weight;
        }
    }

}