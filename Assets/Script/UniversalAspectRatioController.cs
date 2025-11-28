using UnityEngine;
using UnityEngine.UI;

public class UniversalAspectRatioController : MonoBehaviour
{
    [Header("アスペクト比設定")]
    [Tooltip("目標とするアスペクト比（幅/高さ）")]
    public float targetAspectRatio = 16f / 9f;
    
    [Header("制御対象")]
    [Tooltip("制御するカメラ（nullの場合はMainCameraを自動取得）")]
    public Camera targetCamera;
    
    [Tooltip("制御するCanvas（UIのアスペクト比も調整）")]
    public Canvas targetCanvas;
    
    [Header("調整設定")]
    [Tooltip("レターボックス/ピラーボックスを使用するか")]
    public bool useLetterboxing = true;
    
    [Tooltip("レターボックスの背景色")]
    public Color letterboxColor = Color.black;
    
    [Tooltip("Safe Areaを考慮する（モバイル対応）")]
    public bool useSafeArea = true;
    
    [Header("デバッグ情報")]
    [Tooltip("現在のアスペクト比情報を表示")]
    public bool showDebugInfo = false;

    private Camera cam;
    private CanvasScaler canvasScaler;
    private RectTransform canvasRectTransform;
    private float lastScreenWidth;
    private float lastScreenHeight;
    private Rect lastSafeArea;
    
    // デバッグ用GUI
    private string debugInfo = "";

    void Start()
    {
        InitializeComponents();
        UpdateAspectRatio();
        
        // 初期値を記録
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        lastSafeArea = Screen.safeArea;
    }

    void InitializeComponents()
    {
        // カメラの取得
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = FindObjectOfType<Camera>();
            }
        }
        cam = targetCamera;
        
        // Canvasの取得と設定
        if (targetCanvas == null)
        {
            targetCanvas = Object.FindFirstObjectByType<Canvas>();
        }
        
        if (targetCanvas != null)
        {
            canvasScaler = Object.FindFirstObjectByType<CanvasScaler>();
            canvasRectTransform = targetCanvas.GetComponent<RectTransform>();
            
            // CanvasScalerの設定を最適化
            if (canvasScaler != null)
            {
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080); // 16:9基準
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0.5f; // バランス調整
            }
        }
    }

    void Update()
    {
        // 画面サイズまたはSafe Areaが変更された場合
        bool screenChanged = Screen.width != lastScreenWidth || Screen.height != lastScreenHeight;
        bool safeAreaChanged = useSafeArea && !Screen.safeArea.Equals(lastSafeArea);
        
        if (screenChanged || safeAreaChanged)
        {
            UpdateAspectRatio();
            
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            lastSafeArea = Screen.safeArea;
        }
        
        // デバッグ情報の更新
        if (showDebugInfo)
        {
            UpdateDebugInfo();
        }
    }

    void UpdateAspectRatio()
    {
        if (cam == null) return;

        Rect workingArea = useSafeArea ? Screen.safeArea : new Rect(0, 0, Screen.width, Screen.height);
        float currentAspectRatio = workingArea.width / workingArea.height;
        
        if (useLetterboxing)
        {
            ApplyLetterboxing(currentAspectRatio, workingArea);
        }
        else
        {
            ApplyCropping(currentAspectRatio);
        }
        
        // Canvas UIの調整
        UpdateCanvasAspectRatio(currentAspectRatio);
    }

    void ApplyLetterboxing(float currentAspectRatio, Rect workingArea)
    {
        cam.backgroundColor = letterboxColor;
        
        if (currentAspectRatio >= targetAspectRatio)
        {
            // ピラーボックス（左右に黒帯）
            float targetWidth = workingArea.height * targetAspectRatio;
            float widthRatio = targetWidth / workingArea.width;
            float xOffset = (1f - widthRatio) * 0.5f;
            
            cam.rect = new Rect(
                workingArea.x / Screen.width + xOffset,
                workingArea.y / Screen.height,
                widthRatio * workingArea.width / Screen.width,
                workingArea.height / Screen.height
            );
        }
        else
        {
            // レターボックス（上下に黒帯）
            float targetHeight = workingArea.width / targetAspectRatio;
            float heightRatio = targetHeight / workingArea.height;
            float yOffset = (1f - heightRatio) * 0.5f;
            
            cam.rect = new Rect(
                workingArea.x / Screen.width,
                workingArea.y / Screen.height + yOffset,
                workingArea.width / Screen.width,
                heightRatio * workingArea.height / Screen.height
            );
        }
    }

    void ApplyCropping(float currentAspectRatio)
    {
        // 全画面使用
        cam.rect = new Rect(0f, 0f, 1f, 1f);
        
        // FOVを調整してアスペクト比を保持
        if (currentAspectRatio < targetAspectRatio)
        {
            float fovMultiplier = currentAspectRatio / targetAspectRatio;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView * fovMultiplier, 10f, 179f);
        }
    }

    void UpdateCanvasAspectRatio(float currentAspectRatio)
    {
        if (canvasScaler == null) return;
        
        // アスペクト比に基づいてmatchWidthOrHeightを調整
        if (currentAspectRatio > targetAspectRatio)
        {
            // 横長画面：高さ基準
            canvasScaler.matchWidthOrHeight = 1f;
        }
        else
        {
            // 縦長画面：幅基準
            canvasScaler.matchWidthOrHeight = 0f;
        }
    }

    void UpdateDebugInfo()
    {
        Rect safeArea = Screen.safeArea;
        float currentAspect = (float)Screen.width / Screen.height;
        float safeAspect = safeArea.width / safeArea.height;
        
        debugInfo = $"画面解像度: {Screen.width}x{Screen.height}\n" +
                   $"現在のアスペクト比: {currentAspect:F2}\n" +
                   $"目標アスペクト比: {targetAspectRatio:F2}\n" +
                   $"Safe Area: {safeArea}\n" +
                   $"Safe Areaアスペクト比: {safeAspect:F2}\n" +
                   $"カメラRect: {(cam != null ? cam.rect.ToString() : "null")}";
    }

    void OnGUI()
    {
        if (showDebugInfo && !string.IsNullOrEmpty(debugInfo))
        {
            GUI.Box(new Rect(10, 10, 400, 150), debugInfo);
        }
    }

    // 公開メソッド
    public void SetTargetAspectRatio(float newRatio)
    {
        targetAspectRatio = newRatio;
        UpdateAspectRatio();
    }

    public void ToggleLetterboxing()
    {
        useLetterboxing = !useLetterboxing;
        UpdateAspectRatio();
    }
    
    // 一般的なアスペクト比プリセット
    [ContextMenu("16:9 アスペクト比")]
    public void SetAspectRatio16_9() => SetTargetAspectRatio(16f / 9f);
    
    [ContextMenu("4:3 アスペクト比")]
    public void SetAspectRatio4_3() => SetTargetAspectRatio(4f / 3f);
    
    [ContextMenu("1:1 アスペクト比")]
    public void SetAspectRatio1_1() => SetTargetAspectRatio(1f);
    
    [ContextMenu("21:9 アスペクト比（ウルトラワイド）")]
    public void SetAspectRatio21_9() => SetTargetAspectRatio(21f / 9f);
    
    [ContextMenu("18:9 アスペクト比（モバイル）")]
    public void SetAspectRatio18_9() => SetTargetAspectRatio(18f / 9f);
}