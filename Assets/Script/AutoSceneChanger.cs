using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // UIコンポーネントのための参照を追加
using System.Collections;

public class AutoSceneChanger : MonoBehaviour
{
    [Header("シーン切り替え設定")]
    [Tooltip("シーン切り替えまでの待機時間（秒）")]
    public float sceneChangeDelay = 5.0f;
    
    [Tooltip("切り替え先のシーン名")]
    public string targetSceneName;
    
    [Header("オプション")]
    [Tooltip("スタート時に自動的にカウント開始するか")]
    public bool autoStart = true;
    
    [Tooltip("画面をフェードアウトさせるか")]
    public bool useFadeEffect = true;
    
    [Tooltip("フェードアウトの時間（秒）")]
    public float fadeTime = 1.0f;
    
    [Tooltip("シーン切り替え前に効果音を再生するか")]
    public bool playSound = false;
    
    [Tooltip("シーン切り替え時の効果音")]
    public AudioClip transitionSound;
    
    [Range(0, 1)]
    [Tooltip("効果音の音量")]
    public float soundVolume = 0.5f;
    
    private bool isChangingScene = false;
    private AudioSource audioSource;
    
    private void Start()
    {
        // AudioSourceコンポーネントの取得または追加
        if (playSound && transitionSound != null)
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
            StartSceneChangeTimer();
        }
    }
    
    // 外部から呼び出せるタイマー開始メソッド
    public void StartSceneChangeTimer()
    {
        if (!isChangingScene && !string.IsNullOrEmpty(targetSceneName))
        {
            StartCoroutine(ChangeSceneAfterDelay());
        }
        else if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("ターゲットシーン名が設定されていません。インスペクターで設定してください。");
        }
    }
    
    // 指定秒数後にシーンを切り替えるコルーチン
    private IEnumerator ChangeSceneAfterDelay()
    {
        isChangingScene = true;
        
        // 指定秒数待機
        yield return new WaitForSeconds(sceneChangeDelay);
        
        // 効果音を再生
        if (playSound && transitionSound != null && audioSource != null)
        {
            audioSource.clip = transitionSound;
            audioSource.volume = soundVolume;
            audioSource.Play();
            
            // 効果音の再生が完了するまで待機（オプション）
            if (transitionSound.length < 2.0f) // 2秒以下の効果音の場合のみ待機
            {
                yield return new WaitForSeconds(transitionSound.length);
            }
        }
        
        // フェードエフェクトがある場合
        if (useFadeEffect)
        {
            yield return StartCoroutine(FadeOut());
        }
        
        // シーンが存在するか確認
        if (SceneExists(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("シーン「" + targetSceneName + "」が見つかりません。プロジェクトに追加し、Build Settingsに登録してください。");
            isChangingScene = false;
        }
    }
    
    // フェードアウト処理
    private IEnumerator FadeOut()
    {
        // フェード用のキャンバスを作成
        GameObject fadeCanvas = new GameObject("FadeCanvas");
        Canvas canvas = fadeCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // 最前面に表示
        
        // キャンバススケーラーを追加
        CanvasScaler scaler = fadeCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // 黒い画像を追加
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(fadeCanvas.transform, false);
        UnityEngine.UI.Image image = imageObj.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0, 0, 0, 0); // 最初は透明
        
        // 画像を画面いっぱいに
        RectTransform rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        
        // 徐々に黒くする
        float startTime = Time.time;
        while (Time.time < startTime + fadeTime)
        {
            float alpha = Mathf.Lerp(0, 1, (Time.time - startTime) / fadeTime);
            image.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        // 完全に黒くする
        image.color = Color.black;
        yield return new WaitForSeconds(0.5f); // 少し待機
    }
    
    // シーンが存在するか確認するヘルパーメソッド
    private bool SceneExists(string sceneName)
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneNameFromPath == sceneName)
            {
                return true;
            }
        }
        return false;
    }
    
    // シーン切り替えをキャンセル（外部から呼び出し可能）
    public void CancelSceneChange()
    {
        if (isChangingScene)
        {
            StopAllCoroutines();
            isChangingScene = false;
        }
    }
}