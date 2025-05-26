using UnityEngine;
using UnityEngine.SceneManagement;

public class FirebaseTestSceneController : MonoBehaviour
{
    [Header("Scene Management")]
    [SerializeField] private string gameSceneName = "InGame";
    [SerializeField] private bool loadOnStart = false;
    
    private void Start()
    {
        if (loadOnStart)
        {
            CheckURLParameters();
        }
    }
    
    private void CheckURLParameters()
    {
        string url = Application.absoluteURL;
        
        if (url.Contains("firebase-test"))
        {
            Debug.Log("[FirebaseTest] Firebase test mode detected from URL parameters");
            return;
        }
        
        if (url.Contains("multiplayer") || url.Contains("board"))
        {
            Debug.Log("[FirebaseTest] Game mode detected, loading main scene");
            LoadGameScene();
        }
    }
    
    public void LoadGameScene()
    {
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }
    
    public void RestartTest()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void QuitApplication()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
