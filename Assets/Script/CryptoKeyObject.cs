using UnityEngine;
using System.Collections;

/// <summary>
/// 暗号化処理の演出を行う鍵オブジェクト
/// データ転送や暗号化の視覚的な表現を担当
/// </summary>
public class CryptoKeyObject : MonoBehaviour
{
    [Header("鍵オブジェクト設定")]
    [Tooltip("鍵の種類")]
    public KeyType keyType = KeyType.SymmetricKey;
    
    [Tooltip("鍵の名前（表示用）")]
    public string keyName = "共通鍵";
    
    [Header("演出設定")]
    [Tooltip("生成時のアニメーション時間")]
    public float generateAnimationTime = 1f;
    
    [Tooltip("転送アニメーション時間")]
    public float transferAnimationTime = 2f;
    
    [Tooltip("回転速度")]
    public float rotationSpeed = 90f;
    
    [Tooltip("浮遊効果の強さ")]
    public float floatStrength = 0.5f;
    
    [Tooltip("浮遊速度")]
    public float floatSpeed = 2f;
    
    [Header("エフェクト")]
    [Tooltip("生成時のパーティクル")]
    public ParticleSystem generateEffect;
    
    [Tooltip("転送時のパーティクル")]
    public ParticleSystem transferEffect;
    
    [Tooltip("鍵生成音")]
    public AudioClip generateSound;
    
    [Tooltip("転送音")]
    public AudioClip transferSound;
    
    [Header("転送先設定")]
    [Tooltip("転送先の位置")]
    public Transform transferTarget;
    
    // コンポーネント
    private Renderer keyRenderer;
    private AudioSource audioSource;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    
    // 状態管理
    private bool isGenerated = false;
    private bool isTransferring = false;
    
    public enum KeyType
    {
        SymmetricKey,     // 共通鍵
        PublicKey,        // 公開鍵
        PrivateKey,       // 秘密鍵
        SessionKey        // セッション鍵
    }
    
    private void Start()
    {
        keyRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        originalPosition = transform.position;
        originalScale = transform.localScale;
        
        // 初期状態では非表示
        SetVisible(false);
    }
    
    private void Update()
    {
        if (isGenerated && !isTransferring)
        {
            // 浮遊エフェクト
            ApplyFloatingEffect();
            
            // 回転エフェクト
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }
    
    private void ApplyFloatingEffect()
    {
        float newY = originalPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatStrength;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    /// <summary>
    /// 鍵を生成する演出
    /// </summary>
    public void GenerateKey()
    {
        StartCoroutine(GenerateKeyAnimation());
    }
    
    private IEnumerator GenerateKeyAnimation()
    {
        // 初期設定
        SetVisible(true);
        transform.localScale = Vector3.zero;
        
        // 生成エフェクト
        if (generateEffect != null)
        {
            generateEffect.Play();
        }
        
        // 生成音
        PlaySound(generateSound);
        
        // スケールアップアニメーション
        float elapsedTime = 0f;
        while (elapsedTime < generateAnimationTime)
        {
            float progress = elapsedTime / generateAnimationTime;
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, easedProgress);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = originalScale;
        isGenerated = true;
        
        Debug.Log($"鍵生成完了: {keyName} ({keyType})");
    }
    
    /// <summary>
    /// 鍵を転送する演出
    /// </summary>
    /// <param name="target">転送先の位置</param>
    public void TransferKey(Transform target = null)
    {
        Transform destination = target != null ? target : transferTarget;
        
        if (destination != null)
        {
            StartCoroutine(TransferKeyAnimation(destination));
        }
        else
        {
            Debug.LogWarning("転送先が設定されていません");
        }
    }
    
    private IEnumerator TransferKeyAnimation(Transform destination)
    {
        isTransferring = true;
        
        Vector3 startPosition = transform.position;
        Vector3 endPosition = destination.position;
        
        // 転送エフェクト開始
        if (transferEffect != null)
        {
            transferEffect.Play();
        }
        
        // 転送音
        PlaySound(transferSound);
        
        // 移動アニメーション（放物線軌道）
        float elapsedTime = 0f;
        while (elapsedTime < transferAnimationTime)
        {
            float progress = elapsedTime / transferAnimationTime;
            
            // 放物線軌道の計算
            Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, progress);
            float height = Mathf.Sin(progress * Mathf.PI) * 2f; // 放物線の高さ
            currentPosition.y += height;
            
            transform.position = currentPosition;
            
            // 転送中の回転速度アップ
            transform.Rotate(0, rotationSpeed * 2f * Time.deltaTime, 0);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.position = endPosition;
        isTransferring = false;
        
        Debug.Log($"鍵転送完了: {keyName}");
        
        // 転送後の処理（フェードアウト等）
        yield return StartCoroutine(PostTransferEffect());
    }
    
    private IEnumerator PostTransferEffect()
    {
        // フェードアウト効果
        Color originalColor = keyRenderer.material.color;
        float fadeTime = 1f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
            Color newColor = originalColor;
            newColor.a = alpha;
            keyRenderer.material.color = newColor;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 完全に非表示
        SetVisible(false);
    }
    
    /// <summary>
    /// データ暗号化の演出
    /// </summary>
    /// <param name="dataObject">暗号化するデータオブジェクト</param>
    public void EncryptData(GameObject dataObject)
    {
        StartCoroutine(EncryptionAnimation(dataObject));
    }
    
    private IEnumerator EncryptionAnimation(GameObject dataObject)
    {
        Debug.Log($"{keyName}でデータを暗号化中...");
        
        // 鍵とデータの間にエネルギー線を描画
        LineRenderer energyLine = gameObject.AddComponent<LineRenderer>();
        energyLine.material = new Material(Shader.Find("Sprites/Default"));
        
        Color keyColor = GetKeyColor();
        energyLine.startColor = keyColor;
        energyLine.endColor = keyColor;
        energyLine.startWidth = 0.1f;
        energyLine.endWidth = 0.1f;
        energyLine.positionCount = 2;
        
        float animationTime = 2f;
        float elapsedTime = 0f;
        
        while (elapsedTime < animationTime)
        {
            // エネルギー線の描画
            energyLine.SetPosition(0, transform.position);
            energyLine.SetPosition(1, dataObject.transform.position);
            
            // 線の透明度をアニメーション
            float alpha = Mathf.Sin(elapsedTime * 10f) * 0.5f + 0.5f;
            Color lineColor = keyColor;
            lineColor.a = alpha;
            energyLine.startColor = lineColor;
            energyLine.endColor = lineColor;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // エネルギー線を削除
        Destroy(energyLine);
        
        Debug.Log($"データ暗号化完了: {keyName}");
    }
    
    private Color GetKeyColor()
    {
        switch (keyType)
        {
            case KeyType.SymmetricKey: return Color.blue;
            case KeyType.PublicKey: return Color.green;
            case KeyType.PrivateKey: return Color.red;
            case KeyType.SessionKey: return Color.yellow;
            default: return Color.white;
        }
    }
    
    private void SetVisible(bool visible)
    {
        if (keyRenderer != null)
        {
            keyRenderer.enabled = visible;
        }
        
        // 子オブジェクトも制御
        foreach (Transform child in transform)
        {
            Renderer childRenderer = child.GetComponent<Renderer>();
            if (childRenderer != null)
            {
                childRenderer.enabled = visible;
            }
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
    
    /// <summary>
    /// 鍵オブジェクトをリセット
    /// </summary>
    public void ResetKey()
    {
        StopAllCoroutines();
        
        transform.position = originalPosition;
        transform.localScale = originalScale;
        transform.rotation = Quaternion.identity;
        
        isGenerated = false;
        isTransferring = false;
        
        SetVisible(false);
        
        // マテリアルの透明度をリセット
        if (keyRenderer != null)
        {
            Color color = keyRenderer.material.color;
            color.a = 1f;
            keyRenderer.material.color = color;
        }
    }
    
    /// <summary>
    /// 鍵の種類を設定
    /// </summary>
    /// <param name="type">鍵の種類</param>
    /// <param name="name">鍵の名前</param>
    public void SetKeyType(KeyType type, string name = "")
    {
        keyType = type;
        
        if (!string.IsNullOrEmpty(name))
        {
            keyName = name;
        }
        else
        {
            // デフォルト名を設定
            switch (type)
            {
                case KeyType.SymmetricKey: keyName = "共通鍵"; break;
                case KeyType.PublicKey: keyName = "公開鍵"; break;
                case KeyType.PrivateKey: keyName = "秘密鍵"; break;
                case KeyType.SessionKey: keyName = "セッション鍵"; break;
            }
        }
        
        // マテリアルの色を変更
        if (keyRenderer != null)
        {
            keyRenderer.material.color = GetKeyColor();
        }
    }
}