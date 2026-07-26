using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Gameplay;
using Helpers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Quests.Photo {
    public class PhotoCamera : MonoBehaviour {

        [Header("References")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private PhotoFrameUI _photoFrame;
        [SerializeField] private string _targetQuestID;

        private readonly List<Bird> _shotBirds = new();

        private void Update() {
            if (Mouse.current.leftButton.wasPressedThisFrame)
                StartCoroutine(CaptureRoutine());
        }

        private IEnumerator CaptureRoutine() {
            yield return new WaitForEndOfFrame();

            Rect rect = _photoFrame.GetScreenRect();

            rect.x = Mathf.Clamp(rect.x, 0, Screen.width);
            rect.y = Mathf.Clamp(rect.y, 0, Screen.height);
            rect.width = Mathf.Clamp(rect.width, 1, Screen.width - rect.x);
            rect.height = Mathf.Clamp(rect.height, 1, Screen.height - rect.y);

            int width = Mathf.RoundToInt(rect.width);
            int height = Mathf.RoundToInt(rect.height);

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(rect, 0, 0);

            texture.Apply();

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.one * 0.5f);

            List<GameObject> capturedObjects = FindCapturedObjects(_photoFrame.GetScreenRect());
            PhotoData photo = new PhotoData(sprite, capturedObjects);

            OnPhotoTaken(photo);
        }

        private List<GameObject> FindCapturedObjects(Rect screenRect) {
            List<GameObject> objects = new();

            Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

            foreach (Renderer renderer in renderers) {
                Vector3 screenPosition = _mainCamera.WorldToScreenPoint(renderer.bounds.center);
                if (screenPosition.z < 0)
                    continue;

                if (screenRect.Contains(screenPosition))
                    objects.Add(renderer.gameObject);
            }

            return objects;
        }
        
        private void OnPhotoTaken(PhotoData photo) {
            foreach (var go in photo.CapturedObjects) {
                if (!go.TryGetComponent<Bird>(out var bird))
                    continue;
                
                if (_shotBirds.Contains(bird))
                    continue;
                
                _shotBirds.Add(bird);
                Log.Debug("Shot Bird");
                AddQuestProgress();
            }
        }

        private void AddQuestProgress() {
            var quest = QuestsService.Instance.GetActiveQuest(_targetQuestID);
            if (quest == null || !quest.IsActive)
                return;

            quest.AddProgress(1);
            AudioService.Instance.PlayCameraSfx();
        }
    }
}