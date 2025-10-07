using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneHistoryManager : MonoBehaviour
{
    public static SceneHistoryManager Instance;

    private Stack<string> sceneHistory = new Stack<string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log("➡️ Guardando en historial: " + currentScene);
        sceneHistory.Push(currentScene);
        SceneManager.LoadScene(sceneName);
    }


    public void LoadPreviousScene()
    {
        if (sceneHistory.Count > 0)
        {
            string previousScene = sceneHistory.Pop();
            Debug.Log("🔙 Volviendo a: " + previousScene);
            SceneManager.LoadScene(previousScene);
        }
        else
        {
            SceneManager.LoadScene("HomeScene");
        }
    }


    public void ClearHistory()
    {
        sceneHistory.Clear();
    }
}
