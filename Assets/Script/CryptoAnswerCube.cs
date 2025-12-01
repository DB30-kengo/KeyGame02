using UnityEngine;
using UnityEngine.UI; // Shadow クラス用に追加
using System.Collections;

/// <summary>
/// 暗号学習ゲーム用の3D回答キューブ
/// プレイヤーが触れることで回答を選択
/// </summary>
public class CryptoAnswerCube : MonoBehaviour
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
    public Vector3 textOffset = new Vector3(0, 0, 0.6f);
    
    [Tooltip("テキストのスケール")]
    public Vector3 textScale = new Vector3(0.1f, 0.1f, 0.1f);
    
    [Tooltip("フォントサイズ")]
    public int fontSize = 50;

    [Tooltip("テキストに使用するフォント（Inspectorで指定）")]
    public Font textFont; // 追加：Inspector でフォントを設定可能にする
    
    [Header("エフェクト設定")]
    [Tooltip("選択時のサウンド")]
    public AudioClip selectSound;
    
    [Tooltip("ホバー時のサウンド")]
    public AudioClip hoverSound;
    
    [Tooltip("選択後の消去エフェクト時間")]
    public float disappearDelay = 0.3f;
    
    // コンポーネント参照
    private Renderer cubeRenderer;
    private TextMesh textMesh;
    private AudioSource audioSource;
    private CryptoGameManager gameManager;
    private CryptoUIManager uiManager;
    
    // 状態管理
    private bool isSelected = false;
    private bool isActive = true;
    
    private void Start()
    {
        InitializeComponents();
        SetupTextDisplay();
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
        
        // ゲームマネージャーを検索
        var manager = Object.FindFirstObjectByType<CryptoGameManager>();
        gameManager = manager != null ? manager.GetComponent<CryptoGameManager>() : null;
        
        var uiManager = Object.FindFirstObjectByType<CryptoUIManager>();
        this.uiManager = uiManager != null ? uiManager.GetComponent<CryptoUIManager>() : null;

        var cubeButton = Object.FindFirstObjectByType<CubeButton>();
    }
    
    private void SetupTextDisplay()
    {
        // 既に子に AnswerText があれば再作成しない（再表示時の重複防止）
        Transform existing = transform.Find("AnswerText");
        if (existing != null)
        {
            textMesh = existing.GetComponent<TextMesh>();
            if (textMesh != null)
            {
                textMesh.text = answerText;
                return; // 既存テキストを更新して終了
            }
        }

        // テキスト表示用の子オブジェクト作成
        GameObject textObject = new GameObject("AnswerText");
        textObject.transform.SetParent(transform);
        textObject.transform.localPosition = textOffset;
        textObject.transform.localScale = textScale;
        
        // カメラの方向を向くように設定
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            textObject.transform.LookAt(mainCamera.transform);
            textObject.transform.Rotate(0, 180, 0); // 180度回転して正面を向く
        }
        
        textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = answerText;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontSize = fontSize;
        textMesh.color = Color.white; // 白色に変更（見やすく）
        
        // より確実な日本語フォント設定
        Font selectedFont = null;
        
        // まず Inspector で指定されたフォントを優先
        if (textFont != null)
        {
            selectedFont = textFont;
            Debug.Log($"Inspector指定フォント使用: {selectedFont.name}");
        }
        else
        {
            // システムフォントを試す
            try 
            {
                selectedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch
            {
                Debug.LogWarning("LegacyRuntime.ttfの読み込みに失敗");
            }
            
            // フォントが見つからない場合は利用可能なフォントから選択
            if (selectedFont == null)
            {
                Font[] availableFonts = Resources.FindObjectsOfTypeAll<Font>();
                if (availableFonts.Length > 0)
                {
                    selectedFont = availableFonts[0];
                    Debug.Log($"代替フォント使用: {selectedFont.name}");
                }
            }
        }
        
        if (selectedFont != null)
        {
            textMesh.font = selectedFont;
        }
        
        // テキストレンダラーの設定を改善
        MeshRenderer textRenderer = textObject.GetComponent<MeshRenderer>();
        if (textRenderer != null)
        {
            // デフォルトのTextMeshマテリアルを使用
            if (textMesh.font != null && textMesh.font.material != null)
            {
                textRenderer.material = textMesh.font.material;
            }
            else
            {
                // 基本的なマテリアルを作成
                Material textMaterial = new Material(Shader.Find("Legacy Shaders/Diffuse"));
                textMaterial.color = Color.white;
                textRenderer.material = textMaterial;
            }
            
            // レンダリング設定
            textRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            textRenderer.receiveShadows = false;
            textRenderer.sortingOrder = 1000;
        }
        
        Debug.Log($"テキスト表示設定完了: {answerText}, フォント: {selectedFont?.name}");
    }
    
    private void Update()
    {
        // テキストをカメラの方向に向ける
        if (textMesh != null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                textMesh.transform.LookAt(mainCamera.transform);
                textMesh.transform.Rotate(0, 180, 0);
            }
        }
        else
        {
            // 再有効化時に TextMesh が null なら初期化を試みる
            if (cubeRenderer == null) InitializeComponents();
            if (textMesh == null) SetupTextDisplay();
        }
    }
    
    // トリガー接触（推奨）
    private void OnTriggerEnter(Collider other)
    {
        if (!useTriggerCollider || !isActive) return;
        
        Debug.Log($"トリガー接触検出: {other.name} (タグ: {other.tag})");
        
        if (other.CompareTag(playerTag))
        {
            OnPlayerSelect(); // ホバーではなく即座に選択
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!useTriggerCollider || !isActive) return;
        
        if (other.CompareTag(playerTag))
        {
            OnPlayerExit();
        }
    }
    
    // 物理接触
    private void OnCollisionEnter(Collision collision)
    {
        if (useTriggerCollider || !isActive) return;
        
        Debug.Log($"物理接触検出: {collision.gameObject.name} (タグ: {collision.gameObject.tag})");
        
        if (collision.gameObject.CompareTag(playerTag))
        {
            OnPlayerSelect();
        }
    }
    
    private void OnPlayerEnter()
    {
        if (isSelected) return;
        
        SetMaterial(hoverMaterial);
        PlaySound(hoverSound);
        
        // UI効果
        if (uiManager != null)
        {
            uiManager.PulseEffect(transform, 0.1f, 4f);
        }
        
        Debug.Log($"回答キューブ {answerIndex} にホバー: {answerText}");
    }
    
    private void OnPlayerExit()
    {
        if (isSelected) return;
        
        SetMaterial(normalMaterial);
    }
    
    private void OnPlayerSelect()
    {
        if (isSelected || !isActive) return;
        
        // ゲームマネージャーの状態を確認
        if (gameManager == null)
        {
            Debug.LogWarning("GameManagerが見つかりません。再検索を実行します。");
            gameManager = FindObjectOfType<CryptoGameManager>();
            
            if (gameManager == null)
            {
                Debug.LogError("CryptoGameManagerが見つかりません。ゲームオブジェクトにCryptoGameManagerが設定されているか確認してください。");
                return;
            }
        }
        
        isSelected = true;
        isActive = false;
        
        SetMaterial(selectedMaterial);
        PlaySound(selectSound);
        
        // 選択エフェクト
        if (uiManager != null)
        {
            // 強化された回答選択フィードバック
            uiManager.PlayAnswerSelectionFeedback(transform, true);
            Debug.Log($"[CryptoAnswerCube] アニメーション実行: {answerText}");
        }
        else
        {
            Debug.LogWarning("[CryptoAnswerCube] UIManagerが見つかりません。アニメーションをスキップします。");
        }
        
        Debug.Log($"回答選択準備完了: {answerIndex} - {answerText}");
        
        // ゲームマネージャーに回答を通知
        try
        {
            gameManager.OnAnswerSelected(answerIndex);
            Debug.Log($"回答選択完了: {answerIndex} - {answerText}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"回答選択でエラーが発生: {e.Message}");
            // エラーが発生した場合はキューブをリセット
            ResetCube();
            return;
        }
        
        // 選択後の処理
        StartCoroutine(HandlePostSelection());
    }
    
    private IEnumerator HandlePostSelection()
    {
        // 少し待ってから非表示
        yield return new WaitForSeconds(disappearDelay);
        
        // フェードアウト効果
        yield return StartCoroutine(FadeOut());
        
        // オブジェクトを完全に破棄せず無効化（既存処理を維持）
        gameObject.SetActive(false);
    }
    
    private IEnumerator FadeOut()
    {
        Color originalColor = cubeRenderer.material.color;
        Color textOriginalColor = textMesh.color;
        float fadeTime = 0.5f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
            
            Color newColor = originalColor;
            newColor.a = alpha;
            cubeRenderer.material.color = newColor;
            
            Color newTextColor = textOriginalColor;
            newTextColor.a = alpha;
            textMesh.color = newTextColor;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    
    private void SetMaterial(Material material)
    {
        if (cubeRenderer != null && material != null)
        {
            cubeRenderer.material = material;
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
    
    // 外部からの制御メソッド
    public void SetAnswerText(string newText)
    {
        answerText = newText;
        if (textMesh != null)
        {
            textMesh.text = newText;
        }
    }
    
    public void SetAnswerIndex(int index)
    {
        answerIndex = index;
    }
    
    public void ResetCube()
    {
        isSelected = false;
        isActive = true;
        
        // マテリアルを元に戻す（インスタンス化された material を使う）
        if (cubeRenderer != null && normalMaterial != null)
        {
            cubeRenderer.material = normalMaterial;
            // alpha を確実に戻す
            Color color = cubeRenderer.material.color;
            color.a = 1f;
            cubeRenderer.material.color = color;
        }
        
        gameObject.SetActive(true);
        
        // テキストアルファの復帰
        if (textMesh != null)
        {
            Color textColor = textMesh.color;
            textColor.a = 1f;
            textMesh.color = textColor;
        }
        
        // Collider と Renderer を再有効化（保険）
        foreach (var col in GetComponentsInChildren<Collider>(true))
        {
            try { col.enabled = true; } catch { }
        }
        if (cubeRenderer != null) cubeRenderer.enabled = true;
    }
    
    // 新規：外部（Manager の反射名）から呼ばれる公開リセット
    public void ResetState()
    {
        // Manager が呼べる名前に合わせた公開 API
        ResetCube();
        // 追加の保険処理
        ResetVisualsAndComponents();
    }
    
    // 追加ヘルパー：表示系コンポーネントを明示的に復帰
    private void ResetVisualsAndComponents()
    {
        // Renderer / TextMesh / AudioSource / Collider を確実に復帰
        if (cubeRenderer == null) cubeRenderer = GetComponent<Renderer>();
        if (cubeRenderer != null) cubeRenderer.enabled = true;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        foreach (var col in GetComponentsInChildren<Collider>(true))
        {
            try { col.enabled = true; } catch { }
        }
        if (textMesh == null)
        {
            Transform t = transform.Find("AnswerText");
            if (t != null) textMesh = t.GetComponent<TextMesh>();
            if (textMesh == null) SetupTextDisplay();
        }
        if (textMesh != null)
        {
            Color tc = textMesh.color;
            tc.a = 1f;
            textMesh.color = tc;
        }
    }
    
    // 新規：再アクティブ時の初期化保障
    private void OnEnable()
    {
        // オブジェクトが再び有効になった時に必要な参照を整える
        if (cubeRenderer == null) cubeRenderer = GetComponent<Renderer>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (textMesh == null)
        {
            Transform t = transform.Find("AnswerText");
            if (t != null) textMesh = t.GetComponent<TextMesh>();
        }
        // 状態が不整合ならリセット
        if (!isActive || isSelected)
        {
            ResetCube();
        }
    }
    

    
    // ギズモ表示
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(transform.position, transform.localScale);
        
        // テキスト位置表示
        Gizmos.color = Color.yellow;
        Vector3 textPos = transform.position + transform.TransformDirection(textOffset);
        Gizmos.DrawWireSphere(textPos, 0.1f);
        
        #if UNITY_EDITOR
        Vector3 labelPos = transform.position + Vector3.up * 2f;
        UnityEditor.Handles.Label(labelPos, $"回答 {answerIndex}: {answerText}");
        #endif
    }
}