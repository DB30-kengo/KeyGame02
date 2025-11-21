# 🔢 問題番号表示修正 - デバッグ強化レポート

## ❌ 報告された問題

**症状**: 各問題が出題される度に「1/5 ○○暗号方式」と表示される
**期待される動作**: 同じ暗号方式内で「1/5→2/5→3/5→4/5→5/5」と進行

## 🔍 問題原因の分析

### 可能性1: `currentStepIndex`更新タイミングの問題
- 正解後に`currentStepIndex++`が実行される
- その後`TransitionToNextQuestion()`→`StartCurrentQuestion()`→`DisplayQuestion()`→`UpdateQuestionInfo()`の順で実行
- 理論上は正しく動作するはず

### 可能性2: UI表示の更新タイミングの問題  
- 問題情報表示が更新されないまま残っている
- 動的作成されたUI要素の表示問題
- Canvas描画順序の問題

### 可能性3: ゲーム状態管理の問題
- `currentStepIndex`が意図しない場所でリセットされている
- 複数のコルーチンが同時実行されて競合状態が発生している

## ✅ 実装した修正・デバッグ機能

### 1. 詳細デバッグログの追加
```csharp
UpdateQuestionInfo() に追加:
- currentQuestionIndex の表示
- currentStepIndex の表示  
- currentType の表示
- cryptoName の表示
- questionNumber の計算結果表示
- totalQuestions の表示
```

### 2. 問題番号進行テスト機能
```csharp
[ContextMenu("Test Question Number Progress")]
public void TestQuestionNumberProgress()
- 手動で currentStepIndex を 0→1→2→3→4 に変更
- 各ステップでの問題番号表示をテスト
- UI要素の動作確認
```

## 🧪 デバッグ方法

### Unity Inspector での実行手順
1. **CryptoGameManager を選択**
2. **右クリック** → **"Test Question Number Progress"**
3. **Console でログ確認**:
```
[問題情報計算詳細]
  currentQuestionIndex: 0
  currentStepIndex: 0
  currentType: SymmetricKey
  cryptoName: 共通鍵暗号
  questionNumber: 1
  totalQuestions: 5
[問題情報更新] 1/5 共通鍵暗号 (currentStepIndex: 0)
```

### 実際のゲーム実行での確認
1. **ゲーム開始**
2. **最初の問題**: 「1/5 共通鍵暗号」表示確認
3. **正解を選択**
4. **次の問題**: 「2/5 共通鍵暗号」になっているか確認
5. **Console ログで詳細確認**

## 🔧 期待される出力例

### 正常動作時のログ
```
[問題情報更新] 1/5 共通鍵暗号 (currentStepIndex: 0)
[問題情報更新] 2/5 共通鍵暗号 (currentStepIndex: 1)
[問題情報更新] 3/5 共通鍵暗号 (currentStepIndex: 2)
[問題情報更新] 4/5 共通鍵暗号 (currentStepIndex: 3)
[問題情報更新] 5/5 共通鍵暗号 (currentStepIndex: 4)
[問題情報更新] 1/5 公開鍵暗号 (currentStepIndex: 0)  // 次の暗号方式
```

### 異常動作時のログ（問題がある場合）
```
[問題情報更新] 1/5 共通鍵暗号 (currentStepIndex: 0)
[問題情報更新] 1/5 共通鍵暗号 (currentStepIndex: 0)  // 進んでいない
[問題情報更新] 1/5 共通鍵暗号 (currentStepIndex: 0)  // 進んでいない
```

## 🎯 次のステップ

### 1. デバッグ実行
1. **Unity エディタでゲーム実行**
2. **Console ログで詳細確認**
3. **実際の問題番号表示を目視確認**
4. **Inspector右クリックでテスト機能実行**

### 2. 問題特定
- **currentStepIndex の値が正しく更新されているか**
- **UI表示が正しく更新されているか**  
- **複数の問題情報更新が競合していないか**

### 3. 修正対応
問題が特定できれば、以下のような修正を実装:
- UI更新タイミングの調整
- コルーチンの実行順序調整
- 状態管理の強化

## 🚀 実装した機能

### 詳細ログ出力
```csharp
Debug.Log($"[問題情報計算詳細]");
Debug.Log($"  currentQuestionIndex: {currentQuestionIndex}");
Debug.Log($"  currentStepIndex: {currentStepIndex}");
Debug.Log($"  currentType: {currentType}");
Debug.Log($"  cryptoName: {cryptoName}");
Debug.Log($"  questionNumber: {questionNumber}");
Debug.Log($"  totalQuestions: {totalQuestions}");
```

### テスト機能
```csharp
TestQuestionNumberProgress() - 手動進行テスト
TestQuestionNumberSequence() - 自動化されたテストシーケンス
```

## 📊 問題解決手順

1. **✅ デバッグログ強化** - 完了
2. **✅ テスト機能追加** - 完了
3. **🔄 実際のテスト実行** - 次のステップ
4. **❓ 問題特定** - ログ分析後に実施
5. **❓ 具体的修正** - 問題特定後に実施

---

## 🎉 現在の状況

問題番号表示の詳細デバッグ機能を追加しました。Unity エディタでゲームを実行し、Console ログで実際の動作を確認してください。

**重要**: 問題が「1/5→1/5→1/5...」と表示される場合は、`currentStepIndex`が更新されていない可能性があります。Console ログで詳細な状況を確認できるようになりました。
