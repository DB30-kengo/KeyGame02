using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲームのビルド設定とシーン管理のユーティリティ
/// </summary>
public class GameBuildUtility : MonoBehaviour
{
    [Header("Scene Management")]
    [Tooltip("デバッグ情報を表示")]
    public bool showDebugInfo = true;
    
    [ContextMenu("Check Build Settings")]
    public void CheckBuildSettings()
    {
        Debug.Log("[GameBuildUtility] Checking build settings...");
        
        // 現在のシーン情報
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"Current Scene: {currentScene}");
        
        // Build Settingsのシーン一覧
        Debug.Log("Scenes in Build Settings:");
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"  [{i}] {sceneName} ({scenePath})");
        }
        
        // 重要なシーンの存在確認
        CheckSceneExists("HintScene");
        CheckSceneExists("SampleScene");
        CheckSceneExists("MainMenu");
    }
    
    private void CheckSceneExists(string sceneName)
    {
        bool exists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (name == sceneName)
            {
                exists = true;
                break;
            }
        }
        
        string status = exists ? "✓" : "✗";
        string color = exists ? "green" : "red";
        Debug.Log($"<color={color}>{status} {sceneName}</color>");
    }
    
    [ContextMenu("Test Scene Transition")]
    public void TestSceneTransition()
    {
        Debug.Log("[GameBuildUtility] Testing scene transition capabilities...");
        
        // HintSceneへの遷移テスト
        bool canLoadHintScene = Application.CanStreamedLevelBeLoaded("HintScene");
        Debug.Log($"Can load HintScene: {(canLoadHintScene ? "✓" : "✗")}");
        
        if (!canLoadHintScene)
        {
            Debug.LogWarning("HintScene cannot be loaded. Please add it to Build Settings.");
            Debug.Log("Steps to fix:");
            Debug.Log("1. Open File → Build Settings");
            Debug.Log("2. Click 'Add Open Scenes' while HintScene is open");
            Debug.Log("3. Or drag HintScene.unity from Project window to Build Settings");
        }
    }
    
    /// <summary>
    /// PlayerPrefs情報を確認
    /// </summary>
    [ContextMenu("Check PlayerPrefs")]
    public void CheckPlayerPrefs()
    {
        Debug.Log("[GameBuildUtility] PlayerPrefs Status:");
        
        if (PlayerPrefs.HasKey("ReturnScene"))
        {
            string returnScene = PlayerPrefs.GetString("ReturnScene");
            Debug.Log($"  ReturnScene: {returnScene}");
        }
        else
        {
            Debug.Log("  ReturnScene: Not set");
        }
        
        if (PlayerPrefs.HasKey("HintCategory"))
        {
            int category = PlayerPrefs.GetInt("HintCategory");
            Debug.Log($"  HintCategory: {category}");
        }
        else
        {
            Debug.Log("  HintCategory: Not set");
        }
    }
    
    /// <summary>
    /// PlayerPrefsをクリア
    /// </summary>
    [ContextMenu("Clear PlayerPrefs")]
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteKey("ReturnScene");
        PlayerPrefs.DeleteKey("HintCategory");
        PlayerPrefs.Save();
        Debug.Log("[GameBuildUtility] PlayerPrefs cleared.");
    }
    
    /// <summary>
    /// ヒントシステムの統合状況を確認
    /// </summary>
    [ContextMenu("Check Hint System Integration")]
    public void CheckHintSystemIntegration()
    {
        Debug.Log("[GameBuildUtility] Checking hint system integration...");
        
        // CryptoGameManagerの確認
        CryptoGameManager gameManager = FindObjectOfType<CryptoGameManager>();
        if (gameManager != null)
        {
            Debug.Log("✓ CryptoGameManager found");
            
            // ヒント機能の確認
            var hintTransition = gameManager.GetComponent<HintSceneTransition>();
            if (hintTransition != null)
            {
                Debug.Log("✓ HintSceneTransition attached");
            }
            else
            {
                Debug.Log("✗ HintSceneTransition not found");
                Debug.Log("  → Add HintSceneTransition component to CryptoGameManager");
            }
        }
        else
        {
            Debug.Log("✗ CryptoGameManager not found in current scene");
        }
        
        // HintSceneでの確認
        if (SceneManager.GetActiveScene().name == "HintScene")
        {
            GameHintManager hintManager = FindObjectOfType<GameHintManager>();
            HintUIGenerator uiGenerator = FindObjectOfType<HintUIGenerator>();
            
            Debug.Log($"  GameHintManager: {(hintManager != null ? "✓" : "✗")}");
            Debug.Log($"  HintUIGenerator: {(uiGenerator != null ? "✓" : "✗")}");
        }
    }
    
    void Start()
    {
        if (showDebugInfo)
        {
            Debug.Log($"[GameBuildUtility] Scene '{SceneManager.GetActiveScene().name}' loaded.");
            CheckBuildSettings();
        }
    }
}

#if UNITY_EDITOR
/// <summary>
/// エディタ用のヘルパーメニュー
/// </summary>
public static class GameBuildUtilityEditor
{
    [MenuItem("Game Utils/Check Build Settings")]
    public static void CheckBuildSettingsMenu()
    {
        var utility = Object.FindObjectOfType<GameBuildUtility>();
        if (utility == null)
        {
            GameObject go = new GameObject("GameBuildUtility");
            utility = go.AddComponent<GameBuildUtility>();
        }
        utility.CheckBuildSettings();
    }
    
    [MenuItem("Game Utils/Test Scene Transition")]
    public static void TestSceneTransitionMenu()
    {
        var utility = Object.FindObjectOfType<GameBuildUtility>();
        if (utility == null)
        {
            GameObject go = new GameObject("GameBuildUtility");
            utility = go.AddComponent<GameBuildUtility>();
        }
        utility.TestSceneTransition();
    }
    
    [MenuItem("Game Utils/Check Hint System")]
    public static void CheckHintSystemMenu()
    {
        var utility = Object.FindObjectOfType<GameBuildUtility>();
        if (utility == null)
        {
            GameObject go = new GameObject("GameBuildUtility");
            utility = go.AddComponent<GameBuildUtility>();
        }
        utility.CheckHintSystemIntegration();
    }
}
#endif
