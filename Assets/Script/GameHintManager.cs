using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameHintManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("ヒント内容を表示するテキスト")]
    public Text hintContentText;
    
    [Tooltip("ヒントタイトルを表示するテキスト")]
    public Text hintTitleText;
    
    [Tooltip("ヒントカテゴリボタン配列")]
    public Button[] categoryButtons;
    
    [Tooltip("ヒント内選択ボタン配列")]
    public Button[] hintSelectionButtons;
    
    [Tooltip("戻るボタン")]
    public Button backButton;
    
    [Tooltip("メインメニューに戻るボタン")]
    public Button mainMenuButton;
    
    [Header("UI Panels")]
    [Tooltip("カテゴリ選択パネル")]
    public GameObject categoryPanel;
    
    [Tooltip("ヒント表示パネル")]
    public GameObject hintDisplayPanel;
    
    [Tooltip("ヒント選択パネル")]
    public GameObject hintSelectionPanel;
    
    [Header("Visual Settings")]
    [Tooltip("選択中ボタンの色")]
    public Color selectedButtonColor = Color.yellow;
    
    [Tooltip("通常ボタンの色")]
    public Color normalButtonColor = Color.white;
    
    // 現在の状態管理
    private HintCategory currentCategory = HintCategory.None;
    private int currentHintIndex = 0;
    
    // ヒントデータ
    private Dictionary<HintCategory, List<HintData>> hintDatabase;
    
    public enum HintCategory
    {
        None,
        SymmetricKey,    // 共通鍵暗号
        PublicKey,       // 公開鍵暗号
        Hybrid,          // ハイブリッド暗号
        GameControls,    // ゲーム操作
        General          // 一般的なヒント
    }
    
    [System.Serializable]
    public class HintData
    {
        public string title;
        [TextArea(3, 6)]
        public string content;
        
        public HintData(string title, string content)
        {
            this.title = title;
            this.content = content;
        }
    }
    
    void Start()
    {
        InitializeHintDatabase();
        SetupButtons();
        
        // PlayerPrefsから開始カテゴリを確認
        if (PlayerPrefs.HasKey("HintCategory"))
        {
            int categoryIndex = PlayerPrefs.GetInt("HintCategory");
            HintCategory category = (HintCategory)categoryIndex;
            SelectCategory(category);
        }
        else
        {
            ShowCategorySelection();
        }
    }
    
    /// <summary>
    /// ヒントデータベースを初期化
    /// </summary>
    private void InitializeHintDatabase()
    {
        hintDatabase = new Dictionary<HintCategory, List<HintData>>();
        
        // 共通鍵暗号のヒント
        hintDatabase[HintCategory.SymmetricKey] = new List<HintData>
        {
            new HintData("共通鍵暗号とは？", 
                "共通鍵暗号は、暗号化と復号に同じ鍵を使用する暗号方式です。\n\n" +
                "特徴：\n" +
                "• 暗号化が高速\n" +
                "• 鍵の配送が課題\n" +
                "• 対称暗号とも呼ばれる\n\n" +
                "代表例：AES、DES"),
                
            new HintData("鍵配送問題", 
                "共通鍵暗号の最大の課題は、安全に鍵を配送することです。\n\n" +
                "問題点：\n" +
                "• 事前に安全な経路で鍵を共有する必要がある\n" +
                "• 通信相手が多いと鍵管理が複雑\n" +
                "• 鍵が漏洩すると全ての通信が危険\n\n" +
                "解決策：公開鍵暗号との組み合わせ"),
                
            new HintData("ゲーム内での表現", 
                "ゲーム内では以下のように表現されます：\n\n" +
                "1. 同じ鍵で暗号化・復号\n" +
                "2. 鍵の事前共有が必要\n" +
                "3. 高速な処理\n\n" +
                "注目ポイント：\n" +
                "• 鍵が一つだけ表示される\n" +
                "• 送信者と受信者が同じ鍵を使用")
        };
        
        // 公開鍵暗号のヒント
        hintDatabase[HintCategory.PublicKey] = new List<HintData>
        {
            new HintData("公開鍵暗号とは？", 
                "公開鍵暗号は、暗号化と復号に異なる鍵を使用する暗号方式です。\n\n" +
                "特徴：\n" +
                "• 公開鍵と秘密鍵のペア\n" +
                "• 鍵配送問題を解決\n" +
                "• 非対称暗号とも呼ばれる\n\n" +
                "代表例：RSA、楕円曲線暗号"),
                
            new HintData("鍵ペアの仕組み", 
                "公開鍵暗号では2つの鍵を使用します：\n\n" +
                "公開鍵：\n" +
                "• 誰でもアクセス可能\n" +
                "• 暗号化に使用\n" +
                "• 公開しても安全\n\n" +
                "秘密鍵：\n" +
                "• 所有者のみが保持\n" +
                "• 復号に使用\n" +
                "• 絶対に秘匿"),
                
            new HintData("暗号化の流れ", 
                "公開鍵暗号での通信手順：\n\n" +
                "1. 受信者が鍵ペアを生成\n" +
                "2. 公開鍵を送信者に提供\n" +
                "3. 送信者が公開鍵で暗号化\n" +
                "4. 受信者が秘密鍵で復号\n\n" +
                "ゲーム内では鍵の生成→配布→使用の流れを観察しましょう！"),
                
            new HintData("ゲーム内での表現", 
                "ゲーム内では以下に注目：\n\n" +
                "• 2つの鍵（公開鍵・秘密鍵）が表示\n" +
                "• 鍵ペアの生成シーン\n" +
                "• 公開鍵の配布過程\n" +
                "• 異なる鍵での暗号化・復号\n\n" +
                "色分けされた鍵で区別されています！")
        };
        
        // ハイブリッド暗号のヒント
        hintDatabase[HintCategory.Hybrid] = new List<HintData>
        {
            new HintData("ハイブリッド暗号とは？", 
                "ハイブリッド暗号は、共通鍵暗号と公開鍵暗号を組み合わせた暗号方式です。\n\n" +
                "目的：\n" +
                "• 両方の長所を活用\n" +
                "• 短所を補完\n" +
                "• 実用的なセキュリティ\n\n" +
                "現在のインターネット通信の主流方式です！"),
                
            new HintData("ハイブリッドの仕組み", 
                "2段階の暗号化を行います：\n\n" +
                "第1段階（公開鍵暗号）：\n" +
                "• セッション鍵を暗号化\n" +
                "• 鍵配送問題を解決\n\n" +
                "第2段階（共通鍵暗号）：\n" +
                "• 実際のデータを暗号化\n" +
                "• 高速処理を実現\n\n" +
                "両方の利点を獲得！"),
                
            new HintData("暗号化の手順", 
                "ハイブリッド暗号の詳細手順：\n\n" +
                "1. セッション鍵（共通鍵）を生成\n" +
                "2. データをセッション鍵で暗号化\n" +
                "3. セッション鍵を公開鍵で暗号化\n" +
                "4. 両方を送信\n\n" +
                "復号は逆の手順で行います"),
                
            new HintData("復号の手順", 
                "受信側での復号手順：\n\n" +
                "1. 秘密鍵でセッション鍵を復号\n" +
                "2. 復号したセッション鍵でデータを復号\n" +
                "3. 元のデータを取得\n\n" +
                "ゲーム内では2段階の復号過程を観察できます！"),
                
            new HintData("実世界での利用", 
                "ハイブリッド暗号の実用例：\n\n" +
                "• HTTPS通信\n" +
                "• メール暗号化\n" +
                "• VPN通信\n" +
                "• オンラインバンキング\n\n" +
                "私たちの日常生活で広く使われています！")
        };
        
        // ゲーム操作のヒント
        hintDatabase[HintCategory.GameControls] = new List<HintData>
        {
            new HintData("基本操作", 
                "ゲームの基本操作方法：\n\n" +
                "移動：\n" +
                "• WASD または 矢印キー\n" +
                "• マウスでカメラ回転\n\n" +
                "回答：\n" +
                "• 3D回答キューブに触れる\n" +
                "• または画面上のボタンをクリック"),
                
            new HintData("問題の進め方", 
                "効果的な学習方法：\n\n" +
                "1. 問題文をよく読む\n" +
                "2. 3Dアニメーションを観察\n" +
                "3. 鍵の動きに注目\n" +
                "4. 暗号化の流れを理解\n" +
                "5. 答えを選択\n\n" +
                "焦らずじっくり観察しましょう！"),
                
            new HintData("リスポーン機能", 
                "正解時のプレイヤーリセット：\n\n" +
                "• 正解すると自動的に初期位置に戻る\n" +
                "• 次の問題に集中できる\n" +
                "• 回答キューブから離れる\n\n" +
                "設定で高さや位置を調整可能です！")
        };
        
        // 一般的なヒント
        hintDatabase[HintCategory.General] = new List<HintData>
        {
            new HintData("学習のコツ", 
                "効果的な暗号学習法：\n\n" +
                "• アニメーションを注意深く観察\n" +
                "• 鍵の動きと役割を理解\n" +
                "• 間違いを恐れずに挑戦\n" +
                "• 解説をしっかり読む\n\n" +
                "繰り返し学習が重要です！"),
                
            new HintData("暗号の重要性", 
                "現代社会における暗号：\n\n" +
                "• 個人情報の保護\n" +
                "• オンライン取引の安全性\n" +
                "• 国家機密の保護\n" +
                "• デジタル社会の基盤\n\n" +
                "暗号技術は現代の必須知識です！"),
                
            new HintData("さらなる学習", 
                "このゲーム後の学習リソース：\n\n" +
                "• 暗号理論の専門書\n" +
                "• オンライン暗号学コース\n" +
                "• セキュリティ関連の資格\n" +
                "• 実装練習\n\n" +
                "継続的な学習で専門知識を深めましょう！")
        };
    }
    
    /// <summary>
    /// ボタンのイベント設定
    /// </summary>
    private void SetupButtons()
    {
        // カテゴリボタンの設定
        if (categoryButtons.Length >= 5)
        {
            categoryButtons[0].onClick.AddListener(() => SelectCategory(HintCategory.SymmetricKey));
            categoryButtons[1].onClick.AddListener(() => SelectCategory(HintCategory.PublicKey));
            categoryButtons[2].onClick.AddListener(() => SelectCategory(HintCategory.Hybrid));
            categoryButtons[3].onClick.AddListener(() => SelectCategory(HintCategory.GameControls));
            categoryButtons[4].onClick.AddListener(() => SelectCategory(HintCategory.General));
        }
        
        // ヒント選択ボタンの設定
        for (int i = 0; i < hintSelectionButtons.Length; i++)
        {
            int index = i; // クロージャ対応
            hintSelectionButtons[i].onClick.AddListener(() => ShowHint(index));
        }
        
        // 戻るボタン
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBack);
        }
        
        // メインメニューボタン
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
    }
    
    /// <summary>
    /// カテゴリ選択画面を表示
    /// </summary>
    public void ShowCategorySelection()
    {
        currentCategory = HintCategory.None;
        
        if (categoryPanel != null) categoryPanel.SetActive(true);
        if (hintSelectionPanel != null) hintSelectionPanel.SetActive(false);
        if (hintDisplayPanel != null) hintDisplayPanel.SetActive(false);
        
        // カテゴリボタンのテキスト設定
        if (categoryButtons.Length >= 5)
        {
            SetButtonText(categoryButtons[0], "共通鍵暗号");
            SetButtonText(categoryButtons[1], "公開鍵暗号");
            SetButtonText(categoryButtons[2], "ハイブリッド暗号");
            SetButtonText(categoryButtons[3], "ゲーム操作");
            SetButtonText(categoryButtons[4], "一般的なヒント");
        }
    }
    
    /// <summary>
    /// カテゴリを選択
    /// </summary>
    public void SelectCategory(HintCategory category)
    {
        currentCategory = category;
        ShowHintSelection();
    }
    
    /// <summary>
    /// ヒント選択画面を表示
    /// </summary>
    private void ShowHintSelection()
    {
        if (categoryPanel != null) categoryPanel.SetActive(false);
        if (hintSelectionPanel != null) hintSelectionPanel.SetActive(true);
        if (hintDisplayPanel != null) hintDisplayPanel.SetActive(false);
        
        // 選択されたカテゴリのヒント一覧を表示
        if (hintDatabase.ContainsKey(currentCategory))
        {
            List<HintData> hints = hintDatabase[currentCategory];
            
            for (int i = 0; i < hintSelectionButtons.Length; i++)
            {
                if (i < hints.Count)
                {
                    hintSelectionButtons[i].gameObject.SetActive(true);
                    SetButtonText(hintSelectionButtons[i], hints[i].title);
                }
                else
                {
                    hintSelectionButtons[i].gameObject.SetActive(false);
                }
            }
        }
    }
    
    /// <summary>
    /// 指定されたヒントを表示
    /// </summary>
    public void ShowHint(int hintIndex)
    {
        if (!hintDatabase.ContainsKey(currentCategory)) return;
        
        List<HintData> hints = hintDatabase[currentCategory];
        if (hintIndex >= hints.Count) return;
        
        currentHintIndex = hintIndex;
        HintData hint = hints[hintIndex];
        
        if (categoryPanel != null) categoryPanel.SetActive(false);
        if (hintSelectionPanel != null) hintSelectionPanel.SetActive(false);
        if (hintDisplayPanel != null) hintDisplayPanel.SetActive(true);
        
        if (hintTitleText != null) hintTitleText.text = hint.title;
        if (hintContentText != null) hintContentText.text = hint.content;
    }
    
    /// <summary>
    /// 戻る処理
    /// </summary>
    public void GoBack()
    {
        if (hintDisplayPanel != null && hintDisplayPanel.activeSelf)
        {
            // ヒント表示→ヒント選択
            ShowHintSelection();
        }
        else if (hintSelectionPanel != null && hintSelectionPanel.activeSelf)
        {
            // ヒント選択→カテゴリ選択
            ShowCategorySelection();
        }
        else
        {
            // カテゴリ選択→元のシーンに戻る
            ReturnToPreviousScene();
        }
    }
    
    /// <summary>
    /// メインメニューに戻る
    /// </summary>
    public void GoToMainMenu()
    {
        // メインメニューシーンがあれば戻る、なければ元のシーンに戻る
        string mainMenuScene = "MainMenu";
        if (Application.CanStreamedLevelBeLoaded(mainMenuScene))
        {
            SceneManager.LoadScene(mainMenuScene);
        }
        else
        {
            ReturnToPreviousScene();
        }
    }
    
    /// <summary>
    /// ボタンのテキストを設定
    /// </summary>
    private void SetButtonText(Button button, string text)
    {
        if (button != null)
        {
            Text buttonText = button.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = text;
            }
        }
    }
    
    /// <summary>
    /// 外部からカテゴリを設定（他シーンからの呼び出し用）
    /// </summary>
    public void SetCategoryFromExternal(int categoryIndex)
    {
        HintCategory category = (HintCategory)(categoryIndex + 1);
        SelectCategory(category);
    }
    
    /// <summary>
    /// 元のシーンに戻る
    /// </summary>
    private void ReturnToPreviousScene()
    {
        string returnScene = PlayerPrefs.GetString("ReturnScene", "SampleScene");
        
        Debug.Log($"[GameHintManager] Returning to scene: {returnScene}");
        
        try
        {
            // PlayerPrefsをクリア
            PlayerPrefs.DeleteKey("HintCategory");
            PlayerPrefs.Save();
            
            if (Application.CanStreamedLevelBeLoaded(returnScene))
            {
                SceneManager.LoadScene(returnScene);
            }
            else
            {
                Debug.LogWarning($"[GameHintManager] Cannot return to '{returnScene}', loading first scene in build settings.");
                SceneManager.LoadScene(0);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameHintManager] Error returning to previous scene: {e.Message}");
            SceneManager.LoadScene(0);
        }
    }
}
