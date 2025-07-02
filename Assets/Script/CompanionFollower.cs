using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CompanionFollower : MonoBehaviour
{
    [Header("追従設定")]
    [Tooltip("追従するプレイヤーの参照")]
    public Transform playerTransform;

    [Tooltip("プレイヤーとの理想的な距離")]
    public float followDistance = 3.0f;

    [Tooltip("プレイヤーからの位置調整（0:真後ろ, 90:右側, -90:左側）")]
    [Range(-180f, 180f)]
    public float followAngleOffset = 0f;

    [Tooltip("プレイヤーと最小限保つ距離")]
    public float minDistanceToPlayer = 1.5f;

    [Tooltip("移動速度")]
    public float moveSpeed = 3.5f;

    [Tooltip("回転速度")]
    public float rotationSpeed = 5.0f;

    [Tooltip("再経路計算の間隔（秒）")]
    public float pathUpdateInterval = 0.5f;

    [Header("アニメーション設定")]
    [Tooltip("アニメーターの使用")]
    public bool useAnimator = true;

    [Tooltip("移動速度のアニメーターパラメータ名")]
    public string speedParameterName = "Speed";

    [Tooltip("アイドル状態の閾値")]
    public float idleSpeedThreshold = 0.1f;

    // コンポーネント参照
    private NavMeshAgent agent;
    private Animator animator;
    private float nextPathUpdate;
    private Vector3 targetPosition;
    private bool isPathCalculating = false;

    private void Awake()
    {
        // コンポーネントの取得
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }

        if (useAnimator)
        {
            animator = GetComponent<Animator>();
        }

        // NavMeshAgentの初期設定
        agent.speed = moveSpeed;
        agent.angularSpeed = rotationSpeed * 100;
        agent.stoppingDistance = minDistanceToPlayer;
        agent.autoBraking = true;
    }

    private void Start()
    {
        // プレイヤーが指定されていない場合は自動検出
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("CompanionFollower: プレイヤーが見つかりません。Playerタグのついたオブジェクトがシーンにあるか確認してください。");
                enabled = false;
                return;
            }
        }

        // 初期位置を設定
        UpdateTargetPosition();
        agent.SetDestination(targetPosition);
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // 一定間隔で経路を更新
        if (Time.time >= nextPathUpdate && !isPathCalculating)
        {
            nextPathUpdate = Time.time + pathUpdateInterval;
            StartCoroutine(UpdatePath());
        }

        // アニメーターの更新
        if (useAnimator && animator != null)
        {
            float currentSpeed = agent.velocity.magnitude;
            animator.SetFloat(speedParameterName, currentSpeed);
        }
    }

    // 経路更新のコルーチン
    private IEnumerator UpdatePath()
    {
        isPathCalculating = true;

        // 目標位置を更新
        UpdateTargetPosition();

        // 現在地と目標位置の距離が近すぎる場合は更新しない
        if (Vector3.Distance(transform.position, targetPosition) > minDistanceToPlayer * 0.5f)
        {
            agent.SetDestination(targetPosition);
        }

        // プレイヤーとの距離が近すぎる場合は少し離れる
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer < minDistanceToPlayer)
        {
            // プレイヤーから離れる方向に移動
            Vector3 directionFromPlayer = (transform.position - playerTransform.position).normalized;
            Vector3 backOffPosition = transform.position + directionFromPlayer * minDistanceToPlayer;
            agent.SetDestination(backOffPosition);
        }

        yield return null;
        isPathCalculating = false;
    }

    // 目標位置を計算
    private void UpdateTargetPosition()
    {
        // プレイヤーの後ろ（＋オフセット角度）に位置する点を計算
        float angleRad = (playerTransform.eulerAngles.y + followAngleOffset) * Mathf.Deg2Rad;
        float x = Mathf.Sin(angleRad) * followDistance;
        float z = Mathf.Cos(angleRad) * followDistance;

        targetPosition = playerTransform.position - new Vector3(x, 0, z);

        // 目標位置がNavMesh上にあるか確認して調整
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 5.0f, NavMesh.AllAreas))
        {
            targetPosition = hit.position;
        }
    }

    // プレイヤーが遠すぎる場合に瞬間移動する（オプション機能）
    public void TeleportToPlayer()
    {
        if (playerTransform == null) return;

        UpdateTargetPosition();
        agent.Warp(targetPosition);
    }

    // デバッグ表示
    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;

        // 目標位置の表示
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(targetPosition, 0.3f);

        // 理想的な追従距離の円を表示
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Vector3 playerPos = playerTransform.position;
        playerPos.y = transform.position.y;
        Gizmos.DrawWireSphere(playerPos, followDistance);
    }
}