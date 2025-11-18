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
        public Vector3 areaAPosition = new Vector3(-5, 3, 10);
        
        [Tooltip("エリアB（受信側）の位置")]
        public Vector3 areaBPosition = new Vector3(5, 3, 10);
        
        [Header("共通鍵暗号アニメーション")]
        [Tooltip("エリアAでの鍵作成位置")]
        public Vector3 keyCreationPositionA = new Vector3(-5, 4.5f, 10);
        
        [Tooltip("エリアAでの暗号化位置")]
        public Vector3 encryptionPositionA = new Vector3(-5, 3f, 10);
        
        [Tooltip("エリアBでの鍵登場位置")]
        public Vector3 keyAppearPositionB = new Vector3(5, 0.5f, 10);
        
        [Tooltip("エリアBでの復号位置")]
        public Vector3 decryptionPositionB = new Vector3(5, 3f, 10);
        
        [Header("公開鍵暗号アニメーション")]
        [Tooltip("エリアBでの鍵ペア作成位置")]
        public Vector3 keyPairCreationB = new Vector3(5, 2, 10);
        
        [Tooltip("公開鍵送信時の中間位置")]
        public Vector3 publicKeyTransferMid = new Vector3(0, 3, 10);
        
        [Tooltip("エリアAでの公開鍵暗号化位置")]
        public Vector3 publicEncryptPositionA = new Vector3(-5, 2f, 10);
        
        [Header("ハイブリッド暗号アニメーション")]
        [Tooltip("エリアAでの共通鍵暗号化位置")]
        public Vector3 hybridKeyEncryptA = new Vector3(-5, 2.5f, 10);
        
        [Tooltip("エリアAでの平文暗号化位置")]
        public Vector3 hybridDataEncryptA = new Vector3(-5, 1.5f, 10);
        
        [Tooltip("エリアBでの秘密鍵復号位置")]
        public Vector3 privateKeyDecryptB = new Vector3(5, 2.5f, 10);
        
        [Header("転送設定")]
        [Tooltip("転送時の弧の高さ")]
        public float transferArcHeight = 4f;
        
        [Tooltip("転送速度")]
        public float transferDuration = 2.5f;
        
        [Header("旧来の演出用座標（下位互換）")]
        [Tooltip("データ暗号化位置")]
        public Vector3 encryptDataPosition = new Vector3(-5, 1.5f, 10);
        
        [Tooltip("鍵暗号化位置")]
        public Vector3 encryptKeyPosition = new Vector3(-5, 2.5f, 10);
        
        [Tooltip("安全転送位置")]
        public Vector3 secureTransferPosition = new Vector3(5, 1.5f, 10);
        
        [Tooltip("弧の高さ")]
        public float arcHeight = 4f;
        
        [Tooltip("公開鍵表示位置")]
        public Vector3 publicKeyShowPosition = new Vector3(4, 5, 10);
        
        [Tooltip("秘密鍵表示位置")]
        public Vector3 privateKeyShowPosition = new Vector3(6, 5, 10);
        
        [Tooltip("公開鍵でのデータ暗号化位置")]
        public Vector3 publicEncryptDataPosition = new Vector3(-5, 1.5f, 10);
        
        [Tooltip("公開鍵での鍵暗号化位置")]
        public Vector3 publicEncryptKeyPosition = new Vector3(-5, 3f, 10);
        
        [Tooltip("公開鍵配布位置1")]
        public Vector3 publicKeyDistribute1 = new Vector3(-3, 2f, 8);
        
        [Tooltip("公開鍵配布位置2")]
        public Vector3 publicKeyDistribute2 = new Vector3(0, 2.5f, 10);
        
        [Tooltip("公開鍵配布位置3")]
        public Vector3 publicKeyDistribute3 = new Vector3(3, 2f, 12);
        
        [Tooltip("秘密鍵での復号位置")]
        public Vector3 privateDecryptPosition = new Vector3(5, 1.5f, 10);
        
        [Tooltip("秘密鍵隠蔽位置")]
        public Vector3 privateKeyHidePosition = new Vector3(5, -1, 10);
        
        [Tooltip("セッション鍵でのデータ暗号化位置")]
        public Vector3 sessionEncryptDataPosition = new Vector3(-5, 1.5f, 10);
        
        [Tooltip("セッション鍵での鍵暗号化位置")]
        public Vector3 sessionEncryptKeyPosition = new Vector3(-5, 3f, 10);
        
        [Tooltip("セッション鍵暗号化位置")]
        public Vector3 sessionKeyEncryptPosition = new Vector3(-3, 2.5f, 10);
        
        [Tooltip("セッション鍵復号位置")]
        public Vector3 sessionKeyDecryptPosition = new Vector3(5, 2.5f, 10);
        
        [Tooltip("最終データ位置")]
        public Vector3 finalDataPosition = new Vector3(5, 3, 10);
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
    private Dictionary<GameObject, Vector3> originalScales; // 初期スケールを記録する辞書を追加
    
    // エリアBの鍵オブジェクトを管理する変数を追加
    private GameObject keyAtBObject;

    // 共通鍵の表示状態を管理するフラグを追加
    private bool isSymmetricKeyShownAtB = false;

    [Header("エフェクト用マテリアル")]
    public Material glowMaterial;
    public Material encryptedMaterial;

    [Header("UI要素")]
    [Tooltip("フローティングラベルのプレハブ")]
    public GameObject floatingLabelPrefab;

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

    [Header("エリア参照")]
    [Tooltip("エリアAのTransform")]
    public Transform areaA;
    
    [Tooltip("エリアBのTransform")]
    public Transform areaB;
    
    [Header("鍵生成用の色設定")]
    [Tooltip("秘密鍵の色")]
    public Color privateKeyColor = Color.red;
    
    [Tooltip("公開鍵の色")]
    public Color publicKeyColor = Color.blue;
    
    [Header("エフェクト")]
    [Tooltip("鍵生成時のエフェクト")]
    public GameObject keyGenerationEffect;
    
    [Tooltip("転送完了時のエフェクト")]
    public GameObject transferEffect;
    
    // 生成されたオブジェクトを管理するリスト
    private List<GameObject> generatedObjects = new List<GameObject>();

    private void Start()
    {
        InitializeObjectMap();
        RecordOriginalStates();
        InitializeKeyVisibility(); // 鍵の初期表示状態を設定
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
        originalScales = new Dictionary<GameObject, Vector3>(); // 初期スケールを記録
        
        foreach (var obj in objectMap.Values)
        {
            if (obj != null)
            {
                originalPositions[obj] = obj.transform.position;
                originalScales[obj] = obj.transform.localScale; // 初期スケールを記録
                
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
    /// ゲーム開始時の鍵の表示状態を初期化
    /// データキューブのみ表示、4種の鍵は非表示にする
    /// </summary>
    private void InitializeKeyVisibility()
    {
        Debug.Log("CryptoAnimationManager: 鍵の初期表示状態を設定中...");
        
        // データキューブは表示
        if (dataCube != null)
        {
            dataCube.SetActive(true);
            Debug.Log("データキューブを表示に設定");
        }
        else
        {
            Debug.LogWarning("データキューブがnullです");
        }
        
        // 4種の鍵は非表示にする
        if (symmetricKey != null)
        {
            symmetricKey.SetActive(false);
            Debug.Log("共通鍵を非表示に設定");
        }
        else
        {
            Debug.LogWarning("共通鍵がnullです");
        }
        
        if (publicKey != null)
        {
            publicKey.SetActive(false);
            // 公開鍵の位置を設定された表示位置に配置
            publicKey.transform.position = animPositions.publicKeyShowPosition;
            Debug.Log($"公開鍵を非表示に設定し、位置を {animPositions.publicKeyShowPosition} に配置");
        }
        else
        {
            Debug.LogWarning("公開鍵がnullです");
        }
        
        if (privateKey != null)
        {
            privateKey.SetActive(false);
            // 秘密鍵の位置を設定された表示位置に配置
            privateKey.transform.position = animPositions.privateKeyShowPosition;
            Debug.Log($"秘密鍵を非表示に設定し、位置を {animPositions.privateKeyShowPosition} に配置");
        }
        else
        {
            Debug.LogWarning("秘密鍵がnullです");
        }
        
        if (sessionKey != null)
        {
            sessionKey.SetActive(false);
            Debug.Log("セッション鍵を非表示に設定");
        }
        else
        {
            Debug.LogWarning("セッション鍵がnullです");
        }
        
        // 暗号化キューブも最初は非表示
        if (encryptedDataCube != null)
        {
            encryptedDataCube.SetActive(false);
            Debug.Log("暗号化キューブを非表示に設定");
        }
        else
        {
            Debug.LogWarning("暗号化キューブがnullです");
        }
        
        // 演出状態フラグも初期化
        isSymmetricKeyShownAtB = false;
        Debug.Log("共通鍵表示フラグを初期化");
        
        Debug.Log("CryptoAnimationManager: 初期表示状態設定完了 - データキューブのみ表示");
    }
    
    /// <summary>
    /// 全ての鍵オブジェクトを非表示にする
    /// 各暗号方式の1問目開始時に呼び出される
    /// </summary>
    public void HideAllKeys()
    {
        Debug.Log("CryptoAnimationManager: 全ての鍵を非表示に設定中...");
        
        // 共通鍵を非表示
        if (symmetricKey != null)
        {
            symmetricKey.SetActive(false);
            Debug.Log("共通鍵を非表示に設定");
        }
        
        // 公開鍵を非表示
        if (publicKey != null)
        {
            publicKey.SetActive(false);
            Debug.Log("公開鍵を非表示に設定");
        }
        
        // 秘密鍵を非表示
        if (privateKey != null)
        {
            privateKey.SetActive(false);
            Debug.Log("秘密鍵を非表示に設定");
        }
        
        // セッション鍵を非表示
        if (sessionKey != null)
        {
            sessionKey.SetActive(false);
            Debug.Log("セッション鍵を非表示に設定");
        }
        
        // 暗号化キューブも非表示にする
        if (encryptedDataCube != null)
        {
            encryptedDataCube.SetActive(false);
            Debug.Log("暗号化キューブを非表示に設定");
        }
        
        // データキューブは表示状態を維持
        if (dataCube != null)
        {
            dataCube.SetActive(true);
            Debug.Log("データキューブは表示状態を維持");
        }
        
        // 演出状態フラグをリセット
        isSymmetricKeyShownAtB = false;
        Debug.Log("共通鍵表示フラグをリセット");
        
        Debug.Log("CryptoAnimationManager: 全ての鍵の非表示設定完了");
    }
    
    /// <summary>
    /// 鍵の表示状態を強制的にリセット（デバッグ用）
    /// </summary>
    public void ForceResetKeyVisibility()
    {
        Debug.Log("CryptoAnimationManager: 鍵の表示状態を強制リセット");
        InitializeKeyVisibility();
    }
    
    /// <summary>
    /// 現在の鍵の表示状態をログに出力（デバッグ用）
    /// </summary>
    public void LogKeyVisibilityStatus()
    {
        Debug.Log("=== 鍵の表示状態 ===");
        Debug.Log($"データキューブ: {(dataCube != null ? dataCube.activeInHierarchy.ToString() : "null")}");
        Debug.Log($"共通鍵: {(symmetricKey != null ? symmetricKey.activeInHierarchy.ToString() : "null")}");
        Debug.Log($"公開鍵: {(publicKey != null ? publicKey.activeInHierarchy.ToString() : "null")}");
        Debug.Log($"秘密鍵: {(privateKey != null ? privateKey.activeInHierarchy.ToString() : "null")}");
        Debug.Log($"セッション鍵: {(sessionKey != null ? sessionKey.activeInHierarchy.ToString() : "null")}");
        Debug.Log($"暗号化キューブ: {(encryptedDataCube != null ? encryptedDataCube.activeInHierarchy.ToString() : "null")}");
        Debug.Log("==================");
    }
    
    /// <summary>
    /// 問題のタイミングに合わせて鍵を表示する（改良版）
    /// </summary>
    /// <param name="keyType">表示する鍵の種類</param>
    public void ShowKeyForProblem(string keyType)
    {
        GameObject keyToShow = null;
        string keyName = "";
        
        switch (keyType.ToLower())
        {
            case "symmetric":
            case "共通鍵":
                keyToShow = symmetricKey;
                keyName = "共通鍵";
                break;
                
            case "public":
            case "公開鍵":
                keyToShow = publicKey;
                keyName = "公開鍵";
                break;
                
            case "private":
            case "秘密鍵":
                keyToShow = privateKey;
                keyName = "秘密鍵";
                break;
                
            case "session":
            case "セッション鍵":
                keyToShow = sessionKey;
                keyName = "セッション鍵";
                break;
        }
        
        if (keyToShow != null)
        {
            // 既に表示されている場合でも、再度エフェクトを実行して注目を促す
            if (!keyToShow.activeInHierarchy)
            {
                keyToShow.SetActive(true);
                Debug.Log($"{keyName}を表示状態に設定");
            }
            else
            {
                Debug.Log($"{keyName}は既に表示済み - エフェクトのみ実行");
            }
            
            // 鍵の種類に応じて位置を設定
            switch (keyType.ToLower())
            {
                case "public":
                case "公開鍵":
                    keyToShow.transform.position = animPositions.publicKeyShowPosition;
                    Debug.Log($"公開鍵を位置 {animPositions.publicKeyShowPosition} に配置");
                    break;
                    
                case "private":
                case "秘密鍵":
                    keyToShow.transform.position = animPositions.privateKeyShowPosition;
                    Debug.Log($"秘密鍵を位置 {animPositions.privateKeyShowPosition} に配置");
                    break;
            }
            
            // 表示エフェクトを実行
            StartCoroutine(ShowKeyWithEffect(keyToShow));
            Debug.Log($"鍵表示完了: {keyName}");
        }
        else
        {
            Debug.LogWarning($"指定された鍵が見つかりません: {keyType}");
        }
    }
    
    /// <summary>
    /// 特定の暗号方式で使用する鍵を表示
    /// </summary>
    /// <param name="cryptoType">暗号方式</param>
    public void ShowKeysForCryptoType(CryptoGameManager.CryptoType cryptoType)
    {
        Debug.Log($"暗号方式 {cryptoType} の鍵表示開始");
        
        switch (cryptoType)
        {
            case CryptoGameManager.CryptoType.SymmetricKey:
                Debug.Log("共通鍵を表示");
                ShowKeyForProblem("symmetric");
                break;
                
            case CryptoGameManager.CryptoType.PublicKey:
                Debug.Log("公開鍵と秘密鍵を表示");
                ShowKeyForProblem("public");
                ShowKeyForProblem("private");
                break;
                
            case CryptoGameManager.CryptoType.Hybrid:
                Debug.Log("ハイブリッド暗号：最初は鍵を非表示のまま（1問目で鍵ペア生成から開始）");
                // ハイブリッド暗号は1問目の正解時に鍵ペアを表示するため、ここでは何も表示しない
                break;
        }
        
        // 表示状態をログに出力（デバッグ用）
        LogKeyVisibilityStatus();
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
            case "show_key_pair":
                yield return StartCoroutine(ShowKeyPairForPublicKeyCrypto());
                break;
                
            case "move_public_key_to_a":
                yield return StartCoroutine(MovePublicKeyToAreaA());
                break;
                
            case "transform_data_to_encrypted":
                yield return StartCoroutine(TransformDataToEncryptedAtA());
                break;
                
            case "move_encrypted_cube_to_b":
                yield return StartCoroutine(MoveEncryptedCubeToAreaB());
                break;
                
            case "decrypt_cube_at_b":
                yield return StartCoroutine(DecryptCubeAtAreaB());
                break;
                
            case "create_keypair_a":
                yield return StartCoroutine(CreateKeyPairAtA());
                break;
                
            case "transfer_public_key_atob":
                yield return StartCoroutine(TransferPublicKeyAtoB());
                break;
                
            case "encrypt_with_public_a":
                yield return StartCoroutine(EncryptWithPublicKeyAtA());
                break;
                
            case "transfer_encrypted_data_only_atob":
                yield return StartCoroutine(TransferEncryptedDataOnlyAtoB());
                break;
                
            case "decrypt_with_private_b":
                yield return StartCoroutine(DecryptWithPrivateKeyAtB());
                break;
                
            case "create_keypair_b":
                yield return StartCoroutine(CreateKeyPairAtB());
                break;
                
            case "transfer_public_key_btoa":
                yield return StartCoroutine(TransferPublicKeyBtoA());
                break;
                
            case "encrypt_data_at_a":
                yield return StartCoroutine(EncryptDataAtA());
                break;
                
            case "decrypt_at_b":
                yield return StartCoroutine(DecryptDataAtB());
                break;
                
            // ハイブリッド暗号方式の新しい手順（8問対応）
            case "create_hybrid_keypair_b":
                yield return StartCoroutine(CreateHybridKeyPairAtB());
                break;
                
            case "transfer_hybrid_public_btoa":
                yield return StartCoroutine(TransferHybridPublicKeyBtoA());
                break;
                
            case "create_hybrid_symmetric_key_a":
                yield return StartCoroutine(CreateHybridSymmetricKeyAtA());
                break;
                
            case "encrypt_data_with_symmetric_a":
                yield return StartCoroutine(EncryptDataWithSymmetricAtA());
                break;
                
            case "encrypt_symmetric_with_public_a":
                yield return StartCoroutine(EncryptSymmetricKeyWithPublicAtA());
                break;
                
            case "transfer_encrypted_data_and_session_key_to_b":
                yield return StartCoroutine(TransferEncryptedDataAndSessionKeyToB());
                break;
                
            case "decrypt_session_key_to_symmetric_at_b":
                yield return StartCoroutine(DecryptSessionKeyToSymmetricAtB());
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
                
            case "show_keypair_old":  // 重複を避けるため名前を変更
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
            // 1問目正解時：共通鍵は表示せず、位置のみ設定
            Debug.Log("共通鍵の位置を設定（まだ非表示）");
            
            // 共通鍵は非表示のまま位置だけ設定
            symmetricKey.transform.position = animPositions.keyCreationPositionA;
            
            // エフェクトも実行しない（鍵が見えないため）
            Debug.Log("1問目完了：共通鍵は2問目まで非表示");
        }
        
        // IEnumeratorなので何かyield returnする必要がある
        yield return null;
    }
    
    private IEnumerator EncryptDataAtA()
    {
        if (dataCube != null && symmetricKey != null)
        {
            // データキューブを上昇させずに、現在位置で暗号化エフェクトのみ実行
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
        Debug.Log("ShowSymmetricKeyAtB開始");
        
        // 既に共通鍵がエリアBに表示済みの場合はスキップ
        if (isSymmetricKeyShownAtB)
        {
            Debug.Log("共通鍵は既にエリアBに表示済みのため、演出をスキップします");
            yield break;
        }
        
        if (symmetricKey != null)
        {
            // 共通鍵を表示状態に設定
            symmetricKey.SetActive(true);
            Debug.Log("共通鍵を表示状態に設定");
            
            // エリアBの地面位置から開始（重なりを避けるため少しずらす）
            Vector3 startPosition = new Vector3(animPositions.areaBPosition.x + 1f, 0.5f, animPositions.areaBPosition.z - 1f);
            symmetricKey.transform.position = startPosition;
            Debug.Log($"共通鍵を開始位置に配置: {startPosition}");
            
            // 少し待機
            yield return new WaitForSeconds(0.5f);
            
            // エリアBの適切な高さまで上昇（データキューブと重ならない位置）
            Vector3 targetPosition = new Vector3(animPositions.areaBPosition.x + 1f, animPositions.areaBPosition.y + 1.5f, animPositions.areaBPosition.z - 1f);
            Debug.Log($"共通鍵を目標位置に移動開始: {targetPosition}");
            
            // 滑らかに上昇
            yield return StartCoroutine(MoveObject(symmetricKey, targetPosition, moveAnimationTime / 2));
            
            // 光らせるエフェクト
            yield return StartCoroutine(GlowEffect(symmetricKey));
            
            // 表示完了フラグを設定
            isSymmetricKeyShownAtB = true;
            Debug.Log("共通鍵のエリアB表示完了フラグを設定");
            
            Debug.Log("ShowSymmetricKeyAtB完了");
        }
        else
        {
            Debug.LogWarning("共通鍵オブジェクトが見つかりません");
        }
    }
    
    private IEnumerator DecryptDataAtB()
    {
        Debug.Log("DecryptDataAtB開始");
        
        if (encryptedDataCube != null && symmetricKey != null && dataCube != null)
        {
            // エリアBの位置を取得
            Vector3 areaBPosition = animPositions.areaBPosition;
            
            // 共通鍵を復号位置に移動（必要に応じて）
            Vector3 keyPosition = areaBPosition + Vector3.up * 1f;
            if (Vector3.Distance(symmetricKey.transform.position, keyPosition) > 0.5f)
            {
                yield return StartCoroutine(MoveObject(symmetricKey, keyPosition, moveAnimationTime / 2));
            }
            
            // 復号エフェクト
            StartCoroutine(GlowEffect(symmetricKey));
            yield return StartCoroutine(GlowEffect(encryptedDataCube));
            
            // 暗号キューブの現在位置を記録
            Vector3 currentPosition = encryptedDataCube.transform.position;
            
            // 暗号キューブをフェードアウト
            yield return StartCoroutine(FadeOut(encryptedDataCube));
            encryptedDataCube.SetActive(false);
            
            // データキューブを同じ位置にフェードイン
            dataCube.transform.position = currentPosition;
            dataCube.SetActive(true);
            yield return StartCoroutine(FadeIn(dataCube));
            
            Debug.Log("DecryptDataAtB完了");
        }
        else
        {
            Debug.LogWarning("復号に必要なオブジェクト（暗号キューブ、データキューブ、共通鍵）が見つかりません");
        }
    }
    
    // 公開鍵暗号方式
    private IEnumerator CreateKeyPairAtA()
    {
        if (publicKey != null && privateKey != null)
        {
            // エリアAで鍵ペアを生成
            yield return StartCoroutine(MoveObject(publicKey, animPositions.areaAPosition, moveAnimationTime / 2));
            yield return StartCoroutine(MoveObject(privateKey, animPositions.areaAPosition + Vector3.left, moveAnimationTime / 2));
            
            StartCoroutine(GlowEffect(publicKey));
            yield return StartCoroutine(GlowEffect(privateKey));
        }
    }
    
    private IEnumerator TransferPublicKeyAtoB()
    {
        if (publicKey != null)
        {
            // 公開鍵のみをエリアBに転送（秘密鍵は残す）
            yield return StartCoroutine(MoveObjectArc(publicKey, animPositions.areaBPosition, 
                animPositions.transferDuration, animPositions.transferArcHeight));
        }
    }
    
    private IEnumerator EncryptWithPublicKeyAtA()
    {
        if (dataCube != null)
        {
            // エリアAでデータを暗号化（公開鍵は既にエリアBにあるので、暗号化エフェクトのみ）
            yield return StartCoroutine(GlowEffect(dataCube));
            yield return StartCoroutine(TransformToEncrypted());
        }
    }
    
    private IEnumerator TransferEncryptedDataOnlyAtoB()
    {
        if (encryptedDataCube != null)
        {
            // 暗号化されたデータキューブのみをエリアBに転送
            yield return StartCoroutine(MoveObjectArc(encryptedDataCube, animPositions.areaBPosition, 
                animPositions.transferDuration, animPositions.transferArcHeight));
        }
    }
    
    private IEnumerator DecryptWithPrivateKeyAtB()
    {
        if (encryptedDataCube != null && privateKey != null && dataCube != null)
        {
            // 秘密鍵をエリアBの復号位置に移動
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
    
    public IEnumerator CreateKeyPairAtB()
    {
        Debug.Log("エリアBで鍵ペアを生成中...");
        
        if (publicKey != null && privateKey != null)
        {
            // 既存のキーオブジェクトを使用して、設定された位置に配置
            publicKey.transform.position = animPositions.publicKeyShowPosition;
            privateKey.transform.position = animPositions.privateKeyShowPosition;
            
            // キーオブジェクトを表示状態にする
            publicKey.SetActive(true);
            privateKey.SetActive(true);
            
            Debug.Log($"公開鍵を位置 {animPositions.publicKeyShowPosition} に配置");
            Debug.Log($"秘密鍵を位置 {animPositions.privateKeyShowPosition} に配置");
            
            // エフェクトを実行
            StartCoroutine(GlowEffect(privateKey));
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(GlowEffect(publicKey));
            yield return new WaitForSeconds(1f);
        }
        else
        {
            Debug.LogWarning("公開鍵または秘密鍵オブジェクトが見つかりません");
        }
        
        Debug.Log("エリアBでの鍵ペア生成完了");
    }
    
    private IEnumerator TransferPublicKeyBtoA()
    {
        Debug.Log("公開鍵をエリアBからAに転送中...");
        
        // エリアB付近の公開鍵オブジェクトを探す
        GameObject publicKeyToTransfer = null;
        Vector3 areaBPosition = animPositions.areaBPosition;
        
        foreach (GameObject obj in generatedObjects)
        {
            if (obj != null && obj.name.Contains("公開鍵") && 
                Vector3.Distance(obj.transform.position, areaBPosition) < 3f)
            {
                publicKeyToTransfer = obj;
                break;
            }
        }
        
        if (publicKeyToTransfer == null)
        {
            Debug.LogWarning("転送する公開鍵が見つかりません");
            yield break;
        }
        
        // 転送アニメーション
        Vector3 startPos = publicKeyToTransfer.transform.position;
        Vector3 targetPos = animPositions.areaAPosition + Vector3.up * 2f;
        float transferDuration = 2f;
        
        for (float t = 0; t < transferDuration; t += Time.deltaTime)
        {
            float progress = t / transferDuration;
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
            
            // 弧を描くような軌道
            currentPos.y += Mathf.Sin(progress * Mathf.PI) * 2f;
            
            publicKeyToTransfer.transform.position = currentPos;
            yield return null;
        }
        
        publicKeyToTransfer.transform.position = targetPos;
        
        // 転送完了エフェクト
        if (transferEffect != null)
        {
            Instantiate(transferEffect, targetPos, Quaternion.identity);
        }
        
        Debug.Log("公開鍵の転送完了");
    }
    
    // === 公開鍵暗号方式の新しいアニメーション関数（問題順序対応） ===
    
    /// <summary>
    /// 1問目正解後：鍵ペアを表示
    /// </summary>
    private IEnumerator ShowKeyPairForPublicKeyCrypto()
    {
        Debug.Log("公開鍵暗号：1問目正解 - 鍵ペアを表示");
        
        if (publicKey != null && privateKey != null)
        {
            // 鍵を表示状態にする
            publicKey.SetActive(true);
            privateKey.SetActive(true);
            
            // 設定された表示位置に配置
            publicKey.transform.position = animPositions.publicKeyShowPosition;
            privateKey.transform.position = animPositions.privateKeyShowPosition;
            
            Debug.Log($"公開鍵暗号：公開鍵を位置 {animPositions.publicKeyShowPosition} に配置");
            Debug.Log($"公開鍵暗号：秘密鍵を位置 {animPositions.privateKeyShowPosition} に配置");
            
            // 鍵生成エフェクト（時間差をつけて見やすく）
            yield return StartCoroutine(GlowEffect(privateKey));
            yield return new WaitForSeconds(0.3f);
            yield return StartCoroutine(GlowEffect(publicKey));
            
            // 鍵生成完了のエフェクト
            if (keyGenerationEffect != null)
            {
                Instantiate(keyGenerationEffect, animPositions.areaBPosition, Quaternion.identity);
            }
        }
        
        Debug.Log("鍵ペア表示完了");
    }
    
    /// <summary>
    /// 2問目正解後：公開鍵をエリアAの(-5,4,10)に移動
    /// </summary>
    private IEnumerator MovePublicKeyToAreaA()
    {
        Debug.Log("公開鍵暗号：2問目正解 - 公開鍵をエリアAに移動");
        
        if (publicKey != null)
        {
            Vector3 targetPosition = new Vector3(-5, 4, 10);
            Debug.Log($"公開鍵を{targetPosition}に移動開始");
            
            // 滑らかな弧を描いて移動
            yield return StartCoroutine(MoveObjectArc(publicKey, targetPosition, 
                moveAnimationTime, animPositions.transferArcHeight));
            
            // 移動完了エフェクト
            yield return StartCoroutine(GlowEffect(publicKey));
            
            Debug.Log("公開鍵の移動完了");
        }
        else
        {
            Debug.LogWarning("公開鍵オブジェクトが見つかりません");
        }
    }
    
    /// <summary>
    /// 3問目正解後：エリアAのデータキューブを暗号キューブに入れ替え
    /// </summary>
    private IEnumerator TransformDataToEncryptedAtA()
    {
        Debug.Log("公開鍵暗号：3問目正解 - データキューブを暗号キューブに変換");
        
        if (dataCube != null && encryptedDataCube != null)
        {
            // エリアAの位置を取得
            Vector3 areaAPosition = animPositions.areaAPosition;
            
            // データキューブをエリアAに移動（必要に応じて）
            if (Vector3.Distance(dataCube.transform.position, areaAPosition) > 1f)
            {
                yield return StartCoroutine(MoveObject(dataCube, areaAPosition, moveAnimationTime / 2));
            }
            
            // 暗号化エフェクト
            yield return StartCoroutine(GlowEffect(dataCube));
            
            // データキューブの現在位置を記録
            Vector3 currentPosition = dataCube.transform.position;
            
            // データキューブをフェードアウト
            yield return StartCoroutine(FadeOut(dataCube));
            dataCube.SetActive(false);
            
            // 暗号キューブを同じ位置にフェードイン
            encryptedDataCube.transform.position = currentPosition;
            encryptedDataCube.SetActive(true);
            yield return StartCoroutine(FadeIn(encryptedDataCube));
            
            Debug.Log("データキューブから暗号キューブへの変換完了");
        }
        else
        {
            Debug.LogWarning("データキューブまたは暗号キューブが見つかりません");
        }
    }
    
    /// <summary>
    /// 4問目正解後：暗号キューブをエリアBに移動
    /// </summary>
    private IEnumerator MoveEncryptedCubeToAreaB()
    {
        Debug.Log("公開鍵暗号：4問目正解 - 暗号キューブをエリアBに移動");
        
        if (encryptedDataCube != null && encryptedDataCube.activeInHierarchy)
        {
            Vector3 targetPosition = animPositions.areaBPosition;
            Debug.Log($"暗号キューブを{targetPosition}に移動開始");
            
            // 滑らかな弧を描いて移動
            yield return StartCoroutine(MoveObjectArc(encryptedDataCube, targetPosition, 
                moveAnimationTime, animPositions.transferArcHeight));
            
            // 移動完了エフェクト
            yield return StartCoroutine(GlowEffect(encryptedDataCube));
            
            Debug.Log("暗号キューブの移動完了");
        }
        else
        {
            Debug.LogWarning("暗号キューブが見つからないか、非アクティブです");
        }
    }
    
    /// <summary>
    /// 5問目正解後：エリアBで暗号キューブをデータキューブに置き換え
    /// </summary>
    private IEnumerator DecryptCubeAtAreaB()
    {
        Debug.Log("公開鍵暗号：5問目正解 - 暗号キューブを復号してデータキューブに戻す");
        
        if (encryptedDataCube != null && dataCube != null && privateKey != null)
        {
            // エリアBの位置を取得
            Vector3 areaBPosition = animPositions.areaBPosition;
            
            // 秘密鍵を復号位置に移動（必要に応じて）
            Vector3 privateKeyPosition = areaBPosition + Vector3.up * 1f;
            if (Vector3.Distance(privateKey.transform.position, privateKeyPosition) > 0.5f)
            {
                yield return StartCoroutine(MoveObject(privateKey, privateKeyPosition, moveAnimationTime / 2));
            }
            
            // 復号エフェクト
            StartCoroutine(GlowEffect(privateKey));
            yield return StartCoroutine(GlowEffect(encryptedDataCube));
            
            // 暗号キューブの現在位置を記録
            Vector3 currentPosition = encryptedDataCube.transform.position;
            
            // 暗号キューブをフェードアウト
            yield return StartCoroutine(FadeOut(encryptedDataCube));
            encryptedDataCube.SetActive(false);
            
            // データキューブを同じ位置にフェードイン
            dataCube.transform.position = currentPosition;
            dataCube.SetActive(true);
            yield return StartCoroutine(FadeIn(dataCube));
            
            Debug.Log("暗号キューブからデータキューブへの復号完了");
        }
        else
        {
            Debug.LogWarning("復号に必要なオブジェクト（暗号キューブ、データキューブ、秘密鍵）が見つかりません");
        }
    }

    // === ハイブリッド暗号方式の新しいアニメーション関数（8問対応） ===
    
    /// <summary>
    /// ハイブリッド暗号1問目正解後：エリアBにて秘密鍵と公開鍵のペアを表示（セッション鍵は表示しない）
    /// </summary>
    private IEnumerator CreateHybridKeyPairAtB()
    {
        Debug.Log("ハイブリッド暗号1問目：エリアBで鍵ペア生成（秘密鍵と公開鍵のみ）");
        
        if (publicKey != null && privateKey != null)
        {
            // 鍵を表示状態にする
            publicKey.SetActive(true);
            privateKey.SetActive(true);
            
            // セッション鍵は確実に非表示にする
            if (sessionKey != null)
            {
                sessionKey.SetActive(false);
                Debug.Log("セッション鍵を非表示に設定");
            }
            
            // エリアBの位置に配置（設定された表示位置を使用）
            publicKey.transform.position = animPositions.publicKeyShowPosition;
            privateKey.transform.position = animPositions.privateKeyShowPosition;
            
            Debug.Log($"ハイブリッド暗号：公開鍵を位置 {animPositions.publicKeyShowPosition} に配置");
            Debug.Log($"ハイブリッド暗号：秘密鍵を位置 {animPositions.privateKeyShowPosition} に配置");
            
            // 鍵生成エフェクト（秘密鍵が先、公開鍵が後）
            yield return StartCoroutine(GlowEffect(privateKey));
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(GlowEffect(publicKey));
            
            // 鍵生成完了のエフェクト
            if (keyGenerationEffect != null)
            {
                Instantiate(keyGenerationEffect, animPositions.areaBPosition, Quaternion.identity);
            }
            
            Debug.Log("ハイブリッド暗号1問目：鍵ペア生成完了（秘密鍵・公開鍵のみ表示）");
        }
        else
        {
            Debug.LogWarning("鍵ペア生成に必要なオブジェクトが見つかりません");
        }
    }
    
    /// <summary>
    /// ハイブリッド暗号2問目正解後：公開鍵をエリアAに移動させる（移動して見えるように）
    /// </summary>
    private IEnumerator TransferHybridPublicKeyBtoA()
    {
        Debug.Log("ハイブリッド暗号2問目：公開鍵をエリアBからAに移動");
        
        if (publicKey != null)
        {
            Vector3 targetPosition = animPositions.areaAPosition + Vector3.up * 2f;
            
            // 滑らかな弧を描いて移動（2秒かけて移動）
            yield return StartCoroutine(MoveObjectArc(publicKey, targetPosition, 
                2f, animPositions.transferArcHeight));
            
            // 移動完了エフェクト
            yield return StartCoroutine(GlowEffect(publicKey));
            
            Debug.Log("ハイブリッド暗号2問目：公開鍵の移動完了");
        }
        else
        {
            Debug.LogWarning("公開鍵オブジェクトが見つかりません");
        }
    }
    
    /// <summary>
    /// ハイブリッド暗号3問目正解後：エリアAで新たに共通鍵を表示
    /// </summary>
    private IEnumerator CreateHybridSymmetricKeyAtA()
    {
        Debug.Log("ハイブリッド暗号3問目：エリアAで共通鍵生成");
        
        if (symmetricKey != null)
        {
            // 共通鍵オブジェクトを表示状態にする
            symmetricKey.SetActive(true);
            
            // エリアAの共通鍵作成位置に配置
            Vector3 keyPosition = animPositions.areaAPosition + Vector3.up * 1f;
            symmetricKey.transform.position = keyPosition;
            
            // 共通鍵生成エフェクト
            yield return StartCoroutine(GlowEffect(symmetricKey));
            
            // 生成エフェクト
            if (keyGenerationEffect != null)
            {
                Instantiate(keyGenerationEffect, keyPosition, Quaternion.identity);
            }
            
            Debug.Log("ハイブリッド暗号3問目：共通鍵生成完了");
        }
        else
        {
            Debug.LogWarning("共通鍵オブジェクトが見つかりません");
        }
    }
    
    /// <summary>
    /// ハイブリッド暗号4問目正解後：エリアAでデータキューブを暗号化キューブに変える
    /// </summary>
    private IEnumerator EncryptDataWithSymmetricAtA()
    {
        Debug.Log("ハイブリッド暗号4問目：エリアAで共通鍵を使ってデータを暗号化");
        
        if (dataCube != null && symmetricKey != null && encryptedDataCube != null)
        {
            // データキューブをエリアAの位置に移動（必要に応じて）
            Vector3 encryptPosition = animPositions.areaAPosition;
            if (Vector3.Distance(dataCube.transform.position, encryptPosition) > 1f)
            {
                yield return StartCoroutine(MoveObject(dataCube, encryptPosition, moveAnimationTime / 2));
            }
            
            // 共通鍵を暗号化位置近くに移動
            Vector3 keyPosition = encryptPosition + Vector3.up * 1f;
            yield return StartCoroutine(MoveObject(symmetricKey, keyPosition, moveAnimationTime / 2));
            
            // 暗号化エフェクト
            StartCoroutine(GlowEffect(symmetricKey));
            yield return StartCoroutine(GlowEffect(dataCube));
            
            // データキューブを暗号キューブに変換
            Vector3 currentPosition = dataCube.transform.position;
            yield return StartCoroutine(FadeOut(dataCube));
            dataCube.SetActive(false);
            
            encryptedDataCube.transform.position = currentPosition;
            encryptedDataCube.SetActive(true);
            yield return StartCoroutine(FadeIn(encryptedDataCube));
            
            Debug.Log("ハイブリッド暗号4問目：データの暗号化完了");
        }
        else
        {
            Debug.LogWarning("暗号化に必要なオブジェクトが見つかりません");
        }
    }
    
    /// <summary>
    /// ハイブリッド暗号5問目正解後：エリアAで共通鍵オブジェクトをセッション鍵オブジェクトに変える
    /// （暗号化を見てわかるようにするため）
    /// </summary>
    private IEnumerator EncryptSymmetricKeyWithPublicAtA()
    {
        Debug.Log("ハイブリッド暗号5問目：エリアAで公開鍵を使って共通鍵を暗号化（セッション鍵化）");
        
        if (symmetricKey != null && publicKey != null && sessionKey != null)
        {
            // 共通鍵と公開鍵を暗号化位置に移動
            Vector3 encryptPosition = animPositions.areaAPosition + Vector3.up * 2f;
            Vector3 publicKeyTargetPosition = new Vector3(-3.5f, 5f, 10f); // 指定された移動先
            
            yield return StartCoroutine(MoveObject(symmetricKey, encryptPosition, moveAnimationTime / 2));
            yield return StartCoroutine(MoveObject(publicKey, publicKeyTargetPosition, moveAnimationTime / 2));
            
            Debug.Log($"ハイブリッド暗号5問目：公開鍵を位置 {publicKeyTargetPosition} に移動しました");
            
            // 公開鍵暗号化エフェクト
            StartCoroutine(GlowEffect(publicKey));
            yield return StartCoroutine(GlowEffect(symmetricKey));
            
            // 共通鍵を非表示にして、同じ位置にセッション鍵を表示
            Vector3 keyPosition = symmetricKey.transform.position;
            symmetricKey.SetActive(false);
            
            // セッション鍵を表示（暗号化された共通鍵として）
            sessionKey.transform.position = keyPosition;
            sessionKey.SetActive(true);
            
            // セッション鍵の色を変更して暗号化済みを表現
            Renderer sessionRenderer = sessionKey.GetComponent<Renderer>();
            if (sessionRenderer != null && encryptedMaterial != null)
            {
                sessionRenderer.material = encryptedMaterial;
            }
            else if (sessionRenderer != null)
            {
                // 暗号化マテリアルがない場合は色を変更
                Material tempMaterial = new Material(sessionRenderer.material);
                tempMaterial.color = Color.yellow; // セッション鍵らしい色に
                sessionRenderer.material = tempMaterial;
            }
            
            Debug.Log("ハイブリッド暗号5問目：共通鍵のセッション鍵化完了");
        }
        else
        {
            Debug.LogWarning("共通鍵、公開鍵、またはセッション鍵オブジェクトが見つかりません");
        }
    }
    
    /// <summary>
    /// ハイブリッド暗号6問目正解後：暗号化キューブ、セッションキーオブジェクトをエリアBへ送る
    /// （移動して見えるように）
    /// </summary>
    private IEnumerator TransferEncryptedDataAndSessionKeyToB()
    {
        Debug.Log("ハイブリッド暗号6問目：暗号化キューブとセッション鍵をエリアBに転送");
        
        Vector3 targetPosition = animPositions.areaBPosition;
        
        // 暗号化されたセッション鍵を転送（少し高い位置へ）
        if (sessionKey != null)
        {
            Debug.Log("セッション鍵をエリアBに転送開始");
            StartCoroutine(MoveObjectArc(sessionKey, targetPosition + Vector3.up * 2f, 
                2f, animPositions.transferArcHeight));
        }
        
        // 少し間隔を空けて暗号化されたデータキューブを転送
        yield return new WaitForSeconds(0.5f);
        
        if (encryptedDataCube != null && encryptedDataCube.activeInHierarchy)
        {
            Debug.Log("暗号化キューブをエリアBに転送開始");
            yield return StartCoroutine(MoveObjectArc(encryptedDataCube, targetPosition, 
                2f, animPositions.transferArcHeight));
        }
        
        // 転送完了エフェクト
        if (transferEffect != null)
        {
            Instantiate(transferEffect, targetPosition, Quaternion.identity);
        }
        
        Debug.Log("ハイブリッド暗号6問目：転送完了");
    }
    
    /// <summary>
    /// ハイブリッド暗号7問目正解後：エリアBでセッション鍵オブジェクトを共通鍵オブジェクトに変更する
    /// </summary>
    private IEnumerator DecryptSessionKeyToSymmetricAtB()
    {
        Debug.Log("ハイブリッド暗号7問目：エリアBでセッション鍵を共通鍵に復号");
        
        if (sessionKey != null && privateKey != null && symmetricKey != null)
        {
            // 秘密鍵を復号位置に移動
            Vector3 decryptPosition = animPositions.areaBPosition + Vector3.up * 2f;
            yield return StartCoroutine(MoveObject(privateKey, decryptPosition + Vector3.left, moveAnimationTime / 2));
            
            // 復号エフェクト
            StartCoroutine(GlowEffect(privateKey));
            yield return StartCoroutine(GlowEffect(sessionKey));
            
            // セッション鍵の現在位置を記録
            Vector3 sessionKeyPosition = sessionKey.transform.position;
            
            // セッション鍵をフェードアウトして非表示
            yield return StartCoroutine(FadeOut(sessionKey));
            sessionKey.SetActive(false);
            Debug.Log("セッション鍵を非表示にしました");
            
            // 共通鍵を同じ位置に配置してフェードイン
            symmetricKey.transform.position = sessionKeyPosition;
            symmetricKey.SetActive(true);
            
            // 共通鍵のマテリアルを元の状態に戻す（復号済みを表現）
            Renderer symmetricRenderer = symmetricKey.GetComponent<Renderer>();
            if (symmetricRenderer != null && originalMaterials.ContainsKey(symmetricKey))
            {
                symmetricRenderer.material = originalMaterials[symmetricKey];
            }
            else if (symmetricRenderer != null)
            {
                // 元のマテリアルがない場合は共通鍵らしい色に
                Material tempMaterial = new Material(symmetricRenderer.material);
                tempMaterial.color = Color.green; // 共通鍵らしい色
                symmetricRenderer.material = tempMaterial;
            }
            
            // 共通鍵をフェードイン
            yield return StartCoroutine(FadeIn(symmetricKey));
            
            // 復号完了エフェクト
            yield return StartCoroutine(GlowEffect(symmetricKey));
            
            Debug.Log("ハイブリッド暗号7問目：セッション鍵オブジェクトを共通鍵オブジェクトに変更完了");
        }
        else
        {
            Debug.LogWarning("セッション鍵、秘密鍵、または共通鍵オブジェクトが見つかりません");
        }
        
        yield return new WaitForSeconds(1f);
    }
    
    /// <summary>
    /// ハイブリッド暗号8問目正解後：暗号化キューブをデータキューブに変える
    /// </summary>
    private IEnumerator DecryptHybridDataAtB()
    {
        Debug.Log("ハイブリッド暗号8問目：復号した共通鍵でデータを復号");
        
        if (encryptedDataCube != null && sessionKey != null && dataCube != null)
        {
            // セッション鍵をデータ復号位置に移動
            Vector3 decryptPosition = animPositions.areaBPosition + Vector3.up * 1f;
            yield return StartCoroutine(MoveObject(sessionKey, decryptPosition, moveAnimationTime / 2));
            
            // データ復号エフェクト
            StartCoroutine(GlowEffect(sessionKey));
            yield return StartCoroutine(GlowEffect(encryptedDataCube));
            
            // 暗号キューブの現在位置を記録
            Vector3 currentPosition = encryptedDataCube.transform.position;
            
            // 暗号キューブをフェードアウト
            yield return StartCoroutine(FadeOut(encryptedDataCube));
            encryptedDataCube.SetActive(false);
            
            // データキューブを同じ位置にフェードイン
            dataCube.transform.position = currentPosition;
            dataCube.SetActive(true);
            yield return StartCoroutine(FadeIn(dataCube));
            
            // 最終完了エフェクト - すべてのオブジェクトを光らせる
            if (dataCube != null) StartCoroutine(GlowEffect(dataCube));
            if (sessionKey != null) StartCoroutine(GlowEffect(sessionKey));
            if (publicKey != null) StartCoroutine(GlowEffect(publicKey));
            if (privateKey != null) StartCoroutine(GlowEffect(privateKey));
            
            Debug.Log("ハイブリッド暗号8問目：全プロセス完了");
        }
        else
        {
            Debug.LogWarning("復号に必要なオブジェクトが見つかりません");
        }
        
        yield return new WaitForSeconds(2f);
    }
    
    /// <summary>
    /// 鍵表示時のエフェクトを実行するコルーチン
    /// </summary>
    /// <param name="keyObject">エフェクトを適用する鍵オブジェクト</param>
    /// <returns></returns>
    private IEnumerator ShowKeyWithEffect(GameObject keyObject)
    {
        if (keyObject == null) 
        {
            Debug.LogWarning("ShowKeyWithEffect: 鍵オブジェクトがnullです");
            yield break;
        }

        Debug.Log($"ShowKeyWithEffect: {keyObject.name}にエフェクトを適用中");

        // 1. スケールアップエフェクト（注目を促す）
        // 記録されている初期スケールを使用、なければ現在のスケールを使用
        Vector3 originalScale;
        if (originalScales != null && originalScales.ContainsKey(keyObject))
        {
            originalScale = originalScales[keyObject];
        }
        else
        {
            originalScale = keyObject.transform.localScale;
            Debug.LogWarning($"ShowKeyWithEffect: {keyObject.name}の初期スケールが記録されていません。現在のスケールを使用します。");
        }
        
        Vector3 targetScale = originalScale * 1.2f;
        
        float scaleTime = 0.5f;
        float elapsedTime = 0f;
        
        // 拡大アニメーション
        while (elapsedTime < scaleTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / scaleTime;
            keyObject.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }
        
        // 2. 光るエフェクト
        yield return StartCoroutine(GlowEffect(keyObject));
        
        // 3. 元のスケールに戻す
        elapsedTime = 0f;
        while (elapsedTime < scaleTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / scaleTime;
            keyObject.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }
        
        // 確実に初期スケールに戻す
        keyObject.transform.localScale = originalScale;
        
        Debug.Log($"ShowKeyWithEffect: {keyObject.name}のエフェクト完了");
    }

    /// <summary>
    /// 全てのオブジェクトを初期状態にリセット
    /// ゲーム開始時や次の暗号方式に移行する際に呼び出される
    /// </summary>
    public void ResetAllObjects()
    {
        Debug.Log("CryptoAnimationManager: 全オブジェクトをリセット中...");
        
        // 辞書が初期化されていない場合は初期化を実行
        if (originalPositions == null || originalMaterials == null || objectMap == null)
        {
            Debug.LogWarning("CryptoAnimationManager: 辞書が初期化されていません。初期化を実行します。");
            InitializeObjectMap();
            RecordOriginalStates();
            return; // 初期化だけして終了
        }
        
        // アクティブなコルーチンを停止
        StopAllCoroutines();
        
        // 全てのオブジェクトを初期位置に戻す
        foreach (var kvp in originalPositions)
        {
            if (kvp.Key != null)
            {
                kvp.Key.transform.position = kvp.Value;
                
                // スケールも初期状態に戻す（originalScalesから取得）
                if (originalScales != null && originalScales.ContainsKey(kvp.Key))
                {
                    kvp.Key.transform.localScale = originalScales[kvp.Key];
                }
                else
                {
                    // 初期スケールが記録されていない場合はデフォルト値を使用
                    kvp.Key.transform.localScale = Vector3.one;
                    Debug.LogWarning($"ResetAllObjects: {kvp.Key.name}の初期スケールが記録されていません。デフォルト値を使用します。");
                }
            }
        }
        
        // 全てのオブジェクトのマテリアルを初期状態に戻す
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key != null)
            {
                Renderer renderer = kvp.Key.GetComponent<Renderer>();
                if (renderer != null && kvp.Value != null)
                {
                    renderer.material = kvp.Value;
                }
            }
        }
        
        // 表示状態を初期化
        InitializeKeyVisibility();
        
        // 生成されたオブジェクトを削除
        if (generatedObjects != null)
        {
            foreach (GameObject obj in generatedObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            generatedObjects.Clear();
        }
        
        // エリアBの鍵オブジェクトをクリア
        if (keyAtBObject != null)
        {
            Destroy(keyAtBObject);
            keyAtBObject = null;
        }
        
        // 転送状態をリセット
        isTransferActive = false;
        if (transferQueue != null)
        {
            transferQueue.Clear();
        }

        // 演出状態フラグをリセット
        isSymmetricKeyShownAtB = false;
        Debug.Log("共通鍵表示フラグをリセット");
        
        Debug.Log("CryptoAnimationManager: オブジェクトリセット完了");
    }

    // === 基本的なアニメーション効果メソッド ===
    
    /// <summary>
    /// オブジェクトを指定位置まで移動させる
    /// </summary>
    private IEnumerator MoveObject(GameObject obj, Vector3 targetPosition, float duration)
    {
        if (obj == null)
        {
            Debug.LogWarning("MoveObject: オブジェクトがnullです");
            yield break;
        }
        
        Vector3 startPosition = obj.transform.position;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            // スムーズなイージング
            t = Mathf.SmoothStep(0f, 1f, t);
            
            obj.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
        
        obj.transform.position = targetPosition;
    }
    
    /// <summary>
    /// オブジェクトを弧を描きながら移動させる
    /// </summary>
    private IEnumerator MoveObjectArc(GameObject obj, Vector3 targetPosition, float duration, float arcHeight)
    {
        if (obj == null)
        {
            Debug.LogWarning("MoveObjectArc: オブジェクトがnullです");
            yield break;
        }
        
        Vector3 startPosition = obj.transform.position;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            // 線形補間で基本位置を計算
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, t);
            
            // 弧の高さを追加（sin波で弧を描く）
            currentPosition.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
            
            obj.transform.position = currentPosition;
            yield return null;
        }
        
        obj.transform.position = targetPosition;
    }
    
    /// <summary>
    /// オブジェクトを光らせるエフェクト
    /// </summary>
    private IEnumerator GlowEffect(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("GlowEffect: オブジェクトがnullです");
            yield break;
        }
        
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning($"GlowEffect: {obj.name}にRendererが見つかりません");
            yield break;
        }
        
        Material originalMaterial = renderer.material;
        
        // 光るマテリアルがあれば使用、なければ元のマテリアルの色を変更
        if (glowMaterial != null)
        {
            renderer.material = glowMaterial;
        }
        else
        {
            // マテリアルのコピーを作成して色を変更
            Material tempMaterial = new Material(originalMaterial);
            Color glowColor = tempMaterial.color * 2f; // 明るくする
            tempMaterial.color = glowColor;
            renderer.material = tempMaterial;
        }
        
        // 光る時間だけ待機
        yield return new WaitForSeconds(glowEffectTime);
        
        // 元のマテリアルに戻す
        renderer.material = originalMaterial;
    }
    
    /// <summary>
    /// オブジェクトをフェードアウトさせる
    /// </summary>
    private IEnumerator FadeOut(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("FadeOut: オブジェクトがnullです");
            yield break;
        }
        
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning($"FadeOut: {obj.name}にRendererが見つかりません");
            yield break;
        }
        
        Material material = renderer.material;
        Color originalColor = material.color;
        float fadeDuration = 0.5f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            
            Color newColor = originalColor;
            newColor.a = alpha;
            material.color = newColor;
            
            yield return null;
        }
        
        // 完全に透明にする
        Color finalColor = originalColor;
        finalColor.a = 0f;
        material.color = finalColor;
    }
    
    /// <summary>
    /// オブジェクトをフェードインさせる
    /// </summary>
    private IEnumerator FadeIn(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("FadeIn: オブジェクトがnullです");
            yield break;
        }
        
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning($"FadeIn: {obj.name}にRendererが見つかりません");
            yield break;
        }
        
        Material material = renderer.material;
        Color originalColor = material.color;
        float fadeDuration = 0.5f;
        float elapsedTime = 0f;
        
        // 最初は透明にする
        Color startColor = originalColor;
        startColor.a = 0f;
        material.color = startColor;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            
            Color newColor = originalColor;
            newColor.a = alpha;
            material.color = newColor;
            
            yield return null;
        }
        
        // 完全に不透明にする
        material.color = originalColor;
    }
    
    /// <summary>
    /// 指定位置に鍵オブジェクトを作成
    /// </summary>
    private GameObject CreateKeyObject(Vector3 position, Color color, string keyName)
    {
        // プリミティブなキューブを作成
        GameObject keyObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        keyObj.name = keyName;
        keyObj.transform.position = position;
        keyObj.transform.localScale = Vector3.one * 0.5f; // 少し小さくする
        
        // 色を設定
        Renderer renderer = keyObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material keyMaterial = new Material(Shader.Find("Standard"));
            keyMaterial.color = color;
            renderer.material = keyMaterial;
        }
        
        return keyObj;
    }

    /// <summary>
    /// 暗号方式とステップに応じてアニメーションを実行
    /// CryptoGameManagerから呼び出される統合インターフェース
    /// </summary>
    /// <param name="cryptoType">暗号方式</param>
    /// <param name="stepIndex">ステップ番号</param>
    public void ExecuteCryptoTransfer(CryptoGameManager.CryptoType cryptoType, int stepIndex)
    {
        Debug.Log($"ExecuteCryptoTransfer: {cryptoType} - ステップ {stepIndex}");
        
        // 既にアニメーション実行中の場合は待機キューに追加
        if (isTransferActive)
        {
            Debug.Log("アニメーション実行中のため、キューに追加");
            transferQueue.Enqueue(() => ExecuteCryptoTransfer(cryptoType, stepIndex));
            return;
        }
        
        // アニメーションを開始
        StartCoroutine(ExecuteCryptoTransferCoroutine(cryptoType, stepIndex));
    }
    
    /// <summary>
    /// 暗号方式とステップに応じてアニメーションを実行するコルーチン
    /// </summary>
    private IEnumerator ExecuteCryptoTransferCoroutine(CryptoGameManager.CryptoType cryptoType, int stepIndex)
    {
        isTransferActive = true;
        
        try
        {
            switch (cryptoType)
            {
                case CryptoGameManager.CryptoType.SymmetricKey:
                    yield return StartCoroutine(ExecuteSymmetricKeyStep(stepIndex));
                    break;
                    
                case CryptoGameManager.CryptoType.PublicKey:
                    yield return StartCoroutine(ExecutePublicKeyStep(stepIndex));
                    break;
                    
                case CryptoGameManager.CryptoType.Hybrid:
                    yield return StartCoroutine(ExecuteHybridStep(stepIndex));
                    break;
                    
                default:
                    Debug.LogWarning($"未対応の暗号方式: {cryptoType}");
                    break;
            }
        }
        finally
        {
            isTransferActive = false;
            
            // キューに待機中のアニメーションがあれば実行
            if (transferQueue.Count > 0)
            {
                System.Action nextAnimation = transferQueue.Dequeue();
                nextAnimation?.Invoke();
            }
        }
    }
    
    /// <summary>
    /// 共通鍵暗号のステップ実行
    /// </summary>
    private IEnumerator ExecuteSymmetricKeyStep(int stepIndex)
    {
        Debug.Log($"共通鍵暗号ステップ {stepIndex} を実行");
        
        switch (stepIndex)
        {
            case 0: // 鍵生成
                yield return StartCoroutine(CreateSymmetricKeyAtA());
                break;
                
            case 1: // データ暗号化
                yield return StartCoroutine(EncryptDataAtA());
                break;
                
            case 2: // 暗号化データ転送
                yield return StartCoroutine(TransferEncryptedDataAtoB());
                break;
                
            case 3: // エリアBで鍵表示
                yield return StartCoroutine(ShowSymmetricKeyAtB());
                break;
                
            case 4: // データ復号
                yield return StartCoroutine(DecryptDataAtB());
                break;
                
            default:
                Debug.LogWarning($"共通鍵暗号：未対応のステップ {stepIndex}");
                break;
        }
    }
    
    /// <summary>
    /// 公開鍵暗号のステップ実行
    /// </summary>
    private IEnumerator ExecutePublicKeyStep(int stepIndex)
    {
        Debug.Log($"公開鍵暗号ステップ {stepIndex} を実行");
        
        switch (stepIndex)
        {
            case 0: // 鍵ペア生成
                yield return StartCoroutine(ShowKeyPairForPublicKeyCrypto());
                break;
                
            case 1: // 公開鍵転送
                yield return StartCoroutine(MovePublicKeyToAreaA());
                break;
                
            case 2: // データ暗号化
                yield return StartCoroutine(TransformDataToEncryptedAtA());
                break;
                
            case 3: // 暗号化データ転送
                yield return StartCoroutine(MoveEncryptedCubeToAreaB());
                break;
                
            case 4: // データ復号
                yield return StartCoroutine(DecryptCubeAtAreaB());
                break;
                
            default:
                Debug.LogWarning($"公開鍵暗号：未対応のステップ {stepIndex}");
                break;
        }
    }
    
    /// <summary>
    /// ハイブリッド暗号のステップ実行
    /// </summary>
    private IEnumerator ExecuteHybridStep(int stepIndex)
    {
        Debug.Log($"ハイブリッド暗号ステップ {stepIndex} を実行");
        
        switch (stepIndex)
        {
            case 0: // 1問目：鍵ペア生成
                yield return StartCoroutine(CreateHybridKeyPairAtB());
                break;
                
            case 1: // 2問目：公開鍵転送
                yield return StartCoroutine(TransferHybridPublicKeyBtoA());
                break;
                
            case 2: // 3問目：共通鍵生成
                yield return StartCoroutine(CreateHybridSymmetricKeyAtA());
                break;
                
            case 3: // 4問目：データ暗号化
                yield return StartCoroutine(EncryptDataWithSymmetricAtA());
                break;
                
            case 4: // 5問目：共通鍵暗号化（セッション鍵化）
                yield return StartCoroutine(EncryptSymmetricKeyWithPublicAtA());
                break;
                
            case 5: // 6問目：暗号化データとセッション鍵転送
                yield return StartCoroutine(TransferEncryptedDataAndSessionKeyToB());
                break;
                
            case 6: // 7問目：セッション鍵復号
                yield return StartCoroutine(DecryptSessionKeyToSymmetricAtB());
                break;
                
            case 7: // 8問目：データ復号
                yield return StartCoroutine(DecryptHybridDataAtB());
                break;
                
            default:
                Debug.LogWarning($"ハイブリッド暗号：未対応のステップ {stepIndex}");
                break;
        }
    }

    /// <summary>
    /// キー位置テスト用メソッド（デバッグ専用）
    /// Unityエディタから手動で呼び出して位置をテストできます
    /// </summary>
    [ContextMenu("Test Key Positions")]
    public void TestKeyPositions()
    {
        Debug.Log("=== キー位置テスト開始 ===");
        
        if (publicKey != null)
        {
            publicKey.transform.position = animPositions.publicKeyShowPosition;
            publicKey.SetActive(true);
            Debug.Log($"公開鍵を位置 {animPositions.publicKeyShowPosition} に配置しました");
        }
        else
        {
            Debug.LogError("公開鍵オブジェクトが見つかりません");
        }
        
        if (privateKey != null)
        {
            privateKey.transform.position = animPositions.privateKeyShowPosition;
            privateKey.SetActive(true);
            Debug.Log($"秘密鍵を位置 {animPositions.privateKeyShowPosition} に配置しました");
        }
        else
        {
            Debug.LogError("秘密鍵オブジェクトが見つかりません");
        }
        
        Debug.Log("=== キー位置テスト完了 ===");
    }
    
    /// <summary>
    /// 現在のキー位置をログに出力（デバッグ用）
    /// </summary>
    [ContextMenu("Log Current Key Positions")]
    public void LogCurrentKeyPositions()
    {
        Debug.Log("=== 現在のキー位置 ===");
        
        if (publicKey != null)
        {
            Debug.Log($"公開鍵の現在位置: {publicKey.transform.position} (期待位置: {animPositions.publicKeyShowPosition})");
        }
        
        if (privateKey != null)
        {
            Debug.Log($"秘密鍵の現在位置: {privateKey.transform.position} (期待位置: {animPositions.privateKeyShowPosition})");
        }
        
        Debug.Log("==================");
    }
}