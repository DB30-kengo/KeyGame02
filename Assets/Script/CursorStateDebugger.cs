using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// カーソル状態のデバッグ・管理用ヘルパー
/// </summary>
public class CursorStateDebugger : MonoBehaviour
{
    [Header("Debug UI")]
    [Tooltip("カーソル状態を表示するテキスト")]
    public Text debugText;
    
    [Header("Manual Controls")]
    [Tooltip("手動でカーソルを表示")]
    public Button showCursorButton;
    
    [Tooltip("手動でカーソルを非表示")]
    public Button hideCursorButton;
    
    [Tooltip("カーソルロックを解除")]
    public Button unlockCursorButton;
    
    [Tooltip("カーソルをロック")]
    public Button lockCursorButton;
    
    [Header("Auto Debug")]
    [Tooltip("自動的にデバッグ情報を更新")]
    public bool autoUpdate = true;
    
    [Tooltip("更新間隔（秒）")]
    public float updateInterval = 0.5f;
    
    private float nextUpdateTime = 0f;
    
    void Start()
    {
        SetupButtons();
        
        if (debugText == null)
        {
            // 自動でデバッグテキストを作成
            CreateDebugText();
        }
    }
    
    void Update()
    {
        if (autoUpdate && Time.time >= nextUpdateTime)
        {
            UpdateDebugInfo();
            nextUpdateTime = Time.time + updateInterval;
        }
    }
    
    /// <summary>
    /// ボタンの設定
    /// </summary>
    private void SetupButtons()
    {
        if (showCursorButton != null)
            showCursorButton.onClick.AddListener(ShowCursor);
            
        if (hideCursorButton != null)
            hideCursorButton.onClick.AddListener(HideCursor);
            
        if (unlockCursorButton != null)
            unlockCursorButton.onClick.AddListener(UnlockCursor);
            
        if (lockCursorButton != null)
            lockCursorButton.onClick.AddListener(LockCursor);
    }
    
    /// <summary>
    /// デバッグテキストを自動作成
    /// </summary>
    private void CreateDebugText()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        GameObject textObj = new GameObject("CursorDebugText");
        textObj.transform.SetParent(canvas.transform, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.8f);
        rect.anchorMax = new Vector2(0.4f, 1f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        debugText = textObj.AddComponent<Text>();
        debugText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        debugText.fontSize = 12;
        debugText.color = Color.white;
        debugText.alignment = TextAnchor.UpperLeft;
        
        // 背景パネル追加
        Image background = textObj.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.7f);
        
        Debug.Log("[CursorStateDebugger] Debug text created automatically.");
    }
    
    /// <summary>
    /// デバッグ情報を更新
    /// </summary>
    public void UpdateDebugInfo()
    {
        if (debugText == null) return;
        
        string info = "=== CURSOR DEBUG INFO ===\n";
        info += $"Visible: {Cursor.visible}\n";
        info += $"Lock State: {Cursor.lockState}\n";
        info += $"Position: {Input.mousePosition}\n";
        info += $"Time: {System.DateTime.Now:HH:mm:ss}\n";
        
        // PlayerPrefs情報
        if (PlayerPrefs.HasKey("SavedCursorLockState"))
        {
            info += "\n--- SAVED STATE ---\n";
            info += $"Saved Lock: {(CursorLockMode)PlayerPrefs.GetInt("SavedCursorLockState")}\n";
            info += $"Saved Visible: {PlayerPrefs.GetInt("SavedCursorVisible") == 1}\n";
        }
        
        if (PlayerPrefs.HasKey("ReturnScene"))
        {
            info += $"Return Scene: {PlayerPrefs.GetString("ReturnScene")}\n";
        }
        
        debugText.text = info;
    }
    
    /// <summary>
    /// カーソルを表示
    /// </summary>
    [ContextMenu("Show Cursor")]
    public void ShowCursor()
    {
        Cursor.visible = true;
        Debug.Log("[CursorStateDebugger] Cursor shown manually");
        UpdateDebugInfo();
    }
    
    /// <summary>
    /// カーソルを非表示
    /// </summary>
    [ContextMenu("Hide Cursor")]
    public void HideCursor()
    {
        Cursor.visible = false;
        Debug.Log("[CursorStateDebugger] Cursor hidden manually");
        UpdateDebugInfo();
    }
    
    /// <summary>
    /// カーソルロックを解除
    /// </summary>
    [ContextMenu("Unlock Cursor")]
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Debug.Log("[CursorStateDebugger] Cursor unlocked manually");
        UpdateDebugInfo();
    }
    
    /// <summary>
    /// カーソルをロック
    /// </summary>
    [ContextMenu("Lock Cursor")]
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("[CursorStateDebugger] Cursor locked manually");
        UpdateDebugInfo();
    }
    
    /// <summary>
    /// カーソル状態をリセット
    /// </summary>
    [ContextMenu("Reset Cursor to UI Mode")]
    public void ResetToUIMode()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("[CursorStateDebugger] Cursor reset to UI mode");
        UpdateDebugInfo();
    }
    
    /// <summary>
    /// 保存されたカーソル状態をクリア
    /// </summary>
    [ContextMenu("Clear Saved Cursor State")]
    public void ClearSavedState()
    {
        PlayerPrefs.DeleteKey("SavedCursorLockState");
        PlayerPrefs.DeleteKey("SavedCursorVisible");
        PlayerPrefs.DeleteKey("ReturnScene");
        PlayerPrefs.Save();
        Debug.Log("[CursorStateDebugger] Saved cursor state cleared");
        UpdateDebugInfo();
    }
}
