using System.Collections.Generic;
using UnityEngine;


public class PhotoData {
    public Sprite Sprite { get; private set; }
    public List<GameObject> CapturedObjects { get; private set; }

    public PhotoData(Sprite sprite, List<GameObject> capturedObjects) {
        Sprite = sprite;
        CapturedObjects = capturedObjects;
    }
}