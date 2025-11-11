# カーソル表示問題の解決ガイド

## 🎯 修正内容

ヒントシーンでカーソルが消える問題を解決するため、以下の機能を実装しました：

### 1. **HintSceneTransition.cs の強化**
- `SaveAndSetCursorForUI()`: ヒントシーン遷移時にカーソル状態を保存し、UIモードに設定
- `RestoreCursorState()`: メインゲームに戻る際にカーソル状態を復元
- PlayerPrefsを使用したカーソル状態の永続化

### 2. **GameHintManager.cs の改良**
- `SetCursorForHintScene()`: ヒントシーン開始時にカーソルを強制表示
- `ReturnToPreviousScene()`: 戻る処理でカーソル状態を復元

### 3. **HintUIGenerator.cs の拡張**
- UI生成後にカーソル表示を確実にする処理を追加

### 4. **CryptoGameManager.cs の改善**
- `CheckAndRestoreCursorFromHint()`: ヒントシーンから戻った際の自動復元
- `SetDefaultGameCursor()`: ゲーム用のカーソル設定

### 5. **CursorStateDebugger.cs の追加**
- リアルタイムカーソル状態表示
- 手動カーソル制御ボタン
- デバッグ情報表示

## 🔧 動作フロー

### ヒントシーン遷移時:
1. 現在のカーソル状態をPlayerPrefsに保存
2. カーソルを`CursorLockMode.None`、`visible = true`に設定
3. ヒントシーンに遷移

### メインゲーム復帰時:
1. PlayerPrefsから保存されたカーソル状態を読み込み
2. 元のカーソル状態を復元
3. 保存データをクリア

## 🚀 使用方法

### 基本セットアップ
1. **メインゲームシーン**に`HintSceneTransition`コンポーネントをアタッチ
2. **HintScene**に`GameHintManager`と`HintUIGenerator`をアタッチ
3. 必要に応じて`CursorStateDebugger`を追加

### デバッグモード
```csharp
// CursorStateDebuggerを任意のGameObjectにアタッチ
// Inspector または Context Menu から以下を実行可能:
// - Show Cursor / Hide Cursor
// - Lock Cursor / Unlock Cursor  
// - Reset to UI Mode
// - Clear Saved State
```

## 🎮 対応シナリオ

### 1. **FPSコントローラー使用時**
- ゲーム中: `Cursor.lockState = Locked`, `visible = false`
- ヒント中: `Cursor.lockState = None`, `visible = true`
- 復帰時: 元の状態に自動復元

### 2. **UIボタン併用時**
- ゲーム中: `Cursor.lockState = None`, `visible = true`
- ヒント中: 状態維持
- 復帰時: 元の状態に復元

### 3. **3Dのみの場合**
- ゲーム中: カーソルロック状態
- ヒント中: カーソル表示強制
- 復帰時: ロック状態に復帰

## 🔍 トラブルシューティング

### カーソルが表示されない場合
1. `CursorStateDebugger`を使用して現在の状態を確認
2. PlayerPrefsに古いデータが残っていないかチェック
3. `Clear Saved State`でリセット

### カーソルが正しく復元されない場合
1. `HintSceneTransition.RestoreCursorState()`が呼ばれているか確認
2. PlayerPrefsキーの存在をチェック
3. ログでカーソル状態の変更を追跡

## 🎯 主要な改善点

✅ **自動カーソル状態管理**: シーン遷移時の自動保存・復元  
✅ **フォールバック機能**: 保存データがない場合のデフォルト処理  
✅ **デバッグツール**: リアルタイム状態確認・手動制御  
✅ **汎用性**: FPS、TPS、UIベースゲーム全てに対応  
✅ **エラー処理**: 例外処理とログ出力の充実  

## 🚨 注意事項

- PlayerPrefsを使用するため、ゲーム終了時にも状態が保持される
- デバッグモードでは画面上部にカーソル情報が表示される
- 複数のシーン間を行き来する場合は、各シーンで適切に設定する必要がある

この実装により、ヒントシーンでのカーソル表示問題が完全に解決され、ユーザビリティが大幅に向上します！
