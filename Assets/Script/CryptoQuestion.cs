using UnityEngine;

/// <summary>
/// 暗号学習ゲームの問題データ構造
/// </summary>
[System.Serializable]
public class CryptoQuestion
{
    [Header("問題テキスト")]
    [Tooltip("表示する問題文")]
    public string questionText;
    
    [Header("回答選択肢")]
    [Tooltip("回答の選択肢配列")]
    public string[] answers;
    
    [Header("正解情報")]
    [Tooltip("正解の回答インデックス（0から始まる）")]
    public int correctAnswerIndex;
    
    [Header("解説")]
    [Tooltip("各回答に対する解説（answersと同じ順序）")]
    public string[] explanations;
    
    [Header("アニメーション")]
    [Tooltip("この問題で再生するアニメーションのタイプ")]
    public string animationType;
    
    /// <summary>
    /// デフォルトコンストラクタ
    /// </summary>
    public CryptoQuestion()
    {
        questionText = "";
        answers = new string[0];
        correctAnswerIndex = 0;
        explanations = new string[0];
        animationType = "";
    }
    
    /// <summary>
    /// フルパラメータコンストラクタ
    /// </summary>
    public CryptoQuestion(string question, string[] answerOptions, int correctIndex, string[] explanationTexts, string animation = "")
    {
        questionText = question;
        answers = answerOptions;
        correctAnswerIndex = correctIndex;
        explanations = explanationTexts;
        animationType = animation;
    }
    
    /// <summary>
    /// 問題データの検証
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(questionText))
        {
            Debug.LogError("問題文が空です");
            return false;
        }
        
        if (answers == null || answers.Length == 0)
        {
            Debug.LogError("回答選択肢がありません");
            return false;
        }
        
        if (correctAnswerIndex < 0 || correctAnswerIndex >= answers.Length)
        {
            Debug.LogError($"正解インデックスが無効です: {correctAnswerIndex} (回答数: {answers.Length})");
            return false;
        }
        
        if (explanations != null && explanations.Length != answers.Length)
        {
            Debug.LogWarning($"解説の数が回答の数と一致しません。回答: {answers.Length}, 解説: {explanations.Length}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 指定されたインデックスの回答が正解かどうか
    /// </summary>
    public bool IsCorrectAnswer(int answerIndex)
    {
        return answerIndex == correctAnswerIndex;
    }
    
    /// <summary>
    /// 指定されたインデックスの解説を取得
    /// </summary>
    public string GetExplanation(int answerIndex)
    {
        if (explanations != null && answerIndex >= 0 && answerIndex < explanations.Length)
        {
            return explanations[answerIndex];
        }
        
        return IsCorrectAnswer(answerIndex) ? "正解です！" : "不正解です。";
    }
    
    /// <summary>
    /// デバッグ用の文字列表現
    /// </summary>
    public override string ToString()
    {
        return $"[CryptoQuestion] {questionText} (回答数: {answers?.Length ?? 0}, 正解: {correctAnswerIndex})";
    }
}
