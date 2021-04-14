using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasScaleSetting : MonoBehaviour
{
    public int pixelWidth, pixelHeight;
    private int width, height;
    // Start is called before the first frame update
    void Start()
    {
        width = Screen.width;
        height = Screen.height;
        Screen.SetResolution(width, width * pixelWidth / pixelHeight, true);
    }

    // Update is called once per frame
    void Update()
    {
        if(width != Screen.width || height != Screen.height) {
            width = Screen.width;
            height = Screen.height;
            Screen.SetResolution(width, width * pixelWidth / pixelHeight, true);
        }
    }
}
