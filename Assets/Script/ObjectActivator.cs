using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーが触れると他のオブジェクトを表示するスクリプト
/// </summary>
public class ObjectActivator : MonoBehaviour
{
    [Header("トリガー設定")]
    [Tooltip("プレイヤータグ（通常は'Player'）")]
    public string playerTag = "Player";
    
    [Tooltip("トリガーコライダーを使用するか")]
    public bool useTriggerCollider = true;
    
    [Tooltip("一度だけ反応するか（一度表示したら二度と反応しないようにするか）")]
    public bool activateOnce = true;
    
    [Header("表示オブジェクト設定")]
    [Tooltip("触れたときに表示するオブジェクト")]
    public List<GameObject> objectsToActivate = new List<GameObject>();
    
    [Tooltip("オブジェクトを表示するまでの遅延時間（秒）")]
    public float activationDelay = 0.0f;
    
    [Header("効果設定")]
    [Tooltip("触れた時のエフェクト")]
    public GameObject touchEffect;
    
    [Tooltip("触れた時に再生するサウンド")]
    public AudioClip touchSound;
    
    [Tooltip("触れた時にアニメーターのトリガーを実行するか")]
    public bool useTriggerAnimation = false;
    
    [Tooltip("実行するトリガー名")]
    public string animatorTriggerName = "Activate";
    
    // 内部変数
    private bool hasActivated = false;
    private AudioSource audioSource;
    private Animator animator;
    
    private void Start()
    {
        // AudioSourceコンポーネントを取得または追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && touchSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Animatorコンポーネントを取得
        animator = GetComponent<Animator>();
        
        // 表示対象オブジェクトを初期状態で非表示に
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
        
        // エフェクトを非表示に
        if (touchEffect != null)
        {
            touchEffect.SetActive(false);
        }
    }
    
    // 物理的な接触（トリガーでない場合）
    private void OnCollisionEnter(Collision collision)
    {
        if (useTriggerCollider) return;
        
        if (collision.gameObject.CompareTag(playerTag))
        {
            HandleActivation();
        }
    }
    
    // トリガー接触
    private void OnTriggerEnter(Collider other)
    {
        if (!useTriggerCollider) return;
        
        if (other.CompareTag(playerTag))
        {
            HandleActivation();
        }
    }
    
    /// <summary>
    /// オブジェクト接触時の処理
    /// </summary>
    private void HandleActivation()
    {
        // 一度だけ反応モードで既に反応済みなら何もしない
        if (activateOnce && hasActivated)
            return;
            
        hasActivated = true;
        
        // タッチエフェクトを表示
        if (touchEffect != null)
        {
            touchEffect.SetActive(true);
        }
        
        // サウンドを再生
        if (touchSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(touchSound);
        }
        
        // アニメーショントリガーを実行
        if (useTriggerAnimation && animator != null)
        {
            animator.SetTrigger(animatorTriggerName);
        }
        
        // オブジェクトを表示（遅延あり/なし）
        if (activationDelay > 0)
        {
            StartCoroutine(ActivateObjectsWithDelay());
        }
        else
        {
            ActivateObjects();
        }
        
        Debug.Log($"オブジェクト {gameObject.name} がトリガーされ、{objectsToActivate.Count}個のオブジェクトを表示します");
    }
    
    /// <summary>
    /// オブジェクトを遅延表示
    /// </summary>
    private IEnumerator ActivateObjectsWithDelay()
    {
        yield return new WaitForSeconds(activationDelay);
        ActivateObjects();
    }
    
    /// <summary>
    /// オブジェクトを表示
    /// </summary>
    private void ActivateObjects()
    {
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                
                // 表示時のアニメーションなど、追加のロジックをここに記述可能
                Animator objAnimator = obj.GetComponent<Animator>();
                if (objAnimator != null)
                {
                    objAnimator.SetTrigger("Show");
                }
            }
        }
    }
    
    /// <summary>
    /// 外部から手動でオブジェクトを表示するメソッド
    /// </summary>
    public void ActivateObjectsManually()
    {
        HandleActivation();
    }
    
    /// <summary>
    /// 状態をリセットする
    /// </summary>
    public void ResetActivator()
    {
        hasActivated = false;
        
        // エフェクトを非表示に
        if (touchEffect != null)
        {
            touchEffect.SetActive(false);
        }
    }
    
    /// <summary>
    /// 表示オブジェクトを手動で追加
    /// </summary>
    public void AddObjectToActivate(GameObject obj)
    {
        if (obj != null && !objectsToActivate.Contains(obj))
        {
            objectsToActivate.Add(obj);
            obj.SetActive(false);
        }
    }
    
    /// <summary>
    /// 表示オブジェクトを手動で削除
    /// </summary>
    public void RemoveObjectToActivate(GameObject obj)
    {
        if (obj != null && objectsToActivate.Contains(obj))
        {
            objectsToActivate.Remove(obj);
        }
    }
    
    // エディタでの視覚化用
    private void OnDrawGizmosSelected()
    {
        // トリガー範囲を視覚化
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0, 1, 0.5f, 0.3f);
            
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
        
        // 線を引いて表示オブジェクトとの関連を示す
        Gizmos.color = Color.yellow;
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                Gizmos.DrawLine(transform.position, obj.transform.position);
            }
        }
    }
}