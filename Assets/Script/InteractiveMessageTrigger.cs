using UnityEngine;
using System.Collections;

/// <summary>
/// オブジェクトとの接触でメッセージを表示するトリガーコンポーネント
/// </summary>
public class InteractiveMessageTrigger : MonoBehaviour
{
    [Header("インタラクション設定")]
    [Tooltip("接触を検出するタグ（通常は'Player'）")]
    public string targetTag = "Player";
    
    [Tooltip("トリガーが一度だけ発動するか")]
    public bool triggerOnce = true;
    
    [Tooltip("接触判定にトリガーコライダーを使用するか")]
    public bool useTriggerCollider = true;
    
    [Header("メッセージ設定")]
    [Tooltip("表示するメッセージ")]
    [TextArea(2, 5)]
    public string messageToDisplay = "オブジェクトに触れました";
    
    [Tooltip("メッセージキー（追加設定がない場合は空でOK）")]
    public string messageKey = "";
    
    [Header("追加効果")]
    [Tooltip("接触時にオブジェクトを消すか")]
    public bool destroyOnContact = false;
    
    [Tooltip("消える前のエフェクト表示時間（秒）")]
    public float destroyDelay = 0.5f;
    
    [Tooltip("接触時に再生するサウンド")]
    public AudioClip contactSound;
    
    // 内部変数
    private bool hasTriggered = false;
    private AudioSource audioSource;
    
    private void Start()
    {
        // AudioSourceコンポーネントを取得または追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && contactSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // メッセージキーが指定されていない場合は、ゲームオブジェクト名をキーとして使用
        if (string.IsNullOrEmpty(messageKey))
        {
            messageKey = gameObject.name;
        }
        
        // MessageDisplayに自分のメッセージを登録
        if (MessageDisplay.Instance != null)
        {
            MessageDisplay.Instance.AddMessage(messageKey, messageToDisplay);
        }
    }
    
    // 物理的な接触（トリガーでない場合）
    private void OnCollisionEnter(Collision collision)
    {
        if (useTriggerCollider) return; // トリガーモードの場合はスキップ
        
        if (collision.gameObject.CompareTag(targetTag))
        {
            HandleContact();
        }
    }
    
    // トリガー接触
    private void OnTriggerEnter(Collider other)
    {
        if (!useTriggerCollider) return; // トリガーモードでない場合はスキップ
        
        if (other.CompareTag(targetTag))
        {
            HandleContact();
        }
    }
    
    // 接触時の処理
    private void HandleContact()
    {
        // 一度だけ発動するモードで、すでに発動済みなら何もしない
        if (triggerOnce && hasTriggered)
            return;
            
        hasTriggered = true;
        
        // デバッグ情報を追加
        Debug.Log($"オブジェクト '{gameObject.name}' との接触を検出: メッセージ '{messageToDisplay}' を表示します");
        
        // メッセージを表示
        if (MessageDisplay.Instance != null)
        {
            if (!string.IsNullOrEmpty(messageKey))
            {
                // キーでメッセージを表示
                MessageDisplay.Instance.ShowMessage(messageKey);
                Debug.Log($"MessageKeyを使用: '{messageKey}'");
            }
            else
            {
                // 直接メッセージを表示
                MessageDisplay.Instance.DisplayCustomMessage(messageToDisplay);
                Debug.Log("直接メッセージを表示");
            }
        }
        else
        {
            Debug.LogError("MessageDisplay.Instanceがnullです。MessageDisplayコンポーネントがシーンに存在するか確認してください。");
        }
        
        // サウンドを再生
        PlayContactSound();
        
        // 必要に応じてオブジェクトを消す
        if (destroyOnContact)
        {
            StartCoroutine(DestroyAfterDelay());
        }
    }
    
    // サウンド再生
    private void PlayContactSound()
    {
        if (contactSound != null && audioSource != null)
        {
            audioSource.clip = contactSound;
            audioSource.Play();
        }
    }
    
    // 遅延破壊用コルーチン
    private IEnumerator DestroyAfterDelay()
    {
        // コライダーを無効化して二重トリガーを防止
        Collider[] colliders = GetComponents<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
        
        // レンダラーがあれば、フェードアウトなどのエフェクトを追加できます
        // ここでは簡単に待機のみ
        yield return new WaitForSeconds(destroyDelay);
        
        Destroy(gameObject);
    }
    
    // トリガー範囲のギズモ表示
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        
        // コライダーがある場合はそのサイズで描画
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            if (col is BoxCollider)
            {
                BoxCollider boxCol = col as BoxCollider;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCol.center, boxCol.size);
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphereCol = col as SphereCollider;
                Gizmos.DrawSphere(transform.position + sphereCol.center, sphereCol.radius);
            }
            else if (col is CapsuleCollider)
            {
                // 簡易表示
                Gizmos.DrawSphere(transform.position, 1f);
            }
        }
        else
        {
            // コライダーがない場合は基本サイズで表示
            Gizmos.DrawSphere(transform.position, 1f);
        }
    }
}