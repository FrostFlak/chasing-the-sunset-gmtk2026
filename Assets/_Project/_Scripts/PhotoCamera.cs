using System;
using System.Collections;
using System.Collections.Generic;
using Alchemy.Inspector;
using UnityEngine;


public class PhotoCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private PhotoFrameUI _photoFrame;


    private readonly List<PhotoData> _photos = new();


    public IReadOnlyList<PhotoData> Photos => _photos;


    public event Action<PhotoData> OnPhotoTaken;


    private void Update() {
        if (Input.GetMouseButtonDown(0))
            TakePhoto();
    }

    public void TakePhoto()
    {
        StartCoroutine(CaptureRoutine());
    }


    private IEnumerator CaptureRoutine()
    {
        yield return new WaitForEndOfFrame();


        Rect rect = _photoFrame.GetScreenRect();


        rect.x = Mathf.Clamp(
            rect.x,
            0,
            Screen.width
        );

        rect.y = Mathf.Clamp(
            rect.y,
            0,
            Screen.height
        );


        rect.width = Mathf.Clamp(
            rect.width,
            1,
            Screen.width - rect.x
        );

        rect.height = Mathf.Clamp(
            rect.height,
            1,
            Screen.height - rect.y
        );


        // Convert to ReadPixels coordinates

        int width = Mathf.RoundToInt(rect.width);
        int height = Mathf.RoundToInt(rect.height);


        Texture2D texture = new Texture2D(
            width,
            height,
            TextureFormat.RGB24,
            false
        );


        texture.ReadPixels(
            rect,
            0,
            0
        );


        texture.Apply();


        Sprite sprite = Sprite.Create(
            texture,
            new Rect(
                0,
                0,
                width,
                height
            ),
            Vector2.one * 0.5f
        );


        List<GameObject> capturedObjects =
            FindCapturedObjects(
                _photoFrame.GetScreenRect()
            );


        PhotoData photo = new PhotoData(sprite, capturedObjects);
        _photos.Add(photo);

        OnPhotoTaken?.Invoke(photo);
    }
    
    private List<GameObject> FindCapturedObjects(Rect screenRect)
    {
        List<GameObject> objects = new();


        Renderer[] renderers =
            FindObjectsByType<Renderer>(
                FindObjectsSortMode.None
            );


        foreach (Renderer renderer in renderers)
        {
            Vector3 screenPosition =
                _mainCamera.WorldToScreenPoint(
                    renderer.bounds.center
                );


            // Behind camera
            if (screenPosition.z < 0)
                continue;


            if (screenRect.Contains(screenPosition))
            {
                objects.Add(
                    renderer.gameObject
                );
            }
        }


        return objects;
    }
}