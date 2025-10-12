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
    
    [Header("追加設定")]
    [Tooltip("摩擦力（0: 滑りやすい 〜 1: 摩擦が大きい）")]
    [Range(0f, 1f)]
    public float friction = 0.1f;
    
    [Tooltip("コンベアベルトが動作中かどうか")]
    public bool isActive = true;
    
    [Tooltip("オブジェクトの回転を防止するかどうか")]
    public bool preventRotation = true;
    
    [Tooltip("動作音")]
    public AudioClip movingSound;
    
    [Tooltip("動作音の音量")]
    [Range(0f, 1f)]
    public float soundVolume = 0.5f;
    
    // 内部変数
    private List<Rigidbody> objectsOnBelt = new List<Rigidbody>();
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
            
            if (isActive)
            {
                audioSource.Play();
            }
        }
    }
    
    private void FixedUpdate()
    {
        if (!isActive) return;
        
        // ベルト上の全てのオブジェクトを移動
        for (int i = objectsOnBelt.Count - 1; i >= 0; i--)
        {
            Rigidbody rb = objectsOnBelt[i];
            
            if (rb == null)
            {
                // nullの場合はリストから削除
                objectsOnBelt.RemoveAt(i);
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
            
            // 追加のフィードバック（必要に応じて）
            Debug.Log($"オブジェクト {collision.gameObject.name} がコンベアベルトに乗りました");
        }
    }
    
    private void OnCollisionExit(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        
        if (rb != null && objectsOnBelt.Contains(rb))
        {
            // リストから削除
            objectsOnBelt.Remove(rb);
            
            // 追加のフィードバック（必要に応じて）
            Debug.Log($"オブジェクト {collision.gameObject.name} がコンベアベルトから離れました");
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