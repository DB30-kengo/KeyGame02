using UnityEngine;

/// <summary>
/// 敵との接触を検出するスクリプト
/// </summary>
public class EnemyContactHandler : MonoBehaviour
{
    [Tooltip("接触を検出するタグ（通常は'Player'）")]
    public string targetTag = "Player";
    
    [Tooltip("トリガーコライダーを使用するか")]
    public bool useTriggerCollider = true;
    
    [Tooltip("敵に触れた時に表示するメッセージ")]
    [TextArea(2, 3)]
    public string enemyContactMessage = "敵に捕まりました！";
    
    // 内部変数
    private bool hasTriggered = false;
    
    // シーン再読み込み時などに状態をリセット
    private void OnEnable()
    {
        hasTriggered = false;
    }
    
    // 物理的な接触（トリガーでない場合）
    private void OnCollisionEnter(Collision collision)
    {
        if (useTriggerCollider) return; // トリガーモードの場合はスキップ
        
        if (collision.gameObject.CompareTag(targetTag))
        {
            HandlePlayerContact();
        }
    }
    
    // トリガー接触
    private void OnTriggerEnter(Collider other)
    {
        if (!useTriggerCollider) return; // トリガーモードでない場合はスキップ
        
        if (other.CompareTag(targetTag))
        {
            HandlePlayerContact();
        }
    }
    
    // プレイヤー接触時の処理
    private void HandlePlayerContact()
    {
        // 既に発動済みなら何もしない（二重処理防止）
        if (hasTriggered) return;
        
        hasTriggered = true;
        
        Debug.Log("敵がプレイヤーと接触しました");
        
        // マネージャーが存在するか確認
        if (SequentialObjectsManager.Instance != null)
        {
            // カスタムメッセージを設定（オプション）
            if (!string.IsNullOrEmpty(enemyContactMessage))
            {
                SequentialObjectsManager.Instance.gameOverMessage = enemyContactMessage;
            }
            
            // ゲームオーバー処理を実行
            SequentialObjectsManager.Instance.TriggerGameOver();
        }
    }
}