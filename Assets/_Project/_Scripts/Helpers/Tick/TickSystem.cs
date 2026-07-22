using System.Collections.Generic;
using UnityEngine;

namespace Helpers.Tick {
    public class TickSystem : SingletonMonoBehaviour<TickSystem> {
        
        [SerializeField] private float _tickInterval = 0.1f;

        private readonly List<ITick> _tickables = new();
        private readonly List<ITick> _toAdd = new();
        private readonly List<ITick> _toRemove = new();

        private float _accumulator;
        private bool _isTicking;

        public void Register(ITick tickable) {
            if (_isTicking) {
                if (!_toAdd.Contains(tickable))
                    _toAdd.Add(tickable);
            }
            else {
                if (!_tickables.Contains(tickable))
                    _tickables.Add(tickable);
            }
        }

        public void Unregister(ITick tickable) {
            if (_isTicking) {
                if (!_toRemove.Contains(tickable))
                    _toRemove.Add(tickable);
            }
            else {
                _tickables.Remove(tickable);
            }
        }

        private void Update() {
            _accumulator += Time.deltaTime;

            while (_accumulator >= _tickInterval) {
                _accumulator -= _tickInterval;
                RunTick(_tickInterval);
            }
        }

        private void RunTick(float dt) {
            _isTicking = true;

            foreach (var t in _tickables) 
                t.Tick(dt);

            _isTicking = false;

            ApplyPending();
        }

        private void ApplyPending() {
            foreach (var t in _toRemove)
                _tickables.Remove(t);

            foreach (var t in _toAdd) {
                if (!_tickables.Contains(t))
                    _tickables.Add(t);
            }

            _toRemove.Clear();
            _toAdd.Clear();
        }
    }
}