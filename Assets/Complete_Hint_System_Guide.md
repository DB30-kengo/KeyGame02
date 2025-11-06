# 🎮 暗号学習ゲーム ヒントシステム 完全セットアップガイド

## 📋 現在の実装状況

✅ **完了した機能**
- プレイヤーリスポーン機能（高さ調整・地面検出対応）
- GameHintManager（5カテゴリ×複数ヒント対応）
- HintUIGenerator（完全自動UI生成）
- HintSceneTransition（堅牢なシーン遷移）
- CryptoGameManager統合（動的ヒントボタン作成）
- シーン間でのデータ受け渡し（PlayerPrefs使用）

🔧 **今回追加した改善**
- エラーハンドリング強化
- デバッグ情報追加
- 自動セットアップスクリプト
- ビルド設定検証ユーティリティ

## 🚀 セットアップ手順

### 1. メインゲームシーンの設定

1. **CryptoGameManagerオブジェクトを選択**
2. **HintSceneTransitionコンポーネントを追加**
   - Inspector → Add Component → HintSceneTransition
3. **設定値を確認**
   - Hint Scene Name: `HintScene`
   - Show Debug Info: チェック推奨

### 2. HintSceneの作成/設定

#### Option A: 自動セットアップ（推奨）
1. **HintSceneを開く**
2. **空のGameObjectを作成** → 名前を "SceneSetup" に変更
3. **HintSceneSetupスクリプトをアタッチ**
4. **Inspector で "Auto Setup" をチェック**
5. **Play mode でテスト実行** または **右クリック → "Setup Hint Scene"**

#### Option B: 手動セットアップ
1. HintScene_Setup_Guide.md の手順に従って手動作成

### 3. Build Settingsの確認

1. **File → Build Settings** を開く
2. **以下のシーンを追加**:
   - SampleScene（メインゲーム）
   - HintScene
   - MainMenu（存在する場合）

**自動確認方法:**
- GameBuildUtilityをシーンに追加
- 右クリック → "Check Build Settings"

### 4. 動作テスト

#### 基本動作確認
1. **メインゲームシーンでPlay**
2. **ヒントボタンをクリック**
3. **HintSceneへの遷移確認**
4. **各カテゴリの動作確認**
5. **戻るボタンの確認**

#### デバッグ情報確認
- Console ウィンドウで遷移ログを確認
- "[HintSceneTransition]" プレフィックスのログをチェック

## 🔧 トラブルシューティング

### よくある問題と解決法

#### 1. ヒントボタンが表示されない
**原因**: CryptoGameManagerにHintSceneTransitionが未アタッチ

**解決法**:
```csharp
// 自動検出が有効なので、以下で解決されるはず
InitializeHintSystem() // 起動時に自動実行
```

#### 2. シーン遷移ができない
**原因**: Build SettingsにHintSceneが未追加

**解決法**:
1. File → Build Settings
2. HintScene.unityをドラッグ&ドロップ
3. またはHintSceneを開いて "Add Open Scenes"

#### 3. ヒント内容が表示されない
**原因**: HintSceneのUI設定不備

**解決法**:
- HintSceneSetupスクリプトで自動セットアップ実行
- または手動でGameHintManagerのUI参照を設定

#### 4. 戻るボタンが機能しない
**原因**: PlayerPrefsの設定問題

**解決法**:
```csharp
// PlayerPrefsをクリアしてリセット
GameBuildUtility → "Clear PlayerPrefs"
```

### デバッグ用ユーティリティ

#### GameBuildUtility の活用
```csharp
// エディタメニューから実行可能
Game Utils → Check Build Settings    // ビルド設定確認
Game Utils → Test Scene Transition   // 遷移テスト
Game Utils → Check Hint System      // ヒントシステム確認
```

#### ログ出力で状況確認
```csharp
// 主要なログメッセージ
[HintSceneTransition] Transitioning to hint scene with category: X
[GameHintManager] Returning to scene: SceneName
[HintSceneSetup] Hint scene setup completed!
```

## 🎨 カスタマイズ

### UI デザインの調整
```csharp
// HintUIGenerator.cs で色やレイアウト調整可能
primaryColor = new Color(0.2f, 0.4f, 0.8f, 0.9f);      // メイン色
secondaryColor = new Color(0.3f, 0.6f, 1f, 0.8f);      // セカンダリ色
backgroundColor = new Color(0.1f, 0.1f, 0.2f, 0.95f);   // 背景色
```

### ヒント内容の追加
```csharp
// GameHintManager.cs の InitializeHintDatabase() で追加
new HintData("新しいヒント", "詳細な説明内容")
```

### カテゴリの追加
```csharp
// HintCategory enum に新しいカテゴリ追加
public enum HintCategory
{
    // 既存のカテゴリ...
    NewCategory = 6  // 新カテゴリ
}
```

## 📱 使用方法

### プレイヤー向け操作
1. **ヒントボタンクリック** → カテゴリ選択画面
2. **カテゴリ選択** → ヒント一覧表示
3. **ヒント選択** → 詳細内容表示
4. **戻るボタン** → 一つ前の画面に戻る
5. **メインメニュー** → ゲーム開始画面に戻る

### 開発者向けAPI
```csharp
// 特定カテゴリで直接ヒント表示
hintTransition.GoToHintScene(0); // 共通鍵暗号ヒント

// カテゴリ選択画面表示
hintTransition.GoToHintScene(-1);

// 動的ヒントボタン作成
hintTransition.CreateHintButton(parentTransform, "ヒント", categoryIndex);
```

## 🚀 今後の拡張可能な機能

### Phase 1: 基本機能強化
- [ ] アニメーション効果（フェード・スライド）
- [ ] 音響効果（ボタンクリック音）
- [ ] ヒント履歴機能

### Phase 2: 高度な機能
- [ ] 検索機能（ヒント内容の検索）
- [ ] お気に入りシステム
- [ ] 進捗連動ヒント解放

### Phase 3: ユーザビリティ強化
- [ ] ヒント表示統計
- [ ] 学習進度追跡
- [ ] カスタムヒント追加

## ✅ チェックリスト

### セットアップ完了確認
- [ ] CryptoGameManagerにHintSceneTransition追加
- [ ] HintSceneに必要なコンポーネント設置
- [ ] Build SettingsにHintScene追加
- [ ] 基本動作テスト完了
- [ ] シーン遷移テスト完了
- [ ] ヒント表示テスト完了

### 動作確認項目
- [ ] ヒントボタンクリックでシーン遷移
- [ ] カテゴリ選択画面の表示
- [ ] ヒント選択と内容表示
- [ ] 戻るボタンの階層的動作
- [ ] 元のシーンへの正常復帰

この設定により、直感的で堅牢なヒントシステムが完成します！🎉
