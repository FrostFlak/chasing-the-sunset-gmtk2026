using System.Collections;
using UnityEngine;

namespace Helpers {
    public class CameraFinder {

        #region Properties
        public Camera Camera { get; private set; }
        #endregion

        #region FindCamera
        public IEnumerator FindCamera() {
            // Not the best solution for finding the main camera, but it's the most workable. The iteration is working around 2-3 times
            while (Camera == null) {
                Camera = Camera.main;

                yield return new WaitForSeconds(0.1f);
            }
        } 
        #endregion
    }
}