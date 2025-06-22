using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro; // TextMeshProサポート用

public class TypewriterEffect : MonoBehaviour
{
    [Header("タイプライター設定")]
    [Tooltip("表示する完全なテキスト")]
    [TextArea(3, 10)]
    public string fullText;
    
    [Tooltip("文字の表示間隔（秒）")]
    [Range(0.01f, 0.5f)]
    public float typingSpeed = 0.05f;
    
    [Tooltip("自動的に表示を開始するか")]
    public bool startOnEnable = true;
    
    [Header("サウンド設定")]
    [Tooltip("タイピング音を再生するか")]
    public bool playSound = false;
    
    [Tooltip("タイピング効果音")]
    public AudioClip typingSound;
    
    [Tooltip("何文字ごとに音を鳴らすか")]
    [Range(1, 5)]
    public int soundFrequency = 2;
    
    [Range(0, 1)]
    [Tooltip("効果音の音量")]
    public float soundVolume = 0.5f;

    // テキストコンポーネント（TextまたはTextMeshPro）
    private Text uiText;
    private TextMeshProUGUI tmpText;
    private AudioSource audioSource;
    
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        // テキストコンポーネントを取得
        uiText = GetComponent<Text>();
        tmpText = GetComponent<TextMeshProUGUI>();
        
        if (uiText == null && tmpText == null)
        {
            Debug.LogError("TypewriterEffectにはTextまたはTextMeshProUGUIコンポーネントが必要です");
        }
        
        // AudioSourceを設定
        if (playSound && typingSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
    }
    
    private void OnEnable()
    {
        if (startOnEnable)
        {
            StartTyping();
        }
        else
        {
            // 初期状態では空のテキストを表示
            SetText("");
        }
    }

    /// <summary>
    /// タイピング効果を開始します
    /// </summary>
    public void StartTyping()
    {
        // 既に実行中のコルーチンがあれば停止
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        typingCoroutine = StartCoroutine(TypeText());
    }
    
    /// <summary>
    /// タイピングを即座に完了します
    /// </summary>
    public void CompleteTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        
        SetText(fullText);
        isTyping = false;
    }
    
    /// <summary>
    /// 表示するテキストを設定します
    /// </summary>
    public void SetFullText(string text)
    {
        fullText = text;
        
        // 既に表示中なら再スタート
        if (isTyping)
        {
            StartTyping();
        }
    }
    
    /// <summary>
    /// テキストを徐々に表示するコルーチン
    /// </summary>
    private IEnumerator TypeText()
    {
        isTyping = true;
        string currentText = "";
        int charCount = 0;
        
        // テキストを一文字ずつ表示
        for (int i = 0; i < fullText.Length; i++)
        {
            currentText += fullText[i];
            SetText(currentText);
            charCount++;
            
            // 効果音の再生
            if (playSound && audioSource != null && typingSound != null && charCount % soundFrequency == 0)
            {
                audioSource.PlayOneShot(typingSound, soundVolume);
            }
            
            yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;
        typingCoroutine = null;
    }
    
    /// <summary>
    /// テキストコンポーネントにテキストを設定
    /// </summary>
    private void SetText(string text)
    {
        if (uiText != null)
        {
            uiText.text = text;
        }
        else if (tmpText != null)
        {
            tmpText.text = text;
        }
    }
    
    /// <summary>
    /// 現在タイピング中かどうかを取得
    /// </summary>
    public bool IsTyping()
    {
        return isTyping;
    }
}