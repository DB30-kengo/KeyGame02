using UnityEngine;

public class RenderTextureScaler : MonoBehaviour
{
    public RenderTexture renderTexture;
    private int lastScreenWidth;
    private int lastScreenHeight;

    void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            ResizeRenderTexture();
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }
    }

    void ResizeRenderTexture()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            renderTexture.width = Screen.width;
            renderTexture.height = Screen.height;
            renderTexture.Create();
        }
    }
}