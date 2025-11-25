using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 理解度ゲージをリセットするボタン用スクリプト
/// インスペクター上でボタンと対象ProgressTrackerを設定可能
/// シーン間での操作にも対応
/// </summary>
public class ProgressResetButton : MonoBehaviour
{
    /// <summary>
    /// シーン間操作のモード
    /// </summary>
    public enum CrossSceneMode
    {
        UseStaticMethods,     // 静的メソッドを使用
        FindInCurrentScene,   // 現在のシーンで検索
        UsePlayerPrefs       // PlayerPrefs経由で直接操作
    }

    [Header("ボタン設定")]
    [Tooltip("リセットボタン（自動取得されますが、手動設定も可能）")]
    public Button resetButton;
    
    [Header("ProgressTracker設定")]
    [Tooltip("リセット対象のProgressTrackerを設定してください（オプション）")]
    public ProgressTracker progressTracker;
    
    [Tooltip("シーン間での操作を有効にする")]
    public bool enableCrossSceneOperation = true;
    
    [Tooltip("ProgressTrackerが見つからない場合の動作")]
    public CrossSceneMode crossSceneMode = CrossSceneMode.UseStaticMethods;
    
    [Header("確認ダイアログ設定")]
    [Tooltip("リセット前に確認ダイアログを表示するか")]
    public bool showConfirmationDialog = true;
    
    [Tooltip("確認ダイアログのメッセージ")]
    [TextArea(3, 5)]
    public string confirmationMessage = "理解度ゲージをリセットしますか？\n\n進度データと学習統計の両方が削除されます。\n• 各暗号方式の理解度\n• 学習統計（正解数、セット完了数など）\n\nこの操作は取り消せません。";
    
    [Header("リセット範囲設定")]
    [Tooltip("学習統計も一緒にリセットするか")]
    public bool includeStatisticsReset = true;
    
    [Tooltip("統計のみリセットする機能を有効にする")]
    public bool enableStatisticsOnlyReset = true;
    
    [Header("視覚的フィードバック設定")]
    [Tooltip("リセット実行時の視覚効果を有効にする")]
    public bool enableVisualFeedback = true;
    
    [Tooltip("フィードバック表示時間（秒）")]
    [Range(0.5f, 3.0f)]
    public float feedbackDisplayTime = 2.0f;
    
    [Tooltip("フィードバックメッセージ")]
    public string feedbackMessage = "✅ 理解度と学習統計がリセットされました！";
    
    [Header("UI参照（オプション）")]
    [Tooltip("フィードバック表示用テキスト（オプション）")]
    public Text feedbackText;
    
    [Tooltip("フィードバック表示用パネル（オプション）")]
    public GameObject feedbackPanel;
    
    [Header("デバッグ設定")]
    [Tooltip("デバッグログを表示するか")]
    public bool enableDebugLog = true;
    
    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Start()
    {
        InitializeButton();
        ValidateSettings();
    }
    
    /// <summary>
    /// ボタンの初期化
    /// </summary>
    private void InitializeButton()
    {
        // ボタンが設定されていない場合、自動取得
        if (resetButton == null)
        {
            resetButton = GetComponent<Button>();
        }
        
        // ボタンが見つからない場合のエラーハンドリング
        if (resetButton == null)
        {
            Debug.LogError($"[ProgressResetButton] ボタンコンポーネントが見つかりません！ GameObject: {gameObject.name}");
            return;
        }
        
        // ボタンクリックイベントを登録
        resetButton.onClick.AddListener(OnResetButtonClicked);
        
        if (enableDebugLog)
        {
            Debug.Log($"[ProgressResetButton] ボタン初期化完了: {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 設定の妥当性確認
    /// </summary>
    private void ValidateSettings()
    {
        if (progressTracker == null && enableCrossSceneOperation)
        {
            Debug.Log("[ProgressResetButton] ProgressTrackerが設定されていませんが、シーン間操作が有効です。");
            
            if (crossSceneMode == CrossSceneMode.FindInCurrentScene)
            {
                // 現在のシーンで検索を試行
                progressTracker = FindObjectOfType<ProgressTracker>();
                if (progressTracker != null)
                {
                    Debug.Log("[ProgressResetButton] 現在のシーンでProgressTrackerを自動検出しました。");
                }
            }
            else if (crossSceneMode == CrossSceneMode.UseStaticMethods)
            {
                Debug.Log("[ProgressResetButton] 静的メソッド経由でProgressTrackerにアクセスします。");
            }
            else if (crossSceneMode == CrossSceneMode.UsePlayerPrefs)
            {
                Debug.Log("[ProgressResetButton] PlayerPrefs経由で直接操作します。");
            }
        }
        else if (progressTracker == null)
        {
            Debug.LogError("[ProgressResetButton] ProgressTrackerが設定されていません！インスペクターで設定してください。");
            
            // 自動検索を試行
            progressTracker = FindObjectOfType<ProgressTracker>();
            if (progressTracker != null)
            {
                Debug.Log("[ProgressResetButton] ProgressTrackerを自動検出しました。");
            }
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"[ProgressResetButton] 設定確認完了 - ProgressTracker: {(progressTracker != null ? "設定済み" : "未設定")}, CrossScene: {enableCrossSceneOperation}, Mode: {crossSceneMode}");
        }
    }
    
    /// <summary>
    /// リセットボタンクリック時の処理
    /// </summary>
    public void OnResetButtonClicked()
    {
        if (enableDebugLog)
        {
            Debug.Log($"[ProgressResetButton] リセットボタンがクリックされました: {gameObject.name}");
        }
        
        // ProgressTrackerの有効性確認（シーン間操作考慮）
        if (progressTracker == null && !enableCrossSceneOperation)
        {
            Debug.LogError("[ProgressResetButton] ProgressTrackerが設定されておらず、シーン間操作も無効です！");
            ShowErrorFeedback("エラー: ProgressTrackerが見つかりません");
            return;
        }
        
        // 確認ダイアログ表示
        if (showConfirmationDialog)
        {
            ShowConfirmationDialog();
        }
        else
        {
            ExecuteReset();
        }
    }
    
    /// <summary>
    /// 確認ダイアログを表示
    /// </summary>
    private void ShowConfirmationDialog()
    {
        // Unity Editorでの確認ダイアログ
        #if UNITY_EDITOR
        if (UnityEditor.EditorUtility.DisplayDialog("理解度リセット確認", confirmationMessage, "リセット実行", "キャンセル"))
        {
            ExecuteReset();
        }
        else
        {
            if (enableDebugLog)
            {
                Debug.Log("[ProgressResetButton] リセットがキャンセルされました");
            }
        }
        #else
        // ビルド版では直接実行（実際のプロジェクトではカスタムダイアログを実装）
        Debug.Log("[ProgressResetButton] ビルド版: 確認なしでリセット実行");
        ExecuteReset();
        #endif
    }
    
    /// <summary>
    /// リセット実行
    /// </summary>
    private void ExecuteReset()
    {
        if (enableDebugLog)
        {
            Debug.Log("[ProgressResetButton] 理解度リセット実行開始");
        }
        
        try
        {
            bool resetSuccess = false;
            
            // ProgressTrackerが直接設定されている場合
            if (progressTracker != null)
            {
                progressTracker.ManualResetProgress();
                resetSuccess = true;
                if (enableDebugLog)
                {
                    Debug.Log("[ProgressResetButton] 直接ProgressTrackerでリセット完了");
                }
            }
            // シーン間操作が有効な場合
            else if (enableCrossSceneOperation)
            {
                switch (crossSceneMode)
                {
                    case CrossSceneMode.UseStaticMethods:
                        ProgressTracker.ResetProgressStatic();
                        resetSuccess = true;
                        if (enableDebugLog)
                        {
                            Debug.Log("[ProgressResetButton] 静的メソッドでリセット完了");
                        }
                        break;
                        
                    case CrossSceneMode.FindInCurrentScene:
                        ProgressTracker foundTracker = FindObjectOfType<ProgressTracker>();
                        if (foundTracker != null)
                        {
                            foundTracker.ManualResetProgress();
                            resetSuccess = true;
                            if (enableDebugLog)
                            {
                                Debug.Log("[ProgressResetButton] 検索で見つけたProgressTrackerでリセット完了");
                            }
                        }
                        else
                        {
                            // フォールバック：PlayerPrefs経由
                            ProgressTracker.ResetProgressViaPlayerPrefs();
                            resetSuccess = true;
                            if (enableDebugLog)
                            {
                                Debug.Log("[ProgressResetButton] ProgressTracker未検出、PlayerPrefs経由でリセット完了");
                            }
                        }
                        break;
                        
                    case CrossSceneMode.UsePlayerPrefs:
                        ProgressTracker.ResetProgressViaPlayerPrefs();
                        resetSuccess = true;
                        if (enableDebugLog)
                        {
                            Debug.Log("[ProgressResetButton] PlayerPrefs経由でリセット完了");
                        }
                        break;
                }
            }
            
            if (resetSuccess)
            {
                if (enableDebugLog)
                {
                    Debug.Log("[ProgressResetButton] 理解度リセット完了");
                }
                
                // 視覚的フィードバック表示
                if (enableVisualFeedback)
                {
                    StartCoroutine(ShowSuccessFeedback());
                }
            }
            else
            {
                Debug.LogError("[ProgressResetButton] リセットに失敗しました");
                ShowErrorFeedback("リセットに失敗しました");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ProgressResetButton] リセット実行エラー: {e.Message}");
            ShowErrorFeedback($"リセット失敗: {e.Message}");
        }
    }
    
    /// <summary>
    /// 成功フィードバック表示
    /// </summary>
    private IEnumerator ShowSuccessFeedback()
    {
        // フィードバックテキストがある場合
        if (feedbackText != null)
        {
            string originalText = feedbackText.text;
            Color originalColor = feedbackText.color;
            
            feedbackText.text = feedbackMessage;
            feedbackText.color = Color.green;
            
            yield return new WaitForSeconds(feedbackDisplayTime);
            
            feedbackText.text = originalText;
            feedbackText.color = originalColor;
        }
        
        // フィードバックパネルがある場合
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(true);
            yield return new WaitForSeconds(feedbackDisplayTime);
            feedbackPanel.SetActive(false);
        }
        
        // どちらもない場合は、コンソールログのみ
        if (feedbackText == null && feedbackPanel == null)
        {
            Debug.Log($"[ProgressResetButton] {feedbackMessage}");
        }
    }
    
    /// <summary>
    /// エラーフィードバック表示
    /// </summary>
    private void ShowErrorFeedback(string errorMessage)
    {
        if (feedbackText != null)
        {
            StartCoroutine(ShowErrorText(errorMessage));
        }
        else
        {
            Debug.LogError($"[ProgressResetButton] {errorMessage}");
        }
    }
    
    /// <summary>
    /// エラーテキスト表示コルーチン
    /// </summary>
    private IEnumerator ShowErrorText(string errorMessage)
    {
        string originalText = feedbackText.text;
        Color originalColor = feedbackText.color;
        
        feedbackText.text = errorMessage;
        feedbackText.color = Color.red;
        
        yield return new WaitForSeconds(feedbackDisplayTime);
        
        feedbackText.text = originalText;
        feedbackText.color = originalColor;
    }
    
    /// <summary>
    /// 外部からのリセット実行（スクリプトから呼び出し可能）
    /// </summary>
    public void ForceReset()
    {
        if (progressTracker != null)
        {
            ExecuteReset();
        }
        else
        {
            Debug.LogError("[ProgressResetButton] ForceReset: ProgressTrackerが設定されていません");
        }
    }
    
    /// <summary>
    /// ProgressTrackerを動的に設定
    /// </summary>
    public void SetProgressTracker(ProgressTracker tracker)
    {
        progressTracker = tracker;
        
        if (enableDebugLog)
        {
            Debug.Log($"[ProgressResetButton] ProgressTracker設定: {(tracker != null ? tracker.name : "null")}");
        }
    }
    
    /// <summary>
    /// ボタンの有効/無効切り替え
    /// </summary>
    public void SetButtonInteractable(bool interactable)
    {
        if (resetButton != null)
        {
            resetButton.interactable = interactable;
        }
    }
    
    /// <summary>
    /// デストラクタ（リスナーをクリーンアップ）
    /// </summary>
    private void OnDestroy()
    {
        if (resetButton != null)
        {
            resetButton.onClick.RemoveListener(OnResetButtonClicked);
        }
    }
    
    /// <summary>
    /// エディタ用：リセットのテスト
    /// </summary>
    [ContextMenu("Test Reset")]
    public void TestReset()
    {
        if (Application.isPlaying)
        {
            OnResetButtonClicked();
        }
        else
        {
            Debug.Log("[ProgressResetButton] テスト: 理解度リセットを実行する予定");
        }
    }
    
    /// <summary>
    /// エディタ用：設定状況の確認
    /// </summary>
    [ContextMenu("Validate Settings")]
    public void ValidateSettingsMenu()
    {
        ValidateSettings();
        
        string status = "=== ProgressResetButton 設定状況 ===\n";
        status += $"Reset Button: {(resetButton != null ? resetButton.name : "未設定")}\n";
        status += $"Progress Tracker: {(progressTracker != null ? progressTracker.name : "未設定")}\n";
        status += $"Confirmation Dialog: {(showConfirmationDialog ? "有効" : "無効")}\n";
        status += $"Visual Feedback: {(enableVisualFeedback ? "有効" : "無効")}\n";
        status += $"Feedback Text: {(feedbackText != null ? feedbackText.name : "未設定")}\n";
        status += $"Feedback Panel: {(feedbackPanel != null ? feedbackPanel.name : "未設定")}";
        
        Debug.Log(status);
    }

    /// <summary>
    /// 学習統計のみリセット（進度は保持）
    /// </summary>
    public void ResetStatisticsOnly()
    {
        if (enableDebugLog)
        {
            Debug.Log("[ProgressResetButton] 学習統計のみリセット実行");
        }
        
        try
        {
            bool resetSuccess = false;
            
            // ProgressTrackerが直接設定されている場合
            if (progressTracker != null)
            {
                progressTracker.ResetLearningStatsOnly();
                resetSuccess = true;
                if (enableDebugLog)
                {
                    Debug.Log("[ProgressResetButton] 直接ProgressTrackerで統計リセット完了");
                }
            }
            // シーン間操作が有効な場合
            else if (enableCrossSceneOperation)
            {
                switch (crossSceneMode)
                {
                    case CrossSceneMode.UseStaticMethods:
                        if (ProgressTracker.Instance != null)
                        {
                            ProgressTracker.Instance.ResetLearningStatsOnly();
                            resetSuccess = true;
                        }
                        else
                        {
                            // PlayerPrefs経由で統計データのみを削除
                            ResetStatisticsViaPlayerPrefs();
                            resetSuccess = true;
                        }
                        break;
                        
                    case CrossSceneMode.FindInCurrentScene:
                        ProgressTracker foundTracker = FindObjectOfType<ProgressTracker>();
                        if (foundTracker != null)
                        {
                            foundTracker.ResetLearningStatsOnly();
                            resetSuccess = true;
                        }
                        else
                        {
                            ResetStatisticsViaPlayerPrefs();
                            resetSuccess = true;
                        }
                        break;
                        
                    case CrossSceneMode.UsePlayerPrefs:
                        ResetStatisticsViaPlayerPrefs();
                        resetSuccess = true;
                        break;
                }
            }
            
            if (resetSuccess)
            {
                if (enableVisualFeedback)
                {
                    StartCoroutine(ShowStatisticsResetFeedback());
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ProgressResetButton] 統計リセットエラー: {e.Message}");
            ShowErrorFeedback($"統計リセット失敗: {e.Message}");
        }
    }
    
    /// <summary>
    /// PlayerPrefs経由で学習統計のみを削除
    /// </summary>
    private void ResetStatisticsViaPlayerPrefs()
    {
        string[] cryptoTypes = { "SymmetricKey", "PublicKey", "Hybrid" };
        const string STATS_KEY_PREFIX = "CryptoStats_";
        
        foreach (string cryptoType in cryptoTypes)
        {
            string statsKey = STATS_KEY_PREFIX + cryptoType;
            PlayerPrefs.DeleteKey(statsKey);
        }
        
        PlayerPrefs.Save();
        
        if (enableDebugLog)
        {
            Debug.Log("[ProgressResetButton] PlayerPrefs経由で学習統計削除完了");
        }
    }
    
    /// <summary>
    /// 統計リセット用フィードバック表示
    /// </summary>
    private IEnumerator ShowStatisticsResetFeedback()
    {
        string statisticsMessage = "✅ 学習統計がリセットされました！（進度は保持）";
        
        // フィードバックテキストがある場合
        if (feedbackText != null)
        {
            string originalText = feedbackText.text;
            Color originalColor = feedbackText.color;
            
            feedbackText.text = statisticsMessage;
            feedbackText.color = Color.cyan;
            
            yield return new WaitForSeconds(feedbackDisplayTime);
            
            feedbackText.text = originalText;
            feedbackText.color = originalColor;
        }
        
        // フィードバックパネルがある場合
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(true);
            yield return new WaitForSeconds(feedbackDisplayTime);
            feedbackPanel.SetActive(false);
        }
        
        // どちらもない場合は、コンソールログのみ
        if (feedbackText == null && feedbackPanel == null)
        {
            Debug.Log($"[ProgressResetButton] {statisticsMessage}");
        }
    }
}
