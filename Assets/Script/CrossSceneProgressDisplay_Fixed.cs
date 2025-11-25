using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 別シーンのProgressTrackerから理解度ゲージを更新するスクリプト
/// PlayerPrefsを監視して自動更新を行う
/// </summary>
public class CrossSceneProgressDisplay : MonoBehaviour
{
    [Header("UI要素設定")]
    [Tooltip("各暗号方式のSlider（順序：Symmetric, Public, Hybrid）")]
    public Slider[] progressSliders = new Slider[3];
    
    [Tooltip("各暗号方式のText（順序：Symmetric, Public, Hybrid）")]
    public Text[] progressLabels = new Text[3];
    
    [Header("更新設定")]
    [Tooltip("PlayerPrefsの監視間隔（秒）")]
    [Range(0.5f, 5.0f)]
    public float updateInterval = 2.0f;
    
    [Tooltip("起動時に即座に更新するか")]
    public bool updateOnStart = true;
    
    [Tooltip("フォーカス取得時に更新するか")]
    public bool updateOnApplicationFocus = false;
    
    [Header("表示設定")]
    [Tooltip("進度表示フォーマット（{0}=暗号名, {1}=パーセント, {2}=レベル名）")]
    public string progressDisplayFormat = "{0}: {1:F0}% ({2})";
    
    [Tooltip("最大進度値")]
    public float maxProgress = 100f;

    [Header("学習統計表示設定")]
    [Tooltip("学習統計を表示するか")]
    public bool showLearningStatistics = false;
    
    [Tooltip("各暗号方式の学習統計表示Text（順序：Symmetric, Public, Hybrid）")]
    public Text[] statisticsLabels = new Text[3];
    
    [Tooltip("統計表示フォーマット（{0}=正解率, {1}=総問題数, {2}=セット完了数, {3}=連続正解数）")]
    public string statisticsDisplayFormat = "正解率: {0:F1}% | 問題数: {1} | セット: {2} | 連続: {3}";
    
    [Tooltip("統計が0の場合の表示テキスト")]
    public string noStatisticsText = "まだデータがありません";
    
    [Header("更新制御設定")]
    [Tooltip("CryptoGameManagerとの競合を避けるための遅延時間")]
    [Range(0.1f, 2f)]
    public float updateDelay = 0.5f;
    
    [Header("デバッグ設定")]
    [Tooltip("デバッグログを表示するか")]
    public bool enableDebugLog = true;
    
    // 内部データ
    private Dictionary<CryptoGameManager.CryptoType, float> lastKnownProgress;
    private System.DateTime lastUpdateTime;
    private Coroutine updateCoroutine;
    private Coroutine delayedUpdateCoroutine;
    private bool isUpdating = false;
    
    /// <summary>
    /// 暗号方式の順序定義
    /// </summary>
    private readonly CryptoGameManager.CryptoType[] cryptoOrder = 
    {
        CryptoGameManager.CryptoType.SymmetricKey,
        CryptoGameManager.CryptoType.PublicKey,
        CryptoGameManager.CryptoType.Hybrid
    };
    
    /// <summary>
    /// 初期化
    /// </summary>
    private void Start()
    {
        InitializeProgressData();
        ValidateUIElements();
        
        if (updateOnStart)
        {
            UpdateProgressDisplay();
        }
        
        // 定期更新を開始
        StartPeriodicUpdate();
        
        if (enableDebugLog)
        {
            Debug.Log("[CrossSceneProgressDisplay] 初期化完了");
        }
    }
    
    /// <summary>
    /// 進度データの初期化
    /// </summary>
    private void InitializeProgressData()
    {
        lastKnownProgress = new Dictionary<CryptoGameManager.CryptoType, float>
        {
            { CryptoGameManager.CryptoType.SymmetricKey, 0f },
            { CryptoGameManager.CryptoType.PublicKey, 0f },
            { CryptoGameManager.CryptoType.Hybrid, 0f }
        };
        
        lastUpdateTime = System.DateTime.MinValue;
    }
    
    /// <summary>
    /// UI要素の妥当性確認
    /// </summary>
    private void ValidateUIElements()
    {
        // Slidersの確認
        for (int i = 0; i < progressSliders.Length && i < cryptoOrder.Length; i++)
        {
            if (progressSliders[i] == null)
            {
                Debug.LogWarning($"[CrossSceneProgressDisplay] progressSliders[{i}] ({GetCryptoTypeName(cryptoOrder[i])}) が設定されていません");
            }
            else
            {
                progressSliders[i].maxValue = maxProgress;
            }
        }
        
        // Progress Labelsの確認
        for (int i = 0; i < progressLabels.Length && i < cryptoOrder.Length; i++)
        {
            if (progressLabels[i] == null)
            {
                Debug.LogWarning($"[CrossSceneProgressDisplay] progressLabels[{i}] ({GetCryptoTypeName(cryptoOrder[i])}) が設定されていません");
            }
        }
        
        // Statistics Labelsの確認
        if (showLearningStatistics)
        {
            for (int i = 0; i < statisticsLabels.Length && i < cryptoOrder.Length; i++)
            {
                if (statisticsLabels[i] == null)
                {
                    if (enableDebugLog)
                    {
                        Debug.LogWarning($"[CrossSceneProgressDisplay] statisticsLabels[{i}] ({GetCryptoTypeName(cryptoOrder[i])}) が設定されていません");
                    }
                }
            }
            
            // 統計ラベルが1つも設定されていない場合は統計表示を無効化
            bool hasAnyStatisticsLabel = false;
            for (int i = 0; i < statisticsLabels.Length; i++)
            {
                if (statisticsLabels[i] != null)
                {
                    hasAnyStatisticsLabel = true;
                    break;
                }
            }
            
            if (!hasAnyStatisticsLabel)
            {
                showLearningStatistics = false;
                if (enableDebugLog)
                {
                    Debug.LogWarning("[CrossSceneProgressDisplay] 統計ラベルが設定されていないため、統計表示を無効にしました");
                }
            }
        }
    }
    
    /// <summary>
    /// 定期更新を開始
    /// </summary>
    private void StartPeriodicUpdate()
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
        }
        
        updateCoroutine = StartCoroutine(PeriodicUpdateCoroutine());
    }
    
    /// <summary>
    /// 定期更新コルーチン
    /// </summary>
    private IEnumerator PeriodicUpdateCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);
            
            // 最終更新時刻をチェック
            System.DateTime latestUpdateTime = ProgressTracker.GetLastUpdateTime();
            if (latestUpdateTime > lastUpdateTime)
            {
                UpdateProgressDisplay();
                lastUpdateTime = latestUpdateTime;
                
                if (enableDebugLog)
                {
                    Debug.Log("[CrossSceneProgressDisplay] 進度更新を検出、表示を更新しました");
                }
            }
        }
    }
    
    /// <summary>
    /// 進度表示を更新（遅延付き）
    /// </summary>
    public void UpdateProgressDisplay()
    {
        // 既存の遅延更新をキャンセル
        if (delayedUpdateCoroutine != null)
        {
            StopCoroutine(delayedUpdateCoroutine);
        }
        
        // 遅延付きで更新を実行
        delayedUpdateCoroutine = StartCoroutine(UpdateProgressDisplayDelayed());
    }
    
    /// <summary>
    /// 遅延付きで進度表示を更新
    /// </summary>
    private IEnumerator UpdateProgressDisplayDelayed()
    {
        // CryptoGameManagerのアニメーションが完了するのを待つ
        yield return new WaitForSeconds(updateDelay);
        
        UpdateProgressDisplayImmediate();
    }
    
    /// <summary>
    /// 即座に進度表示を更新
    /// </summary>
    private void UpdateProgressDisplayImmediate()
    {
        // 既に更新中の場合はスキップ
        if (isUpdating)
        {
            if (enableDebugLog)
            {
                Debug.Log("[CrossSceneProgressDisplay] 更新中のためスキップ");
            }
            return;
        }
        
        isUpdating = true;
        
        try
        {
            // 最新の進度データを取得
            Dictionary<CryptoGameManager.CryptoType, float> currentProgress = ProgressTracker.GetAllProgressStatic();
            
            if (enableDebugLog)
            {
                Debug.Log($"[CrossSceneProgressDisplay] 遅延更新実行 - 値: [{string.Join(", ", System.Linq.Enumerable.Select(currentProgress.Values, v => v.ToString("F1")))}]");
            }
            
            // UI要素を更新
            for (int i = 0; i < cryptoOrder.Length; i++)
            {
                CryptoGameManager.CryptoType cryptoType = cryptoOrder[i];
                float progress = currentProgress[cryptoType];
                
                // Slider更新（即座に設定、アニメーションなし）
                if (i < progressSliders.Length && progressSliders[i] != null)
                {
                    // スライダーのmaxValueに応じて適切な値を設定
                    if (progressSliders[i].maxValue == 1f)
                    {
                        progressSliders[i].value = progress / 100f;
                    }
                    else
                    {
                        progressSliders[i].value = progress;
                    }
                    
                    if (enableDebugLog)
                    {
                        Debug.Log($"[CrossSceneProgressDisplay] Slider[{i}] 更新: {progress:F1}% -> {progressSliders[i].value:F3} (maxValue: {progressSliders[i].maxValue})");
                    }
                }
                
                // Label更新
                if (i < progressLabels.Length && progressLabels[i] != null)
                {
                    string cryptoName = GetCryptoTypeName(cryptoType);
                    string levelName = GetLevelName(progress);
                    string displayText = string.Format(progressDisplayFormat, cryptoName, progress, levelName);
                    progressLabels[i].text = displayText;
                }
                
                // 学習統計更新
                if (showLearningStatistics && i < statisticsLabels.Length && statisticsLabels[i] != null)
                {
                    UpdateStatisticsDisplay(cryptoType, i);
                }
                
                // 変更をトラッキング
                if (!lastKnownProgress[cryptoType].Equals(progress))
                {
                    lastKnownProgress[cryptoType] = progress;
                    
                    if (enableDebugLog)
                    {
                        Debug.Log($"[CrossSceneProgressDisplay] {cryptoType} 進度更新: {progress:F1}%");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugLog)
            {
                Debug.LogError($"[CrossSceneProgressDisplay] 更新エラー: {e.Message}");
            }
        }
        finally
        {
            isUpdating = false;
        }
    }
    
    /// <summary>
    /// 暗号方式名を取得
    /// </summary>
    private string GetCryptoTypeName(CryptoGameManager.CryptoType type)
    {
        switch (type)
        {
            case CryptoGameManager.CryptoType.SymmetricKey: return "共通鍵暗号";
            case CryptoGameManager.CryptoType.PublicKey: return "公開鍵暗号";
            case CryptoGameManager.CryptoType.Hybrid: return "ハイブリッド暗号";
            default: return "Unknown";
        }
    }
    
    /// <summary>
    /// 理解レベル名を取得
    /// </summary>
    private string GetLevelName(float progress)
    {
        if (progress >= 90f) return "上級者";
        if (progress >= 75f) return "熟練者";
        if (progress >= 50f) return "応用理解";
        if (progress >= 25f) return "基礎理解";
        return "初心者";
    }
    
    /// <summary>
    /// アプリケーションフォーカス時の処理
    /// </summary>
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && updateOnApplicationFocus)
        {
            try
            {
                UpdateProgressDisplay();
                
                if (enableDebugLog)
                {
                    Debug.Log("[CrossSceneProgressDisplay] アプリケーションフォーカス時に表示を更新しました");
                }
            }
            catch (System.Exception e)
            {
                if (enableDebugLog)
                {
                    Debug.LogError($"[CrossSceneProgressDisplay] アプリケーションフォーカス時の更新でエラー: {e.Message}");
                }
            }
        }
    }
    
    /// <summary>
    /// 手動更新（外部から呼び出し可能）
    /// </summary>
    public void ManualUpdate()
    {
        UpdateProgressDisplayImmediate(); // 遅延なしで即座に更新
        
        if (enableDebugLog)
        {
            Debug.Log("[CrossSceneProgressDisplay] 手動更新が実行されました");
        }
    }
    
    /// <summary>
    /// 更新間隔を動的に変更
    /// </summary>
    public void SetUpdateInterval(float newInterval)
    {
        updateInterval = Mathf.Clamp(newInterval, 0.1f, 10f);
        
        // コルーチンを再開始
        StartPeriodicUpdate();
        
        if (enableDebugLog)
        {
            Debug.Log($"[CrossSceneProgressDisplay] 更新間隔を {updateInterval:F1}秒 に変更しました");
        }
    }
    
    /// <summary>
    /// 定期更新の停止
    /// </summary>
    public void StopPeriodicUpdate()
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
            
            if (enableDebugLog)
            {
                Debug.Log("[CrossSceneProgressDisplay] 定期更新を停止しました");
            }
        }
    }
    
    /// <summary>
    /// 現在の進度情報をログ出力
    /// </summary>
    [ContextMenu("Log Current Progress")]
    public void LogCurrentProgress()
    {
        Dictionary<CryptoGameManager.CryptoType, float> currentProgress = ProgressTracker.GetAllProgressStatic();
        
        string progressInfo = "=== 現在の理解度進度 ===\n";
        foreach (var kvp in currentProgress)
        {
            progressInfo += $"{GetCryptoTypeName(kvp.Key)}: {kvp.Value:F1}% ({GetLevelName(kvp.Value)})\n";
        }
        progressInfo += $"最終更新: {ProgressTracker.GetLastUpdateTime()}";
        
        Debug.Log(progressInfo);
    }
    
    /// <summary>
    /// 破棄時の処理
    /// </summary>
    private void OnDestroy()
    {
        StopPeriodicUpdate();
        
        // 遅延更新もキャンセル
        if (delayedUpdateCoroutine != null)
        {
            StopCoroutine(delayedUpdateCoroutine);
            delayedUpdateCoroutine = null;
        }
    }

    /// <summary>
    /// 学習統計表示を更新
    /// </summary>
    private void UpdateStatisticsDisplay(CryptoGameManager.CryptoType cryptoType, int index)
    {
        // インデックスと配列の安全性確認
        if (index < 0 || index >= statisticsLabels.Length || statisticsLabels[index] == null)
        {
            if (enableDebugLog)
            {
                Debug.LogWarning($"[CrossSceneProgressDisplay] 統計ラベル[{index}]が無効です");
            }
            return;
        }
        
        try
        {
            // ProgressTrackerインスタンスから統計を取得
            if (ProgressTracker.Instance != null)
            {
                var stats = ProgressTracker.Instance.GetLearningStats(cryptoType);
                
                if (stats.totalQuestions > 0)
                {
                    float accuracy = stats.GetAccuracy();
                    string displayText = string.Format(statisticsDisplayFormat, 
                        accuracy, 
                        stats.totalQuestions, 
                        stats.setsCompleted, 
                        stats.currentStreak);
                    statisticsLabels[index].text = displayText;
                }
                else
                {
                    statisticsLabels[index].text = noStatisticsText;
                }
            }
            else
            {
                // ProgressTrackerが利用できない場合はPlayerPrefsから直接読み取り
                UpdateStatisticsFromPlayerPrefs(cryptoType, index);
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugLog)
            {
                Debug.LogError($"[CrossSceneProgressDisplay] 統計表示更新エラー {cryptoType}: {e.Message}");
            }
            
            // エラー時は安全なテキストを設定
            if (statisticsLabels[index] != null)
            {
                statisticsLabels[index].text = "統計読み込みエラー";
            }
        }
    }
    
    /// <summary>
    /// PlayerPrefs経由で学習統計を更新
    /// </summary>
    private void UpdateStatisticsFromPlayerPrefs(CryptoGameManager.CryptoType cryptoType, int index)
    {
        // インデックスと配列の安全性確認
        if (index < 0 || index >= statisticsLabels.Length || statisticsLabels[index] == null)
        {
            if (enableDebugLog)
            {
                Debug.LogWarning($"[CrossSceneProgressDisplay] PlayerPrefs統計更新: ラベル[{index}]が無効です");
            }
            return;
        }
        
        string statsKey = "CryptoStats_" + cryptoType.ToString();
        
        if (PlayerPrefs.HasKey(statsKey))
        {
            try
            {
                string jsonData = PlayerPrefs.GetString(statsKey);
                var stats = JsonUtility.FromJson<ProgressTracker.LearningStats>(jsonData);
                
                if (stats != null && stats.totalQuestions > 0)
                {
                    float accuracy = stats.GetAccuracy();
                    string displayText = string.Format(statisticsDisplayFormat, 
                        accuracy, 
                        stats.totalQuestions, 
                        stats.setsCompleted, 
                        stats.currentStreak);
                    statisticsLabels[index].text = displayText;
                }
                else
                {
                    statisticsLabels[index].text = noStatisticsText;
                }
            }
            catch (System.Exception e)
            {
                if (enableDebugLog)
                {
                    Debug.LogWarning($"[CrossSceneProgressDisplay] 統計データ読み込みエラー {cryptoType}: {e.Message}");
                }
                statisticsLabels[index].text = noStatisticsText;
            }
        }
        else
        {
            statisticsLabels[index].text = noStatisticsText;
        }
    }

    /// <summary>
    /// 学習統計表示の有効/無効を切り替え
    /// </summary>
    public void SetStatisticsDisplay(bool enabled)
    {
        showLearningStatistics = enabled;
        
        if (enableDebugLog)
        {
            Debug.Log($"[CrossSceneProgressDisplay] 学習統計表示を{(enabled ? "有効" : "無効")}にしました");
        }
        
        // 即座に表示を更新
        UpdateProgressDisplay();
    }
    
    /// <summary>
    /// 統計ラベルを動的に設定
    /// </summary>
    public void SetStatisticsLabels(Text[] labels)
    {
        statisticsLabels = labels ?? new Text[3];
        
        if (enableDebugLog)
        {
            Debug.Log($"[CrossSceneProgressDisplay] 統計ラベルを設定しました ({statisticsLabels.Length}個)");
        }
        
        // 設定後に妥当性確認
        ValidateUIElements();
    }
}
