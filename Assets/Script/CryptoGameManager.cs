using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

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
    
    [Header("Progress UI")]
    public Slider[] progressSliders; // 3つの暗号方式用
    public Text[] progressLabels;

    [Header("3D Animation System")]
    public CryptoAnimationManager animationManager;
    
    // ゲーム状態
    private CryptoType[] currentGameSet;
    private int currentQuestionIndex = 0;
    private int currentStepIndex = 0;
    private float gameTimer;
    private bool isGameActive = false;
    
    // スコア管理
    private int correctAnswers = 0;
    private int totalQuestions = 0;
    
    // 進捗管理
    private ProgressTracker progressTracker;
    
    public enum CryptoType
    {
        SymmetricKey,    // 共通鍵暗号
        PublicKey,       // 公開鍵暗号
        Hybrid           // ハイブリッド暗号
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
        
        StartNewGameSet();
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
        
        // 3Dオブジェクトをリセット
        if (animationManager != null)
        {
            animationManager.ResetAllObjects();
        }
        
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
        List<CryptoType> types = GetEnabledCryptoTypes();
        
        if (types.Count == 0)
        {
            Debug.LogError("有効な暗号方式がありません！");
            return new CryptoType[0];
        }
        
        // シャッフル
        for (int i = 0; i < types.Count; i++)
        {
            CryptoType temp = types[i];
            int randomIndex = UnityEngine.Random.Range(i, types.Count);
            types[i] = types[randomIndex];
            types[randomIndex] = temp;
        }
        
        return types.ToArray();
    }
    
    private void StartCurrentQuestion()
    {
        if (currentQuestionIndex >= questionsPerSet)
        {
            EndGameSet();
            return;
        }

        CryptoType currentType = currentGameSet[currentQuestionIndex];
        
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
        
        // ハイブリッド暗号の5問目（stepIndex=4）の特別デバッグ
        if (currentType == CryptoType.Hybrid && currentStepIndex == 4)
        {
            Debug.Log($"[ハイブリッド5問目デバッグ] 問題文: {question.questionText}");
            Debug.Log($"[ハイブリッド5問目デバッグ] 選択肢数: {question.answers.Length}");
            for (int i = 0; i < question.answers.Length; i++)
            {
                Debug.Log($"[ハイブリッド5問目デバッグ] 選択肢{i}: {question.answers[i]}");
            }
            Debug.Log($"[ハイブリッド5問目デバッグ] 正解インデックス: {question.correctAnswerIndex}");
            Debug.Log($"[ハイブリッド5問目デバッグ] 選択されたインデックス: {answerIndex}");
            Debug.Log($"[ハイブリッド5問目デバッグ] 判定結果: {(answerIndex == question.correctAnswerIndex ? "正解" : "不正解")}");
            
            // 問題データベースから直接データを取得して比較
            var hybridQuestions = CryptoQuestionDatabase.GetQuestion(CryptoType.Hybrid, 4);
            Debug.Log($"[直接取得テスト] 問題文: {hybridQuestions.questionText}");
            Debug.Log($"[直接取得テスト] 正解インデックス: {hybridQuestions.correctAnswerIndex}");
            Debug.Log($"[直接取得テスト] 選択肢配列: [{string.Join(", ", hybridQuestions.answers)}]");
        }
        
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
        
        // ハイブリッド5問目の特別処理を追加
        if (currentType == CryptoType.Hybrid && currentStepIndex == 4)
        {
            Debug.Log($"[ハイブリッド5問目 特別処理] 選択: {answerIndex}, 正解: {question.correctAnswerIndex}");
            Debug.Log($"[ハイブリッド5問目 特別処理] 比較結果: {answerIndex} == {question.correctAnswerIndex} = {answerIndex == question.correctAnswerIndex}");
            
            // 強制的に正解として扱う（テスト用）
            if (answerIndex == 2 && question.correctAnswerIndex == 2)
            {
                Debug.Log("[ハイブリッド5問目 強制修正] インデックス2を正解として処理");
                isCorrect = true;
            }
        }
        
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
            // explanationsの配列範囲チェックを追加
            string explanation = "";
            if (question.explanations != null && answerIndex < question.explanations.Length)
            {
                explanation = question.explanations[answerIndex];
            }
            else
            {
                explanation = "解説が見つかりません";
                Debug.LogWarning($"解説が見つかりません。answerIndex: {answerIndex}, explanations配列長: {(question.explanations?.Length ?? 0)}");
            }
            
            // 間違えた場合：即座に同じ問題を再出題
            StartCoroutine(RetryCurrentQuestion(explanation));
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
    
    private IEnumerator RetryCurrentQuestion(string explanation)
    {
        // 解説を短時間表示
        if (explanationPanel != null && explanationText != null)
        {
            explanationPanel.SetActive(true);
            explanationText.text = explanation;
            
            yield return new WaitForSeconds(2f); // 解説時間を短縮
            
            explanationPanel.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(1f); // 解説パネルがない場合
        }
        
        // UI色をリセット
        if (questionText != null)
        {
            questionText.color = Color.white;
        }
        
        // 同じ問題を再表示（ステップを進めない）
        CryptoType currentType = currentGameSet[currentQuestionIndex];
        CryptoQuestion question = CryptoQuestionDatabase.GetQuestion(currentType, currentStepIndex);
        DisplayQuestion(question);
        
        Debug.Log("同じ問題を再出題");
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
    
    private IEnumerator DelayedIncorrectAnswer(string explanation)
    {
        yield return new WaitForSeconds(1f); // フィードバック表示時間
        
        // UI色をリセット
        if (questionText != null)
        {
            questionText.color = Color.white;
        }
        
        OnIncorrectAnswer(explanation);
    }
    
    private void OnCorrectAnswer()
    {
        // 理解度を更新
        CryptoType currentType = currentGameSet[currentQuestionIndex];
        progressTracker.UpdateProgress(currentType, 20f); // 5問構成なので20%ずつ
        
        // 次のステップまたは次の問題へ
        currentStepIndex++;
        
        if (currentStepIndex >= CryptoQuestionDatabase.GetStepCount(currentType))
        {
            // 次の暗号方式へ
            currentQuestionIndex++;
            currentStepIndex = 0;
            
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
            StartCoroutine(TransitionToNextQuestion());
        }
    }
    
    private void OnIncorrectAnswer(string explanation)
    {
        StartCoroutine(ShowExplanation(explanation));
    }
    
    private IEnumerator ShowExplanation(string explanation)
    {
        explanationPanel.SetActive(true);
        explanationText.text = explanation;
        
        yield return new WaitForSeconds(3f);
        
        explanationPanel.SetActive(false);
        
        // 正解として次に進む（学習重視）
        OnCorrectAnswer();
    }
    
    private IEnumerator TransitionToNextQuestion()
    {
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
        resultPanel.SetActive(true);
        
        float accuracy = totalQuestions > 0 ? (float)correctAnswers / totalQuestions * 100f : 0f;
        string evaluation = GetEvaluation(accuracy);
        
        resultText.text = $"セット完了！\n" +
                         $"正解数: {correctAnswers}/{totalQuestions}\n" +
                         $"正解率: {accuracy:F1}%\n" +
                         $"評価: {evaluation}";
                         
        StartCoroutine(AutoRestartCountdown());
    }
    
    private string GetEvaluation(float accuracy)
    {
        if (accuracy >= 90f) return "⭐⭐⭐ Perfect!";
        if (accuracy >= 70f) return "⭐⭐ Great!";
        if (accuracy >= 50f) return "⭐ Good!";
        return "Keep Learning!";
    }
    
    private IEnumerator AutoRestartCountdown()
    {
        for (int i = 5; i > 0; i--)
        {
            resultText.text += $"\n\n{i}秒後に新しいセット開始...";
            yield return new WaitForSeconds(1f);
        }
        
        resultPanel.SetActive(false);
        StartNewGameSet();
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
                    progressSliders[i].value = progressValues[i] / 100f;
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
    
    private void UpdateProgressText()
    {
        if (progressText != null && currentGameSet != null && currentQuestionIndex < currentGameSet.Length)
        {
            string cryptoName = GetCryptoTypeName(currentGameSet[currentQuestionIndex]);
            // 暗号方式別に問題数を取得して表示
            int totalSteps = CryptoQuestionDatabase.GetStepCount(currentGameSet[currentQuestionIndex]);
            progressText.text = $"問題 {currentStepIndex + 1}/{totalSteps} - {cryptoName}";
        }
        else if (progressText == null)
        {
            Debug.LogWarning("Progress Text が割り当てられていません");
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
    
    // 外部から呼び出し可能なリスタート機能
    public void RestartGame()
    {
        StopAllCoroutines();
        resultPanel.SetActive(false);
        explanationPanel.SetActive(false);
        StartNewGameSet();
    }
}