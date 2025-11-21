# 📐 UI配置座標 詳細仕様書

## 画面解像度別対応表

### FullHD (1920x1080) - 基準解像度
```
┌─────────────────────────────────────────────┐ ← Y: 0 (Top)
│  Timer: (0, -40)     CurrentScore: (-150, -50)│
│         [03:00]           [スコア: 120点]       │
├─────────────────────────────────────────────┤ ← Y: -100
│                                             │
│                 ゲームエリア                  │
│                                             │
│                                             │ ← Y: 0 (Center)
│              [FinalResultPanel]             │
│                (0, 0, 600x450)              │
│                                             │
│                                             │
├─────────────────────────────────────────────┤ ← Y: 100  
│Progress: (20, 120)                          │
│[██████░░] 75%                               │
└─────────────────────────────────────────────┘ ← Y: 540 (Bottom)
X:-960                    X:0                    X:960
(Left)                 (Center)                (Right)
```

## 具体的座標仕様

### 1. CurrentScoreText (現在スコア)
```
=== 画面位置 ===
スクリーン座標: 画面右上から150px左、50px下
相対位置: Top-Right Anchor

=== RectTransform詳細 ===
anchorMin: (1, 1)           // 右上角
anchorMax: (1, 1)           // 右上角  
anchoredPosition: (-150, -50) // アンカーからのオフセット
sizeDelta: (250, 60)        // 幅x高さ
pivot: (1, 1)               // 右上基準

=== 実装コード例 ===
RectTransform rect = currentScoreText.GetComponent<RectTransform>();
rect.anchorMin = new Vector2(1, 1);
rect.anchorMax = new Vector2(1, 1);
rect.anchoredPosition = new Vector2(-150, -50);
rect.sizeDelta = new Vector2(250, 60);
```

### 2. TimerText (タイマー表示) - オプション
```
=== 画面位置 ===
スクリーン座標: 画面上中央から40px下
相対位置: Top-Center Anchor

=== RectTransform詳細 ===
anchorMin: (0.5, 1)         // 上中央
anchorMax: (0.5, 1)         // 上中央
anchoredPosition: (0, -40)  // 中央から40px下
sizeDelta: (150, 50)        // 幅x高さ
pivot: (0.5, 1)             // 上中央基準
```

### 3. FinalResultPanel (最終結果パネル)
```
=== 画面位置 ===
スクリーン座標: 画面中央
相対位置: Center Anchor

=== RectTransform詳細 ===
anchorMin: (0.5, 0.5)       // 中央
anchorMax: (0.5, 0.5)       // 中央
anchoredPosition: (0, 0)    // 中央
sizeDelta: (600, 450)       // 幅x高さ
pivot: (0.5, 0.5)           // 中央基準

=== マージン設定 ===
上下左右に最低60px以上の余白確保
最小画面サイズ: 720x480 以上対応
```

### 4. FinalScoreText (最終スコア詳細)
```
=== 画面位置 ===
スクリーン座標: FinalResultPanel内に完全収まる
相対位置: Parent Stretch

=== RectTransform詳細 ===
anchorMin: (0, 0)           // 親の左下
anchorMax: (1, 1)           // 親の右上
offsetMin: (30, 30)         // 左下からのオフセット
offsetMax: (-30, -30)       // 右上からのオフセット
pivot: (0.5, 0.5)           // 中央基準
```

## レスポンシブ対応

### 画面比率別調整
```csharp
public class ResponsiveUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform currentScoreText;
    public RectTransform finalResultPanel;
    
    void Start()
    {
        AdjustForScreenRatio();
    }
    
    void AdjustForScreenRatio()
    {
        float aspectRatio = (float)Screen.width / Screen.height;
        
        // 16:9 (1.78) より横長の場合
        if (aspectRatio > 1.8f)
        {
            // スコア表示をより右に
            currentScoreText.anchoredPosition = new Vector2(-100, -50);
        }
        // 4:3 (1.33) より縦長の場合  
        else if (aspectRatio < 1.4f)
        {
            // パネルサイズを縦に調整
            finalResultPanel.sizeDelta = new Vector2(500, 400);
        }
    }
}
```

### モバイル対応 (Safe Area)
```csharp
public class SafeAreaUI : MonoBehaviour
{
    public RectTransform safeAreaTransform;
    
    void Start()
    {
        ApplySafeArea();
    }
    
    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        
        safeAreaTransform.anchorMin = anchorMin;
        safeAreaTransform.anchorMax = anchorMax;
    }
}
```

## Z-Index / Sort Order 管理

### レイヤー優先順位
```
最前面 (最後に描画)
├─ 5: Final Result Panel + Text
├─ 4: Current Score Text  
├─ 3: Timer Text
├─ 2: Progress UI
├─ 1: Game UI (回答ボタンなど)
└─ 0: Background UI
最背面 (最初に描画)

Canvas設定:
- Main Game Canvas: Sort Order = 0
- Score UI Canvas: Sort Order = 10 (専用Canvas使用時)
```

## アニメーション配置考慮

### パネル表示アニメーション用の初期位置
```
FinalResultPanel 表示前:
- Scale: (0, 0, 1) → (1, 1, 1)
- Position: (0, -100, 0) → (0, 0, 0) 
- Alpha: 0 → 1

CurrentScoreText スコア変更時:
- Scale: (1, 1, 1) → (1.2, 1.2, 1) → (1, 1, 1)
- Position維持
- Color: 通常色 → ハイライト色 → 通常色
```

## デバッグ用座標確認

### Unity Console での座標出力
```csharp
[ContextMenu("Debug UI Positions")]
public void DebugUIPositions()
{
    Debug.Log($"CurrentScore: {currentScoreText.anchoredPosition}");
    Debug.Log($"FinalPanel: {finalResultPanel.anchoredPosition}");
    Debug.Log($"Screen: {Screen.width}x{Screen.height}");
    Debug.Log($"Canvas Scale: {transform.root.GetComponent<Canvas>().scaleFactor}");
}
```

この詳細仕様に従って実装すれば、どの画面サイズでも適切に表示されるUI配置が実現できます！
