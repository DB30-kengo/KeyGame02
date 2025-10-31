using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CryptoQuestion
{
    public string questionText;
    public string[] answers;
    public int correctAnswerIndex;
    public string[] explanations; // 各選択肢の解説
    
    // 3D演出情報を追加
    public string animationType; // "encrypt", "decrypt", "transfer_key", "transfer_data", "none"
    public string[] animationTargets; // 演出対象オブジェクト
    public Vector3[] targetPositions; // 移動先座標
}

public static class CryptoQuestionDatabase
{
    private static Dictionary<CryptoGameManager.CryptoType, List<CryptoQuestion>> questionDatabase;
    
    static CryptoQuestionDatabase()
    {
        InitializeDatabase();
    }
    
    private static void InitializeDatabase()
    {
        questionDatabase = new Dictionary<CryptoGameManager.CryptoType, List<CryptoQuestion>>();
        
        // 共通鍵暗号の問題（5問に拡張）
        questionDatabase[CryptoGameManager.CryptoType.SymmetricKey] = new List<CryptoQuestion>
        {
            new CryptoQuestion
            {
                questionText = "共通鍵暗号で使用する鍵は？",
                answers = new string[] { "共通鍵", "公開鍵ペア" },
                correctAnswerIndex = 0,
                explanations = new string[] 
                { 
                    "", 
                    "❌ 共通鍵暗号では同じ鍵を使用\n\n公開鍵ペアは公開鍵暗号で使用します\n\n💡 共通鍵暗号 = 1つの鍵で暗号化と復号" 
                },
                animationType = "show_symmetric_key",
                animationTargets = new string[] { "SymmetricKey" },
                targetPositions = new Vector3[] { new Vector3(0, 2, 0) }
            },
            new CryptoQuestion
            {
                questionText = "データを暗号化するには？",
                answers = new string[] { "共通鍵で暗号化", "公開鍵で暗号化" },
                correctAnswerIndex = 0,
                explanations = new string[] 
                { 
                    "", 
                    "❌ 共通鍵暗号では共通鍵を使用\n\n同じ鍵で暗号化と復号を行います" 
                },
                animationType = "encrypt_data",
                animationTargets = new string[] { "DataCube", "SymmetricKey" },
                targetPositions = new Vector3[] { new Vector3(-2, 1, 0), new Vector3(-2, 2, 0) }
            },
            new CryptoQuestion
            {
                questionText = "暗号化されたデータの見た目は？",
                answers = new string[] { "読めない形に変化", "元のまま" },
                correctAnswerIndex = 0,
                explanations = new string[] 
                { 
                    "", 
                    "❌ 暗号化により内容が変化\n\n暗号化されたデータは元の形では読み取れません" 
                },
                animationType = "transform_encrypted",
                animationTargets = new string[] { "DataCube" },
                targetPositions = new Vector3[] { new Vector3(-2, 1, 0) }
            },
            new CryptoQuestion
            {
                questionText = "鍵の送信方法は？",
                answers = new string[] { "暗号文と一緒に", "事前配布済み" },
                correctAnswerIndex = 1,
                explanations = new string[] 
                { 
                    "❌ 鍵配送問題が発生！\n\n鍵と暗号文を同じ経路で送ると\n盗聴者に両方見られます\n\n💡 解決策: 事前に安全な方法で鍵を配布",
                    "" 
                },
                animationType = "transfer_key_secure",
                animationTargets = new string[] { "SymmetricKey" },
                targetPositions = new Vector3[] { new Vector3(5, 1, 0) }
            },
            new CryptoQuestion
            {
                questionText = "受信者は何で復号する？",
                answers = new string[] { "同じ共通鍵", "別の鍵" },
                correctAnswerIndex = 0,
                explanations = new string[] 
                { 
                    "", 
                    "❌ 共通鍵暗号では同じ鍵を使用\n\n暗号化と復号に同じ鍵を使うのが特徴です" 
                },
                animationType = "decrypt_data",
                animationTargets = new string[] { "DataCube", "SymmetricKey" },
                targetPositions = new Vector3[] { new Vector3(5, 1, 0), new Vector3(5, 2, 0) }
            }
        };
        
        // 公開鍵暗号の問題（5問に拡張）
        questionDatabase[CryptoGameManager.CryptoType.PublicKey] = new List<CryptoQuestion>
        {
            new CryptoQuestion
            {
                questionText = "公開鍵暗号で使用する鍵は？",
                answers = new string[] { "共通鍵", "公開鍵ペア" },
                correctAnswerIndex = 1,
                explanations = new string[] 
                { 
                    "❌ 公開鍵暗号には鍵ペアが必要\n\n共通鍵では暗号化と復号に\n同じ鍵を使用します\n\n💡 公開鍵暗号 = 異なる鍵で暗号化と復号",
                    "" 
                },
                animationType = "show_key_pair",
                animationTargets = new string[] { "PublicKey", "PrivateKey" },
                targetPositions = new Vector3[] { new Vector3(-1, 2, 0), new Vector3(1, 2, 0) }
            },
            new CryptoQuestion
            {
                questionText = "データ暗号化に使う鍵は？",
                answers = new string[] { "自分の秘密鍵", "相手の公開鍵" },
                correctAnswerIndex = 1,
                explanations = new string[] 
                { 
                    "❌ なりすまし問題が発生！\n\n秘密鍵で暗号化すると\n誰でも公開鍵で復号できます\n\n💡 正解: 相手の公開鍵で暗号化\n→相手のみ復号可能",
                    "" 
                },
                animationType = "encrypt_with_public",
                animationTargets = new string[] { "DataCube", "PublicKey" },
                targetPositions = new Vector3[] { new Vector3(-2, 1, 0), new Vector3(-2, 2, 0) }
            },
            new CryptoQuestion
            {
                questionText = "公開鍵の配布方法は？",
                answers = new string[] { "秘密にする", "公開する" },
                correctAnswerIndex = 1,
                explanations = new string[] 
                { 
                    "❌ 公開鍵は文字通り公開\n\n公開鍵を秘密にすると\n誰も暗号化できません\n\n💡 公開鍵は自由に配布可能",
                    "" 
                },
                animationType = "transfer_public_key",
                animationTargets = new string[] { "PublicKey" },
                targetPositions = new Vector3[] { new Vector3(0, 1, 3) }
            },
            new CryptoQuestion
            {
                questionText = "復号に使う鍵は？",
                answers = new string[] { "自分の秘密鍵", "相手の公開鍵" },
                correctAnswerIndex = 0,
                explanations = new string[] 
                { 
                    "",
                    "❌ 復号には秘密鍵が必要\n\n公開鍵で暗号化されたデータは\n対応する秘密鍵でのみ復号可能\n\n💡 暗号化=相手の公開鍵\n復号=自分の秘密鍵" 
                },
                animationType = "decrypt_with_private",
                animationTargets = new string[] { "DataCube", "PrivateKey" },
                targetPositions = new Vector3[] { new Vector3(5, 1, 0), new Vector3(5, 2, 0) }
            },
            new CryptoQuestion
            {
                questionText = "秘密鍵の管理方法は？",
                answers = new string[] { "厳重に秘匿", "自由に配布" },
                correctAnswerIndex = 0,
                explanations = new string[] 
                { 
                    "", 
                    "❌ 秘密鍵が漏れると危険\n\n秘密鍵を知られると\n暗号が解読されてしまいます\n\n💡 秘密鍵は絶対に秘匿" 
                },
                animationType = "secure_private_key",
                animationTargets = new string[] { "PrivateKey" },
                targetPositions = new Vector3[] { new Vector3(5, 0, 0) }
            }
        };
        
        // ハイブリッド暗号の問題（5問に拡張）
        questionDatabase[CryptoGameManager.CryptoType.Hybrid] = new List<CryptoQuestion>
        {
            new CryptoQuestion
            {
                questionText = "大きなデータの暗号化方式は？",
                answers = new string[] { "公開鍵で直接", "セッション鍵で" },
                correctAnswerIndex = 1,
                explanations = new string[] 
                { 
                    "❌ 処理速度の問題！\n\n大きなデータを公開鍵暗号で\n直接暗号化すると非常に遅い\n\n💡 解決策: 高速な共通鍵暗号\n（セッション鍵）を使用",
                    "" 
                },
                animationType = "show_session_key",
                animationTargets = new string[] { "SessionKey" },
                targetPositions = new Vector3[] { new Vector3(0, 2, 0) }
            },
            new CryptoQuestion
            {
                questionText = "セッション鍵でデータを？",
                answers = new string[] { "暗号化する", "復号化する" },
                correctAnswerIndex = 0,
                explanations = new string[] 
                { 
                    "", 
                    "❌ まず暗号化が必要\n\nセッション鍵を使って\nデータを暗号化します" 
                },
                animationType = "encrypt_with_session",
                animationTargets = new string[] { "DataCube", "SessionKey" },
                targetPositions = new Vector3[] { new Vector3(-2, 1, 0), new Vector3(-2, 2, 0) }
            },
            new CryptoQuestion
            {
                questionText = "セッション鍵の送信方法は？",
                answers = new string[] { "平文で送信", "公開鍵暗号で" },
                correctAnswerIndex = 1,
                explanations = new string[] 
                { 
                    "❌ セッション鍵が盗聴される！\n\nセッション鍵が平文だと\nデータも復号されてしまいます\n\n💡 正解: 公開鍵暗号で\nセッション鍵を保護",
                    "" 
                },
                animationType = "encrypt_session_key",
                animationTargets = new string[] { "SessionKey", "PublicKey" },
                targetPositions = new Vector3[] { new Vector3(0, 1, 0), new Vector3(0, 2, 0) }
            },
            new CryptoQuestion
            {
                questionText = "受信者の復号順序は？",
                answers = new string[] { "セッション鍵→データ", "データ→セッション鍵" },
                correctAnswerIndex = 0,
                explanations = new string[] 
                { 
                    "",
                    "❌ 復号順序が間違い！\n\n最初にセッション鍵を復号しないと\nデータを復号できません\n\n💡 正しい順序:\n1.セッション鍵復号\n2.データ復号" 
                },
                animationType = "decrypt_sequence",
                animationTargets = new string[] { "SessionKey", "DataCube", "PrivateKey" },
                targetPositions = new Vector3[] { new Vector3(3, 2, 0), new Vector3(5, 1, 0), new Vector3(5, 2, 0) }
            },
            new CryptoQuestion
            {
                questionText = "ハイブリッド暗号の利点は？",
                answers = new string[] { "高速＋安全", "低速だが安全" },
                correctAnswerIndex = 0,
                explanations = new string[] 
                { 
                    "", 
                    "❌ ハイブリッドの特徴は速度\n\n共通鍵の高速性と\n公開鍵の安全性を両立\n\n💡 大きなデータでも高速暗号化" 
                },
                animationType = "show_advantages",
                animationTargets = new string[] { "DataCube", "SessionKey", "PublicKey" },
                targetPositions = new Vector3[] { new Vector3(5, 1, 0), new Vector3(3, 2, 0), new Vector3(1, 2, 0) }
            }
        };
    }
    
    public static CryptoQuestion GetQuestion(CryptoGameManager.CryptoType cryptoType, int stepIndex)
    {
        if (questionDatabase.ContainsKey(cryptoType))
        {
            var questions = questionDatabase[cryptoType];
            if (stepIndex < questions.Count)
            {
                return questions[stepIndex];
            }
        }
        
        // フォールバック
        return new CryptoQuestion
        {
            questionText = "問題が見つかりません",
            answers = new string[] { "はい", "いいえ" },
            correctAnswerIndex = 0,
            explanations = new string[] { "", "問題データエラー" }
        };
    }
    
    public static int GetStepCount(CryptoGameManager.CryptoType cryptoType)
    {
        if (questionDatabase.ContainsKey(cryptoType))
        {
            return questionDatabase[cryptoType].Count;
        }
        return 1;
    }
    
    // ランダムな問題バリエーションを追加する機能
    public static void AddQuestionVariation(CryptoGameManager.CryptoType cryptoType, CryptoQuestion question)
    {
        if (questionDatabase.ContainsKey(cryptoType))
        {
            questionDatabase[cryptoType].Add(question);
        }
    }
    
    // 将来的な拡張用：難易度別問題取得
    public static CryptoQuestion GetQuestionByDifficulty(CryptoGameManager.CryptoType cryptoType, int stepIndex, int difficulty)
    {
        // 現在は基本実装のみ
        return GetQuestion(cryptoType, stepIndex);
    }
}