using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Clips")]
    public AudioClip bgmClip;
    public AudioClip correctClip;
    public AudioClip wrongClip;

    [Header("Volumes")]
    [Range(0f,1f)] public float bgmVolume = 0.6f;
    [Range(0f,1f)] public float sfxVolume = 1f;

    // 内部で自動作成するAudioSource
    private AudioSource bgmSource;
    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // BGM用
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;
        if (bgmClip != null) bgmSource.clip = bgmClip;

        // SFX用
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        // 2D 効果音にする（距離減衰などで消えるのを防ぐ）
        sfxSource.spatialBlend = 0f;
        // 音量は常に sfxVolume を反映
        sfxSource.volume = sfxVolume;

        // 実行時に誤って無効/ミュートになっていると再生されないため明示的に有効化
        sfxSource.enabled = true;
        sfxSource.mute = false;

        // エディタ上でのみインスペクタ非表示にする（ランタイム副作用を避ける）
        #if UNITY_EDITOR
        sfxSource.hideFlags = HideFlags.HideInInspector;
        #endif
    }

    void Start()
    {
        if (bgmSource.clip != null) bgmSource.Play();
    }

    // インスペクターで割り当てたクリップを再生
    public void PlayCorrect()
    {
        if (correctClip == null) { Debug.LogWarning("PlayCorrect: correctClip is null"); return; }
        if (sfxSource == null) { Debug.LogWarning("PlayCorrect: sfxSource is null"); return; }

        // デバッグ: 状態を出力
        Debug.Log($"PlayCorrect: playing '{correctClip.name}' length={correctClip.length:F2}s sfxVolume={sfxSource.volume:F2} enabled={sfxSource.enabled} mute={sfxSource.mute}");

        // 通常は PlayOneShot で再生
        if (sfxSource.enabled && !sfxSource.mute)
        {
            sfxSource.PlayOneShot(correctClip);
            return;
        }

        // フォールバック: PlayOneShot が使えない場合は PlayClipAtPoint で必ず鳴らす
        Debug.LogWarning("PlayCorrect: sfxSource not usable, falling back to PlayClipAtPoint");
        AudioSource.PlayClipAtPoint(correctClip, GetListenerPosition(), sfxVolume);
    }

    public void PlayWrong()
    {
        if (wrongClip == null) { Debug.LogWarning("PlayWrong: wrongClip is null"); return; }
        if (sfxSource == null) { Debug.LogWarning("PlayWrong: sfxSource is null"); return; }

        // デバッグ: 状態を出力
        Debug.Log($"PlayWrong: playing '{wrongClip.name}' length={wrongClip.length:F2}s sfxVolume={sfxSource.volume:F2} enabled={sfxSource.enabled} mute={sfxSource.mute}");

        if (sfxSource.enabled && !sfxSource.mute)
        {
            sfxSource.PlayOneShot(wrongClip);
            return;
        }

        Debug.LogWarning("PlayWrong: sfxSource not usable, falling back to PlayClipAtPoint");
        AudioSource.PlayClipAtPoint(wrongClip, GetListenerPosition(), sfxVolume);
    }

    // 任意のクリップをSFXとして再生する汎用メソッド
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) { Debug.LogWarning("PlaySFX: clip is null"); return; }
        if (sfxSource == null) { Debug.LogWarning("PlaySFX: sfxSource is null"); return; }

        Debug.Log($"PlaySFX: playing '{clip.name}' sfxVolume={sfxSource.volume:F2} enabled={sfxSource.enabled} mute={sfxSource.mute}");

        if (sfxSource.enabled && !sfxSource.mute)
        {
            sfxSource.PlayOneShot(clip);
            return;
        }

        Debug.LogWarning("PlaySFX: sfxSource not usable, falling back to PlayClipAtPoint");
        AudioSource.PlayClipAtPoint(clip, GetListenerPosition(), sfxVolume);
    }

    // 追加: 単純な文字列比較で正解/不正解を判定して効果音を鳴らす
    // 期待値 expected と実際の answer を比較して一致なら正解、そうでなければ不正解
    public bool EvaluateAnswer(string expected, string answer, bool ignoreCase = true)
    {
        if (expected == null || answer == null)
        {
            Debug.LogWarning("EvaluateAnswer: expected or answer is null");
            return false;
        }

        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        bool isCorrect = string.Equals(expected.Trim(), answer.Trim(), comparison);

        Debug.Log($"EvaluateAnswer: expected='{expected}' answer='{answer}' result={isCorrect}");
        if (isCorrect) PlayCorrect(); else PlayWrong();
        return isCorrect;
    }

    // 追加: 汎用判定を受け取る EvaluateAnswer。呼び出し側が true/false を返す判定を渡す
    public bool EvaluateAnswer(Func<bool> predicate)
    {
        if (predicate == null)
        {
            Debug.LogWarning("EvaluateAnswer(predicate): predicate is null");
            return false;
        }

        bool isCorrect;
        try
        {
            isCorrect = predicate();
        }
        catch (Exception ex)
        {
            Debug.LogError($"EvaluateAnswer(predicate): exception evaluating predicate: {ex}");
            isCorrect = false;
        }

        Debug.Log($"EvaluateAnswer(predicate): result={isCorrect}");
        if (isCorrect) PlayCorrect(); else PlayWrong();
        return isCorrect;
    }

    // BGMの音量や停止・再開を制御するヘルパー
    public void SetBgmVolume(float vol)
    {
        bgmVolume = Mathf.Clamp01(vol);
        if (bgmSource != null) bgmSource.volume = bgmVolume;
    }

    // 追加: SFX 用の音量設定メソッド
    public void SetSfxVolume(float vol)
    {
        sfxVolume = Mathf.Clamp01(vol);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    // Inspector で値を変更したときにエディタ上でも反映する（実行中も）
    void OnValidate()
    {
        // Editor/Inspector update
        if (bgmSource != null) bgmSource.volume = Mathf.Clamp01(bgmVolume);
        if (sfxSource != null) sfxSource.volume = Mathf.Clamp01(sfxVolume);
    }

    public void StopBgm() => bgmSource?.Stop();
    public void PauseBgm() => bgmSource?.Pause();
    public void ResumeBgm() { if (bgmSource != null && !bgmSource.isPlaying) bgmSource.Play(); }

    // AudioListener の位置を取るユーティリティ（存在しない場合は Vector3.zero）
    private Vector3 GetListenerPosition()
    {
        var listener = FindObjectOfType<AudioListener>();
        return listener != null ? listener.transform.position : Vector3.zero;
    }
}