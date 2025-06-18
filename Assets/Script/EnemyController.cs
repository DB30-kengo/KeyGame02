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
    private float nextPlayerSearchTime = 0f;
    private const float PLAYER_SEARCH_INTERVAL = 0.5f; // 0.5秒ごとにプレイヤー再検索
    
    void Awake()
    {
        // NavMeshAgentコンポーネントの取得
        agent = GetComponent<NavMeshAgent>();
        
        // Animatorがあれば取得
        animator = GetComponent<Animator>();
    }
    
    void Start()
    {
        InitializeAgent();
        FindPlayer();
    }
    
    void OnEnable()
    {
        // コンポーネントが有効になったときに初期化
        InitializeAgent();
        FindPlayer();
    }
    
    private void InitializeAgent()
    {
        if (agent != null)
        {
            agent.speed = moveSpeed;
            
            // NavMeshが生成されているか確認
            if (!agent.isOnNavMesh)
            {
                Debug.LogWarning(gameObject.name + ": NavMeshAgent is not on NavMesh! Attempting to fix...");
                
                // NavMeshAgentの再初期化を試みる
                agent.enabled = false;
                transform.position = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
                agent.enabled = true;
            }
        }
    }
    
    private void FindPlayer()
    {
        // プレイヤーが指定されていない場合は自動検出
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("Player found: " + player.name);
            }
            else
            {
                Debug.LogWarning("Player not found! Make sure it has the 'Player' tag.");
            }
        }
    }
    
    void Update()
    {
        // 定期的にプレイヤーを再検索
        if (player == null && Time.time > nextPlayerSearchTime)
        {
            FindPlayer();
            nextPlayerSearchTime = Time.time + PLAYER_SEARCH_INTERVAL;
        }
        
        // 必要なコンポーネントがない場合は処理しない
        if (player == null || agent == null)
            return;
            
        // NavMeshAgentが有効でないか、NavMesh上にない場合は再初期化を試みる
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            Debug.LogWarning(gameObject.name + ": NavMeshAgent issue detected. Attempting to fix...");
            InitializeAgent();
            return;
        }
            
        // プレイヤーとの距離を計算
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // 検出範囲内かどうか確認
        if (distanceToPlayer <= detectionRange || alwaysChase)
        {
            playerDetected = true;
            
            try
            {
                // プレイヤーを追いかける
                agent.SetDestination(player.position);
                
                // アニメーターがあれば走るアニメーションを再生
                if (animator != null)
                {
                    animator.SetBool("IsChasing", true);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error setting destination: " + e.Message);
            }
        }
        else
        {
            if (playerDetected)
            {
                // プレイヤーを見失った
                if (agent.isOnNavMesh)
                {
                    agent.ResetPath();
                }
                playerDetected = false;
                
                // アニメーターがあればアイドル状態に戻す
                if (animator != null)
                {
                    animator.SetBool("IsChasing", false);
                }
            }
        }
    }
    
    // OnTriggerEnterでプレイヤーとの衝突を検出（必要に応じて）
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ここにプレイヤーとの衝突時の処理を追加
            Debug.Log("Player collided with enemy!");
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