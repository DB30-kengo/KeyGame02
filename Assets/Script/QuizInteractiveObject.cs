using UnityEngine;

/// <summary>
/// クイズで選択できるインタラクティブなオブジェクト
/// </summary>
public class QuizInteractiveObject : MonoBehaviour
{
    [Tooltip("このオブジェクトのID（QuizManagerで設定したIDと対応）")]
    public int objectId;
    
    [Tooltip("選択時のエフェクト（パーティクルやライトなど）")]
    public GameObject selectionEffect;
    
    [Tooltip("プレイヤータグ（通常は'Player'）")]
    public string playerTag = "Player";
    
    [Tooltip("トリガーコライダーを使用するか")]
    public bool useTriggerCollider = true;
    
    [Tooltip("一度だけ選択可能にする")]
    public bool selectOnce = false;
    
    [Tooltip("選択時のハイライト色")]
    public Color highlightColor = new Color(1f, 0.8f, 0.2f);
    
    // 内部変数
    private bool hasBeenSelected = false;
    private Renderer objectRenderer;
    private Color originalColor;
    
    private void Start()
    {
        // レンダラーコンポーネントを取得
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
        
        // 選択エフェクトを初期状態で非表示に
        if (selectionEffect != null)
        {
            selectionEffect.SetActive(false);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (useTriggerCollider) return;
        
        if (collision.gameObject.CompareTag(playerTag))
        {
            HandleSelection();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!useTriggerCollider) return;
        
        if (other.CompareTag(playerTag))
        {
            HandleSelection();
        }
    }
    
    /// <summary>
    /// オブジェクト選択時の処理
    /// </summary>
    private void HandleSelection()
    {
        // 既に選択済みで、一度だけ選択可能な設定なら何もしない
        if (selectOnce && hasBeenSelected) return;
        
        hasBeenSelected = true;
        
        // QuizManagerに選択を通知
        if (QuizManager.Instance != null)
        {
            QuizManager.Instance.RecordPlayerSelection(objectId);
        }
        else
        {
            Debug.LogWarning("QuizManagerが見つかりません");
        }
        
        // 選択エフェクトを表示
        if (selectionEffect != null)
        {
            selectionEffect.SetActive(true);
            
            // 一定時間後にエフェクトを非表示にする
            StartCoroutine(DisableEffectAfterDelay(1.0f));
        }
        
        // オブジェクトの色を変更
        if (objectRenderer != null)
        {
            objectRenderer.material.color = highlightColor;
            
            // 一定時間後に元の色に戻す
            StartCoroutine(ResetColorAfterDelay(0.5f));
        }
    }
    
    /// <summary>
    /// 一定時間後にエフェクトを非表示にする
    /// </summary>
    private System.Collections.IEnumerator DisableEffectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (selectionEffect != null)
        {
            selectionEffect.SetActive(false);
        }
    }
    
    /// <summary>
    /// 一定時間後に色を元に戻す
    /// </summary>
    private System.Collections.IEnumerator ResetColorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (objectRenderer != null)
        {
            objectRenderer.material.color = originalColor;
        }
    }
    
    /// <summary>
    /// 選択状態をリセットする（新しい問題が始まるときなど）
    /// </summary>
    public void ResetSelection()
    {
        hasBeenSelected = false;
        
        if (objectRenderer != null)
        {
            objectRenderer.material.color = originalColor;
        }
        
        if (selectionEffect != null)
        {
            selectionEffect.SetActive(false);
        }
    }
}
