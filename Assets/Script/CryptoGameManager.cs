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
    [Tooltip("出題する暗号方式を選択してください")]
    public bool enableSymmetricKey = true;  // 共通鍵暗号
    public bool enablePublicKey = true;     // 公開鍵暗号
    public bool enableHybrid = true;        // ハイブリッド暗号
    
    [Header("UI References")]
    public Text questionText;
    public Text progressText;
    public Text timerText;
    public Text explanationText;
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
    
    [Header("Player Management")]
    [Tooltip("プレイヤーオブジェクト（正解時に位置をリセット）")]
    public Transform player;
    
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

    [Header("Progress Animation Settings")]
    [Tooltip("進捗スライダーのアニメーション時間")]
    public float progressAnimationDuration = 0.5f;
    
    [Tooltip("進捗増加時の色（正解時）")]
    public Color progressIncreaseColor = Color.green;
    
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
                animationManager = FindObjectOfType<CryptoAnimationManager>();
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
                    CharacterController characterController = FindObjectOfType<CharacterController>();
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
        // UIボタンがある場合はカーソルを表示、3Dのみの場合は非表示
        if (answerButtons != null && answerButtons.Length > 0)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        if (isGameActive)
        {
            gameTimer -= Time.deltaTime;
            UpdateTimerDisplay();
            
            if (gameTimer <= 0)
            {
                EndGameSet();
            }
        }
    }
    
    public void StartNewGameSet()
    {
        // 理解度ゲージを新しいゲーム用にリセット
        if (progressTracker != null)
        {
            progressTracker.ResetProgressForNewGame();
            Debug.Log("CryptoGameManager: 理解度ゲージをリセットしました");
        }
        else
        {
            Debug.LogWarning("CryptoGameManager: ProgressTrackerが見つかりません");
        }
        
        // 有効化された暗号方式を確認
        List<CryptoType> availableTypes = GetEnabledCryptoTypes();
        if (availableTypes.Count == 0)
        {
            Debug.LogWarning("有効な暗号方式がありません。すべての暗号方式を有効化します。");
            enableSymmetricKey = true;
            enablePublicKey = true;
            enableHybrid = true;
            availableTypes = GetEnabledCryptoTypes();
        }
        
        // questionsPerSetを有効な暗号方式数に合わせて調整
        questionsPerSet = availableTypes.Count;
        
        // ランダムに暗号方式の順序を決定
        currentGameSet = GenerateRandomCryptoSet();
        currentQuestionIndex = 0;
        currentStepIndex = 0;
        gameTimer = gameSetDuration;
        isGameActive = true;
        correctAnswers = 0;
        totalQuestions = 0;
        currentScore = 0;
        
        // スコア表示の初期化
        UpdateScoreDisplay();
        
        // 3Dオブジェクトをリセット
        if (animationManager != null)
        {
            animationManager.ResetAllObjects();
        }
        
        Debug.Log($"[StartGame] ゲーム開始時の状態 - currentQuestionIndex: {currentQuestionIndex}, currentStepIndex: {currentStepIndex}");
        
        UpdateProgressDisplay();
        StartCurrentQuestion();
    }
    
    private List<CryptoType> GetEnabledCryptoTypes()
    {
        List<CryptoType> types = new List<CryptoType>();
        
        if (enableSymmetricKey) types.Add(CryptoType.SymmetricKey);
        if (enablePublicKey) types.Add(CryptoType.PublicKey);
        if (enableHybrid) types.Add(CryptoType.Hybrid);
        
        return types;
    }
    
    private CryptoType[] GenerateRandomCryptoSet()
    {
        List<CryptoType> orderedTypes = new List<CryptoType>();
        
        // 固定順序で暗号方式を追加: 1.Symmetric → 2.Public Key → 3.Hybrid
        if (enableSymmetricKey) orderedTypes.Add(CryptoType.SymmetricKey);
        if (enablePublicKey) orderedTypes.Add(CryptoType.PublicKey);
        if (enableHybrid) orderedTypes.Add(CryptoType.Hybrid);
        
        if (orderedTypes.Count == 0)
        {
            Debug.LogError("有効な暗号方式がありません！");
            return new CryptoType[0];
        }
        
        Debug.Log($"固定順序で暗号方式を設定: {string.Join(" → ", orderedTypes)}");
        return orderedTypes.ToArray();
    }
    
    private void StartCurrentQuestion()
    {
        Debug.Log($"[StartCurrentQuestion呼び出し] currentQuestionIndex: {currentQuestionIndex}, currentStepIndex: {currentStepIndex}, questionsPerSet: {questionsPerSet}");
        
        if (currentQuestionIndex >= questionsPerSet)
        {
            EndGameSet();
            return;
        }

        CryptoType currentType = currentGameSet[currentQuestionIndex];
        
        Debug.Log($"[StartCurrentQuestion] 暗号タイプ: {currentType}, ステップ: {currentStepIndex}");
        
        // 1問目の場合は鍵を非表示にして問題のみ表示
        // 2問目以降で初めて鍵を表示
        if (animationManager != null)
        {
            if (currentStepIndex == 0)
            {
                // 1問目：鍵を非表示にする（暗号方式が変わった時の初期化）
                Debug.Log($"暗号方式 {currentType} の1問目：鍵を非表示に設定");
                animationManager.HideAllKeys();
            }
            else if (currentStepIndex == 1)
            {
                // 2問目：暗号方式に応じた鍵を表示
                Debug.Log($"暗号方式 {currentType} の2問目：鍵を表示 (ステップ: {currentStepIndex})");
                animationManager.ShowKeysForCryptoType(currentType);
            }
            // 3問目以降は鍵が既に表示されているので何もしない
        }

        CryptoQuestion question = CryptoQuestionDatabase.GetQuestion(currentType, currentStepIndex);
        
        DisplayQuestion(question);
        UpdateProgressText();
        UpdateProgressDisplay(); // スライダーとラベルも更新
    }
    
    private void DisplayQuestion(CryptoQuestion question)
    {
        if (question == null)
        {
            Debug.LogError("CryptoQuestion が null です");
            return;
        }
        
        if (questionText != null)
        {
            questionText.text = question.questionText;
        }
        else
        {
            Debug.LogWarning("Question Text が割り当てられていません");
        }
        
        // 3D回答キューブの設定（優先）
        if (answerCubes != null && answerCubes.Length >= 4)
        {
            Debug.Log($"回答キューブ設定開始: {answerCubes.Length}個のキューブ");
            
            for (int i = 0; i < answerCubes.Length && i < question.answers.Length; i++)
            {
                if (answerCubes[i] != null)
                {
                    answerCubes[i].SetAnswerText(question.answers[i]);
                    answerCubes[i].SetAnswerIndex(i);
                    answerCubes[i].SetActive(true);
                    Debug.Log($"キューブ {i} 設定完了: {question.answers[i]}");
                }
                else
                {
                    Debug.LogError($"Answer Cube {i} が null です");
                }
            }
            
            // 使用しないキューブを非表示
            for (int i = question.answers.Length; i < answerCubes.Length; i++)
            {
                if (answerCubes[i] != null)
                {
                    answerCubes[i].SetActive(false);
                }
            }
        }
        // UIボタンの設定（フォールバック）
        else if (answerButtons != null && answerButtons.Length >= 4)
        {
            Debug.Log("UIボタンを使用してフォールバック表示");
            
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i < question.answers.Length)
                {
                    if (answerButtons[i] != null)
                    {
                        answerButtons[i].gameObject.SetActive(true);
                        Text buttonText = answerButtons[i].GetComponentInChildren<Text>();
                        if (buttonText != null)
                        {
                            buttonText.text = question.answers[i];
                        }
                        
                        // クリックイベントを設定
                        int answerIndex = i;
                        answerButtons[i].onClick.RemoveAllListeners();
                        answerButtons[i].onClick.AddListener(() => OnAnswerSelected(answerIndex));
                    }
                    else
                    {
                        Debug.LogError($"Answer Button {i} が null です");
                    }
                }
                else
                {
                    if (answerButtons[i] != null)
                    {
                        answerButtons[i].gameObject.SetActive(false);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("回答システムが設定されていません。Answer Cubes または Answer Buttons を設定してください。");
        }
    }
    
    public void OnAnswerSelected(int answerIndex)
    {
        // ゲーム状態の詳細チェックとデバッグ情報
        Debug.Log($"[ゲーム状態チェック] currentGameSet: {(currentGameSet != null ? "存在" : "null")}");
        Debug.Log($"[ゲーム状態チェック] currentGameSet.Length: {(currentGameSet?.Length ?? 0)}");
        Debug.Log($"[ゲーム状態チェック] currentQuestionIndex: {currentQuestionIndex}");
        Debug.Log($"[ゲーム状態チェック] isGameActive: {isGameActive}");
        
        // ゲーム状態が無効な場合は強制初期化
        if (currentGameSet == null || currentQuestionIndex >= currentGameSet.Length || !isGameActive)
        {
            Debug.LogWarning("ゲーム状態が無効です。強制的に初期化を実行します。");
            
            // 強制初期化
            StartNewGameSet();
            
            // 初期化後も状態が無効な場合はエラー
            if (currentGameSet == null || currentQuestionIndex >= currentGameSet.Length)
            {
                Debug.LogError("初期化後もゲーム状態が無効です。ゲーム設定を確認してください。");
                return;
            }
        }
        
        CryptoType currentType = currentGameSet[currentQuestionIndex];
        CryptoQuestion question = CryptoQuestionDatabase.GetQuestion(currentType, currentStepIndex);
        
        // 詳細なデバッグログを追加
        Debug.Log($"[判定詳細] 暗号タイプ: {currentType}, ステップ: {currentStepIndex}");
        Debug.Log($"[判定詳細] 回答選択: {answerIndex}, 正解: {question.correctAnswerIndex}");
        Debug.Log($"[判定詳細] 選択された回答: {(answerIndex < question.answers.Length ? question.answers[answerIndex] : "範囲外")}");
        Debug.Log($"[判定詳細] 正解の回答: {(question.correctAnswerIndex < question.answers.Length ? question.answers[question.correctAnswerIndex] : "範囲外")}");
        
        // 配列範囲チェックを追加
        if (answerIndex < 0 || answerIndex >= question.answers.Length)
        {
            Debug.LogError($"無効な回答インデックス: {answerIndex}, 回答数: {question.answers.Length}");
            return;
        }
        
        if (question.correctAnswerIndex < 0 || question.correctAnswerIndex >= question.answers.Length)
        {
            Debug.LogError($"無効な正解インデックス: {question.correctAnswerIndex}, 回答数: {question.answers.Length}");
            return;
        }
        
        totalQuestions++;
        
        // 判定処理を明確に分離
        bool isCorrect = (answerIndex == question.correctAnswerIndex);
        Debug.Log($"[最終判定] 結果: {(isCorrect ? "正解" : "不正解")}");
        
        // スコア処理
        if (isCorrect)
        {
            currentScore += pointsPerCorrect;
            Debug.Log($"✅ 正解! スコア: {currentScore}点 (+{pointsPerCorrect}点)");
        }
        else
        {
            currentScore += pointsPerIncorrect; // pointsPerIncorrectは負の値
            Debug.Log($"❌ 不正解! スコア: {currentScore}点 ({pointsPerIncorrect}点)");
        }
        
        // スコア表示を更新
        UpdateScoreDisplay();
        // 即座にフィードバックを表示
        if (questionText != null)
        {
            if (isCorrect)
            {
                questionText.text = "✅ 正解！";
                questionText.color = Color.green;
                
                // 正解時：3D演出と転送システムを実行
                if (animationManager != null)
                {
                    // アニメーションタイプに応じて適切なメソッドを呼び出し
                    string animationType = question.animationType;
                    
                    if (animationType == "create_keypair_b")
                    {
                        // エリアBでの鍵ペア生成（公開鍵暗号方式の最初の問題）
                        animationManager.CreateKeyPairAtB();
                    }
                    else
                    {
                        // 従来のアニメーション
                        animationManager.PlayCorrectAnswerAnimation(question);
                    }
                    
                    // 適切なタイミングで転送を実行
                    StartCoroutine(DelayedTransferExecution(currentType, currentStepIndex));
                }
            }
            else
            {
                questionText.text = "❌ 不正解";
                questionText.color = Color.red;
            }
        }
        
        if (isCorrect)
        {
            correctAnswers++;
            StartCoroutine(DelayedCorrectAnswer());
        }
        else
        {
            Debug.Log($"❌ 不正解 - answerIndex: {answerIndex}, 正解: {question.correctAnswerIndex}");
            
            // explanationsの配列範囲チェックを追加
            string explanation = "";
            
            Debug.Log($"🔍 解説取得開始 - answerIndex: {answerIndex}");
            Debug.Log($"   - question.explanations: {(question.explanations != null ? "存在" : "null")}");
            Debug.Log($"   - explanations配列長: {(question.explanations?.Length ?? 0)}");
            
            if (question.explanations != null && answerIndex < question.explanations.Length)
            {
                explanation = question.explanations[answerIndex];
                Debug.Log($"✅ 直接解説取得成功: '{explanation}'");
                Debug.Log($"   - 解説長: {explanation?.Length ?? 0}");
                Debug.Log($"   - 解説内容詳細: '{explanation}'");
            }
            else
            {
                Debug.Log($"⚠️ 直接解説取得失敗、フォールバックを使用");
                
                // フォールバック: 正解の解説を使用
                if (question.explanations != null && question.correctAnswerIndex < question.explanations.Length)
                {
                    string correctExplanation = question.explanations[question.correctAnswerIndex];
                    explanation = $"正解は「{question.answers[question.correctAnswerIndex]}」です。\n\n{correctExplanation}";
                    Debug.Log($"📝 正解解説使用: '{explanation}'");
                    Debug.Log($"   - 正解解説の元テキスト: '{correctExplanation}'");
                }
                else
                {
                    explanation = $"正解は「{question.answers[question.correctAnswerIndex]}」です。\n\nこの問題について再度考えてみましょう。";
                    Debug.LogWarning($"⚠️ 解説が見つからないため、デフォルト解説使用");
                }
            }
            
            Debug.Log($"🎯 最終的な解説: '{explanation}'");
            Debug.Log($"   - 最終解説長: {explanation?.Length ?? 0}");
            Debug.Log($"   - 最終解説null確認: {explanation == null}");
            
            // 間違えた場合：ゲームがアクティブなら同じ問題を再出題
            if (isGameActive)
            {
                StartCoroutine(RetryCurrentQuestion(explanation));
            }
            else
            {
                Debug.Log("ゲーム終了のため、RetryCurrentQuestionをスキップします");
            }
        }
    }
    
    /// <summary>
    /// アニメーション完了後に転送を実行
    /// </summary>
    private IEnumerator DelayedTransferExecution(CryptoType cryptoType, int stepIndex)
    {
        // アニメーション完了を待つ
        yield return new WaitForSeconds(2f);
        
        // 転送システムを実行
        if (animationManager != null)
        {
            animationManager.ExecuteCryptoTransfer(cryptoType, stepIndex);
        }
    }
    
    /// <summary>
    /// コルーチンで結果を返すためのヘルパークラス
    /// </summary>
    public class CoroutineResult<T>
    {
        public T Result { get; set; }
        public bool IsCompleted { get; set; }
        
        public CoroutineResult()
        {
            IsCompleted = false;
        }
    }

    private IEnumerator RetryCurrentQuestion(string explanation)
    {
        Debug.Log($"[RetryCurrentQuestion] 開始 - currentQuestionIndex: {currentQuestionIndex}, currentStepIndex: {currentStepIndex}");
        Debug.Log($"[RetryCurrentQuestion] 受け取った解説: '{explanation}'");
        Debug.Log($"[RetryCurrentQuestion] 解説長: {explanation?.Length ?? 0}");
        Debug.Log($"[RetryCurrentQuestion] 解説null確認: {explanation == null}");
        
        // ゲームが非アクティブな場合は静かに終了
        if (!isGameActive)
        {
            Debug.Log("[RetryCurrentQuestion] ゲームが既に終了しているため、処理を中断します");
            yield break;
        }
        
        // ゲーム状態の詳細確認
        bool gameStateValid = true;
        string invalidReason = "";
        
        if (currentGameSet == null)
        {
            gameStateValid = false;
            invalidReason = "currentGameSetがnull";
        }
        else if (currentQuestionIndex >= currentGameSet.Length)
        {
            gameStateValid = false;
            invalidReason = $"currentQuestionIndex({currentQuestionIndex}) >= currentGameSet.Length({currentGameSet.Length})";
        }
        else if (currentQuestionIndex < 0)
        {
            gameStateValid = false;
            invalidReason = $"currentQuestionIndex({currentQuestionIndex}) < 0";
        }
        else if (!isGameActive)
        {
            gameStateValid = false;
            invalidReason = "ゲームが非アクティブ";
        }
        
        if (!gameStateValid)
        {
            Debug.LogError($"RetryCurrentQuestion: 無効なゲーム状態 - {invalidReason}");
            
            // すべての問題が完了した場合
            if (currentQuestionIndex >= currentGameSet?.Length)
            {
                Debug.Log("全問題完了のため、ゲーム終了処理を実行");
                yield return StartCoroutine(EndGame());
            }
            else
            {
                Debug.Log("ゲーム状態をリセットしています...");
                yield return StartCoroutine(ForceGameReset());
            }
            yield break;
        }

        // プレイヤーの位置をリセット（解説表示より先に実行）
        StartCoroutine(ResetPlayerPosition(GetPlayerResetPosition()));
        Debug.Log("プレイヤー位置リセット開始 - 解説表示前に実行");
        
        Debug.Log($"[解説表示開始] 解説内容: '{explanation}'");
        
        // キャンバス上の専用解説パネル表示を使用
        yield return StartCoroutine(ShowExplanationOnCanvas(explanation));
        
        // UI色をリセット
        if (questionText != null)
        {
            questionText.color = Color.white;
        }
        
        // 再度安全性をチェックしてから問題を再表示
        if (currentGameSet == null || currentQuestionIndex >= currentGameSet.Length || currentQuestionIndex < 0)
        {
            Debug.LogError("問題再表示時にもゲーム状態が無効です。ゲームを再開始します。");
            yield return StartCoroutine(ForceGameReset());
            yield break;
        }
        
        // 同じ問題を再表示（ステップを進めない）
        CryptoType currentType = currentGameSet[currentQuestionIndex];
        CryptoQuestion question = CryptoQuestionDatabase.GetQuestion(currentType, currentStepIndex);
        
        if (question == null)
        {
            Debug.LogError($"問題データが取得できません。CryptoType: {currentType}, StepIndex: {currentStepIndex}");
            yield return StartCoroutine(ForceGameReset());
            yield break;
        }
        
        DisplayQuestion(question);
        
        Debug.Log("同じ問題を再出題 - プレイヤー位置もリセット");
    }

    /// <summary>
    /// 解説パネルの存在を確保する
    /// </summary>
    private IEnumerator EnsureExplanationPanelExists()
    {
        Debug.Log("🔧 解説パネル存在確認開始");
        
        // 既存パネルの検索
        if (explanationPanel == null)
        {
            // 1. 名前による検索
            GameObject panel = GameObject.Find("ExplanationPanel");
            if (panel != null)
            {
                explanationPanel = panel;
                Debug.Log("✅ ExplanationPanel を名前検索で発見");
            }
            else
            {
                // 2. タグによる検索
                GameObject[] taggedPanels = GameObject.FindGameObjectsWithTag("UI");
                foreach (GameObject obj in taggedPanels)
                {                if (obj.name.Contains("Explanation") && obj.name.Contains("Panel"))
                {
                    explanationPanel = obj;
                    Debug.Log($"✅ ExplanationPanel をタグ検索で発見: {obj.name}");
                    break;
                    }
                }
            }
        }
        
        // 既存テキストの検索
        if (explanationText == null)
        {
            Debug.Log("🔍 ExplanationText の詳細検索開始");
            
            if (explanationPanel != null)
            {
                // パネルの子要素からテキストを検索
                Text childText = explanationPanel.GetComponentInChildren<Text>();
                if (childText != null)
                {
                    explanationText = childText;
                    Debug.Log($"✅ ExplanationText をパネル子要素から発見: {childText.name}");
                }
                else
                {
                    Debug.LogWarning("⚠️ パネル内にTextコンポーネントが見つかりません");
                    
                    // パネルの全子要素を詳細確認
                    Component[] allComponents = explanationPanel.GetComponentsInChildren<Component>();
                    Debug.Log($"📋 パネル内の全コンポーネント数: {allComponents.Length}");
                    foreach (Component comp in allComponents)
                    {
                        Debug.Log($"   - {comp.GetType().Name}: {comp.name}");
                    }
                }
            }
            
            if (explanationText == null)
            {
                // シーン全体からテキストを検索
                Text[] allTexts = FindObjectsOfType<Text>();
                Debug.Log($"🔍 シーン内の全Textコンポーネント数: {allTexts.Length}");
                
                foreach (Text text in allTexts)
                {
                    Debug.Log($"   - Text発見: {text.name} (親: {(text.transform.parent?.name ?? "なし")})");
                    
                    if (text.name == "ExplanationText" || text.name.Contains("Explanation"))
                    {
                        explanationText = text;
                        Debug.Log($"✅ ExplanationText をシーン検索で発見: {text.name}");
                        break;
                    }
                }
                
                if (explanationText == null)
                {
                    Debug.LogWarning("⚠️ 適切なExplanationTextが見つかりません");
                }
            }
        }
        
        // UI要素が見つからない場合は動的作成
        if (explanationPanel == null || explanationText == null)
        {
            Debug.Log("🔧 解説パネルが見つからないため動的作成を実行");
            yield return StartCoroutine(CreateExplanationPanelDynamically("テスト解説"));
            yield return new WaitForEndOfFrame(); // 作成完了待機
        }
        
        // 最終確認
        Debug.Log($"✅ 解説パネル存在確認完了 - Panel: {(explanationPanel != null ? "存在" : "なし")}, Text: {(explanationText != null ? "存在" : "なし")}");
    }

    /// <summary>
    /// 解説パネルを表示する
    /// </summary>
    private IEnumerator DisplayExplanationPanel(string explanation, CoroutineResult<bool> result)
    {
        Debug.Log($"[解説パネル表示] 開始: '{explanation}'");
        
        // パネルの状態を詳細確認
        if (explanationPanel != null)
        {
            Debug.Log($"[パネル詳細] 名前: {explanationPanel.name}");
            Debug.Log($"[パネル詳細] アクティブ: {explanationPanel.activeSelf}");
            Debug.Log($"[パネル詳細] 階層内アクティブ: {explanationPanel.activeInHierarchy}");
            Debug.Log($"[パネル詳細] 親: {(explanationPanel.transform.parent?.name ?? "なし")}");
            
            // RectTransform確認
            RectTransform rectTransform = explanationPanel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Debug.Log($"[パネル詳細] 位置: {rectTransform.position}");
                Debug.Log($"[パネル詳細] サイズ: {rectTransform.sizeDelta}");
                Debug.Log($"[パネル詳細] アンカー: {rectTransform.anchoredPosition}");
            }
        }
        
        // エラーハンドリングをする前に基本操作を実行
        bool hasError = false;
        string errorMessage = "";
        
        try
        {
            // テキスト設定
            if (explanationText != null)
            {
                Debug.Log($"[テキスト詳細] 設定前のテキスト: '{explanationText.text}'");
                Debug.Log($"[テキスト詳細] テキストコンポーネント名: {explanationText.name}");
                Debug.Log($"[テキスト詳細] アクティブ: {explanationText.gameObject.activeSelf}");
                Debug.Log($"[テキスト詳細] 階層内アクティブ: {explanationText.gameObject.activeInHierarchy}");
                Debug.Log($"[テキスト詳細] 色: {explanationText.color}");
                Debug.Log($"[テキスト詳細] フォントサイズ: {explanationText.fontSize}");
                Debug.Log($"[テキスト詳細] 親: {(explanationText.transform.parent?.name ?? "なし")}");
                
                explanationText.text = explanation;
                Debug.Log($"✅ 解説テキスト設定完了: '{explanation}'");
                Debug.Log($"[テキスト確認] 設定後のテキスト: '{explanationText.text}'");
                
                // テキストの視認性を強化
                explanationText.color = Color.white;
                explanationText.fontSize = 28; // より大きなフォントサイズ
                
                // フォントを確実に設定
                Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (defaultFont != null)
                {
                    explanationText.font = defaultFont;
                    Debug.Log("✅ デフォルトフォント明示的設定");
                }
                
                // テキスト表示の強制更新
                explanationText.enabled = false;
                explanationText.enabled = true;
                
                Debug.Log($"✅ テキスト強化設定完了 - フォント: {(explanationText.font != null ? explanationText.font.name : "null")}");
                
                // RectTransform確認
                RectTransform textRect = explanationText.GetComponent<RectTransform>();
                if (textRect != null)
                {
                    Debug.Log($"[テキスト詳細] 位置: {textRect.position}");
                    Debug.Log($"[テキスト詳細] サイズ: {textRect.sizeDelta}");
                    Debug.Log($"[テキスト詳細] アンカー: {textRect.anchoredPosition}");
                }
            }
            else
            {
                Debug.LogError("❌ explanationTextがnullです！");
            }
            
            // Canvas順序を最前面に設定
            Canvas explanationCanvas = explanationPanel.GetComponentInParent<Canvas>();
            if (explanationCanvas != null)
            {
                explanationCanvas.sortingOrder = 1000;
                Debug.Log("✅ Canvas順序を最前面に設定");
            }
            else
            {
                Debug.LogWarning("⚠️ 解説パネルのCanvasが見つかりません");
            }
            
            // パネルをアクティブ化
            explanationPanel.SetActive(true);
            Debug.Log("✅ 解説パネルをアクティブ化");
            
            // テキストも明示的にアクティブ化
            if (explanationText != null && explanationText.gameObject != explanationPanel)
            {
                explanationText.gameObject.SetActive(true);
                Debug.Log("✅ 解説テキストもアクティブ化");
            }
        }
        catch (System.Exception e)
        {
            hasError = true;
            errorMessage = e.Message;
        }
        
        if (hasError)
        {
            Debug.LogError($"❌ 解説パネル表示エラー: {errorMessage}");
            result.Result = false;
            result.IsCompleted = true;
            yield break;
        }
        
        // アクティブ化後の状態確認
        yield return new WaitForEndOfFrame(); // UI更新待機
        
        Debug.Log($"[表示確認] パネルアクティブ: {explanationPanel.activeSelf}");
        Debug.Log($"[表示確認] 階層内アクティブ: {explanationPanel.activeInHierarchy}");
        
        // テキストの状態詳細確認
        if (explanationText != null)
        {
            Debug.Log($"[テキスト表示確認] テキスト内容: '{explanationText.text}'");
            Debug.Log($"[テキスト表示確認] テキストアクティブ: {explanationText.gameObject.activeSelf}");
            Debug.Log($"[テキスト表示確認] 階層内アクティブ: {explanationText.gameObject.activeInHierarchy}");
            Debug.Log($"[テキスト表示確認] 色: {explanationText.color}");
            Debug.Log($"[テキスト表示確認] アルファ値: {explanationText.color.a}");
            Debug.Log($"[テキスト表示確認] フォントサイズ: {explanationText.fontSize}");
            Debug.Log($"[テキスト表示確認] 有効状態: {explanationText.enabled}");
            
            // テキストが見えない場合の強制設定
            if (explanationText.color.a < 1.0f || !explanationText.enabled)
            {
                explanationText.color = Color.white;
                explanationText.enabled = true;
                Debug.Log("🔧 テキストの視認性を強制修正");
            }
            
            // 追加の視認性チェック
            RectTransform textRect = explanationText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                Debug.Log($"[テキスト位置] localPosition: {textRect.localPosition}");
                Debug.Log($"[テキスト位置] anchoredPosition: {textRect.anchoredPosition}");
                Debug.Log($"[テキスト位置] sizeDelta: {textRect.sizeDelta}");
                
                // サイズが0の場合は修正
                if (textRect.sizeDelta.x <= 0 || textRect.sizeDelta.y <= 0)
                {
                    textRect.sizeDelta = new Vector2(400, 200);
                    Debug.Log("🔧 テキストサイズを修正");
                }
            }
        }
        
        // 表示時間
        Debug.Log("⏰ 解説表示中... 5秒間");
        yield return new WaitForSeconds(5f);
        
        // パネルを非表示
        try
        {
            explanationPanel.SetActive(false);
            Debug.Log("✅ 解説パネル非表示化完了");
            
            // Canvas順序をリセット
            Canvas explanationCanvas = explanationPanel.GetComponentInParent<Canvas>();
            if (explanationCanvas != null)
            {
                explanationCanvas.sortingOrder = 0;
            }
            
            result.Result = true;
            result.IsCompleted = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 解説パネル非表示エラー: {e.Message}");
            result.Result = false;
            result.IsCompleted = true;
        }
    }

    /// <summary>
    /// 解説表示のフォールバック処理
    /// </summary>
    private IEnumerator ShowExplanationFallback(string explanation)
    {
        Debug.Log("🔄 解説表示フォールバック開始");
        
        // 1. questionTextでの表示
        if (questionText != null)
        {
            string originalText = questionText.text;
            Color originalColor = questionText.color;
            
            questionText.text = $"📝 解説: {explanation}";
            questionText.color = new Color(1f, 0.8f, 0.4f, 1f); // オレンジ色
            
            Debug.Log("📝 questionTextで解説表示中");
            yield return new WaitForSeconds(5f);
            
            // 元に戻す
            questionText.text = originalText;
            questionText.color = originalColor;
            
            Debug.Log("✅ questionTextフォールバック完了");
        }
        else
        {
            // 2. 最後の手段：コンソールのみ
            Debug.LogError($"📝 解説内容（表示手段なし）: {explanation}");
            yield return new WaitForSeconds(3f);
        }
    }

    /// <summary>
    /// 解説パネルを動的に作成（改良版）
    /// </summary>
    private IEnumerator CreateExplanationPanelDynamically(string explanation)
    {
        Debug.Log("🔧 解説パネル動的作成開始（改良版）");
        
        // 1. 最適なCanvasを検索
        Canvas mainCanvas = FindBestCanvas();
        if (mainCanvas == null)
        {
            Debug.LogError("❌ 適切なCanvasが見つかりません");
            yield break;
        }
        
        Debug.Log($"✅ Canvas発見: {mainCanvas.name} (sortingOrder: {mainCanvas.sortingOrder})");
        
        // 2. 既存の解説パネルを削除（重複回避）
        GameObject existingPanel = GameObject.Find("ExplanationPanel");
        if (existingPanel != null)
        {
            Debug.Log("🗑️ 既存の解説パネルを削除");
            DestroyImmediate(existingPanel);
            yield return new WaitForEndOfFrame();
        }
        
        // エラーハンドリング用フラグ
        bool hasError = false;
        string errorMessage = "";
        
        try
        {
            // 3. ExplanationPanelを作成
            GameObject panelObj = new GameObject("ExplanationPanel");
            panelObj.transform.SetParent(mainCanvas.transform, false);
            
            // 4. RectTransform設定（画面中央、適切なサイズ）
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.15f, 0.25f);  // より中央寄り
            panelRect.anchorMax = new Vector2(0.85f, 0.75f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            Debug.Log($"✅ パネル位置設定: anchorMin={panelRect.anchorMin}, anchorMax={panelRect.anchorMax}");
            
            // 5. 背景画像設定
            UnityEngine.UI.Image panelImage = panelObj.AddComponent<UnityEngine.UI.Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.2f, 0.95f); // ダークブルー系
            
            // 6. 視認性向上のための外枠
            UnityEngine.UI.Outline outline = panelObj.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = Color.yellow;
            outline.effectDistance = new Vector2(3, 3);
            
            // 7. 影効果追加
            UnityEngine.UI.Shadow shadow = panelObj.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(5, -5);
            
            Debug.Log("✅ パネル装飾設定完了");
            
            // 8. ExplanationTextを作成
            GameObject textObj = new GameObject("ExplanationText");
            textObj.transform.SetParent(panelObj.transform, false);
            
            // 9. テキスト用RectTransform設定
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(40f, 40f);  // 余白を広げる
            textRect.offsetMax = new Vector2(-40f, -40f);
            
            // 10. Text コンポーネント設定
            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = explanation;
            textComponent.fontSize = 28; // より大きなフォントサイズ
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.lineSpacing = 1.3f;
            textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
            textComponent.verticalOverflow = VerticalWrapMode.Truncate;
            
            // 11. フォント設定（優先度順）
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont != null)
            {
                textComponent.font = defaultFont;
                Debug.Log("✅ LegacyRuntimeフォント設定");
            }
            else
            {
                // Arial フォントを検索
                Font arialFont = Resources.FindObjectsOfTypeAll<Font>()
                    .FirstOrDefault(f => f.name.ToLower().Contains("arial"));
                if (arialFont != null)
                {
                    textComponent.font = arialFont;
                    Debug.Log($"✅ Arialフォント設定: {arialFont.name}");
                }
                else
                {
                    Debug.LogWarning("⚠️ 適切なフォントが見つかりません、デフォルトを使用");
                }
            }
            
            Debug.Log($"✅ テキストコンポーネント作成: '{textComponent.text}'");
            Debug.Log($"   - 名前: {textComponent.name}");
            Debug.Log($"   - フォント: {(textComponent.font != null ? textComponent.font.name : "null")}");
            Debug.Log($"   - フォントサイズ: {textComponent.fontSize}");
            Debug.Log($"   - 色: {textComponent.color}");
            Debug.Log($"   - 親: {textComponent.transform.parent.name}");
            
            // 12. フォント設定（旧実装は削除）
            // SetBestFont(textComponent); ← これをコメントアウト
            
            // 12. テキスト装飾
            UnityEngine.UI.Outline textOutline = textObj.AddComponent<UnityEngine.UI.Outline>();
            textOutline.effectColor = Color.black;
            textOutline.effectDistance = new Vector2(2, 2);
            
            // 13. テキストが正しく表示されるかテスト
            textComponent.text = "【テスト表示】このテキストが見えますか？";
            Debug.Log($"✅ テスト用テキスト設定完了: '{textComponent.text}'");
            
            Debug.Log("✅ テキスト設定完了");
            
            // 14. 参照を設定
            explanationPanel = panelObj;
            explanationText = textComponent;
            
            Debug.Log($"🔗 参照設定完了:");
            Debug.Log($"   - explanationPanel: {(explanationPanel != null ? explanationPanel.name : "null")}");
            Debug.Log($"   - explanationText: {(explanationText != null ? explanationText.name : "null")}");
            
            // 15. Canvas順序を最前面に設定
            mainCanvas.sortingOrder = 1000;
            
            // 16. テキストを元の解説内容に戻す
            textComponent.text = explanation;
            Debug.Log($"✅ 解説テキスト復元: '{explanation}'");
            
            // 17. 初期状態は非表示
            explanationPanel.SetActive(false);
        }
        catch (System.Exception e)
        {
            hasError = true;
            errorMessage = e.Message;
        }
        
        if (hasError)
        {
            Debug.LogError($"❌ 解説パネル動的作成エラー: {errorMessage}");
            yield break;
        }
        
        // 16. 作成完了の検証
        yield return new WaitForEndOfFrame(); // UI更新完了待機
        
        bool creationSuccess = (explanationPanel != null && explanationText != null);
        Debug.Log($"✅ 解説パネル動的作成完了 - 成功: {creationSuccess}");
        
        if (creationSuccess)
        {
            Debug.Log($"📝 パネル詳細: {explanationPanel.name}, テキスト: {explanationText.name}");
            Debug.Log($"📝 Canvas: {mainCanvas.name}, 親: {explanationPanel.transform.parent.name}");
        }
        else
        {
            Debug.LogError("❌ 解説パネル作成後の検証に失敗");
        }
    }
    
    /// <summary>
    /// 最適なCanvasを検索
    /// </summary>
    private Canvas FindBestCanvas()
    {
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        
        // 1. ScreenSpace-Overlayを優先
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay && canvas.gameObject.activeInHierarchy)
            {
                return canvas;
            }
        }
        
        // 2. アクティブなCanvasを検索
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.gameObject.activeInHierarchy)
            {
                return canvas;
            }
        }
        
        // 3. 最初のCanvasを使用
        if (allCanvases.Length > 0)
        {
            return allCanvases[0];
        }
        
        return null;
    }
    
    /// <summary>
    /// 最適なフォントを設定
    /// </summary>
    private void SetBestFont(Text textComponent)
    {
        // 1. デフォルトフォントを試行
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont != null)
        {
            textComponent.font = defaultFont;
            Debug.Log("✅ デフォルトフォント設定");
            return;
        }
        
        // 2. Arialフォントを検索
        Font[] allFonts = Resources.FindObjectsOfTypeAll<Font>();
        foreach (Font font in allFonts)
        {
            if (font.name.Contains("Arial") || font.name.Contains("arial"))
            {
                textComponent.font = font;
                Debug.Log($"✅ Arialフォント設定: {font.name}");
                return;
            }
        }
        
        // 3. 最初に見つかったフォントを使用
        if (allFonts.Length > 0)
        {
            textComponent.font = allFonts[0];
            Debug.Log($"✅ 代替フォント設定: {allFonts[0].name}");
        }
        else
        {
            Debug.LogWarning("⚠️ フォントが見つかりませんでした");
        }
    }

    /// <summary>
    /// 解説パネル設定の検証
    /// </summary>
    private IEnumerator ValidateExplanationPanelSetup()
    {
        yield return new WaitForSeconds(0.5f); // UI初期化待機
        
        Debug.Log("🔍 解説パネル設定を検証中...");
        
        bool isValid = true;
        
        if (explanationPanel == null)
        {
            Debug.LogWarning("⚠️ ExplanationPanel が設定されていません");
            isValid = false;
        }
        else
        {
            Debug.Log($"✅ ExplanationPanel 設定済み: {explanationPanel.name}");
        }
        
        if (explanationText == null)
        {
            Debug.LogWarning("⚠️ ExplanationText が設定されていません");
            isValid = false;
        }
        else
        {
            Debug.Log($"✅ ExplanationText 設定済み: {explanationText.name}");
        }
        
        if (!isValid)
        {
            Debug.LogWarning("🔧 解説パネル要素が不完全です。動的作成を準備します。");
            // 必要に応じて動的作成を実行
            if (explanationPanel == null)
            {
                yield return StartCoroutine(CreateExplanationPanelDynamically("初期化テスト"));
            }
        }
        else
        {
            Debug.Log("🎉 解説パネル設定は完璧です！");
            
            // 初期状態でパネルを非表示にする
            if (explanationPanel != null)
            {
                explanationPanel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// ゲーム状態を強制的にリセットする
    /// </summary>
    private IEnumerator ForceGameReset()
    {
        Debug.Log("強制的にゲーム状態をリセット中...");
        
        // UI表示をクリア
        if (questionText != null)
        {
            questionText.text = "ゲームを再開始しています...";
            questionText.color = Color.white;
        }
        
        // 回答キューブを非表示
        if (answerCubes != null)
        {
            foreach (var cube in answerCubes)
            {
                if (cube != null)
                    cube.SetActive(false);
            }
        }
        
        // UIボタンを非表示
        if (answerButtons != null)
        {
            foreach (Button btn in answerButtons)
            {
                if (btn != null)
                    btn.gameObject.SetActive(false);
            }
        }
        
        yield return new WaitForSeconds(1f);
        
        // ゲームを新たに開始
        StartNewGameSet();
    }
    
    private IEnumerator DelayedCorrectAnswer()
    {
        yield return new WaitForSeconds(1f); // フィードバック表示時間
        
        // UI色をリセット
        if (questionText != null)
        {
            questionText.color = Color.white;
        }
        
        OnCorrectAnswer();
    }
    
    /// <summary>
    /// プレイヤーの位置をリセットする（設定システム対応・CharacterController対応・床抜け防止・ThirdPersonController対応）
    /// </summary>
    /// <param name="targetPosition">リセット先の位置</param>
    private IEnumerator ResetPlayerPosition(Vector3 targetPosition)
    {
        if (player == null)
        {
            Debug.LogWarning("プレイヤーオブジェクトが設定されていません");
            yield break;
        }

        Debug.Log($"プレイヤーを {targetPosition} にリセット中...");

        // 設定システムから詳細パラメータを取得
        bool useGroundDetection = resetSettings?.useGroundDetection ?? true;
        float groundDistance = resetSettings?.groundDetectionDistance ?? 20f;
        float heightOffset = resetSettings?.heightOffset ?? 1.5f;
        bool forceHeightOffset = resetSettings?.forceHeightOffset ?? false;
        bool shouldResetRotation = resetSettings?.resetRotation ?? false;
        Vector3 resetRotation = resetSettings?.resetRotationEuler ?? Vector3.zero;

        // ThirdPersonControllerを一時的に無効化（CharacterController.Moveエラーを防ぐ）
        MonoBehaviour targetController = null;
        bool wasThirdPersonControllerEnabled = false;
        
        // StarterAssetsのThirdPersonControllerを検索して無効化
        MonoBehaviour[] components = player.GetComponents<MonoBehaviour>();
        
        foreach (var component in components)
        {
            if (component.GetType().Name == "ThirdPersonController")
            {
                targetController = component;
                wasThirdPersonControllerEnabled = component.enabled;
                component.enabled = false;
                Debug.Log("ThirdPersonControllerを一時的に無効化");
                break;
            }
        }

        // CharacterControllerがある場合の特別処理
        CharacterController characterController = player.GetComponent<CharacterController>();
        if (characterController != null)
        {
            Debug.Log("CharacterController検出 - 安全なリセット処理を実行");
            
            // CharacterControllerを一時的に無効化
            characterController.enabled = false;
            yield return new WaitForFixedUpdate(); // 物理更新を待つ
            
            // 地面検出を使用するかどうかで処理を分岐
            Vector3 finalPosition = targetPosition;
            
            if (!useGroundDetection)
            {
                // 地面検出を使用しない場合：設定位置をそのまま使用
                Debug.Log($"地面検出無効 - 設定位置をそのまま使用: {targetPosition}");
                player.position = targetPosition;
            }
            else
            {
                // 地面検出を使用する場合：まず高めの位置に設定
                Vector3 safeTargetPosition = targetPosition;
                safeTargetPosition.y += heightOffset + 2f; // 安全マージンを追加
                player.position = safeTargetPosition;
                Debug.Log($"地面検出有効 - 一時的に高い位置に配置: {safeTargetPosition}");
            }
            
            // 向きもリセットする場合
            if (shouldResetRotation)
            {
                player.rotation = Quaternion.Euler(resetRotation);
                Debug.Log($"プレイヤーの向きをリセット: {resetRotation}");
            }
            
            yield return new WaitForFixedUpdate(); // 物理更新を待つ
            
            // CharacterControllerを再有効化
            characterController.enabled = true;
            yield return new WaitForFixedUpdate(); // 物理更新を待つ
            
            // 地面検出処理
            if (useGroundDetection)
            {
                // 地面に向かってレイキャストして正確な地面位置を取得
                RaycastHit hit;
                Vector3 rayStart = player.position + Vector3.up * 5f; // 十分に高い位置から開始
                
                Debug.Log($"地面検出開始 - レイキャスト開始位置: {rayStart}, 検出距離: {groundDistance}m");
                
                if (Physics.Raycast(rayStart, Vector3.down, out hit, groundDistance, ~0, QueryTriggerInteraction.Ignore))
                {
                    // 地面が見つかった場合、その位置に設定（CharacterControllerの高さを考慮）
                    Vector3 groundPosition = hit.point;
                    float controllerHeightOffset = characterController.height * 0.5f + characterController.skinWidth;
                    groundPosition.y += controllerHeightOffset + heightOffset;
                    
                    // CharacterController.Moveを使用して安全に移動
                    Vector3 moveVector = groundPosition - player.position;
                    characterController.Move(moveVector);
                    
                    Debug.Log($"地面検出成功！最終配置位置: {groundPosition}");
                }
                else
                {
                    // 地面が見つからない場合の処理
                    Debug.LogWarning($"地面が検出されませんでした（検出距離: {groundDistance}m）");
                    
                    if (forceHeightOffset)
                    {
                        // 強制オフセットが有効な場合、目標位置にオフセットを適用
                        Vector3 offsetPosition = targetPosition;
                        offsetPosition.y += heightOffset;
                        Vector3 moveVector = offsetPosition - player.position;
                        characterController.Move(moveVector);
                        Debug.Log($"強制オフセット適用 - 最終位置: {offsetPosition}");
                    }
                    else
                    {
                        // 元の目標位置を使用
                        Vector3 moveVector = targetPosition - player.position;
                        characterController.Move(moveVector);
                        Debug.Log($"目標位置をそのまま使用: {targetPosition}");
                    }
                }
            }
            
            Debug.Log("CharacterController付きプレイヤーの位置リセット完了");
        }
        else
        {
            // Rigidbodyがある場合の処理
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Debug.Log("Rigidbody検出 - 物理リセット処理を実行");
                
                // 物理的な速度をリセット
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                
                Vector3 finalRigidbodyPosition = targetPosition;
                
                // 地面検出処理
                if (useGroundDetection)
                {
                    RaycastHit hit;
                    Vector3 rayStart = targetPosition + Vector3.up * 10f;
                    
                    if (Physics.Raycast(rayStart, Vector3.down, out hit, groundDistance, ~0, QueryTriggerInteraction.Ignore))
                    {
                        finalRigidbodyPosition = hit.point;
                        finalRigidbodyPosition.y += heightOffset;
                        Debug.Log($"Rigidbody地面検出成功 - 配置位置: {finalRigidbodyPosition}");
                    }
                    else if (forceHeightOffset)
                    {
                        finalRigidbodyPosition.y += heightOffset;
                        Debug.Log($"Rigidbody強制オフセット適用: +{heightOffset}m");
                    }
                }
                else if (forceHeightOffset)
                {
                    finalRigidbodyPosition.y += heightOffset;
                    Debug.Log($"Rigidbody地面検出無効・オフセット適用: +{heightOffset}m");
                }
                
                // 位置を設定
                rb.MovePosition(finalRigidbodyPosition);
                
                // 向きもリセットする場合
                if (shouldResetRotation)
                {
                    rb.MoveRotation(Quaternion.Euler(resetRotation));
                    Debug.Log($"Rigidbodyプレイヤーの向きをリセット: {resetRotation}");
                }
                
                Debug.Log($"Rigidbody付きプレイヤーの位置リセット完了 - 最終位置: {finalRigidbodyPosition}");
            }
            else
            {
                // 通常のTransformによる移動
                Vector3 finalTransformPosition = targetPosition;
                
                // 地面検出処理
                if (useGroundDetection)
                {
                    RaycastHit hit;
                    Vector3 rayStart = targetPosition + Vector3.up * 10f;
                    
                    if (Physics.Raycast(rayStart, Vector3.down, out hit, groundDistance, ~0, QueryTriggerInteraction.Ignore))
                    {
                        finalTransformPosition = hit.point;
                        finalTransformPosition.y += heightOffset;
                        Debug.Log($"Transform地面検出成功 - 配置位置: {finalTransformPosition}");
                    }
                    else if (forceHeightOffset)
                    {
                        finalTransformPosition.y += heightOffset;
                        Debug.Log($"Transform強制オフセット適用: +{heightOffset}m");
                    }
                }
                else if (forceHeightOffset)
                {
                    finalTransformPosition.y += heightOffset;
                    Debug.Log($"Transform地面検出無効・オフセット適用: +{heightOffset}m");
                }
                
                player.position = finalTransformPosition;
                
                // 向きもリセットする場合
                if (shouldResetRotation)
                {
                    player.rotation = Quaternion.Euler(resetRotation);
                    Debug.Log($"Transformプレイヤーの向きをリセット: {resetRotation}");
                }
                
                Debug.Log($"Transform直接操作でプレイヤーの位置リセット完了 - 最終位置: {finalTransformPosition}");
            }
        }

        // ThirdPersonControllerを再有効化
        if (targetController != null && wasThirdPersonControllerEnabled)
        {
            yield return new WaitForFixedUpdate(); // 物理更新を待つ
            targetController.enabled = true;
            Debug.Log("ThirdPersonControllerを再有効化");
        }

        // カメラとプレイヤーコントローラーが安定するまで待機
        yield return new WaitForSeconds(0.2f);
        
        Debug.Log($"プレイヤーリセット完了 - 最終位置: {player.position}");
    }

    private void OnCorrectAnswer()
    {
        Debug.Log($"[OnCorrectAnswer開始] currentStepIndex: {currentStepIndex}, currentQuestionIndex: {currentQuestionIndex}");
        
        // 理解度を更新
        CryptoType currentType = currentGameSet[currentQuestionIndex];
        progressTracker.UpdateProgress(currentType, 20f); // 5問構成なので20%ずつ
        
        // 進捗UI表示を即座に更新（アニメーション付き）
        UpdateProgressDisplay();
        
        // 次のステップまたは次の問題へ
        currentStepIndex++;
        Debug.Log($"[OnCorrectAnswer] currentStepIndex増加後: {currentStepIndex}");
        
        // 進捗テキストを更新（currentStepIndex増加後）
        UpdateProgressText();
        
        // 進捗詳細情報を表示（最新の進捗テキストの後）
        ShowProgressDetails(currentType);
        
        // プレイヤーの位置をリセット（設定システムを使用）
        StartCoroutine(ResetPlayerPosition(GetPlayerResetPosition()));
        
        if (currentStepIndex >= CryptoQuestionDatabase.GetStepCount(currentType))
        {
            // 次の暗号方式へ
            currentQuestionIndex++;
            currentStepIndex = 0;
            Debug.Log($"[OnCorrectAnswer] 次の暗号方式へ移行 - currentQuestionIndex: {currentQuestionIndex}, currentStepIndex: {currentStepIndex}");
            
            if (currentQuestionIndex < questionsPerSet)
            {
                StartCoroutine(TransitionToNextCryptoType());
            }
            else
            {
                EndGameSet();
            }
        }
        else
        {
            // 同じ暗号方式の次のステップへ
            Debug.Log($"[OnCorrectAnswer] 同じ暗号方式の次のステップへ - currentStepIndex: {currentStepIndex}");
            StartCoroutine(TransitionToNextQuestion());
        }
    }
    
    private IEnumerator TransitionToNextQuestion()
    {
        Debug.Log($"[TransitionToNextQuestion開始] currentStepIndex: {currentStepIndex}, currentQuestionIndex: {currentQuestionIndex}");
        
        questionText.text = "次の問題に進みます...";
        
        // 3D回答キューブを非表示
        if (answerCubes != null)
        {
            foreach (var cube in answerCubes)
            {
                if (cube != null)
                    cube.SetActive(false);
            }
        }
        
        // UIボタンを無効化
        if (answerButtons != null)
        {
            foreach (Button btn in answerButtons)
            {
                btn.gameObject.SetActive(false);
            }
        }
        
        yield return new WaitForSeconds(1f);
        
        Debug.Log($"[TransitionToNextQuestion] StartCurrentQuestion()呼び出し前 - currentStepIndex: {currentStepIndex}");
        StartCurrentQuestion();
    }
    
    private IEnumerator TransitionToNextCryptoType()
    {
        questionText.text = "次の暗号方式に進みます...";
        
        // 3D回答キューブを非表示
        if (answerCubes != null)
        {
            foreach (var cube in answerCubes)
            {
                if (cube != null)
                    cube.SetActive(false);
            }
        }
        
        // UIボタンを無効化
        if (answerButtons != null)
        {
            foreach (Button btn in answerButtons)
            {
                btn.gameObject.SetActive(false);
            }
        }
        
        // 3Dオブジェクトをリセット
        if (animationManager != null)
        {
            animationManager.ResetAllObjects();
        }
        
        yield return new WaitForSeconds(2f); // 少し長めに待機
        
        StartCurrentQuestion();
    }
    
    private void EndGameSet()
    {
        isGameActive = false;
        ShowResults();
    }
    
    private void ShowResults()
    {
        // ゲーム終了時は FinalResultPanel のみを表示し、resultPanel は使用しない
        // resultPanel.SetActive(true);  // ← コメントアウト
        
        float accuracy = totalQuestions > 0 ? (float)correctAnswers / totalQuestions * 100f : 0f;
        string evaluation = GetEvaluation(accuracy);
        
        // resultText は使用せず、FinalScoreText のみで表示
        // resultText.text = $"セット完了！\n" +
        //                  $"正解数: {correctAnswers}/{totalQuestions}\n" +
        //                  $"正解率: {accuracy:F1}%\n" +
        //                  $"最終スコア: {currentScore}点\n" +
        //                  $"評価: {evaluation}";
        
        // 最終スコア表示のみ実行
        ShowFinalScore();
        
        StartCoroutine(WaitForRestartInput());
    }
    
    /// <summary>
    /// 最終スコア表示（Enterキー対応版）
    /// </summary>
    private void ShowFinalScore()
    {
        if (finalResultPanel != null)
        {
            finalResultPanel.SetActive(true);
            
            float accuracy = totalQuestions > 0 ? (float)correctAnswers / totalQuestions * 100f : 0f;
            string evaluation = GetEvaluation(accuracy);
            
            if (finalScoreText != null)
            {
                finalScoreText.text = $"ゲーム完了！\n\n" +
                                    $"最終スコア: {currentScore}点\n" +
                                    $"正解数: {correctAnswers}/{totalQuestions}\n" +
                                    $"正答率: {accuracy:F1}%\n" +
                                    $"評価: {evaluation}\n\n" +
                                    $"✨ Enterキーでもう一度プレイ ✨";
                
                Debug.Log($"✅ 最終スコア表示完了 - スコア: {currentScore}点, 正答率: {accuracy:F1}%");
            }
            
            // Canvas順序を最前面に設定
            Canvas canvas = finalResultPanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = 1000;
            }
        }
        else
        {
            Debug.LogWarning("⚠️ FinalResultPanelが設定されていません");
        }
    }
    
    private string GetEvaluation(float accuracy)
    {
        if (accuracy >= 90f) return "⭐⭐⭐ Perfect!";
        if (accuracy >= 70f) return "⭐⭐ Great!";
        if (accuracy >= 50f) return "⭐ Good!";
        return "Keep Learning!";
    }
    
    private void UpdateProgressDisplay()
    {
        if (progressTracker == null)
        {
            Debug.LogWarning("ProgressTracker が見つかりません");
            return;
        }
        
        float[] progressValues = progressTracker.GetAllProgress();
        string[] cryptoNames = { "共通鍵", "公開鍵", "ハイブリッド" };
        
        if (progressSliders != null && progressLabels != null)
        {
            for (int i = 0; i < progressSliders.Length && i < progressValues.Length; i++)
            {
                if (progressSliders[i] != null)
                {
                    // 既存のアニメーションがある場合は停止
                    if (sliderAnimations.ContainsKey(i))
                    {
                        StopCoroutine(sliderAnimations[i]);
                    }
                    
                    // 新しいアニメーションを開始
                    sliderAnimations[i] = StartCoroutine(AnimateProgressSlider(progressSliders[i], progressValues[i] / 100f, i));
                }
                
                if (i < progressLabels.Length && progressLabels[i] != null)
                {
                    progressLabels[i].text = $"{cryptoNames[i]} {progressValues[i]:F0}%";
                }
            }
        }
        else
        {
            Debug.LogWarning("Progress Sliders または Progress Labels が割り当てられていません");
        }
    }
    
    /// <summary>
    /// 進捗スライダーをスムーズにアニメーション
    /// </summary>
    private IEnumerator AnimateProgressSlider(Slider slider, float targetValue, int index)
    {
        float startValue = slider.value;
        float elapsedTime = 0f;
        bool isIncreasing = targetValue > startValue;
        
        // 進捗増加時は色を変更
        Image fillImage = slider.fillRect.GetComponent<Image>();
        Color originalColor = fillImage.color;
        
        if (isIncreasing && fillImage != null)
        {
            fillImage.color = progressIncreaseColor;
        }
        
        // アニメーション実行
        while (elapsedTime < progressAnimationDuration)
        {
            float t = elapsedTime / progressAnimationDuration;
            float easedT = Mathf.SmoothStep(0f, 1f, t); // スムーズなアニメーション
            slider.value = Mathf.Lerp(startValue, targetValue, easedT);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 最終値を設定
        slider.value = targetValue;
        
        // 色を元に戻す（少し遅らせて）
        if (isIncreasing && fillImage != null)
        {
            yield return new WaitForSeconds(0.2f);
            
            float colorElapsed = 0f;
            float colorDuration = 0.3f;
            
            while (colorElapsed < colorDuration)
            {
                float t = colorElapsed / colorDuration;
                fillImage.color = Color.Lerp(progressIncreaseColor, originalColor, t);
                colorElapsed += Time.deltaTime;
                yield return null;
            }
            
            fillImage.color = originalColor;
        }
        
        // アニメーション完了を記録
        if (sliderAnimations.ContainsKey(index))
        {
            sliderAnimations.Remove(index);
        }
    }
    
    private void UpdateProgressText()
    {
        if (progressText != null && currentGameSet != null && currentQuestionIndex < currentGameSet.Length)
        {
            string cryptoName = GetCryptoTypeName(currentGameSet[currentQuestionIndex]);
            // 暗号方式別に問題数を取得して表示
            int totalSteps = CryptoQuestionDatabase.GetStepCount(currentGameSet[currentQuestionIndex]);
            
            // 詳細デバッグログ（ProgressText用）
            Debug.Log($"[ProgressText更新詳細]");
            Debug.Log($"  currentQuestionIndex: {currentQuestionIndex}");
            Debug.Log($"  currentStepIndex: {currentStepIndex}");
            Debug.Log($"  cryptoName: {cryptoName}");
            Debug.Log($"  totalSteps: {totalSteps}");
            Debug.Log($"  計算結果: 問題 {currentStepIndex + 1}/{totalSteps} - {cryptoName}");
            
            progressText.text = $"問題 {currentStepIndex + 1}/{totalSteps} - {cryptoName}";
            
            Debug.Log($"[ProgressText更新完了] '{progressText.text}'");
        }
        else if (progressText == null)
        {
            Debug.LogWarning("Progress Text が割り当てられていません");
        }
        else
        {
            Debug.LogWarning($"UpdateProgressText: ゲーム状態無効 - currentGameSet: {(currentGameSet != null ? "存在" : "null")}, currentQuestionIndex: {currentQuestionIndex}");
        }
    }
    
    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(gameTimer / 60f);
            int seconds = Mathf.FloorToInt(gameTimer % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
        else
        {
            Debug.LogWarning("Timer Text が割り当てられていません");
        }
    }
    
    private string GetCryptoTypeName(CryptoType type)
    {
        switch (type)
        {
            case CryptoType.SymmetricKey: return "共通鍵暗号";
            case CryptoType.PublicKey: return "公開鍵暗号";
            case CryptoType.Hybrid: return "ハイブリッド暗号";
            default: return "Unknown";
        }
    }
    
    /// <summary>
    /// 設定に基づいて実際の復帰位置を取得
    /// </summary>
    private Vector3 GetPlayerResetPosition()
    {
        if (resetSettings == null)
        {
            // デフォルト位置を返す
            return new Vector3(0, 3, 5);
        }
        
        Vector3 basePosition;
        
        switch (resetSettings.resetType)
        {
            case ResetPositionType.Custom:
                basePosition = resetSettings.customPosition;
                break;
                
            case ResetPositionType.Preset:
                basePosition = GetPresetPosition(resetSettings.presetPosition);
                break;
                
            case ResetPositionType.Transform:
                if (resetSettings.referenceTransform != null)
                {
                    basePosition = resetSettings.referenceTransform.position;
                }
                else
                {
                    Debug.LogWarning("Reference Transform が設定されていません。カスタム位置を使用します。");
                    basePosition = resetSettings.customPosition;
                }
                break;
                
            default:
                basePosition = resetSettings.customPosition;
                break;
        }
        
        // 高さオフセットを強制適用する場合
        if (resetSettings.forceHeightOffset)
        {
            basePosition.y += resetSettings.heightOffset;
            Debug.Log($"高さオフセットを強制適用: +{resetSettings.heightOffset}m (最終Y座標: {basePosition.y})");
        }
        
        return basePosition;
    }
    
    /// <summary>
    /// プリセット位置を取得
    /// </summary>
    private Vector3 GetPresetPosition(PresetPosition preset)
    {
        switch (preset)
        {
            case PresetPosition.Center:
                return new Vector3(0, 3, 5);
            case PresetPosition.FarCenter:
                return new Vector3(0, 3, 10);
            case PresetPosition.LeftSide:
                return new Vector3(-5, 3, 5);
            case PresetPosition.RightSide:
                return new Vector3(5, 3, 5);
            case PresetPosition.HighCenter:
                return new Vector3(0, 8, 5);
            case PresetPosition.StartPosition:
                return new Vector3(0, 1, 0);
            default:
                return new Vector3(0, 3, 5);
        }
    }

    /// <summary>
    /// 手動で理解度をリセットする（デバッグ用）
    /// </summary>
    public void ManualResetProgress()
    {
        if (progressTracker != null)
        {
            progressTracker.ResetProgressForNewGame();
            UpdateProgressDisplay(); // UI更新
            Debug.Log("CryptoGameManager: 理解度を手動でリセットしました");
        }
        else
        {
            Debug.LogWarning("CryptoGameManager: ProgressTrackerが見つかりません");
        }
    }
    
    /// <summary>
    /// スコア表示を更新
    /// </summary>
    private void UpdateScoreDisplay()
    {
        if (currentScoreText != null)
        {
            currentScoreText.text = $"スコア: {currentScore}点";
        }
    }
    
    /// <summary>
    /// 正解時のスコア加算処理
    /// </summary>
    public void AddCorrectAnswerScore()
    {
        currentScore += pointsPerCorrect;
        UpdateScoreDisplay();
        Debug.Log($"正解！ +{pointsPerCorrect}点 (合計: {currentScore}点)");
    }
    
    /// <summary>
    /// 不正解時のスコア減点処理
    /// </summary>
    public void AddIncorrectAnswerScore()
    {
        currentScore += pointsPerIncorrect;
        // スコアが負の数にならないように調整
        if (currentScore < 0) currentScore = 0;
        UpdateScoreDisplay();
        Debug.Log($"不正解... {pointsPerIncorrect}点 (合計: {currentScore}点)");
    }
    
    /// <summary>
    /// 進捗の詳細情報を表示（正解時の追加情報）
    /// </summary>
    public void ShowProgressDetails(CryptoType cryptoType)
    {
        if (progressTracker == null) return;
        
        float progress = progressTracker.GetProgress(cryptoType);
        int completedSteps = Mathf.RoundToInt(progress / 20f); // 20%刻みなので
        int totalSteps = CryptoQuestionDatabase.GetStepCount(cryptoType);
        
        string cryptoName = GetCryptoTypeName(cryptoType);
        string detailMessage = $"{cryptoName}: {completedSteps}/{totalSteps}問完了 ({progress:F0}%)";
        
        Debug.Log($"[進捗詳細] {detailMessage}");
        
        // UI上に短時間表示するフローティングメッセージ（オプション）
        StartCoroutine(ShowFloatingProgressMessage(detailMessage));
    }
    
    /// <summary>
    /// 進捗メッセージをフローティング表示
    /// </summary>
    private IEnumerator ShowFloatingProgressMessage(string message)
    {
        // 既存のprogressTextを一時的に使用して詳細表示
        if (progressText != null)
        {
            Color originalColor = progressText.color;
            
            // 詳細情報を短時間表示
            progressText.text = message;
            progressText.color = progressIncreaseColor;
            
            yield return new WaitForSeconds(2f);
            
            // 最新の進捗情報を再計算して表示
            UpdateProgressText();
            progressText.color = originalColor;
        }
    }
    
    /// <summary>
    /// すべての暗号方式の完了度をチェック
    /// </summary>
    public bool IsAllCryptoTypesCompleted()
    {
        if (progressTracker == null) return false;
        
        float[] progressValues = progressTracker.GetAllProgress();
        
        foreach (float progress in progressValues)
        {
            if (progress < 100f) return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 総合的な学習進捗を取得
    /// </summary>
    public float GetOverallLearningProgress()
    {
        if (progressTracker == null) return 0f;
        
        return progressTracker.GetOverallProgress();
    }

    /// <summary>
    /// デバッグ用：進捗表示システムの総合テスト
    /// </summary>
    [ContextMenu("Test Progress Animation System")]
    public void TestProgressAnimationSystem()
    {
        if (!enableDebugFunctions) return;
        
        StartCoroutine(TestProgressSequence());
    }
    
    private IEnumerator TestProgressSequence()
    {
        Debug.Log("=== 進捗アニメーションシステムテスト開始 ===");
        
        // 1. リセット
        if (progressTracker != null)
        {
            progressTracker.ResetProgressForNewGame();
            UpdateProgressDisplay();
        }
        
        yield return new WaitForSeconds(1f);
        
        // 2. 共通鍵暗号の進捗テスト
        Debug.Log("共通鍵暗号の進捗テスト開始");
        for (int i = 1; i <= 5; i++)
        {
            progressTracker.UpdateProgress(CryptoType.SymmetricKey, 20f);
            UpdateProgressDisplay();
            ShowProgressDetails(CryptoType.SymmetricKey);
            yield return new WaitForSeconds(1.5f);
        }
        
        yield return new WaitForSeconds(1f);
        
        // 3. 公開鍵暗号の進捗テスト
        Debug.Log("公開鍵暗号の進捗テスト開始");
        for (int i = 1; i <= 5; i++)
        {
            progressTracker.UpdateProgress(CryptoType.PublicKey, 20f);
            UpdateProgressDisplay();
            ShowProgressDetails(CryptoType.PublicKey);
            yield return new WaitForSeconds(1.5f);
        }
        
        yield return new WaitForSeconds(1f);
        
        // 4. ハイブリッド暗号の進捗テスト
        Debug.Log("ハイブリッド暗号の進捗テスト開始");
        for (int i = 1; i <= 5; i++)
        {
            progressTracker.UpdateProgress(CryptoType.Hybrid, 20f);
            UpdateProgressDisplay();
            ShowProgressDetails(CryptoType.Hybrid);
            yield return new WaitForSeconds(1.5f);
        }
        
        // 5. 完了チェック
        bool allCompleted = IsAllCryptoTypesCompleted();
        float overallProgress = GetOverallLearningProgress();
        
        Debug.Log($"=== テスト完了 ===");
        Debug.Log($"全暗号方式完了: {allCompleted}");
        Debug.Log($"総合進捗: {overallProgress:F1}%");
    }

    /// <summary>
    /// デバッグ用：解説システムのテスト
    /// </summary>
    [ContextMenu("Test Explanation System")]
    public void TestExplanationSystem()
    {
        if (!enableDebugFunctions) return;
        
        StartCoroutine(TestExplanationSequence());
    }
    
    private IEnumerator TestExplanationSequence()
    {
        Debug.Log("=== 解説システムテスト開始 ===");
        
        // テスト用解説文
        string[] testExplanations = {
            "❌ テスト解説1: これは不正解の説明です。",
            "✅ テスト解説2: これは正解の説明です。",
            "❌ テスト解説3: 長い解説テストです。共通鍵暗号では送信者と受信者が同じ暗号鍵を使用して、データの暗号化と復号化を行います。この方式では鍵の配布が重要な課題となります。"
        };
        
        foreach (string explanation in testExplanations)
        {
            Debug.Log($"解説テスト実行: {explanation}");
            if (isGameActive)
            {
                yield return StartCoroutine(RetryCurrentQuestion(explanation));
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                Debug.Log("ゲーム非アクティブのため、解説テストをスキップします");
                break;
            }
        }
        
        Debug.Log("=== 解説システムテスト完了 ===");
    }
    
    /// <summary>
    /// デバッグ用：UI要素の状態確認
    /// </summary>
    [ContextMenu("Check UI Elements Status")]
    public void CheckUIElementsStatus()
    {
        if (!enableDebugFunctions) return;
        
        Debug.Log("=== UI要素状態確認 ===");
        Debug.Log($"questionText: {(questionText != null ? "✅設定済み" : "❌未設定")}");
        Debug.Log($"explanationPanel: {(explanationPanel != null ? "✅設定済み" : "❌未設定")}");
        Debug.Log($"explanationText: {(explanationText != null ? "✅設定済み" : "❌未設定")}");
        Debug.Log($"currentScoreText: {(currentScoreText != null ? "✅設定済み" : "❌未設定")}");
        Debug.Log($"finalResultPanel: {(finalResultPanel != null ? "✅設定済み" : "❌未設定")}");
        
        if (explanationPanel != null)
        {
            Debug.Log($"explanationPanel アクティブ状態: {explanationPanel.activeInHierarchy}");
        }
        
        if (explanationText != null)
        {
            Debug.Log($"explanationText 内容: '{explanationText.text}'");
        }
        
        Debug.Log("=== UI要素状態確認完了 ===");
    }

    /// <summary>
    /// デバッグ用：不足しているUI要素の自動検索
    /// </summary>
    [ContextMenu("Auto Find Missing UI Elements")]
    public void AutoFindMissingUIElements()
    {
        if (!enableDebugFunctions) return;
        
        Debug.Log("=== UI要素自動検索開始 ===");
        
        // ExplanationPanel の自動検索
        if (explanationPanel == null)
        {
            GameObject panel = GameObject.Find("ExplanationPanel");
            if (panel != null)
            {
                explanationPanel = panel;
                Debug.Log("✅ ExplanationPanel を自動検出しました");
            }
            else
            {
                // より詳細な検索
                GameObject[] allObjects = FindObjectsOfType<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name.Contains("Explanation") && obj.name.Contains("Panel"))
                    {
                        explanationPanel = obj;
                        Debug.Log($"✅ ExplanationPanel を部分一致で検出: {obj.name}");
                        break;
                    }
                }
                
                if (explanationPanel == null)
                {
                    Debug.LogWarning("❌ ExplanationPanel が見つかりません。動的作成を試行します。");
                    StartCoroutine(CreateExplanationPanelDynamically("テスト解説"));
                }
            }
        }
        
        // ExplanationText の自動検索
        if (explanationText == null)
        {
            // 名前での検索
            Text[] allTexts = FindObjectsOfType<Text>();
            foreach (Text text in allTexts)
            {
                if (text.name == "ExplanationText" || text.name.Contains("Explanation"))
                {
                    explanationText = text;
                    Debug.Log($"✅ ExplanationText を自動検出しました: {text.name}");
                    break;
                }
            }
            
            // ExplanationPanelの子要素から検索
            if (explanationText == null && explanationPanel != null)
            {
                Text childText = explanationPanel.GetComponentInChildren<Text>();
                if (childText != null)
                {
                    explanationText = childText;
                    Debug.Log($"✅ ExplanationText を子要素から検出: {childText.name}");
                }
            }
            
            if (explanationText == null)
            {
                Debug.LogWarning("❌ ExplanationText が見つかりません。手動作成が必要です。");
            }
        }
        
        // CurrentScoreText の自動検索
        if (currentScoreText == null)
        {
            Text[] allTexts = FindObjectsOfType<Text>();
            foreach (Text text in allTexts)
            {
                if (text.name == "CurrentScoreText" || text.name.Contains("Score"))
                {
                    currentScoreText = text;
                    Debug.Log($"✅ CurrentScoreText を自動検出しました: {text.name}");
                    break;
                }
            }
        }
        
        Debug.Log("=== UI要素自動検索完了 ===");
        
        // 検索後の状態確認
        CheckUIElementsStatus();
    }

    /// <summary>
    /// 解説パネル表示テスト（デバッグ用）
    /// </summary>
    [ContextMenu("Test Explanation Panel")]
    public void TestExplanationPanel()
    {
        if (!enableDebugFunctions) return;
        
        Debug.Log("🧪 解説パネル表示テスト開始");
        
        string testExplanation = "これはテスト用の解説です。解説パネルが正常に表示されるかを確認します。";
        
        StartCoroutine(TestExplanationDisplay(testExplanation));
    }
    
    /// <summary>
    /// 解説表示テスト実行
    /// </summary>
    private IEnumerator TestExplanationDisplay(string explanation)
    {
        Debug.Log($"🧪 解説テスト: {explanation}");
        
        // UI要素の自動検索
        AutoFindMissingUIElements();
        yield return new WaitForSeconds(0.1f);
        
        // 解説パネル表示処理をテスト
        if (explanationPanel != null && explanationText != null)
        {
            explanationPanel.SetActive(true);
            explanationText.text = explanation;
            
            Debug.Log("✅ 解説パネルテスト表示成功");
            
            yield return new WaitForSeconds(5f); // 5秒間表示
            
            explanationPanel.SetActive(false);
            Debug.Log("✅ 解説パネルテスト終了");
        }
        else
        {
            Debug.LogError("❌ 解説パネルテスト失敗：UI要素が見つかりません");
            
            // 動的作成を試行
            yield return StartCoroutine(CreateExplanationPanelDynamically(explanation));
            
            if (explanationPanel != null && explanationText != null)
            {
                explanationPanel.SetActive(true);
                yield return new WaitForSeconds(5f);
                explanationPanel.SetActive(false);
                Debug.Log("✅ 動的作成パネルでテスト完了");
            }
        }
    }

    /// <summary>
    /// 強制的に解説パネルを表示する（デバッグ用）
    /// </summary>
    [ContextMenu("Force Show Explanation Panel")]
    public void ForceShowExplanationPanel()
    {
        if (!enableDebugFunctions) return;
        
        Debug.Log("🔧 解説パネル強制表示開始");
        
        string testExplanation = "これは強制表示テストです。解説パネルが正常に表示されるかを確認します。";
        
        StartCoroutine(ForceDisplayExplanation(testExplanation));
    }
    
    /// <summary>
    /// 強制的に解説を表示する実行部分
    /// </summary>
    private IEnumerator ForceDisplayExplanation(string explanation)
    {
        Debug.Log("🔧 強制解説表示実行中...");
        
        // 1. まずUI要素を再検索
        AutoFindMissingUIElements();
        yield return new WaitForSeconds(0.1f);
        
        // 2. UI要素の状態を詳細チェック
        Debug.Log($"[強制表示] explanationPanel: {(explanationPanel != null ? $"存在({explanationPanel.name})" : "null")}");
        Debug.Log($"[強制表示] explanationText: {(explanationText != null ? $"存在({explanationText.name})" : "null")}");
        
        // 3. パネルが存在しない場合は動的作成
        if (explanationPanel == null)
        {
            Debug.Log("🔧 解説パネルが存在しないため動的作成を実行");
            yield return StartCoroutine(CreateExplanationPanelDynamically(explanation));
            yield return new WaitForSeconds(0.2f);
        }
        
        // 4. 解説表示を実行
        if (explanationPanel != null && explanationText != null)
        {
            Debug.Log("✅ UI要素確認完了、解説表示を開始");
            
            // パネルを前面に表示
            explanationPanel.SetActive(true);
            explanationText.text = explanation;
            
            // Canvas順序を最前面に
            Canvas canvas = explanationPanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = 2000;
            }
            
            Debug.Log($"✅ 強制解説表示成功: '{explanation}'");
            
            yield return new WaitForSeconds(5f);
            
            // 元に戻す
            explanationPanel.SetActive(false);
            if (canvas != null)
            {
                canvas.sortingOrder = 0;
            }
            
            Debug.Log("✅ 強制解説表示終了");
        }
        else
        {
            Debug.LogError("❌ 強制解説表示失敗: UI要素が作成できませんでした");
            
            // 最終手段：console出力のみ
            Debug.LogError($"[解説内容] {explanation}");
        }
    }

    /// <summary>
    /// ProgressTextの手動テスト用メソッド
    /// </summary>
    [ContextMenu("Test ProgressText Update")]
    public void TestProgressTextUpdate()
    {
        Debug.Log("=== ProgressText テスト開始 ===");
        Debug.Log($"progressText == null: {progressText == null}");
        
        if (progressText != null)
        {
            Debug.Log($"progressText.gameObject.name: {progressText.gameObject.name}");
            Debug.Log($"progressText.gameObject.activeInHierarchy: {progressText.gameObject.activeInHierarchy}");
            Debug.Log($"現在のprogressText.text: '{progressText.text}'");
            
            // 手動でテスト値を設定
            progressText.text = "テスト: 2/5 共通鍵暗号";
            Debug.Log($"テスト後のprogressText.text: '{progressText.text}'");
            
            // 実際のUpdateProgressTextメソッドをテスト
            Debug.Log($"currentGameSet == null: {currentGameSet == null}");
            Debug.Log($"currentQuestionIndex: {currentQuestionIndex}");
            Debug.Log($"currentStepIndex: {currentStepIndex}");
            
            if (currentGameSet != null && currentQuestionIndex < currentGameSet.Length)
            {
                string cryptoName = GetCryptoTypeName(currentGameSet[currentQuestionIndex]);
                int totalSteps = CryptoQuestionDatabase.GetStepCount(currentGameSet[currentQuestionIndex]);
                Debug.Log($"cryptoName: {cryptoName}, totalSteps: {totalSteps}");
                Debug.Log($"生成される文字列: '問題 {currentStepIndex + 1}/{totalSteps} - {cryptoName}'");
                
                UpdateProgressText();
            }
        }
        Debug.Log("=== ProgressText テスト終了 ===");
    }
    
    /// <summary>
    /// currentStepIndexを手動で進めるテスト用メソッド
    /// </summary>
    [ContextMenu("Test Step Progress")]
    public void TestStepProgress()
    {
        Debug.Log("=== ステップ進行テスト ===");
        currentStepIndex++;
        Debug.Log($"currentStepIndex を {currentStepIndex} に増加");
        UpdateProgressText();
        Debug.Log("=== ステップ進行テスト終了 ===");
    }

    /// <summary>
    /// キャンバス解説システムのクイックテスト（デバッグ用）
    /// </summary>
    [ContextMenu("Quick Test Canvas Explanation")]
    public void QuickTestCanvasExplanation()
    {
        if (!enableDebugFunctions) return;
        
        string testExplanation = "🧪 【キャンバステスト解説】これはキャンバス上の専用パネルに表示される解説テストです。\n\n共通鍵暗号では送信者と受信者が同じ暗号鍵を使用してデータの暗号化と復号化を行います。この方式では鍵の安全な配布が重要な課題となります。\n\n公開鍵暗号と組み合わせることで、より安全な通信システムを構築できます。";
        
        Debug.Log("🧪 キャンバス解説システムテスト開始");
        Debug.Log($"🧪 テスト用解説内容: '{testExplanation}'");
        Debug.Log($"🧪 テスト用解説長: {testExplanation.Length}");
        
        StartCoroutine(ShowExplanationOnCanvas(testExplanation));
    }

    /// <summary>
    /// 解説パネル表示の即座テスト（デバッグ用）
    /// </summary>
    [ContextMenu("Test Explanation Panel Immediate")]
    public void TestExplanationPanelImmediate()
    {
        if (!enableDebugFunctions) return;
        
        Debug.Log("🧪 解説パネル即座テスト開始");
        StartCoroutine(ImmediateExplanationTest());
    }
    
    /// <summary>
    /// 解説テキスト表示強制テスト（デバッグ用）
    /// </summary>
    [ContextMenu("Force Show Explanation Text")]
    public void ForceShowExplanationText()
    {
        if (!enableDebugFunctions) return;
        
        Debug.Log("🔧 解説テキスト強制表示テスト");
        StartCoroutine(ForceExplanationTextTest());
    }
    
    /// <summary>
    /// 解説テキストの強制表示テスト
    /// </summary>
    private IEnumerator ForceExplanationTextTest()
    {
        string testExplanation = "【強制テスト】これは解説テキストの強制表示テストです。このテキストが表示されていれば、解説システムは正常に動作しています。";
        
        Debug.Log("🔧 解説パネル存在確認開始");
        yield return StartCoroutine(EnsureExplanationPanelExists());
        
        if (explanationPanel != null && explanationText != null)
        {
            Debug.Log("✅ 解説パネルとテキストが存在、強制表示開始");
            
            // テキストを強制設定
            explanationText.text = testExplanation;
            explanationText.color = Color.yellow; // 目立つ色に変更
            explanationText.fontSize = 36;
            explanationText.enabled = true;
            
            // パネルを強制表示
            explanationPanel.SetActive(true);
            
            // Canvas順序を最前面に
            Canvas canvas = explanationPanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = 2000;
            }
            
            Debug.Log($"🔧 強制表示設定完了 - テキスト: '{explanationText.text}'");
            
            yield return new WaitForSeconds(3f);
            
            // 元に戻す
            explanationPanel.SetActive(false);
            if (canvas != null)
            {
                canvas.sortingOrder = 0;
            }
            
            Debug.Log("✅ 強制表示テスト完了");
        }
        else
        {
            Debug.LogError("❌ 解説パネルまたはテキストが見つかりません");
        }
    }
    
    /// <summary>
    /// 即座の解説表示テスト
    /// </summary>
    private IEnumerator ImmediateExplanationTest()
    {
        string testExplanation = "🧪 これは即座テスト用の解説です。パネルが正しく表示されるかを確認します。";
        
        Debug.Log("🔧 解説パネル存在確保開始");
        yield return StartCoroutine(EnsureExplanationPanelExists());
        
        Debug.Log("📋 解説表示テスト実行");
        CoroutineResult<bool> testResult = new CoroutineResult<bool>();
        yield return StartCoroutine(DisplayExplanationPanel(testExplanation, testResult));
        bool success = testResult.Result;
        
        if (!success)
        {
            Debug.Log("🔄 フォールバック表示テスト");
            yield return StartCoroutine(ShowExplanationFallback(testExplanation));
        }
        
        Debug.Log("✅ 即座テスト完了");
    }
    
    /// <summary>
    /// 解説パネル強制再作成（デバッグ用）
    /// </summary>
    [ContextMenu("Force Recreate Explanation Panel")]
    public void ForceRecreateExplanationPanel()
    {
        if (!enableDebugFunctions) return;
        
        Debug.Log("🔧 解説パネル強制再作成開始");
        
        // 既存パネルを削除
        if (explanationPanel != null)
        {
            DestroyImmediate(explanationPanel);
            explanationPanel = null;
            explanationText = null;
            Debug.Log("🗑️ 既存パネル削除完了");
        }
        
        // 強制再作成
        StartCoroutine(CreateExplanationPanelDynamically("🔧 強制再作成テスト用の解説です。"));
    }

    /// <summary>
    /// 解説パネル問題診断テスト（デバッグ用）
    /// </summary>
    [ContextMenu("Diagnose Explanation Panel")]
    public void DiagnoseExplanationPanel()
    {
        if (!enableDebugFunctions) return;
        
        Debug.Log("🔍 解説パネル診断開始");
        StartCoroutine(DiagnoseExplanationPanelCoroutine());
    }
    
    /// <summary>
    /// 解説パネル診断のコルーチン
    /// </summary>
    private IEnumerator DiagnoseExplanationPanelCoroutine()
    {
        Debug.Log("=== 解説パネル診断レポート ===");
        
        // 1. 基本変数チェック
        Debug.Log($"1. 基本変数状態:");
        Debug.Log($"   - explanationPanel: {(explanationPanel != null ? "存在" : "null")}");
        Debug.Log($"   - explanationText: {(explanationText != null ? "存在" : "null")}");
        
        if (explanationPanel != null)
        {
            Debug.Log($"   - パネル名: {explanationPanel.name}");
            Debug.Log($"   - パネルアクティブ: {explanationPanel.activeSelf}");
            Debug.Log($"   - パネル階層アクティブ: {explanationPanel.activeInHierarchy}");
        }
        
        if (explanationText != null)
        {
            Debug.Log($"   - テキスト名: {explanationText.name}");
            Debug.Log($"   - テキスト内容: '{explanationText.text}'");
            Debug.Log($"   - テキストアクティブ: {explanationText.gameObject.activeSelf}");
        }
        
        // 2. パネル再作成テスト
        Debug.Log($"2. パネル再作成テスト:");
        yield return StartCoroutine(EnsureExplanationPanelExists());
        
        Debug.Log($"   - 再作成後 explanationPanel: {(explanationPanel != null ? "存在" : "null")}");
        Debug.Log($"   - 再作成後 explanationText: {(explanationText != null ? "存在" : "null")}");
        
        // 3. 表示テスト
        if (explanationPanel != null && explanationText != null)
        {
            Debug.Log($"3. 表示テスト実行:");
            
            string testText = "🧪 これは診断テストです。このテキストが表示されれば解説システムは動作しています。";
            
            CoroutineResult<bool> testResult = new CoroutineResult<bool>();
            yield return StartCoroutine(DisplayExplanationPanel(testText, testResult));
            
            Debug.Log($"   - 表示テスト結果: {(testResult.Result ? "成功" : "失敗")}");
        }
        else
        {
            Debug.LogError("❌ パネルまたはテキストが作成されませんでした");
        }
        
        // 4. Canvas確認
        Debug.Log($"4. Canvas状態確認:");
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        Debug.Log($"   - シーン内Canvas数: {allCanvases.Length}");
        
        for (int i = 0; i < allCanvases.Length; i++)
        {
            Canvas canvas = allCanvases[i];
            Debug.Log($"   - Canvas{i+1}: {canvas.name} (sortingOrder: {canvas.sortingOrder}, アクティブ: {canvas.gameObject.activeSelf})");
        }
        
        Debug.Log("=== 診断完了 ===");
    }

    /// <summary>
    /// 不正解時の解説表示をシミュレート（デバッグ用）
    /// </summary>
    [ContextMenu("Simulate Wrong Answer")]
    public void SimulateWrongAnswer()
    {
        if (!enableDebugFunctions) return;
        
        Debug.Log("🧪 不正解シミュレーション開始");
        StartCoroutine(SimulateWrongAnswerCoroutine());
    }
    
    /// <summary>
    /// 不正解時の解説表示シミュレーション
    /// </summary>
    private IEnumerator SimulateWrongAnswerCoroutine()
    {
        // 現在の問題から解説を取得してテスト
        if (currentGameSet != null && currentQuestionIndex < currentGameSet.Length)
        {
            CryptoType currentType = currentGameSet[currentQuestionIndex];
            CryptoQuestion currentQuestion = CryptoQuestionDatabase.GetQuestion(currentType, currentStepIndex);
            
            if (currentQuestion != null)
            {
                Debug.Log($"📝 現在の問題: {currentQuestion.questionText}");
                Debug.Log($"📝 選択肢数: {currentQuestion.answers.Length}");
                Debug.Log($"📝 解説配列: {(currentQuestion.explanations != null ? currentQuestion.explanations.Length : 0)}個");
                
                // 不正解選択肢（正解以外）をランダム選択
                int wrongAnswerIndex = 0;
                while (wrongAnswerIndex == currentQuestion.correctAnswerIndex)
                {
                    wrongAnswerIndex = UnityEngine.Random.Range(0, currentQuestion.answers.Length);
                }
                
                string simulatedExplanation = "";
                if (currentQuestion.explanations != null && wrongAnswerIndex < currentQuestion.explanations.Length)
                {
                    simulatedExplanation = currentQuestion.explanations[wrongAnswerIndex];
                }
                else
                {
                    simulatedExplanation = $"【シミュレーション】選択肢「{currentQuestion.answers[wrongAnswerIndex]}」は不正解です。正解は「{currentQuestion.answers[currentQuestion.correctAnswerIndex]}」です。";
                }
                
                Debug.Log($"🎭 シミュレーション解説: '{simulatedExplanation}'");
                
                // RetryCurrentQuestionを直接呼び出し（ゲームがアクティブな場合のみ）
                if (isGameActive)
                {
                    yield return StartCoroutine(RetryCurrentQuestion(simulatedExplanation));
                }
                else
                {
                    Debug.Log("ゲーム非アクティブのため、シミュレーション解説をスキップします");
                }
            }
            else
            {
                Debug.LogError("❌ 現在の問題が見つかりません");
            }
        }
        else
        {
            // デフォルト解説でテスト
            string defaultExplanation = "【テスト解説】これは不正解時の解説表示テストです。実際の解説パネルが正しく動作するかを確認しています。";
            Debug.Log($"🔧 デフォルト解説使用: '{defaultExplanation}'");
            if (isGameActive)
            {
                yield return StartCoroutine(RetryCurrentQuestion(defaultExplanation));
            }
            else
            {
                Debug.Log("ゲーム非アクティブのため、デフォルト解説テストをスキップします");
            }
        }
        
        Debug.Log("✅ 不正解シミュレーション完了");
    }

    /// <summary>
    /// Enterキー入力待機とゲーム再開処理
    /// </summary>
    private IEnumerator WaitForRestartInput()
    {
        Debug.Log("🎮 Enterキー入力待機開始");
        
        // Enterキーが押されるまで待機
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Debug.Log("✅ Enterキーが押されました。ゲームを再開始します。");
                break;
            }
            yield return null;
        }
        
        // ゲーム再開処理
        yield return StartCoroutine(RestartGameCoroutine());
    }
    
    /// <summary>
    /// ゲーム再開処理（コルーチン版）
    /// </summary>
    private IEnumerator RestartGameCoroutine()
    {
        Debug.Log("🔄 ゲーム再開処理開始");
        
        // UIパネルを非表示
        if (resultPanel != null) resultPanel.SetActive(false);
        if (finalResultPanel != null) finalResultPanel.SetActive(false);
        if (explanationPanel != null) explanationPanel.SetActive(false);
        
        // Canvas順序をリセット
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            canvas.sortingOrder = 0;
        }
        
        // スコアとプログレスをリセット
        currentScore = 0;
        correctAnswers = 0;
        totalQuestions = 0;
        currentQuestionIndex = 0;
        currentStepIndex = 0;
        
        // プログレス表示をリセット
        if (progressTracker != null)
        {
            progressTracker.ResetProgressForNewGame();
        }
        
        // スコア表示をリセット
        UpdateScoreDisplay();
        
        // 新しいゲームを開始
        yield return new WaitForSeconds(0.5f); // 少し待機してからスタート
        
        Debug.Log("🎯 新しいゲーム開始");
        Start();
    }

    /// <summary>
    /// ゲーム終了処理（コルーチン版）
    /// </summary>
    private IEnumerator EndGame()
    {
        Debug.Log("🏁 ゲーム終了処理開始");
        
        isGameActive = false;
        
        // 少し待機してから結果表示
        yield return new WaitForSeconds(1f);
        
        ShowResults();
    }

    /// <summary>
    /// シンプルで確実な解説表示（テキスト表示問題修正版）
    /// </summary>
    private IEnumerator ShowExplanationSimple(string explanation)
    {
        Debug.Log($"🎯 シンプル解説表示開始: '{explanation}'");
        Debug.Log($"🔍 explanation変数の詳細確認:");
        Debug.Log($"   - 長さ: {(explanation?.Length ?? 0)}");
        Debug.Log($"   - null確認: {(explanation == null ? "null" : "not null")}");
        Debug.Log($"   - 空確認: {(string.IsNullOrEmpty(explanation) ? "empty" : "not empty")}");
        Debug.Log($"   - 内容: '{explanation}'");
        
        // 既存の questionText を使用した確実な表示
        if (questionText != null)
        {
            Debug.Log($"🔍 questionText詳細確認:");
            Debug.Log($"   - 名前: {questionText.name}");
            Debug.Log($"   - 現在のテキスト: '{questionText.text}'");
            Debug.Log($"   - アクティブ: {questionText.gameObject.activeSelf}");
            Debug.Log($"   - 階層内アクティブ: {questionText.gameObject.activeInHierarchy}");
            
            // 元のテキストと色を保存
            string originalText = questionText.text;
            Color originalColor = questionText.color;
            
            // 解説内容の最終確認と安全性チェック
            string safeExplanation = string.IsNullOrEmpty(explanation) ? "解説内容が取得できませんでした" : explanation;
            string displayText = $"💡 解説\n\n{safeExplanation}\n\n⏳ 3秒後に問題を再表示します...";
            
            Debug.Log($"🔧 表示テキスト設定:");
            Debug.Log($"   - safeExplanation: '{safeExplanation}'");
            Debug.Log($"   - displayText: '{displayText}'");
            
            // 解説表示用の設定
            questionText.text = displayText;
            questionText.color = new Color(1f, 0.9f, 0.3f, 1f); // 明るい黄色
            questionText.fontSize = Math.Max(questionText.fontSize, 24); // 最低24ポイント
            
            Debug.Log($"✅ questionTextで解説表示中 - 設定後のテキスト: '{questionText.text}'");
            Debug.Log($"   - フォントサイズ: {questionText.fontSize}");
            Debug.Log($"   - 色: {questionText.color}");
            
            // 3秒間表示
            yield return new WaitForSeconds(3f);
            
            // 元に戻す
            questionText.text = originalText;
            questionText.color = originalColor;
            
            Debug.Log("✅ シンプル解説表示完了");
        }
        else
        {
            // questionText が無い場合はログのみ
            Debug.LogError($"❌ 表示手段なし - 解説内容: {explanation}");
            yield return new WaitForSeconds(2f);
        }
    }

    /// <summary>
    /// キャンバス上に専用解説パネルを表示（確実表示版）
    /// </summary>
    private IEnumerator ShowExplanationOnCanvas(string explanation)
    {
        Debug.Log($"🎯 キャンバス解説表示開始: '{explanation}'");
        
        // 安全性チェック
        string safeExplanation = string.IsNullOrEmpty(explanation) ? "解説内容が取得できませんでした" : explanation;
        
        // 既存の解説パネルを削除
        GameObject existingPanel = GameObject.Find("DynamicExplanationPanel");
        if (existingPanel != null)
        {
            DestroyImmediate(existingPanel);
            yield return new WaitForEndOfFrame();
        }
        
        // 最適なCanvasを取得
        Canvas targetCanvas = FindBestCanvas();
        if (targetCanvas == null)
        {
            Debug.LogError("❌ Canvasが見つかりません！");
            yield break;
        }
        
        Debug.Log($"✅ Canvas発見: {targetCanvas.name}");
        
        GameObject explanationPanel = null;
        bool creationSuccess = false;
        
        try
        {
            // 1. 解説パネル作成
            explanationPanel = new GameObject("DynamicExplanationPanel");
            explanationPanel.transform.SetParent(targetCanvas.transform, false);
            
            // 2. RectTransform設定（画面中央、大きめサイズ）
            RectTransform panelRect = explanationPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.2f);
            panelRect.anchorMax = new Vector2(0.9f, 0.8f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // 3. 背景画像設定
            UnityEngine.UI.Image panelImage = explanationPanel.AddComponent<UnityEngine.UI.Image>();
            panelImage.color = new Color(0.05f, 0.05f, 0.15f, 0.95f); // 濃い青
            
            // 4. 外枠設定
            UnityEngine.UI.Outline panelOutline = explanationPanel.AddComponent<UnityEngine.UI.Outline>();
            panelOutline.effectColor = Color.yellow;
            panelOutline.effectDistance = new Vector2(4, 4);
            
            // 5. 影効果
            UnityEngine.UI.Shadow panelShadow = explanationPanel.AddComponent<UnityEngine.UI.Shadow>();
            panelShadow.effectColor = new Color(0, 0, 0, 0.7f);
            panelShadow.effectDistance = new Vector2(6, -6);
            
            // 6. テキスト部分作成
            GameObject textObject = new GameObject("ExplanationText");
            textObject.transform.SetParent(explanationPanel.transform, false);
            
            // 7. テキスト用RectTransform設定
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(30f, 30f);  // 余白
            textRect.offsetMax = new Vector2(-30f, -30f);
            
            // 8. Textコンポーネント設定
            Text textComponent = textObject.AddComponent<Text>();
            textComponent.text = $"💡 解説\n\n{safeExplanation}\n\n✨ 5秒後に問題を再表示します ✨";
            textComponent.fontSize = 32;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.lineSpacing = 1.4f;
            textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
            textComponent.verticalOverflow = VerticalWrapMode.Truncate;
            
            // 9. フォント設定
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont != null)
            {
                textComponent.font = defaultFont;
                Debug.Log("✅ デフォルトフォント設定成功");
            }
            
            // 10. テキスト装飾
            UnityEngine.UI.Outline textOutline = textObject.AddComponent<UnityEngine.UI.Outline>();
            textOutline.effectColor = Color.black;
            textOutline.effectDistance = new Vector2(3, 3);
            
            // 11. Canvas順序を最前面に設定
            targetCanvas.sortingOrder = 1000;
            
            Debug.Log($"✅ 解説パネル作成完了: '{textComponent.text}'");
            Debug.Log($"   - パネルサイズ: {panelRect.sizeDelta}");
            Debug.Log($"   - テキストサイズ: {textRect.sizeDelta}");
            Debug.Log($"   - フォント: {(textComponent.font != null ? textComponent.font.name : "null")}");
            
            creationSuccess = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 解説パネル作成エラー: {e.Message}");
            Debug.LogError($"❌ スタックトレース: {e.StackTrace}");
            creationSuccess = false;
        }
        
        // パネル作成が成功した場合のみ表示処理を実行
        if (creationSuccess && explanationPanel != null)
        {
            // 12. パネル表示
            explanationPanel.SetActive(true);
            
            // 13. 表示時間待機（try-catchの外でyield return使用）
            yield return new WaitForSeconds(5f);
            
            // 14. パネル削除
            if (explanationPanel != null)
            {
                DestroyImmediate(explanationPanel);
                Debug.Log("✅ 解説パネル削除完了");
            }
            
            // 15. Canvas順序をリセット
            targetCanvas.sortingOrder = 0;
        }
        
        Debug.Log("✅ キャンバス解説表示完了");
    }

}
