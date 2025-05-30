using UnityEngine;

public class URLParameterHandler
{
    public bool ShouldLoadFromURL => Application.absoluteURL.Contains("board");

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

    public string GetMatchParameter()
    {
        if (!Application.absoluteURL.Contains("match="))
        {
            return string.Empty;
        }

        string matchParam = Application.absoluteURL.Split("match=")[1];
        
        if (matchParam.Contains("&"))
        {
            matchParam = matchParam.Split('&')[0];
        }

        return matchParam;
    }

    public void LogCurrentURL()
    {
        Debug.Log("url:" + Application.absoluteURL);
    }
}
