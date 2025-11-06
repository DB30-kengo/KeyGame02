using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HintSceneの自動セットアップ用スクリプト
/// エディタ上で右クリック → Create Hint Scene Setup で実行可能
/// </summary>
public class HintSceneSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [Tooltip("シーンの自動セットアップを実行")]
    public bool autoSetup = true;
    
    [Header("Generated Objects (Auto-filled)")]
    public Canvas canvas;
    public GameHintManager hintManager;
    public HintUIGenerator uiGenerator;
    
    void Start()
    {
        if (autoSetup)
        {
            SetupHintScene();
        }
    }
    
    /// <summary>
    /// HintSceneの完全セットアップ
    /// </summary>
    [ContextMenu("Setup Hint Scene")]
    public void SetupHintScene()
    {
        Debug.Log("[HintSceneSetup] Starting automatic hint scene setup...");
        
        // 1. Canvasの作成/設定
        SetupCanvas();
        
        // 2. EventSystemの確認
        SetupEventSystem();
        
        // 3. GameHintManagerの作成
        SetupHintManager();
        
        // 4. HintUIGeneratorの作成と実行
        SetupUIGenerator();
        
        Debug.Log("[HintSceneSetup] Hint scene setup completed!");
    }
    
    /// <summary>
    /// Canvasをセットアップ
    /// </summary>
    private void SetupCanvas()
    {
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            Debug.Log("[HintSceneSetup] Canvas created and configured.");
        }
        else
        {
            Debug.Log("[HintSceneSetup] Existing Canvas found.");
        }
    }
    
    /// <summary>
    /// EventSystemをセットアップ
    /// </summary>
    private void SetupEventSystem()
    {
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            Debug.Log("[HintSceneSetup] EventSystem created.");
        }
        else
        {
            Debug.Log("[HintSceneSetup] EventSystem already exists.");
        }
    }
    
    /// <summary>
    /// GameHintManagerをセットアップ
    /// </summary>
    private void SetupHintManager()
    {
        GameObject hintManagerObj = GameObject.Find("HintManager");
        if (hintManagerObj == null)
        {
            hintManagerObj = new GameObject("HintManager");
        }
        
        hintManager = hintManagerObj.GetComponent<GameHintManager>();
        if (hintManager == null)
        {
            hintManager = hintManagerObj.AddComponent<GameHintManager>();
            Debug.Log("[HintSceneSetup] GameHintManager created.");
        }
        else
        {
            Debug.Log("[HintSceneSetup] GameHintManager already exists.");
        }
    }
    
    /// <summary>
    /// HintUIGeneratorをセットアップ
    /// </summary>
    private void SetupUIGenerator()
    {
        GameObject uiGeneratorObj = GameObject.Find("UIGenerator");
        if (uiGeneratorObj == null)
        {
            uiGeneratorObj = new GameObject("UIGenerator");
        }
        
        uiGenerator = uiGeneratorObj.GetComponent<HintUIGenerator>();
        if (uiGenerator == null)
        {
            uiGenerator = uiGeneratorObj.AddComponent<HintUIGenerator>();
        }
        
        // UI自動生成を有効にして実行
        uiGenerator.generateUI = true;
        uiGenerator.parentCanvas = canvas;
        uiGenerator.hintManager = hintManager;
        
        // UIを生成（エディタモードでも実行可能）
        if (Application.isPlaying)
        {
            uiGenerator.GenerateHintUI();
        }
        else
        {
            Debug.Log("[HintSceneSetup] UI will be generated when Play mode starts. You can also call 'Generate Hint UI' from the HintUIGenerator component menu.");
        }
        
        Debug.Log("[HintSceneSetup] HintUIGenerator configured.");
    }
    
    /// <summary>
    /// 手動でUIを生成（エディタから呼び出し可能）
    /// </summary>
    [ContextMenu("Generate UI Now")]
    public void GenerateUIManually()
    {
        if (uiGenerator != null)
        {
            uiGenerator.GenerateHintUI();
            Debug.Log("[HintSceneSetup] UI generated manually.");
        }
        else
        {
            Debug.LogError("[HintSceneSetup] UIGenerator not found. Please run Setup Hint Scene first.");
        }
    }
    
    /// <summary>
    /// 情報表示
    /// </summary>
    [ContextMenu("Show Setup Info")]
    public void ShowSetupInfo()
    {
        Debug.Log($"[HintSceneSetup] Current Setup Status:");
        Debug.Log($"  Canvas: {(canvas != null ? "✓" : "✗")}");
        Debug.Log($"  EventSystem: {(FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null ? "✓" : "✗")}");
        Debug.Log($"  HintManager: {(hintManager != null ? "✓" : "✗")}");
        Debug.Log($"  UIGenerator: {(uiGenerator != null ? "✓" : "✗")}");
    }
}
