using System;
using System.IO;
using UnityEngine;

public class BoardPromptLogger : MonoBehaviour
{
    #region Fields

    private const string LOG_FOLDER_PATH = "Assets/Logs";

    #endregion

    #region Public Methods

    /// <summary>
    /// Logs prompts and responses into a file with the board title and timestamp.
    /// </summary>
    public static void LogBoardCreation(string boardTitle, string prompt1, string response1, string prompt2, string response2)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{boardTitle}_{timestamp}.txt";
        string filePath = Path.Combine(LOG_FOLDER_PATH, fileName);

        string logContent = $"Prompt 1: {prompt1}\nResponse 1: {response1}\n\nPrompt 2: {prompt2}\nResponse 2: {response2}";

        SaveLogToFile(filePath, logContent);
    }

    #endregion

    #region Private Methods
    private static void SaveLogToFile(string filePath, string content)
    {
        Directory.CreateDirectory(LOG_FOLDER_PATH); // Ensure the directory exists
        File.WriteAllText(filePath, content);
    }
    #endregion
}
