using UnityEngine;
using System.Collections;

public class ObjectBouncer : MonoBehaviour
{
    [Header("跳ね返り設定")]
    [Tooltip("跳ね返る力の強さ")]
    public float bounceForce = 5f;
    
    [Tooltip("跳ね返る方向（プレイヤーから離れる方向）の影響度")]
    [Range(0f, 1f)]
    public float directionInfluence = 0.7f;
    
    [Tooltip("上方向への跳ね返り力")]
    public float upwardForce = 2f;
    
    [Header("検出設定")]
    [Tooltip("プレイヤーのタグ")]
    public string playerTag = "Player";
    
    [Tooltip("衝突時の効果音")]
    public AudioClip bounceSound;
    
    [Tooltip("効果音の音量")]
    [Range(0f, 1f)]
    public float soundVolume = 0.5f;
    
    [Header("NavMeshAgent設定")]
    [Tooltip("跳ね返り後、NavMeshAgentを再有効化するまでの時間")]
    public float reactivateAgentTime = 1.5f;
    
    // コンポーネントへの参照
    private Rigidbody rb;
    private AudioSource audioSource;
    
    // 最後の衝突時間（連続衝突防止用）
    private float lastBounceTime = 0f;
    private float bounceCooldown = 0.2f;
    
    private void Awake()
    {
        // Rigidbodyの取得または追加
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.Log("Rigidbodyコンポーネントが自動的に追加されました");
        }
        
        // NavMeshAgentがある場合の特別な設定
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            // NavMeshAgentがある場合は物理挙動の設定を調整
            rb.isKinematic = true; // 通常時は物理挙動を無効化
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        
        // オブジェクトが回転しないように設定（必要に応じて）
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        // AudioSourceの取得または追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && bounceSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        BounceFromObject(collision.gameObject);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        BounceFromObject(other.gameObject);
    }
    
    private void BounceFromObject(GameObject otherObject)
    {
        // プレイヤーとの衝突を検出
        if (otherObject.CompareTag(playerTag))
        {
            // クールダウン確認（連続衝突防止）
            if (Time.time < lastBounceTime + bounceCooldown)
                return;
                
            lastBounceTime = Time.time;
            
            // プレイヤーからの方向を計算（跳ね返る方向）
            Vector3 bounceDirection = transform.position - otherObject.transform.position;
            bounceDirection.y = 0; // 水平方向のみを考慮
            bounceDirection.Normalize();
            
            // 最終的な跳ね返り方向を計算（水平方向と上方向の組み合わせ）
            Vector3 finalBounceDirection = (bounceDirection * directionInfluence) + (Vector3.up * upwardForce);
            
            // NavMeshAgentがある場合は一時的に無効化
            UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            bool hadAgent = false;
            
            if (agent != null && agent.enabled)
            {
                hadAgent = true;
                agent.enabled = false;
            }
            
            // 一時的にkinematicを無効化して物理挙動を有効に
            bool wasKinematic = rb.isKinematic;
            rb.isKinematic = false;
            
            // 力を加える
            rb.linearVelocity = Vector3.zero; // 現在の速度をリセット
            rb.AddForce(finalBounceDirection * bounceForce, ForceMode.Impulse);
            
            // 効果音を再生
            PlayBounceSound();
            
            // NavMeshAgentとkinematic状態を一定時間後に再設定
            StartCoroutine(ResetPhysicsState(agent, hadAgent, wasKinematic));
        }
    }
    
    private IEnumerator ResetPhysicsState(UnityEngine.AI.NavMeshAgent agent, bool hadAgent, bool wasKinematic)
    {
        // 一定時間待機（跳ね返りアニメーションが終わるまで）
        yield return new WaitForSeconds(reactivateAgentTime);
        
        // kinematic状態を元に戻す
        rb.isKinematic = wasKinematic;
        
        // NavMeshAgentが存在していれば再有効化
        if (hadAgent && agent != null)
        {
            // 現在の位置を保存
            Vector3 currentPos = transform.position;
            
            // NavMeshAgentを再有効化
            agent.enabled = true;
            
            // NavMeshに適切に配置されるように位置を調整
            agent.Warp(currentPos);
            
            // EnemyControllerがあれば通知
            EnemyController enemyController = GetComponent<EnemyController>();
            if (enemyController != null)
            {
                // 必要に応じてEnemyControllerの状態をリセット
                SendMessage("OnBounceComplete", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
    
    private void PlayBounceSound()
    {
        if (bounceSound != null && audioSource != null)
        {
            audioSource.clip = bounceSound;
            audioSource.volume = soundVolume;
            audioSource.Play();
        }
    }
}