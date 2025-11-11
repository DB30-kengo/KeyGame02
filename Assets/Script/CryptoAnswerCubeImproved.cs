using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 暗号学習ゲーム用の3D回答キューブ（改良版）
/// TextMeshProを使用してより確実にテキストを表示
/// </summary>
public class CryptoAnswerCubeImproved : MonoBehaviour
{
    [Header("回答設定")]
    [Tooltip("この回答の番号（0 or 1）")]
    public int answerIndex = 0;
    
    [Tooltip("回答テキスト")]
    public string answerText = "回答1";
    
    [Header("インタラクション設定")]
    [Tooltip("接触を検出するタグ")]
    public string playerTag = "Player";
    
    [Tooltip("トリガー接触を使用するか")]
    public bool useTriggerCollider = true;
    
    [Header("ビジュアル設定")]
    [Tooltip("通常時のマテリアル")]
    public Material normalMaterial;
    
    [Tooltip("ホバー時のマテリアル")]
    public Material hoverMaterial;
    
    [Tooltip("選択時のマテリアル")]
    public Material selectedMaterial;
    
    [Header("テキスト表示")]
    [Tooltip("テキスト表示位置のオフセット")]
    public Vector3 textOffset = new Vector3(0, 1, 0);
    
    [Tooltip("テキストのスケール")]
    public float textSize = 2f;
    
    // コンポーネント参照
    private Renderer cubeRenderer;
    private GameObject textCanvas;
    private Text uiText;
    private AudioSource audioSource;
    private CryptoGameManager gameManager;
    private CryptoUIManager uiManager;
    
    // 状態管理
    private bool isSelected = false;
    private bool isActive = true;
    
    private void Start()
    {
        InitializeComponents();
        SetupUITextDisplay();
        SetMaterial(normalMaterial);
    }
    
    private void InitializeComponents()
    {
        cubeRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        gameManager = FindObjectOfType<CryptoGameManager>();
        uiManager = FindObjectOfType<CryptoUIManager>();
    }
    
    private void SetupUITextDisplay()
    {
        try
        {
            // WorldSpace Canvasを使用してより確実にテキスト表示
            textCanvas = new GameObject("TextCanvas");
            textCanvas.transform.SetParent(transform);
            textCanvas.transform.localPosition = textOffset;
            textCanvas.transform.localScale = Vector3.one;
            
            // Canvas設定
            Canvas canvas = textCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 1000;
            
            // WorldSpaceキャンバスのサイズ設定
            RectTransform canvasRect = textCanvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(2, 1); // 幅2、高さ1の世界座標サイズ
            
            // テキストオブジェクト作成
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(textCanvas.transform, false);
            
            // RectTransform設定
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            // Textコンポーネント追加
            uiText = textObject.AddComponent<Text>();
            
            // フォントの確実な設定
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                // LegacyRuntimeフォントを試す
                try 
                {
                    font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
                catch 
                {
                    Debug.LogWarning("デフォルトフォントの取得に失敗");
                }
            }
            
            uiText.font = font;
            uiText.text = answerText;
            uiText.fontSize = 36; // 大きめのフォントサイズ
            uiText.color = Color.black; // 黒色テキスト
            uiText.alignment = TextAnchor.MiddleCenter;
            uiText.horizontalOverflow = HorizontalWrapMode.Overflow;
            uiText.verticalOverflow = VerticalWrapMode.Overflow;
            uiText.fontStyle = FontStyle.Bold; // 太字で見やすく
            
            // 白い背景を追加（確実に見えるように）
            Image background = textObject.AddComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0.9f); // 白い半透明背景
            
            Debug.Log($"UIテキスト設定完了: {answerText}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"UIテキスト表示設定中にエラーが発生: {ex.Message}");
            SetupFallbackTextMesh();
        }
    }
    
    private void SetupFallbackTextMesh()
    {
        try
        {
            Debug.Log("フォールバックTextMesh設定を開始します");
            
            // シンプルなTextMesh使用
            GameObject textObject = new GameObject("FallbackText");
            textObject.transform.SetParent(transform);
            textObject.transform.localPosition = textOffset;
            textObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            
            TextMesh textMesh = textObject.AddComponent<TextMesh>();
            textMesh.text = answerText;
            textMesh.fontSize = 50;
            textMesh.color = Color.white;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            
            // フォント設定
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                textMesh.font = font;
            }
            
            Debug.Log("フォールバックTextMesh設定完了");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"フォールバック設定も失敗: {ex.Message}");
        }
    }
    
    private void Update()
    {
        // キャンバスをカメラの方向に向ける
        if (textCanvas != null && Camera.main != null)
        {
            textCanvas.transform.LookAt(Camera.main.transform);
            textCanvas.transform.Rotate(0, 180, 0);
        }
    }
    
    // ...existing trigger and collision methods...
    
    private void OnTriggerEnter(Collider other)
    {
        if (!useTriggerCollider || !isActive) return;
        
        Debug.Log($"トリガー接触検出: {other.name} (タグ: {other.tag})");
        
        if (other.CompareTag(playerTag))
        {
            OnPlayerSelect();
        }
    }
    
    private void OnPlayerSelect()
    {
        if (isSelected || !isActive) return;
        
        isSelected = true;
        isActive = false;
        
        SetMaterial(selectedMaterial);
        
        if (gameManager != null)
        {
            gameManager.OnAnswerSelected(answerIndex);
        }
        
        Debug.Log($"回答選択: {answerIndex} - {answerText}");
        
        StartCoroutine(HandlePostSelection());
    }
    
    private IEnumerator HandlePostSelection()
    {
        yield return new WaitForSeconds(0.3f);
        gameObject.SetActive(false);
    }
    
    private void SetMaterial(Material material)
    {
        if (cubeRenderer != null && material != null)
        {
            cubeRenderer.material = material;
        }
    }
    
    public void SetAnswerText(string newText)
    {
        answerText = newText;
        if (uiText != null)
        {
            uiText.text = newText;
        }
    }
    
    public void SetAnswerIndex(int index)
    {
        answerIndex = index;
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
        gameObject.SetActive(active);
        
        if (active)
        {
            isSelected = false;
            SetMaterial(normalMaterial);
        }
    }
}