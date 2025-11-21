using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CryptoUIManager : MonoBehaviour
{
    [Header("Visual Effects")]
    public ParticleSystem correctAnswerEffect;
    public AudioSource correctAnswerSound;
    public AudioSource incorrectAnswerSound;
    
    [Header("Button Effects")]
    public float buttonScaleEffect = 1.2f;
    public float buttonEffectDuration = 0.2f;
     [Header("Progress Animation")]
    public float progressBarAnimationSpeed = 2f;

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
    }

    public void PlayIncorrectAnswerEffects()
    {
        // サウンドエフェクト
        if (incorrectAnswerSound != null)
        {
            incorrectAnswerSound.Play();
        }
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
}