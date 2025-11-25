using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProgressTracker : MonoBehaviour
{
    [Header("Progress Settings")]
    public float maxProgress = 100f;
    
    [Header("段階的学習進度設定")]
    [Tooltip("正解時の進度増加量")]
    [Range(1f, 20f)]
    public float correctAnswerIncrement = 8f;
    
    [Tooltip("不正解時の進度減少量")]
    [Range(0f, 15f)]
    public float incorrectAnswerDecrement = 5f;
    
    [Tooltip("1セット完了時のボーナス進度")]
    [Range(0f, 15f)]
    public float setCompletionBonus = 5f;
    
    [Tooltip("連続正解時の追加ボーナス")]
    [Range(0f, 10f)]
    public float streakBonus = 2f;
    
    [Header("進度減衰設定")]
    [Tooltip("最小進度（この値以下には下がらない）")]
    [Range(0f, 25f)]
    public float minimumProgress = 0f;
    
    [Tooltip("時間経過による自然減衰を有効にする")]
    public bool enableNaturalDecay = false;
    
    [Tooltip("自然減衰率（1日あたりの減少率）")]
    [Range(0f, 5f)]
    public float naturalDecayRate = 1f;
    
    [Header("シーン間永続化設定")]
    [Tooltip("シーン切り替え時にオブジェクトを保持するか")]
    public bool persistAcrossScenes = true;
    
    [Tooltip("シングルトンパターンを使用するか")]
    public bool useSingletonPattern = true;
    
    // シングルトンインスタンス
    public static ProgressTracker Instance { get; private set; }
    
    // 各暗号方式の理解度
    private Dictionary<CryptoGameManager.CryptoType, float> progressData;
    
    // 学習統計情報
    private Dictionary<CryptoGameManager.CryptoType, LearningStats> learningStats;
    
    [System.Serializable]
    public class LearningStats
    {
        public int totalQuestions = 0;          // 総問題数
        public int correctAnswers = 0;          // 正解数
        public int incorrectAnswers = 0;        // 不正解数
        public int setsCompleted = 0;           // 完了セット数
        public int currentStreak = 0;           // 現在の連続正解数
        public int maxStreak = 0;               // 最大連続正解数
        public System.DateTime lastPlayDate;    // 最終プレイ日時
        
        public float GetAccuracy()
        {
            if (totalQuestions == 0) return 0f;
            return (float)correctAnswers / totalQuestions * 100f;
        }
    }
    
    // データ永続化用キー
    private const string PROGRESS_KEY_PREFIX = "CryptoProgress_";
    private const string STATS_KEY_PREFIX = "CryptoStats_";
    private const string LAST_UPDATE_KEY = "ProgressLastUpdate";
    
    private void Awake()
    {
        // シングルトンパターンの実装
        if (useSingletonPattern)
        {
            if (Instance != null && Instance != this)
            {
                Debug.Log($"[ProgressTracker] 既存のインスタンスを検出。重複を削除します: {gameObject.name}");
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }
        
        // シーン間永続化
        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[ProgressTracker] シーン間永続化が有効になりました: {gameObject.name}");
        }
        
        InitializeProgress();
        LoadProgress();
        LoadLearningStats();
    }
    
    private void Start()
    {
        // 起動時に自然減衰を適用
        ApplyNaturalDecay();
    }

    private void InitializeProgress()
    {
        progressData = new Dictionary<CryptoGameManager.CryptoType, float>
        {
            { CryptoGameManager.CryptoType.SymmetricKey, 0f },
            { CryptoGameManager.CryptoType.PublicKey, 0f },
            { CryptoGameManager.CryptoType.Hybrid, 0f }
        };
        
        learningStats = new Dictionary<CryptoGameManager.CryptoType, LearningStats>
        {
            { CryptoGameManager.CryptoType.SymmetricKey, new LearningStats() },
            { CryptoGameManager.CryptoType.PublicKey, new LearningStats() },
            { CryptoGameManager.CryptoType.Hybrid, new LearningStats() }
        };
    }
    
    /// <summary>
    /// 正解時の進度更新
    /// </summary>
    public void OnCorrectAnswer(CryptoGameManager.CryptoType cryptoType)
    {
        if (!progressData.ContainsKey(cryptoType)) return;
        
        // 統計更新
        var stats = learningStats[cryptoType];
        stats.totalQuestions++;
        stats.correctAnswers++;
        stats.currentStreak++;
        stats.maxStreak = Mathf.Max(stats.maxStreak, stats.currentStreak);
        stats.lastPlayDate = System.DateTime.Now;
        
        // 進度計算
        float increment = correctAnswerIncrement;
        
        // 連続正解ボーナス
        if (stats.currentStreak >= 3)
        {
            increment += streakBonus;
            Debug.Log($"[ProgressTracker] 連続正解ボーナス適用: +{streakBonus}% (連続{stats.currentStreak}問正解)");
        }
        
        // 進度更新
        float currentProgress = progressData[cryptoType];
        float newProgress = Mathf.Min(currentProgress + increment, maxProgress);
        progressData[cryptoType] = newProgress;
        
        Debug.Log($"[ProgressTracker] 正解! {cryptoType}: {currentProgress:F1}% → {newProgress:F1}% (+{increment:F1}%)");
        
        SaveProgress();
        SaveLearningStats();
        NotifyProgressChanged();
    }
    
    /// <summary>
    /// 不正解時の進度更新
    /// </summary>
    public void OnIncorrectAnswer(CryptoGameManager.CryptoType cryptoType)
    {
        if (!progressData.ContainsKey(cryptoType)) return;
        
        // 統計更新
        var stats = learningStats[cryptoType];
        stats.totalQuestions++;
        stats.incorrectAnswers++;
        stats.currentStreak = 0; // 連続正解をリセット
        stats.lastPlayDate = System.DateTime.Now;
        
        // 進度減少（最小進度以下にはならない）
        float currentProgress = progressData[cryptoType];
        float newProgress = Mathf.Max(currentProgress - incorrectAnswerDecrement, minimumProgress);
        progressData[cryptoType] = newProgress;
        
        Debug.Log($"[ProgressTracker] 不正解... {cryptoType}: {currentProgress:F1}% → {newProgress:F1}% (-{incorrectAnswerDecrement:F1}%)");
        
        SaveProgress();
        SaveLearningStats();
        NotifyProgressChanged();
    }
    
    /// <summary>
    /// セット完了時のボーナス
    /// </summary>
    public void OnSetCompleted(CryptoGameManager.CryptoType cryptoType)
    {
        if (!progressData.ContainsKey(cryptoType)) return;
        
        // 統計更新
        var stats = learningStats[cryptoType];
        stats.setsCompleted++;
        
        // セット完了ボーナス
        if (setCompletionBonus > 0)
        {
            float currentProgress = progressData[cryptoType];
            float newProgress = Mathf.Min(currentProgress + setCompletionBonus, maxProgress);
            progressData[cryptoType] = newProgress;
            
            Debug.Log($"[ProgressTracker] セット完了ボーナス! {cryptoType}: {currentProgress:F1}% → {newProgress:F1}% (+{setCompletionBonus:F1}%)");
        }
        
        SaveProgress();
        SaveLearningStats();
        NotifyProgressChanged();
    }
    
    /// <summary>
    /// 旧式の進度更新メソッド（後方互換性のため保持）
    /// </summary>
    [System.Obsolete("OnCorrectAnswer/OnIncorrectAnswerを使用してください")]
    public void UpdateProgress(CryptoGameManager.CryptoType cryptoType, float increment)
    {
        if (progressData.ContainsKey(cryptoType))
        {
            float currentProgress = progressData[cryptoType];
            float newProgress = Mathf.Clamp(currentProgress + increment, minimumProgress, maxProgress);
            progressData[cryptoType] = newProgress;
            
            SaveProgress();
            NotifyProgressChanged();
        }
    }
    
    public float GetProgress(CryptoGameManager.CryptoType cryptoType)
    {
        if (progressData.ContainsKey(cryptoType))
        {
            return progressData[cryptoType];
        }
        return 0f;
    }
    
    public float[] GetAllProgress()
    {
        return new float[]
        {
            GetProgress(CryptoGameManager.CryptoType.SymmetricKey),
            GetProgress(CryptoGameManager.CryptoType.PublicKey),
            GetProgress(CryptoGameManager.CryptoType.Hybrid)
        };
    }
    
    public float GetOverallProgress()
    {
        float total = 0f;
        foreach (var progress in progressData.Values)
        {
            total += progress;
        }
        return total / progressData.Count;
    }
    
    public int GetCurrentLevel(CryptoGameManager.CryptoType cryptoType)
    {
        float progress = GetProgress(cryptoType);
        
        if (progress >= 90f) return 4; // マスターレベル
        if (progress >= 75f) return 3; // 実践理解
        if (progress >= 50f) return 2; // 応用理解
        if (progress >= 25f) return 1; // 基礎理解
        return 0; // 初心者
    }
    
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

    private void SaveProgress()
    {
        foreach (var kvp in progressData)
        {
            string key = PROGRESS_KEY_PREFIX + kvp.Key.ToString();
            PlayerPrefs.SetFloat(key, kvp.Value);
        }
        PlayerPrefs.Save();
    }
    
    private void LoadProgress()
    {
        var keys = new List<CryptoGameManager.CryptoType>(progressData.Keys);
        foreach (var cryptoType in keys)
        {
            string key = PROGRESS_KEY_PREFIX + cryptoType.ToString();
            if (PlayerPrefs.HasKey(key))
            {
                progressData[cryptoType] = PlayerPrefs.GetFloat(key);
            }
        }
    }
    
    // プログレス完全リセット（デバッグ用）
    public void ResetAllProgress()
    {
        foreach (var cryptoType in progressData.Keys)
        {
            string key = PROGRESS_KEY_PREFIX + cryptoType.ToString();
            PlayerPrefs.DeleteKey(key);
        }
        
        InitializeProgress();
        PlayerPrefs.Save();
        
        Debug.Log("All progress has been reset!");
    }
    
    // 統計情報取得
    public Dictionary<string, object> GetProgressStats()
    {
        var stats = new Dictionary<string, object>();
        
        stats["overall_progress"] = GetOverallProgress();
        stats["symmetric_progress"] = GetProgress(CryptoGameManager.CryptoType.SymmetricKey);
        stats["public_progress"] = GetProgress(CryptoGameManager.CryptoType.PublicKey);
        stats["hybrid_progress"] = GetProgress(CryptoGameManager.CryptoType.Hybrid);
        
        stats["symmetric_level"] = GetCurrentLevel(CryptoGameManager.CryptoType.SymmetricKey);
        stats["public_level"] = GetCurrentLevel(CryptoGameManager.CryptoType.PublicKey);
        stats["hybrid_level"] = GetCurrentLevel(CryptoGameManager.CryptoType.Hybrid);
        
        return stats;
    }
    
    // 弱点分析
    public CryptoGameManager.CryptoType GetWeakestArea()
    {
        float minProgress = float.MaxValue;
        CryptoGameManager.CryptoType weakestType = CryptoGameManager.CryptoType.SymmetricKey;
        
        foreach (var kvp in progressData)
        {
            if (kvp.Value < minProgress)
            {
                minProgress = kvp.Value;
                weakestType = kvp.Key;
            }
        }
        
        return weakestType;
    }
    
    // 達成度チェック
    public bool IsAllMastered()
    {
        foreach (var progress in progressData.Values)
        {
            if (progress < 90f)
                return false;
        }
        return true;
    }

    /// <summary>
    /// 手動リセット用メソッド（ボタンから呼び出し）
    /// ボタンが押されたときのみ理解度をリセット
    /// </summary>
    public void ManualResetProgress()
    {
        Debug.Log("ProgressTracker: 手動リセットが実行されました");
        
        // メモリ内のデータをリセット
        InitializeProgress();
        
        // PlayerPrefsからも削除
        foreach (var cryptoType in progressData.Keys)
        {
            string progressKey = PROGRESS_KEY_PREFIX + cryptoType.ToString();
            string statsKey = STATS_KEY_PREFIX + cryptoType.ToString();
            PlayerPrefs.DeleteKey(progressKey);
            PlayerPrefs.DeleteKey(statsKey);
        }
        
        // 最終更新時刻も削除
        PlayerPrefs.DeleteKey(LAST_UPDATE_KEY);
        
        PlayerPrefs.Save();
        
        Debug.Log("ProgressTracker: 全ての理解度と学習統計がリセットされました");
        
        // イベント通知（他のシーンの UI更新用）
        NotifyProgressChanged();
    }
    
    /// <summary>
    /// 学習統計のみをリセット（進度は維持）
    /// </summary>
    public void ResetLearningStatsOnly()
    {
        Debug.Log("[ProgressTracker] 学習統計のみリセット開始");
        
        // メモリ内の統計データをリセット
        foreach (var cryptoType in learningStats.Keys.ToList())
        {
            learningStats[cryptoType] = new LearningStats();
        }
        
        // PlayerPrefsから統計データを削除
        foreach (var cryptoType in learningStats.Keys)
        {
            string statsKey = STATS_KEY_PREFIX + cryptoType.ToString();
            PlayerPrefs.DeleteKey(statsKey);
        }
        
        PlayerPrefs.Save();
        
        Debug.Log("[ProgressTracker] 学習統計リセット完了（進度は維持）");
    }
    
    /// <summary>
    /// 静的メソッド：シングルトンインスタンスから手動リセットを実行
    /// </summary>
    public static void ResetProgressStatic()
    {
        if (Instance != null)
        {
            Instance.ManualResetProgress();
        }
        else
        {
            Debug.LogWarning("[ProgressTracker] インスタンスが見つかりません。PlayerPrefsから直接削除します。");
            ResetProgressViaPlayerPrefs();
        }
    }
    
    /// <summary>
    /// PlayerPrefs経由での直接リセット（インスタンスがない場合）
    /// </summary>
    public static void ResetProgressViaPlayerPrefs()
    {
        // 各暗号方式のデータを削除
        string[] cryptoTypes = { "SymmetricKey", "PublicKey", "Hybrid" };
        
        foreach (string cryptoType in cryptoTypes)
        {
            string progressKey = PROGRESS_KEY_PREFIX + cryptoType;
            string statsKey = STATS_KEY_PREFIX + cryptoType;
            PlayerPrefs.DeleteKey(progressKey);
            PlayerPrefs.DeleteKey(statsKey);
        }
        
        PlayerPrefs.DeleteKey(LAST_UPDATE_KEY);
        PlayerPrefs.Save();
        
        Debug.Log("[ProgressTracker] PlayerPrefs経由で進度と学習統計のリセット完了");
    }
    
    /// <summary>
    /// 進度変更の通知（UI更新トリガー）
    /// </summary>
    private void NotifyProgressChanged()
    {
        // 最終更新時刻を記録
        PlayerPrefs.SetString(LAST_UPDATE_KEY, System.DateTime.Now.ToBinary().ToString());
        PlayerPrefs.Save();
        
        Debug.Log("[ProgressTracker] 進度変更が通知されました");
    }
    
    /// <summary>
    /// 最終更新時刻を取得
    /// </summary>
    public static System.DateTime GetLastUpdateTime()
    {
        if (PlayerPrefs.HasKey(LAST_UPDATE_KEY))
        {
            string timeString = PlayerPrefs.GetString(LAST_UPDATE_KEY);
            if (long.TryParse(timeString, out long timeBinary))
            {
                return System.DateTime.FromBinary(timeBinary);
            }
        }
        return System.DateTime.MinValue;
    }
    
    /// <summary>
    /// 静的メソッド：進度データを取得（インスタンス不要）
    /// </summary>
    public static float GetProgressStatic(CryptoGameManager.CryptoType cryptoType)
    {
        if (Instance != null)
        {
            return Instance.GetProgress(cryptoType);
        }
        else
        {
            // PlayerPrefsから直接取得
            string key = PROGRESS_KEY_PREFIX + cryptoType.ToString();
            return PlayerPrefs.GetFloat(key, 0f);
        }
    }
    
    /// <summary>
    /// 静的メソッド：全ての進度データを取得
    /// </summary>
    public static Dictionary<CryptoGameManager.CryptoType, float> GetAllProgressStatic()
    {
        var result = new Dictionary<CryptoGameManager.CryptoType, float>();
        
        if (Instance != null)
        {
            // インスタンスから取得
            result[CryptoGameManager.CryptoType.SymmetricKey] = Instance.GetProgress(CryptoGameManager.CryptoType.SymmetricKey);
            result[CryptoGameManager.CryptoType.PublicKey] = Instance.GetProgress(CryptoGameManager.CryptoType.PublicKey);
            result[CryptoGameManager.CryptoType.Hybrid] = Instance.GetProgress(CryptoGameManager.CryptoType.Hybrid);
        }
        else
        {
            // PlayerPrefsから直接取得
            result[CryptoGameManager.CryptoType.SymmetricKey] = PlayerPrefs.GetFloat(PROGRESS_KEY_PREFIX + "SymmetricKey", 0f);
            result[CryptoGameManager.CryptoType.PublicKey] = PlayerPrefs.GetFloat(PROGRESS_KEY_PREFIX + "PublicKey", 0f);
            result[CryptoGameManager.CryptoType.Hybrid] = PlayerPrefs.GetFloat(PROGRESS_KEY_PREFIX + "Hybrid", 0f);
        }
        
        return result;
    }
    
    /// <summary>
    /// 旧自動リセットメソッド（廃止予定）
    /// 互換性のため残すが、実行しない
    /// </summary>
    [System.Obsolete("自動リセットは廃止されました。ManualResetProgress()を使用してください。")]
    public void ResetProgressForNewGame()
    {
        Debug.Log("ProgressTracker: 自動リセットは無効化されています。手動リセットボタンを使用してください。");
        // 何も実行しない（ゲージを保持）
    }

    /// <summary>
    /// 学習統計の保存
    /// </summary>
    private void SaveLearningStats()
    {
        foreach (var kvp in learningStats)
        {
            var cryptoType = kvp.Key;
            var stats = kvp.Value;
            string statsKey = STATS_KEY_PREFIX + cryptoType.ToString();
            
            // JSON形式で統計データを保存
            string jsonData = JsonUtility.ToJson(stats);
            PlayerPrefs.SetString(statsKey, jsonData);
        }
        
        PlayerPrefs.Save();
        Debug.Log("[ProgressTracker] 学習統計保存完了");
    }
    
    /// <summary>
    /// 学習統計の読み込み
    /// </summary>
    private void LoadLearningStats()
    {
        foreach (var cryptoType in learningStats.Keys.ToList())
        {
            string statsKey = STATS_KEY_PREFIX + cryptoType.ToString();
            
            if (PlayerPrefs.HasKey(statsKey))
            {
                string jsonData = PlayerPrefs.GetString(statsKey);
                try
                {
                    var loadedStats = JsonUtility.FromJson<LearningStats>(jsonData);
                    if (loadedStats != null)
                    {
                        learningStats[cryptoType] = loadedStats;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[ProgressTracker] {cryptoType}の学習統計読み込みエラー: {e.Message}");
                    learningStats[cryptoType] = new LearningStats();
                }
            }
        }
        
        Debug.Log("[ProgressTracker] 学習統計読み込み完了");
    }
    
    /// <summary>
    /// 学習統計の取得
    /// </summary>
    public LearningStats GetLearningStats(CryptoGameManager.CryptoType cryptoType)
    {
        if (learningStats.ContainsKey(cryptoType))
        {
            return learningStats[cryptoType];
        }
        return new LearningStats();
    }
    
    /// <summary>
    /// 全ての学習統計を取得
    /// </summary>
    public Dictionary<CryptoGameManager.CryptoType, LearningStats> GetAllLearningStats()
    {
        return new Dictionary<CryptoGameManager.CryptoType, LearningStats>(learningStats);
    }

    /// <summary>
    /// 自然減衰の適用（時間経過による進度減少）
    /// </summary>
    private void ApplyNaturalDecay()
    {
        if (!enableNaturalDecay || naturalDecayRate <= 0f) return;
        
        System.DateTime lastUpdate = GetLastUpdateTime();
        if (lastUpdate == System.DateTime.MinValue) return; // 初回起動時はスキップ
        
        System.TimeSpan timeDifference = System.DateTime.Now - lastUpdate;
        double daysPassed = timeDifference.TotalDays;
        
        if (daysPassed >= 1.0) // 1日以上経過した場合のみ適用
        {
            float decayAmount = (float)daysPassed * naturalDecayRate;
            
            foreach (var cryptoType in progressData.Keys.ToList())
            {
                float currentProgress = progressData[cryptoType];
                float newProgress = Mathf.Max(currentProgress - decayAmount, minimumProgress);
                
                if (newProgress != currentProgress)
                {
                    progressData[cryptoType] = newProgress;
                    Debug.Log($"[ProgressTracker] 自然減衰適用 {cryptoType}: {currentProgress:F1}% → {newProgress:F1}% (-{decayAmount:F1}%)");
                }
            }
            
            // 保存と通知
            SaveProgress();
            SaveLearningStats();
            NotifyProgressChanged();
            
            Debug.Log($"[ProgressTracker] 自然減衰処理完了 ({daysPassed:F1}日経過)");
        }
    }
}