# ヒントシステム コンポーネント除去ガイド

## 手順1: メインゲームマネージャーの設定

### GameManagerまたはCryptoGameManagerを見つける
1. Hierarchyパネルで以下の名前を探してください：
   - `GameManager`
   - `CryptoGameManager`
   - `Manager`
   - または類似の管理系オブジェクト

### 2. Inspectorで無効化するコンポーネント
選択したGameObjectのInspectorで、以下のコンポーネントの**チェックボックスを外して**無効化：

```
✅ Crypto Game Manager (Script)         ← これは残す
□ Real Hint System (Script)             ← チェックを外す
□ Hint System Launcher (Script)         ← チェックを外す  
□ Hint Scene Transition (Script)        ← チェックを外す
□ Hint Detail Display Fixer (Script)    ← チェックを外す
□ Hint System Visual Guide (Script)     ← チェックを外す
```

## 手順2: 独立したヒントシステムオブジェクトの無効化

### Canvas配下のヒント関連UI
1. `Canvas`を展開
2. 以下のようなオブジェクトを探して非アクティブ化：
   - `HintButton`
   - `HintPanel`
   - `HintUI`
   - `HintSystem`

### ヒント専用GameObjectを探す
Hierarchyで以下の名前を探して非アクティブ化：
- `RealHintSystem`
- `HintManager`
- `HintLauncher`

**非アクティブ化方法：**
```
□ オブジェクト名 ← 左のチェックボックスを外す
```

## 手順3: 新しいシンプルヒントシステムの有効化

### NewHintManagerを追加
1. GameManagerを選択
2. `Add Component`をクリック
3. `New Hint Manager`を検索して追加
4. 設定を確認：
   ```
   Display Type: Notification (推奨)
   Hint Key: H
   Auto Setup: ✅
   ```

## 手順4: 動作確認

### テスト方法
1. ゲームを実行
2. `H`キーを押してヒントが表示されるか確認
3. `F9`キーで表示スタイルが切り替わるか確認

### 問題が発生した場合
- Console（Window → General → Console）でエラーメッセージを確認
- 古いヒントシステムが完全に無効化されているか再確認

## 設定例：正しい状態

### GameManagerの設定例
```
GameManager (GameObject)
├─ ✅ Crypto Game Manager (Script)
├─ ✅ New Hint Manager (Script)      ← 新しいシステム
├─ □ Real Hint System (Script)       ← 無効化済み
├─ □ Hint System Launcher (Script)   ← 無効化済み
└─ □ Hint Scene Transition (Script)  ← 無効化済み
```

### Canvas設定例
```
Canvas (GameObject)
├─ ✅ Main UI Panel
├─ □ HintButton                      ← 非アクティブ
└─ □ HintPanel                       ← 非アクティブ
```

## 注意事項

1. **スクリプトファイルは削除しない**
   - .csファイルは残しておく
   - 必要に応じて後で再有効化可能

2. **段階的に無効化**
   - 一度に全部ではなく、一つずつ無効化してテスト

3. **バックアップ推奨**
   - シーンファイルのバックアップを作成
   - プロジェクト全体のバックアップも推奨

## トラブルシューティング

### ヒントが表示されない場合
1. NewHintManagerが正しく追加されているか確認
2. Auto Setupがチェックされているか確認
3. 古いシステムが完全に無効化されているか確認

### エラーが発生する場合
1. Consoleでエラー内容を確認
2. 無効化し忘れたコンポーネントがないか確認
3. 段階的に無効化して原因を特定
