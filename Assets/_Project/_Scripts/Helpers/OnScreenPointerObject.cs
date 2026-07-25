using UnityEngine;
using UnityEngine.UI;

namespace OnScreenPointerPlugin {
    public class OnScreenPointerObject : MonoBehaviour {

        [SerializeField] private RectTransform _uiContainer;
        [SerializeField] private Vector2 _offsetLocal;
        [SerializeField] private bool _moveInCircle;
        [SerializeField, Range(0f, 1f)] public float _circleSizeNormalized = 0.5f;

        [SerializeField] private Sprite _inScreenSprite;
        [SerializeField] private Sprite _outScreenSprite;
        [SerializeField] private Image _uiImagePrefab;

        private Camera _camera;
        private Image _uiImage;
        private bool _isPointerInScreen;

        private int ScreenSizeX => _camera.pixelWidth;
        private int ScreenSizeY => _camera.pixelHeight;

        private Vector2 ScreenMidPoint => new((float)ScreenSizeX / 2, (float)ScreenSizeY / 2);

        private void Start() {
            _camera = Camera.main;
            _uiImage = Instantiate(_uiImagePrefab, _uiContainer, true);
            _uiImage.gameObject.SetActive(true);
        }

        private void OnDestroy() {
            if (_uiImage?.gameObject)
                Destroy(_uiImage.gameObject);
        }

        private void Update() {
            var screenPos = MyScreenPosition(transform);

            _isPointerInScreen = IsPointerInScreen(screenPos);

            if (_isPointerInScreen) {
                _uiImage.sprite = _inScreenSprite;
                _uiImage.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else {
                _uiImage.sprite = _outScreenSprite;
                Vector2 screenPosCentered = (Vector2)screenPos - ScreenMidPoint;

                if (screenPos.z < 0)
                    screenPosCentered *= -1;

                float angle = Mathf.Atan2(screenPosCentered.y, screenPosCentered.x);
                screenPosCentered = PositionPointerObjectOffScreen(angle);
                screenPos = screenPosCentered + ScreenMidPoint;

                _uiImage.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
            }

            screenPos = ClampToOffsetBounds(screenPos);
            _uiImage.transform.position = screenPos;
        }

        private Vector2 PositionPointerObjectOffScreen(float angle) {
            if (_moveInCircle) {
                float smallerDimOfScreen = 0;
                smallerDimOfScreen = Mathf.Min(ScreenSizeX, ScreenSizeY);
                smallerDimOfScreen = smallerDimOfScreen * 0.5f * _circleSizeNormalized;
                float x = Mathf.Cos(angle) * smallerDimOfScreen;
                float y = Mathf.Sin(angle) * smallerDimOfScreen;
                return new Vector2(x, y);
            }
            else {
                float largerDimOfScreen = 0;
                largerDimOfScreen = Mathf.Max(ScreenSizeX, ScreenSizeY);
                largerDimOfScreen *= 2;
                float x = Mathf.Cos(angle) * largerDimOfScreen;
                float y = Mathf.Sin(angle) * largerDimOfScreen;
                return new Vector2(x, y);
            }
        }

        private Vector2 ClampToOffsetBounds(Vector2 screenPos) {
            int x = (int)Mathf.Clamp(screenPos.x, _offsetLocal.x * ScreenSizeX,
                ScreenSizeX - _offsetLocal.x * ScreenSizeX);
            int y = (int)Mathf.Clamp(screenPos.y, _offsetLocal.y * ScreenSizeY,
                ScreenSizeY - _offsetLocal.y * ScreenSizeY);

            return new Vector2(x, y);
        }

        private Vector3 MyScreenPosition(Transform tr) {
            var screenpos = _camera.WorldToScreenPoint(tr.position);
            return screenpos;
        }

        private bool IsPointerInScreen(Vector3 screenPosition) {
            bool isTargetVisible = screenPosition.z > 0 && screenPosition.x > 0 &&
                                   screenPosition.x < _camera.pixelWidth && screenPosition.y > 0 &&
                                   screenPosition.y < _camera.pixelHeight;
            return isTargetVisible;
        }
    }
}