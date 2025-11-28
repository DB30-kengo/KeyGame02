using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// 順序付きオブジェクトインタラクションを管理するマネージャー
/// </summary>
public class SequentialObjectsManager : MonoBehaviour
{
    [Header("基本設定")]
    [Tooltip("現在の進行段階（0=まだ何も触れていない）")]
    public int currentStage = 0;
    
    [Tooltip("最大段階数（オブジェクト総数）")]
    public int maxStages = 5; // 5ステージに設定
    
    [Header("メッセージUI設定")]
    [Tooltip("メッセージを表示するテキストコンポーネント")]
    public Text messageText;
    
    [Tooltip("メッセージのフェードイン時間")]
    public float fadeInTime = 0.5f;
    
    [Header("ゲームオーバー設定")]
    [Tooltip("順番を間違えた時に表示するUIキャンバス")]
    public GameObject gameOverCanvas;
    
    [Tooltip("ゲームオーバー後、シーン切り替えまでの時間（秒）")]
    public float sceneChangeDelay = 3.0f;
    
    [Tooltip("ゲームオーバー後に移動するシーン名")]
    public string gameOverSceneName = "GameOver";
    
    [Header("ゲームクリア設定")]
    [Tooltip("ステージ5に到達した時に表示するゲームクリアキャンバス")]
    public GameObject gameClearCanvas;
    
    [Tooltip("ゲームクリアメッセージ")]
    [TextArea(2, 3)]
    public string gameClearMessage = "ゲームクリア！おめでとう！";
    
    [Tooltip("ゲームクリア後、シーン切り替えまでの時間（秒）")]
    public float clearSceneChangeDelay = 5.0f;
    
    [Tooltip("ゲームクリア後に移動するシーン名（空白の場合は移動しない）")]
    public string clearSceneName = "";
    
    [Header("UI検索設定")]
    [Tooltip("ゲームオーバーキャンバスの名前（タグが見つからない場合に使用）")]
    public string gameOverCanvasName = "GameOverCanvas";
    
    [Tooltip("ゲームクリアキャンバスの名前（タグが見つからない場合に使用）")]
    public string gameClearCanvasName = "GameClearCanvas";
    
    [Tooltip("メッセージテキストの名前（タグが見つからない場合に使用）")]
    public string messageTextName = "MessageText";
    
    [Header("段階別メッセージ")]
    [Tooltip("初期メッセージ（シーン読み込み時に表示）")]
    [TextArea(2, 3)]
    public string initialMessage = "目的地を目指そう！";
    
    [Tooltip("1つ目のオブジェクトに触れた時のメッセージ")]
    [TextArea(2, 3)]
    public string stage1Message = "1つ目のオブジェクトを見つけました！";
    
    [Tooltip("2つ目のオブジェクトに触れた時のメッセージ")]
    [TextArea(2, 3)]
    public string stage2Message = "2つ目のオブジェクトを見つけました！";
    
    [Tooltip("3つ目のオブジェクトに触れた時のメッセージ")]
    [TextArea(2, 3)]
    public string stage3Message = "3つ目のオブジェクトを見つけました！";
    
    [Tooltip("4つ目のオブジェクトに触れた時のメッセージ")]
    [TextArea(2, 3)]
    public string stage4Message = "すべてのオブジェクトを見つけました！";
    
    [Tooltip("5つ目のオブジェクトに触れた時のメッセージ")]
    [TextArea(2, 3)]
    public string stage5Message = "最後のオブジェクトを見つけました！";
    
    [Tooltip("ゲームオーバー時のメッセージ")]
    [TextArea(2, 3)]
    public string gameOverMessage = "順番を間違えました...";
    
    // シングルトンインスタンス
    public static SequentialObjectsManager Instance { get; private set; }
    
    // 内部変数
    private Coroutine currentMessageCoroutine;
    private bool isGameCleared = false;
    
    // シーン間で保持する状態変数
    [HideInInspector]
    public bool shouldShowGameOver = false;
    
    [HideInInspector]
    public bool shouldShowGameClear = false;
    
    private void Awake()
    {
        // シングルトンパターンを修正
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // シーン切り替えイベントのリスナーを登録
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // ゲームオーバーキャンバスを初期状態で非表示
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(false);
        }
        
        // ゲームクリアキャンバスを初期状態で非表示
        if (gameClearCanvas != null)
        {
            gameClearCanvas.SetActive(false);
        }
    }
    
    private void Start()
    {
        // 初期メッセージを表示
        DisplayMessage(initialMessage);
    }
    
    // シーン読み込み時に呼ばれるメソッド
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("シーンがロードされました: " + scene.name);
        
        // UIの参照を再設定
        SetupReferences();
        
        // シーンが更新されたとき、ゲームオーバーやゲームクリア状態でなければ初期メッセージを表示
        if (!shouldShowGameOver && !shouldShowGameClear)
        {
            // 初期メッセージを表示
            DisplayMessage(initialMessage);
        }
        
        // シーン読み込み後、少し待ってから状態を復元（UIが完全にロードされるのを待つ）
        StartCoroutine(RestoreStateAfterDelay(0.5f));
    }
    
    // 遅延して状態を復元するコルーチン - 修正版
    private IEnumerator RestoreStateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // ゲームオーバー状態の復元
        if (shouldShowGameOver)
        {
            Debug.Log("ゲームオーバー状態を復元します");
            
            // ゲームオーバーメッセージを表示
            DisplayMessage(gameOverMessage);
            
            // ゲームオーバーキャンバスを表示
            if (gameOverCanvas != null)
            {
                // キャンバスの表示を確実に行う追加処理
                Canvas canvasComponent = gameOverCanvas.GetComponent<Canvas>();
                if (canvasComponent != null)
                {
                    canvasComponent.enabled = true;
                    
                    // キャンバスの優先度を高くする
                    canvasComponent.sortingOrder = 10;
                    Debug.Log("キャンバスコンポーネントを有効化、優先度を設定しました");
                }
                
                // CanvasGroupがあれば透明度を1に設定
                CanvasGroup canvasGroup = gameOverCanvas.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                    Debug.Log("CanvasGroupの透明度を1に設定しました");
                }
                
                // キャンバス内のすべての子オブジェクトも有効化
                foreach (Transform child in gameOverCanvas.transform)
                {
                    child.gameObject.SetActive(true);
                }
                
                // キャンバス自体を有効化
                gameOverCanvas.SetActive(true);
                Debug.Log("ゲームオーバーキャンバスを表示しました: " + gameOverCanvas.name);
                
                // 強制的にキャンバスを最前面に表示
                if (gameOverCanvas.transform.parent != null)
                {
                    gameOverCanvas.transform.SetAsLastSibling();
                }
            }
            else
            {
                Debug.LogWarning("ゲームオーバーキャンバスが見つかりません");
            }
            
            // フラグをリセット
            shouldShowGameOver = false;
        }
        
        // ゲームクリア状態の復元
        if (shouldShowGameClear)
        {
            Debug.Log("ゲームクリア状態を復元します");
            isGameCleared = true;
            
            // ゲームクリアメッセージを表示
            DisplayMessage(gameClearMessage);
            
            // ゲームクリアキャンバスを表示
            if (gameClearCanvas != null)
            {
                // キャンバスの表示を確実に行う追加処理
                Canvas canvasComponent = gameClearCanvas.GetComponent<Canvas>();
                if (canvasComponent != null)
                {
                    canvasComponent.enabled = true;
                    
                    // キャンバスの優先度を高くする
                    canvasComponent.sortingOrder = 10;
                    Debug.Log("キャンバスコンポーネントを有効化、優先度を設定しました");
                }
                
                // CanvasGroupがあれば透明度を1に設定
                CanvasGroup canvasGroup = gameClearCanvas.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                    Debug.Log("CanvasGroupの透明度を1に設定しました");
                }
                
                // キャンバス内のすべての子オブジェクトも有効化
                foreach (Transform child in gameClearCanvas.transform)
                {
                    child.gameObject.SetActive(true);
                }
                
                // キャンバス自体を有効化
                gameClearCanvas.SetActive(true);
                Debug.Log("ゲームクリアキャンバスを表示しました: " + gameClearCanvas.name);
                
                // 強制的にキャンバスを最前面に表示
                if (gameClearCanvas.transform.parent != null)
                {
                    gameClearCanvas.transform.SetAsLastSibling();
                }
            }
            else
            {
                Debug.LogWarning("ゲームクリアキャンバスが見つかりません");
            }
            
            // フラグをリセット
            shouldShowGameClear = false;
        }
    }
    
    // OnDestroyでリスナーを解除
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// オブジェクトに触れた時のインタラクションを処理
    /// </summary>
    /// <param name="stageNumber">オブジェクトの段階番号（1からスタート）</param>
    /// <returns>正しい順番だったかどうか</returns>
    public bool InteractWithObject(int stageNumber)
    {
        // ゲームクリア済みなら何もしない
        if (isGameCleared) return true;
        
        // 正しい順番かチェック
        if (stageNumber == currentStage + 1)
        {
            // 正しい順番の場合
            currentStage = stageNumber;
            
            // 段階に応じたメッセージを表示
            string message = GetStageMessage(stageNumber);
            DisplayMessage(message);
            
            Debug.Log($"正しい順番: ステージ {stageNumber} に進みました");
            
            // ステージ5（最終ステージ）に到達したらゲームクリア
            if (stageNumber == 5)
            {
                TriggerGameClear();
            }
            
            return true;
        }
        else
        {
            // 間違った順番の場合、特定の条件でのみゲームオーバーにする
            if (IsGameOverSequence(currentStage, stageNumber))
            {
                Debug.Log($"ゲームオーバーになる順番違反: 現在 {currentStage} から {stageNumber} に触れました");
                return false; // ゲームオーバー
            }
            else
            {
                // その他の順番違反は無視する（何も起きない）
                Debug.Log($"無視される順番違反: 現在 {currentStage} から {stageNumber} に触れました");
                return true; // ゲームオーバーにしない
            }
        }
    }
    
    /// <summary>
    /// 特定の順序違反がゲームオーバーになるかどうかをチェック
    /// </summary>
    /// <param name="currentStage">現在のステージ</param>
    /// <param name="attemptedStage">触れようとしたステージ</param>
    /// <returns>ゲームオーバーになるべき順序違反かどうか</returns>
    public bool IsGameOverSequence(int currentStage, int attemptedStage)
    {
        // 1→3 の場合または 0→3 の場合にゲームオーバーにする
        return (currentStage == 1 && attemptedStage == 3) || 
               (currentStage == 0 && attemptedStage == 3);
    }
    
    /// <summary>
    /// ゲームオーバー処理
    /// </summary>
    public void TriggerGameOver()
    {
        // すでにゲームオーバー処理中なら何もしない
        if (gameOverCanvas != null && gameOverCanvas.activeInHierarchy)
            return;
            
        Debug.Log("ゲームオーバー処理を開始します");
        
        // ゲームオーバーメッセージを表示
        DisplayMessage(gameOverMessage);
        
        // ゲームオーバーキャンバスを表示
        if (gameOverCanvas != null)
        {
            // 即時にキャンバスを有効化して確実に表示
            // キャンバスコンポーネントを有効化
            Canvas canvasComponent = gameOverCanvas.GetComponent<Canvas>();
            if (canvasComponent != null)
            {
                canvasComponent.enabled = true;
                canvasComponent.sortingOrder = 10;
            }
            
            // CanvasGroupがあれば透明度を1に設定
            CanvasGroup canvasGroup = gameOverCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            
            // 子オブジェクトをすべて有効化
            foreach (Transform child in gameOverCanvas.transform)
            {
                child.gameObject.SetActive(true);
            }
            
            // キャンバス自体を有効化
            gameOverCanvas.SetActive(true);
            
            // 最前面に表示
            if (gameOverCanvas.transform.parent != null)
            {
                gameOverCanvas.transform.SetAsLastSibling();
            }
        }
        
        // シーン切り替え用フラグを設定
        shouldShowGameOver = true;
        
        // 一定時間後にシーンを切り替え
        StartCoroutine(LoadGameOverScene());
    }
    
    /// <summary>
    /// ゲームクリア処理
    /// </summary>
    public void TriggerGameClear()
    {
        // すでにゲームクリア処理中なら何もしない
        if (isGameCleared) return;
        
        isGameCleared = true;
        Debug.Log("ゲームクリア処理を開始します");
        
        // ゲームクリアメッセージを表示
        DisplayMessage(gameClearMessage);
        
        // ゲームクリアキャンバスを表示
        if (gameClearCanvas != null)
        {
            // 即時にキャンバスを有効化して確実に表示
            // キャンバスコンポーネントを有効化
            Canvas canvasComponent = gameClearCanvas.GetComponent<Canvas>();
            if (canvasComponent != null)
            {
                canvasComponent.enabled = true;
                canvasComponent.sortingOrder = 10;
            }
            
            // CanvasGroupがあれば透明度を1に設定
            CanvasGroup canvasGroup = gameClearCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            
            // 子オブジェクトをすべて有効化
            foreach (Transform child in gameClearCanvas.transform)
            {
                child.gameObject.SetActive(true);
            }
            
            // キャンバス自体を有効化
            gameClearCanvas.SetActive(true);
            
            // 最前面に表示
            if (gameClearCanvas.transform.parent != null)
            {
                gameClearCanvas.transform.SetAsLastSibling();
            }
        }
        
        // シーン切り替え用フラグを設定
        shouldShowGameClear = true;
        
        // 次のシーンが設定されている場合、一定時間後に移動
        if (!string.IsNullOrEmpty(clearSceneName))
        {
            StartCoroutine(LoadClearScene());
        }
    }
    
    // ゲームオーバーシーンに移動するコルーチン
    private IEnumerator LoadGameOverScene()
    {
        yield return new WaitForSeconds(sceneChangeDelay);
        
        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            Debug.Log("ゲームオーバーシーンに移動します: " + gameOverSceneName);
            
            // シーンを再読み込み
            SceneManager.LoadScene(gameOverSceneName);
        }
    }
    
    // ゲームクリアシーンに移動するコルーチン
    private IEnumerator LoadClearScene()
    {
        yield return new WaitForSeconds(clearSceneChangeDelay);
        
        if (!string.IsNullOrEmpty(clearSceneName))
        {
            Debug.Log("クリアシーンに移動します: " + clearSceneName);
            
            // シーンを再読み込み
            SceneManager.LoadScene(clearSceneName);
        }
    }
    
    // 状態をリセットするメソッド
    public void ResetState()
    {
        // 以下のリセットはシーン切り替え時には行わない
        if (!shouldShowGameOver && !shouldShowGameClear)
        {
            // 通常のリセット処理
            currentStage = 0;
            isGameCleared = false;
            
            // キャンバスを非表示
            if (gameOverCanvas != null)
            {
                gameOverCanvas.SetActive(false);
            }
            
            if (gameClearCanvas != null)
            {
                gameClearCanvas.SetActive(false);
            }
            
            // メッセージテキストを初期状態に戻す
            if (messageText != null)
            {
                DisplayMessage(initialMessage);
            }
        }
        
        // コルーチンのリセットは常に行う
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
            currentMessageCoroutine = null;
        }
    }
    
    // UI参照を再設定するメソッド（改善版）
    private void SetupReferences()
    {
        Debug.Log("UI参照を再設定しています");
        
        // メッセージテキストの参照を探す
        if (messageText == null)
        {
            // タグで検索
            GameObject messageObj = null;
            try
            {
                messageObj = GameObject.FindGameObjectWithTag("MessageText");
            }
            catch (UnityException)
            {
                Debug.LogWarning("MessageTextタグが存在しません");
            }
            
            if (messageObj != null)
            {
                messageText = messageObj.GetComponent<Text>();
                Debug.Log("MessageTextをタグで見つけました");
            }
            else
            {
                // 名前で検索
                GameObject textObj = GameObject.Find(messageTextName);
                if (textObj != null)
                {
                    messageText = textObj.GetComponent<Text>();
                    if (messageText != null)
                    {
                        Debug.Log("MessageTextを名前で見つけました");
                    }
                }
                else
                {
                    // シーン内のすべてのTextを検索
                    Text[] allTexts = FindObjectsByType<Text>(FindObjectsSortMode.None);
                    if (allTexts.Length > 0)
                    {
                        messageText = allTexts[0]; // 最初に見つかったTextを使用
                        Debug.LogWarning("最初に見つかったTextコンポーネントをメッセージ表示に使用します");
                    }
                    else
                    {
                        Debug.LogWarning("メッセージ表示用のTextが見つかりません。インスペクターで手動設定してください。");
                    }
                }
            }
        }
        
        // ゲームオーバーキャンバスの参照を探す
        if (gameOverCanvas == null)
        {
            // タグで検索
            GameObject overCanvas = null;
            try
            {
                overCanvas = GameObject.FindGameObjectWithTag("GameOverCanvas");
            }
            catch (UnityException)
            {
                Debug.LogWarning("GameOverCanvasタグが存在しません");
            }
            
            if (overCanvas != null)
            {
                gameOverCanvas = overCanvas;
                Debug.Log("GameOverCanvasをタグで見つけました");
            }
            else
            {
                // 名前で検索
                GameObject canvasObj = GameObject.Find(gameOverCanvasName);
                if (canvasObj != null)
                {
                    gameOverCanvas = canvasObj;
                    Debug.Log("GameOverCanvasを名前で見つけました");
                }
                else
                {
                    // すべてのキャンバスを検索して名前に「over」を含むものを探す
                    Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                    foreach (Canvas canvas in allCanvases)
                    {
                        if (canvas.name.ToLower().Contains("over") || 
                            canvas.name.ToLower().Contains("fail"))
                        {
                            gameOverCanvas = canvas.gameObject;
                            Debug.Log("GameOverCanvasを名前の一部で見つけました: " + canvas.name);
                            break;
                        }
                    }
                    
                    if (gameOverCanvas == null)
                    {
                        Debug.LogWarning("GameOverCanvasが見つかりません。インスペクターで手動設定してください。");
                    }
                }
            }
        }
        
        // ゲームクリアキャンバスの参照を探す
        if (gameClearCanvas == null)
        {
            // タグで検索
            GameObject clearCanvas = null;
            try
            {
                clearCanvas = GameObject.FindGameObjectWithTag("GameClearCanvas");
            }
            catch (UnityException)
            {
                Debug.LogWarning("GameClearCanvasタグが存在しません");
            }
            
            if (clearCanvas != null)
            {
                gameClearCanvas = clearCanvas;
                Debug.Log("GameClearCanvasをタグで見つけました");
            }
            else
            {
                // 名前で検索
                GameObject canvasObj = GameObject.Find(gameClearCanvasName);
                if (canvasObj != null)
                {
                    gameClearCanvas = canvasObj;
                    Debug.Log("GameClearCanvasを名前で見つけました");
                }
                else
                {
                    // すべてのキャンバスを検索して名前に「clear」を含むものを探す
                    Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                    foreach (Canvas canvas in allCanvases)
                    {
                        if (canvas.name.ToLower().Contains("clear") || 
                            canvas.name.ToLower().Contains("complete") || 
                            canvas.name.ToLower().Contains("success"))
                        {
                            gameClearCanvas = canvas.gameObject;
                            Debug.Log("GameClearCanvasを名前の一部で見つけました: " + canvas.name);
                            break;
                        }
                    }
                    
                    if (gameClearCanvas == null)
                    {
                        Debug.LogWarning("GameClearCanvasが見つかりません。インスペクターで手動設定してください。");
                    }
                }
            }
        }
        
        // キャンバスが見つかったら初期設定を行う
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(false); // 初期状態は非表示
            
            // CanvasGroupを追加
            if (gameOverCanvas.GetComponent<CanvasGroup>() == null)
            {
                gameOverCanvas.AddComponent<CanvasGroup>();
            }
        }
        
        if (gameClearCanvas != null)
        {
            gameClearCanvas.SetActive(false); // 初期状態は非表示
            
            // CanvasGroupを追加
            if (gameClearCanvas.GetComponent<CanvasGroup>() == null)
            {
                gameClearCanvas.AddComponent<CanvasGroup>();
            }
        }
    }
    
    // 段階に応じたメッセージを取得
    private string GetStageMessage(int stageNumber)
    {
        switch (stageNumber)
        {
            case 1: return stage1Message;
            case 2: return stage2Message;
            case 3: return stage3Message;
            case 4: return stage4Message;
            case 5: return stage5Message;
            default: return $"ステージ {stageNumber} に進みました";
        }
    }
    
    // メッセージ表示処理
    private void DisplayMessage(string message)
    {
        if (messageText == null)
        {
            // メッセージテキストがnullの場合は再取得を試みる
            SetupReferences();
            
            if (messageText == null)
            {
                Debug.LogWarning("MessageTextが見つかりません。インスペクターで手動設定してください。");
                return;
            }
        }
        
        // 現在実行中のコルーチンがあればキャンセル
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
        }
        
        // 新しいメッセージを表示
        currentMessageCoroutine = StartCoroutine(ShowMessageCoroutine(message));
    }
    
    // メッセージ表示コルーチン
    private IEnumerator ShowMessageCoroutine(string message)
    {
        // テキストを設定
        messageText.text = message;
        
        // フェードイン
        float elapsedTime = 0;
        Color textColor = messageText.color;
        textColor.a = 0;
        messageText.color = textColor;
        
        while (elapsedTime < fadeInTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeInTime);
            textColor.a = alpha;
            messageText.color = textColor;
            yield return null;
        }
        
        // 完全に表示された状態を維持
        textColor.a = 1;
        messageText.color = textColor;
        
        // メッセージは次のメッセージが表示されるまで表示したままにする
        while (true)
        {
            yield return null;
        }
    }
    
    // デバッグ用メソッド - キャンバスの強制表示
    public void ForceShowCanvas(GameObject canvas)
    {
        if (canvas == null) return;
        
        Debug.Log("キャンバスを強制表示します: " + canvas.name);
        
        // キャンバスコンポーネントを有効化
        Canvas canvasComponent = canvas.GetComponent<Canvas>();
        if (canvasComponent != null)
        {
            canvasComponent.enabled = true;
            canvasComponent.sortingOrder = 100; // 最前面
        }
        
        // CanvasGroupの設定
        CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = canvas.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        // 子オブジェクトをすべて有効化
        foreach (Transform child in canvas.transform)
        {
            child.gameObject.SetActive(true);
        }
        
        // キャンバス自体を有効化
        canvas.SetActive(true);
        
        // 最前面に表示
        if (canvas.transform.parent != null)
        {
            canvas.transform.SetAsLastSibling();
        }
    }
}