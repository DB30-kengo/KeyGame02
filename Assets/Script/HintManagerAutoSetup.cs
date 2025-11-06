using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GameHintManager の UI参照を自動設定するヘルパー
/// </summary>
public class HintManagerAutoSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [Tooltip("自動設定を実行")]
    public bool autoSetup = false;
    
    [Tooltip("設定対象のGameHintManager")]
    public GameHintManager hintManager;
    
    void Start()
    {
        if (autoSetup && hintManager != null)
        {
            AutoSetupHintManager();
        }
    }
    
    /// <summary>
    /// GameHintManager のUI参照を自動設定
    /// </summary>
    [ContextMenu("Auto Setup GameHintManager")]
    public void AutoSetupHintManager()
    {
        if (hintManager == null)
        {
            hintManager = FindObjectOfType<GameHintManager>();
            if (hintManager == null)
            {
                Debug.LogError("GameHintManager が見つかりません！");
                return;
            }
        }
        
        // UI References の設定
        SetupUIReferences();
        
        // UI Panels の設定  
        SetupUIPanels();
        
        // Visual Settings の設定
        SetupVisualSettings();
        
        Debug.Log("GameHintManager の自動設定が完了しました！");
    }
    
    private void SetupUIReferences()
    {
        // Hint Content Text の設定
        Text hintContentText = FindUIComponent<Text>("HintContent");
        if (hintContentText != null)
        {
            SetPrivateField(hintManager, "hintContentText", hintContentText);
            Debug.Log("Hint Content Text を設定しました");
        }
        
        // Hint Title Text の設定
        Text hintTitleText = FindUIComponent<Text>("HintTitle");
        if (hintTitleText != null)
        {
            SetPrivateField(hintManager, "hintTitleText", hintTitleText);
            Debug.Log("Hint Title Text を設定しました");
        }
        
        // Category Buttons の設定
        Button[] categoryButtons = new Button[5];
        string[] categoryButtonNames = {
            "Button_共通鍵暗号",
            "Button_公開鍵暗号", 
            "Button_ハイブリッド暗号",
            "Button_ゲーム操作",
            "Button_一般ヒント"
        };
        
        for (int i = 0; i < categoryButtonNames.Length; i++)
        {
            Button btn = FindUIComponent<Button>(categoryButtonNames[i]);
            if (btn != null)
            {
                categoryButtons[i] = btn;
            }
        }
        SetPrivateField(hintManager, "categoryButtons", categoryButtons);
        Debug.Log($"Category Buttons を設定しました ({categoryButtons.Length}個)");
        
        // Hint Selection Buttons の設定
        Button[] hintSelectionButtons = new Button[6];
        for (int i = 0; i < 6; i++)
        {
            Button btn = FindUIComponent<Button>($"Button_ヒント {i + 1}");
            if (btn != null)
            {
                hintSelectionButtons[i] = btn;
            }
        }
        SetPrivateField(hintManager, "hintSelectionButtons", hintSelectionButtons);
        Debug.Log($"Hint Selection Buttons を設定しました ({hintSelectionButtons.Length}個)");
        
        // Back Button の設定
        Button backButton = FindUIComponent<Button>("Button_← 戻る");
        if (backButton != null)
        {
            SetPrivateField(hintManager, "backButton", backButton);
            Debug.Log("Back Button を設定しました");
        }
        
        // Main Menu Button の設定
        Button mainMenuButton = FindUIComponent<Button>("Button_メインメニュー");
        if (mainMenuButton != null)
        {
            SetPrivateField(hintManager, "mainMenuButton", mainMenuButton);
            Debug.Log("Main Menu Button を設定しました");
        }
    }
    
    private void SetupUIPanels()
    {
        // Category Panel の設定
        GameObject categoryPanel = GameObject.Find("CategoryPanel");
        if (categoryPanel != null)
        {
            SetPrivateField(hintManager, "categoryPanel", categoryPanel);
            Debug.Log("Category Panel を設定しました");
        }
        
        // Hint Display Panel の設定
        GameObject hintDisplayPanel = GameObject.Find("HintDisplayPanel");
        if (hintDisplayPanel != null)
        {
            SetPrivateField(hintManager, "hintDisplayPanel", hintDisplayPanel);
            Debug.Log("Hint Display Panel を設定しました");
        }
        
        // Hint Selection Panel の設定
        GameObject hintSelectionPanel = GameObject.Find("HintSelectionPanel");
        if (hintSelectionPanel != null)
        {
            SetPrivateField(hintManager, "hintSelectionPanel", hintSelectionPanel);
            Debug.Log("Hint Selection Panel を設定しました");
        }
    }
    
    private void SetupVisualSettings()
    {
        // Selected Button Color: 黄色
        SetPrivateField(hintManager, "selectedButtonColor", Color.yellow);
        
        // Normal Button Color: 白
        SetPrivateField(hintManager, "normalButtonColor", Color.white);
        
        Debug.Log("Visual Settings を設定しました");
    }
    
    /// <summary>
    /// 名前でUIコンポーネントを検索
    /// </summary>
    private T FindUIComponent<T>(string objectName) where T : Component
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains(objectName))
            {
                T component = obj.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }
        }
        
        Debug.LogWarning($"{objectName} という名前の {typeof(T).Name} コンポーネントが見つかりません");
        return null;
    }
    
    /// <summary>
    /// リフレクションを使用してprivateフィールドを設定
    /// </summary>
    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public);
            
        if (field != null)
        {
            field.SetValue(target, value);
        }
        else
        {
            Debug.LogWarning($"フィールド '{fieldName}' が見つかりません");
        }
    }
}
