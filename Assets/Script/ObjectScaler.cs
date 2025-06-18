using UnityEngine;
using System.Collections;

public class ObjectScaler : MonoBehaviour
{
    [Header("サイズ変更設定")]
    [Tooltip("触れたときの拡大倍率")]
    public Vector3 scaleMultiplier = new Vector3(1.5f, 1.5f, 1.5f);
    [Tooltip("サイズ変更にかかる時間（秒）")]
    public float scaleTime = 0.5f;
    [Tooltip("拡大状態を維持する時間（秒）")]
    public float displayDuration = 3.0f;
    
    [Header("インタラクション設定")]
    [Tooltip("インタラクションを検知するタグ（通常は'Player'）")]
    public string targetTag = "Player";
    [Tooltip("効果音")]
    public AudioClip scaleSound;
    [Range(0, 1)]
    [Tooltip("効果音の音量")]
    public float soundVolume = 0.5f;

    private Vector3 originalScale;
    private bool isScaling = false;
    private AudioSource audioSource;

    private void Awake()
    {
        // 元のサイズを保存
        originalScale = transform.localScale;

        // AudioSourceコンポーネントを取得または追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && scaleSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    
    private void Start()
    {
        // シーン開始時に必ずオブジェクトを表示状態にする
        gameObject.SetActive(true);
        // 拡大状態をリセット
        transform.localScale = originalScale;
        // 処理中フラグをリセット
        isScaling = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーに触れた場合かつ、まだ処理中でない場合
        if (other.CompareTag(targetTag) && !isScaling)
        {
            // 拡大処理を開始
            StartCoroutine(ScaleAndHide());
            
            // 効果音を再生
            PlaySound();
        }
    }

    private System.Collections.IEnumerator ScaleAndHide()
    {
        isScaling = true;
        Vector3 targetScale = Vector3.Scale(originalScale, scaleMultiplier);
        float elapsedTime = 0f;

        // オブジェクトを徐々に拡大
        while (elapsedTime < scaleTime)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / scaleTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 確実に目標サイズに設定
        transform.localScale = targetScale;

        // 指定時間待機
        yield return new WaitForSeconds(displayDuration);

        // 非表示にする前に元のサイズに戻す
        elapsedTime = 0f;
        while (elapsedTime < scaleTime)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime / scaleTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // オブジェクトを非表示にする
        gameObject.SetActive(false);
        isScaling = false;
    }

    private void PlaySound()
    {
        if (scaleSound != null && audioSource != null)
        {
            audioSource.clip = scaleSound;
            audioSource.volume = soundVolume;
            audioSource.Play();
        }
    }

    // 外部から呼び出し可能なリセットメソッド
    public void ResetObject()
    {
        transform.localScale = originalScale;
        gameObject.SetActive(true);
        isScaling = false;
        StopAllCoroutines();
    }
}