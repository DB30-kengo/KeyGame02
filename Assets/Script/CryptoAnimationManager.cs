using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 暗号学習ゲームの3D演出を管理するクラス
/// 正解時にデータキューブや鍵オブジェクトがアニメーションする
/// </summary>
public class CryptoAnimationManager : MonoBehaviour
{
    [Header("3Dオブジェクト参照")]
    [Tooltip("データキューブオブジェクト")]
    public GameObject dataCube;
    
    [Tooltip("暗号化後のデータキューブ")]
    public GameObject encryptedDataCube;
    
    [Tooltip("共通鍵オブジェクト")]
    public GameObject symmetricKey;
    
    [Tooltip("公開鍵オブジェクト")]
    public GameObject publicKey;
    
    [Tooltip("秘密鍵オブジェクト")]
    public GameObject privateKey;
    
    [Tooltip("セッション鍵オブジェクト")]
    public GameObject sessionKey;
    
    [Header("ベルトコンベア")]
    [Tooltip("ベルトコンベアのトリガー位置")]
    public Transform conveyorDropPoint;
    
    [Header("演出設定")]
    [Tooltip("移動アニメーション時間")]
    public float moveAnimationTime = 2f;
    
    [Tooltip("変形アニメーション時間")]
    public float transformAnimationTime = 1f;
    
    [Tooltip("光るエフェクトの時間")]
    public float glowEffectTime = 1f;
    
    [System.Serializable]
    public class AnimationPositions
    {
        [Header("共通鍵暗号アニメーション")]
        [Tooltip("暗号化時のデータキューブ移動先")]
        public Vector3 encryptDataPosition = new Vector3(-2, 1, 0);
        
        [Tooltip("暗号化時の共通鍵移動先")]
        public Vector3 encryptKeyPosition = new Vector3(-2, 2, 0);
        
        [Tooltip("鍵の安全転送先")]
        public Vector3 secureTransferPosition = new Vector3(5, 1, 0);
        
        [Tooltip("復号時のデータ移動先")]
        public Vector3 decryptDataPosition = new Vector3(5, 1, 0);
        
        [Tooltip("復号時の鍵移動先")]
        public Vector3 decryptKeyPosition = new Vector3(5, 2, 0);
        
        [Header("公開鍵暗号アニメーション")]
        [Tooltip("鍵ペア表示位置（公開鍵）")]
        public Vector3 publicKeyShowPosition = new Vector3(-1, 2, 0);
        
        [Tooltip("鍵ペア表示位置（秘密鍵）")]
        public Vector3 privateKeyShowPosition = new Vector3(1, 2, 0);
        
        [Tooltip("公開鍵暗号化時のデータ移動先")]
        public Vector3 publicEncryptDataPosition = new Vector3(-2, 1, 0);
        
        [Tooltip("公開鍵暗号化時の公開鍵移動先")]
        public Vector3 publicEncryptKeyPosition = new Vector3(-2, 2, 0);
        
        [Tooltip("公開鍵配布先1")]
        public Vector3 publicKeyDistribute1 = new Vector3(-2, 1, 3);
        
        [Tooltip("公開鍵配布先2")]
        public Vector3 publicKeyDistribute2 = new Vector3(0, 1, 3);
        
        [Tooltip("公開鍵配布先3")]
        public Vector3 publicKeyDistribute3 = new Vector3(2, 1, 3);
        
        [Tooltip("秘密鍵復号時の移動先")]
        public Vector3 privateDecryptPosition = new Vector3(5, 2, 0);
        
        [Tooltip("秘密鍵隠蔽位置")]
        public Vector3 privateKeyHidePosition = new Vector3(5, -1, 0);
        
        [Header("ハイブリッド暗号アニメーション")]
        [Tooltip("セッション鍵暗号化時のデータ移動先")]
        public Vector3 sessionEncryptDataPosition = new Vector3(-2, 1, 0);
        
        [Tooltip("セッション鍵暗号化時のセッション鍵移動先")]
        public Vector3 sessionEncryptKeyPosition = new Vector3(-2, 2, 0);
        
        [Tooltip("セッション鍵の公開鍵暗号化時の移動先")]
        public Vector3 sessionKeyEncryptPosition = new Vector3(0, 1, 0);
        
        [Tooltip("セッション鍵復号時の移動先")]
        public Vector3 sessionKeyDecryptPosition = new Vector3(3, 2, 0);
        
        [Tooltip("最終データ復号位置")]
        public Vector3 finalDataPosition = new Vector3(5, 1, 0);
        
        [Header("特殊演出設定")]
        [Tooltip("弧を描く移動の高さ")]
        public float arcHeight = 3f;
        
        [Tooltip("中間地点のオフセット")]
        public Vector3 meetPointOffset = Vector3.zero;
    }
    
    [Header("アニメーション移動先座標")]
    [Tooltip("アニメーションで使用する移動先座標")]
    public AnimationPositions animPositions = new AnimationPositions();
    
    [Header("初期位置記録")]
    public Vector3 dataCubeStartPos;
    public Vector3 symmetricKeyStartPos;
    public Vector3 publicKeyStartPos;
    public Vector3 privateKeyStartPos;
    public Vector3 sessionKeyStartPos;
    
    // コンポーネント参照
    private Dictionary<string, GameObject> objectMap;
    private Dictionary<GameObject, Vector3> originalPositions;
    private Dictionary<GameObject, Material> originalMaterials;
    
    [Header("エフェクト用マテリアル")]
    public Material glowMaterial;
    public Material encryptedMaterial;
    
    private void Start()
    {
        InitializeObjectMap();
        RecordOriginalStates();
    }
    
    private void InitializeObjectMap()
    {
        objectMap = new Dictionary<string, GameObject>
        {
            { "DataCube", dataCube },
            { "EncryptedDataCube", encryptedDataCube },
            { "SymmetricKey", symmetricKey },
            { "PublicKey", publicKey },
            { "PrivateKey", privateKey },
            { "SessionKey", sessionKey }
        };
    }
    
    private void RecordOriginalStates()
    {
        originalPositions = new Dictionary<GameObject, Vector3>();
        originalMaterials = new Dictionary<GameObject, Material>();
        
        foreach (var obj in objectMap.Values)
        {
            if (obj != null)
            {
                originalPositions[obj] = obj.transform.position;
                
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    originalMaterials[obj] = renderer.material;
                }
            }
        }
        
        // 暗号化キューブは最初は非表示
        if (encryptedDataCube != null)
        {
            encryptedDataCube.SetActive(false);
        }
    }
    
    /// <summary>
    /// 問題の正解時に3D演出を実行
    /// </summary>
    /// <param name="question">正解した問題</param>
    public void PlayCorrectAnswerAnimation(CryptoQuestion question)
    {
        if (question == null || string.IsNullOrEmpty(question.animationType))
        {
            Debug.Log("演出情報がありません");
            return;
        }
        
        StartCoroutine(ExecuteAnimation(question));
    }
    
    private IEnumerator ExecuteAnimation(CryptoQuestion question)
    {
        Debug.Log($"演出開始: {question.animationType}");
        
        switch (question.animationType)
        {
            case "show_symmetric_key":
                yield return StartCoroutine(ShowSymmetricKey());
                break;
                
            case "encrypt_data":
                yield return StartCoroutine(EncryptDataAnimation());
                break;
                
            case "transform_encrypted":
                yield return StartCoroutine(TransformToEncrypted());
                break;
                
            case "transfer_key_secure":
                yield return StartCoroutine(TransferKeySecure());
                break;
                
            case "decrypt_data":
                yield return StartCoroutine(DecryptDataAnimation());
                break;
                
            case "show_key_pair":
                yield return StartCoroutine(ShowKeyPair());
                break;
                
            case "encrypt_with_public":
                yield return StartCoroutine(EncryptWithPublicKey());
                break;
                
            case "transfer_public_key":
                yield return StartCoroutine(TransferPublicKey());
                break;
                
            case "decrypt_with_private":
                yield return StartCoroutine(DecryptWithPrivateKey());
                break;
                
            case "secure_private_key":
                yield return StartCoroutine(SecurePrivateKey());
                break;
                
            case "show_session_key":
                yield return StartCoroutine(ShowSessionKey());
                break;
                
            case "encrypt_with_session":
                yield return StartCoroutine(EncryptWithSessionKey());
                break;
                
            case "encrypt_session_key":
                yield return StartCoroutine(EncryptSessionKey());
                break;
                
            case "decrypt_sequence":
                yield return StartCoroutine(DecryptSequence());
                break;
                
            case "show_advantages":
                yield return StartCoroutine(ShowAdvantages());
                break;
                
            default:
                Debug.LogWarning($"未知の演出タイプ: {question.animationType}");
                break;
        }
        
        Debug.Log("演出完了");
    }
    
    // === 共通鍵暗号の演出 ===
    private IEnumerator ShowSymmetricKey()
    {
        if (symmetricKey != null)
        {
            // 鍵を光らせて注目
            yield return StartCoroutine(GlowEffect(symmetricKey));
        }
    }
    
    private IEnumerator EncryptDataAnimation()
    {
        // データキューブと共通鍵を設定された位置に移動
        if (dataCube != null && symmetricKey != null)
        {
            yield return StartCoroutine(MoveObject(dataCube, animPositions.encryptDataPosition, moveAnimationTime / 2));
            yield return StartCoroutine(MoveObject(symmetricKey, animPositions.encryptKeyPosition, moveAnimationTime / 2));
            
            // 光るエフェクト
            yield return StartCoroutine(GlowEffect(dataCube));
        }
    }
    
    private IEnumerator TransformToEncrypted()
    {
        if (dataCube != null && encryptedDataCube != null)
        {
            // データキューブを暗号化キューブに変換
            Vector3 position = dataCube.transform.position;
            
            // フェードアウト
            yield return StartCoroutine(FadeOut(dataCube));
            dataCube.SetActive(false);
            
            // 暗号化キューブをフェードイン
            encryptedDataCube.transform.position = position;
            encryptedDataCube.SetActive(true);
            yield return StartCoroutine(FadeIn(encryptedDataCube));
        }
    }
    
    private IEnumerator TransferKeySecure()
    {
        if (symmetricKey != null)
        {
            // 鍵を安全な経路で転送
            yield return StartCoroutine(MoveObjectArc(symmetricKey, animPositions.secureTransferPosition, moveAnimationTime, animPositions.arcHeight));
        }
    }
    
    private IEnumerator DecryptDataAnimation()
    {
        if (encryptedDataCube != null && symmetricKey != null && dataCube != null)
        {
            // 暗号化キューブを元のデータキューブに戻す
            Vector3 position = encryptedDataCube.transform.position;
            
            yield return StartCoroutine(FadeOut(encryptedDataCube));
            encryptedDataCube.SetActive(false);
            
            dataCube.transform.position = position;
            dataCube.SetActive(true);
            yield return StartCoroutine(FadeIn(dataCube));
        }
    }
    
    // === 公開鍵暗号の演出 ===
    private IEnumerator ShowKeyPair()
    {
        if (publicKey != null && privateKey != null)
        {
            // 設定された位置に移動してから光らせる
            StartCoroutine(MoveObject(publicKey, animPositions.publicKeyShowPosition, moveAnimationTime / 2));
            yield return StartCoroutine(MoveObject(privateKey, animPositions.privateKeyShowPosition, moveAnimationTime / 2));
            
            // 鍵ペアを同時に光らせる
            StartCoroutine(GlowEffect(publicKey));
            yield return StartCoroutine(GlowEffect(privateKey));
        }
    }
    
    private IEnumerator EncryptWithPublicKey()
    {
        if (dataCube != null && publicKey != null)
        {
            yield return StartCoroutine(MoveObject(dataCube, animPositions.publicEncryptDataPosition, moveAnimationTime / 2));
            yield return StartCoroutine(MoveObject(publicKey, animPositions.publicEncryptKeyPosition, moveAnimationTime / 2));
            
            yield return StartCoroutine(GlowEffect(dataCube));
            yield return StartCoroutine(TransformToEncrypted());
        }
    }
    
    private IEnumerator TransferPublicKey()
    {
        if (publicKey != null)
        {
            // 公開鍵を設定された3箇所に配布
            Vector3[] distributePositions = {
                animPositions.publicKeyDistribute1,
                animPositions.publicKeyDistribute2,
                animPositions.publicKeyDistribute3
            };
            
            for (int i = 0; i < distributePositions.Length; i++)
            {
                GameObject keyClone = Instantiate(publicKey);
                StartCoroutine(MoveObject(keyClone, distributePositions[i], moveAnimationTime));
                
                // 1秒後に削除
                Destroy(keyClone, moveAnimationTime + 1f);
            }
            
            yield return new WaitForSeconds(moveAnimationTime);
        }
    }
    
    private IEnumerator DecryptWithPrivateKey()
    {
        if (encryptedDataCube != null && privateKey != null && dataCube != null)
        {
            yield return StartCoroutine(MoveObject(privateKey, animPositions.privateDecryptPosition, moveAnimationTime));
            yield return StartCoroutine(GlowEffect(encryptedDataCube));
            
            // 復号
            yield return StartCoroutine(DecryptDataAnimation());
        }
    }
    
    private IEnumerator SecurePrivateKey()
    {
        if (privateKey != null)
        {
            // 秘密鍵を設定された隠蔽位置に移動
            yield return StartCoroutine(MoveObject(privateKey, animPositions.privateKeyHidePosition, moveAnimationTime));
            
            // 透明化
            yield return StartCoroutine(FadeOut(privateKey));
        }
    }
    
    // === ハイブリッド暗号の演出 ===
    private IEnumerator ShowSessionKey()
    {
        if (sessionKey != null)
        {
            yield return StartCoroutine(GlowEffect(sessionKey));
        }
    }
    
    private IEnumerator EncryptWithSessionKey()
    {
        if (dataCube != null && sessionKey != null)
        {
            yield return StartCoroutine(MoveObject(dataCube, animPositions.sessionEncryptDataPosition, moveAnimationTime / 2));
            yield return StartCoroutine(MoveObject(sessionKey, animPositions.sessionEncryptKeyPosition, moveAnimationTime / 2));
            
            yield return StartCoroutine(GlowEffect(dataCube));
            yield return StartCoroutine(TransformToEncrypted());
        }
    }
    
    private IEnumerator EncryptSessionKey()
    {
        if (sessionKey != null && publicKey != null)
        {
            yield return StartCoroutine(MoveObject(sessionKey, animPositions.sessionKeyEncryptPosition, moveAnimationTime / 2));
            yield return StartCoroutine(MoveObject(publicKey, animPositions.sessionKeyEncryptPosition + Vector3.up, moveAnimationTime / 2));
            
            yield return StartCoroutine(GlowEffect(sessionKey));
        }
    }
    
    private IEnumerator DecryptSequence()
    {
        // 1. セッション鍵復号
        if (sessionKey != null && privateKey != null)
        {
            yield return StartCoroutine(MoveObject(privateKey, animPositions.sessionKeyDecryptPosition + Vector3.up, moveAnimationTime / 2));
            yield return StartCoroutine(GlowEffect(sessionKey));
        }
        
        // 2. データ復号
        if (encryptedDataCube != null && sessionKey != null)
        {
            yield return StartCoroutine(MoveObject(sessionKey, animPositions.finalDataPosition + Vector3.up, moveAnimationTime / 2));
            yield return StartCoroutine(DecryptDataAnimation());
        }
    }
    
    private IEnumerator ShowAdvantages()
    {
        // すべてのオブジェクトを一斉に光らせて利点を表現
        if (dataCube != null) StartCoroutine(GlowEffect(dataCube));
        if (sessionKey != null) StartCoroutine(GlowEffect(sessionKey));
        if (publicKey != null) StartCoroutine(GlowEffect(publicKey));
        
        yield return new WaitForSeconds(glowEffectTime);
    }
    
    // === 演出用のヘルパーメソッド ===
    private IEnumerator MoveObject(GameObject obj, Vector3 targetPos, float duration)
    {
        if (obj == null) yield break;
        
        Vector3 startPos = obj.transform.position;
        float elapsedTime = 0;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            t = Mathf.SmoothStep(0f, 1f, t); // スムーズな加減速
            
            obj.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        
        obj.transform.position = targetPos;
    }
    
    private IEnumerator MoveObjectArc(GameObject obj, Vector3 targetPos, float duration, float arcHeight)
    {
        if (obj == null) yield break;
        
        Vector3 startPos = obj.transform.position;
        float elapsedTime = 0;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
            
            obj.transform.position = currentPos;
            yield return null;
        }
        
        obj.transform.position = targetPos;
    }
    
    private IEnumerator GlowEffect(GameObject obj)
    {
        if (obj == null) yield break;
        
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null) yield break;
        
        Material originalMat = renderer.material;
        
        // 光るマテリアルに変更
        if (glowMaterial != null)
        {
            renderer.material = glowMaterial;
        }
        
        yield return new WaitForSeconds(glowEffectTime);
        
        // 元のマテリアルに戻す
        renderer.material = originalMat;
    }
    
    private IEnumerator FadeOut(GameObject obj)
    {
        if (obj == null) yield break;
        
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null) yield break;
        
        Material mat = renderer.material;
        Color originalColor = mat.color;
        
        float elapsedTime = 0;
        while (elapsedTime < transformAnimationTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / transformAnimationTime);
            
            Color newColor = originalColor;
            newColor.a = alpha;
            mat.color = newColor;
            
            yield return null;
        }
    }
    
    private IEnumerator FadeIn(GameObject obj)
    {
        if (obj == null) yield break;
        
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null) yield break;
        
        Material mat = renderer.material;
        Color originalColor = mat.color;
        
        // 最初は透明
        Color transparentColor = originalColor;
        transparentColor.a = 0f;
        mat.color = transparentColor;
        
        float elapsedTime = 0;
        while (elapsedTime < transformAnimationTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / transformAnimationTime);
            
            Color newColor = originalColor;
            newColor.a = alpha;
            mat.color = newColor;
            
            yield return null;
        }
        
        mat.color = originalColor;
    }
    
    /// <summary>
    /// オブジェクトをベルトコンベアに落とす
    /// </summary>
    public void DropToConveyor(GameObject obj)
    {
        if (obj != null && conveyorDropPoint != null)
        {
            StartCoroutine(MoveObject(obj, conveyorDropPoint.position, moveAnimationTime));
        }
    }
    
    /// <summary>
    /// すべてのオブジェクトを初期位置にリセット
    /// </summary>
    public void ResetAllObjects()
    {
        foreach (var kvp in originalPositions)
        {
            if (kvp.Key != null)
            {
                kvp.Key.transform.position = kvp.Value;
                kvp.Key.SetActive(true);
                
                // マテリアルもリセット
                if (originalMaterials.ContainsKey(kvp.Key))
                {
                    Renderer renderer = kvp.Key.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = originalMaterials[kvp.Key];
                    }
                }
            }
        }
        
        // 暗号化キューブは非表示
        if (encryptedDataCube != null)
        {
            encryptedDataCube.SetActive(false);
        }
    }
}