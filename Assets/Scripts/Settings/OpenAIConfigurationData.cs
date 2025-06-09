using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "OpenAIConfig", menuName = "Game Of Duck/OpenAI Configuration")]
public class OpenAIConfigurationData : ScriptableObject
{
    [SerializeField, Title("OpenAI API Configuration")]
    [InfoBox("Enter your OpenAI API Key here. This will be used for all AI-related operations. Leave empty to disable AI board generation.")]
    [PropertySpace(SpaceBefore = 10, SpaceAfter = 10)]
    private string _apiKey = "";

    [SerializeField, ReadOnly]
    [InfoBox("API Key Status", InfoMessageType.None)]
    private string _apiKeyStatus = "Not Set";

    public string ApiKey 
    { 
        get => _apiKey; 
        set 
        { 
            _apiKey = value;
            UpdateApiKeyStatus();
        } 
    }

    public bool HasApiKey => !string.IsNullOrEmpty(_apiKey);

    private void OnValidate()
    {
        UpdateApiKeyStatus();
    }

    private void UpdateApiKeyStatus()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _apiKeyStatus = "Not Set - AI board generation disabled";
        }
        else
        {
            _apiKeyStatus = $"Set ({_apiKey.Substring(0, Mathf.Min(7, _apiKey.Length))}...)";
        }
    }

    [Button("Clear API Key")]
    [PropertySpace(SpaceBefore = 10)]
    private void ClearApiKey()
    {
        _apiKey = "";
        UpdateApiKeyStatus();
        Debug.Log("API Key cleared. AI board generation will be disabled.");
    }
}