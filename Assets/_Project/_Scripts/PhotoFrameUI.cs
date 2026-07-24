using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhotoFrameUI : MonoBehaviour {
    
    [SerializeField] private RectTransform _frame;
    [SerializeField] private RectTransform _canvasRect;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private float _moveSpeed = 15f;

    private void Update() {
        FollowMouse();
    }

    private void FollowMouse() {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            mousePosition,
            _canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : _canvas.worldCamera,
            out Vector2 targetPosition
        );


        // Keep current size, only move position
        Vector2 clampedPosition = ClampPosition(targetPosition);


        _frame.anchoredPosition = Vector2.Lerp(
            _frame.anchoredPosition,
            clampedPosition,
            Time.deltaTime * _moveSpeed
        );
    }


    private Vector2 ClampPosition(Vector2 position) {
        Vector2 canvasSize = _canvasRect.rect.size;
        Vector2 frameSize = _frame.rect.size;


        float xLimit =
            (canvasSize.x - frameSize.x) * 0.5f;


        float yLimit =
            (canvasSize.y - frameSize.y) * 0.5f;


        return new Vector2(
            Mathf.Clamp(
                position.x,
                -xLimit,
                xLimit
            ),

            Mathf.Clamp(
                position.y,
                -yLimit,
                yLimit
            )
        );
    }

    public Rect GetScreenRect() {
        Vector3[] corners = new Vector3[4];
        _frame.GetWorldCorners(corners);

        Camera cam = null;

        if (_canvas.renderMode is RenderMode.ScreenSpaceCamera or RenderMode.WorldSpace) 
            cam = _canvas.worldCamera;

        Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
        Vector2 topRight = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);

        Rect rect = new Rect(
            bottomLeft.x,
            bottomLeft.y,
            topRight.x - bottomLeft.x,
            topRight.y - bottomLeft.y
        );

        return rect;
    }
}