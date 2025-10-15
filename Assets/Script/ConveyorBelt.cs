using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ベルトコンベアのように上に乗ったオブジェクトを指定方向に滑らかに移動させるスクリプト
/// </summary>
public class ConveyorBelt : MonoBehaviour
{
    public enum ConveyorDirection
    {
        XAxis,  // X軸方向
        ZAxis   // Z軸方向
    }

    [Header("移動設定")]
    [Tooltip("移動速度（毎秒）")]
    public float speed = 2.0f;
    
    [Tooltip("方向軸の選択")]
    public ConveyorDirection movementAxis = ConveyorDirection.XAxis;
    
    [Tooltip("移動方向（正：右/前、負：左/後）")]
    public float direction = 1.0f;
    
    [Tooltip("移動させる対象のタグ（空欄の場合はすべてのオブジェクトが対象）")]
    public string targetTag = "";
    
    [Header("静止時間設定")]
    [Tooltip("触れた後の静止時間（秒）")]
    public float delayBeforeMoving = 1.0f;
    
    [Header("追加設定")]
    [Tooltip("摩擦力（0: 滑りやすい 〜 1: 摩擦が大きい）")]
    [Range(0f, 1f)]
    public float friction = 0.1f;
    
    [Tooltip("コンベアベルトが動作中かどうか")]
    public bool isActive = true;
    
    [Tooltip("オブジェクトの回転を防止するかどうか")]
    public bool preventRotation = true;
    
    [Tooltip("オブジェクトのrotation値を完全に固定するかどうか")]
    public bool fixRotationValues = true;
    
    [Tooltip("動作音")]
    public AudioClip movingSound;
    
    [Tooltip("動作音の音量")]
    [Range(0f, 1f)]
    public float soundVolume = 0.5f;
    
    // 内部変数
    private List<Rigidbody> objectsOnBelt = new List<Rigidbody>();
    private Dictionary<Rigidbody, bool> objectsReadyToMove = new Dictionary<Rigidbody, bool>();
    private Dictionary<Rigidbody, Quaternion> objectsInitialRotation = new Dictionary<Rigidbody, Quaternion>();
    private AudioSource audioSource;
    
    private void Start()
    {
        // オーディオソースの設定
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && movingSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.volume = soundVolume;
            audioSource.clip = movingSound;
            
            // エディタでの保存フラグ問題を回避
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(gameObject);
            }
            #endif
            
            if (isActive)
            {
                audioSource.Play();
            }
        }
    }
    
    private void FixedUpdate()
    {
        if (!isActive) return;
        
        // ベルト上の全てのオブジェクトを移動（移動準備ができているもののみ）
        for (int i = objectsOnBelt.Count - 1; i >= 0; i--)
        {
            Rigidbody rb = objectsOnBelt[i];
            
            if (rb == null)
            {
                // nullの場合はリストから削除
                objectsOnBelt.RemoveAt(i);
                if (objectsReadyToMove.ContainsKey(rb))
                {
                    objectsReadyToMove.Remove(rb);
                }
                if (objectsInitialRotation.ContainsKey(rb))
                {
                    objectsInitialRotation.Remove(rb);
                }
                continue;
            }
            
            // 移動準備ができていない場合はスキップ
            if (!objectsReadyToMove.ContainsKey(rb) || !objectsReadyToMove[rb])
            {
                continue;
            }
            
            // 選択した軸に基づいて方向ベクトルを作成
            Vector3 forceDirection;
            Vector3 targetVelocity;
            
            if (movementAxis == ConveyorDirection.XAxis)
            {
                // X軸方向の移動
                forceDirection = new Vector3(direction, 0, 0);
                targetVelocity = new Vector3(speed * direction, rb.linearVelocity.y, rb.linearVelocity.z);
            }
            else
            {
                // Z軸方向の移動
                forceDirection = new Vector3(0, 0, direction);
                targetVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, speed * direction);
            }
            
            float forceAmount = speed * (1f - friction);
            
            // Rigidbodyに速度を適用（滑らかな動き）
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, 1f - friction);
            
            // 回転を防ぐ設定が有効な場合
            if (preventRotation)
            {
                rb.angularVelocity = Vector3.zero;
            }

            // 回転値を固定する設定が有効な場合
            if (fixRotationValues && objectsInitialRotation.ContainsKey(rb))
            {
                rb.rotation = objectsInitialRotation[rb];
            }
        }
    }
    
    private void OnCollisionStay(Collision collision)
    {
        if (!isActive) return;
        
        // 対象タグが設定されていて、そのタグと一致しない場合は無視
        if (!string.IsNullOrEmpty(targetTag) && !collision.gameObject.CompareTag(targetTag))
        {
            return;
        }
        
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        
        if (rb != null && !objectsOnBelt.Contains(rb))
        {
            // リストに追加
            objectsOnBelt.Add(rb);
            // 最初は移動準備ができていない状態に設定
            objectsReadyToMove[rb] = false;

            // 初期回転値を記録
            if (fixRotationValues)
            {
                objectsInitialRotation[rb] = rb.rotation;
            }
            
            // 遅延後に移動を開始するコルーチンを開始
            StartCoroutine(DelayedMovementStart(rb));
            
            // 追加のフィードバック（必要に応じて）
            Debug.Log($"オブジェクト {collision.gameObject.name} がコンベアベルトに乗りました（{delayBeforeMoving}秒後に移動開始）");
        }
    }
    
    private void OnCollisionExit(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        
        if (rb != null && objectsOnBelt.Contains(rb))
        {
            // リストから削除
            objectsOnBelt.Remove(rb);
            if (objectsReadyToMove.ContainsKey(rb))
            {
                objectsReadyToMove.Remove(rb);
            }
            if (objectsInitialRotation.ContainsKey(rb))
            {
                objectsInitialRotation.Remove(rb);
            }
            
            // 追加のフィードバック（必要に応じて）
            Debug.Log($"オブジェクト {collision.gameObject.name} がコンベアベルトから離れました");
        }
    }
    
    /// <summary>
    /// 指定した時間後にオブジェクトの移動を開始するコルーチン
    /// </summary>
    private IEnumerator DelayedMovementStart(Rigidbody rb)
    {
        // 指定した秒数だけ待機
        yield return new WaitForSeconds(delayBeforeMoving);
        
        // オブジェクトがまだベルト上にある場合のみ移動を開始
        if (objectsOnBelt.Contains(rb) && objectsReadyToMove.ContainsKey(rb))
        {
            objectsReadyToMove[rb] = true;
            Debug.Log($"オブジェクト {rb.gameObject.name} の移動が開始されました");
        }
    }
    
    /// <summary>
    /// コンベアベルトの動作をオン/オフする
    /// </summary>
    public void ToggleBelt(bool activate)
    {
        isActive = activate;
        
        // サウンドの制御
        if (audioSource != null && movingSound != null)
        {
            if (isActive && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
            else if (!isActive && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
    }
    
    /// <summary>
    /// コンベアベルトの速度を変更する
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    
    /// <summary>
    /// コンベアベルトの方向を反転する
    /// </summary>
    public void ReverseDirection()
    {
        direction *= -1;
    }
    
    /// <summary>
    /// コンベアベルトの移動軸を変更する
    /// </summary>
    public void SetMovementAxis(ConveyorDirection newAxis)
    {
        movementAxis = newAxis;
    }
    
    /// <summary>
    /// 回転防止機能をオン/オフする
    /// </summary>
    public void SetPreventRotation(bool prevent)
    {
        preventRotation = prevent;
    }
}