using UnityEngine;

public class URLParameterHandler
{
    public bool ShouldLoadFromURL => Application.absoluteURL.Contains("board");
    public bool IsMultiplayerMode => Application.absoluteURL.Contains("multiplayer");

    public string GetBoardParameter()
    {
        if (!ShouldLoadFromURL)
        {
            return string.Empty;
        }

        string boardParam = Application.absoluteURL.Split("board=")[1];
        
        if (boardParam.Contains("&"))
        {
            boardParam = boardParam.Split('&')[0];
        }

        return boardParam;
    }

    public void LogCurrentURL()
    {
        Debug.Log("url:" + Application.absoluteURL);
    }
}
