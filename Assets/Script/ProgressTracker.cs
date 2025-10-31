using System.Collections.Generic;
using UnityEngine;

public class ProgressTracker : MonoBehaviour
{
    [Header("Progress Settings")]
    public float maxProgress = 100f;
    public float progressIncrement = 5f;
    public float progressDecayRate = 0.1f; // 時間経過による減衰（使用する場合）
    
    // 各暗号方式の理解度
    private Dictionary<CryptoGameManager.CryptoType, float> progressData;
    
    // データ永続化用キー
    private const string PROGRESS_KEY_PREFIX = "CryptoProgress_";
    
    private void Awake()
    {
        InitializeProgress();
        LoadProgress();
    }
    
    private void InitializeProgress()
    {
        progressData = new Dictionary<CryptoGameManager.CryptoType, float>
        {
            { CryptoGameManager.CryptoType.SymmetricKey, 0f },
            { CryptoGameManager.CryptoType.PublicKey, 0f },
            { CryptoGameManager.CryptoType.Hybrid, 0f }
        };
    }
    
    public void UpdateProgress(CryptoGameManager.CryptoType cryptoType, float increment)
    {
        if (progressData.ContainsKey(cryptoType))
        {
            float currentProgress = progressData[cryptoType];
            float newProgress = Mathf.Min(currentProgress + increment, maxProgress);
            progressData[cryptoType] = newProgress;
            
            // レベルアップチェック
            CheckLevelUp(cryptoType, currentProgress, newProgress);
            
            // データ保存
            SaveProgress();
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
    
    public string GetLevelName(int level)
    {
        switch (level)
        {
            case 4: return "暗号マスター";
            case 3: return "実践理解";
            case 2: return "応用理解";
            case 1: return "基礎理解";
            default: return "初心者";
        }
    }
    
    private void CheckLevelUp(CryptoGameManager.CryptoType cryptoType, float oldProgress, float newProgress)
    {
        int oldLevel = GetLevelFromProgress(oldProgress);
        int newLevel = GetLevelFromProgress(newProgress);
        
        if (newLevel > oldLevel)
        {
            OnLevelUp(cryptoType, newLevel);
        }
    }
    
    private int GetLevelFromProgress(float progress)
    {
        if (progress >= 90f) return 4;
        if (progress >= 75f) return 3;
        if (progress >= 50f) return 2;
        if (progress >= 25f) return 1;
        return 0;
    }
    
    private void OnLevelUp(CryptoGameManager.CryptoType cryptoType, int newLevel)
    {
        string cryptoName = GetCryptoTypeName(cryptoType);
        string levelName = GetLevelName(newLevel);
        
        // レベルアップ通知を表示
        ShowLevelUpNotification(cryptoName, levelName);
        
        Debug.Log($"Level Up! {cryptoName}: {levelName}");
    }
    
    private void ShowLevelUpNotification(string cryptoName, string levelName)
    {
        // UIマネージャーがあれば通知を表示
        var uiManager = FindObjectOfType<CryptoUIManager>();
        if (uiManager != null)
        {
            uiManager.ShowLevelUpNotification(cryptoName, levelName);
        }
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
}