using UnityEngine;
using UnityEngine.Rendering;

public class AspectRatioController : MonoBehaviour
{
    [Header("アスペクト比設定")]
    [Tooltip("目標とするアスペクト比（幅/高さ）")]
    public float targetAspectRatio = 16f / 9f; // デフォルト16:9
    
    [Tooltip("カメラの参照（空の場合は自動でMainCameraを取得）")]
    public Camera targetCamera;
    
    [Header("調整方法")]
    [Tooltip("true: レターボックス/ピラーボックス, false: クロップ")]
    public bool useLetterboxing = true;
    
    [Header("背景色（レターボックス使用時）")]
    public Color backgroundColor = Color.black;

    private Camera cam;
    private float lastScreenWidth;
    private float lastScreenHeight;

    void Start()
    {
        // カメラの参照を取得
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = FindObjectOfType<Camera>();
            }
        }
        
        cam = targetCamera;
        
        if (cam == null)
        {
            Debug.LogError("AspectRatioController: カメラが見つかりません！");
            return;
        }

        // 初期設定
        UpdateCameraAspect();
        
        // 画面サイズを記録
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

    void Update()
    {
        // 画面サイズが変わった場合のみ更新
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            UpdateCameraAspect();
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }
    }

    void UpdateCameraAspect()
    {
        if (cam == null) return;

        float currentAspectRatio = (float)Screen.width / (float)Screen.height;
        
        if (useLetterboxing)
        {
            // レターボックス/ピラーボックス方式
            if (currentAspectRatio >= targetAspectRatio)
            {
                // 画面が目標より横長 → ピラーボックス（左右に黒帯）
                float width = targetAspectRatio / currentAspectRatio;
                float height = 1f;
                
                cam.rect = new Rect((1f - width) / 2f, 0f, width, height);
            }
            else
            {
                // 画面が目標より縦長 → レターボックス（上下に黒帯）
                float width = 1f;
                float height = currentAspectRatio / targetAspectRatio;
                
                cam.rect = new Rect(0f, (1f - height) / 2f, width, height);
            }
            
            // 背景色を設定
            cam.backgroundColor = backgroundColor;
        }
        else
        {
            // クロップ方式（画面全体を使用し、必要に応じて切り取り）
            cam.rect = new Rect(0f, 0f, 1f, 1f);
            
            // Field of Viewを調整してアスペクト比を維持
            if (currentAspectRatio < targetAspectRatio)
            {
                // 画面が縦長の場合、FOVを狭めて対応
                float fovMultiplier = currentAspectRatio / targetAspectRatio;
                cam.fieldOfView = cam.fieldOfView * fovMultiplier;
            }
        }
    }
    
    // エディタでのリアルタイム調整用
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateCameraAspect();
        }
    }
    
    // 目標アスペクト比を動的に変更するメソッド
    public void SetTargetAspectRatio(float newAspectRatio)
    {
        targetAspectRatio = newAspectRatio;
        UpdateCameraAspect();
    }
    
    // よく使用されるアスペクト比のプリセット
    public void SetAspectRatio16_9() { SetTargetAspectRatio(16f / 9f); }
    public void SetAspectRatio4_3() { SetTargetAspectRatio(4f / 3f); }
    public void SetAspectRatio1_1() { SetTargetAspectRatio(1f / 1f); }
    public void SetAspectRatio21_9() { SetTargetAspectRatio(21f / 9f); }

    private void OnPreRender()
    {
        // ここに処理を追加すると、カメラの描画前に実行されます
    }

    private void OnPostRender()
    {
        // ここに処理を追加すると、カメラの描画後に実行されます
    }
}