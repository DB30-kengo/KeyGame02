# 📊 理解度スライダーとラベルの更新タイミング詳細解説

## 🎯 更新タイミング一覧

### 1. ゲーム開始時
```
タイミング: StartNewGameSet() 実行時
呼び出し順序:
1. progressTracker.ResetProgressForNewGame() // 進捗リセット
2. UpdateProgressDisplay() // UI更新
3. StartCurrentQuestion() // 最初の問題表示
   └── UpdateProgressDisplay() // 再度UI更新

表示内容:
- 共通鍵: 0%
- 公開鍵: 0%  
- ハイブリッド: 0%
```

### 2. 正解時（即座に反映）⭐ 新機能
```
タイミング: OnCorrectAnswer() 実行時
呼び出し順序:
1. progressTracker.UpdateProgress(currentType, 20f) // 進捗+20%
2. UpdateProgressDisplay() // 即座にUI更新
3. プレイヤー位置リセット
4. 次の問題へ遷移

表示例（共通鍵1問目正解時）:
- 共通鍵: 0% → 20% ⬆️
- 公開鍵: 0%
- ハイブリッド: 0%
```

### 3. 問題遷移時
```
タイミング: StartCurrentQuestion() 実行時
呼び出し順序:
1. 暗号方式に応じた鍵表示制御
2. DisplayQuestion() // 問題表示
3. UpdateProgressText() // 進捗テキスト更新
4. UpdateProgressDisplay() // スライダー・ラベル更新

表示内容:
現在の理解度をリアルタイム反映
```

### 4. 暗号方式切り替え時
```
タイミング: TransitionToNextCryptoType() 完了後
呼び出し順序:
1. StartCurrentQuestion()
   └── UpdateProgressDisplay()

表示例（共通鍵→公開鍵切り替え時）:
- 共通鍵: 100% ✅
- 公開鍵: 0% ← 新しい暗号方式開始
- ハイブリッド: 0%
```

### 5. ゲーム終了時
```
タイミング: EndGameSet() 実行時
呼び出し順序:
1. ShowResults() // 最終結果表示
2. 5秒後自動再開で StartNewGameSet()
   └── UpdateProgressDisplay() // リセット後の表示

最終表示例:
- 共通鍵: 100%
- 公開鍵: 100%
- ハイブリッド: 100%
```

### 6. 手動リセット時（デバッグ用）
```
タイミング: ManualResetProgress() 実行時
呼び出し順序:
1. progressTracker.ResetProgressForNewGame()
2. UpdateProgressDisplay()

表示結果: 全て0%にリセット
```

## 🔄 進捗計算ロジック

### 基本計算式
```csharp
// 各暗号方式は5問構成
1問正解 = +20%の進捗
5問すべて正解 = 100%

スライダー値 = progress / 100f  // 0.0～1.0の範囲
ラベルテキスト = "{暗号方式名} {progress:F0}%"
```

### 進捗値の例
```
問題1正解後: 20%  (1/5)
問題2正解後: 40%  (2/5)  
問題3正解後: 60%  (3/5)
問題4正解後: 80%  (4/5)
問題5正解後: 100% (5/5) ← 完了
```

## 📱 UI更新の実装詳細

### UpdateProgressDisplay()メソッド
```csharp
private void UpdateProgressDisplay()
{
    // ProgressTrackerから最新データ取得
    float[] progressValues = progressTracker.GetAllProgress();
    string[] cryptoNames = { "共通鍵", "公開鍵", "ハイブリッド" };
    
    // スライダーとラベルを同時更新
    for (int i = 0; i < progressSliders.Length && i < progressValues.Length; i++)
    {
        if (progressSliders[i] != null)
        {
            progressSliders[i].value = progressValues[i] / 100f; // スライダー更新
        }
        
        if (i < progressLabels.Length && progressLabels[i] != null)
        {
            progressLabels[i].text = $"{cryptoNames[i]} {progressValues[i]:F0}%"; // ラベル更新
        }
    }
}
```

### 必要なUI要素（Inspector設定）
```
CryptoGameManager:
├─ Progress Sliders: Slider[] (3つの配列)
│  ├─ [0] SymmetricKeySlider (共通鍵用)
│  ├─ [1] PublicKeySlider (公開鍵用)  
│  └─ [2] HybridSlider (ハイブリッド用)
└─ Progress Labels: Text[] (3つの配列)
   ├─ [0] SymmetricKeyLabel
   ├─ [1] PublicKeyLabel
   └─ [2] HybridLabel
```

## 🎨 視覚的な更新効果

### スライダーの動き
```
正解前: ████████░░ 80%
正解後: ██████████ 100% ← スムーズに増加
```

### ラベルの変化
```
正解前: "共通鍋 80%"
正解後: "共通鍵 100%" ← 数値が即座に更新
```

### 色の変化（推奨カスタマイズ）
```csharp
// 進捗に応じた色変更例
if (progressValue < 30f)
    slider.fillRect.GetComponent<Image>().color = Color.red;    // 赤: 低進捗
else if (progressValue < 70f)  
    slider.fillRect.GetComponent<Image>().color = Color.yellow; // 黄: 中進捗
else
    slider.fillRect.GetComponent<Image>().color = Color.green;  // 緑: 高進捗
```

## 🚀 パフォーマンス最適化

### 更新頻度
```
❌ 毎フレーム更新 → 不要な処理
✅ 必要時のみ更新 → 効率的

更新タイミング:
- 正解時（進捗変化）
- 問題遷移時（表示確認）
- ゲーム開始・リセット時
```

### エラー処理
```csharp
// null チェック例
if (progressSliders != null && progressLabels != null)
{
    // 安全な更新処理
}
else
{
    Debug.LogWarning("Progress UI elements not assigned");
}
```

## 🔧 デバッグ用機能

### コンソール出力
```csharp
Debug.Log($"進捗更新: {cryptoType} → {newProgress:F0}%");
```

### Inspector確認
```
ProgressTracker (Script):
├─ Max Progress: 100
├─ Progress Increment: 5
└─ Current Values:
   ├─ Symmetric Key: 60%
   ├─ Public Key: 20%  
   └─ Hybrid: 0%
```

### 手動テスト
```
Inspector右クリック → "Manual Reset Progress"
→ 全て0%にリセットして動作確認
```

## 📋 まとめ

理解度スライダーとラベルの更新タイミング:

1. **ゲーム開始時**: 0%でリセット
2. **正解時**: +20%で即座に更新 ⭐ 
3. **問題遷移時**: 現在値を確認表示
4. **暗号方式切り替え時**: 次の暗号方式の進捗表示
5. **ゲーム終了時**: 最終結果表示

**正解した瞬間にスライダーが動いてラベルが更新される**ようになりました！
