using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 暗号学習ゲームの問題データベース
/// アニメーション連携対応版
/// </summary>
public static class CryptoQuestionDatabase
{
    private static Dictionary<CryptoGameManager.CryptoType, List<CryptoQuestion>> questionDatabase;
    
    // 追加: 回答シャッフルを遅延させるためのデフォルト時間（秒）
    public static float AnswerShuffleDelay = 2.0f;

    // 追加: シャッフル要求を外部に通知するイベント
    // 呼び出し側はこのイベントを購読して実際のシャッフル処理を行うこと
    public static event Action RequestShuffle;

    // 追加: 指定の遅延後に RequestShuffle を発火するコルーチン（MonoBehaviour側で StartCoroutine して使用）
    public static IEnumerator RequestShuffleAfterDelay(float additionalDelay = 0f)
    {
        yield return new WaitForSeconds(AnswerShuffleDelay + additionalDelay);
        RequestShuffle?.Invoke();
    }
    
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
                questionText = "共通鍵暗号方式 - 第1問\n共通鍵暗号方式で\n最初に行うことは?",
                answers = new string[] { "鍵ペアを生成する", "同じ共通鍵を\n送受信者が共有する", "公開鍵を配布する", "セッション鍵を\n生成する" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "不正解！\n \n鍵ペアは公開鍵暗号で使用します",
                    "正解！\n \n送信者と受信者が同じ鍵を\n事前に共有する必要があります",
                    "不正解！\n \n公開鍵の配布は公開鍵暗号で使用します",
                    "不正解！\n \nセッション鍵はハイブリッド暗号で使用します"
                },
                animationType = "create_symmetric_key_a"
            },
            
            // 2問目: データ暗号化
            new CryptoQuestion {
                questionText = "共通鍵暗号方式 - 第2問\n共通鍵でデータを\n暗号化する目的は?",
                answers = new string[] { "データを圧縮する", "データを見えなくする", "データを高速化する", "データを削除する" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "不正解！\n \nデータ圧縮は暗号化の目的ではありません",
                    "正解！\n \n共通鍵暗号は、\nデータを第三者に読まれないように暗号化します",
                    "不正解！\n \n暗号化は処理速度を\n上げるためのものではありません",
                    "不正解！\n \nデータを削除するわけではありません"
                },
                animationType = "encrypt_data_a"
            },
            
            // 3問目: 暗号化データ転送
            new CryptoQuestion {
                questionText = "共通鍵暗号方式 - 第3問\n暗号化されたデータは\n誰が読める?",
                answers = new string[] { "誰でも読める", "共通鍵を持つ人だけ", "送信者だけ", "受信者だけ" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "不正解！\n \n暗号化されたデータは\n誰でも読めるわけではありません",
                    "正解！\n \n暗号化されたデータは、\n同じ共通鍵を持つ人だけが復号できます",
                    "不正解！\n \n送信者だけでなく、\n受信者も読めます",
                    "不正解！\n \n受信者だけでなく、\n同じ鍵を持つ人なら読めます"
                },
                animationType = "transfer_encrypted_data_atob"
            },
            
            // 4問目: エリアBで鍵表示
            new CryptoQuestion {
                questionText = "共通鍵暗号方式 - 第4問\n受信者がデータを\n読むために必要なものは?",
                answers = new string[] { "公開鍵", "秘密鍵", "送信者と同じ共通鍵", "新しい鍵" },
                correctAnswerIndex = 2,
                explanations = new string[] {
                    "不正解！\n \n公開鍵は公開鍵暗号で使用します",
                    "不正解！\n \n秘密鍵は公開鍵暗号で使用します",
                    "正解！\n \n共通鍵暗号では、\n送信者と受信者が同じ鍵を使用します",
                    "不正解！\n \n新しい鍵では復号できません"
                },
                animationType = "show_symmetric_key_b"
            },
            
            // 5問目: データ復号
            new CryptoQuestion {
                questionText = "共通鍵暗号方式 - 第5問\n共通鍵暗号方式の\n最大の課題は?",
                answers = new string[] { "処理が遅い", "鍵の安全な共有が\n難しい", "暗号化できない", "復号できない" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "不正解！\n \n共通鍵暗号は処理が高速です",
                    "正解！\n \n課題は鍵をどうやって安全に\n渡すかという「鍵配送問題」です",
                    "不正解！\n \n暗号化は可能です",
                    "不正解！\n \n復号は可能です"
                },
                animationType = "decrypt_data_b"
            }
        };
        
        // 公開鍵暗号方式の問題（5問）- アニメーション順序対応
        questionDatabase[CryptoGameManager.CryptoType.PublicKey] = new List<CryptoQuestion>
        {
            // 1問目: 鍵ペア生成
            new CryptoQuestion {
                questionText = "公開鍵暗号方式 - 第1問\n公開鍵暗号方式では\n何を最初に生成する?",
                answers = new string[] { "1つの共通鍵", "公開鍵と秘密鍵のペア", "セッション鍵", "パスワード" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "不正解！\n \n共通鍵は共通鍵暗号で使用します",
                    "正解！\n \n公開鍵暗号では、\n受信者が公開鍵と秘密鍵のペアを生成します",
                    "不正解！\n \nセッション鍵はハイブリッド暗号で使用します",
                    "不正解！\n \nパスワードとは異なります"
                },
                animationType = "show_key_pair"
            },
            
            // 2問目: 公開鍵配布
            new CryptoQuestion {
                questionText = "公開鍵暗号方式 - 第2問\n公開鍵はどのように扱う?",
                answers = new string[] { "厳重に秘密にする", "誰にでも公開できる", "送信者だけに渡す", "暗号化して送る" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "不正解！\n \n秘密にするのは秘密鍵です",
                    "正解！\n \n公開鍵は名前の通り公開しても安全で、\n誰にでも配布できます",
                    "不正解！\n \n送信者だけでなく誰にでも配布できます",
                    "不正解！\n \n公開鍵自体は暗号化せずに送れます"
                },
                animationType = "move_public_key_to_a"
            },
            
            // 3問目: データ暗号化
            new CryptoQuestion {
                questionText = "公開鍵暗号方式 - 第3問\n送信者は何を使って\nデータを暗号化する?",
                answers = new string[] { "送信者の秘密鍵", "受信者の公開鍵", "共通鍵", "セッション鍵" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "不正解！\n \n送信者の秘密鍵では暗号化しません",
                    "正解！\n \n公開鍵暗号では、\n受信者の公開鍵でデータを暗号化します",
                    "不正解！\n \n共通鍵は共通鍵暗号で使用します",
                    "不正解！\n \nセッション鍵はハイブリッド暗号で使用します"
                },
                animationType = "transform_data_to_encrypted"
            },
            
            // 4問目: 復号権限
            new CryptoQuestion {
                questionText = "公開鍵暗号方式 - 第4問\n公開鍵で暗号化されたデータを\n復号できるのは?",
                answers = new string[] { "誰でも", "公開鍵を持つ人", "対応する\n秘密鍵を持つ人", "送信者だけ" },
                correctAnswerIndex = 2,
                explanations = new string[] {
                    "不正解！\n \n誰でも復号できるわけではありません",
                    "不正解！\n \n公開鍵では復号できません",
                    "正解！\n \n公開鍵で暗号化されたデータは、\n対応する秘密鍵でのみ復号できます",
                    "不正解！\n \n送信者だけでなく、秘密鍵を持つ受信者が復号します"
                },
                animationType = "move_encrypted_cube_to_b"
            },
            
            // 5問目: 利点
            new CryptoQuestion {
                questionText = "公開鍵暗号方式 - 第5問\n公開鍵暗号方式の利点は?",
                answers = new string[] { "処理が高速", "鍵配送問題を\n解決できる", "暗号強度が弱い", "鍵管理が複雑" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "不正解！\n \n処理速度は共通鍵暗号より遅いです",
                    "正解！\n \n公開鍵暗号は、  鍵を安全に送る必要がないため、\n鍵配送問題を解決します",
                    "不正解！\n \n暗号強度は強いです",
                    "不正解！\n \n鍵管理はシンプルです"
                },
                animationType = "decrypt_cube_at_b"
            }
        };
        
        // ハイブリッド暗号方式の問題（8問）- アニメーション順序対応
        questionDatabase[CryptoGameManager.CryptoType.Hybrid] = new List<CryptoQuestion>
        {
            // 1問目: 鍵ペア準備
            new CryptoQuestion {
                questionText = "ハイブリッド暗号方式 - 第1問\nハイブリッド暗号方式では\n最初に何を準備する?",
                answers = new string[] { "共通鍵のみ", "公開鍵と秘密鍵のペア", "セッション鍵のみ", "パスワード" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "不正解！\n \n共通鍵のみではありません",
                    "正解！\n \nハイブリッド暗号では、\nまず受信者が公開鍵暗号の鍵ペアを生成します",
                    "不正解！\n \nセッション鍵は後で生成します",
                    "不正解！\n \nパスワードとは異なります"
                },
                animationType = "create_hybrid_keypair_b"
            },
            
            // 2問目: 公開鍵送信
            new CryptoQuestion {
                questionText = "ハイブリッド暗号方式 - 第2問\n受信者は送信者に何を送る?",
                answers = new string[] { "秘密鍵", "公開鍵", "共通鍵", "データ" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "不正解！\n \n秘密鍵は絶対に送りません",
                    "正解！\n \n受信者は公開鍵を送信者に送ります。\n公開鍵は安全に送れます",
                    "不正解！\n \n共通鍵は送信者が生成します",
                    "不正解！\n \nデータは送信者が送ります"
                },
                animationType = "transfer_hybrid_public_btoa"
            },
            
            // 3問目: セッション鍵生成
            new CryptoQuestion {
                questionText = "ハイブリッド暗号方式 - 第3問\n送信者は次に何を生成する?",
                answers = new string[] { "新しい公開鍵", "共通鍵\n（セッション鍵）", "秘密鍵", "パスワード" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "不正解！\n \n公開鍵は既にあります",
                    "正解！\n \n送信者は、データ暗号化用の\n共通鍵（セッション鍵）を生成します",
                    "不正解！\n \n秘密鍵は受信者が持っています",
                    "不正解！\n \nパスワードとは異なります"
                },
                animationType = "create_hybrid_symmetric_key_a"
            },
            
            // 4問目: データ暗号化
            new CryptoQuestion {
                questionText = "ハイブリッド暗号方式 - 第4問\n大きなデータは何で暗号化する?",
                answers = new string[] { "公開鍵", "共通鍵\n（セッション鍵）", "秘密鍵", "パスワード" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "不正解！\n \n公開鍵は鍵の暗号化に使用します",
                    "正解！\n \n共通鍵暗号は高速なので、\n大きなデータの暗号化に適しています",
                    "不正解！\n \n秘密鍵は復号に使用します",
                    "不正解！\n \nパスワードとは異なります"
                },
                animationType = "encrypt_data_with_symmetric_a"
            },
            
            // 5問目: セッション鍵暗号化
            new CryptoQuestion {
                questionText = "ハイブリッド暗号方式 - 第5問\n共通鍵（セッション鍵）は\n何で暗号化する?",
                answers = new string[] { "別の共通鍵", "受信者の公開鍵", "送信者の秘密鍵", "暗号化しない" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "不正解！\n \n別の共通鍵では鍵配送問題が解決しません",
                    "正解！\n \n共通鍵を受信者の公開鍵で暗号化することで、\n安全に送信できます",
                    "不正解！\n \n送信者の秘密鍵では暗号化しません",
                    "不正解！\n \n暗号化しないと安全に送れません"
                },
                animationType = "encrypt_symmetric_with_public_a"
            },
            
            // 6問目: 両方転送
            new CryptoQuestion {
                questionText = "ハイブリッド暗号方式 - 第6問\n送信者は受信者に何を送る?",
                answers = new string[] { "データだけ", "鍵だけ", "暗号化されたデータと\n暗号化された鍵", "公開鍵" },
                correctAnswerIndex = 2,
                explanations = new string[] {
                    "不正解！\n \nデータだけでは復号できません",
                    "不正解！\n \n鍵だけではデータがありません",
                    "正解！\n \n暗号化されたデータと、\n暗号化されたセッション鍵の両方を送ります",
                    "不正解！\n \n公開鍵は既に送っています"
                },
                animationType = "transfer_encrypted_data_and_session_key_to_b"
            },
            
            // 7問目: セッション鍵復号
            new CryptoQuestion {
                questionText = "ハイブリッド暗号方式 - 第7問\n受信者は最初に何を復号する?",
                answers = new string[] { "データ", "暗号化された共通鍵\n(セッション鍵)", "公開鍵", "パスワード" },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "不正解！\n \nデータを復号するにはまず鍵が必要です",
                    "正解！\n \nまず秘密鍵で暗号化された鍵を\n復号し元の共通鍵を取り出します",
                    "不正解！\n \n公開鍵は復号する必要がありません",
                    "不正解！\n \nパスワードは関係ありません"
                },
                animationType = "decrypt_session_key_to_symmetric_at_b"
            },
            
            // 8問目: データ復号と利点
            new CryptoQuestion {
                questionText = "ハイブリッド暗号方式 - 第8問\nハイブリッド方式の\n最大の利点は?",
                answers = new string[] { 
                    "処理が遅い", 
                    "共通鍵の高速性と\n公開鍵の安全性を両立", 
                    "鍵管理が複雑", 
                    "暗号強度が弱い" 
                },
                correctAnswerIndex = 1,
                explanations = new string[] {
                    "不正解！\n \n処理は高速です",
                    "正解！\n \n共通鍵暗号の高速性と公開鍵暗号の安全性の\n両方の利点を活用します。",
                    "不正解！\n \n鍵管理は効率的です",
                    "不正解！\n \n暗号強度は強いです"
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