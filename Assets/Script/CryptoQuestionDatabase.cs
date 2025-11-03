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
        
        // 共通鍵暗号の問題（新しい手順に対応、5問）
        questionDatabase[CryptoGameManager.CryptoType.SymmetricKey] = new List<CryptoQuestion>
        {
            // 手順1: 共通鍵を作成(エリアa)
            new CryptoQuestion
            {
                questionText = "共通鍵暗号において、送信者（エリアA）が最初に何を作成する必要がありますか？",
                answers = new string[] { "公開鍵", "共通鍵", "秘密鍵", "デジタル署名" },
                correctAnswerIndex = 1,
                explanations = new string[] 
                {
                    "❌ 公開鍵は公開鍵暗号で使用されます。",
                    "✅ 正解！共通鍵暗号では送信者と受信者が同じ鍵を共有します。",
                    "❌ 秘密鍵は公開鍵暗号で使用されます。",
                    "❌ デジタル署名は認証に使用されます。"
                },
                animationType = "create_symmetric_key_a"
            },
            
            // 手順2: 平文の暗号化(エリアa)
            new CryptoQuestion
            {
                questionText = "エリアAで作成した共通鍵を使って、平文データを暗号化します。共通鍵暗号の処理特徴は？",
                answers = new string[] { "高速で効率的", "鍵配布が簡単", "計算負荷が高い", "ネットワークが不要" },
                correctAnswerIndex = 0,
                explanations = new string[]
                {
                    "✅ 正解！共通鍵暗号は高速で大量のデータ処理に適しています。",
                    "❌ 共通鍵暗号の課題は安全な鍵配布です。",
                    "❌ 共通鍵暗号は計算負荷が低いのが特徴です。",
                    "❌ 鍵の配布にはネットワークが必要です。"
                },
                animationType = "encrypt_data_a"
            },
            
            // 手順3: 暗号文を送信(エリアaからb)
            new CryptoQuestion
            {
                questionText = "暗号化されたデータをエリアAからエリアBに送信します。この暗号文の安全性は何によって保たれますか？",
                answers = new string[] { "送信経路の暗号化", "共通鍵の秘匿性", "データの圧縮", "送信速度" },
                correctAnswerIndex = 1,
                explanations = new string[]
                {
                    "❌ 送信経路も重要ですが、根本的な安全性は鍵にあります。",
                    "✅ 正解！共通鍵が秘密に保たれている限り、暗号文は安全です。",
                    "❌ 圧縮は安全性とは関係ありません。",
                    "❌ 送信速度は安全性に影響しません。"
                },
                animationType = "transfer_encrypted_data_atob"
            },
            
            // 手順4: 共通鍵を事前に送信(エリアbの真下からもう一つの共通鍵が登場)
            new CryptoQuestion
            {
                questionText = "受信者（エリアB）が暗号文を復号するために必要なものは？",
                answers = new string[] { "新しい鍵", "送信者と同じ共通鍵", "公開鍵", "パスワード" },
                correctAnswerIndex = 1,
                explanations = new string[]
                {
                    "❌ 新しい鍵では復号できません。",
                    "✅ 正解！共通鍵暗号では暗号化と復号に同じ鍵を使用します。",
                    "❌ 公開鍵は公開鍵暗号で使用されます。",
                    "❌ パスワードだけでは復号できません。"
                },
                animationType = "show_symmetric_key_b"
            },
            
            // 手順5: 暗号文を復号(エリアb)
            new CryptoQuestion
            {
                questionText = "エリアBで共通鍵を使って復号が完了しました。共通鍵暗号の最大の課題は？",
                answers = new string[] { "暗号化が遅い", "安全な鍵配布", "計算が複雑", "データサイズが大きくなる" },
                correctAnswerIndex = 1,
                explanations = new string[]
                {
                    "❌ 共通鍵暗号は高速です。",
                    "✅ 正解！事前に安全に鍵を共有する必要があることが最大の課題です。",
                    "❌ 共通鍵暗号は計算が単純です。",
                    "❌ データサイズはあまり変わりません。"
                },
                animationType = "decrypt_data_b"
            }
        };

        // 公開鍵暗号の問題（新しい手順に対応、5問）
        questionDatabase[CryptoGameManager.CryptoType.PublicKey] = new List<CryptoQuestion>
        {
            // 手順1: 公開鍵と秘密鍵の作成（エリアb）
            new CryptoQuestion
            {
                questionText = "公開鍵暗号において、受信者（エリアB）が最初に作成するのは？",
                answers = new string[] { "共通鍵", "鍵ペア（公開鍵と秘密鍵）", "デジタル署名", "ハッシュ値" },
                correctAnswerIndex = 1,
                explanations = new string[]
                {
                    "❌ 共通鍵は共通鍵暗号で使用されます。",
                    "✅ 正解！公開鍵暗号では公開鍵と秘密鍵のペアを作成します。",
                    "❌ デジタル署名は認証に使用されます。",
                    "❌ ハッシュ値は整合性確認に使用されます。"
                },
                animationType = "show_key_pair"
            },
            
            // 手順2: 公開鍵を送信（エリアbからa）
            new CryptoQuestion
            {
                questionText = "作成した鍵ペアのうち、エリアBからエリアAに送信するのはどちらですか？",
                answers = new string[] { "秘密鍵", "公開鍵", "両方", "どちらも送信しない" },
                correctAnswerIndex = 1,
                explanations = new string[]
                {
                    "❌ 秘密鍵は絶対に他人に知られてはいけません。",
                    "✅ 正解！公開鍵は誰に知られても安全なので送信します。",
                    "❌ 秘密鍵は秘密にしておく必要があります。",
                    "❌ 公開鍵は送信する必要があります。"
                },
                animationType = "move_public_key_to_a"
            },
            
            // 手順3: 公開鍵で平文の暗号化(エリアa)
            new CryptoQuestion
            {
                questionText = "エリアAで受信した公開鍵を使って暗号化します。公開鍵暗号の特徴は？",
                answers = new string[] { "高速処理", "鍵配布が安全", "大容量データに最適", "計算負荷が低い" },
                correctAnswerIndex = 1,
                explanations = new string[]
                {
                    "❌ 公開鍵暗号は処理が重いです。",
                    "✅ 正解！公開鍵は公開しても安全なので、鍵配布の問題が解決されます。",
                    "❌ 公開鍵暗号は大容量データには不向きです。",
                    "❌ 公開鍵暗号は計算負荷が高いです。"
                },
                animationType = "transform_data_to_encrypted"
            },
            
            // 手順4: 暗号文を送信(エリアaからb)
            new CryptoQuestion
            {
                questionText = "公開鍵で暗号化されたデータをエリアAからエリアBに送信します。この暗号文を復号できるのは？",
                answers = new string[] { "公開鍵の持ち主", "秘密鍵の持ち主", "暗号化した人", "誰でも" },
                correctAnswerIndex = 1,
                explanations = new string[]
                {
                    "❌ 公開鍵では復号できません。",
                    "✅ 正解！公開鍵で暗号化されたデータは対応する秘密鍵でのみ復号できます。",
                    "❌ 暗号化した人は秘密鍵を持っていません。",
                    "❌ 秘密鍵を持つ人のみが復号できます。"
                },
                animationType = "move_encrypted_cube_to_b"
            },
            
            // 手順5: 秘密鍵で復号化（エリアb）
            new CryptoQuestion
            {
                questionText = "エリアBで秘密鍵による復号が完了しました。公開鍵暗号の利点は？",
                answers = new string[] { "処理が高速", "事前の鍵共有が不要", "計算が簡単", "データ圧縮効果" },
                correctAnswerIndex = 1,
                explanations = new string[]
                {
                    "❌ 公開鍵暗号は処理が重いです。",
                    "✅ 正解！事前に秘密の鍵を共有する必要がないのが大きな利点です。",
                    "❌ 公開鍵暗号は計算が複雑です。",
                    "❌ データ圧縮とは関係ありません。"
                },
                animationType = "decrypt_cube_at_b"
            }
        };

        // ハイブリッド暗号の問題（新しい手順に対応、8問に拡張）
        questionDatabase[CryptoGameManager.CryptoType.Hybrid] = new List<CryptoQuestion>
        {
            // 手順1: 公開鍵と秘密鍵を作成（エリアb）
            new CryptoQuestion
            {
                questionText = "ハイブリッド暗号において、受信者（エリアB）が最初に作成するのは？",
                answers = new string[] { "共通鍵のみ", "公開鍵と秘密鍵のペア", "デジタル証明書", "ハッシュ値" },
                correctAnswerIndex = 1,
                explanations = new string[]
                {
                    "❌ 共通鍵は後で使用されます。",
                    "✅ 正解！ハイブリッド暗号でも公開鍵暗号の仕組みを利用します。",
                    "❌ 証明書は認証で使用されます。",
                    "❌ ハッシュ値は整合性確認で使用されます。"
                },
                animationType = "create_hybrid_keypair_b"
            },
            
            // 手順2: 公開鍵を送信（エリアbからa）
            new CryptoQuestion
            {
                questionText = "エリアBからエリアAに公開鍵を送信する理由は？",
                answers = new string[] { "データ暗号化のため", "共通鍵の暗号化のため", "身元確認のため", "速度向上のため" },
                correctAnswerIndex = 1,
                explanations = new string[]
                {
                    "❌ データは共通鍵で暗号化されます。",
                    "✅ 正解！ハイブリッド暗号では共通鍵を公開鍵で暗号化します。",
                    "❌ 身元確認は別の仕組みです。",
                    "❌ 速度向上が目的ではありません。"
                },
                animationType = "transfer_hybrid_public_btoa"
            },
            
            // 手順3: 公開鍵で共通鍵を暗号化（エリアa）
            new CryptoQuestion
            {
                questionText = "エリアAで公開鍵を使って共通鍵を暗号化します。この方法の利点は？",
                answers = new string[] { "高速なデータ処理", "安全な鍵配布", "計算負荷の軽減", "データ圧縮" },
                correctAnswerIndex = 1,
                explanations = new string[]
                {
                    "❌ データ処理自体は共通鍵で行います。",
                    "✅ 正解！公開鍵暗号により共通鍵を安全に送信できます。",
                    "❌ 公開鍵暗号は計算負荷が高いです。",
                    "❌ データ圧縮とは関係ありません。"
                },
                animationType = "encrypt_symmetric_with_public_a"
            },
            
            // 手順4: 暗号化した鍵を送信（エリアaからb）
            new CryptoQuestion
            {
                questionText = "暗号化された共通鍵をエリアAからエリアBに送信します。受信者はどのように共通鍵を取得しますか？",
                answers = new string[] { "公開鍵で復号", "秘密鍵で復号", "パスワードで復号", "自動で復号" },
                correctAnswerIndex = 1,
                explanations = new string[]
                {
                    "❌ 公開鍵では復号できません。",
                    "✅ 正解！公開鍵で暗号化されたものは秘密鍵で復号します。",
                    "❌ パスワードは使用しません。",
                    "❌ 自動では復号できません。"
                },
                animationType = "transfer_encrypted_key_atob"
            },
            
            // 手順5: 暗号文を復号（エリアb）
            new CryptoQuestion
            {
                questionText = "エリアBで秘密鍵を使って共通鍵を復号しました。次に必要なのは？",
                answers = new string[] { "データの送信", "新しい鍵の作成", "暗号化されたデータの受信", "システムの再起動" },
                correctAnswerIndex = 2,
                explanations = new string[]
                {
                    "❌ データは送信者側で既に準備されています。",
                    "❌ 新しい鍵は必要ありません。",
                    "✅ 正解！共通鍵を取得した後、暗号化されたデータを受信します。",
                    "❌ システムの再起動は不要です。"
                },
                animationType = "decrypt_symmetric_key_b"
            },
            
            // 手順6: 平文を共通鍵で暗号化（エリアa)
            new CryptoQuestion
            {
                questionText = "エリアAで実際のデータを共通鍵で暗号化します。共通鍵を使う理由は？",
                answers = new string[] { "安全性が高い", "処理速度が速い", "鍵配布が簡単", "計算が複雑" },
                correctAnswerIndex = 1,
                explanations = new string[]
                {
                    "❌ 安全性は公開鍵暗号と同等です。",
                    "✅ 正解！大容量データの処理には高速な共通鍵暗号が適しています。",
                    "❌ 鍵配布は公開鍵暗号で解決済みです。",
                    "❌ 共通鍵暗号は計算が単純です。"
                },
                animationType = "encrypt_data_with_symmetric_a"
            },
            
            // 手順7: 暗号文を送信（エリアaからb)
            new CryptoQuestion
            {
                questionText = "共通鍵で暗号化されたデータをエリアAからエリアBに送信します。この時点で受信者が持っているのは？",
                answers = new string[] { "公開鍵のみ", "秘密鍵と共通鍵", "共通鍵のみ", "何も持っていない" },
                correctAnswerIndex = 1,
                explanations = new string[]
                {
                    "❌ 公開鍵だけでは復号できません。",
                    "✅ 正解！受信者は秘密鍵と、それで復号した共通鍵を持っています。",
                    "❌ 秘密鍵も必要です。",
                    "❌ 既に鍵を復号しています。"
                },
                animationType = "transfer_hybrid_data_atob"
            },
            
            // 手順8: 秘密鍵で復号化（エリアb）
            new CryptoQuestion
            {
                questionText = "エリアBで共通鍵を使って最終的にデータを復号しました。ハイブリッド暗号の最大の利点は？",
                answers = new string[] { "計算が簡単", "両方の暗号方式の利点を活用", "鍵が一つだけ", "ネットワーク不要" },
                correctAnswerIndex = 1,
                explanations = new string[]
                {
                    "❌ 実際には計算が複雑になります。",
                    "✅ 正解！公開鍵暗号の安全な鍵配布と共通鍵暗号の高速処理を両立しています。",
                    "❌ 実際には複数の鍵を使用します。",
                    "❌ ネットワークは必要です。"
                },
                animationType = "decrypt_hybrid_data_b"
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