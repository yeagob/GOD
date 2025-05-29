using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class BoardDataService
{
    private const string BOARDS_COLLECTION_FILE = "boardsCollection.json";
    private List<string> _boardNames = new List<string>();

    public async Task<List<BoardData>> LoadBoardsData()
    {
        List<BoardData> boards = new List<BoardData>();

        string boardsCollectionJson = await LoadTextFileAsync(BOARDS_COLLECTION_FILE);
        if (string.IsNullOrEmpty(boardsCollectionJson))
        {
            Debug.LogError("Boards collection file is empty or could not be loaded.");
            return boards;
        }

        BoardsCollection boardsCollection = JsonUtility.FromJson<BoardsCollection>(boardsCollectionJson);
        _boardNames.Clear();

        foreach (string boardName in boardsCollection.BoardNames)
        {
            string boardJson = await LoadTextFileAsync($"{boardName}.json");
            if (string.IsNullOrEmpty(boardJson))
            {
                Debug.LogError($"Failed to load board data for {boardName}");
                continue;
            }

            BoardData boardData = JsonUtility.FromJson<BoardData>(boardJson);
            boards.Add(boardData);
            _boardNames.Add(boardName);
        }

        return boards;
    }

    public async Task<BoardData> LoadBoardData(string boardName)
    {
        if (string.IsNullOrEmpty(boardName))
        {
            Debug.LogError("Board name is empty or null.");
            return null;
        }

        string boardJson = await LoadTextFileAsync($"{boardName}.json");

        if (string.IsNullOrEmpty(boardJson))
        {
            Debug.LogError($"Failed to load board data for {boardName}");
            return null;
        }

        BoardData boardData = JsonUtility.FromJson<BoardData>(boardJson);
        return boardData;
    }

    public async Task<BoardData> LoadDefaultBoard(string defaultBoardName)
    {
        string boardJson = await LoadTextFileAsync(defaultBoardName);
        return new BoardData(boardJson);
    }

    public List<string> GetBoardNames()
    {
        return new List<string>(_boardNames);
    }

    private async Task<string> LoadTextFileAsync(string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            using (UnityWebRequest www = UnityWebRequest.Get(filePath))
            {
                var request = www.SendWebRequest();

                while (!request.isDone)
                {
                    await Task.Yield();
                }

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error loading file: " + www.error);
                    return null;
                }
                else
                {
                    return www.downloadHandler.text;
                }
            }
        }
        else
        {
            if (File.Exists(filePath))
            {
                return await Task.Run(() => File.ReadAllText(filePath));
            }
            else
            {
                Debug.LogError("File not found: " + filePath);
                return null;
            }
        }
    }

    [System.Serializable]
    private class BoardsCollection
    {
        public List<string> BoardNames;
    }
}
