using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ランダムに問題を出題し、プレイヤーの回答順序を記録・判定するマネージャークラス
/// </summary>
public class QuizManager : MonoBehaviour
{
    [System.Serializable]
    public class QuizQuestion
    {
        [Tooltip("問題を出題するかどうか")]
        public bool isEnabled = true;
        
        [Tooltip("問題文")]
        public string question;
        
        [Tooltip("正解となるオブジェクトID（順番通りに並べる）")]
        public List<int> correctAnswerSequence;
        
        [Tooltip("問題の説明テキスト（オプション）")]
        [TextArea(2, 5)]
        public string description;
        
        [Tooltip("誤った順序に対するフィードバック（オプション）")]
        public List<string> wrongSequenceFeedback;
    }

    [Header("問題設定")]
    [Tooltip("問題リスト")]
    public List<QuizQuestion> questions = new List<QuizQuestion>();
    
    [Tooltip("ランダム出題を有効にする")]
    public bool randomizeQuestions = true;
    
    [Header("UI設定")]
    [Tooltip("問題文を表示するテキスト")]
    public Text questionText;
    
    [Tooltip("説明テキストを表示するUI")]
    public Text descriptionText;
    
    [Tooltip("正解時に表示するUIキャンバス")]
    public GameObject correctCanvas;
    
    [Tooltip("不正解時に表示するUIキャンバス")]
    public GameObject incorrectCanvas;
    
    [Tooltip("回答状態を表示するテキスト（何番目の選択かなど）")]
    public Text answerStatusText;
    
    [Tooltip("フィードバックを表示するテキスト")]
    public Text feedbackText;
    
    [Header("インタラクション設定")]
    [Tooltip("回答時のサウンドエフェクト")]
    public AudioClip selectSound;
    
    [Tooltip("正解時のサウンドエフェクト")]
    public AudioClip correctSound;
    
    [Tooltip("不正解時のサウンドエフェクト")]
    public AudioClip incorrectSound;

    [Header("ゲーム進行")]
    [Tooltip("正解表示時間（秒）")]
    public float resultDisplayTime = 2.0f;
    
    [Tooltip("次の問題までの待機時間（秒）")]
    public float nextQuestionDelay = 1.5f;

    // 内部変数
    private int currentQuestionIndex = -1;
    private QuizQuestion currentQuestion;
    private List<int> playerAnswerSequence = new List<int>();
    private bool isAnswering = false;
    private bool isShowingResult = false;
    private List<int> remainingQuestionIndices = new List<int>();
    private int totalAnswers = 0;
    private int correctAnswers = 0;
    private AudioSource audioSource;

    // シングルトンインスタンス
    public static QuizManager Instance { get; private set; }

    private void Awake()
    {
        // シングルトンパターン
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // AudioSourceコンポーネントを取得または追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // UIキャンバスを初期状態で非表示に
        if (correctCanvas != null)
        {
            correctCanvas.SetActive(false);
        }
        
        if (incorrectCanvas != null)
        {
            incorrectCanvas.SetActive(false);
        }
        
        // 問題インデックスのリストを初期化（有効な問題のみ）
        remainingQuestionIndices.Clear();
        for (int i = 0; i < questions.Count; i++)
        {
            // 有効な問題のみをリストに追加
            if (questions[i].isEnabled)
            {
                remainingQuestionIndices.Add(i);
            }
        }
        
        // 有効な問題がない場合は警告
        if (remainingQuestionIndices.Count == 0)
        {
            Debug.LogWarning("有効な問題がありません。少なくとも1つの問題を有効にしてください。");
        }
    }

    private void Start()
    {
        // 最初の問題を出題
        StartNextQuestion();
    }

    /// <summary>
    /// 次の問題を出題する
    /// </summary>
    public void StartNextQuestion()
    {
        // 結果表示中なら何もしない
        if (isShowingResult) return;
        
        // プレイヤーの回答をリセット
        playerAnswerSequence.Clear();
        isAnswering = true;
        
        // 残りの問題がなければ、有効な問題を再度出題可能にする
        if (remainingQuestionIndices.Count == 0)
        {
            Debug.Log("全問題を出題済み。問題リストをリセットします。");
            remainingQuestionIndices.Clear();
            for (int i = 0; i < questions.Count; i++)
            {
                // 有効な問題のみをリストに追加
                if (questions[i].isEnabled)
                {
                    remainingQuestionIndices.Add(i);
                }
            }
            
            // 有効な問題がない場合は警告して終了
            if (remainingQuestionIndices.Count == 0)
            {
                Debug.LogWarning("有効な問題がありません。少なくとも1つの問題を有効にしてください。");
                return;
            }
        }
        
        // ランダムまたは順番に問題を選択
        if (randomizeQuestions)
        {
            int randomIndex = Random.Range(0, remainingQuestionIndices.Count);
            currentQuestionIndex = remainingQuestionIndices[randomIndex];
            remainingQuestionIndices.RemoveAt(randomIndex);
        }
        else
        {
            currentQuestionIndex = remainingQuestionIndices[0];
            remainingQuestionIndices.RemoveAt(0);
        }
        
        // 現在の問題を設定
        currentQuestion = questions[currentQuestionIndex];
        
        // 問題文を表示
        if (questionText != null)
        {
            questionText.text = currentQuestion.question;
        }
        
        // 説明テキストを表示（あれば）
        if (descriptionText != null && !string.IsNullOrEmpty(currentQuestion.description))
        {
            descriptionText.text = currentQuestion.description;
            descriptionText.gameObject.SetActive(true);
        }
        else if (descriptionText != null)
        {
            descriptionText.gameObject.SetActive(false);
        }
        
        // フィードバックテキストをクリア
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
        
        // 回答状態をリセット
        UpdateAnswerStatusText();
        
        // 全てのクイズオブジェクトをリセット
        ResetAllQuizObjects();
        
        Debug.Log($"問題を出題: {currentQuestion.question}");
    }

    /// <summary>
    /// プレイヤーが選択したオブジェクトを記録する
    /// </summary>
    /// <param name="objectId">選択されたオブジェクトのID</param>
    public void RecordPlayerSelection(int objectId)
    {
        // 回答中でなければ何もしない
        if (!isAnswering || isShowingResult) return;
        
        // 選択を記録
        playerAnswerSequence.Add(objectId);
        Debug.Log($"プレイヤーが選択: オブジェクトID {objectId}");
        
        // 選択音を再生
        PlaySound(selectSound);
        
        // 回答状態を更新
        UpdateAnswerStatusText();
        
        // 回答数が正解の数に達したら判定
        if (playerAnswerSequence.Count >= currentQuestion.correctAnswerSequence.Count)
        {
            // 回答完了
            isAnswering = false;
            
            // 正解判定
            CheckAnswer();
        }
    }

    /// <summary>
    /// 回答状態テキストを更新
    /// </summary>
    private void UpdateAnswerStatusText()
    {
        if (answerStatusText != null)
        {
            answerStatusText.text = $"選択: {playerAnswerSequence.Count} / {currentQuestion.correctAnswerSequence.Count}";
        }
    }

    /// <summary>
    /// 回答が正解かチェック
    /// </summary>
    private void CheckAnswer()
    {
        bool isCorrect = true;
        
        // 長さが異なる場合は不正解
        if (playerAnswerSequence.Count != currentQuestion.correctAnswerSequence.Count)
        {
            isCorrect = false;
        }
        else
        {
            // 各要素を比較
            for (int i = 0; i < playerAnswerSequence.Count; i++)
            {
                if (playerAnswerSequence[i] != currentQuestion.correctAnswerSequence[i])
                {
                    isCorrect = false;
                    break;
                }
            }
        }
        
        // 結果表示
        ShowResult(isCorrect);
        
        // 統計を更新
        totalAnswers++;
        if (isCorrect)
        {
            correctAnswers++;
        }
        
        Debug.Log($"回答結果: {(isCorrect ? "正解" : "不正解")}");
    }

    /// <summary>
    /// 結果を表示
    /// </summary>
    /// <param name="isCorrect">正解かどうか</param>
    private void ShowResult(bool isCorrect)
    {
        isShowingResult = true;
        
        // 結果に応じたUIを表示
        if (isCorrect)
        {
            if (correctCanvas != null)
            {
                correctCanvas.SetActive(true);
            }
            
            // 正解音を再生
            PlaySound(correctSound);
            
            // フィードバックテキストを更新
            if (feedbackText != null)
            {
                feedbackText.text = "正解！";
            }
        }
        else
        {
            if (incorrectCanvas != null)
            {
                incorrectCanvas.SetActive(true);
            }
            
            // 不正解音を再生
            PlaySound(incorrectSound);
            
            // 間違った順序に対するフィードバックを表示
            if (feedbackText != null && currentQuestion.wrongSequenceFeedback != null && currentQuestion.wrongSequenceFeedback.Count > 0)
            {
                // 最初に間違えた場所を特定
                int firstWrongIndex = GetFirstWrongAnswerIndex();
                
                // フィードバックを表示（インデックスに対応するものがあれば）
                if (firstWrongIndex >= 0 && firstWrongIndex < currentQuestion.wrongSequenceFeedback.Count)
                {
                    feedbackText.text = currentQuestion.wrongSequenceFeedback[firstWrongIndex];
                }
                else
                {
                    feedbackText.text = "不正解。もう一度チャレンジしてみよう！";
                }
            }
            else if (feedbackText != null)
            {
                feedbackText.text = "不正解。もう一度チャレンジしてみよう！";
            }
        }
        
        // 一定時間後に結果表示を終了し、次の問題へ
        StartCoroutine(HideResultAfterDelay(isCorrect));
    }
    
    /// <summary>
    /// 最初に間違えた回答のインデックスを取得
    /// </summary>
    private int GetFirstWrongAnswerIndex()
    {
        for (int i = 0; i < playerAnswerSequence.Count; i++)
        {
            if (i >= currentQuestion.correctAnswerSequence.Count || 
                playerAnswerSequence[i] != currentQuestion.correctAnswerSequence[i])
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// 一定時間後に結果表示を非表示にする
    /// </summary>
    private IEnumerator HideResultAfterDelay(bool wasCorrect)
    {
        // 結果表示時間だけ待機
        yield return new WaitForSeconds(resultDisplayTime);
        
        // 結果UIを非表示
        if (wasCorrect && correctCanvas != null)
        {
            correctCanvas.SetActive(false);
        }
        else if (!wasCorrect && incorrectCanvas != null)
        {
            incorrectCanvas.SetActive(false);
        }
        
        // 次の問題までの待機時間
        yield return new WaitForSeconds(nextQuestionDelay);
        
        // 結果表示状態を解除
        isShowingResult = false;
        
        // 次の問題へ
        StartNextQuestion();
    }

    /// <summary>
    /// 全てのクイズオブジェクトをリセットする
    /// </summary>
    private void ResetAllQuizObjects()
    {
        QuizInteractiveObject[] quizObjects = FindObjectsOfType<QuizInteractiveObject>();
        foreach (var obj in quizObjects)
        {
            obj.ResetSelection();
        }
    }

    /// <summary>
    /// サウンドを再生
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// 統計情報を表示
    /// </summary>
    public string GetStatistics()
    {
        float correctRate = totalAnswers > 0 ? (float)correctAnswers / totalAnswers * 100f : 0f;
        return $"正解率: {correctRate:F1}%（{correctAnswers}/{totalAnswers}）";
    }
}