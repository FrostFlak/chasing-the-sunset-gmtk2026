using Helpers.ExtMethods;
using UnityEngine;

namespace Player {
    public class PlayerFootsteps : MonoBehaviour {
        
        [Header("SFX")]
        [SerializeField] private AudioSource _footstepsSource;
        [SerializeField] private AudioClip[] _footstepsSfx;

        public void PlayRandom() {
            _footstepsSource.pitch = Random.Range(0.95f, 1.05f);
            _footstepsSource.PlayOneShot(_footstepsSfx.GetRandom());
        }
    }
}