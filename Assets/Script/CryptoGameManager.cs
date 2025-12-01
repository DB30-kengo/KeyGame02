using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection; // 追加：リフレクション呼び出し用


/// <summary>
/// 暗号学習ゲームの中核管理システム（ヒント機能なし・純粋ゲーム版）
/// </summary>
public class CryptoGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float gameSetDuration = 180f; // 3分
    public int questionsPerSet = 3;
    
    [Header("暗号方式選択 - Crypto Type Selection")]
    [Tooltip("出題する暗号方式を選択してください（チェックを外すとその方式は出題されません）")]
    public bool enableSymmetricKey = true;  // 共通鍵暗号
    public bool enablePublicKey = true;     // 公開鍵暗号
    public bool enableHybrid = true;        // ハイブリッド暗号
    
    [Header("UI References")]
    public Text questionText;
    public Text progressText;
    public Text timerText;
    // 解説パネル本文（既存）
    public Text explanationText;
    // 解説パネルヘッダー（1行目に「✅ 正解！」等を中央表示するための Text）
    public Text explanationHeaderText;
    public GameObject explanationPanel;
    public Button[] answerButtons; // UIボタン用（オプション）
    public CryptoAnswerCube[] answerCubes; // 3D回答キューブ（メイン）
    public GameObject resultPanel;
    public Text resultText;
    
    [Header("Score UI - スコア表示")]
    [Tooltip("現在のスコアを表示するテキスト")]
    public Text currentScoreText;
    [Tooltip("最終スコア表示用のテキスト")]
    public Text finalScoreText;
    [Tooltip("最終結果パネル")]
    public GameObject finalResultPanel;
    
    [Header("Progress UI")]
    public Slider[] progressSliders; // 3つの暗号方式用
    public Text[] progressLabels;

    [Header("3D Animation System")]
    public CryptoAnimationManager animationManager;
    
    [Header("UI Animation System")]
    [Tooltip("UI アニメーション管理")]
    public CryptoUIManager uiManager;
    
    [Header("Player Management")]
    [Tooltip("プレイヤーオブジェクト（正解時に位置をリセット）")]
    public Transform player;
    
    [Tooltip("プレイヤー入力制御コンポーネント")]
    public StarterAssets.StarterAssetsInputs playerInput;
    
    [Space(5)]
    [Tooltip("プレイヤーの復帰位置設定")]
    public PlayerResetSettings resetSettings;
    
    [System.Serializable]
    public class PlayerResetSettings
    {
        [Header("復帰位置の種類")]
        [Tooltip("復帰位置の設定方法を選択")]
        public ResetPositionType resetType = ResetPositionType.Custom;
        
        [Header("カスタム位置設定")]
        [Tooltip("カスタム復帰位置（Reset Type が Custom の場合に使用）")]
        public Vector3 customPosition = new Vector3(0, 3, 5);
        
        [Header("プリセット位置")]
        [Tooltip("プリセット復帰位置（Reset Type が Preset の場合に使用）")]
        public PresetPosition presetPosition = PresetPosition.Center;
        
        [Header("Transform参照")]
        [Tooltip("Transform参照復帰位置（Reset Type が Transform の場合に使用）")]
        public Transform referenceTransform;
        
        [Header("詳細設定")]
        [Tooltip("復帰時にプレイヤーの向きもリセットするか")]
        public bool resetRotation = false;
        
        [Tooltip("復帰後のプレイヤーの向き（Reset Rotation が true の場合）")]
        public Vector3 resetRotationEuler = Vector3.zero;
        
        [Header("高さ調整設定")]
        [Tooltip("地面検出を使用するか（無効にすると設定位置をそのまま使用）")]
        public bool useGroundDetection = true;
        
        [Tooltip("地面検出の最大距離")]
        [Range(1f, 50f)]
        public float groundDetectionDistance = 20f;
        
        [Tooltip("復帰位置の高さオフセット（地面からの高さ）")]
        [Range(0f, 10f)]
        public float heightOffset = 1.5f;
        
        [Tooltip("高さオフセットを強制適用（地面検出に関係なく常に適用）")]
        public bool forceHeightOffset = false;
    }
    
    public enum ResetPositionType
    {
        Custom,     // カスタム位置
        Preset,     // プリセット位置
        Transform   // Transform参照
    }
    
    public enum PresetPosition
    {
        Center,         // 中央 (0, 3, 5)
        FarCenter,      // 遠い中央 (0, 3, 10)
        LeftSide,       // 左側 (-5, 3, 5)
        RightSide,      // 右側 (5, 3, 5)
        HighCenter,     // 高い中央 (0, 8, 5)
        StartPosition   // 開始位置 (0, 1, 0)
    }
    
    // ゲーム状態
    private CryptoType[] currentGameSet;
    private int currentQuestionIndex = 0;
    private int currentStepIndex = 0;
    private float gameTimer;
    private bool isGameActive = false;
    
    // スコア管理
    private int correctAnswers = 0;
    private int totalQuestions = 0;
    private int currentScore = 0;
    private int pointsPerCorrect = 10;      // 正解時の獲得ポイント
    private int pointsPerIncorrect = -5;    // 不正解時の減点
    
    // 進捗管理
    private ProgressTracker progressTracker;
    
    /// <summary>
    /// 現在のゲームタイプを取得（外部アクセス用）
    /// </summary>
    public CryptoType? CurrentCryptoType
    {
        get
        {
            if (currentGameSet != null && currentQuestionIndex >= 0 && currentQuestionIndex < currentGameSet.Length)
            {
                return currentGameSet[currentQuestionIndex];
            }
            return null;
        }
    }
    
    /// <summary>
    /// ゲームが進行中かどうか
    /// </summary>
    public bool IsGameActive => isGameActive;
    
    public enum CryptoType
    {
        SymmetricKey,    // 共通鍵暗号
        PublicKey,       // 公開鍵暗号
        Hybrid           // ハイブリッド暗号
    }
    
    [Header("Debug Functions")]
    [Space(10)]
    public bool enableDebugFunctions = false;
    
    [ContextMenu("Test Add Score (Correct Answer)")]
    public void TestAddCorrectScore()
    {
        if (!enableDebugFunctions) return;
        AddCorrectAnswerScore();
        Debug.Log("デバッグ: 正解スコア追加テスト実行");
    }
    
    [ContextMenu("Test Add Score (Incorrect Answer)")]
    public void TestAddIncorrectScore()
    {
        if (!enableDebugFunctions) return;
        AddIncorrectAnswerScore();
        Debug.Log("デバッグ: 不正解スコア追加テスト実行");
    }
    
    [ContextMenu("Test Show Final Score")]
    public void TestShowFinalScore()
    {
        if (!enableDebugFunctions) return;
        
        // テスト用データ設定
        totalQuestions = 10;
        correctAnswers = 7;
        currentScore = 65; // 7*10 - 3*5 = 65点の例
        
        ShowFinalScore();
        Debug.Log("デバッグ: 最終スコア表示テスト実行 - Enterキー対応版");
    }
    
    [ContextMenu("Test Reset Score")]
    public void TestResetScore()
    {
        if (!enableDebugFunctions) return;
        currentScore = 0;
        correctAnswers = 0;
        totalQuestions = 0;
        UpdateScoreDisplay();
        Debug.Log("デバッグ: スコアリセットテスト実行");
    }

    /// <summary>
    /// 正解時のスコア加算
    /// </summary>
    private void AddCorrectAnswerScore()
    {
        currentScore += pointsPerCorrect;
        correctAnswers++;
        totalQuestions++;
        
        UpdateScoreDisplay();
        
        Debug.Log("正解スコア加算: +" + pointsPerCorrect + "点 (総合: " + currentScore + "点)");
    }
    
    /// <summary>
    /// 不正解時のスコア減点
    /// </summary>
    private void AddIncorrectAnswerScore()
    {
        currentScore = Mathf.Max(0, currentScore + pointsPerIncorrect); // マイナスにならないように
        totalQuestions++;
        
        UpdateScoreDisplay();
        
        Debug.Log("不正解スコア減点: " + pointsPerIncorrect + "点 (総合: " + currentScore + "点)");
    }
    
    /// <summary>
    /// スコア表示の更新
    /// </summary>
    private void UpdateScoreDisplay()
    {
        if (currentScoreText != null)
        {
            currentScoreText.text = "スコア: " + currentScore;
        }
    }
    
    /// <summary>
    /// 最終スコアの表示とEnterキー待機
    /// </summary>
    private void ShowFinalScore()
    {
        // プレイヤー入力を無効化
        DisablePlayerInput();
        
        if (finalResultPanel != null)
        {
            finalResultPanel.SetActive(true);
        }
        
        // 最終スコア計算
        float accuracy = totalQuestions > 0 ? (float)correctAnswers / totalQuestions * 100 : 0;
        string grade = GetScoreGrade(accuracy);
        
        string finalMessage = "ゲーム終了！\n\n" +
                             "正解数: " + correctAnswers + " / " + totalQuestions + "\n" +
                             "正解率: " + accuracy.ToString("F1") + "%\n" +
                             "最終スコア: " + currentScore + "点\n" +
                             "評価: " + grade + "\n\n" +
                             "Enterキーでゲームを再開";
        
        if (finalScoreText != null)
        {
            finalScoreText.text = finalMessage;
        }
        
        // 結果表示パネルもアクティブに
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            if (resultText != null)
            {
                resultText.text = finalMessage;
            }
        }
        
        Debug.Log("最終結果表示: " + correctAnswers + "/" + totalQuestions + " (正解率: " + accuracy.ToString("F1") + "%), スコア: " + currentScore);
        
        // Enterキー待機の開始
        StartCoroutine(WaitForEnterToRestart());
    }
    
    /// <summary>
    /// スコアに基づく評価取得
    /// </summary>
    private string GetScoreGrade(float accuracy)
    {
        if (accuracy >= 95) return "S (完璧!)";
        if (accuracy >= 80) return "A (優秀)";
        if (accuracy >= 65) return "B (良好)";
        if (accuracy >= 50) return "C (合格)";
        return "D (要復習)";
    }
    
    /// <summary>
    /// Enterキー待機とゲーム再開
    /// </summary>
    private IEnumerator WaitForEnterToRestart()
    {
        while (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            yield return null;
        }
        
        Debug.Log("Enterキー押下 - ゲーム再開");
        
        // UI要素を非表示
        if (finalResultPanel != null)
            finalResultPanel.SetActive(false);
        if (resultPanel != null)
            resultPanel.SetActive(false);
        
        // スコアリセット
        ResetGameScores();
        
        // プレイヤー入力を再開
        EnablePlayerInput();
        
        // 新しいゲームセット開始
        StartNewGameSet();
    }
    
    /// <summary>
    /// ゲームスコアのリセット
    /// </summary>
    private void ResetGameScores()
    {
        currentScore = 0;
        correctAnswers = 0;
        totalQuestions = 0;
        currentQuestionIndex = 0;
        currentStepIndex = 0;
        
        UpdateScoreDisplay();
        
        Debug.Log("ゲームスコアリセット完了");
    }

    /// <summary>
    /// エディタ用：回答ランダム化のテスト
    /// </summary>
    [ContextMenu("Test Answer Randomization")]
    public void TestAnswerRandomization()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("[TestAnswerRandomization] Play mode でのみ実行可能です");
            return;
        }
        
        Debug.Log("=== 回答ランダム化テスト開始 ===");
        
        // テスト用の問題データを作成
        var testQuestion = new CryptoQuestion
        {
            questionText = "テスト問題：正しい暗号方式はどれですか？",
            answers = new string[] { "AES", "RSA", "DES", "SHA" },
            correctAnswerIndex = 0, // AESが正解
            explanations = new string[] 
            {
                "✅ 正解！AESは現代的な共通鍵暗号です。",
                "❌ RSAは公開鍵暗号です。",
                "❌ DESは古い暗号方式です。",
                "❌ SHAはハッシュ関数です。"
            }
        };

        Debug.Log($"テスト問題: {testQuestion.questionText}");
        Debug.Log($"元の回答順序: [{string.Join(", ", testQuestion.answers)}]");
        Debug.Log($"正解: {testQuestion.answers[testQuestion.correctAnswerIndex]} (インデックス: {testQuestion.correctAnswerIndex})");

        // 複数回ランダム化をテスト
        bool originalDebugSetting = showAnswerRandomizationDebug;
        showAnswerRandomizationDebug = true;
        
        for (int i = 0; i < 5; i++)
        {
            Debug.Log($"\n--- ランダム化テスト {i + 1} ---");
            SetRandomizedAnswers(testQuestion);
        }

        showAnswerRandomizationDebug = originalDebugSetting;
        Debug.Log("=== 回答ランダム化テスト完了 ===");
    }
    
    /// <summary>
    /// エディタ用：現在の回答配置状況を表示
    /// </summary>
    [ContextMenu("Show Current Answer Layout")]
    public void ShowCurrentAnswerLayout()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("[ShowCurrentAnswerLayout] Play mode でのみ実行可能です");
            return;
        }
        
        if (answerCubes == null || answerCubes.Length == 0)
        {
            Debug.LogWarning("回答キューブが設定されていません");
            return;
        }

        Debug.Log("=== 現在の回答配置状況 ===");
        
        for (int i = 0; i < answerCubes.Length; i++)
        {
            if (answerCubes[i] != null)
            {
                string answerText = answerCubes[i].answerText;
                int answerIndex = answerCubes[i].answerIndex;
                Vector3 position = answerCubes[i].transform.position;
                bool isActive = answerCubes[i].gameObject.activeSelf;
                
                Debug.Log($"キューブ {i}: 回答「{answerText}」(インデックス: {answerIndex}) - 位置: {position} - アクティブ: {isActive}");
            }
            else
            {
                Debug.LogWarning($"キューブ {i}: null");
            }
        }
        
        Debug.Log("=== 回答配置表示完了 ===");
    }

    [Header("デバッグ・テスト機能")]
    [Space(10)]
    [Tooltip("回答ランダム化のデバッグ情報を表示するか")]
    public bool showAnswerRandomizationDebug = true;
    
    [Tooltip("各問題で同じランダムシードを使用するか（テスト用）")]
    public bool useFixedRandomSeed = false;
    
    [Tooltip("固定ランダムシード値（テスト用）")]
    public int fixedRandomSeed = 12345;

    // 追加: プレイヤーリスポーン→シャッフルのタイミング調整用パラメータ
    [Header("Timing Settings - タイミング設定")]
    [Tooltip("プレイヤーをリスポーン（ResetPlayerPosition 呼び出し）した後、回答キューブをシャッフルするまで待つ時間（秒）")]
    public float playerRespawnToShuffleDelay = 0.5f;

    [Tooltip("回答キューブをシャッフルした後、物理/描画の安定を待つ追加の時間（秒）")]
    public float shuffleStabilizationDelay = 0.15f;

    [Header("Progress Animation Settings")]
    [Tooltip("進捗スライダーのアニメーション時間")]
    public float progressAnimationDuration = 0.5f;
    
    [Tooltip("進捗増加時の色（正解時）")]
    public Color progressIncreaseColor = Color.green;
    
    [Tooltip("不正解時のテキスト色")]
    public Color incorrectTextColor = Color.red;
    
    [Tooltip("通常時の色")]
    public Color progressNormalColor = Color.white;
    
    // 進捗アニメーション管理用
    private Dictionary<int, Coroutine> sliderAnimations = new Dictionary<int, Coroutine>();

    // セットアップ中に回答キューブ等のコライダーを自動で有効化しないためのフラグ
    // (プレイヤー位置リセットとシャッフル中の誤トリガー防止用)
    private bool suppressColliderEnableDuringSetup = false;

    /// <summary>
    /// Inspector表示用のUI設定状況
    /// </summary>
    [Space(10)]
    [Header("=== UI設定状況確認 ===")]
    [SerializeField] private bool showUIStatus = true;
    
    /// <summary>
    /// Inspector上でUI設定状況を表示
    /// </summary>
    private void OnValidate()
    {
        if (!showUIStatus) return;
        
        // この関数はエディタでのみ実行されるため、実行時のログは出力しない
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEngine.Debug.Log("=== UI設定状況チェック ===");
            UnityEngine.Debug.Log($"ExplanationPanel: {(explanationPanel != null ? "✅設定済み" : "❌未設定")}");
            UnityEngine.Debug.Log($"ExplanationText: {(explanationText != null ? "✅設定済み" : "❌未設定")}");
            UnityEngine.Debug.Log($"QuestionText: {(questionText != null ? "✅設定済み" : "❌未設定")}");
            UnityEngine.Debug.Log($"CurrentScoreText: {(currentScoreText != null ? "✅設定済み" : "❌未設定")}");
            UnityEngine.Debug.Log("=========================");
        }
        #endif
    }

    private void Start()
    {
        progressTracker = GetComponent<ProgressTracker>();
        if (progressTracker == null)
            progressTracker = gameObject.AddComponent<ProgressTracker>();
        
        // CryptoAnimationManagerが未設定の場合は自動検索
        if (animationManager == null)
        {
            animationManager = GetComponent<CryptoAnimationManager>();
            if (animationManager == null)
            {
                animationManager = UnityEngine.Object.FindFirstObjectByType<CryptoAnimationManager>();
            }
        }
        
        // プレイヤーオブジェクトが未設定の場合は自動検索
        if (player == null)
        {
            // まずPlayerタグで検索
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log($"プレイヤーを自動検出しました: {player.name} (Playerタグ)");
            }
            else
            {
                // FirstPersonControllerという名前で検索
                playerObj = GameObject.Find("FirstPersonController");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                    Debug.Log($"プレイヤーを自動検出しました: {player.name} (名前検索)");
                }
                else
                {
                    // CharacterControllerコンポーネントがついているオブジェクトを検索
                    CharacterController characterController = UnityEngine.Object.FindFirstObjectByType<CharacterController>();
                    if (characterController != null)
                    {
                        player = characterController.transform;
                        Debug.Log($"プレイヤーを自動検出しました: {player.name} (CharacterController)");
                    }
                    else
                    {
                        Debug.LogWarning("プレイヤーオブジェクトが見つかりません。Inspectorで手動設定してください。");
                    }
                }
            }
        }
        
        // プレイヤー入力コンポーネントの自動検索
        if (playerInput == null)
        {
            // プレイヤーオブジェクトから検索
            if (player != null)
            {
                playerInput = player.GetComponent<StarterAssets.StarterAssetsInputs>();
                if (playerInput != null)
                {
                    Debug.Log($"プレイヤー入力コンポーネントを自動検出しました: {player.name}");
                }
            }
            
            // まだ見つからない場合はシーン全体から検索
            if (playerInput == null)
            {
                playerInput = UnityEngine.Object.FindFirstObjectByType<StarterAssets.StarterAssetsInputs>();
                if (playerInput != null)
                {
                    Debug.Log($"プレイヤー入力コンポーネントをシーンから検出しました: {playerInput.name}");
                }
                else
                {
                    Debug.LogWarning("プレイヤー入力コンポーネントが見つかりません。ゲーム終了時の入力制御ができない可能性があります。");
                }
            }
        }
        
        // ゲーム用カーソル設定
        SetGameCursor();
        
        // UI要素を自動検索して初期化（複数回実行で確実性向上）
        StartCoroutine(InitializeUIElementsSequence());
    }
    
    /// <summary>
    /// UI要素初期化シーケンス
    /// </summary>
    private IEnumerator InitializeUIElementsSequence()
    {
        Debug.Log("🔧 UI要素初期化シーケンス開始");
        
        // 初回自動検索
        AutoFindMissingUIElements();
        yield return new WaitForSeconds(0.1f);
        
        // 解説パネル初期化確認
        yield return StartCoroutine(ValidateExplanationPanelSetup());
        
        // 2回目の検索（より確実に）
        yield return new WaitForSeconds(0.1f);
        AutoFindMissingUIElements();
        
        // 最終確認
        Debug.Log("🔧 最終UI要素確認:");
        Debug.Log($"  explanationPanel: {(explanationPanel != null ? "✅" : "❌")}");
        Debug.Log($"  explanationText: {(explanationText != null ? "✅" : "❌")}");
        Debug.Log($"  questionText: {(questionText != null ? "✅" : "❌")}");
        Debug.Log($"  currentScoreText: {(currentScoreText != null ? "✅" : "❌")}");

		// 追加: QuestionText と回答ボタンの確実な可視化（Play 中にオブジェクトが無効化されているケース対策）
		if (questionText != null)
		{
			EnsureTextVisible(questionText);
		}
		if (answerButtons != null)
		{
			foreach (var b in answerButtons)
			{
				if (b == null) continue;
				b.gameObject.SetActive(true);
				var btnComp = b.GetComponent<UnityEngine.UI.Button>();
				if (btnComp != null) btnComp.interactable = true;
				var txt = b.GetComponentInChildren<Text>(true);
				if (txt != null) EnsureTextVisible(txt);
			}
		}

        // 解説パネルの確実な準備
        bool explanationReady = false;
        if (explanationPanel == null || explanationText == null)
        {
            Debug.LogWarning("⚠️ 解説パネル要素が不完全です。確実な作成を実行します。");
            yield return StartCoroutine(CreateExplanationPanelDynamically("初期化テスト"));
            
            // 作成後の検証
            if (explanationPanel != null && explanationText != null)
            {
                explanationReady = true;
                Debug.Log("✅ 解説パネル動的作成成功");
            }
            else
            {
                Debug.LogError("❌ 解説パネル作成失敗 - ゲーム中に再作成します");
            }
        }
        else
        {
            explanationReady = true;
            Debug.Log("✅ 解説パネル既存要素確認完了");
        }
        
        // 解説パネルの初期状態を確実に設定
        if (explanationReady && explanationPanel != null)
        {
            explanationPanel.SetActive(false);
            Debug.Log("✅ 解説パネル初期状態設定完了");
        }
        
        Debug.Log("✅ UI要素初期化シーケンス完了");
        
        // ゲーム開始
        StartNewGameSet();
    }

    /// <summary>
    /// ゲーム用カーソル設定
    /// </summary>
    private void SetGameCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("ゲーム用カーソル設定完了");
    }
    
    /// <summary>
    /// 不足しているUI要素の自動検索
    /// </summary>
    private void AutoFindMissingUIElements()
    {
        // 解説パネルの検索
        if (explanationPanel == null)
        {
            GameObject foundPanel = GameObject.Find("ExplanationPanel");
            if (foundPanel == null)
            {
                foundPanel = GameObject.Find("Explanation Panel");
            }
            if (foundPanel == null)
            {
                foundPanel = GameObject.Find("Panel_Explanation");
            }
            
            if (foundPanel != null)
            {
                explanationPanel = foundPanel;
                Debug.Log($"解説パネルを自動検出: {foundPanel.name}");
            }
        }
        
        // 解説テキストの検索
        if (explanationText == null)
        {
            // 既存の探索ロジック
            Text foundText = UnityEngine.Object.FindObjectOfType<Text>();
            if (foundText != null && (foundText.name.Contains("Explanation") || foundText.name.Contains("explanation")))
            {
                explanationText = foundText;
                Debug.Log($"解説テキストを自動検出: {foundText.name}");
            }
        }

        // 解説ヘッダーが未設定なら、解説パネル内をより確実に検索（"ExplanationHeader" 名を優先）
        if (explanationHeaderText == null && explanationPanel != null)
        {
            Transform headerTf = explanationPanel.transform.Find("ExplanationHeader");
            if (headerTf != null)
            {
                explanationHeaderText = headerTf.GetComponent<Text>();
            }
            else
            {
                // 子要素の Text を走査して "Header" を含むものを探す（柔軟検出）
                foreach (var txt in explanationPanel.GetComponentsInChildren<Text>(true))
                {
                    if (txt != null && txt.name.IndexOf("Header", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        explanationHeaderText = txt;
                        break;
                    }
                }
            }
            if (explanationHeaderText != null)
            {
                Debug.Log($"解説ヘッダーを自動検出: {explanationHeaderText.name}");
            }
        }

        // 問題テキストの検索
        if (questionText == null)
        {
            GameObject questionObj = GameObject.Find("QuestionText");
            if (questionObj == null)
            {
                questionObj = GameObject.Find("Question Text");
            }
            if (questionObj == null)
            {
                questionObj = GameObject.Find("Text_Question");
            }
            
            if (questionObj != null)
            {
                questionText = questionObj.GetComponent<Text>();
                if (questionText != null)
                {
                    Debug.Log($"問題テキストを自動検出: {questionObj.name}");
                }
            }
        }
        
        // スコアテキストの検索
        if (currentScoreText == null)
        {
            GameObject scoreObj = GameObject.Find("CurrentScoreText");
            if (scoreObj == null)
            {
                scoreObj = GameObject.Find("Score Text");
            }
            if (scoreObj == null)
            {
                scoreObj = GameObject.Find("Text_Score");
            }
            
            if (scoreObj != null)
            {
                currentScoreText = scoreObj.GetComponent<Text>();
                if (currentScoreText != null)
                {
                    Debug.Log($"スコアテキストを自動検出: {scoreObj.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// 解説パネル設定の検証
    /// </summary>
    private IEnumerator ValidateExplanationPanelSetup()
    {
        yield return new WaitForSeconds(0.1f);
        
        if (explanationPanel == null || explanationText == null)
        {
            Debug.LogWarning("解説パネル要素が不完全です。動的作成を実行します。");
            yield return StartCoroutine(CreateExplanationPanelDynamically("初期化テスト"));
        }
        else
        {
            Debug.Log("解説パネル設定が完了しています。");
        }
    }
    
    /// <summary>
    /// 解説パネルの動的作成
    /// </summary>
    private IEnumerator CreateExplanationPanelDynamically(string initialText)
    {
        // 基本的な解説パネルを動的作成
        if (explanationPanel == null)
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                GameObject panel = new GameObject("ExplanationPanel");
                // add RectTransform so UI children layout correctly
                var panelRT = panel.AddComponent<RectTransform>();
                panel.transform.SetParent(canvas.transform, false);
                // optional background
                var img = panel.AddComponent<UnityEngine.UI.Image>();
                img.color = new Color(0f, 0f, 0f, 0.6f);
                explanationPanel = panel;

                Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                // ---- ヘッダー Text を先に作成（中央表示、太字） ----
                GameObject headerObj = new GameObject("ExplanationHeader");
                headerObj.transform.SetParent(panel.transform, false);
                explanationHeaderText = headerObj.AddComponent<Text>();
                explanationHeaderText.text = ""; // 初期は空
                explanationHeaderText.fontSize = 28;
                explanationHeaderText.fontStyle = FontStyle.Bold;
                explanationHeaderText.alignment = TextAnchor.MiddleCenter;
                explanationHeaderText.color = Color.white;
                if (defaultFont != null) explanationHeaderText.font = defaultFont;

                // header RectTransform layout
                RectTransform headerRT = explanationHeaderText.GetComponent<RectTransform>();
                headerRT.anchorMin = new Vector2(0.5f, 1f);
                headerRT.anchorMax = new Vector2(0.5f, 1f);
                headerRT.pivot = new Vector2(0.5f, 1f);
                headerRT.anchoredPosition = new Vector2(0f, -10f);
                headerRT.sizeDelta = new Vector2(600f, 40f);

                // ---- 本文 Text を作成（左寄せ） ----
                GameObject textObj = new GameObject("ExplanationBody");
                textObj.transform.SetParent(panel.transform, false);

                explanationText = textObj.AddComponent<Text>();
                explanationText.text = initialText;
                explanationText.fontSize = 20;
                explanationText.alignment = TextAnchor.UpperLeft;
                explanationText.color = Color.white;
                if (defaultFont != null)
                {
                    explanationText.font = defaultFont;
                }

                // body RectTransform layout: 横幅いっぱい、ヘッダー下に配置
                RectTransform bodyRT = explanationText.GetComponent<RectTransform>();
                bodyRT.anchorMin = new Vector2(0f, 1f);
                bodyRT.anchorMax = new Vector2(1f, 1f);
                bodyRT.pivot = new Vector2(0.5f, 1f);
                bodyRT.anchoredPosition = new Vector2(0f, -60f);
                bodyRT.sizeDelta = new Vector2(1000f, 300f);

                Debug.Log("解説パネル（ヘッダー＋本文）を動的作成しました");
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            // 既存パネルがあるがヘッダーがない場合は追加してレイアウトする
            if (explanationHeaderText == null && explanationPanel != null)
            {
                GameObject headerObj = new GameObject("ExplanationHeader");
                headerObj.transform.SetParent(explanationPanel.transform, false);
                explanationHeaderText = headerObj.AddComponent<Text>();
                explanationHeaderText.text = "";
                explanationHeaderText.fontSize = 28;
                explanationHeaderText.fontStyle = FontStyle.Bold;
                explanationHeaderText.alignment = TextAnchor.MiddleCenter;
                explanationHeaderText.color = Color.white;
                Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (defaultFont != null) explanationHeaderText.font = defaultFont;

                RectTransform headerRT = explanationHeaderText.GetComponent<RectTransform>();
                headerRT.anchorMin = new Vector2(0.5f, 1f);
                headerRT.anchorMax = new Vector2(0.5f, 1f);
                headerRT.pivot = new Vector2(0.5f, 1f);
                headerRT.anchoredPosition = new Vector2(0f, -10f);
                headerRT.sizeDelta = new Vector2(600f, 40f);

                // 既存の explanationText がある場合は位置を下げる
                if (explanationText != null)
                {
                    RectTransform bodyRT = explanationText.GetComponent<RectTransform>();
                    if (bodyRT != null)
                    {
                        bodyRT.anchorMin = new Vector2(0f, 1f);
                        bodyRT.anchorMax = new Vector2(1f, 1f);
                        bodyRT.pivot = new Vector2(0.5f, 1f);
                        bodyRT.anchoredPosition = new Vector2(0f, -60f);
                        bodyRT.sizeDelta = new Vector2(1000f, 300f);
                    }
                }

                Debug.Log("既存解説パネルにヘッダーを追加しました");
            }
        }

        yield return null;
    }
    
    /// <summary>
    /// 新しいゲームセットの開始
    /// </summary>
    public void StartNewGameSet()
    {
        Debug.Log("新しいゲームセット開始");
        
        // ゲーム状態のリセット
        currentQuestionIndex = 0;
        currentStepIndex = 0;
        gameTimer = gameSetDuration;
        isGameActive = true;

        // 追加: タイマー表示を初期化（カウントダウンがすぐに見えるようにする）
        UpdateTimerText();
        
        // ゲームセットの生成（暗号方式の組み合わせ）
        GenerateGameSet();
        
        // 最初の問題開始
        if (currentGameSet != null && currentGameSet.Length > 0)
        {
            StartCurrentQuestion();
        }
        else
        {
            Debug.LogError("ゲームセットの生成に失敗しました");
        }
    }
    
    /// <summary>
    /// ゲームセットの生成（固定順序：共通鍵→公開鍵→ハイブリッド）
    /// </summary>
    private void GenerateGameSet()
    {
        List<CryptoType> orderedTypes = new List<CryptoType>();
        
        // 固定順序で暗号方式を追加
        if (enableSymmetricKey) orderedTypes.Add(CryptoType.SymmetricKey);
        if (enablePublicKey) orderedTypes.Add(CryptoType.PublicKey);
        if (enableHybrid) orderedTypes.Add(CryptoType.Hybrid);
        
        if (orderedTypes.Count == 0)
        {
            Debug.LogError("有効な暗号方式が選択されていません");
            return;
        }
        
        // questionsPerSetまで順序通りに設定
        currentGameSet = new CryptoType[questionsPerSet];
        for (int i = 0; i < questionsPerSet; i++)
        {
            if (i < orderedTypes.Count)
            {
                currentGameSet[i] = orderedTypes[i];
            }
            else
            {
                // questionsPerSetが暗号方式数より多い場合はループ
                currentGameSet[i] = orderedTypes[i % orderedTypes.Count];
            }
        }
        
        Debug.Log($"ゲームセット生成（固定順序): [{string.Join(", ", currentGameSet)}]");
    }
    
    /// <summary>
    /// 現在の問題開始
    /// </summary>
    private void StartCurrentQuestion()
    {
        // 変更: 直接処理せず、順序保証のためコルーチンで実行する
        StartCoroutine(StartCurrentQuestionRoutine());
    }

    // 新規: StartCurrentQuestion の順序を保証するコルーチン
    private IEnumerator StartCurrentQuestionRoutine()
    {
        if (currentGameSet == null || currentQuestionIndex >= currentGameSet.Length)
        {
            Debug.Log("ゲームセット完了");
            ShowFinalScore();
            yield break;
        }

        CryptoType currentType = currentGameSet[currentQuestionIndex];

        // インタラクションを一旦無効化して、シャッフルや移動で誤入力が発生しないようにする
        DisableAnswerInteractions();

        // セットアップ開始フラグ: この間、ResetVisualState/ResetCubeRuntimeState はコライダーを有効化しない
        suppressColliderEnableDuringSetup = true;
        Debug.Log("StartCurrentQuestionRoutine: suppressColliderEnableDuringSetup = true");

        // プレイヤー位置をまず確実にリセット（これによりプレイヤーがシャッフル中に誤って触れない）
        if (player != null)
        {
            ResetPlayerPosition();
            Debug.Log("StartCurrentQuestionRoutine: プレイヤー位置を先にリセットしました");

            // 追加: インスペクタで調整可能な遅延を挟むことで
            // リスポーン完了（ビューの安定や物理の反映）を待ってからシャッフルを行う
            if (playerRespawnToShuffleDelay > 0f)
            {
                Debug.Log($"StartCurrentQuestionRoutine: プレイヤーリスポーン完了後、{playerRespawnToShuffleDelay:F2}s 待機してからシャッフルします");
                yield return new WaitForSeconds(playerRespawnToShuffleDelay);
            }
        }

        // 少し待って物理/Transformの安定を待つ（EndOfFrame）
        yield return new WaitForEndOfFrame();

        // 公開鍵/ハイブリッド時の事前処理（既存ロジックを維持）
        if ((currentType == CryptoType.PublicKey || currentType == CryptoType.Hybrid) && currentStepIndex == 0 && animationManager != null)
        {
            if (animationManager.dataCube != null)
            {
                animationManager.ForceSetDataCubePosition(new Vector3(-5f, 3f, 10f));
            }
            animationManager.HideAllKeys();
            Debug.Log("公開鍵/ハイブリッド暗号方式1問目開始前: 全ての鍵を非表示");
        }

        var question = CryptoQuestionDatabase.GetQuestion(currentType, currentStepIndex);

        if (question == null)
        {
            Debug.LogError($"問題データが見つかりません: {currentType}, ステップ {currentStepIndex}");
            // インタラクションは復帰しておく
            suppressColliderEnableDuringSetup = false;
            Debug.Log("StartCurrentQuestionRoutine: suppressColliderEnableDuringSetup = false (error path)");
            EnableAnswerInteractions();
            yield break;
        }
        
        // UI更新（テキスト可視化含む）
        if (questionText != null)
        {
            questionText.text = question.questionText;
            EnsureTextVisible(questionText);
        }
        
        // 暗号方式に応じた UI アニメ再生（非同期ではない想定）
        PlayCryptoTypeAnimation(currentType);
        
        // 回答キューブをシャッフルして設定（プレイヤーは既にリセット済みなので誤選択を防げる）
        SetRandomizedAnswers(question);
        
        // シャッフル後の物理/描画の安定を少し待つ（パラメータ化）
        yield return new WaitForEndOfFrame();
        if (shuffleStabilizationDelay > 0f)
        {
            Debug.Log($"StartCurrentQuestionRoutine: シャッフル後の安定化を {shuffleStabilizationDelay:F2}s 待機します");
            yield return new WaitForSeconds(shuffleStabilizationDelay);
        }

        // セットアップ完了：コライダーの自動有効化を許可してからインタラクションを復帰する
        suppressColliderEnableDuringSetup = false;
        Debug.Log("StartCurrentQuestionRoutine: suppressColliderEnableDuringSetup = false (setup complete)");

        // 最後にインタラクションを復帰
        EnableAnswerInteractions();

        Debug.Log($"問題開始: {currentType}, ステップ: {currentStepIndex} (順序保証済み)");
    }
    
    /// <summary>
    /// 暗号方式に応じたアニメーションを再生
    /// </summary>
    private void PlayCryptoTypeAnimation(CryptoType cryptoType)
    {
        // UIManagerを探索
        if (uiManager == null)
        {
            uiManager = UnityEngine.Object.FindFirstObjectByType<CryptoUIManager>();
        }
        
        if (uiManager != null)
        {
            // 質問テキストを対象としてアニメーション実行
            Transform animationTarget = questionText?.transform;
            uiManager.PlayCryptoTypeAnimation(cryptoType, animationTarget);
            
            Debug.Log($"[CryptoGameManager] {cryptoType} アニメーション再生開始");
        }
        else
        {
            Debug.LogWarning("[CryptoGameManager] CryptoUIManagerが見つかりません。アニメーションをスキップします。");
        }
    }
    
    /// <summary>
    /// 不正解が選択された時の処理
    /// </summary>
    public void OnIncorrectAnswerSelected(int selectedAnswerIndex = -1)
    {
        if (currentGameSet == null || currentQuestionIndex >= currentGameSet.Length)
        {
            Debug.LogError("OnIncorrectAnswerSelected: 無効なゲーム状態です");
            return;
        }
        
        Debug.Log("不正解処理開始");
        
        // 現在の問題情報を取得
        CryptoType currentType = currentGameSet[currentQuestionIndex];
        var question = CryptoQuestionDatabase.GetQuestion(currentType, currentStepIndex);
        
        if (question != null)
        {
            // 解説パネル表示（選択された回答に対応する解説を表示）
            ShowExplanationPanel(question, false, selectedAnswerIndex); // false = 不正解
        }
        
        // 不正解表示
        ShowResultMessage("❌ 不正解", incorrectTextColor);
        
        // プレイヤー位置を即座にリセット
        if (player != null)
        {
            ResetPlayerPosition();
            Debug.Log("不正解時: プレイヤー位置をリセットしました");
        }
        
        // 不正解処理（ゲージ減少）
        if (progressTracker != null)
        {
            progressTracker.OnIncorrectAnswer(currentType);
            Debug.Log("[OnIncorrectAnswerSelected] 進度減少処理完了: " + currentType);
        }
        
        // スコア減点
        AddIncorrectAnswerScore();
        
        // 進度UI更新
        UpdateProgressDisplay();
        
        // 不正解時の視覚効果
        StartCoroutine(ShowIncorrectAnswerEffect(currentType));
        
        // しばらく待ってから同じ問題を再表示
        StartCoroutine(HandleIncorrectAnswerDelay());
    }
    
    /// <summary>
    /// 正解時の処理
    /// </summary>
    private void OnCorrectAnswer(int selectedAnswerIndex = -1)
    {
        Debug.Log("正解処理開始");
        
        // 現在の問題情報を取得
        if (currentGameSet != null && currentQuestionIndex < currentGameSet.Length)
        {
            CryptoType currentType = currentGameSet[currentQuestionIndex];
            var question = CryptoQuestionDatabase.GetQuestion(currentType, currentStepIndex);
            
            if (question != null)
            {
                // 解説パネル表示（正解の解説を表示）
                ShowExplanationPanel(question, true, selectedAnswerIndex); // true = 正解
            }
        }
        
        // スコア加算
        AddCorrectAnswerScore();
        
        // 進度更新
        if (progressTracker != null && currentGameSet != null && currentQuestionIndex < currentGameSet.Length)
        {
            CryptoType currentType = currentGameSet[currentQuestionIndex];
            progressTracker.OnCorrectAnswer(currentType);
        }
        
        // アニメーション実行
        if (currentGameSet != null && currentQuestionIndex < currentGameSet.Length)
        {
            CryptoType currentType = currentGameSet[currentQuestionIndex];
            var question = CryptoQuestionDatabase.GetQuestion(currentType, currentStepIndex);
            
            if (question != null && !string.IsNullOrEmpty(question.animationType))
            {
                Debug.Log("正解アニメーション実行: " + question.animationType);
                PlayCorrectAnswerAnimation(currentType, question);
            }
        }
        
        // 次のステップまたは次の問題へ
        currentStepIndex++;
        
        // 現在の暗号方式の全問題をクリアしたか確認
        if (currentGameSet != null && currentQuestionIndex < currentGameSet.Length)
        {
            CryptoType currentType = currentGameSet[currentQuestionIndex];
            int totalQuestions = CryptoQuestionDatabase.GetQuestionCount(currentType);
            
            if (currentStepIndex >= totalQuestions)
            {
                // 次の暗号方式へ
                currentQuestionIndex++;
                currentStepIndex = 0;
                Debug.Log("暗号方式完了。次の方式へ移行: インデックス " + currentQuestionIndex);
            }
        }
        
        // プレイヤー位置リセット
        if (player != null)
        {
            ResetPlayerPosition();
        }
        
        // 次の問題開始
        StartCoroutine(StartNextQuestionDelay());
    }
    
    /// <summary>
    /// 解説パネルの表示
    /// </summary>
    /// <param name="question">問題データ</param>
    /// <param name="isCorrect">正解かどうか</param>
    /// <param name="selectedAnswerIndex">選択された回答のインデックス</param>
    private void ShowExplanationPanel(CryptoQuestion question, bool isCorrect, int selectedAnswerIndex = -1)
    {
        if (explanationPanel == null || explanationText == null)
        {
            Debug.LogWarning("解説パネルまたは解説テキストが設定されていません。動的作成を試行します。");
            StartCoroutine(CreateExplanationPanelAndShow(question, isCorrect, selectedAnswerIndex));
            return;
        }

        // ヘッダー（中央）と本文（左寄せ）を分けて設定する
        if (explanationHeaderText != null)
        {
            // ヘッダー有効化・テキスト設定（改行は入れない）
            explanationHeaderText.gameObject.SetActive(true);
            explanationHeaderText.text = isCorrect ? "✅ 正解！" : "❌ 不正解";

            // 色を正誤で分ける（正解=緑、不正解=赤）
            explanationHeaderText.color = isCorrect ? Color.green : Color.red;

            // 確実に中央表示・太字に
            explanationHeaderText.alignment = TextAnchor.MiddleCenter;
            explanationHeaderText.fontStyle = FontStyle.Bold;

            // ヘッダーの RectTransform を確実に設定（パネル上部中央）
            RectTransform headerRT = explanationHeaderText.GetComponent<RectTransform>();
            if (headerRT != null)
            {
                headerRT.anchorMin = new Vector2(0.5f, 1f);
                headerRT.anchorMax = new Vector2(0.5f, 1f);
                headerRT.pivot = new Vector2(0.5f, 1f);
                headerRT.anchoredPosition = new Vector2(0f, -10f);
                headerRT.sizeDelta = new Vector2(Mathf.Max(400f, headerRT.sizeDelta.x), 40f);
            }

            // ヘッダーをパネル内の先頭（上）に移動して本文が下に来るようにする
            explanationHeaderText.transform.SetAsFirstSibling();
        }

        // 本文は選択回答＋解説のみ（GetExplanationBody を使用）
        string body = GetExplanationBody(question, isCorrect, selectedAnswerIndex);
        explanationText.text = body;
        explanationText.gameObject.SetActive(true);

        // 本文の RectTransform をヘッダーの下に来るように調整（既存パネルにも対応）
        RectTransform bodyRT = explanationText.GetComponent<RectTransform>();
        if (bodyRT != null)
        {
            bodyRT.anchorMin = new Vector2(0f, 1f);
            bodyRT.anchorMax = new Vector2(1f, 1f);
            bodyRT.pivot = new Vector2(0.5f, 1f);
            // ヘッダー高さに応じて下げる（40 はヘッダーの sizeDelta.y を想定）
            float headerHeight = 40f;
            bodyRT.anchoredPosition = new Vector2(0f, -(headerHeight + 10f));
            // 高さはある程度確保
            bodyRT.sizeDelta = new Vector2(1000f, 300f);
        }

        // パネルを表示
        explanationPanel.SetActive(true);

        Debug.Log("解説パネル表示: " + (isCorrect ? "正解" : "不正解") +
                 (selectedAnswerIndex >= 0 ? " (選択回答: " + selectedAnswerIndex + ")" : ""));

        // 一定時間後に非表示
        StartCoroutine(HideExplanationPanelAfterDelay());
    }
    
    /// <summary>
    /// 解説パネルの動的作成と表示
    /// </summary>
    /// <param name="question">問題データ</param>
    /// <param name="isCorrect">正解かどうか</param>
    /// <param name="selectedAnswerIndex">選択された回答のインデックス</param>
    private IEnumerator CreateExplanationPanelAndShow(CryptoQuestion question, bool isCorrect, int selectedAnswerIndex = -1)
    {
        yield return StartCoroutine(CreateExplanationPanelDynamically("作成中..."));
        
        if (explanationPanel != null && explanationText != null)
        {
            ShowExplanationPanel(question, isCorrect, selectedAnswerIndex);
        }
        else
        {
            Debug.LogError("解説パネルの動的作成に失敗しました");
        }
    }
    
    /// <summary>
    /// 解説本文のみを返す（ヘッダーは ShowExplanationPanel 側で表示）
    /// </summary>
    private string GetExplanationBody(CryptoQuestion question, bool isCorrect, int selectedAnswerIndex = -1)
    {
        if (question == null)
        {
            return isCorrect ? "よくできました！" : "もう一度チャレンジしてください。";
        }

        string explanation = "";

        // 対応する解説テキストを選ぶ
        if (selectedAnswerIndex >= 0 && question.explanations != null &&
            selectedAnswerIndex < question.explanations.Length)
        {
            explanation = question.explanations[selectedAnswerIndex];
        }
        else if (isCorrect && question.explanations != null &&
                 question.correctAnswerIndex >= 0 && question.correctAnswerIndex < question.explanations.Length)
        {
            explanation = question.explanations[question.correctAnswerIndex];
        }
        else
        {
            explanation = isCorrect ? "よくできました！" : "もう一度チャレンジしてください。";
        }

        // 選択した回答を先頭に追加（あれば）
        if (selectedAnswerIndex >= 0 && question.answers != null && selectedAnswerIndex < question.answers.Length)
        {
            string selectedAnswer = question.answers[selectedAnswerIndex];
            return "選択した回答: 「" + selectedAnswer + "」\n\n" + explanation;
        }

        return explanation;
    }
    
    /// <summary>
    /// 解説パネルを一定時間後に非表示
    /// </summary>
    private IEnumerator HideExplanationPanelAfterDelay()
    {
        yield return new WaitForSeconds(5f); // 5秒間表示
        
        if (explanationPanel != null)
        {
            explanationPanel.SetActive(false);
            Debug.Log("解説パネルを非表示にしました");
        }
    }

    /// <summary>
    /// 正解時のアニメーション再生
    /// </summary>
    private void PlayCorrectAnswerAnimation(CryptoType cryptoType, CryptoQuestion question)
    {
        if (animationManager != null && !string.IsNullOrEmpty(question.animationType))
        {
            Debug.Log("3Dアニメーション再生: " + question.animationType);
            animationManager.PlayCorrectAnswerAnimation(question);
        }
        else
        {
            Debug.LogWarning("AnimationManagerが未設定、またはアニメーションタイプが空です");
        }
    }
    
    /// <summary>
    /// 不正解時の視覚効果
    /// </summary>
    private IEnumerator ShowIncorrectAnswerEffect(CryptoType cryptoType)
    {
        Debug.Log("不正解エフェクト表示: " + cryptoType);
        
        // ここで視覚的なエフェクトを実装可能
        // 例：画面の赤いフラッシュ、音響効果など
        
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("不正解エフェクト完了");
    }
    
    /// <summary>
    /// 進度表示の更新
    /// </summary>
    private void UpdateProgressDisplay()
    {
        if (progressTracker == null) return;

        // 各進捗値を取得（ProgressTracker が 0..1 または 0..100 のいずれを返しても扱える）
        float rawSym = progressTracker.GetProgress(CryptoType.SymmetricKey);
        float rawPub = progressTracker.GetProgress(CryptoType.PublicKey);
        float rawHybrid = progressTracker.GetProgress(CryptoType.Hybrid);

        float percentSym = NormalizeToPercent(rawSym);
        float percentPub = NormalizeToPercent(rawPub);
        float percentHybrid = NormalizeToPercent(rawHybrid);

        // スライダーが存在する場合はスライダーを 0..100 のレンジで更新（Inspector の設定値に依存せず統一）
        if (progressSliders != null && progressSliders.Length >= 3)
        {
            SetSliderToPercent(progressSliders[0], percentSym);
            SetSliderToPercent(progressSliders[1], percentPub);
            SetSliderToPercent(progressSliders[2], percentHybrid);
        }

        // 進度ラベルの更新（常にパーセント表記）
        if (progressLabels != null && progressLabels.Length >= 3)
        {
            progressLabels[0].text = "共通鍵暗号: " + percentSym.ToString("F1") + "%";
            progressLabels[1].text = "公開鍵暗号: " + percentPub.ToString("F1") + "%";
            progressLabels[2].text = "ハイブリッド暗号: " + percentHybrid.ToString("F1") + "%";
        }

        Debug.Log("進度表示更新完了 （% 表示をスライダーと整合）");
    }

    /// <summary>
    /// raw（0..1 または 0..100）を常に 0..100 のパーセントに変換する
    /// </summary>
    private float NormalizeToPercent(float raw)
    {
        // raw が 1 より大きければ既にパーセント（0..100）とみなす。
        if (raw > 1.5f)
            return Mathf.Clamp(raw, 0f, 100f);

        // それ以外は 0..1 と見なして 100倍して clamp
        return Mathf.Clamp01(raw) * 100f;
    }

    /// <summary>
    /// スライダーを 0..100 の範囲に統一して値をセットする（null チェック含む）
    /// </summary>
    private void SetSliderToPercent(Slider s, float percent)
    {
        if (s == null) return;

        s.minValue = 0f;
        s.maxValue = 100f;
        s.wholeNumbers = false;
        // 値を代入（Clamp により安全）
        s.value = Mathf.Clamp(percent, s.minValue, s.maxValue);
    }
    
    /// 以下、他スクリプトから呼ばれる未定義メソッドの最小実装を追加します。

    // プレイヤー入力を無効化（Enter待ちなどで使用）
    private void DisablePlayerInput()
    {
        if (playerInput != null)
        {
            try { playerInput.enabled = false; } catch { /* コンポーネントに enabled がない場合無視 */ }
        }
        // カーソルの復帰（UI操作用）
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // プレイヤー入力を有効化（ゲーム再開時など）
    private void EnablePlayerInput()
    {
        if (playerInput != null)
        {
            try { playerInput.enabled = true; } catch { /* ignore */ }
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 回答の相互作用を無効化（問題表示中の誤操作防止）
    private void DisableAnswerInteractions()
    {
        // UIボタン無効化
        if (answerButtons != null)
        {
            foreach (var btn in answerButtons)
            {
                if (btn == null) continue;
                try { btn.interactable = false; } catch { }
            }
        }

        // 3Dキューブのコライダーを無効化して選択できないようにする
        if (answerCubes != null)
        {
            foreach (var cube in answerCubes)
            {
                if (cube == null || cube.gameObject == null) continue;
                try
                {
                    foreach (var col in cube.GetComponentsInChildren<Collider>(true))
                    {
                        if (col != null) col.enabled = false;
                    }
                }
                catch { }
            }
        }

        Debug.Log("DisableAnswerInteractions: 回答の相互作用を無効化しました");
    }
    
    // 回答の相互作用を有効化（問題応答受付を再開）
    private void EnableAnswerInteractions()
    {
        // UIボタンを有効化（既存の onClick リスナーは維持）
        if (answerButtons != null)
        {
            foreach (var btn in answerButtons)
            {
                if (btn == null) continue;
                try { btn.interactable = true; } catch { }
                // ボタンが非表示になっている場合は表示しておく（UI の意図に従って適宜調整可能）
                try { if (!btn.gameObject.activeSelf) btn.gameObject.SetActive(true); } catch { }
            }
        }

        // 3Dキューブのコライダーを有効化して選択可能にする
        if (answerCubes != null)
        {
            foreach (var cube in answerCubes)
            {
                if (cube == null || cube.gameObject == null) continue;
                try
                {
                    foreach (var col in cube.GetComponentsInChildren<Collider>(true))
                    {
                        if (col != null) col.enabled = true;
                    }
                }
                catch { }
            }
        }

        Debug.Log("EnableAnswerInteractions: 回答の相互作用を有効化しました");
    }

    // 外部 UI / キューブ等から回答が選択された時に呼ばれる共通入口
    // selectedIndex は表示上の回答インデックス（SetRandomizedAnswers 実装に合わせる必要あり）
    public void OnAnswerSelected(int selectedIndex)
    {
        if (currentGameSet == null || currentQuestionIndex >= currentGameSet.Length)
        {
            Debug.LogWarning("OnAnswerSelected: 無効なゲーム状態");
            return;
        }

        CryptoType currentType = currentGameSet[currentQuestionIndex];
        var question = CryptoQuestionDatabase.GetQuestion(currentType, currentStepIndex);
        if (question == null)
        {
            Debug.LogWarning("OnAnswerSelected: 問題データが見つかりません");
            return;
        }

        // 単純判定: 選択インデックスと question.correctAnswerIndex を比較
        if (selectedIndex == question.correctAnswerIndex)
        {
            OnCorrectAnswer(selectedIndex);
        }
        else
        {
            OnIncorrectAnswerSelected(selectedIndex);
        }
    }

    // 回答候補（キューブ・ボタン）をランダム化して設定する簡易実装
    private void SetRandomizedAnswers(CryptoQuestion question)
    {
	if (question == null) return;

	// Try to refresh any null entries in answerCubes array from the scene
	TryRefreshAnswerCubes();

	// Shuffle indices (fixed seed optional)
	List<int> order = Enumerable.Range(0, question.answers.Length).ToList();
	if (useFixedRandomSeed)
	{
		System.Random rng = new System.Random(fixedRandomSeed);
		order = order.OrderBy(_ => rng.Next()).ToList();
	}
	else
	{
		order = order.OrderBy(_ => UnityEngine.Random.value).ToList();
	}

	// Collect available cubes (non-null)
	List<CryptoAnswerCube> availableCubes = new List<CryptoAnswerCube>();
	if (answerCubes != null)
	{
		foreach (var c in answerCubes)
		{
			if (c != null) availableCubes.Add(c);
		}
	}

	int idx = 0;

	// Assign to 3D cubes first (as many as available)
	for (; idx < availableCubes.Count && idx < order.Count; idx++)
	{
		var cube = availableCubes[idx];
		int srcIdx = order[idx];
		string ansText = question.answers[srcIdx];

		// --- 追加: キューブのランタイム状態を強制リセット（選択済みフラグや無効化されたコンポーネントを復帰） ---
		ResetCubeRuntimeState(cube);

		// Update script properties
		cube.answerText = ansText;
		cube.answerIndex = srcIdx;

		// Update child UI/TextMesh if present
		var uiText = cube.GetComponentInChildren<Text>(true);
		if (uiText != null)
		{
			uiText.text = ansText;
			EnsureTextVisible(uiText);
		}
		var textMesh = cube.GetComponentInChildren<TextMesh>(true);
		if (textMesh != null)
		{
			textMesh.text = ansText;
			textMesh.gameObject.SetActive(true);
		}

		// Ensure visible and interactive
		cube.gameObject.SetActive(true);
		ResetVisualState(cube.gameObject);
	}

	// If more answers remain, use UI buttons as fallback
	for (int j = idx; j < order.Count; j++)
	{
		int srcIdx = order[j];
		string ansText = question.answers[srcIdx];
		int buttonSlot = j - idx; // map remaining answers to buttons sequentially

		// Prefer matching button index if available
		if (answerButtons != null && buttonSlot < answerButtons.Length)
		{
			var btn = answerButtons[buttonSlot];
			if (btn != null)
			{
				var txt = btn.GetComponentInChildren<Text>(true);
				if (txt != null)
				{
					txt.text = ansText;
					EnsureTextVisible(txt);
				}
				btn.onClick.RemoveAllListeners();
				int captured = srcIdx;
				btn.onClick.AddListener(() => OnAnswerSelected(captured));
				btn.gameObject.SetActive(true);
				ResetVisualState(btn.gameObject);
				var btnComp = btn.GetComponent<UnityEngine.UI.Button>();
				if (btnComp != null) btnComp.interactable = true;
				continue;
			}
		}

		// If no button slot is available, log (should not happen if UI configured)
		Debug.LogWarning("[SetRandomizedAnswers] 回答割当先不足 - インデックス: " + srcIdx);
	}

	// Hide any extra cubes beyond availableAnswers (safety)
	// ここもキューブを隠す前に内部フラグをクリアしておく
	if (availableCubes.Count > order.Count)
	{
		for (int k = order.Count; k < availableCubes.Count; k++)
		{
			if (availableCubes[k] != null)
			{
				ResetCubeRuntimeState(availableCubes[k]); // 追加：内部状態クリア
				availableCubes[k].gameObject.SetActive(false);
			}
		}
	}

	// If there are more button slots than needed, hide the extras
	if (answerButtons != null)
	{
		int usedButtons = Mathf.Max(0, order.Count - availableCubes.Count);
		for (int b = usedButtons; b < answerButtons.Length; b++)
		{
			if (answerButtons[b] != null)
			{
				// keep any buttons used (0..usedButtons-1), hide the rest
				if (b >= usedButtons) answerButtons[b].gameObject.SetActive(false);
			}
		}
	}

	if (showAnswerRandomizationDebug)
	{
		Debug.Log($"SetRandomizedAnswers: 配置順 = [{string.Join(", ", order)}], cubesAvailable={availableCubes.Count}, buttonsAvailable={(answerButtons!=null?answerButtons.Length:0)}");
	}
}

// Try to refill null entries in answerCubes from the scene (non-destructive)
private void TryRefreshAnswerCubes()
{
	if (answerCubes == null) return;

	// If any slot is null, attempt to find CryptoAnswerCube components in scene and refill empty slots
	bool anyNull = false;
	for (int i = 0; i < answerCubes.Length; i++)
	{
		if (answerCubes[i] == null)
		{
			anyNull = true;
			break;
		}
	}
	if (!anyNull) return;

	var sceneCubes = UnityEngine.Object.FindObjectsOfType<CryptoAnswerCube>(true);
	if (sceneCubes == null || sceneCubes.Length == 0) return;

	// Try to fill null slots with scene instances not already referenced
	HashSet<CryptoAnswerCube> referenced = new HashSet<CryptoAnswerCube>();
	foreach (var c in answerCubes) if (c != null) referenced.Add(c);
	int si = 0;
	for (int i = 0; i < answerCubes.Length && si < sceneCubes.Length; i++)
	{
		if (answerCubes[i] == null)
		{
			// find next scene cube not referenced
			while (si < sceneCubes.Length && referenced.Contains(sceneCubes[si])) si++;
			if (si < sceneCubes.Length)
			{
				answerCubes[i] = sceneCubes[si];
				referenced.Add(sceneCubes[si]);
				si++;
			}
		}
	}
}

// --- 追加: テキストと可視状態を確実に復帰させるヘルパー ---
private void EnsureTextVisible(Text txt)
{
	if (txt == null) return;

	// GameObject とコンポーネントを有効化
	if (!txt.gameObject.activeSelf) txt.gameObject.SetActive(true);
	try { txt.enabled = true; } catch { /* コンポーネントに enabled がない場合無視 */ }

	// 透明になっていたら alpha を復帰
	Color c = txt.color;
	if (c.a <= 0.01f)
	{
		c.a = 1f;
		txt.color = c;
	}

	// 親 Canvas / CanvasGroup を有効化
	var parentCanvas = txt.GetComponentInParent<Canvas>(true);
	if (parentCanvas != null && !parentCanvas.gameObject.activeSelf)
	{
		parentCanvas.gameObject.SetActive(true);
	}
	foreach (var cg in txt.GetComponentsInParent<CanvasGroup>(true))
	{
		try
		{
			cg.alpha = 1f;
			cg.interactable = true;
			cg.blocksRaycasts = true;
		}
		catch { }
	}

	// CanvasRenderer の復帰
	foreach (var cr in txt.GetComponentsInChildren<CanvasRenderer>(true))
	{
		try { cr.gameObject.SetActive(true); } catch { }
	}
}

// --- 追加: GameObject とその子供の表示/相互作用コンポーネントを復帰 ---
private void ResetVisualState(GameObject go)
{
	if (go == null) return;

	// GameObject 自体
	if (!go.activeSelf) go.SetActive(true);

	// Renderer 系（MeshRenderer, SkinnedMeshRenderer, SpriteRenderer 等）
	foreach (var r in go.GetComponentsInChildren<Renderer>(true))
	{
		try { r.enabled = true; } catch { }
	}

	// UI Graphic（Text, Image, RawImage 等）
	foreach (var g in go.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
	{
		try { g.enabled = true; } catch { }
	}

	// CanvasRenderer を有効化（UI 表示補助）
	foreach (var cr in go.GetComponentsInChildren<CanvasRenderer>(true))
	{
		try { cr.gameObject.SetActive(true); } catch { }
	}

	// Collider を有効化（選択判定の復帰）
	// 注意: セットアップ中はコライダーの自動有効化を抑止する
	foreach (var col in go.GetComponentsInChildren<Collider>(true))
	{
		try
		{
			if (!suppressColliderEnableDuringSetup) col.enabled = true;
			else col.enabled = false; // 明示的に無効化しておく（安全策）
		}
		catch { }
	}

	// Animator を有効化
	foreach (var anim in go.GetComponentsInChildren<Animator>(true))
	{
		try { anim.enabled = true; } catch { }
	}

	// CanvasGroup の復帰
	foreach (var cg in go.GetComponentsInChildren<CanvasGroup>(true))
	{
		try
		{
			cg.alpha = 1f;
			cg.interactable = true;
			cg.blocksRaycasts = true;
		}
		catch { }
	}

	// 親 Canvas の復帰（念のため）
	var parentCanvas = go.GetComponentInParent<Canvas>(true);
	if (parentCanvas != null && !parentCanvas.gameObject.activeSelf)
	{
		try { parentCanvas.gameObject.SetActive(true); } catch { }
	}
}

// --- 新規追加メソッド: 各 CryptoAnswerCube のランタイム状態を可能な限り復帰させる ---
private void ResetCubeRuntimeState(CryptoAnswerCube cube)
{
	if (cube == null) return;

	GameObject go = cube.gameObject;

	// 1) 共通コンポーネントの復帰
	try { if (!go.activeSelf) go.SetActive(true); } catch { }
	ResetVisualState(go);

	// 2) MonoBehaviour コンポーネントを有効化（スクリプトが無効化されている場合の復帰）
	foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
	{
		try { mb.enabled = true; } catch { }
	}

	// 3) 反射で「使用済み」フラグ等の bool フィールドを false に戻す
	foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
	{
		var t = mb.GetType();
		// よくあるフィールド名パターンを探索して false にする
		foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
		{
			if (f.FieldType == typeof(bool))
			{
				string n = f.Name.ToLower();
				if (n.Contains("used") || n.Contains("selected") || n.Contains("isused") || n.Contains("isselected") || n.Contains("consumed"))
				{
					try { f.SetValue(mb, false); } catch { }
				}
			}
		}
		// また、property もチェック
		foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
		{
			if (!p.CanWrite) continue;
			if (p.PropertyType == typeof(bool))
			{
				string n = p.Name.ToLower();
				if (n.Contains("used") || n.Contains("selected") || n.Contains("consumed"))
				{
					try { p.SetValue(mb, false, null); } catch { }
				}
			}
		}
		// メソッドで復帰可能なら呼ぶ（ResetState, Restore, Initialize 等）
		string[] methodNames = new string[] { "ResetState", "Restore", "Initialize", "Reset", "ClearState", "Refresh" };
		foreach (var mn in methodNames)
		{
			var mi = t.GetMethod(mn, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (mi != null && mi.GetParameters().Length == 0)
			{
				try { mi.Invoke(mb, null); } catch { }
			}
		}
	}

	// 4) Collider等を再有効化（保険）
	foreach (var col in go.GetComponentsInChildren<Collider>(true))
	{
		try
		{
			// セットアップ中はここで有効化しない（StartCurrentQuestionRoutine の終了時に EnableAnswerInteractions が確実に有効化する）
			if (!suppressColliderEnableDuringSetup) col.enabled = true;
			else col.enabled = false;
		}
		catch { }
	}
	// 5) Rigidbody の kinematic を触らないが存在確認（保険）
	foreach (var rb in go.GetComponentsInChildren<Rigidbody>(true))
	{
		try { /* no-op */ } catch { }
	}
}

// 追加: 結果表示・プレイヤーリセット・遅延処理の最小実装（不足していたため追加）
	private void ShowResultMessage(string message, Color color)
	{
		if (resultPanel != null)
			resultPanel.SetActive(true);

		if (resultText != null)
		{
			resultText.text = message;
			try { resultText.color = color; } catch { }
		}

		Debug.Log("ShowResultMessage: " + message);

		// 自動で結果パネルを短時間後に閉じる（必要なら delay を調整）
		StartCoroutine(HideResultPanelAfterDelay(2f));
	}

	private IEnumerator HideResultPanelAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		if (resultPanel != null)
			resultPanel.SetActive(false);
	}

	private void ResetPlayerPosition()
	{
		if (player == null) return;

		Vector3 targetPos = player.position;
		Quaternion targetRot = player.rotation;

		// 基本位置決定
		switch (resetSettings.resetType)
		{
			case ResetPositionType.Custom:
				targetPos = resetSettings.customPosition;
				break;
			case ResetPositionType.Preset:
				switch (resetSettings.presetPosition)
				{
					case PresetPosition.Center: targetPos = new Vector3(0f, 3f, 5f); break;
					case PresetPosition.FarCenter: targetPos = new Vector3(0f, 3f, 10f); break;
					case PresetPosition.LeftSide: targetPos = new Vector3(-5f, 3f, 5f); break;
					case PresetPosition.RightSide: targetPos = new Vector3(5f, 3f, 5f); break;
					case PresetPosition.HighCenter: targetPos = new Vector3(0f, 8f, 5f); break;
					case PresetPosition.StartPosition: targetPos = new Vector3(0f, 1f, 0f); break;
				}
				break;
			case ResetPositionType.Transform:
				if (resetSettings.referenceTransform != null)
				{
					targetPos = resetSettings.referenceTransform.position;
					targetRot = resetSettings.referenceTransform.rotation;
				}
				break;
		}

		// 地面検出/オフセット
		if (resetSettings.useGroundDetection)
		{
			RaycastHit hit;
			float rayStartY = targetPos.y + resetSettings.groundDetectionDistance;
			if (Physics.Raycast(new Vector3(targetPos.x, rayStartY, targetPos.z), Vector3.down, out hit, resetSettings.groundDetectionDistance + 1f))
			{
				targetPos.y = hit.point.y + resetSettings.heightOffset;
			}
			else if (resetSettings.forceHeightOffset)
			{
				targetPos.y += resetSettings.heightOffset;
			}
		}
		else if (resetSettings.forceHeightOffset)
		{
			targetPos.y += resetSettings.heightOffset;
		}

		// 向きリセット
		if (resetSettings.resetRotation)
		{
			targetRot = Quaternion.Euler(resetSettings.resetRotationEuler);
		}

		player.position = targetPos;
		player.rotation = targetRot;
	}

	private IEnumerator HandleIncorrectAnswerDelay()
	{
		// 不正解後の短い遅延後に同じ問題を再表示
		yield return new WaitForSeconds(1.5f);
		StartCurrentQuestion();
	}

	private IEnumerator StartNextQuestionDelay()
	{
		// 正解後の短い遅延後に次の問題を開始
		yield return new WaitForSeconds(1.0f);
		StartCurrentQuestion();
	}

	// --- 追加: 毎フレームでタイマーを減算し表示更新、タイムアップで結果表示へ ---
	private void Update()
	{
		if (!isGameActive) return;

		if (gameTimer > 0f)
		{
			gameTimer -= Time.deltaTime;
			if (gameTimer <= 0f)
			{
				gameTimer = 0f;
				isGameActive = false;
				UpdateTimerText();
				Debug.Log("タイムアップ: ゲーム終了処理を開始します");

				// 変更: タイムアップ時の残問題を不正解として扱う処理を呼ぶ
				HandleTimeUp();

				ShowFinalScore();
			}
			else
			{
				UpdateTimerText();
			}
		}
	}

	// --- 新規追加: タイムアップ時に残っている全てのステップを不正解としてカウントし、進捗/スコアに反映 ---
	private void HandleTimeUp()
	{
		// 必要情報がなければ何もしない
		if (currentGameSet == null || currentGameSet.Length == 0)
		{
			Debug.LogWarning("HandleTimeUp: 現在のゲームセットが未設定のため残り不正解処理をスキップします");
			return;
		}

		// 全体のステップ数（各暗号方式ごとの問題数）を算出
		int totalStepsInSet = 0;
		for (int i = 0; i < currentGameSet.Length; i++)
		{
			try
			{
				int cnt = CryptoQuestionDatabase.GetQuestionCount(currentGameSet[i]);
				totalStepsInSet += Mathf.Max(0, cnt);
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"HandleTimeUp: GetQuestionCount 取得中に例外 (index {i}): {ex.Message}");
			}
		}

		// 既に回答済み（正誤含む）のステップ数は totalQuestions
		int remaining = Mathf.Max(0, totalStepsInSet - totalQuestions);
		if (remaining <= 0)
		{
			Debug.Log("HandleTimeUp: 未回答はありません");
			return;
		}

		Debug.Log($"HandleTimeUp: 未回答ステップを不正解としてカウントします（残り: {remaining}）");

		// 進捗トラッカーへ各残ステップを不正解として通知する
		if (progressTracker != null)
		{
			// 残りのステップを currentQuestionIndex/currentStepIndex から順に列挙してトラッキング
			for (int qi = currentQuestionIndex; qi < currentGameSet.Length; qi++)
			{
				CryptoType type = currentGameSet[qi];
				int startStep = (qi == currentQuestionIndex) ? currentStepIndex : 0;
				int cnt = 0;
				try { cnt = Mathf.Max(0, CryptoQuestionDatabase.GetQuestionCount(type)); } catch { cnt = 0; }

				for (int s = startStep; s < cnt; s++)
				{
					try
					{
						progressTracker.OnIncorrectAnswer(type);
					}
					catch (Exception ex)
					{
						Debug.LogWarning($"HandleTimeUp: progressTracker.OnIncorrectAnswer で例外 ({type}): {ex.Message}");
					}
				}
			}
		}

		// スコアに不正解分をまとめて適用（負にならないように clamp）
		int totalPenalty = pointsPerIncorrect * remaining; // pointsPerIncorrect は負の値の想定
		currentScore = Mathf.Max(0, currentScore + totalPenalty);

		// 総問題数を増やす（正解数は変わらない）
		totalQuestions += remaining;

		// スコア表示更新
		UpdateScoreDisplay();

		Debug.Log($"HandleTimeUp: 不正解で減点適用: {totalPenalty} (残り{remaining}) -> currentScore={currentScore}, totalQuestions={totalQuestions}");
	}

	// --- 追加: タイマー表示更新ヘルパー ---
	private void UpdateTimerText()
	{
		if (timerText == null) return;
		timerText.text = FormatTime(gameTimer);
	}

	// --- 追加: 秒→mm:ss 変換 ---
	private string FormatTime(float seconds)
	{
		int totalSec = Mathf.Max(0, Mathf.CeilToInt(seconds));
		int minutes = totalSec / 60;
		int secs = totalSec % 60;
		return string.Format("{0:00}:{1:00}", minutes, secs);
	}
} // class end