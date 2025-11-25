using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// ボタンクリックでシーン切り替えを行うスクリプト
/// インスペクター上でシーン名を設定可能
/// </summary>
public class SceneTransitionButton : MonoBehaviour
{
    [Header("シーン切り替え設定")]
    [Tooltip("切り替え先のシーン名を入力してください")]
    public string targetSceneName = "";
    
    [Header("トランジション設定")]
    [Tooltip("シーン切り替え時にフェードアウト効果を使用するか")]
    public bool useFadeTransition = true;
    
    [Tooltip("フェードアウトの時間（秒）")]
    [Range(0.1f, 3.0f)]
    public float fadeTime = 1.0f;
    
    [Header("ボタン設定")]
    [Tooltip("ボタンコンポーネント（自動取得されますが、手動設定も可能）")]
    public Button targetButton;
    
    [Header("確認ダイアログ設定")]
    [Tooltip("シーン切り替え前に確認ダイアログを表示するか")]
    public bool showConfirmationDialog = false;
    
    [Tooltip("確認ダイアログのメッセージ")]
    [TextArea(2, 4)]
    public string confirmationMessage = "このシーンに移動しますか？";
    
    [Header("デバッグ設定")]
    [Tooltip("デバッグログを表示するか")]
    public bool enableDebugLog = true;
    
    // フェード用のパネル
    private GameObject fadePanel;
    private CanvasGroup fadeCanvasGroup;
    
    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Start()
    {
        InitializeButton();
        CreateFadePanel();
    }
    
    /// <summary>
    /// ボタンの初期化
    /// </summary>
    private void InitializeButton()
    {
        // ボタンが設定されていない場合、自動取得
        if (targetButton == null)
        {
            targetButton = GetComponent<Button>();
        }
        
        // ボタンが見つからない場合のエラーハンドリング
        if (targetButton == null)
        {
            Debug.LogError($"[SceneTransitionButton] ボタンコンポーネントが見つかりません！ GameObject: {gameObject.name}");
            return;
        }
        
        // ボタンクリックイベントを登録
        targetButton.onClick.AddListener(OnButtonClicked);
        
        if (enableDebugLog)
        {
            Debug.Log($"[SceneTransitionButton] ボタン初期化完了: {gameObject.name} -> {targetSceneName}");
        }
    }
    
    /// <summary>
    /// フェード用パネルを作成
    /// </summary>
    private void CreateFadePanel()
    {
        if (!useFadeTransition) return;
        
        // 親Canvasを取得
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null) return;
        
        // フェードパネル作成
        fadePanel = new GameObject("FadePanel");
        fadePanel.transform.SetParent(parentCanvas.transform, false);
        
        // RectTransform設定（全画面）
        RectTransform rectTransform = fadePanel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Image設定（黒色）
        Image image = fadePanel.AddComponent<Image>();
        image.color = Color.black;
        
        // CanvasGroup設定
        fadeCanvasGroup = fadePanel.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        
        // 初期状態で非表示
        fadePanel.SetActive(false);
        
        if (enableDebugLog)
        {
            Debug.Log("[SceneTransitionButton] フェードパネル作成完了");
        }
    }
    
    /// <summary>
    /// ボタンクリック時の処理
    /// </summary>
    public void OnButtonClicked()
    {
        if (enableDebugLog)
        {
            Debug.Log($"[SceneTransitionButton] ボタンがクリックされました: {gameObject.name}");
        }
        
        // シーン名の検証
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[SceneTransitionButton] シーン名が設定されていません！");
            return;
        }
        
        // 確認ダイアログ表示
        if (showConfirmationDialog)
        {
            ShowConfirmationDialog();
        }
        else
        {
            StartSceneTransition();
        }
    }
    
    /// <summary>
    /// 確認ダイアログを表示
    /// </summary>
    private void ShowConfirmationDialog()
    {
        // シンプルな確認ダイアログ（Unity Editorでのみ動作）
        #if UNITY_EDITOR
        if (UnityEditor.EditorUtility.DisplayDialog("シーン切り替え確認", confirmationMessage, "はい", "いいえ"))
        {
            StartSceneTransition();
        }
        #else
        // ビルド版では直接実行（実際のプロジェクトではカスタムダイアログを使用）
        StartSceneTransition();
        #endif
    }
    
    /// <summary>
    /// シーン切り替え開始
    /// </summary>
    public void StartSceneTransition()
    {
        if (enableDebugLog)
        {
            Debug.Log($"[SceneTransitionButton] シーン切り替え開始: {targetSceneName}");
        }
        
        StartCoroutine(TransitionToScene());
    }
    
    /// <summary>
    /// シーン切り替えコルーチン
    /// </summary>
    private IEnumerator TransitionToScene()
    {
        // フェード効果
        if (useFadeTransition && fadePanel != null)
        {
            yield return StartCoroutine(FadeOut());
        }
        
        // シーンが存在するかチェック
        if (!IsSceneInBuildSettings(targetSceneName))
        {
            Debug.LogError($"[SceneTransitionButton] シーン '{targetSceneName}' がBuild Settingsに含まれていません！");
            
            // フェードインで元に戻す
            if (useFadeTransition && fadePanel != null)
            {
                yield return StartCoroutine(FadeIn());
            }
            yield break;
        }
        
        // シーン読み込み
        bool loadSuccess = false;
        System.Exception loadException = null;
        
        try
        {
            SceneManager.LoadScene(targetSceneName);
            loadSuccess = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SceneTransitionButton] シーン読み込みエラー: {e.Message}");
            loadException = e;
            loadSuccess = false;
        }
        
        // エラーが発生した場合のフェードイン処理
        if (!loadSuccess && useFadeTransition && fadePanel != null)
        {
            yield return StartCoroutine(FadeIn());
        }
    }
    
    /// <summary>
    /// フェードアウト効果
    /// </summary>
    private IEnumerator FadeOut()
    {
        fadePanel.SetActive(true);
        fadeCanvasGroup.blocksRaycasts = true;
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeTime);
            yield return null;
        }
        
        fadeCanvasGroup.alpha = 1f;
    }
    
    /// <summary>
    /// フェードイン効果
    /// </summary>
    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeTime));
            yield return null;
        }
        
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        fadePanel.SetActive(false);
    }
    
    /// <summary>
    /// シーンがBuild Settingsに含まれているかチェック
    /// </summary>
    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneNameFromPath == sceneName)
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// デストラクタ（リスナーをクリーンアップ）
    /// </summary>
    private void OnDestroy()
    {
        if (targetButton != null)
        {
            targetButton.onClick.RemoveListener(OnButtonClicked);
        }
    }
    
    /// <summary>
    /// 外部からシーン名を設定（スクリプトから呼び出し可能）
    /// </summary>
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
        
        if (enableDebugLog)
        {
            Debug.Log($"[SceneTransitionButton] ターゲットシーン設定: {sceneName}");
        }
    }
    
    /// <summary>
    /// 即座にシーン切り替え（フェード効果なし）
    /// </summary>
    public void LoadSceneImmediate()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[SceneTransitionButton] シーン名が設定されていません！");
            return;
        }
        
        if (!IsSceneInBuildSettings(targetSceneName))
        {
            Debug.LogError($"[SceneTransitionButton] シーン '{targetSceneName}' がBuild Settingsに含まれていません！");
            return;
        }
        
        SceneManager.LoadScene(targetSceneName);
    }
    
    /// <summary>
    /// エディタ用：ボタンのテスト
    /// </summary>
    [ContextMenu("Test Button Click")]
    public void TestButtonClick()
    {
        if (Application.isPlaying)
        {
            OnButtonClicked();
        }
        else
        {
            Debug.Log($"[SceneTransitionButton] テスト: {targetSceneName} に切り替える予定");
        }
    }
}
