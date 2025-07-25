using UnityEngine;
using System.Collections;

/// <summary>
/// 順序付きインタラクションを行うオブジェクト
/// </summary>
public class SequentialInteractiveObject : MonoBehaviour
{
    [Header("インタラクション設定")]
    [Tooltip("このオブジェクトの順番（1から始まる）")]
    public int stageNumber = 1;
    
    [Tooltip("接触を検出するタグ（通常は'Player'）")]
    public string targetTag = "Player";
    
    [Tooltip("一度だけ発動するか")]
    public bool triggerOnce = true;
    
    [Tooltip("接触判定にトリガーコライダーを使用するか")]
    public bool useTriggerCollider = true;
    
    [Header("効果設定")]
    [Tooltip("正しい順番で接触した時にオブジェクトを消すか")]
    public bool destroyOnCorrectSequence = false;
    
    [Tooltip("接触時に再生するサウンド")]
    public AudioClip contactSound;
    
    [Tooltip("消える前のエフェクト表示時間（秒）")]
    public float destroyDelay = 0.5f;
    
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
    }
    
    // シーン再読み込み時などに状態をリセット
    private void OnEnable()
    {
        hasTriggered = false;
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
        Debug.Log($"オブジェクト '{gameObject.name}' との接触を検出: 順番 {stageNumber}");
        
        // マネージャーが存在するか確認
        if (SequentialObjectsManager.Instance == null)
        {
            Debug.LogError("SequentialObjectsManagerが見つかりません");
            return;
        }
        
        // 正しい順番かどうかをマネージャーに確認
        bool isCorrectSequence = SequentialObjectsManager.Instance.InteractWithObject(stageNumber);
        
        // サウンドを再生
        PlayContactSound();
        
        if (isCorrectSequence)
        {
            // 正しい順番だった場合の処理
            if (destroyOnCorrectSequence)
            {
                StartCoroutine(DestroyAfterDelay());
            }
        }
        else
        {
            // 間違った順番だった場合の処理（1→3または0→3の場合のみゲームオーバーになる）
            SequentialObjectsManager.Instance.TriggerGameOver();
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
        
        yield return new WaitForSeconds(destroyDelay);
        
        Destroy(gameObject);
    }
    
    // ギズモ表示
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.5f, 0, 0.5f);
        
        // オブジェクトの順番を表示
        Vector3 labelPos = transform.position + Vector3.up * 0.5f;
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(labelPos, $"順番: {stageNumber}");
        #endif
        
        // コライダーの可視化
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
        }
    }
}