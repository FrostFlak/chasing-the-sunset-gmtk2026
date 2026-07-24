using UnityEngine;
using UnityEngine.UI;

public class PhotoGallery : MonoBehaviour
{
    [SerializeField] private PhotoCamera _photoCamera;

    [SerializeField] private Image[] _slots;


    private void OnEnable()
    {
        _photoCamera.OnPhotoTaken += AddPhoto;
    }


    private void OnDisable()
    {
        _photoCamera.OnPhotoTaken -= AddPhoto;
    }


    private void Start()
    {
        Refresh();
    }


    private void AddPhoto(PhotoData photo)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].sprite == null)
            {
                _slots[i].sprite = photo.Sprite;
                _slots[i].enabled = true;

                return;
            }
        }


        Debug.Log("Gallery is full");
    }


    public void Refresh()
    {
        var photos = _photoCamera.Photos;


        for (int i = 0; i < _slots.Length; i++)
        {
            if (i < photos.Count)
            {
                _slots[i].sprite = photos[i].Sprite;
                _slots[i].enabled = true;
            }
            else
            {
                _slots[i].sprite = null;
                _slots[i].enabled = false;
            }
        }
    }
}