using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "OpenAIConfig", menuName = "Game Of Duck/OpenAI Configuration")]
public class OpenAIConfigurationData : ScriptableObject
{
    [SerializeField, Title("OpenAI API Configuration")]
    [InfoBox("Enter your OpenAI API Key here. This will be used for all AI-related operations.")]
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

    public bool IsApiKeyValid => !string.IsNullOrEmpty(_apiKey) && _apiKey.StartsWith("sk-");

    private void OnValidate()
    {
        UpdateApiKeyStatus();
    }

    private void UpdateApiKeyStatus()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _apiKeyStatus = "Not Set";
        }
        else if (!_apiKey.StartsWith("sk-"))
        {
            _apiKeyStatus = "Invalid Format";
        }
        else
        {
            _apiKeyStatus = $"Valid ({_apiKey.Substring(0, 7)}...)";
        }
    }

    [Button("Validate API Key")]
    [PropertySpace(SpaceBefore = 10)]
    private void ValidateApiKey()
    {
        if (IsApiKeyValid)
        {
            Debug.Log("API Key format is valid!");
        }
        else
        {
            Debug.LogWarning("API Key format is invalid. OpenAI API keys should start with 'sk-'");
        }
    }

    [Button("Clear API Key")]
    private void ClearApiKey()
    {
        _apiKey = "";
        UpdateApiKeyStatus();
        Debug.Log("API Key cleared.");
    }
}