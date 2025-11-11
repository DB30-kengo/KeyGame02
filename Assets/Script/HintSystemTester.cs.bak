using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// ヒントシステムの統合テストスクリプト
/// 実装が正常に機能するかを自動的に検証
/// </summary>
public class HintSystemTester : MonoBehaviour
{
    [Header("Test Settings")]
    [Tooltip("テスト実行を自動開始")]
    public bool runTestsOnStart = false;
    
    [Tooltip("各テストの間隔（秒）")]
    [Range(1f, 5f)]
    public float testInterval = 2f;
    
    [Header("Test Results")]
    [Tooltip("テスト結果の表示")]
    public bool showResults = true;
    
    [System.Serializable]
    public class TestResult
    {
        public string testName;
        public bool passed;
        public string details;
        
        public TestResult(string name, bool result, string info = "")
        {
            testName = name;
            passed = result;
            details = info;
        }
    }
    
    private System.Collections.Generic.List<TestResult> testResults = new System.Collections.Generic.List<TestResult>();
    
    void Start()
    {
        if (runTestsOnStart)
        {
            StartCoroutine(RunAllTests());
        }
    }
    
    /// <summary>
    /// すべてのテストを実行
    /// </summary>
    [ContextMenu("Run All Tests")]
    public void RunAllTestsMenu()
    {
        StartCoroutine(RunAllTests());
    }
    
    private IEnumerator RunAllTests()
    {
        Debug.Log("=== ヒントシステム統合テスト開始 ===");
        testResults.Clear();
        
        // 1. コンポーネント存在確認
        yield return StartCoroutine(TestComponentsExistence());
        yield return new WaitForSeconds(testInterval);
        
        // 2. Build Settings確認
        yield return StartCoroutine(TestBuildSettings());
        yield return new WaitForSeconds(testInterval);
        
        // 3. ヒント遷移機能確認
        yield return StartCoroutine(TestHintTransition());
        yield return new WaitForSeconds(testInterval);
        
        // 4. GameHintManager機能確認
        yield return StartCoroutine(TestGameHintManager());
        yield return new WaitForSeconds(testInterval);
        
        // 5. UI生成機能確認
        yield return StartCoroutine(TestUIGeneration());
        yield return new WaitForSeconds(testInterval);
        
        // 結果表示
        DisplayTestResults();
        
        Debug.Log("=== ヒントシステム統合テスト完了 ===");
    }
    
    /// <summary>
    /// コンポーネント存在確認テスト
    /// </summary>
    private IEnumerator TestComponentsExistence()
    {
        Debug.Log("[テスト1] コンポーネント存在確認...");
        
        // CryptoGameManager確認
        CryptoGameManager gameManager = FindObjectOfType<CryptoGameManager>();
        testResults.Add(new TestResult("CryptoGameManager存在", gameManager != null, 
            gameManager != null ? "✓ 発見" : "✗ 未発見"));
        
        // HintSceneTransition確認
        HintSceneTransition transition = FindObjectOfType<HintSceneTransition>();
        testResults.Add(new TestResult("HintSceneTransition存在", transition != null,
            transition != null ? "✓ 発見" : "✗ 未発見"));
        
        // CryptoGameManagerにHintSceneTransitionがアタッチされているか
        bool hasTransition = gameManager != null && gameManager.GetComponent<HintSceneTransition>() != null;
        testResults.Add(new TestResult("GameManagerへのTransition統合", hasTransition,
            hasTransition ? "✓ 統合済み" : "✗ 未統合"));
        
        yield return null;
    }
    
    /// <summary>
    /// Build Settings確認テスト
    /// </summary>
    private IEnumerator TestBuildSettings()
    {
        Debug.Log("[テスト2] Build Settings確認...");
        
        // 現在のシーン確認
        string currentScene = SceneManager.GetActiveScene().name;
        testResults.Add(new TestResult("現在のシーン", !string.IsNullOrEmpty(currentScene),
            $"シーン名: {currentScene}"));
        
        // HintSceneの存在確認
        bool hintSceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName == "HintScene")
            {
                hintSceneExists = true;
                break;
            }
        }
        
        testResults.Add(new TestResult("HintScene Build設定", hintSceneExists,
            hintSceneExists ? "✓ Build Settingsに追加済み" : "✗ Build Settingsに未追加"));
        
        // シーンロード可能性確認
        bool canLoadHintScene = Application.CanStreamedLevelBeLoaded("HintScene");
        testResults.Add(new TestResult("HintSceneロード可能", canLoadHintScene,
            canLoadHintScene ? "✓ ロード可能" : "✗ ロード不可"));
        
        yield return null;
    }
    
    /// <summary>
    /// ヒント遷移機能確認テスト
    /// </summary>
    private IEnumerator TestHintTransition()
    {
        Debug.Log("[テスト3] ヒント遷移機能確認...");
        
        HintSceneTransition transition = FindObjectOfType<HintSceneTransition>();
        if (transition == null)
        {
            testResults.Add(new TestResult("遷移機能テスト", false, "HintSceneTransitionが見つからない"));
            yield break;
        }
        
        // 設定値確認
        bool hasCorrectSceneName = transition.hintSceneName == "HintScene";
        testResults.Add(new TestResult("シーン名設定", hasCorrectSceneName,
            $"設定値: {transition.hintSceneName}"));
        
        // 現在のシーン名設定確認
        bool hasCurrentSceneName = !string.IsNullOrEmpty(transition.currentSceneName);
        testResults.Add(new TestResult("現在シーン名設定", hasCurrentSceneName,
            $"現在シーン: {transition.currentSceneName}"));
        
        yield return null;
    }
    
    /// <summary>
    /// GameHintManager機能確認テスト
    /// </summary>
    private IEnumerator TestGameHintManager()
    {
        Debug.Log("[テスト4] GameHintManager機能確認...");
        
        // HintSceneでのテストかどうか確認
        string currentScene = SceneManager.GetActiveScene().name;
        
        if (currentScene == "HintScene")
        {
            GameHintManager hintManager = FindObjectOfType<GameHintManager>();
            testResults.Add(new TestResult("GameHintManager存在", hintManager != null,
                hintManager != null ? "✓ HintSceneで発見" : "✗ HintSceneで未発見"));
            
            if (hintManager != null)
            {
                // ヒントデータベース確認（リフレクション使用）
                System.Reflection.FieldInfo databaseField = typeof(GameHintManager).GetField("hintDatabase", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (databaseField != null)
                {
                    var database = databaseField.GetValue(hintManager);
                    testResults.Add(new TestResult("ヒントデータベース", database != null,
                        database != null ? "✓ 初期化済み" : "✗ 未初期化"));
                }
            }
        }
        else
        {
            testResults.Add(new TestResult("GameHintManagerテスト", true, 
                "現在HintSceneではないためスキップ"));
        }
        
        yield return null;
    }
    
    /// <summary>
    /// UI生成機能確認テスト
    /// </summary>
    private IEnumerator TestUIGeneration()
    {
        Debug.Log("[テスト5] UI生成機能確認...");
        
        HintUIGenerator uiGenerator = FindObjectOfType<HintUIGenerator>();
        testResults.Add(new TestResult("HintUIGenerator存在", uiGenerator != null,
            uiGenerator != null ? "✓ 発見" : "✗ 未発見"));
        
        // Canvas存在確認
        Canvas canvas = FindObjectOfType<Canvas>();
        testResults.Add(new TestResult("Canvas存在", canvas != null,
            canvas != null ? "✓ 発見" : "✗ 未発見"));
        
        // EventSystem存在確認
        UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        testResults.Add(new TestResult("EventSystem存在", eventSystem != null,
            eventSystem != null ? "✓ 発見" : "✗ 未発見"));
        
        yield return null;
    }
    
    /// <summary>
    /// テスト結果を表示
    /// </summary>
    private void DisplayTestResults()
    {
        if (!showResults) return;
        
        Debug.Log("=== テスト結果 ===");
        
        int passedTests = 0;
        int totalTests = testResults.Count;
        
        foreach (TestResult result in testResults)
        {
            string status = result.passed ? "✅ PASS" : "❌ FAIL";
            Debug.Log($"{status} {result.testName}: {result.details}");
            
            if (result.passed) passedTests++;
        }
        
        float successRate = totalTests > 0 ? (float)passedTests / totalTests * 100f : 0f;
        
        Debug.Log("==================");
        Debug.Log($"テスト結果: {passedTests}/{totalTests} 成功 ({successRate:F1}%)");
        
        if (successRate >= 90f)
        {
            Debug.Log("🎉 優秀！システムは正常に動作しています！");
        }
        else if (successRate >= 70f)
        {
            Debug.Log("⚠️ 良好。いくつかの問題を修正することを推奨します。");
        }
        else
        {
            Debug.Log("🚨 問題あり。セットアップを見直してください。");
        }
    }
    
    /// <summary>
    /// PlayerPrefsをテスト用に設定
    /// </summary>
    [ContextMenu("Set Test PlayerPrefs")]
    public void SetTestPlayerPrefs()
    {
        PlayerPrefs.SetString("ReturnScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("HintCategory", 0);
        PlayerPrefs.Save();
        Debug.Log("テスト用PlayerPrefsを設定しました");
    }
    
    /// <summary>
    /// PlayerPrefsをクリア
    /// </summary>
    [ContextMenu("Clear Test PlayerPrefs")]
    public void ClearTestPlayerPrefs()
    {
        PlayerPrefs.DeleteKey("ReturnScene");
        PlayerPrefs.DeleteKey("HintCategory");
        PlayerPrefs.Save();
        Debug.Log("テスト用PlayerPrefsをクリアしました");
    }
    
    /// <summary>
    /// 簡易動作テスト
    /// </summary>
    [ContextMenu("Quick Test")]
    public void QuickTest()
    {
        Debug.Log("=== 簡易テスト実行 ===");
        
        // 基本コンポーネントチェック
        CryptoGameManager gm = FindObjectOfType<CryptoGameManager>();
        HintSceneTransition ht = FindObjectOfType<HintSceneTransition>();
        
        Debug.Log($"CryptoGameManager: {(gm != null ? "✓" : "✗")}");
        Debug.Log($"HintSceneTransition: {(ht != null ? "✓" : "✗")}");
        Debug.Log($"現在のシーン: {SceneManager.GetActiveScene().name}");
        Debug.Log($"HintSceneロード可能: {(Application.CanStreamedLevelBeLoaded("HintScene") ? "✓" : "✗")}");
        
        Debug.Log("===================");
    }
}
