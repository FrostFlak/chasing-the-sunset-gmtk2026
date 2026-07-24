using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class FadeScreenUI : MonoBehaviour {
        
        [SerializeField] private Image _image;
        
        public void FadeIn(float duration) {
            _image.DOFade(1, duration).From(0);
        }
        
        public void FadeOut(float duration) {
            _image.DOFade(0, duration).From(1);
        }
    }
}