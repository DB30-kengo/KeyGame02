using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 暗号学習ゲーム用UI管理システム
/// </summary>
public class CryptoUIManager : MonoBehaviour
{
    [Header("==== スクリプト修復確認 ====")]
    [Tooltip("このフィールドが表示されていれば、スクリプトは正常に読み込まれています")]
    public bool scriptIsWorking = true;

    [Header("Visual Effects")]
    public ParticleSystem correctAnswerEffect;
    public AudioSource correctAnswerSound;
    public AudioSource incorrectAnswerSound;
    
    [Header("Result Display")]
    [Tooltip("正解・不正解を表示するテキスト")]
    public Text resultText;
    [Tooltip("進捗（例: 問題 2/3 - 公開鍵暗号方式）を表示するテキスト")]
    public Text progressText;
    [Tooltip("正解時の表示テキスト")]
    public string correctText = "正解!";
    [Tooltip("不正解時の表示テキスト")]
    public string incorrectText = "不正解...";
    [Tooltip("正解時のテキスト色")]
    public Color correctTextColor = Color.green;
    [Tooltip("不正解時のテキスト色")]
    public Color incorrectTextColor = Color.red;
    [Tooltip("結果表示の継続時間")]
    public float resultDisplayDuration = 2f;
    
    [Header("Button Effects")]
    public float buttonScaleEffect = 1.2f;
    public float buttonEffectDuration = 0.2f;
    [Header("Progress Animation")]
    public float progressBarAnimationSpeed = 2f;

    [Header("Crypto Type Animations")]
    [Tooltip("共通鍵暗号用の色")]
    public Color symmetricKeyColor = Color.blue;
    [Tooltip("公開鍵暗号用の色")]
    public Color publicKeyColor = Color.red;
    [Tooltip("ハイブリッド暗号用の色")]
    public Color hybridColor = Color.green;
    
    [Header("Animation Settings")]
    public float cryptoTypeAnimationDuration = 1.5f;
    public float keyAnimationSpeed = 3f;
    public ParticleSystem encryptionParticles;
    public ParticleSystem decryptionParticles;

    // 鍵・データキューブの初期位置保存用
    [Header("Animation Objects")]
    public Transform symmetricKeyObject;
    public Transform publicKeyObject;
    public Transform privateKeyObject;
    public Transform dataCubeObject;

    private Vector3 symmetricKeyInitialPos;
    private Quaternion symmetricKeyInitialRot;
    private Vector3 publicKeyInitialPos;
    private Quaternion publicKeyInitialRot;
    private Vector3 privateKeyInitialPos;
    private Quaternion privateKeyInitialRot;
    private Vector3 dataCubeInitialPos;
    private Quaternion dataCubeInitialRot;

    // 結果表示専用コルーチンの参照（StopAllCoroutines を避けるため）
    private Coroutine resultCoroutine;

    private void Awake()
    {
        // 初期位置を保存
        if (symmetricKeyObject != null)
        {
            symmetricKeyInitialPos = symmetricKeyObject.localPosition;
            symmetricKeyInitialRot = symmetricKeyObject.localRotation;
        }
        if (publicKeyObject != null)
        {
            publicKeyInitialPos = publicKeyObject.localPosition;
            publicKeyInitialRot = publicKeyObject.localRotation;
        }
        if (privateKeyObject != null)
        {
            privateKeyInitialPos = privateKeyObject.localPosition;
            privateKeyInitialRot = privateKeyObject.localRotation;
        }
        if (dataCubeObject != null)
        {
            dataCubeInitialPos = dataCubeObject.localPosition;
            dataCubeInitialRot = dataCubeObject.localRotation;
        }
    }

    private void Start()
    {
        Debug.Log("[CryptoUIManager] スクリプトが正常に初期化されました");
        
        // 基本的な初期化
        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// スクリプトが正常に動作しているかのテスト
    /// </summary>
    [ContextMenu("Test Script Function")]
    public void TestScriptFunction()
    {
        Debug.Log("[CryptoUIManager] スクリプトテスト実行 - 正常に動作しています");
        
        if (resultText != null)
        {
            resultText.text = "スクリプト正常動作確認";
            resultText.gameObject.SetActive(true);
        }
    }

    public void PlayCorrectAnswerEffects()
    {
        // パーティクルエフェクト
        if (correctAnswerEffect != null)
        {
            correctAnswerEffect.Play();
        }
        
        // サウンドエフェクト
        if (correctAnswerSound != null)
        {
            correctAnswerSound.Play();
        }
        
        // 正解テキスト表示（明示的に正解フラグを渡す）
        ShowResultText(correctText, correctTextColor, true);
    }

    public void PlayIncorrectAnswerEffects()
    {
        // サウンドエフェクト
        if (incorrectAnswerSound != null)
        {
            incorrectAnswerSound.Play();
        }
        
        // 不正解テキスト表示（明示的に不正解フラグを渡す）
        ShowResultText(incorrectText, incorrectTextColor, false);
    }
    
    public void AnimateButtonPress(Button button)
    {
        if (button != null)
        {
            StartCoroutine(ButtonPressAnimation(button.transform));
        }
    }
    
    private IEnumerator ButtonPressAnimation(Transform buttonTransform)
    {
        Vector3 originalScale = buttonTransform.localScale;
        Vector3 targetScale = originalScale * buttonScaleEffect;
        
        // スケールアップ
        float elapsedTime = 0f;
        while (elapsedTime < buttonEffectDuration / 2f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (buttonEffectDuration / 2f);
            buttonTransform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }
        
        // スケールダウン
        elapsedTime = 0f;
        while (elapsedTime < buttonEffectDuration / 2f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (buttonEffectDuration / 2f);
            buttonTransform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }
        
        buttonTransform.localScale = originalScale;
    }
    
    public void AnimateProgressBar(Slider progressSlider, float targetValue)
    {
        if (progressSlider != null)
        {
            StartCoroutine(ProgressBarAnimation(progressSlider, targetValue));
        }
    }
    
    private IEnumerator ProgressBarAnimation(Slider slider, float targetValue)
    {
        float startValue = slider.value;
        float elapsedTime = 0f;
        float animationDuration = Mathf.Abs(targetValue - startValue) / progressBarAnimationSpeed;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            slider.value = Mathf.Lerp(startValue, targetValue, t);
            yield return null;
        }
        
        slider.value = targetValue;
    }
    
    // 色変化アニメーション
    public void AnimateColorChange(Graphic graphic, Color targetColor, float duration = 1f)
    {
        if (graphic != null)
        {
            StartCoroutine(ColorChangeAnimation(graphic, targetColor, duration));
        }
    }
    
    private IEnumerator ColorChangeAnimation(Graphic graphic, Color targetColor, float duration)
    {
        Color startColor = graphic.color;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            graphic.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        
        graphic.color = targetColor;
    }
    
    // テキストタイプライター効果
    public void TypewriterText(Text textComponent, string fullText, float typeSpeed = 0.05f)
    {
        if (textComponent != null)
        {
            StartCoroutine(TypewriterEffect(textComponent, fullText, typeSpeed));
        }
    }
    
    private IEnumerator TypewriterEffect(Text textComponent, string fullText, float typeSpeed)
    {
        textComponent.text = "";
        
        for (int i = 0; i <= fullText.Length; i++)
        {
            textComponent.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(typeSpeed);
        }
    }
    
    // パルス効果（重要な情報を強調）
    public void PulseEffect(Transform target, float intensity = 0.2f, float speed = 2f)
    {
        if (target != null)
        {
            StartCoroutine(PulseAnimation(target, intensity, speed));
        }
    }
    
    private IEnumerator PulseAnimation(Transform target, float intensity, float speed)
    {
        Vector3 originalScale = target.localScale;
        
        while (target.gameObject.activeInHierarchy)
        {
            float scale = 1f + Mathf.Sin(Time.time * speed) * intensity;
            target.localScale = originalScale * scale;
            yield return null;
        }
        
        target.localScale = originalScale;
    }
    
    // シェイク効果（間違い選択時など）
    public void ShakeEffect(Transform target, float duration = 0.5f, float intensity = 10f)
    {
        if (target != null)
        {
            StartCoroutine(ShakeAnimation(target, duration, intensity));
        }
    }
    
    private IEnumerator ShakeAnimation(Transform target, float duration, float intensity)
    {
        Vector3 originalPosition = target.localPosition;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            
            target.localPosition = originalPosition + new Vector3(x, y, 0);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        target.localPosition = originalPosition;
    }

    /// <summary>
    /// 暗号方式に応じたアニメーションを再生
    /// </summary>
    public void PlayCryptoTypeAnimation(CryptoGameManager.CryptoType cryptoType, Transform targetElement = null)
    {
        // 暗号方式切り替え時にオブジェクト位置リセット
        ResetAnimationObjectsForCryptoType(cryptoType);

        Debug.Log($"[CryptoUIManager] 暗号方式アニメーション開始: {cryptoType}");
        
        switch (cryptoType)
        {
            case CryptoGameManager.CryptoType.SymmetricKey:
                PlaySymmetricKeyAnimation(targetElement);
                break;
            case CryptoGameManager.CryptoType.PublicKey:
                PlayPublicKeyAnimation(targetElement);
                break;
            case CryptoGameManager.CryptoType.Hybrid:
                PlayHybridAnimation(targetElement);
                break;
            default:
                Debug.LogWarning($"未対応の暗号方式: {cryptoType}");
                break;
        }
    }
    
    /// <summary>
    /// 共通鍵暗号のアニメーション
    /// </summary>
    private void PlaySymmetricKeyAnimation(Transform target)
    {
        Debug.Log("[CryptoUIManager] 共通鍵暗号アニメーション再生");
        
        // 青色でパルス効果
        if (target != null)
        {
            StartCoroutine(CryptoTypeColorPulse(target, symmetricKeyColor));
        }
        
        // 暗号化パーティクル
        if (encryptionParticles != null)
        {
            var main = encryptionParticles.main;
            main.startColor = symmetricKeyColor;
            encryptionParticles.Play();
        }
        
        // シンプルなキー交換アニメーション
        StartCoroutine(SymmetricKeyVisualization());
    }
    
    /// <summary>
    /// 公開鍵暗号のアニメーション
    /// </summary>
    private void PlayPublicKeyAnimation(Transform target)
    {
        Debug.Log("[CryptoUIManager] 公開鍵暗号アニメーション再生");
        
        // 赤色でパルス効果
        if (target != null)
        {
            StartCoroutine(CryptoTypeColorPulse(target, publicKeyColor));
        }
        
        // 復号化パーティクル
        if (decryptionParticles != null)
        {
            var main = decryptionParticles.main;
            main.startColor = publicKeyColor;
            decryptionParticles.Play();
        }
        
        // 公開鍵・秘密鍵ペアアニメーション
        StartCoroutine(PublicKeyVisualization());
    }
    
    /// <summary>
    /// ハイブリッド暗号のアニメーション
    /// </summary>
    private void PlayHybridAnimation(Transform target)
    {
        Debug.Log("[CryptoUIManager] ハイブリッド暗号アニメーション再生");
        
        // 緑色でパルス効果
        if (target != null)
        {
            StartCoroutine(CryptoTypeColorPulse(target, hybridColor));
        }
        
        // 両方のパーティクルを使用
        if (encryptionParticles != null && decryptionParticles != null)
        {
            var encMain = encryptionParticles.main;
            encMain.startColor = hybridColor;
            encryptionParticles.Play();
            
            StartCoroutine(DelayedParticlePlay(decryptionParticles, hybridColor, 1f));
        }
        
        // ハイブリッド暗号の複合アニメーション
        StartCoroutine(HybridCryptoVisualization());
    }
    
    /// <summary>
    /// 暗号方式の色でパルス効果
    /// </summary>
    private IEnumerator CryptoTypeColorPulse(Transform target, Color cryptoColor)
    {
        Graphic graphic = target.GetComponent<Graphic>();
        if (graphic == null) yield break;
        
        Color originalColor = graphic.color;
        float elapsed = 0f;
        
        while (elapsed < cryptoTypeAnimationDuration)
        {
            float intensity = Mathf.Sin(elapsed * keyAnimationSpeed) * 0.5f + 0.5f;
            graphic.color = Color.Lerp(originalColor, cryptoColor, intensity * 0.7f);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        graphic.color = originalColor;
    }
    
    /// <summary>
    /// 共通鍵暗号の視覚化
    /// </summary>
    private IEnumerator SymmetricKeyVisualization()
    {
        Debug.Log("[Animation] 共通鍵暗号: 同一の鍵で暗号化・復号化");
        yield return new WaitForSeconds(1f);
        
        // ここでUI要素のアニメーションを実装可能
        // 例：鍵のアイコンを表示・移動させる
    }
    
    /// <summary>
    /// 公開鍵暗号の視覚化
    /// </summary>
    private IEnumerator PublicKeyVisualization()
    {
        Debug.Log("[Animation] 公開鍵暗号: 公開鍵で暗号化、秘密鍵で復号化");
        yield return new WaitForSeconds(1f);
        
        // ここでキーペアのアニメーションを実装可能
    }
    
    /// <summary>
    /// ハイブリッド暗号の視覚化
    /// </summary>
    private IEnumerator HybridCryptoVisualization()
    {
        Debug.Log("[Animation] ハイブリッド暗号: 共通鍵暗号と公開鍵暗号の組み合わせ");
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("[Animation] 段階1: 共通鍵でデータを暗号化");
        yield return new WaitForSeconds(0.7f);
        
        Debug.Log("[Animation] 段階2: 公開鍵で共通鍵を暗号化");
        yield return new WaitForSeconds(0.8f);
    }
    
    /// <summary>
    /// 遅延してパーティクルを再生
    /// </summary>
    private IEnumerator DelayedParticlePlay(ParticleSystem particles, Color color, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (particles != null)
        {
            var main = particles.main;
            main.startColor = color;
            particles.Play();
        }
    }

    /// <summary>
    /// 3Dオブジェクト用のボタンプレスアニメーション
    /// </summary>
    public void Animate3DButtonPress(Transform target)
    {
        if (target != null)
        {
            StartCoroutine(ButtonPressAnimation(target));
        }
    }
    
    /// <summary>
    /// 回答選択時の視覚的フィードバック強化
    /// </summary>
    public void PlayAnswerSelectionFeedback(Transform target, bool isCorrect = true)
    {
        if (target == null) return;
        
        if (isCorrect)
        {
            // 正解時のエフェクト
            StartCoroutine(CorrectAnswerSequence(target));
        }
        else
        {
            // 不正解時のエフェクト
            StartCoroutine(IncorrectAnswerSequence(target));
        }
    }
    
    /// <summary>
    /// 正解時のアニメーション sequence
    /// </summary>
    private IEnumerator CorrectAnswerSequence(Transform target)
    {
        // 1. スケールアップエフェクト
        StartCoroutine(ButtonPressAnimation(target));
        
        // 2. 正解エフェクト再生（テキスト表示含む）
        PlayCorrectAnswerEffects();
        
        // 3. パルス効果を開始
        StartCoroutine(PulseAnimation(target, 0.2f, 4f));
        
        yield return new WaitForSeconds(1f);
        
        // パルス効果を終了するために gameObject を一時的に非アクティブ化
        target.gameObject.SetActive(false);
        yield return new WaitForEndOfFrame();
        target.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// 不正解時のアニメーション sequence
    /// </summary>
    private IEnumerator IncorrectAnswerSequence(Transform target)
    {
        // 不正解エフェクト再生（テキスト表示含む）
        PlayIncorrectAnswerEffects();
        
        // シェイク効果
        StartCoroutine(ShakeAnimation(target, 0.5f, 0.02f));
        
        yield return new WaitForSeconds(0.5f);
    }
    
    /// <summary>
    /// 正解・不正解のテキストを表示
    /// </summary>
    // isCorrect: true=正解（green等）、false=不正解（red等）
	private void ShowResultText(string text, Color color, bool isCorrect)
	{
		if (resultText == null) return;

		// 正誤フラグがある場合は明示色を優先する
		Color useColor = isCorrect ? correctTextColor : incorrectTextColor;

		// 既存の結果表示コルーチンのみ停止して差し替える（StopAllCoroutines は使わない）
		if (resultCoroutine != null)
		{
			StopCoroutine(resultCoroutine);
			resultCoroutine = null;
		}

		// 表示に入る前に即時で色・スケール・表示状態を設定して
		// 「前の（緑）表示が一瞬見える」時間を潰す
		resultText.text = text;
		resultText.gameObject.SetActive(true);
		resultText.transform.localScale = Vector3.one * 0.5f; // 初期小スケール
		resultText.color = new Color(useColor.r, useColor.g, useColor.b, 0f); // RGB を即座にセット、alpha は0

		// コルーチンでフェード／スケールを実行
		resultCoroutine = StartCoroutine(DisplayResultText(useColor));
	}

	/// <summary>
	/// 結果テキストの表示アニメーション
	/// ※ 引数を color のみに簡略化（テキストは ShowResultText 側でセット済み）
	/// </summary>
	private IEnumerator DisplayResultText(Color color)
	{
		// resultText.text は ShowResultText で既にセットしているためここでは再設定しない
		// resultText.gameObject.SetActive(true); // ここでは不要（既に有効化済み）

		// フェードイン
		float elapsed = 0f;
		float fadeInDuration = 0.3f;

		while (elapsed < fadeInDuration)
		{
			elapsed += Time.deltaTime;
			float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
			resultText.color = new Color(color.r, color.g, color.b, alpha);

			// スケールアニメーション
			float scale = Mathf.Lerp(0.5f, 1.2f, elapsed / fadeInDuration);
			resultText.transform.localScale = Vector3.one * scale;

			yield return null;
		}

		resultText.color = color;
		resultText.transform.localScale = Vector3.one;

		// 表示継続
		yield return new WaitForSeconds(resultDisplayDuration);

		// フェードアウト
		elapsed = 0f;
		float fadeOutDuration = 0.3f;

		while (elapsed < fadeOutDuration)
		{
			elapsed += Time.deltaTime;
			float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
			resultText.color = new Color(color.r, color.g, color.b, alpha);
			yield return null;
		}

		resultText.gameObject.SetActive(false);
		// 終了したので参照をクリア
		resultCoroutine = null;
	}
    
    // 暗号方式切り替え時にアニメーション用オブジェクトの位置をリセット
    public void ResetAnimationObjectsForCryptoType(CryptoGameManager.CryptoType cryptoType)
    {
        // 共通鍵暗号方式
        if (cryptoType == CryptoGameManager.CryptoType.SymmetricKey)
        {
            if (symmetricKeyObject != null)
            {
                symmetricKeyObject.localPosition = symmetricKeyInitialPos;
                symmetricKeyObject.localRotation = symmetricKeyInitialRot;
                symmetricKeyObject.gameObject.SetActive(true);
            }
            if (dataCubeObject != null)
            {
                dataCubeObject.localPosition = dataCubeInitialPos;
                dataCubeObject.localRotation = dataCubeInitialRot;
                dataCubeObject.gameObject.SetActive(true);
            }
            if (publicKeyObject != null) publicKeyObject.gameObject.SetActive(false);
            if (privateKeyObject != null) privateKeyObject.gameObject.SetActive(false);
        }
        // 公開鍵暗号方式
        else if (cryptoType == CryptoGameManager.CryptoType.PublicKey)
        {
            if (publicKeyObject != null)
            {
                publicKeyObject.localPosition = publicKeyInitialPos;
                publicKeyObject.localRotation = publicKeyInitialRot;
                publicKeyObject.gameObject.SetActive(true);
            }
            if (privateKeyObject != null)
            {
                privateKeyObject.localPosition = privateKeyInitialPos;
                privateKeyObject.localRotation = privateKeyInitialRot;
                privateKeyObject.gameObject.SetActive(true);
            }
            if (symmetricKeyObject != null) symmetricKeyObject.gameObject.SetActive(false);

            // データキューブを送信者側（エリアA）に戻す
            if (dataCubeObject != null)
            {
                dataCubeObject.position = new Vector3(-5f, 3f, 10f);
                dataCubeObject.gameObject.SetActive(true);
                Debug.Log("UIManager: DataCubeを(-5,3,10)に移動（positionで設定）");
            }

            // CryptoAnimationManager に DataCube の位置を固定するように指示（上書きされないようにロック）
            var anim = FindObjectOfType<CryptoAnimationManager>();
            if (anim != null)
            {
                anim.LockDataCubePosition(true, new Vector3(-5f, 3f, 10f));
            }
        }
        // ハイブリッド暗号方式
        else if (cryptoType == CryptoGameManager.CryptoType.Hybrid)
        {
            if (symmetricKeyObject != null)
            {
                symmetricKeyObject.localPosition = symmetricKeyInitialPos;
                symmetricKeyObject.localRotation = symmetricKeyInitialRot;
                symmetricKeyObject.gameObject.SetActive(true);
            }
            if (publicKeyObject != null)
            {
                publicKeyObject.localPosition = publicKeyInitialPos;
                publicKeyObject.localRotation = publicKeyInitialRot;
                publicKeyObject.gameObject.SetActive(true);
            }
            if (privateKeyObject != null)
            {
                privateKeyObject.localPosition = privateKeyInitialPos;
                privateKeyObject.localRotation = privateKeyInitialRot;
                privateKeyObject.gameObject.SetActive(true);
            }
            if (dataCubeObject != null)
            {
                dataCubeObject.localPosition = dataCubeInitialPos;
                dataCubeObject.localRotation = dataCubeInitialRot;
                dataCubeObject.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// ProgressText を更新する。外部から (現在, 合計, 暗号方式) を渡して表示を更新してください。
    /// 例: "問題 2/3 - 公開鍵暗号方式"
    /// </summary>
    public void SetProgress(int currentQuestion, int totalQuestions, CryptoGameManager.CryptoType cryptoType)
    {
        if (progressText == null) return;

        // 範囲チェック
        int current = Mathf.Clamp(currentQuestion, 1, Mathf.Max(1, totalQuestions));
        int total = Mathf.Max(1, totalQuestions);

        string label = GetCryptoTypeLabel(cryptoType);
        progressText.text = $"問題 {current}/{total} - {label}";
        Debug.Log($"[CryptoUIManager] ProgressText 更新: {progressText.text}");
    }

    // 暗号方式を表示用の日本語ラベルに変換
    private string GetCryptoTypeLabel(CryptoGameManager.CryptoType cryptoType)
    {
        switch (cryptoType)
        {
            case CryptoGameManager.CryptoType.SymmetricKey:
                return "共通鍵暗号方式";
            case CryptoGameManager.CryptoType.PublicKey:
                return "公開鍵暗号方式";
            case CryptoGameManager.CryptoType.Hybrid:
                return "ハイブリッド暗号方式";
            default:
                return cryptoType.ToString();
        }
    }
}