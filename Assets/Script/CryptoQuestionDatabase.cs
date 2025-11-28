using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 暗号学習ゲームの問題データベース
/// アニメーション連携対応版
/// </summary>
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
        
        // 共通鍵暗号方式の問題（5問）- アニメーション順序対応
        questionDatabase[CryptoGameManager.CryptoType.SymmetricKey] = new List<CryptoQuestion>
        {
            // 1問目: 鍵生成
            new CryptoQuestion {
                questionText = "共通鍵暗号方式で最初に行うことは?",
                answers = new string[] { "鍵ペアを生成する", "同じ鍵を送受信者が共有する", "公開鍵を配布する", "セッション鍵を生成する" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "鍵ペアは公開鍵暗号で使用します",
                    "✅正解！共通鍵暗号では、送信者と受信者が同じ鍵を事前に共有する必要があります",
                    "公開鍵の配布は公開鍵暗号で使用します",
                    "セッション鍵はハイブリッド暗号で使用します"
                },
                animationType = "create_symmetric_key_a"
            },
            
            // 2問目: データ暗号化
            new CryptoQuestion {
                questionText = "共通鍵でデータを暗号化する目的は?",
                answers = new string[] { "データを圧縮する", "データを見えなくする", "データを高速化する", "データを削除する" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "圧縮は暗号化の目的ではありません",
                    "✅正解！共通鍵暗号は、データを第三者に読まれないように暗号化します",
                    "暗号化は処理速度を上げるためのものではありません",
                    "データを削除するわけではありません"
                },
                animationType = "encrypt_data_a"
            },
            
            // 3問目: 暗号化データ転送
            new CryptoQuestion {
                questionText = "暗号化されたデータは誰が読める?",
                answers = new string[] { "誰でも読める", "共通鍵を持つ人だけ", "送信者だけ", "受信者だけ" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "暗号化されたデータは誰でも読めるわけではありません",
                    "✅正解！暗号化されたデータは、同じ共通鍵を持つ人だけが復号できます",
                    "送信者だけでなく、受信者も読めます",
                    "受信者だけでなく、同じ鍵を持つ人なら読めます"
                },
                animationType = "transfer_encrypted_data_atob"
            },
            
            // 4問目: エリアBで鍵表示
            new CryptoQuestion {
                questionText = "受信者がデータを読むために必要なものは?",
                answers = new string[] { "公開鍵", "秘密鍵", "送信者と同じ共通鍵", "新しい鍵" },
                correctAnswerIndex = 2,
                explanations = new string[] {
                    "公開鍵は公開鍵暗号で使用します",
                    "秘密鍵は公開鍵暗号で使用します",
                    "✅正解！共通鍵暗号では、送信者と受信者が同じ鍵を使用します",
                    "新しい鍵では復号できません"
                },
                animationType = "show_symmetric_key_b"
            },
            
            // 5問目: データ復号
            new CryptoQuestion {
                questionText = "共通鍵暗号方式の最大の課題は?",
                answers = new string[] { "処理が遅い", "鍵の安全な共有が難しい", "暗号化できない", "復号できない" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "共通鍵暗号は処理が高速です",
                    "✅正解！共通鍵暗号の課題は、鍵をどうやって安全に相手に渡すかという「鍵配送問題」です",
                    "暗号化は可能です",
                    "復号は可能です"
                },
                animationType = "decrypt_data_b"
            }
        };
        
        // 公開鍵暗号方式の問題（5問）- アニメーション順序対応
        questionDatabase[CryptoGameManager.CryptoType.PublicKey] = new List<CryptoQuestion>
        {
            // 1問目: 鍵ペア生成
            new CryptoQuestion {
                questionText = "公開鍵暗号方式では何を最初に生成する?",
                answers = new string[] { "1つの共通鍵", "公開鍵と秘密鍵のペア", "セッション鍵", "パスワード" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "共通鍵は共通鍵暗号で使用します",
                    "✅正解！公開鍵暗号では、受信者が公開鍵と秘密鍵のペアを生成します",
                    "セッション鍵はハイブリッド暗号で使用します",
                    "パスワードとは異なります"
                },
                animationType = "show_key_pair"
            },
            
            // 2問目: 公開鍵配布
            new CryptoQuestion {
                questionText = "公開鍵はどのように扱う?",
                answers = new string[] { "厳重に秘密にする", "誰にでも公開できる", "送信者だけに渡す", "暗号化して送る" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "秘密にするのは秘密鍵です",
                    "✅正解！公開鍵は名前の通り公開しても安全で、誰にでも配布できます",
                    "送信者だけでなく誰にでも配布できます",
                    "公開鍵自体は暗号化せずに送れます"
                },
                animationType = "move_public_key_to_a"
            },
            
            // 3問目: データ暗号化
            new CryptoQuestion {
                questionText = "送信者は何を使ってデータを暗号化する?",
                answers = new string[] { "自分の秘密鍵", "受信者の公開鍵", "共通鍵", "セッション鍵" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "自分の秘密鍵では暗号化しません",
                    "✅正解！公開鍵暗号では、受信者の公開鍵でデータを暗号化します",
                    "共通鍵は共通鍵暗号で使用します",
                    "セッション鍵はハイブリッド暗号で使用します"
                },
                animationType = "transform_data_to_encrypted"
            },
            
            // 4問目: 復号権限
            new CryptoQuestion {
                questionText = "公開鍵で暗号化されたデータを復号できるのは?",
                answers = new string[] { "誰でも", "公開鍵を持つ人", "対応する秘密鍵を持つ人", "送信者だけ" },
                correctAnswerIndex = 2,
                explanations = new string[] {
                    "誰でも復号できるわけではありません",
                    "公開鍵では復号できません",
                    "✅正解！公開鍵で暗号化されたデータは、対応する秘密鍵でのみ復号できます",
                    "送信者だけでなく、秘密鍵を持つ受信者が復号します"
                },
                animationType = "move_encrypted_cube_to_b"
            },
            
            // 5問目: 利点
            new CryptoQuestion {
                questionText = "公開鍵暗号方式の利点は?",
                answers = new string[] { "処理が高速", "鍵配送問題を解決できる", "暗号強度が弱い", "鍵管理が複雑" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "処理速度は共通鍵暗号より遅いです",
                    "✅正解！公開鍵暗号は、鍵を安全に送る必要がないため、鍵配送問題を解決します",
                    "暗号強度は強いです",
                    "鍵管理はシンプルです"
                },
                animationType = "decrypt_cube_at_b"
            }
        };
        
        // ハイブリッド暗号方式の問題（8問）- アニメーション順序対応
        questionDatabase[CryptoGameManager.CryptoType.Hybrid] = new List<CryptoQuestion>
        {
            // 1問目: 鍵ペア準備
            new CryptoQuestion {
                questionText = "ハイブリッド暗号方式では最初に何を準備する?",
                answers = new string[] { "共通鍵のみ", "公開鍵と秘密鍵のペア", "セッション鍵のみ", "パスワード" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "共通鍵のみではありません",
                    "✅正解！ハイブリッド暗号では、まず受信者が公開鍵暗号の鍵ペアを生成します",
                    "セッション鍵は後で生成します",
                    "パスワードとは異なります"
                },
                animationType = "create_hybrid_keypair_b"
            },
            
            // 2問目: 公開鍵送信
            new CryptoQuestion {
                questionText = "受信者は送信者に何を送る?",
                answers = new string[] { "秘密鍵", "公開鍵", "共通鍵", "データ" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "秘密鍵は絶対に送りません",
                    "✅正解！受信者は公開鍵を送信者に送ります。公開鍵は安全に送れます",
                    "共通鍵は送信者が生成します",
                    "データは送信者が送ります"
                },
                animationType = "transfer_hybrid_public_btoa"
            },
            
            // 3問目: セッション鍵生成
            new CryptoQuestion {
                questionText = "送信者は次に何を生成する?",
                answers = new string[] { "新しい公開鍵", "共通鍵（セッション鍵）", "秘密鍵", "パスワード" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "公開鍵は既にあります",
                    "✅正解！送信者は、データ暗号化用の共通鍵（セッション鍵）を生成します",
                    "秘密鍵は受信者が持っています",
                    "パスワードとは異なります"
                },
                animationType = "create_hybrid_symmetric_key_a"
            },
            
            // 4問目: データ暗号化
            new CryptoQuestion {
                questionText = "大きなデータは何で暗号化する?",
                answers = new string[] { "公開鍵", "共通鍵（セッション鍵）", "秘密鍵", "パスワード" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "公開鍵は鍵の暗号化に使用します",
                    "✅正解！共通鍵暗号は高速なので、大きなデータの暗号化に適しています",
                    "秘密鍵は復号に使用します",
                    "パスワードとは異なります"
                },
                animationType = "encrypt_data_with_symmetric_a"
            },
            
            // 5問目: セッション鍵暗号化
            new CryptoQuestion {
                questionText = "共通鍵（セッション鍵）は何で暗号化する?",
                answers = new string[] { "別の共通鍵", "受信者の公開鍵", "送信者の秘密鍵", "暗号化しない" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "別の共通鍵では鍵配送問題が解決しません",
                    "✅正解！共通鍵を公開鍵で暗号化することで、安全に送信できます",
                    "送信者の秘密鍵では暗号化しません",
                    "暗号化しないと安全に送れません"
                },
                animationType = "encrypt_symmetric_with_public_a"
            },
            
            // 6問目: 両方転送
            new CryptoQuestion {
                questionText = "送信者は受信者に何を送る?",
                answers = new string[] { "データだけ", "鍵だけ", "暗号化されたデータと暗号化された鍵", "公開鍵" },
                correctAnswerIndex = 2,
                explanations = new string[] {
                    "データだけでは復号できません",
                    "鍵だけではデータがありません",
                    "✅正解！暗号化されたデータと、暗号化されたセッション鍵の両方を送ります",
                    "公開鍵は既に送っています"
                },
                animationType = "transfer_encrypted_data_and_session_key_to_b"
            },
            
            // 7問目: セッション鍵復号
            new CryptoQuestion {
                questionText = "受信者は最初に何を復号する?",
                answers = new string[] { "データ", "暗号化されたセッション鍵", "公開鍵", "パスワード" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "データを復号するにはまず鍵が必要です",
                    "✅正解！まず秘密鍵で暗号化されたセッション鍵を復号し、元の共通鍵を取り出します",
                    "公開鍵は復号する必要がありません",
                    "パスワードは関係ありません"
                },
                animationType = "decrypt_session_key_to_symmetric_at_b"
            },
            
            // 8問目: データ復号と利点
            new CryptoQuestion {
                questionText = "ハイブリッド暗号方式の最大の利点は?",
                answers = new string[] { 
                    "処理が遅い", 
                    "共通鍵の高速性と公開鍵の安全性を両立", 
                    "鍵管理が複雑", 
                    "暗号強度が弱い" 
                },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "処理は高速です",
                    "✅正解！ハイブリッド暗号は、共通鍵暗号の高速性と公開鍵暗号の安全性の両方の利点を活用します。実際のSSL/TLSなどでも使用されています",
                    "鍵管理は効率的です",
                    "暗号強度は強いです"
                },
                animationType = "decrypt_hybrid_data_b"
            }
        };
        
        Debug.Log($"問題データベース初期化完了: 共通鍵{questionDatabase[CryptoGameManager.CryptoType.SymmetricKey].Count}問, " +
                  $"公開鍵{questionDatabase[CryptoGameManager.CryptoType.PublicKey].Count}問, " +
                  $"ハイブリッド{questionDatabase[CryptoGameManager.CryptoType.Hybrid].Count}問");
    }
    
    /// <summary>
    /// 指定された暗号方式とステップの問題を取得
    /// </summary>
    public static CryptoQuestion GetQuestion(CryptoGameManager.CryptoType type, int stepIndex)
    {
        if (questionDatabase == null)
        {
            InitializeDatabase();
        }
        
        if (questionDatabase.ContainsKey(type))
        {
            var questions = questionDatabase[type];
            if (stepIndex >= 0 && stepIndex < questions.Count)
            {
                return questions[stepIndex];
            }
        }
        
        Debug.LogError($"問題が見つかりません: {type}, ステップ {stepIndex}");
        return null;
    }
    
    /// <summary>
    /// 指定された暗号方式の問題数を取得
    /// </summary>
    public static int GetQuestionCount(CryptoGameManager.CryptoType type)
    {
        if (questionDatabase == null)
        {
            InitializeDatabase();
        }
        
        if (questionDatabase.ContainsKey(type))
        {
            return questionDatabase[type].Count;
        }
        
        return 0;
    }
}