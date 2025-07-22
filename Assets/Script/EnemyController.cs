using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    [Header("ターゲット設定")]
    [Tooltip("プレイヤーのTransform")]
    public Transform player;
    
    [Tooltip("プレイヤーとの接触後に向かうオブジェクト")]
    public Transform afterContactTarget;
    
    [Header("検出設定")]
    [Tooltip("プレイヤーを検出する範囲")]
    public float detectionRange = 10f;
    
    [Tooltip("常にプレイヤーを追いかけるか")]
    public bool alwaysChase = false;
    
    [Header("移動設定")]
    [Tooltip("移動速度")]
    public float moveSpeed = 3.5f;
    
    [Tooltip("回転速度")]
    public float rotationSpeed = 5f;
    
    [Header("接触設定")]
    [Tooltip("接触判定のためのコライダー半径")]
    public float contactRadius = 1.0f;
    
    [Header("サウンド設定")]
    [Tooltip("足音用のオーディオクリップ")]
    public AudioClip[] footstepSounds;
    
    [Range(0, 1)]
    [Tooltip("足音の音量")]
    public float footstepVolume = 0.5f;
    
    // 内部変数
    private NavMeshAgent agent;
    private bool playerDetected = false;
    private bool hasContactedPlayer = false;
    private Animator animator;
    
    void Start()
    {
        // NavMeshAgentコンポーネントの取得
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
        }
        
        // Animatorがあれば取得
        animator = GetComponent<Animator>();
        
        // プレイヤーが指定されていない場合は自動検出
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        
        // 初期状態：停止しておく（または初期パトロールポイントを設定することも可）
        if (agent != null)
        {
            agent.isStopped = true;
        }
    }
    
    void Update()
    {
        if (player == null || agent == null) return;
        
        // プレイヤーとの接触をチェック
        CheckPlayerContact();
        
        // プレイヤーに接触済みの場合
        if (hasContactedPlayer)
        {
            // 接触後のターゲットに向かう
            if (afterContactTarget != null)
            {
                agent.isStopped = false;
                agent.SetDestination(afterContactTarget.position);
                
                // アニメーターがあれば走るアニメーションを再生
                if (animator != null)
                {
                    animator.SetBool("IsChasing", true);
                }
            }
        }
        // まだプレイヤーに接触していない場合
        else
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // 検出範囲内かどうか確認
            if (distanceToPlayer <= detectionRange || alwaysChase)
            {
                playerDetected = true;
                
                // プレイヤーを追いかける
                agent.isStopped = false;
                agent.SetDestination(player.position);
                
                // アニメーターがあれば走るアニメーションを再生
                if (animator != null)
                {
                    animator.SetBool("IsChasing", true);
                }
            }
            else if (playerDetected)
            {
                // プレイヤーを見失った
                playerDetected = false;
                agent.isStopped = true;
                
                // アニメーターがあればアイドル状態に戻す
                if (animator != null)
                {
                    animator.SetBool("IsChasing", false);
                }
            }
        }
    }
    
    // プレイヤーとの接触をチェック
    private void CheckPlayerContact()
    {
        if (hasContactedPlayer || player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // 接触判定の距離以内に入ったら接触と見なす
        if (distanceToPlayer <= contactRadius)
        {
            hasContactedPlayer = true;
            Debug.Log("プレイヤーと接触しました");
            
            // プレイヤーとの接触時にメッセージを表示（追加）
            if (MessageDisplay.Instance != null)
            {
                MessageDisplay.Instance.ShowMessage("caught");
            }
            else
            {
                Debug.LogWarning("MessageDisplayインスタンスが見つかりません");
            }
        }
    }
    
    // 物理的な接触判定（追加）
    private void OnCollisionEnter(Collision collision)
    {
        if (!hasContactedPlayer && collision.gameObject.CompareTag("Player"))
        {
            hasContactedPlayer = true;
            Debug.Log("衝突判定でプレイヤーと接触しました");
            
            // プレイヤーとの接触時にメッセージを表示
            if (MessageDisplay.Instance != null)
            {
                MessageDisplay.Instance.ShowMessage("caught");
            }
            else
            {
                Debug.LogWarning("MessageDisplayインスタンスが見つかりません");
            }
        }
    }
    
    // トリガー接触判定（追加）
    private void OnTriggerEnter(Collider other)
    {
        if (!hasContactedPlayer && other.CompareTag("Player"))
        {
            hasContactedPlayer = true;
            Debug.Log("トリガー判定でプレイヤーと接触しました");
            
            // プレイヤーとの接触時にメッセージを表示
            if (MessageDisplay.Instance != null)
            {
                MessageDisplay.Instance.ShowMessage("caught");
            }
            else
            {
                Debug.LogWarning("MessageDisplayインスタンスが見つかりません");
            }
        }
    }
    
    // アニメーションイベント用のメソッド
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (footstepSounds != null && footstepSounds.Length > 0)
            {
                var index = Random.Range(0, footstepSounds.Length);
                AudioSource.PlayClipAtPoint(footstepSounds[index], transform.position, footstepVolume);
            }
        }
    }
    
    // ギズモを描画（エディターでの視覚化用）
    void OnDrawGizmosSelected()
    {
        // 検出範囲を視覚化
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 接触範囲を視覚化
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, contactRadius);
    }
    
    // プレイヤーとの接触状態をリセット（外部から呼び出せるようにpublic）
    public void ResetContactState()
    {
        hasContactedPlayer = false;
        playerDetected = false;
        
        if (agent != null)
        {
            agent.isStopped = true;
        }
    }
}