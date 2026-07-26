using Helpers;
using Helpers.ExtMethods;
using UnityEngine;

namespace Gameplay {
    public class AudioService : SingletonMonoBehaviour<AudioService> {
    
        [Header("Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _envSource;
        [Header("Music")]
        [SerializeField] private AudioClip[] _music;
        [Header("Ambient")]
        [SerializeField] private AudioClip[] _ambient;
        [Header("SFX")]
        [SerializeField] private AudioClip[] _treeHitsSFX;
        [SerializeField] private AudioClip[] _pickupSFX;
        [SerializeField] private AudioClip[] _houseBuildSFX;
        [SerializeField] private AudioClip _cameraShotSfx;
        [SerializeField] private AudioClip _pencilSfx;

        public void PlayTreeHitSFX() => PlaySfx(_treeHitsSFX.GetRandom());
        
        public void PlayPickupSFX() => PlaySfx(_pickupSFX.GetRandom());
        
        public void PlayBuildSFX() => PlaySfx(_houseBuildSFX.GetRandom());
        
        private void PlaySfx(AudioClip clip) {
            _sfxSource.pitch = Random.Range(0.95f, 1.05f);
            _sfxSource.PlayOneShot(clip);
        }

        public void PlayMusic() {
            _musicSource.clip = _music.GetRandom();
            _musicSource.loop = true;
            _musicSource.Play();
        }
        
        public void PlayAmbient() {
            _envSource.clip = _ambient.GetRandom();
            _envSource.loop = true;
            _envSource.Play();
        }

        public void PlayCameraSfx() => PlaySfx(_cameraShotSfx);

        public void PlayPencilSfx() => PlaySfx(_pencilSfx);
    }
}