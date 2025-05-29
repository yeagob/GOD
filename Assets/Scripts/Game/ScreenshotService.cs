using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class ScreenshotService : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void GeneratePDFFromUnity(string base64Image, string name);

    [SerializeField] private Camera _downloadCamera;

    public void Initialize(Camera downloadCamera)
    {
        _downloadCamera = downloadCamera;
        _downloadCamera.gameObject.SetActive(false);
    }

    public void DownloadBoard(BoardController boardController)
    {
        StartCoroutine(DownloadBoardCoroutine(boardController));
    }

    private IEnumerator DownloadBoardCoroutine(BoardController boardController)
    {
        _downloadCamera.gameObject.SetActive(true);
        boardController.EnableTextTiles(true);

        yield return null;

        RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
        _downloadCamera.targetTexture = renderTexture;
        Texture2D screenShot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);

        _downloadCamera.Render();
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        screenShot.Apply();

        _downloadCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        byte[] bytes = screenShot.EncodeToPNG();
        string base64Image = System.Convert.ToBase64String(bytes);

        GeneratePDFFromUnity(base64Image, boardController.BoardData.tittle);

        yield return null;

        _downloadCamera.gameObject.SetActive(false);
        boardController.EnableTextTiles(false);
    }
}
