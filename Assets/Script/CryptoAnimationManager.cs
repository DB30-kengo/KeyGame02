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

	[Header("エフェクト用マテリアル")]
	public Material glowMaterial;
	public Material encryptedMaterial;

	[Header("エフェクト")]
	[Tooltip("鍵生成時のエフェクト")]
	public GameObject keyGenerationEffect;

	[Tooltip("転送完了時のエフェクト")]
	public GameObject transferEffect;

	// アニメーション用の固定値
	private readonly Vector3 areaAPosition = new Vector3(-5, 3, 10);
	private readonly Vector3 areaBPosition = new Vector3(5, 3, 10);
	private readonly Vector3 keyCreationPositionA = new Vector3(-5, 4.5f, 10);
	private readonly Vector3 encryptionPositionA = new Vector3(-5, 3f, 10);
	private readonly Vector3 keyAppearPositionB = new Vector3(5, 0.5f, 10);
	private readonly Vector3 decryptionPositionB = new Vector3(5, 3f, 10);
	private readonly float moveAnimationTime = 2f;
	private readonly float glowEffectTime = 1f;
	private readonly float transferArcHeight = 4f;
	private readonly float transferDuration = 2.5f;

	// 追加の固定座標（animPositions の代替）
	private readonly Vector3 publicKeyShowPosition = new Vector3(4, 5, 10);
	private readonly Vector3 privateKeyShowPosition = new Vector3(6, 5, 10);
	private readonly Vector3 publicKeyDistribute1 = new Vector3(-3, 2f, 8);
	private readonly Vector3 publicKeyDistribute2 = new Vector3(0, 2.5f, 10);
	private readonly Vector3 publicKeyDistribute3 = new Vector3(3, 2f, 12);
	private readonly Vector3 publicEncryptDataPosition = new Vector3(-5, 1.5f, 10);
	private readonly Vector3 publicEncryptKeyPosition = new Vector3(-5, 3f, 10);
	private readonly Vector3 privateDecryptPosition = new Vector3(5, 1.5f, 10);
	private readonly Vector3 privateKeyHidePos = new Vector3(5, -1, 10);
	private readonly Vector3 sessionEncryptDataPosition = new Vector3(-5, 1.5f, 10);
	private readonly Vector3 sessionEncryptKeyPosition = new Vector3(-5, 3f, 10);
	private readonly Vector3 sessionKeyEncryptPosition = new Vector3(-3, 2.5f, 10);
	private readonly Vector3 sessionKeyDecryptPosition = new Vector3(5, 2.5f, 10);
	private readonly Vector3 finalDataPosition = new Vector3(5, 3, 10);

	// コンポーネント参照
	private Dictionary<string, GameObject> objectMap;
	private Dictionary<GameObject, Vector3> originalPositions;
	private Dictionary<GameObject, Material> originalMaterials;

	// 生成されたオブジェクトを管理するリスト（animPositions系の処理で参照されていた）
	private List<GameObject> generatedObjects = new List<GameObject>();
	// エリアBの鍵オブジェクトを管理する変数を追加
	private GameObject keyAtBObject;

	// 共通鍵の表示状態を管理するフラグを追加
	private bool isSymmetricKeyShownAtB = false;

	// DataCube の位置を外部からロックするためのフラグ
	private bool isDataCubeLocked = false;

	/// <summary>
	/// オブジェクトの位置設定用ヘルパー（DataCube がロックされている場合は無視）
	/// かつ DataCube の位置が変更されるたびにデバッグログを出力します。
	/// </summary>
	private void SetObjectPosition(GameObject obj, Vector3 pos)
	{
		if (obj == null) return;
		// DataCube の変更はロックを尊重
		if (isDataCubeLocked && obj == dataCube)
		{
			Debug.Log($"SetObjectPosition: DataCube位置変更要求を無視（ロック中） target={pos}");
			return;
		}

		// 現在位置と異なる場合のみ適用＆ログ
		Vector3 before = obj.transform.position;
		if (before != pos)
		{
			obj.transform.position = pos;
			if (obj == dataCube)
			{
				Debug.Log($"DataCube 位置変更: {before} -> {pos}");
			}
			else
			{
				// 必要なら他オブジェクトのログも出す（簡潔に）
				Debug.Log($"{obj.name} 位置変更: {before} -> {pos}");
			}
		}
	}

	/// <summary>
	/// DataCube の位置をロック/アンロックする。ロック時は任意で位置を設定する。
	/// </summary>
	public void LockDataCubePosition(bool locked, Vector3? lockedPosition = null)
	{
		isDataCubeLocked = locked;
		if (locked && lockedPosition.HasValue && dataCube != null)
		{
			// ロック時に位置を強制設定（ログを残す）
			Vector3 before = dataCube.transform.position;
			dataCube.transform.position = lockedPosition.Value;
			Debug.Log($"LockDataCubePosition: ロックして位置を強制設定 {before} -> {lockedPosition.Value}");
		}
	}

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
			publicKey.transform.position = publicKeyShowPosition; // 固定値に置き換え
			Debug.Log($"公開鍵を非表示に設定し、位置を {publicKeyShowPosition} に配置");
		}
		else
		{
			Debug.LogWarning("公開鍵がnullです");
		}

		if (privateKey != null)
		{
			privateKey.SetActive(false);
			privateKey.transform.position = privateKeyShowPosition; // 固定値に置き換え
			Debug.Log($"秘密鍵を非表示に設定し、位置を {privateKeyShowPosition} に配置");
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
					keyToShow.transform.position = publicKeyShowPosition;
					Debug.Log($"公開鍵を位置 {publicKeyShowPosition} に配置");
					break;

				case "private":
				case "秘密鍵":
					keyToShow.transform.position = privateKeyShowPosition;
					Debug.Log($"秘密鍵を位置 {privateKeyShowPosition} に配置");
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
			yield return StartCoroutine(GlowEffect(symmetricKey, Color.yellow));
		}
	}

	private IEnumerator EncryptDataAnimation()
	{
		// データキューブと共通鍵を設定された位置に移動
		if (dataCube != null && symmetricKey != null)
		{
			yield return StartCoroutine(MoveObject(dataCube, encryptionPositionA, moveAnimationTime / 2)); // 固定値に置き換え
			yield return StartCoroutine(MoveObject(symmetricKey, keyCreationPositionA, moveAnimationTime / 2)); // 固定値に置き換え

			// 光るエフェクト
			yield return StartCoroutine(GlowEffect(dataCube, Color.cyan));
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

	// TransferKeySecure: animPositions.secureTransferPosition / animPositions.arcHeight -> 固定値 / transferArcHeight
	private IEnumerator TransferKeySecure()
	{
		if (symmetricKey != null)
		{
			// 鍵を安全な経路で転送 (secureTransferPosition を固定値に置換)
			yield return StartCoroutine(MoveObjectArc(symmetricKey, new Vector3(5f, 1.5f, 10f), moveAnimationTime, transferArcHeight));
		}
	}

	private IEnumerator DecryptDataAnimation()
	{
		if (encryptedDataCube != null && symmetricKey != null && dataCube != null)
		{
			Vector3 position = encryptedDataCube.transform.position;

			yield return StartCoroutine(FadeOut(encryptedDataCube));
			encryptedDataCube.SetActive(false);

			SetObjectPosition(dataCube, position);
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
			StartCoroutine(MoveObject(publicKey, new Vector3(4, 5, 10), moveAnimationTime / 2)); // 固定値に置き換え
			yield return StartCoroutine(MoveObject(privateKey, new Vector3(6, 5, 10), moveAnimationTime / 2)); // 固定値に置き換え

			// 鍵ペアを同時に光らせる
			StartCoroutine(GlowEffect(publicKey, Color.blue));
			yield return StartCoroutine(GlowEffect(privateKey, Color.red));
		}
	}

	private IEnumerator EncryptWithPublicKey()
	{
		if (dataCube != null && publicKey != null)
		{
			yield return StartCoroutine(MoveObject(dataCube, publicEncryptDataPosition, moveAnimationTime / 2));
			yield return StartCoroutine(MoveObject(publicKey, publicEncryptKeyPosition, moveAnimationTime / 2));

			yield return StartCoroutine(GlowEffect(dataCube, Color.cyan));
			yield return StartCoroutine(TransformToEncrypted());
		}
	}

	private IEnumerator TransferPublicKey()
	{
		if (publicKey != null)
		{
			// 公開鍵を設定された3箇所に配布
			Vector3[] distributePositions = {
				publicKeyDistribute1,
				publicKeyDistribute2,
				publicKeyDistribute3
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
			yield return StartCoroutine(MoveObject(privateKey, privateDecryptPosition, moveAnimationTime));
			yield return StartCoroutine(GlowEffect(encryptedDataCube, Color.yellow));

			Vector3 position = encryptedDataCube.transform.position;
			yield return StartCoroutine(FadeOut(encryptedDataCube));
			encryptedDataCube.SetActive(false);

			SetObjectPosition(dataCube, position);
			dataCube.SetActive(true);
			yield return StartCoroutine(FadeIn(dataCube));
		}
	}

	private IEnumerator SecurePrivateKey()
	{
		if (privateKey != null)
		{
			// 秘密鍵を設定された隠蔽位置に移動
			yield return StartCoroutine(MoveObject(privateKey, privateKeyHidePos, moveAnimationTime));

			// 透明化
			yield return StartCoroutine(FadeOut(privateKey));
		}
	}

	// === ハイブリッド暗号の演出 ===
	private IEnumerator ShowSessionKey()
	{
		if (sessionKey != null)
		{
			yield return StartCoroutine(GlowEffect(sessionKey, Color.green));
		}
	}

	private IEnumerator EncryptWithSessionKey()
	{
		if (dataCube != null && sessionKey != null)
		{
			yield return StartCoroutine(MoveObject(dataCube, sessionEncryptDataPosition, moveAnimationTime / 2));
			yield return StartCoroutine(MoveObject(sessionKey, sessionEncryptKeyPosition, moveAnimationTime / 2));

			yield return StartCoroutine(GlowEffect(dataCube, Color.cyan));
			yield return StartCoroutine(TransformToEncrypted());
		}
	}

	private IEnumerator EncryptSessionKey()
	{
		if (sessionKey != null && publicKey != null)
		{
			yield return StartCoroutine(MoveObject(sessionKey, sessionKeyEncryptPosition, moveAnimationTime / 2));
			yield return StartCoroutine(MoveObject(publicKey, sessionKeyEncryptPosition + Vector3.up, moveAnimationTime / 2));

			yield return StartCoroutine(GlowEffect(sessionKey, Color.green));
		}
	}

	private IEnumerator DecryptSequence()
	{
		// 1. セッション鍵復号
		if (sessionKey != null && privateKey != null)
		{
			yield return StartCoroutine(MoveObject(privateKey, sessionKeyDecryptPosition + Vector3.up, moveAnimationTime / 2));
			yield return StartCoroutine(GlowEffect(sessionKey, Color.green));
		}

		// 2. データ復号
		if (encryptedDataCube != null && sessionKey != null)
		{
			yield return StartCoroutine(MoveObject(sessionKey, finalDataPosition + Vector3.up, moveAnimationTime / 2));
			yield return StartCoroutine(DecryptDataAnimation());
		}
	}

	private IEnumerator ShowAdvantages()
	{
		// すべてのオブジェクトを一斉に光らせて利点を表現
		if (dataCube != null) StartCoroutine(GlowEffect(dataCube, Color.cyan));
		if (sessionKey != null) StartCoroutine(GlowEffect(sessionKey, Color.green));
		if (publicKey != null) StartCoroutine(GlowEffect(publicKey, Color.blue));
		if (privateKey != null) StartCoroutine(GlowEffect(privateKey, Color.red));

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
			symmetricKey.transform.position = keyCreationPositionA;

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
			yield return StartCoroutine(GlowEffect(dataCube, Color.cyan));
			yield return StartCoroutine(TransformToEncrypted());
		}
	}

	private IEnumerator TransferEncryptedDataAtoB()
	{
		if (encryptedDataCube != null)
		{
			yield return StartCoroutine(MoveObjectArc(encryptedDataCube, areaBPosition, transferDuration, transferArcHeight));
		}
	}

	private IEnumerator ShowSymmetricKeyAtB()
	{
		Debug.Log("ShowSymmetricKeyAtB開始");

		if (isSymmetricKeyShownAtB)
		{
			Debug.Log("共通鍵は既にエリアBに表示済みのため、演出をスキップします");
			yield break;
		}

		if (symmetricKey != null)
		{
			symmetricKey.SetActive(true);
			Debug.Log("共通鍵を表示状態に設定");

			Vector3 startPosition = new Vector3(areaBPosition.x + 1f, 0.5f, areaBPosition.z - 1f);
			symmetricKey.transform.position = startPosition;
			Debug.Log($"共通鍵を開始位置に配置: {startPosition}");

			yield return new WaitForSeconds(0.5f);

			Vector3 targetPosition = new Vector3(areaBPosition.x + 1f, areaBPosition.y + 1.5f, areaBPosition.z - 1f);
			Debug.Log($"共通鍵を目標位置に移動開始: {targetPosition}");

			yield return StartCoroutine(MoveObject(symmetricKey, targetPosition, moveAnimationTime / 2));

			yield return StartCoroutine(GlowEffect(symmetricKey, Color.yellow));
			isSymmetricKeyShownAtB = true;
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
			Vector3 keyPosition = areaBPosition + Vector3.up * 1f;
			if (Vector3.Distance(symmetricKey.transform.position, keyPosition) > 0.5f)
			{
				yield return StartCoroutine(MoveObject(symmetricKey, keyPosition, moveAnimationTime / 2));
			}

			StartCoroutine(GlowEffect(symmetricKey, Color.yellow));
			yield return StartCoroutine(GlowEffect(encryptedDataCube, Color.yellow));

			// 暗号キューブの現在位置を記録
			Vector3 currentPosition = encryptedDataCube.transform.position;

			// 暗号キューブをフェードアウト
			yield return StartCoroutine(FadeOut(encryptedDataCube));
			encryptedDataCube.SetActive(false);

			// データキューブを同じ位置にフェードイン
			SetObjectPosition(dataCube, currentPosition);
			dataCube.SetActive(true);
			yield return StartCoroutine(FadeIn(dataCube));
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
			yield return StartCoroutine(MoveObject(publicKey, areaAPosition, moveAnimationTime / 2));
			yield return StartCoroutine(MoveObject(privateKey, areaAPosition + Vector3.left, moveAnimationTime / 2));

			StartCoroutine(GlowEffect(publicKey, Color.blue));
			yield return StartCoroutine(GlowEffect(privateKey, Color.red));
		}
	}

	private IEnumerator TransferPublicKeyAtoB()
	{
		if (publicKey != null)
		{
			yield return StartCoroutine(MoveObjectArc(publicKey, areaBPosition, transferDuration, transferArcHeight));
		}
	}

	private IEnumerator EncryptWithPublicKeyAtA()
	{
		if (dataCube != null)
		{
			// エリアAでデータを暗号化（公開鍵は既にエリアBにあるので、暗号化エフェクトのみ）
			yield return StartCoroutine(GlowEffect(dataCube, Color.cyan));
			yield return StartCoroutine(TransformToEncrypted());
		}
	}

	private IEnumerator TransferEncryptedDataOnlyAtoB()
	{
		if (encryptedDataCube != null)
		{
			yield return StartCoroutine(MoveObjectArc(encryptedDataCube, areaBPosition, transferDuration, transferArcHeight));
		}
	}

	private IEnumerator DecryptWithPrivateKeyAtB()
	{
		if (encryptedDataCube != null && privateKey != null && dataCube != null)
		{
			// 秘密鍵をエリアBの復号位置に移動
			yield return StartCoroutine(MoveObject(privateKey, privateDecryptPosition, moveAnimationTime / 2));
			yield return StartCoroutine(GlowEffect(encryptedDataCube, Color.yellow));

			// 復号化
			Vector3 position = encryptedDataCube.transform.position;
			yield return StartCoroutine(FadeOut(encryptedDataCube));
			encryptedDataCube.SetActive(false);

			SetObjectPosition(dataCube, position);
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
			publicKey.transform.position = publicKeyShowPosition;
			privateKey.transform.position = privateKeyShowPosition;

			// キーオブジェクトを表示状態にする
			publicKey.SetActive(true);
			privateKey.SetActive(true);

			Debug.Log($"公開鍵を位置 {publicKeyShowPosition} に配置");
			Debug.Log($"秘密鍵を位置 {privateKeyShowPosition} に配置");

			// エフェクトを実行
			StartCoroutine(GlowEffect(privateKey, Color.red));
			yield return new WaitForSeconds(0.5f);
			StartCoroutine(GlowEffect(publicKey, Color.blue));
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
		Vector3 areaBPosition = this.areaBPosition;

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
		Vector3 targetPos = areaAPosition + Vector3.up * 2f;
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
			publicKey.transform.position = publicKeyShowPosition;
			privateKey.transform.position = privateKeyShowPosition;

			Debug.Log($"公開鍵暗号：公開鍵を位置 {publicKeyShowPosition} に配置");
			Debug.Log($"公開鍵暗号：秘密鍵を位置 {privateKeyShowPosition} に配置");

			// 鍵生成エフェクト（時間差をつけて見やすく）
			yield return StartCoroutine(GlowEffect(privateKey, Color.red));
			yield return new WaitForSeconds(0.3f);
			yield return StartCoroutine(GlowEffect(publicKey, Color.blue));

			// 鍵生成完了のエフェクト
			if (keyGenerationEffect != null)
			{
				Instantiate(keyGenerationEffect, areaBPosition, Quaternion.identity);
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
			Vector3 targetPosition = new Vector3(-5, 4, 10); // 固定値に置き換え
			Debug.Log($"公開鍵を{targetPosition}に移動開始");

			// 滑らかな弧を描いて移動
			yield return StartCoroutine(MoveObjectArc(publicKey, targetPosition,
				moveAnimationTime, transferArcHeight));

			// 移動完了エフェクト
			yield return StartCoroutine(GlowEffect(publicKey, Color.blue));

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
			Vector3 areaAPosition = this.areaAPosition;

			// データキューブをエリアAに移動（必要に応じて）
			if (Vector3.Distance(dataCube.transform.position, areaAPosition) > 1f)
			{
				yield return StartCoroutine(MoveObject(dataCube, areaAPosition, moveAnimationTime / 2));
			}

			// 暗号化エフェクト
			yield return StartCoroutine(GlowEffect(dataCube, Color.cyan));

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
			Vector3 targetPosition = areaBPosition;
			Debug.Log($"暗号キューブを{targetPosition}に移動開始");

			// 滑らかな弧を描いて移動
			yield return StartCoroutine(MoveObjectArc(encryptedDataCube, targetPosition,
				moveAnimationTime, transferArcHeight));

			// 移動完了エフェクト
			yield return StartCoroutine(GlowEffect(encryptedDataCube, Color.yellow));

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
			Vector3 areaBPosition = this.areaBPosition;
			
			// 秘密鍵を復号位置に移動（必要に応じて）
			Vector3 privateKeyPosition = areaBPosition + Vector3.up * 1f;
			if (Vector3.Distance(privateKey.transform.position, privateKeyPosition) > 0.5f)
			{
				yield return StartCoroutine(MoveObject(privateKey, privateKeyPosition, moveAnimationTime / 2));
			}
			
			// 復号エフェクト
			StartCoroutine(GlowEffect(privateKey, Color.red));
			yield return StartCoroutine(GlowEffect(encryptedDataCube, Color.yellow));
			
			// 暗号キューブの現在位置を記録
			Vector3 currentPosition = encryptedDataCube.transform.position;
			
			// 暗号キューブをフェードアウト
			yield return StartCoroutine(FadeOut(encryptedDataCube));
			encryptedDataCube.SetActive(false);
			
			// データキューブを同じ位置にフェードイン
			SetObjectPosition(dataCube, currentPosition);
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
			publicKey.transform.position = publicKeyShowPosition;
			privateKey.transform.position = privateKeyShowPosition;

			Debug.Log($"ハイブリッド暗号：公開鍵を位置 {publicKeyShowPosition} に配置");
			Debug.Log($"ハイブリッド暗号：秘密鍵を位置 {privateKeyShowPosition} に配置");

			// 鍵生成エフェクト（秘密鍵が先、公開鍵が後）
			yield return StartCoroutine(GlowEffect(privateKey, Color.red));
			yield return new WaitForSeconds(0.5f);
			yield return StartCoroutine(GlowEffect(publicKey, Color.blue));

			// 鍵生成完了のエフェクト
			if (keyGenerationEffect != null)
			{
				Instantiate(keyGenerationEffect, areaBPosition, Quaternion.identity);
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
			Vector3 targetPosition = areaAPosition + Vector3.up * 2f; // 固定値に置き換え

			// 滑らかな弧を描いて移動（2秒かけて移動）
			yield return StartCoroutine(MoveObjectArc(publicKey, targetPosition,
				2f, transferArcHeight));

			// 移動完了エフェクト
			yield return StartCoroutine(GlowEffect(publicKey, Color.blue));

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
			Vector3 keyPosition = areaAPosition + Vector3.up * 1f;
			symmetricKey.transform.position = keyPosition;

			// 共通鍵生成エフェクト
			yield return StartCoroutine(GlowEffect(symmetricKey, Color.yellow));

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
			Vector3 encryptPosition = areaAPosition;
			if (Vector3.Distance(dataCube.transform.position, encryptPosition) > 1f)
			{
				yield return StartCoroutine(MoveObject(dataCube, encryptPosition, moveAnimationTime / 2));
			}

			// 共通鍵を暗号化位置近くに移動
			Vector3 keyPosition = encryptPosition + Vector3.up * 1f;
			yield return StartCoroutine(MoveObject(symmetricKey, keyPosition, moveAnimationTime / 2));

			// 暗号化エフェクト
			StartCoroutine(GlowEffect(symmetricKey, Color.yellow));
			yield return StartCoroutine(GlowEffect(dataCube, Color.cyan));

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
			Vector3 encryptPosition = areaAPosition + Vector3.up * 2f;
			Vector3 publicKeyTargetPosition = new Vector3(-3.5f, 5f, 10f); // 指定された移動先

			yield return StartCoroutine(MoveObject(symmetricKey, encryptPosition, moveAnimationTime / 2));
			yield return StartCoroutine(MoveObject(publicKey, publicKeyTargetPosition, moveAnimationTime / 2));

			Debug.Log($"ハイブリッド暗号5問目：公開鍵を位置 {publicKeyTargetPosition} に移動しました");

			// 公開鍵暗号化エフェクト
			StartCoroutine(GlowEffect(publicKey, Color.blue));
			yield return StartCoroutine(GlowEffect(symmetricKey, Color.yellow));

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

		if (sessionKey != null)
		{
			Debug.Log("セッション鍵をエリアBに転送開始");
			StartCoroutine(MoveObjectArc(sessionKey, areaBPosition + Vector3.up * 2f, 2f, transferArcHeight)); // 固定値に置き換え
		}

		yield return new WaitForSeconds(0.5f);

		if (encryptedDataCube != null && encryptedDataCube.activeInHierarchy)
		{
			Debug.Log("暗号化キューブをエリアBに転送開始");
			yield return StartCoroutine(MoveObjectArc(encryptedDataCube, areaBPosition, 2f, transferArcHeight)); // 固定値に置き換え
		}

		// 転送完了エフェクト
		if (transferEffect != null)
		{
			Instantiate(transferEffect, areaBPosition, Quaternion.identity);
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
			Vector3 decryptPosition = areaBPosition + Vector3.up * 2f;
			yield return StartCoroutine(MoveObject(privateKey, decryptPosition + Vector3.left, moveAnimationTime / 2));

			// 復号エフェクト
			StartCoroutine(GlowEffect(privateKey, Color.red));
			yield return StartCoroutine(GlowEffect(sessionKey, Color.green));

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
			yield return StartCoroutine(GlowEffect(symmetricKey, Color.yellow));

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
			Vector3 decryptPosition = areaBPosition + Vector3.up * 1f;
			yield return StartCoroutine(MoveObject(sessionKey, decryptPosition, moveAnimationTime / 2));

			// データ復号エフェクト
			StartCoroutine(GlowEffect(sessionKey, Color.green));
			yield return StartCoroutine(GlowEffect(encryptedDataCube, Color.yellow));

			// 暗号キューブの現在位置を記録
			Vector3 currentPosition = encryptedDataCube.transform.position;

			// 暗号キューブをフェードアウト
			yield return StartCoroutine(FadeOut(encryptedDataCube));
			encryptedDataCube.SetActive(false);

			// データキューブを同じ位置にフェードイン
			SetObjectPosition(dataCube, currentPosition);
			dataCube.SetActive(true);
			yield return StartCoroutine(FadeIn(dataCube));

			// 最終完了エフェクト - すべてのオブジェクトを光らせる
			if (dataCube != null) StartCoroutine(GlowEffect(dataCube, Color.cyan));
			if (sessionKey != null) StartCoroutine(GlowEffect(sessionKey, Color.green));
			if (publicKey != null) StartCoroutine(GlowEffect(publicKey, Color.blue));
			if (privateKey != null) StartCoroutine(GlowEffect(privateKey, Color.red));

			Debug.Log("ハイブリッド暗号8問目：全プロセス完了");
		}
		else
		{
			Debug.LogWarning("復号に必要なオブジェクトが見つかりません");
		}

		yield return new WaitForSeconds(2f);
	}

	// --- 3Dアニメーション用の基本メソッド ---
	private IEnumerator GlowEffect(GameObject target, Color glowColor, float duration = 1f)
	{
		if (target == null) yield break;
		var renderer = target.GetComponent<Renderer>();
		if (renderer == null) yield break;
		Material originalMaterial = renderer.material;
		if (glowMaterial != null)
		{
			renderer.material = glowMaterial;
		}
		else
		{
			Material tempMaterial = new Material(originalMaterial);
			tempMaterial.color = glowColor * 2f;
			renderer.material = tempMaterial;
		}
		yield return new WaitForSeconds(glowEffectTime);
		renderer.material = originalMaterial;
	}

	/// <summary>
	/// オブジェクトを指定位置に移動
	/// </summary>
	private IEnumerator MoveObject(GameObject obj, Vector3 targetPosition, float duration)
	{
		if (obj == null) yield break;
		Vector3 startPosition = obj.transform.position;
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
			Vector3 current = Vector3.Lerp(startPosition, targetPosition, t);
			SetObjectPosition(obj, current);
			yield return null;
		}
		SetObjectPosition(obj, targetPosition);
	}

	// --- MoveObjectArc: 同様に SetObjectPosition を使用 ---
	private IEnumerator MoveObjectArc(GameObject obj, Vector3 targetPosition, float duration, float arcHeight)
	{
		if (obj == null) yield break;
		Vector3 startPosition = obj.transform.position;
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / duration;
			Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, t);
			currentPosition.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
			SetObjectPosition(obj, currentPosition);
			yield return null;
		}
		SetObjectPosition(obj, targetPosition);
	}

	/// <summary>
	/// オブジェクトをフェードアウト
	/// </summary>
	private IEnumerator FadeOut(GameObject obj)
	{
		if (obj == null) yield break;
		Renderer renderer = obj.GetComponent<Renderer>();
		if (renderer == null) yield break;
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
		Color finalColor = originalColor;
		finalColor.a = 0f;
		material.color = finalColor;
	}

	/// <summary>
	/// オブジェクトをフェードイン
	/// </summary>
	private IEnumerator FadeIn(GameObject obj)
	{
		if (obj == null) yield break;
		Renderer renderer = obj.GetComponent<Renderer>();
		if (renderer == null) yield break;
		Material material = renderer.material;
		Color originalColor = material.color;
		float fadeDuration = 0.5f;
		float elapsedTime = 0f;
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
		material.color = originalColor;
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
		Vector3 originalScale = keyObject.transform.localScale;
		Vector3 targetScale = originalScale * 1.2f;
		float scaleTime = 0.5f;
		float elapsedTime = 0f;
		while (elapsedTime < scaleTime)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / scaleTime;
			keyObject.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
			yield return null;
		}

		// 2. 光るエフェクト
		yield return StartCoroutine(GlowEffect(keyObject, Color.yellow));

		// 3. 元のスケールに戻す
		elapsedTime = 0f;
		while (elapsedTime < scaleTime)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / scaleTime;
			keyObject.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
			yield return null;
		}
		keyObject.transform.localScale = originalScale;

		Debug.Log($"ShowKeyWithEffect: {keyObject.name}のエフェクト完了");
	}

	// === デバッグ用メソッド ===
	private void OnDrawGizmosSelected()
	{
		// エリアAとエリアBの位置を表示するコードを削除
	}

	/// <summary>
	/// 共通鍵暗号方式の問題終了後に鍵・データキューブをリセット
	/// </summary>
	public void ResetAfterSymmetricKeyQuestions()
	{


		// SymmetricKeyを(-5,5,10)へ移動し非表示
		if (symmetricKey != null)
		{
			symmetricKey.transform.position = new Vector3(-5f, 5f, 10f);
			symmetricKey.SetActive(false);
			Debug.Log("SymmetricKeyを(-5,5,10)に移動し非表示にしました");
		}
		// DataCubeを(-5,3,10)へ移動
		if (dataCube != null)
		{
			SetObjectPosition(dataCube, new Vector3(-5f, 3f, 10f));
			dataCube.SetActive(true);
			Debug.Log("DataCubeを(-5,3,10)に移動しました");
		}
	}

	/// <summary>
	/// DataCubeの位置を強制的に固定する
	/// </summary>
	public void ForceSetDataCubePosition(Vector3 position)
	{
		if (dataCube != null)
		{
			dataCube.transform.position = position;
			Debug.Log($"DataCubeの位置を強制的に{position}に設定しました");
		}
	}
}