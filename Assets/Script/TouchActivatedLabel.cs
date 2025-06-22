using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.AI; // NavMeshAgentのために追加

public class TouchActivatedLabel : MonoBehaviour
{
    [Header("UI設定")]
    [Tooltip("表示するUIキャンバス")]
    public GameObject uiCanvas;
    [Tooltip("キャンバスをフェードインさせる場合はチェック")]
    public bool useFadeEffect = true;
    [Tooltip("フェードインの速度")]
    public float fadeSpeed = 1.0f;

    [Header("サウンド設定")]
    [Tooltip("再生するサウンド")]
    public AudioClip interactionSound;
    [Range(0, 1)]
    [Tooltip("サウンドの音量")]
    public float soundVolume = 0.5f;

    [Header("インタラクション設定")]
    [Tooltip("インタラクションを検知するタグ（通常は'Player'）")]
    public string targetTag = "Player";
    [Tooltip("一度だけ表示する場合はチェック")]
    public bool triggerOnce = true;

    [Header("プレイヤー制御")]
    [Tooltip("UIを表示中にプレイヤーの操作を無効にする")]
    public bool disablePlayerControl = true;
    [Tooltip("プレイヤー操作を無効にするまでの遅延時間（秒）")]
    public float playerControlDelay = 0.5f;
    
    [Header("敵の制御")]
    [Tooltip("UIを表示中に敵の動きを無効にする")]
    public bool disableEnemies = true;
    [Tooltip("無効にする敵の検出範囲（このオブジェクトを中心とした半径）")]
    public float enemyDetectionRadius = 20f;

    private bool hasTriggered = false;
    private CanvasGroup canvasGroup;
    private AudioSource audioSource;
    private GameObject playerObject;
    private StarterAssets.ThirdPersonController playerController;
    private EnemyController[] affectedEnemies = new EnemyController[0];

    private void Awake()
    {
        // キャンバスの初期設定
        if (uiCanvas != null)
        {
            // キャンバスが最初は非表示
            uiCanvas.SetActive(false);
            
            // CanvasGroupコンポーネントを取得（フェード効果用）
            canvasGroup = uiCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null && useFadeEffect)
            {
                canvasGroup = uiCanvas.AddComponent<CanvasGroup>();
            }
        }

        // AudioSourceコンポーネントを取得または追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && interactionSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤータグを持つオブジェクトと衝突したとき
        if (other.CompareTag(targetTag))
        {
            // プレイヤーオブジェクトを保存
            playerObject = other.gameObject;
            playerController = playerObject.GetComponent<StarterAssets.ThirdPersonController>();

            // 一度だけ表示する設定で、既に表示されている場合は何もしない
            if (triggerOnce && hasTriggered)
                return;

            // UIキャンバスを表示
            ShowCanvas();
            
            // サウンドを再生
            PlaySound();
            
            hasTriggered = true;
        }
    }

    private void ShowCanvas()
    {
        if (uiCanvas == null)
            return;

        uiCanvas.SetActive(true);

        // プレイヤーコントロールを遅延して無効にする
        if (disablePlayerControl && playerController != null)
        {
            StartCoroutine(DisablePlayerControlWithDelay(playerControlDelay));
        }

        // 敵の動きを無効にする
        if (disableEnemies)
        {
            // すべてのEnemyControllerを検索（シーン内の全ての敵）
            EnemyController[] allEnemies = FindObjectsOfType<EnemyController>();
            
            if (allEnemies.Length > 0)
            {
                Debug.Log(allEnemies.Length + "体の敵が見つかりました");
                affectedEnemies = allEnemies;
                
                foreach (var enemy in allEnemies)
                {
                    if (enemy != null)
                    {
                        // EnemyControllerを無効化
                        enemy.enabled = false;
                        
                        // NavMeshAgentも無効化（移動を確実に止めるため）
                        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                        if (agent != null)
                        {
                            agent.isStopped = true;
                            agent.velocity = Vector3.zero;
                        }
                        
                        Debug.Log("敵を停止: " + enemy.name);
                    }
                }
            }
            else
            {
                Debug.Log("敵が見つかりませんでした");
            }
        }

        // フェード効果を使用する場合
        if (useFadeEffect && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            StartCoroutine(FadeIn());
        }
    }

    // プレイヤーコントロールを遅延して無効にするコルーチン
    private IEnumerator DisablePlayerControlWithDelay(float delay)
    {
        // 指定された秒数待機
        yield return new WaitForSeconds(delay);
        
        // ThirdPersonControllerを無効化
        if (playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("プレイヤーコントロールを無効化しました（" + delay + "秒後）");
        }
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float alpha = 0f;
        
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            canvasGroup.alpha = alpha;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }

    private void PlaySound()
    {
        if (interactionSound != null && audioSource != null)
        {
            audioSource.clip = interactionSound;
            audioSource.volume = soundVolume;
            audioSource.Play();
        }
    }

    // UIを非表示にするメソッド（外部から呼び出し可能）
    public void HideCanvas()
    {
        if (uiCanvas == null)
            return;

        // プレイヤーコントロールを即座に再有効化
        if (disablePlayerControl && playerController != null)
        {
            playerController.enabled = true;
        }

        // 敵の動きを再有効化
        if (disableEnemies && affectedEnemies != null)
        {
            foreach (var enemy in affectedEnemies)
            {
                if (enemy != null)
                {
                    // EnemyControllerを再有効化
                    enemy.enabled = true;
                    
                    // NavMeshAgentも再有効化
                    NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                    if (agent != null)
                    {
                        agent.isStopped = false;
                    }
                }
            }
        }

        if (useFadeEffect && canvasGroup != null)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            uiCanvas.SetActive(false);
        }
    }

    private System.Collections.IEnumerator FadeOut()
    {
        float alpha = 1f;
        
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            canvasGroup.alpha = alpha;
            yield return null;
        }
        
        uiCanvas.SetActive(false);
    }
}