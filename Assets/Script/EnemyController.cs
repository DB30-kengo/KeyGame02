using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public Transform player;          // プレイヤーのTransform
    public float detectionRange = 10f; // 検出範囲
    public float moveSpeed = 3.5f;     // 移動速度
    public float rotationSpeed = 5f;   // 回転速度
    public bool alwaysChase = false;   // 常に追いかけるか
    
    // 足音用のオーディオクリップ
    public AudioClip[] footstepSounds;
    [Range(0, 1)] public float footstepVolume = 0.5f;
    
    private NavMeshAgent agent;
    private bool playerDetected = false;
    private Animator animator;
    
    void Start()
    {
        // NavMeshAgentコンポーネントの取得
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        
        // Animatorがあれば取得
        animator = GetComponent<Animator>();
        
        // プレイヤーが指定されていない場合は自動検出
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }
    
    void Update()
    {
        if (player == null)
            return;
            
        // プレイヤーとの距離を計算
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // 検出範囲内かどうか確認
        if (distanceToPlayer <= detectionRange || alwaysChase)
        {
            playerDetected = true;
            
            // プレイヤーを追いかける
            agent.SetDestination(player.position);
            
            // アニメーターがあれば走るアニメーションを再生
            if (animator != null)
            {
                animator.SetBool("IsChasing", true);
            }
        }
        else
        {
            if (playerDetected)
            {
                // プレイヤーを見失った
                agent.ResetPath();
                playerDetected = false;
                
                // アニメーターがあればアイドル状態に戻す
                if (animator != null)
                {
                    animator.SetBool("IsChasing", false);
                }
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
    }
}