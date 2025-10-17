using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーがオブジェクトに触れた時に、そのオブジェクトのポジションを変更するスクリプト
/// </summary>
public class ObjectPositionChanger : MonoBehaviour
{
    [Header("ポジション変更設定")]
    [Tooltip("変更後のポジション（ワールド座標）")]
    public Vector3 targetPosition = new Vector3(0, 0, 0);
    
    [Tooltip("相対的な移動かどうか（現在位置からの相対移動）")]
    public bool useRelativePosition = false;
    
    [Tooltip("ポジション変更の速度（0で瞬間移動）")]
    public float moveSpeed = 0f;
    
    [Header("対象設定")]
    [Tooltip("プレイヤーのタグ（空欄の場合は全てのオブジェクトが対象）")]
    public string playerTag = "Player";
    
    [Tooltip("変更できるオブジェクトのタグ（空欄の場合は全てのオブジェクトが対象）")]
    public string targetObjectTag = "";
    
    [Header("実行設定")]
    [Tooltip("一度だけ実行するかどうか")]
    public bool executeOnce = true;
    
    [Tooltip("触れてから実行までの遅延時間（秒）")]
    public float executionDelay = 0f;
    
    [Header("フィードバック設定")]
    [Tooltip("実行時の効果音")]
    public AudioClip executionSound;
    
    [Tooltip("効果音の音量")]
    [Range(0f, 1f)]
    public float soundVolume = 1f;
    
    [Tooltip("実行時のパーティクル効果")]
    public ParticleSystem executionParticles;
    
    // 内部変数
    private bool hasExecuted = false;
    private AudioSource audioSource;
    private Dictionary<GameObject, Coroutine> movingObjects = new Dictionary<GameObject, Coroutine>();
    
    private void Start()
    {
        // AudioSourceの設定
        if (executionSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.clip = executionSound;
            audioSource.volume = soundVolume;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }
    
    /// <summary>
    /// 衝突処理を統一的に処理する
    /// </summary>
    private void HandleCollision(GameObject touchingObject)
    {
        // 一度だけ実行する設定で既に実行済みの場合は無視
        if (executeOnce && hasExecuted)
        {
            return;
        }
        
        // プレイヤータグが設定されており、触れたオブジェクトがプレイヤーでない場合は無視
        if (!string.IsNullOrEmpty(playerTag) && !touchingObject.CompareTag(playerTag))
        {
            return;
        }
        
        // 対象オブジェクトタグが設定されており、このオブジェクトが対象でない場合は無視
        if (!string.IsNullOrEmpty(targetObjectTag) && !gameObject.CompareTag(targetObjectTag))
        {
            return;
        }
        
        // 遅延実行
        if (executionDelay > 0f)
        {
            StartCoroutine(DelayedExecution(touchingObject));
        }
        else
        {
            ExecutePositionChange(touchingObject);
        }
    }
    
    /// <summary>
    /// 遅延実行のコルーチン
    /// </summary>
    private IEnumerator DelayedExecution(GameObject touchingObject)
    {
        yield return new WaitForSeconds(executionDelay);
        ExecutePositionChange(touchingObject);
    }
    
    /// <summary>
    /// ポジション変更を実行する
    /// </summary>
    private void ExecutePositionChange(GameObject touchingObject)
    {
        // 実行済みフラグを設定
        if (executeOnce)
        {
            hasExecuted = true;
        }
        
        // 移動先の計算
        Vector3 finalPosition;
        if (useRelativePosition)
        {
            finalPosition = transform.position + targetPosition;
        }
        else
        {
            finalPosition = targetPosition;
        }
        
        // 移動実行
        if (moveSpeed <= 0f)
        {
            // 瞬間移動
            transform.position = finalPosition;
            Debug.Log($"オブジェクト {gameObject.name} が瞬間移動しました: {finalPosition}");
        }
        else
        {
            // 滑らかな移動
            if (movingObjects.ContainsKey(gameObject))
            {
                StopCoroutine(movingObjects[gameObject]);
            }
            movingObjects[gameObject] = StartCoroutine(SmoothMove(finalPosition));
        }
        
        // エフェクト再生
        PlayEffects();
        
        Debug.Log($"プレイヤー {touchingObject.name} がオブジェクト {gameObject.name} に触れました。ポジションを変更します。");
    }
    
    /// <summary>
    /// 滑らかな移動のコルーチン
    /// </summary>
    private IEnumerator SmoothMove(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float journey = 0f;
        
        while (journey <= 1f)
        {
            journey += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, journey);
            yield return null;
        }
        
        transform.position = targetPos;
        Debug.Log($"オブジェクト {gameObject.name} が移動完了しました: {targetPos}");
        
        // 移動完了後、辞書から削除
        if (movingObjects.ContainsKey(gameObject))
        {
            movingObjects.Remove(gameObject);
        }
    }
    
    /// <summary>
    /// エフェクトを再生する
    /// </summary>
    private void PlayEffects()
    {
        // 効果音再生
        if (audioSource != null && executionSound != null)
        {
            audioSource.Play();
        }
        
        // パーティクル再生
        if (executionParticles != null)
        {
            executionParticles.Play();
        }
    }
    
    /// <summary>
    /// 実行状態をリセットする（外部から呼び出し可能）
    /// </summary>
    public void ResetExecution()
    {
        hasExecuted = false;
        Debug.Log($"オブジェクト {gameObject.name} の実行状態がリセットされました");
    }
    
    /// <summary>
    /// ターゲットポジションを動的に変更する
    /// </summary>
    public void SetTargetPosition(Vector3 newPosition)
    {
        targetPosition = newPosition;
    }
    
    /// <summary>
    /// 移動速度を動的に変更する
    /// </summary>
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
    
    /// <summary>
    /// 相対位置モードを切り替える
    /// </summary>
    public void SetUseRelativePosition(bool useRelative)
    {
        useRelativePosition = useRelative;
    }
}