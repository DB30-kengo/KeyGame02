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
        }
        else
        {
            Debug.LogWarning($"MessageDisplay: キー '{messageKey}' に対応するメッセージが見つかりません。");
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