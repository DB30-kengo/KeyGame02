using UnityEngine;
using UnityEngine.UI;

public class InteractionTrigger : MonoBehaviour
{
    [Header("UI設定")]
    [Tooltip("表示するUIキャンバス")]
    public GameObject uiCanvas;
    [Tooltip("キャンバスをフェードインさせる場合はチェック")]
    public bool useFadeEffect = true;
    [Tooltip("フェードインの速度")]
    public float fadeSpeed = 1.0f;

    [Header("サウンド設定")]
    [Tooltip("再生するサウンド")]
    public AudioClip interactionSound;
    [Range(0, 1)]
    [Tooltip("サウンドの音量")]
    public float soundVolume = 0.5f;

    [Header("インタラクション設定")]
    [Tooltip("インタラクションを検知するタグ（通常は'Player'）")]
    public string targetTag = "Player";
    [Tooltip("一度だけ表示する場合はチェック")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;
    private CanvasGroup canvasGroup;
    private AudioSource audioSource;

    private void Awake()
    {
        // キャンバスの初期設定
        if (uiCanvas != null)
        {
            // キャンバスが最初は非表示
            uiCanvas.SetActive(false);
            
            // CanvasGroupコンポーネントを取得（フェード効果用）
            canvasGroup = uiCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null && useFadeEffect)
            {
                canvasGroup = uiCanvas.AddComponent<CanvasGroup>();
            }
        }

        // AudioSourceコンポーネントを取得または追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && interactionSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤータグを持つオブジェクトと衝突したとき
        if (other.CompareTag(targetTag))
        {
            // 一度だけ表示する設定で、既に表示されている場合は何もしない
            if (triggerOnce && hasTriggered)
                return;

            // UIキャンバスを表示
            ShowCanvas();
            
            // サウンドを再生
            PlaySound();
            
            hasTriggered = true;
        }
    }

    private void ShowCanvas()
    {
        if (uiCanvas == null)
            return;

        uiCanvas.SetActive(true);

        // フェード効果を使用する場合
        if (useFadeEffect && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            StartCoroutine(FadeIn());
        }
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float alpha = 0f;
        
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            canvasGroup.alpha = alpha;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }

    private void PlaySound()
    {
        if (interactionSound != null && audioSource != null)
        {
            audioSource.clip = interactionSound;
            audioSource.volume = soundVolume;
            audioSource.Play();
        }
    }

    // UIを非表示にするメソッド（外部から呼び出し可能）
    public void HideCanvas()
    {
        if (uiCanvas == null)
            return;

        if (useFadeEffect && canvasGroup != null)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            uiCanvas.SetActive(false);
        }
    }

    private System.Collections.IEnumerator FadeOut()
    {
        float alpha = 1f;
        
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            canvasGroup.alpha = alpha;
            yield return null;
        }
        
        uiCanvas.SetActive(false);
    }
}