using UnityEngine;
using System.Collections;

public class TimedRotation : MonoBehaviour
{
    [Header("回転設定")]
    [Tooltip("指定秒数後にオブジェクトを回転させる")]
    public float rotationDelay = 3.0f;
    
    [Tooltip("Y軸の回転角度")]
    public float yRotationAngle = -90.0f;
    
    [Tooltip("回転する速度（度/秒）")]
    public float rotationSpeed = 90.0f;
    
    [Header("オプション")]
    [Tooltip("スタート時に自動的にカウント開始するか")]
    public bool autoStart = true;
    
    [Tooltip("回転時に効果音を再生するか")]
    public bool playSound = false;
    
    [Tooltip("回転時の効果音")]
    public AudioClip rotationSound;
    
    [Range(0, 1)]
    [Tooltip("効果音の音量")]
    public float soundVolume = 0.5f;
    
    private bool hasRotated = false;
    private AudioSource audioSource;
    
    private void Start()
    {
        // AudioSourceコンポーネントの取得または追加
        if (playSound && rotationSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
        
        // 自動開始
        if (autoStart)
        {
            StartRotationTimer();
        }
    }
    
    // 外部から呼び出せるタイマー開始メソッド
    public void StartRotationTimer()
    {
        if (!hasRotated)
        {
            StartCoroutine(RotateAfterDelay());
        }
    }
    
    // 指定秒数後に回転させるコルーチン
    private IEnumerator RotateAfterDelay()
    {
        // 指定秒数待機
        yield return new WaitForSeconds(rotationDelay);
        
        // 目標の回転値を設定
        Quaternion targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, yRotationAngle, transform.rotation.eulerAngles.z);
        
        // 効果音を再生
        if (playSound && rotationSound != null && audioSource != null)
        {
            audioSource.clip = rotationSound;
            audioSource.volume = soundVolume;
            audioSource.Play();
        }
        
        // 徐々に回転
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );
            yield return null;
        }
        
        // 確実に目標角度に設定
        transform.rotation = targetRotation;
        hasRotated = true;
    }
    
    // 回転状態をリセット（外部から呼び出し可能）
    public void ResetRotation()
    {
        hasRotated = false;
        StopAllCoroutines();
    }
}