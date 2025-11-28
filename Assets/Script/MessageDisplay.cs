using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ゲーム内のメッセージを画面下部に表示するためのコンポーネント
/// </summary>
public class MessageDisplay : MonoBehaviour
{
    [Header("UI設定")]
    [Tooltip("メッセージを表示するテキストコンポーネント")]
    public Text messageText;
    
    [Tooltip("メッセージの表示時間（秒）")]
    public float displayDuration = 3.0f;
    
    [Tooltip("メッセージのフェードイン時間（秒）")]
    public float fadeInTime = 0.5f;
    
    [Tooltip("メッセージのフェードアウト時間（秒）")]
    public float fadeOutTime = 0.5f;
    
    [Header("メッセージプリセット")]
    [Tooltip("ゲーム開始時のメッセージ")]
    public string startMessage = "目的地を目指そう！";
    
    [Tooltip("敵に触れられた時のメッセージ")]
    public string caughtByEnemyMessage = "データを盗まれた";
    
    [Header("ゲーム連携設定")]
    [Tooltip("敵に捕まった時にゲージを減少させるか")]
    public bool enableProgressDecrease = true;
    
    [Tooltip("ゲージ減少を通知するCryptoGameManager")]
    public CryptoGameManager gameManager;
    
    [Tooltip("敵に捕まった時にプレイヤー移動を一時停止するか")]
    public bool disableMovementOnCaught = true;
    
    [Tooltip("移動停止時間（秒）")]
    [Range(0.5f, 5.0f)]
    public float movementDisableDuration = 2.0f;
    
    // 追加メッセージ用の辞書
    private Dictionary<string, string> messageDict = new Dictionary<string, string>();
    
    // 現在のコルーチン（キャンセル用）
    private Coroutine currentDisplayCoroutine;
    
    // シングルトンインスタンス
    public static MessageDisplay Instance { get; private set; }
    
    private void Awake()
    {
        // シングルトンパターン
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // テキストコンポーネントの確認
        if (messageText == null)
        {
            Debug.LogError("MessageDisplay: テキストコンポーネントが設定されていません。");
        }
        else
        {
            // 初期状態は透明に
            Color textColor = messageText.color;
            textColor.a = 0;
            messageText.color = textColor;
        }
        
        // 基本メッセージを辞書に追加
        messageDict.Add("start", startMessage);
        messageDict.Add("caught", caughtByEnemyMessage);
        
        // CryptoGameManagerの自動検索
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<CryptoGameManager>();
            if (gameManager != null)
            {
                Debug.Log("[MessageDisplay] CryptoGameManagerを自動検出しました");
            }
        }
    }
    
    private void Start()
    {
        // ゲーム開始時のメッセージを表示
        ShowMessage("start");
    }
    
    /// <summary>
    /// 事前定義されたメッセージを表示
    /// </summary>
    /// <param name="messageKey">メッセージのキー（"start", "caught"など）</param>
    public void ShowMessage(string messageKey)
    {
        if (messageDict.TryGetValue(messageKey, out string message))
        {
            DisplayMessage(message);
            
            // 敵に捕まった場合の特別な処理
            if (messageKey == "caught")
            {
                HandleEnemyCaught();
            }
        }
        else
        {
            Debug.LogWarning($"MessageDisplay: キー '{messageKey}' に対応するメッセージが見つかりません。");
        }
    }
    
    /// <summary>
    /// 敵に捕まった時の処理
    /// </summary>
    private void HandleEnemyCaught()
    {
        Debug.Log("[MessageDisplay] 敵に捕まった時の処理を実行");
        
        // プレイヤー移動を一時的に無効化
        if (disableMovementOnCaught)
        {
            StartCoroutine(TemporarilyDisableMovement());
        }
        
        if (enableProgressDecrease && gameManager != null)
        {
            // 不正解処理を実行してゲージを減少させる
            gameManager.OnIncorrectAnswerSelected();
            Debug.Log("[MessageDisplay] ゲージ減少処理を実行しました");
        }
        else if (enableProgressDecrease)
        {
            Debug.LogWarning("[MessageDisplay] ゲージ減少が有効ですが、CryptoGameManagerが見つかりません");
            
            // フォールバック: ProgressTrackerに直接アクセス
            if (ProgressTracker.Instance != null)
            {
                // CryptoGameManagerを再検索してみる
                var foundGameManager = FindObjectOfType<CryptoGameManager>();
                if (foundGameManager != null)
                {
                    gameManager = foundGameManager; // 次回用にキャッシュ
                    
                    // 現在のゲームタイプを取得
                    var currentType = foundGameManager.CurrentCryptoType;
                    if (currentType.HasValue)
                    {
                        ProgressTracker.Instance.OnIncorrectAnswer(currentType.Value);
                        Debug.Log($"[MessageDisplay] ProgressTracker経由でゲージ減少処理を実行しました: {currentType.Value}");
                    }
                    else
                    {
                        // ゲームタイプが取得できない場合はデフォルト値を使用
                        ProgressTracker.Instance.OnIncorrectAnswer(CryptoGameManager.CryptoType.SymmetricKey);
                        Debug.Log("[MessageDisplay] ProgressTracker経由でゲージ減少処理を実行しました（デフォルト: SymmetricKey）");
                    }
                }
                else
                {
                    // CryptoGameManagerが見つからない場合はデフォルト値でゲージ減少
                    ProgressTracker.Instance.OnIncorrectAnswer(CryptoGameManager.CryptoType.SymmetricKey);
                    Debug.Log("[MessageDisplay] CryptoGameManager未検出、デフォルト値でゲージ減少処理を実行");
                }
            }
            else
            {
                Debug.LogError("[MessageDisplay] ProgressTrackerも見つかりません。ゲージ減少処理をスキップします。");
            }
        }
    }
    
    /// <summary>
    /// 一時的にプレイヤー移動を無効化
    /// </summary>
    private IEnumerator TemporarilyDisableMovement()
    {
        if (gameManager != null && gameManager.playerInput != null)
        {
            var playerInput = gameManager.playerInput;
            bool wasEnabled = playerInput.IsInputEnabled();
            
            if (wasEnabled)
            {
                playerInput.SetInputEnabled(false);
                Debug.Log($"[MessageDisplay] プレイヤー移動を{movementDisableDuration}秒間無効化");
                
                yield return new WaitForSeconds(movementDisableDuration);
                
                // ゲームが進行中の場合のみ入力を再有効化
                if (gameManager.IsGameActive)
                {
                    playerInput.SetInputEnabled(true);
                    Debug.Log("[MessageDisplay] プレイヤー移動を再有効化");
                }
                else
                {
                    Debug.Log("[MessageDisplay] ゲーム終了のため、プレイヤー移動は無効のまま");
                }
            }
        }
        else
        {
            Debug.LogWarning("[MessageDisplay] プレイヤー入力コンポーネントが見つかりません。移動無効化をスキップします。");
        }
    }
    
    /// <summary>
    /// カスタムメッセージを表示
    /// </summary>
    /// <param name="message">表示するメッセージ</param>
    public void DisplayCustomMessage(string message)
    {
        DisplayMessage(message);
    }
    
    /// <summary>
    /// 新しいメッセージを辞書に追加
    /// </summary>
    /// <param name="key">メッセージのキー</param>
    /// <param name="message">表示するメッセージ</param>
    public void AddMessage(string key, string message)
    {
        if (messageDict.ContainsKey(key))
        {
            messageDict[key] = message; // 既存のキーなら上書き
        }
        else
        {
            messageDict.Add(key, message); // 新しいキーなら追加
        }
    }
    
    // 実際にメッセージを表示する内部メソッド
    private void DisplayMessage(string message)
    {
        if (messageText == null) return;
        
        // 現在実行中のコルーチンがあればキャンセル
        if (currentDisplayCoroutine != null)
        {
            StopCoroutine(currentDisplayCoroutine);
        }
        
        // 新しいメッセージを表示
        currentDisplayCoroutine = StartCoroutine(ShowMessageCoroutine(message));
    }
    
    // メッセージ表示コルーチン
    private IEnumerator ShowMessageCoroutine(string message)
    {
        // テキストを設定
        messageText.text = message;
        
        // フェードイン
        float elapsedTime = 0;
        Color textColor = messageText.color;
        
        while (elapsedTime < fadeInTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeInTime);
            textColor.a = alpha;
            messageText.color = textColor;
            yield return null;
        }
        
        // 完全に表示された状態を維持（次のメッセージが来るまで表示したまま）
        textColor.a = 1;
        messageText.color = textColor;
        
        // フェードアウトはせず、次のメッセージ表示まで維持する
        // このコルーチンは新しいメッセージが表示される際に
        // DisplayMessage()でStopCoroutineされるため、
        // ここでは無限に待機する
        while (true)
        {
            yield return null;
        }
    }
}