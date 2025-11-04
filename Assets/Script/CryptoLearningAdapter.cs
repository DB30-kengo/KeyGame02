using UnityEngine;
using UnityEngine.UI;

public class CryptoLearningAdapter : MonoBehaviour
{
    [Header("暗号学習ゲーム統合")]
    public CryptoGameManager cryptoGameManager;
    public SequentialObjectsManager sequentialManager;
    
    [Header("3D回答オブジェクト")]
    public GameObject answerCube1;
    public GameObject answerCube2;
    public GameObject answerCube3;  // 新しく追加
    public GameObject answerCube4;  // 新しく追加
    public TextMesh answerText1;
    public TextMesh answerText2;
    public TextMesh answerText3;    // 新しく追加
    public TextMesh answerText4;    // 新しく追加
    
    [Header("UI統合")]
    public GameObject cryptoUI;
    public GameObject originalUI;
    
    private bool isInCryptoMode = false;
    
    private void Start()
    {
        // 暗号学習モードと通常モードを切り替え
        SetCryptoMode(false);
    }
    
    public void StartCryptoLearning()
    {
        SetCryptoMode(true);
        
        if (cryptoGameManager != null)
        {
            cryptoGameManager.StartNewGameSet();
        }
    }
    
    public void ExitCryptoLearning()
    {
        SetCryptoMode(false);
        
        // 通常のシーケンシャルゲームに戻る
        if (sequentialManager != null)
        {
            sequentialManager.ResetState();
        }
    }
    
    private void SetCryptoMode(bool cryptoMode)
    {
        isInCryptoMode = cryptoMode;
        
        // UI切り替え
        if (cryptoUI != null)
            cryptoUI.SetActive(cryptoMode);
            
        if (originalUI != null)
            originalUI.SetActive(!cryptoMode);
            
        // 3D回答オブジェクト切り替え
        if (answerCube1 != null)
            answerCube1.SetActive(cryptoMode);
            
        if (answerCube2 != null)
            answerCube2.SetActive(cryptoMode);
            
        if (answerCube3 != null)
            answerCube3.SetActive(cryptoMode);
            
        if (answerCube4 != null)
            answerCube4.SetActive(cryptoMode);
    }
    
    // 3D回答オブジェクトのテキスト更新（四択対応）
    public void UpdateAnswerTexts(string answer1, string answer2, string answer3, string answer4)
    {
        if (answerText1 != null)
            answerText1.text = answer1;
            
        if (answerText2 != null)
            answerText2.text = answer2;
            
        if (answerText3 != null)
            answerText3.text = answer3;
            
        if (answerText4 != null)
            answerText4.text = answer4;
    }

    // 3D回答オブジェクトのテキスト更新（二択互換）
    public void UpdateAnswerTexts(string answer1, string answer2)
    {
        UpdateAnswerTexts(answer1, answer2, "", "");
    }
    
    // 既存のSequentialObjectsManagerから呼び出される
    public void OnSequentialObjectTouched(int stageNumber)
    {
        if (!isInCryptoMode)
        {
            // 通常のシーケンシャルゲーム処理
            if (sequentialManager != null)
            {
                sequentialManager.InteractWithObject(stageNumber);
            }
        }
        else
        {
            // 暗号学習モードでの特別処理
            HandleCryptoStageInteraction(stageNumber);
        }
    }
    
    private void HandleCryptoStageInteraction(int stageNumber)
    {
        // 各ステージで異なる暗号方式を学習
        switch (stageNumber)
        {
            case 1:
                // 共通鍵暗号学習開始
                StartSpecificCryptoType(CryptoGameManager.CryptoType.SymmetricKey);
                break;
            case 2:
                // 公開鍵暗号学習
                StartSpecificCryptoType(CryptoGameManager.CryptoType.PublicKey);
                break;
            case 3:
                // ハイブリッド暗号学習
                StartSpecificCryptoType(CryptoGameManager.CryptoType.Hybrid);
                break;
            default:
                // その他のステージでは通常処理
                if (sequentialManager != null)
                {
                    sequentialManager.InteractWithObject(stageNumber);
                }
                break;
        }
    }
    
    private void StartSpecificCryptoType(CryptoGameManager.CryptoType cryptoType)
    {
        // 特定の暗号方式の学習開始
        if (cryptoGameManager != null)
        {
            // 現在のCryptoGameManagerを拡張して特定方式のみ学習できるようにする
            Debug.Log($"暗号学習開始: {cryptoType}");
        }
    }
}