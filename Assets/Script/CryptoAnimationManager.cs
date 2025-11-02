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
        [Header("エリア設定")]
        [Tooltip("エリアA（送信側）の位置")]
        public Vector3 areaAPosition = new Vector3(-5, 1, 10);
        
        [Tooltip("エリアB（受信側）の位置")]
        public Vector3 areaBPosition = new Vector3(5, 1, 10);
        
        [Header("共通鍵暗号アニメーション")]
        [Tooltip("エリアAでの鍵作成位置")]
        public Vector3 keyCreationPositionA = new Vector3(-5, 2, 10);
        
        [Tooltip("エリアAでの暗号化位置")]
        public Vector3 encryptionPositionA = new Vector3(-5, 1.5f, 10);
        
        [Tooltip("エリアBでの鍵登場位置")]
        public Vector3 keyAppearPositionB = new Vector3(5, 0, 10);
        
        [Tooltip("エリアBでの復号位置")]
        public Vector3 decryptionPositionB = new Vector3(5, 1.5f, 10);
        
        [Header("公開鍵暗号アニメーション")]
        [Tooltip("エリアBでの鍵ペア作成位置")]
        public Vector3 keyPairCreationB = new Vector3(5, 2, 10);
        
        [Tooltip("公開鍵送信時の中間位置")]
        public Vector3 publicKeyTransferMid = new Vector3(0, 2, 10);
        
        [Tooltip("エリアAでの公開鍵暗号化位置")]
        public Vector3 publicEncryptPositionA = new Vector3(-5, 1.5f, 10);
        
        [Header("ハイブリッド暗号アニメーション")]
        [Tooltip("エリアAでの共通鍵暗号化位置")]
        public Vector3 hybridKeyEncryptA = new Vector3(-5, 2, 10);
        
        [Tooltip("エリアAでの平文暗号化位置")]
        public Vector3 hybridDataEncryptA = new Vector3(-5, 1, 10);
        
        [Tooltip("エリアBでの秘密鍵復号位置")]
        public Vector3 privateKeyDecryptB = new Vector3(5, 2, 10);
        
        [Header("転送設定")]
        [Tooltip("転送時の弧の高さ")]
        public float transferArcHeight = 3f;
        
        [Tooltip("転送速度")]
        public float transferDuration = 2f;
        
        [Header("旧来の演出用座標（下位互換）")]
        [Tooltip("データ暗号化位置")]
        public Vector3 encryptDataPosition = new Vector3(-5, 1.5f, 10);
        
        [Tooltip("鍵暗号化位置")]
        public Vector3 encryptKeyPosition = new Vector3(-5, 2, 10);
        
        [Tooltip("安全転送位置")]
        public Vector3 secureTransferPosition = new Vector3(5, 1, 10);
        
        [Tooltip("弧の高さ")]
        public float arcHeight = 3f;
        
        [Tooltip("公開鍵表示位置")]
        public Vector3 publicKeyShowPosition = new Vector3(5, 2, 10);
        
        [Tooltip("秘密鍵表示位置")]
        public Vector3 privateKeyShowPosition = new Vector3(5, 1, 10);
        
        [Tooltip("公開鍵でのデータ暗号化位置")]
        public Vector3 publicEncryptDataPosition = new Vector3(-5, 1.5f, 10);
        
        [Tooltip("公開鍵での鍵暗号化位置")]
        public Vector3 publicEncryptKeyPosition = new Vector3(-5, 2, 10);
        
        [Tooltip("公開鍵配布位置1")]
        public Vector3 publicKeyDistribute1 = new Vector3(-2, 1, 10);
        
        [Tooltip("公開鍵配布位置2")]
        public Vector3 publicKeyDistribute2 = new Vector3(0, 1, 10);
        
        [Tooltip("公開鍵配布位置3")]
        public Vector3 publicKeyDistribute3 = new Vector3(2, 1, 10);
        
        [Tooltip("秘密鍵での復号位置")]
        public Vector3 privateDecryptPosition = new Vector3(5, 1.5f, 10);
        
        [Tooltip("秘密鍵隠蔽位置")]
        public Vector3 privateKeyHidePosition = new Vector3(5, -1, 10);
        
        [Tooltip("セッション鍵でのデータ暗号化位置")]
        public Vector3 sessionEncryptDataPosition = new Vector3(-5, 1.5f, 10);
        
        [Tooltip("セッション鍵での鍵暗号化位置")]
        public Vector3 sessionEncryptKeyPosition = new Vector3(-5, 2, 10);
        
        [Tooltip("セッション鍵暗号化位置")]
        public Vector3 sessionKeyEncryptPosition = new Vector3(-3, 1.5f, 10);
        
        [Tooltip("セッション鍵復号位置")]
        public Vector3 sessionKeyDecryptPosition = new Vector3(5, 1.5f, 10);
        
        [Tooltip("最終データ位置")]
        public Vector3 finalDataPosition = new Vector3(5, 1, 10);
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

    [System.Serializable]
    public class TransferAreas
    {
        [Header("送信側エリア")]
        [Tooltip("送信者の位置")]
        public Transform senderArea;
        
        [Tooltip("送信準備エリア")]
        public Transform senderStagingArea;
        
        [Header("受信側エリア")]
        [Tooltip("受信者の位置")]
        public Transform receiverArea;
        
        [Tooltip("受信準備エリア")]
        public Transform receiverStagingArea;
        
        [Header("転送経路")]
        [Tooltip("データ転送経路のウェイポイント")]
        public Transform[] dataTransferPath;
        
        [Tooltip("鍵転送経路のウェイポイント")]
        public Transform[] keyTransferPath;
        
        [Header("転送タイミング")]
        [Tooltip("データ転送の遅延時間")]
        public float dataTransferDelay = 1f;
        
        [Tooltip("鍵転送の遅延時間")]
        public float keyTransferDelay = 0.5f;
        
        [Tooltip("転送完了後の待機時間")]
        public float transferCompleteDelay = 2f;
    }
    
    [Header("転送システム")]
    [Tooltip("送信・受信エリアの設定")]
    public TransferAreas transferAreas = new TransferAreas();
    
    // 転送状態管理
    private bool isTransferActive = false;
    private Queue<System.Action> transferQueue = new Queue<System.Action>();

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
    /// <param="question">正解した問題</param>
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
            // 共通鍵暗号方式の新しい手順
            case "create_symmetric_key_a":
                yield return StartCoroutine(CreateSymmetricKeyAtA());
                break;
                
            case "encrypt_data_a":
                yield return StartCoroutine(EncryptDataAtA());
                break;
                
            case "transfer_encrypted_data_atob":
                yield return StartCoroutine(TransferEncryptedDataAtoB());
                break;
                
            case "show_symmetric_key_b":
                yield return StartCoroutine(ShowSymmetricKeyAtB());
                break;
                
            case "decrypt_data_b":
                yield return StartCoroutine(DecryptDataAtB());
                break;
                
            // 公開鍵暗号方式の新しい手順
            case "create_keypair_b":
                yield return StartCoroutine(CreateKeyPairAtB());
                break;
                
            case "transfer_public_key_btoa":
                yield return StartCoroutine(TransferPublicKeyBtoA());
                break;
                
            case "encrypt_with_public_a":
                yield return StartCoroutine(EncryptWithPublicKeyAtA());
                break;
                
            case "transfer_encrypted_data_atob_public":
                yield return StartCoroutine(TransferEncryptedDataAtoBPublic());
                break;
                
            case "decrypt_with_private_b":
                yield return StartCoroutine(DecryptWithPrivateKeyAtB());
                break;
                
            // ハイブリッド暗号方式の新しい手順
            case "create_hybrid_keypair_b":
                yield return StartCoroutine(CreateHybridKeyPairAtB());
                break;
                
            case "transfer_hybrid_public_btoa":
                yield return StartCoroutine(TransferHybridPublicKeyBtoA());
                break;
                
            case "encrypt_symmetric_with_public_a":
                yield return StartCoroutine(EncryptSymmetricKeyWithPublicAtA());
                break;
                
            case "transfer_encrypted_key_atob":
                yield return StartCoroutine(TransferEncryptedKeyAtoB());
                break;
                
            case "decrypt_symmetric_key_b":
                yield return StartCoroutine(DecryptSymmetricKeyAtB());
                break;
                
            case "encrypt_data_with_symmetric_a":
                yield return StartCoroutine(EncryptDataWithSymmetricAtA());
                break;
                
            case "transfer_hybrid_data_atob":
                yield return StartCoroutine(TransferHybridDataAtoB());
                break;
                
            case "decrypt_hybrid_data_b":
                yield return StartCoroutine(DecryptHybridDataAtB());
                break;
                
            // 旧来の演出（下位互換のため残す）
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
    
    // === 新しい手順に対応したアニメーション関数 ===
    
    // 共通鍵暗号方式
    private IEnumerator CreateSymmetricKeyAtA()
    {
        if (symmetricKey != null)
        {
            yield return StartCoroutine(MoveObject(symmetricKey, animPositions.keyCreationPositionA, moveAnimationTime));
            yield return StartCoroutine(GlowEffect(symmetricKey));
        }
    }
    
    private IEnumerator EncryptDataAtA()
    {
        if (dataCube != null && symmetricKey != null)
        {
            yield return StartCoroutine(MoveObject(dataCube, animPositions.encryptionPositionA, moveAnimationTime / 2));
            yield return StartCoroutine(GlowEffect(dataCube));
            yield return StartCoroutine(TransformToEncrypted());
        }
    }
    
    private IEnumerator TransferEncryptedDataAtoB()
    {
        if (encryptedDataCube != null)
        {
            yield return StartCoroutine(MoveObjectArc(encryptedDataCube, animPositions.areaBPosition, 
                animPositions.transferDuration, animPositions.transferArcHeight));
        }
    }
    
    private IEnumerator ShowSymmetricKeyAtB()
    {
        if (symmetricKey != null)
        {
            // エリアBの真下から共通鍵が登場
            GameObject keyAtB = Instantiate(symmetricKey);
            keyAtB.transform.position = animPositions.keyAppearPositionB;
            yield return StartCoroutine(MoveObject(keyAtB, animPositions.areaBPosition, moveAnimationTime));
            yield return StartCoroutine(GlowEffect(keyAtB));
        }
    }
    
    private IEnumerator DecryptDataAtB()
    {
        if (encryptedDataCube != null && dataCube != null)
        {
            Vector3 position = encryptedDataCube.transform.position;
            yield return StartCoroutine(MoveObject(encryptedDataCube, animPositions.decryptionPositionB, moveAnimationTime / 2));
            yield return StartCoroutine(GlowEffect(encryptedDataCube));
            
            // 復号化
            yield return StartCoroutine(FadeOut(encryptedDataCube));
            encryptedDataCube.SetActive(false);
            
            dataCube.transform.position = position;
            dataCube.SetActive(true);
            yield return StartCoroutine(FadeIn(dataCube));
        }
    }
    
    // 公開鍵暗号方式
    private IEnumerator CreateKeyPairAtB()
    {
        if (publicKey != null && privateKey != null)
        {
            yield return StartCoroutine(MoveObject(publicKey, animPositions.keyPairCreationB, moveAnimationTime / 2));
            yield return StartCoroutine(MoveObject(privateKey, animPositions.keyPairCreationB + Vector3.left, moveAnimationTime / 2));
            
            StartCoroutine(GlowEffect(publicKey));
            yield return StartCoroutine(GlowEffect(privateKey));
        }
    }
    
    private IEnumerator TransferPublicKeyBtoA()
    {
        if (publicKey != null)
        {
            yield return StartCoroutine(MoveObjectArc(publicKey, animPositions.areaAPosition, 
                animPositions.transferDuration, animPositions.transferArcHeight));
        }
    }
    
    private IEnumerator EncryptWithPublicKeyAtA()
    {
        if (dataCube != null && publicKey != null)
        {
            yield return StartCoroutine(MoveObject(dataCube, animPositions.publicEncryptPositionA, moveAnimationTime / 2));
            yield return StartCoroutine(GlowEffect(dataCube));
            yield return StartCoroutine(TransformToEncrypted());
        }
    }
    
    private IEnumerator TransferEncryptedDataAtoBPublic()
    {
        if (encryptedDataCube != null)
        {
            yield return StartCoroutine(MoveObjectArc(encryptedDataCube, animPositions.areaBPosition, 
                animPositions.transferDuration, animPositions.transferArcHeight));
        }
    }
    
    private IEnumerator DecryptWithPrivateKeyAtB()
    {
        if (encryptedDataCube != null && privateKey != null && dataCube != null)
        {
            yield return StartCoroutine(MoveObject(privateKey, animPositions.privateKeyDecryptB, moveAnimationTime / 2));
            yield return StartCoroutine(GlowEffect(encryptedDataCube));
            
            // 復号化
            Vector3 position = encryptedDataCube.transform.position;
            yield return StartCoroutine(FadeOut(encryptedDataCube));
            encryptedDataCube.SetActive(false);
            
            dataCube.transform.position = position;
            dataCube.SetActive(true);
            yield return StartCoroutine(FadeIn(dataCube));
        }
    }
    
    // ハイブリッド暗号方式
    private IEnumerator CreateHybridKeyPairAtB()
    {
        if (publicKey != null && privateKey != null)
        {
            yield return StartCoroutine(MoveObject(publicKey, animPositions.keyPairCreationB, moveAnimationTime / 2));
            yield return StartCoroutine(MoveObject(privateKey, animPositions.keyPairCreationB + Vector3.left, moveAnimationTime / 2));
            
            StartCoroutine(GlowEffect(publicKey));
            yield return StartCoroutine(GlowEffect(privateKey));
        }
    }
    
    private IEnumerator TransferHybridPublicKeyBtoA()
    {
        if (publicKey != null)
        {
            yield return StartCoroutine(MoveObjectArc(publicKey, animPositions.areaAPosition, 
                animPositions.transferDuration, animPositions.transferArcHeight));
        }
    }
    
    private IEnumerator EncryptSymmetricKeyWithPublicAtA()
    {
        if (symmetricKey != null && publicKey != null)
        {
            yield return StartCoroutine(MoveObject(symmetricKey, animPositions.hybridKeyEncryptA, moveAnimationTime / 2));
            yield return StartCoroutine(GlowEffect(symmetricKey));
            
            // 共通鍵の暗号化エフェクト
            yield return StartCoroutine(ScaleEffect(symmetricKey, 1.2f, 1f));
        }
    }
    
    private IEnumerator TransferEncryptedKeyAtoB()
    {
        if (symmetricKey != null)
        {
            yield return StartCoroutine(MoveObjectArc(symmetricKey, animPositions.areaBPosition, 
                animPositions.transferDuration, animPositions.transferArcHeight));
        }
    }
    
    private IEnumerator DecryptSymmetricKeyAtB()
    {
        if (symmetricKey != null && privateKey != null)
        {
            yield return StartCoroutine(MoveObject(privateKey, animPositions.privateKeyDecryptB, moveAnimationTime / 2));
            yield return StartCoroutine(GlowEffect(symmetricKey));
        }
    }
    
    private IEnumerator EncryptDataWithSymmetricAtA()
    {
        if (dataCube != null)
        {
            yield return StartCoroutine(MoveObject(dataCube, animPositions.hybridDataEncryptA, moveAnimationTime / 2));
            yield return StartCoroutine(GlowEffect(dataCube));
            yield return StartCoroutine(TransformToEncrypted());
        }
    }
    
    private IEnumerator TransferHybridDataAtoB()
    {
        if (encryptedDataCube != null)
        {
            yield return StartCoroutine(MoveObjectArc(encryptedDataCube, animPositions.areaBPosition, 
                animPositions.transferDuration, animPositions.transferArcHeight));
        }
    }
    
    private IEnumerator DecryptHybridDataAtB()
    {
        if (encryptedDataCube != null && symmetricKey != null && dataCube != null)
        {
            Vector3 position = encryptedDataCube.transform.position;
            yield return StartCoroutine(GlowEffect(encryptedDataCube));
            
            // 復号化
            yield return StartCoroutine(FadeOut(encryptedDataCube));
            encryptedDataCube.SetActive(false);
            
            dataCube.transform.position = position;
            dataCube.SetActive(true);
            yield return StartCoroutine(FadeIn(dataCube));
        }
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
        // 初期化がまだされていない場合は先に初期化を実行
        if (originalPositions == null || originalMaterials == null || objectMap == null)
        {
            Debug.LogWarning("CryptoAnimationManager: 初期化がまだ完了していません。初期化を実行します。");
            InitializeObjectMap();
            RecordOriginalStates();
        }
        
        // originalPositionsが空の場合も再初期化
        if (originalPositions.Count == 0)
        {
            Debug.LogWarning("CryptoAnimationManager: 元の位置が記録されていません。再初期化します。");
            RecordOriginalStates();
        }
        
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
                    if (renderer != null && originalMaterials[kvp.Key] != null)
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
        
        Debug.Log("CryptoAnimationManager: オブジェクトリセット完了");
    }
    
    // === 新しい転送システム（ベルトコンベア代替） ===
    
    /// <summary>
    /// 送信側から受信側へのデータ転送アニメーション
    /// </summary>
    public void TransferDataToReceiver()
    {
        if (!isTransferActive)
        {
            StartCoroutine(ExecuteDataTransfer());
        }
        else
        {
            // 転送中の場合はキューに追加
            transferQueue.Enqueue(() => StartCoroutine(ExecuteDataTransfer()));
        }
    }
    
    /// <summary>
    /// 送信側から受信側への鍵転送アニメーション
    /// </summary>
    public void TransferKeyToReceiver(GameObject keyObject)
    {
        if (!isTransferActive)
        {
            StartCoroutine(ExecuteKeyTransfer(keyObject));
        }
        else
        {
            // 転送中の場合はキューに追加
            transferQueue.Enqueue(() => StartCoroutine(ExecuteKeyTransfer(keyObject)));
        }
    }
    
    private IEnumerator ExecuteDataTransfer()
    {
        isTransferActive = true;
        
        // 転送するデータオブジェクトを決定（暗号化されている場合は暗号化キューブ）
        GameObject dataToTransfer = (encryptedDataCube != null && encryptedDataCube.activeInHierarchy) 
            ? encryptedDataCube : dataCube;
            
        if (dataToTransfer == null || transferAreas.receiverArea == null)
        {
            Debug.LogWarning("転送に必要なオブジェクトまたはエリアが設定されていません");
            isTransferActive = false;
            yield break;
        }
        
        Debug.Log("データ転送開始");
        
        // 1. 送信準備
        if (transferAreas.senderStagingArea != null)
        {
            yield return StartCoroutine(MoveObject(dataToTransfer, transferAreas.senderStagingArea.position, moveAnimationTime / 2));
            yield return new WaitForSeconds(transferAreas.dataTransferDelay);
        }
        
        // 2. 転送経路に沿って移動
        if (transferAreas.dataTransferPath != null && transferAreas.dataTransferPath.Length > 0)
        {
            yield return StartCoroutine(MoveAlongPath(dataToTransfer, transferAreas.dataTransferPath, moveAnimationTime));
        }
        else
        {
            // 経路が設定されていない場合は直接移動
            yield return StartCoroutine(MoveObject(dataToTransfer, transferAreas.receiverArea.position, moveAnimationTime));
        }
        
        // 3. 受信完了演出
        yield return StartCoroutine(GlowEffect(dataToTransfer));
        yield return new WaitForSeconds(transferAreas.transferCompleteDelay);
        
        Debug.Log("データ転送完了");
        isTransferActive = false;
        
        // キューに待機中のタスクがあれば実行
        ProcessTransferQueue();
    }
    
    private IEnumerator ExecuteKeyTransfer(GameObject keyObject)
    {
        isTransferActive = true;
        
        if (keyObject == null || transferAreas.receiverArea == null)
        {
            Debug.LogWarning("転送に必要なオブジェクトまたはエリアが設定されていません");
            isTransferActive = false;
            yield break;
        }
        
        Debug.Log($"鍵転送開始: {keyObject.name}");
        
        // 1. 送信準備
        if (transferAreas.senderStagingArea != null)
        {
            yield return StartCoroutine(MoveObject(keyObject, transferAreas.senderStagingArea.position, moveAnimationTime / 2));
            yield return new WaitForSeconds(transferAreas.keyTransferDelay);
        }
        
        // 2. 鍵転送経路に沿って移動（セキュリティを表現するために弧を描く）
        if (transferAreas.keyTransferPath != null && transferAreas.keyTransferPath.Length > 0)
        {
            yield return StartCoroutine(MoveAlongPathSecure(keyObject, transferAreas.keyTransferPath, moveAnimationTime));
        }
        else
        {
            // 経路が設定されていない場合は弧を描いて移動（安全な転送を表現）
            yield return StartCoroutine(MoveObjectArc(keyObject, transferAreas.receiverArea.position, moveAnimationTime, animPositions.arcHeight));
        }
        
        // 3. 受信完了演出
        yield return StartCoroutine(GlowEffect(keyObject));
        yield return new WaitForSeconds(transferAreas.transferCompleteDelay);
        
        Debug.Log("鍵転送完了");
        isTransferActive = false;
        
        // キューに待機中のタスクがあれば実行
        ProcessTransferQueue();
    }
    
    /// <summary>
    /// 指定された経路に沿ってオブジェクトを移動
    /// </summary>
    private IEnumerator MoveAlongPath(GameObject obj, Transform[] waypoints, float totalDuration)
    {
        if (obj == null || waypoints == null || waypoints.Length == 0) yield break;
        
        float segmentDuration = totalDuration / waypoints.Length;
        
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                yield return StartCoroutine(MoveObject(obj, waypoints[i].position, segmentDuration));
                
                // ウェイポイント通過時の軽いエフェクト
                if (i < waypoints.Length - 1) // 最後のポイント以外
                {
                    yield return StartCoroutine(ScaleEffect(obj, 1.1f, 0.3f));
                }
            }
        }
    }
    
    /// <summary>
    /// セキュアな経路に沿って鍵を移動（弧を描きながら）
    /// </summary>
    private IEnumerator MoveAlongPathSecure(GameObject obj, Transform[] waypoints, float totalDuration)
    {
        if (obj == null || waypoints == null || waypoints.Length == 0) yield break;
        
        float segmentDuration = totalDuration / waypoints.Length;
        
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                // 弧を描いて移動（セキュリティを表現）
                yield return StartCoroutine(MoveObjectArc(obj, waypoints[i].position, segmentDuration, animPositions.arcHeight * 0.5f));
                
                // ウェイポイント通過時のセキュリティエフェクト
                if (i < waypoints.Length - 1)
                {
                    yield return StartCoroutine(GlowEffect(obj));
                }
            }
        }
    }
    
    /// <summary>
    /// オブジェクトのスケールエフェクト
    /// </summary>
    private IEnumerator ScaleEffect(GameObject obj, float maxScale, float duration)
    {
        if (obj == null) yield break;
        
        Vector3 originalScale = obj.transform.localScale;
        Vector3 targetScale = originalScale * maxScale;
        
        // 拡大
        float elapsedTime = 0;
        while (elapsedTime < duration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (duration / 2);
            obj.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }
        
        // 縮小
        elapsedTime = 0;
        while (elapsedTime < duration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (duration / 2);
            obj.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }
        
        obj.transform.localScale = originalScale;
    }
    
    /// <summary>
    /// 転送キューの処理
    /// </summary>
    private void ProcessTransferQueue()
    {
        if (transferQueue.Count > 0 && !isTransferActive)
        {
            System.Action nextTransfer = transferQueue.Dequeue();
            nextTransfer?.Invoke();
        }
    }
    
    /// <summary>
    /// 暗号方式に応じた適切な転送を実行
    /// </summary>
    public void ExecuteCryptoTransfer(CryptoGameManager.CryptoType cryptoType, int stepIndex)
    {
        switch (cryptoType)
        {
            case CryptoGameManager.CryptoType.SymmetricKey:
                ExecuteSymmetricKeyTransfer(stepIndex);
                break;
                
            case CryptoGameManager.CryptoType.PublicKey:
                ExecutePublicKeyTransfer(stepIndex);
                break;
                
            case CryptoGameManager.CryptoType.Hybrid:
                ExecuteHybridTransfer(stepIndex);
                break;
        }
    }
    
    private void ExecuteSymmetricKeyTransfer(int stepIndex)
    {
        switch (stepIndex)
        {
            case 0: // 鍵の表示
                // 転送はまだ行わない
                break;
            case 1: // データ暗号化
                // 転送はまだ行わない
                break;
            case 2: // 暗号化確認
                // 転送はまだ行わない
                break;
            case 3: // 鍵の安全転送
                TransferKeyToReceiver(symmetricKey);
                break;
            case 4: // データ復号
                TransferDataToReceiver();
                break;
        }
    }
    
    private void ExecutePublicKeyTransfer(int stepIndex)
    {
        switch (stepIndex)
        {
            case 0: // 鍵ペア表示
                // 転送はまだ行わない
                break;
            case 1: // 公開鍵で暗号化
                // 転送はまだ行わない
                break;
            case 2: // 公開鍵配布
                TransferKeyToReceiver(publicKey);
                break;
            case 3: // 秘密鍵で復号
                TransferDataToReceiver();
                break;
            case 4: // 秘密鍵保護
                // データは既に転送済み
                break;
        }
    }
    
    private void ExecuteHybridTransfer(int stepIndex)
    {
        switch (stepIndex)
        {
            case 0: // セッション鍵表示
                // 転送はまだ行わない
                break;
            case 1: // セッション鍵で暗号化
                // 転送はまだ行わない
                break;
            case 2: // セッション鍵を公開鍵で暗号化
                TransferKeyToReceiver(sessionKey);
                break;
            case 3: // 復号シーケンス
                TransferDataToReceiver();
                break;
            case 4: // 利点表示
                // すべて転送済み
                break;
        }
    }
}