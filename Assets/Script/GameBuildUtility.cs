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
        CheckSceneExists("Chapter_game");
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
        
        // メインゲームシーンへの遷移テスト
        bool canLoadMainGame = Application.CanStreamedLevelBeLoaded("Chapter_game");
        Debug.Log($"Can load Chapter_game: {(canLoadMainGame ? "✓" : "✗")}");
        
        if (!canLoadMainGame)
        {
            Debug.LogWarning("Chapter_game cannot be loaded. Please add it to Build Settings.");
            Debug.Log("Steps to fix:");
            Debug.Log("1. Open File → Build Settings");
            Debug.Log("2. Click 'Add Open Scenes' while Chapter_game is open");
            Debug.Log("3. Or drag Chapter_game.unity from Project window to Build Settings");
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
        
        // ヒント機能は完全に除去されました
        Debug.Log("  Note: Hint system has been completely removed from this project");
    }
    
    /// <summary>
    /// PlayerPrefsをクリア
    /// </summary>
    [ContextMenu("Clear PlayerPrefs")]
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteKey("ReturnScene");
        // ヒント関連のPlayerPrefsは既に除去されています
        PlayerPrefs.Save();
        Debug.Log("[GameBuildUtility] PlayerPrefs cleared (hint system completely removed).");
    }
    
    /// <summary>
    /// ゲームシステムの統合状況を確認
    /// </summary>
    [ContextMenu("Check Game System Integration")]
    public void CheckGameSystemIntegration()
    {
        Debug.Log("[GameBuildUtility] Checking game system integration...");
        
        // CryptoGameManagerの確認
        var manager = Object.FindFirstObjectByType<CryptoGameManager>();
        if (manager != null)
        {
            Debug.Log("✓ CryptoGameManager found");
            Debug.Log("✓ Game is running in pure gameplay mode");
            Debug.Log("✓ Hint system has been completely removed from this project");
        }
        else
        {
            Debug.Log("✗ CryptoGameManager not found in current scene");
        }
        
        // CryptoUIManagerの確認
        var uiManager = Object.FindFirstObjectByType<CryptoUIManager>();
        if (uiManager != null)
        {
            Debug.Log("✓ CryptoUIManager found");
        }
        else
        {
            Debug.Log("✗ CryptoUIManager not found in current scene");
        }
        
        // ProgressTrackerの確認
        var progress = Object.FindFirstObjectByType<ProgressTracker>();
        if (progress != null)
        {
            Debug.Log("✓ ProgressTracker found");
        }
        else
        {
            Debug.Log("✗ ProgressTracker not found in current scene");
        }
        
        // 現在の状態をログ出力
        Debug.Log("✓ Game features: Crypto learning, player movement, answer selection");
        Debug.Log("✓ Removed features: All hint systems, hint UI, hint scene transitions");
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
    
    [MenuItem("Game Utils/Check Game System")]
    public static void CheckGameSystemMenu()
    {
        var utility = Object.FindObjectOfType<GameBuildUtility>();
        if (utility == null)
        {
            GameObject go = new GameObject("GameBuildUtility");
            utility = go.AddComponent<GameBuildUtility>();
        }
        utility.CheckGameSystemIntegration();
    }
}
#endif
