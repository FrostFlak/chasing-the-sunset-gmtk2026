using System;
using System.Collections.Generic;
using UnityEngine;
namespace Helpers.ExtMethods {
    public static class DistanceHelper {
        /// <summary>
        /// Finds the closest object in a list of components.
        /// </summary>
        /// <typeparam name="T">Any component type (GameObject, Transform, Generator, etc.)</typeparam>
        /// <param name="origin">The point to measure from.</param>
        /// <param name="targets">The list of objects to check.</param>
        /// <returns>The closest object of type T, or null if the list is empty.</returns>
        public static T GetClosest<T>(Vector3 origin, List<T> targets) where T : Component {
            if (targets == null || targets.Count == 0) return null;

            T closest = null;
            float minDistanceSqr = float.PositiveInfinity;

            foreach (T target in targets) {
                if (target == null) 
                    continue;

                Vector3 directionToTarget = target.transform.position - origin;
                float dSqrToTarget = directionToTarget.sqrMagnitude;

                if (dSqrToTarget < minDistanceSqr) {
                    minDistanceSqr = dSqrToTarget;
                    closest = target;
                }
            }

            return closest;
        }
        
        /// <summary>
        /// Finds the closest object in a list of components with condition
        /// </summary>
        /// <typeparam name="T">Any component type (GameObject, Transform, Generator, etc.)</typeparam>
        /// <param name="origin">The point to measure from.</param>
        /// <param name="targets">The list of objects to check.</param>
        /// <returns>The closest object of type T, or null if the list is empty.</returns>
        public static T GetClosest<T>(Vector3 origin, List<T> targets, Predicate<T> condition = null) where T : Component {
            if (targets == null || targets.Count == 0) 
                return null;

            T closest = null;
            float minDistanceSqr = float.PositiveInfinity;

            for (int i = 0; i < targets.Count; i++) {
                T target = targets[i];
                if (target == null) 
                    continue;

                if (condition != null && !condition(target)) 
                    continue;

                Vector3 directionToTarget = target.transform.position - origin;
                float dSqrToTarget = directionToTarget.sqrMagnitude;

                if (dSqrToTarget < minDistanceSqr) {
                    minDistanceSqr = dSqrToTarget;
                    closest = target;
                }
            }

            return closest;
        }
    }
}