using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;

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
    private int pointsPerIncorrect = -2;    // 不正解時の減点
    
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
        currentScore = 68; // 7*10 - 3*2 = 68点の例
        
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
                bodyRT.sizeDelta = new Vector2(0f, 200f);

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
                        bodyRT.sizeDelta = new Vector2(0f, 200f);
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
        if (currentGameSet == null || currentQuestionIndex >= currentGameSet.Length)
        {
            Debug.Log("ゲームセット完了");
            ShowFinalScore();
            return;
        }
        
        CryptoType currentType = currentGameSet[currentQuestionIndex];

        // 公開鍵暗号方式 or ハイブリッド暗号方式の1問目開始前にリセット処理
        if ((currentType == CryptoType.PublicKey || currentType == CryptoType.Hybrid) && currentStepIndex == 0 && animationManager != null)
        {
            // DataCubeを(-5,3,10)に移動（ワールド座標で確実に移動）
            if (animationManager.dataCube != null)
            {
                animationManager.ForceSetDataCubePosition(new Vector3(-5f, 3f, 10f));
            }
            // 全ての鍵を非表示
            animationManager.HideAllKeys();
            Debug.Log("公開鍵/ハイブリッド暗号方式1問目開始前: 全ての鍵を非表示");
        }

        var question = CryptoQuestionDatabase.GetQuestion(currentType, currentStepIndex);
        
        if (question == null)
        {
            Debug.LogError($"問題データが見つかりません: {currentType}, ステップ {currentStepIndex}");
            return;
        }
        
        // UI更新
        if (questionText != null)
        {
            questionText.text = question.questionText;
        }
        
        // 暗号方式に応じたアニメーション再生
        PlayCryptoTypeAnimation(currentType);
        
        // 回答キューブをランダム化して設定
        SetRandomizedAnswers(question);
        
        Debug.Log($"問題開始: {currentType}, ステップ: {currentStepIndex}");
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
            explanationHeaderText.gameObject.SetActive(true);
            // 正解時は末尾に改行を入れてヘッダーの下に余白を作る
            explanationHeaderText.text = isCorrect ? "✅ 正解！\n" : "❌ 不正解";
            // 明示的に中央寄せ／太字を保証
            explanationHeaderText.alignment = TextAnchor.MiddleCenter;
            explanationHeaderText.fontStyle = FontStyle.Bold;
            // ensure header is above body in hierarchy
            explanationHeaderText.transform.SetAsLastSibling();
        }

        // 本文は選択回答＋解説のみ（GetExplanationBody を使用）
        string body = GetExplanationBody(question, isCorrect, selectedAnswerIndex);
        explanationText.text = body;
        explanationText.gameObject.SetActive(true);

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
        
        // 進度スライダーの更新
        if (progressSliders != null && progressSliders.Length >= 3)
        {
            progressSliders[0].value = progressTracker.GetProgress(CryptoType.SymmetricKey);
            progressSliders[1].value = progressTracker.GetProgress(CryptoType.PublicKey);
            progressSliders[2].value = progressTracker.GetProgress(CryptoType.Hybrid);
        }
        
        // 進度ラベルの更新
        if (progressLabels != null && progressLabels.Length >= 3)
        {
            progressLabels[0].text = "共通鍵暗号: " + progressTracker.GetProgress(CryptoType.SymmetricKey).ToString("F1") + "%";
            progressLabels[1].text = "公開鍵暗号: " + progressTracker.GetProgress(CryptoType.PublicKey).ToString("F1") + "%";
            progressLabels[2].text = "ハイブリッド暗号: " + progressTracker.GetProgress(CryptoType.Hybrid).ToString("F1") + "%";
        }
        
        Debug.Log("進度表示更新完了");
    }

    /// <summary>
    /// 回答キューブにランダムな順序で回答を設定
    /// </summary>
    /// <param name="question">設定する問題データ</param>
    private void SetRandomizedAnswers(CryptoQuestion question)
    {
        // 入力検証
        if (question == null)
        {
            Debug.LogError("[SetRandomizedAnswers] ❌ 問題データがnullです");
            return;
        }

        if (question.answers == null || question.answers.Length == 0)
        {
            Debug.LogError("[SetRandomizedAnswers] ❌ 回答データが無効です（null または空の配列）");
            return;
        }

        if (question.correctAnswerIndex < 0 || question.correctAnswerIndex >= question.answers.Length)
        {
            Debug.LogError("[SetRandomizedAnswers] ❌ 正解インデックスが無効です: " + question.correctAnswerIndex + " (回答数: " + question.answers.Length + ")");
            return;
        }
        
        if (answerCubes == null)
        {
            Debug.LogError("[SetRandomizedAnswers] ❌ 回答キューブ配列がnullです");
            return;
        }

        if (answerCubes.Length < question.answers.Length)
        {
            Debug.LogError("[SetRandomizedAnswers] ❌ 回答キューブが不足しています。必要: " + question.answers.Length + ", 利用可能: " + answerCubes.Length);
            return;
        }

        // null チェック
        int validCubeCount = 0;
        for (int i = 0; i < question.answers.Length && i < answerCubes.Length; i++)
        {
            if (answerCubes[i] != null)
            {
                validCubeCount++;
            }
        }

        if (validCubeCount < question.answers.Length)
        {
            Debug.LogError("[SetRandomizedAnswers] ❌ 有効な回答キューブが不足しています。必要: " + question.answers.Length + ", 有効: " + validCubeCount);
            return;
        }

        if (showAnswerRandomizationDebug)
        {
            Debug.Log("[SetRandomizedAnswers] 🎯 回答ランダム化開始");
            Debug.Log("[SetRandomizedAnswers] 問題: " + question.questionText);
            Debug.Log("[SetRandomizedAnswers] 回答数: " + question.answers.Length + ", キューブ数: " + answerCubes.Length);
        }
        
        // テスト用の固定シード設定
        if (useFixedRandomSeed)
        {
            UnityEngine.Random.InitState(fixedRandomSeed);
            if (showAnswerRandomizationDebug)
            {
                Debug.Log("[SetRandomizedAnswers] 🔧 固定シードを使用: " + fixedRandomSeed);
            }
        }
        
        // 回答の順序をランダム化するためのインデックス配列を作成
        int[] answerIndices = new int[question.answers.Length];
        for (int i = 0; i < answerIndices.Length; i++)
        {
            answerIndices[i] = i;
        }
        
        // Fisher-Yates shuffle でランダム化
        for (int i = answerIndices.Length - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            int temp = answerIndices[i];
            answerIndices[i] = answerIndices[randomIndex];
            answerIndices[randomIndex] = temp;
        }
        
        if (showAnswerRandomizationDebug)
        {
            Debug.Log("[SetRandomizedAnswers] 回答順序: [" + string.Join(", ", answerIndices) + "]");
            Debug.Log("[SetRandomizedAnswers] 元の回答リスト: [" + string.Join(", ", question.answers) + "]");
            Debug.Log("[SetRandomizedAnswers] 正解インデックス: " + question.correctAnswerIndex + " (正解: '" + question.answers[question.correctAnswerIndex] + "')");
        }
        
        // ランダム化された順序でキューブに回答を設定
        int correctCubePosition = -1; // 正解が配置されたキューブの位置を記録
        
        for (int cubeIndex = 0; cubeIndex < question.answers.Length && cubeIndex < answerCubes.Length; cubeIndex++)
        {
            if (answerCubes[cubeIndex] != null)
            {
                int answerIndex = answerIndices[cubeIndex];
                string answerText = question.answers[answerIndex];
                bool isCorrect = (answerIndex == question.correctAnswerIndex);
                
                answerCubes[cubeIndex].SetAnswerText(answerText);
                answerCubes[cubeIndex].SetAnswerIndex(answerIndex);
                answerCubes[cubeIndex].SetActive(true);
                
                if (isCorrect)
                {
                    correctCubePosition = cubeIndex;
                }
                
                if (showAnswerRandomizationDebug)
                {
                    Vector3 cubePos = answerCubes[cubeIndex].transform.position;
                    Debug.Log("キューブ " + cubeIndex + " 設定完了: '" + answerText + "' (元インデックス: " + answerIndex + ") " + (isCorrect ? "✅正解" : "❌") + " - 位置: " + cubePos);
                }
            }
            else
            {
                Debug.LogError("Answer Cube " + cubeIndex + " が null です");
            }
        }
        
        // 使用しないキューブを非表示
        for (int i = question.answers.Length; i < answerCubes.Length; i++)
        {
            if (answerCubes[i] != null)
            {
                answerCubes[i].SetActive(false);
                if (showAnswerRandomizationDebug)
                {
                    Debug.Log("キューブ " + i + " を非表示にしました");
                }
            }
        }
        
        if (showAnswerRandomizationDebug)
        {
            string correctAnswerText = question.answers[question.correctAnswerIndex];
            Debug.Log("[SetRandomizedAnswers] ✅ 回答ランダム化完了");
            Debug.Log("[SetRandomizedAnswers] 正解: 「" + correctAnswerText + "」がキューブ " + correctCubePosition + " に配置されました");
            Debug.Log("[SetRandomizedAnswers] プレイヤーは位置を覚えられません - 毎回ランダムです！");
        }
    }

    /// <summary>
    /// 結果メッセージの表示（正解・不正解）
    /// </summary>
    private void ShowResultMessage(string message, Color color)
    {
        // ResultText は UI 上で使わない運用に変更しました。
        // 必要ならログ出力のみ行う（実際の表示は解説パネル側で行う）
        Debug.Log($"結果メッセージ(非表示運用): {message}");
    }
    
    /// <summary>
    /// プレイヤー位置のリセット
    /// </summary>
    private void ResetPlayerPosition()
    {
        if (player == null)
        {
            Debug.LogWarning("プレイヤーオブジェクトが設定されていません");
            return;
        }
        
        Vector3 resetPos = resetSettings.customPosition;
        
        switch (resetSettings.resetType)
        {
            case ResetPositionType.Custom:
                resetPos = resetSettings.customPosition;
                break;
            case ResetPositionType.Preset:
                resetPos = GetPresetPosition(resetSettings.presetPosition);
                break;
            case ResetPositionType.Transform:
                if (resetSettings.referenceTransform != null)
                {
                    resetPos = resetSettings.referenceTransform.position;
                }
                else
                {
                    Debug.LogWarning("参照Transformが設定されていません。カスタム位置を使用します");
                    resetPos = resetSettings.customPosition;
                }
                break;
        }
        
        // 高さ調整
        if (resetSettings.useGroundDetection)
        {
            RaycastHit hit;
            Vector3 rayStart = new Vector3(resetPos.x, resetPos.y + 10, resetPos.z);
            if (Physics.Raycast(rayStart, Vector3.down, out hit, resetSettings.groundDetectionDistance))
            {
                resetPos.y = hit.point.y + resetSettings.heightOffset;
                Debug.Log("地面検出成功: 高さ " + resetPos.y);
            }
            else
            {
                Debug.LogWarning("地面検出失敗。設定位置をそのまま使用します");
                if (resetSettings.forceHeightOffset)
                {
                    resetPos.y += resetSettings.heightOffset;
                }
            }
        }
        else if (resetSettings.forceHeightOffset)
        {
            resetPos.y += resetSettings.heightOffset;
        }
        
        // CharacterControllerの場合は一時的に無効化
        CharacterController cc = player.GetComponent<CharacterController>();
        bool ccWasEnabled = false;
        if (cc != null)
        {
            ccWasEnabled = cc.enabled;
            cc.enabled = false;
        }
        
        // 位置をリセット
        player.position = resetPos;
        
        // 向きリセット
        if (resetSettings.resetRotation)
        {
            player.rotation = Quaternion.Euler(resetSettings.resetRotationEuler);
        }
        
        // CharacterControllerを再有効化
        if (cc != null && ccWasEnabled)
        {
            cc.enabled = true;
        }
        
        Debug.Log("プレイヤー位置リセット完了: " + resetPos);
    }
    
    /// <summary>
    /// プリセット位置の取得
    /// </summary>
    private Vector3 GetPresetPosition(PresetPosition preset)
    {
        switch (preset)
        {
            case PresetPosition.Center: return new Vector3(0, 3, 5);
            case PresetPosition.FarCenter: return new Vector3(0, 3, 10);
            case PresetPosition.LeftSide: return new Vector3(-5, 3, 5);
            case PresetPosition.RightSide: return new Vector3(5, 3, 5);
            case PresetPosition.HighCenter: return new Vector3(0, 8, 5);
            case PresetPosition.StartPosition: return new Vector3(0, 1, 0);
            default: return new Vector3(0, 3, 5);
        }
    }
    
    /// <summary>
    /// 次の問題開始の遅延
    /// </summary>
    private IEnumerator StartNextQuestionDelay()
    {
        yield return new WaitForSeconds(1.0f);
        StartCurrentQuestion();
    }

    /// <summary>
    /// 回答が選択された時の処理（CryptoAnswerCubeから呼ばれる）
    /// </summary>
    public void OnAnswerSelected(int selectedAnswerIndex)
    {
        if (!isGameActive)
        {
            Debug.LogWarning("ゲームが非アクティブ状態のため、回答選択を無視します");
            return;
        }
        
        if (currentGameSet == null || currentQuestionIndex >= currentGameSet.Length)
        {
            Debug.LogError("OnAnswerSelected: 無効なゲーム状態です");
            return;
        }
        
        // 現在の問題情報を取得
        CryptoType currentType = currentGameSet[currentQuestionIndex];
        var question = CryptoQuestionDatabase.GetQuestion(currentType, currentStepIndex);
        
        if (question == null)
        {
            Debug.LogError("OnAnswerSelected: 問題データの取得に失敗しました");
            return;
        }
        
        Debug.Log("[OnAnswerSelected] 選択された回答: インデックス " + selectedAnswerIndex);
        Debug.Log("[OnAnswerSelected] 正解インデックス: " + question.correctAnswerIndex);
        Debug.Log("[OnAnswerSelected] 現在の問題: " + currentType + ", ステップ: " + currentStepIndex);
        
        // 回答をチェックして処理
        bool isCorrect = (selectedAnswerIndex == question.correctAnswerIndex);
        
        if (isCorrect)
        {
            Debug.Log("✅ 正解!");
            OnCorrectAnswer(selectedAnswerIndex);
        }
        else
        {
            Debug.Log("❌ 不正解...");
            OnIncorrectAnswerSelected(selectedAnswerIndex);
        }
    }

    /// <summary>
    /// 不正解後の遅延処理
    /// </summary>
    private IEnumerator HandleIncorrectAnswerDelay()
    {
        // 2秒間待機
        yield return new WaitForSeconds(2.0f);
        
        // 同じ問題を再表示する場合
        if (currentGameSet != null && currentQuestionIndex < currentGameSet.Length)
        {
            Debug.Log("[HandleIncorrectAnswerDelay] 同じ問題を再表示します");
            StartCurrentQuestion(); // 同じ問題を再表示（答えは再ランダム化される）
        }
    }

    /// <summary>
    /// プレイヤー入力を無効化
    /// </summary>
    private void DisablePlayerInput()
    {
        if (playerInput != null)
        {
            playerInput.SetInputEnabled(false);
            Debug.Log("プレイヤー入力を無効化しました");
        }
        else
        {
            Debug.LogWarning("PlayerInputコンポーネントが見つかりません - 入力制御をスキップ");
        }
    }
    
    /// <summary>
    /// プレイヤー入力を有効化
    /// </summary>
    private void EnablePlayerInput()
    {
        if (playerInput != null)
        {
            playerInput.SetInputEnabled(true);
            Debug.Log("プレイヤー入力を有効化しました");
        }
        else
        {
            Debug.LogWarning("PlayerInputコンポーネントが見つかりません - 入力制御をスキップ");
        }
    }

    // 注意: ここに残っていた不完全な #if UNITY_EDITOR ブロック（MenuItem 定義）を削除しました。
    //       エディタ専用のメニュー追加が必要な場合は、Assets/.../Editor フォルダに別ファイルを作成してください。
}