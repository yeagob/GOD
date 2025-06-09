using System.Threading.Tasks;
using UnityEngine;

public class AIOptimizationTester : MonoBehaviour
{
    private OptimizedBoardCreationService _optimizedService;

    void Start()
    {
        _optimizedService = new OptimizedBoardCreationService();
        TestOptimization();
    }

    private async void TestOptimization()
    {
        Debug.Log("Starting AI Optimization Test...");
        
        try
        {
            var gameData = await _optimizedService.CreateBaseGameData("Matemáticas básicas para estudiantes de primaria");
            
            if (gameData != null)
            {
                Debug.Log($"✅ Success! Generated {gameData.questions.Count} questions and {gameData.challenges.Count} challenges");
                Debug.Log($"Title: {gameData.tittle}");
                Debug.Log($"Proposal: {gameData.proposal}");
                
                foreach (var question in gameData.questions)
                {
                    Debug.Log($"Question: {question.statement} (Correct: {question.correctId})");
                }
                
                foreach (var challenge in gameData.challenges)
                {
                    Debug.Log($"Challenge: {challenge}");
                }
            }
            else
            {
                Debug.LogError("❌ Failed to generate content");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Error during test: {ex.Message}");
        }
    }
}
