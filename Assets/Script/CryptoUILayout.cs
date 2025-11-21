using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 暗号学習ゲーム用のUI配置設定スクリプト
/// </summary>
public class CryptoUILayout : MonoBehaviour
{
    [Header("UI Layout Settings")]
    [Tooltip("自動でUI配置を行うか")]
    public bool autoSetupLayout = false;  // 手動配置を優先するため無効化

    [Header("Custom Settings Override")]
    [Tooltip("エディタで設定したカスタム値を優先する")]
    public bool preserveCustomSettings = true;
    
    [Header("Custom UI Values (preserveCustomSettings=true時に使用)")]
    [Tooltip("QuestionTextのカスタムフォントサイズ")]
    public int customQuestionFontSize = 32;
    [Tooltip("ProgressTextのカスタムフォントサイズ")]
    public int customProgressFontSize = 20;
    [Tooltip("TimerTextのカスタムフォントサイズ")]
    public int customTimerFontSize = 24;

    [Header("UI Elements")]
    public Canvas mainCanvas;
    public Text timerText;
    public Text progressText;
    public Text questionText;
    public Button[] answerButtons;
    public GameObject progressPanel;
    public Slider[] progressSliders;
    public Text[] progressLabels;
    
    private void Start()
    {
        if (autoSetupLayout)
        {
            SetupUILayout();
        }
    }
    
    public void SetupUILayout()
    {
        SetupCanvas();
        SetupHeaderArea();
        SetupQuestionArea();
        SetupAnswerArea();
        SetupProgressArea();
    }
    
    private void SetupCanvas()
    {
        if (mainCanvas != null)
        {
            // Canvas Scaler設定
            CanvasScaler scaler = mainCanvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }
        }
    }
    
    private void SetupHeaderArea()
    {
        // タイマーテキスト配置
        if (timerText != null)
        {
            RectTransform timerRect = timerText.GetComponent<RectTransform>();
            
            // カスタム設定を優先しない場合のみ位置を変更
            if (!preserveCustomSettings)
            {
                timerRect.anchorMin = new Vector2(1, 1);
                timerRect.anchorMax = new Vector2(1, 1);
                timerRect.pivot = new Vector2(1, 1);
                timerRect.anchoredPosition = new Vector2(-50, -30);
                timerRect.sizeDelta = new Vector2(150, 40);
                timerText.alignment = TextAnchor.MiddleRight;
            }
            
            // フォントサイズと色は設定に応じて適用
            timerText.fontSize = preserveCustomSettings ? customTimerFontSize : 24;
            timerText.color = Color.white;
        }
        
        // 進捗テキスト配置
        if (progressText != null)
        {
            RectTransform progressRect = progressText.GetComponent<RectTransform>();
            
            // カスタム設定を優先しない場合のみ位置を変更
            if (!preserveCustomSettings)
            {
                progressRect.anchorMin = new Vector2(0.5f, 1);
                progressRect.anchorMax = new Vector2(0.5f, 1);
                progressRect.pivot = new Vector2(0.5f, 1);
                progressRect.anchoredPosition = new Vector2(0, -80);
                progressRect.sizeDelta = new Vector2(400, 30);
                progressText.alignment = TextAnchor.MiddleCenter;
            }
            
            // フォントサイズと色は設定に応じて適用
            progressText.fontSize = preserveCustomSettings ? customProgressFontSize : 20;
            progressText.color = Color.yellow;
        }
    }
    
    private void SetupQuestionArea()
    {
        if (questionText != null)
        {
            RectTransform questionRect = questionText.GetComponent<RectTransform>();
            
            // カスタム設定を優先しない場合のみ位置を変更
            if (!preserveCustomSettings)
            {
                questionRect.anchorMin = new Vector2(0.5f, 0.5f);
                questionRect.anchorMax = new Vector2(0.5f, 0.5f);
                questionRect.pivot = new Vector2(0.5f, 0.5f);
                questionRect.anchoredPosition = new Vector2(0, 150);
                questionRect.sizeDelta = new Vector2(800, 100);
                questionText.alignment = TextAnchor.MiddleCenter;
            }
            
            // フォントサイズと色は設定に応じて適用
            questionText.fontSize = preserveCustomSettings ? customQuestionFontSize : 32;
            questionText.color = Color.white;
        }
    }
    
    private void SetupAnswerArea()
    {
        if (answerButtons != null && answerButtons.Length >= 2)
        {
            // 回答ボタン1
            if (answerButtons[0] != null)
            {
                RectTransform button1Rect = answerButtons[0].GetComponent<RectTransform>();
                button1Rect.anchorMin = new Vector2(0.5f, 0.5f);
                button1Rect.anchorMax = new Vector2(0.5f, 0.5f);
                button1Rect.pivot = new Vector2(0.5f, 0.5f);
                button1Rect.anchoredPosition = new Vector2(-200, -50);
                button1Rect.sizeDelta = new Vector2(180, 60);
                
                // ボタンの見た目設定
                SetupButtonAppearance(answerButtons[0]);
            }
            
            // 回答ボタン2
            if (answerButtons[1] != null)
            {
                RectTransform button2Rect = answerButtons[1].GetComponent<RectTransform>();
                button2Rect.anchorMin = new Vector2(0.5f, 0.5f);
                button2Rect.anchorMax = new Vector2(0.5f, 0.5f);
                button2Rect.pivot = new Vector2(0.5f, 0.5f);
                button2Rect.anchoredPosition = new Vector2(200, -50);
                button2Rect.sizeDelta = new Vector2(180, 60);
                
                SetupButtonAppearance(answerButtons[1]);
            }
        }
    }
    
    private void SetupButtonAppearance(Button button)
    {
        if (button != null)
        {
            // ボタンの色設定
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.4f, 1f, 0.8f);      // 青
            colors.highlightedColor = new Color(0.2f, 1f, 0.4f, 0.9f); // 緑
            colors.pressedColor = new Color(1f, 0.8f, 0.2f, 1f);       // 金
            button.colors = colors;
            
            // テキスト設定
            Text buttonText = button.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.fontSize = 18;
                buttonText.color = Color.white;
                buttonText.alignment = TextAnchor.MiddleCenter;
            }
        }
    }
    
    private void SetupProgressArea()
    {
        if (progressPanel != null)
        {
            RectTransform panelRect = progressPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0);
            panelRect.anchorMax = new Vector2(0.5f, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.anchoredPosition = new Vector2(0, 20);
            panelRect.sizeDelta = new Vector2(900, 120);
        }
        
        // プログレススライダーとラベルの配置
        string[] cryptoNames = { "共通鍵", "公開鍵", "ハイブリッド" };
        Color[] progressColors = { Color.blue, Color.green, Color.yellow };
        
        for (int i = 0; i < progressSliders.Length && i < 3; i++)
        {
            if (progressSliders[i] != null)
            {
                // スライダー配置
                RectTransform sliderRect = progressSliders[i].GetComponent<RectTransform>();
                sliderRect.anchorMin = new Vector2(0.5f, 1);
                sliderRect.anchorMax = new Vector2(0.5f, 1);
                sliderRect.pivot = new Vector2(0.5f, 1);
                sliderRect.anchoredPosition = new Vector2(0, -30 - (i * 30));
                sliderRect.sizeDelta = new Vector2(600, 20);
                
                // スライダーの色設定
                Image fillImage = progressSliders[i].fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = progressColors[i];
                }
            }
            
            if (i < progressLabels.Length && progressLabels[i] != null)
            {
                // ラベル配置
                RectTransform labelRect = progressLabels[i].GetComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0.5f, 1);
                labelRect.anchorMax = new Vector2(0.5f, 1);
                labelRect.pivot = new Vector2(0, 0.5f);
                labelRect.anchoredPosition = new Vector2(-320, -40 - (i * 30));
                labelRect.sizeDelta = new Vector2(120, 20);
                
                progressLabels[i].text = $"{cryptoNames[i]} 0%";
                progressLabels[i].fontSize = 14;
                progressLabels[i].color = Color.white;
                progressLabels[i].alignment = TextAnchor.MiddleLeft;
            }
        }
    }

    /// <summary>
    /// エディタで設定された現在の値を取得してカスタム設定に保存
    /// </summary>
    [ContextMenu("Capture Current UI Settings")]
    public void CaptureCurrentUISettings()
    {
        if (questionText != null)
        {
            customQuestionFontSize = questionText.fontSize;
            Debug.Log($"QuestionText FontSize captured: {customQuestionFontSize}");
        }
        
        if (progressText != null)
        {
            customProgressFontSize = progressText.fontSize;
            Debug.Log($"ProgressText FontSize captured: {customProgressFontSize}");
        }
        
        if (timerText != null)
        {
            customTimerFontSize = timerText.fontSize;
            Debug.Log($"TimerText FontSize captured: {customTimerFontSize}");
        }
        
        Debug.Log("現在のUI設定をキャプチャしました。preserveCustomSettings = true にしてください。");
    }
    
    /// <summary>
    /// カスタム設定でUI要素を更新
    /// </summary>
    [ContextMenu("Apply Custom Settings")]
    public void ApplyCustomSettings()
    {
        if (preserveCustomSettings)
        {
            if (questionText != null) questionText.fontSize = customQuestionFontSize;
            if (progressText != null) progressText.fontSize = customProgressFontSize;
            if (timerText != null) timerText.fontSize = customTimerFontSize;
            
            Debug.Log("カスタム設定を適用しました。");
        }
        else
        {
            Debug.LogWarning("preserveCustomSettings が false です。true に設定してください。");
        }
    }
}