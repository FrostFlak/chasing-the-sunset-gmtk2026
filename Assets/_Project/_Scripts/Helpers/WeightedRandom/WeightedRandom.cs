using System.Collections.Generic;
using UnityEngine;
namespace Helpers.WeightedRandom {
    public class WeightedRandom<T> {
        
        private readonly List<WeightedItem<T>> _items = new();
        private readonly float _totalWeight;

        public WeightedRandom(IEnumerable<WeightedItem<T>> items) {
            foreach (var item in items) {
                if (item.Weight <= 0f)
                    return;

                _items.Add(item);
                _totalWeight += item.Weight;
            }
        }

        public T GetRandom() {
            if (_items.Count == 0) {
                Log.Error("No items in weighted list");
                return default;
            }

            float roll = Random.value * _totalWeight;
            float cumulative = 0f;

            foreach (var item in _items) {
                cumulative += item.Weight;
                if (roll <= cumulative)
                    return item.Item;
            }

            return _items[^1].Item;
        }
    }
}