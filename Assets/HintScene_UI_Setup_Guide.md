# ヒントシーンUI動作設定ガイド

## 🎯 目的
ヒントシーンでパネルやボタンクリック時に、適切にシーン切り替えや暗号方式のヒント文表示が動作するようにします。

## 🔧 設定手順

### 1. **HintSceneの基本セットアップ**

#### Step 1: HintSceneを開く
```
1. Assets/Scenes/HintScene.unity を開く
2. 既存のGameObjectがあれば確認
```

#### Step 2: 必須コンポーネントを配置
HintSceneに以下のスクリプトをアタッチされたGameObjectを作成：

```
GameObject "HintSceneInitializer" (推奨)
├── HintSceneInitializer.cs
├── GameHintManager.cs  
├── HintUIGenerator.cs
├── HintManagerAutoSetup.cs
└── HintUITester.cs (デバッグ用)
```

### 2. **自動セットアップの実行**

#### 方法A: HintSceneInitializer使用（推奨）
1. `HintSceneInitializer`コンポーネントのInspectorを開く
2. 「Initialize Hint Scene」ボタンをクリック
3. 自動的にUI要素とイベントが設定される

#### 方法B: 手動セットアップ
1. `HintUIGenerator`の「Generate Hint UI」を実行
2. `HintManagerAutoSetup`の「Auto Setup GameHintManager」を実行
3. `GameHintManager`のSetupButtonsメソッドが呼ばれることを確認

### 3. **動作確認**

#### デバッグモード使用
1. `HintUITester`コンポーネントで「Log Current State」実行
2. コンソールログで以下を確認：
   - UI要素が正しく検出されているか
   - イベントリスナーが設定されているか
   - パネルの表示/非表示が機能するか

#### 手動テスト
1. Play Modeに入る
2. 数字キー 1-3 でカテゴリを選択テスト
3. ESCキーで戻る機能をテスト
4. 画面右上のテストボタンで機能確認

## 🛠️ トラブルシューティング

### 問題1: ボタンが反応しない
**原因**: UI要素の参照が設定されていない
**解決策**:
```csharp
// HintManagerAutoSetup を使用して自動設定
var setup = FindObjectOfType<HintManagerAutoSetup>();
setup.AutoSetupHintManager();
```

### 問題2: パネルが切り替わらない
**原因**: パネルのGameObject参照がnull
**解決策**:
1. `HintSceneInitializer.LogSceneObjects()` でオブジェクト一覧確認
2. 不足しているパネルを手動作成
3. または `EmergencyUIFix()` を実行

### 問題3: ヒント内容が表示されない
**原因**: Text コンポーネントの参照不足
**解決策**:
```csharp
// GameHintManager で自動検出を実行
hintManager.AutoDetectUIElements(); // 内部で自動実行される
```

### 問題4: イベントが重複登録される
**原因**: 複数回SetupButtonsが呼ばれている
**解決策**:
```csharp
// GameHintManager で RemoveAllListeners() が自動実行される
// 手動で解決する場合:
button.onClick.RemoveAllListeners();
button.onClick.AddListener(action);
```

## 🎮 動作フロー

### カテゴリ選択時:
1. ユーザーがカテゴリボタンをクリック
2. `SelectCategory(HintCategory.SymmetricKey)` が呼ばれる
3. `ShowHintSelection()` で画面が切り替わる
4. 該当カテゴリのヒント一覧が表示される

### ヒント表示時:
1. ユーザーがヒント選択ボタンをクリック
2. `ShowHint(index)` が呼ばれる
3. `ShowHintDisplay()` で詳細画面に切り替わる
4. タイトルと内容が Text コンポーネントに設定される

### 戻る処理:
1. ユーザーが戻るボタンをクリック
2. `GoBack()` が現在の状態を判定
3. 適切な前の画面に戻る
4. または `ReturnToPreviousScene()` でメインゲームに戻る

## 🚀 高度な設定

### カスタムヒントの追加:
```csharp
// GameHintManager.InitializeHintDatabase() 内で
hintDatabase[HintCategory.Custom] = new List<HintData>
{
    new HintData("カスタムタイトル", "カスタム内容")
};
```

### UI スタイルのカスタマイズ:
```csharp
// HintUIGenerator.CreateCategoryPanel() 内で色やサイズを調整
image.color = new Color(0.2f, 0.3f, 0.8f, 0.9f); // カスタム色
```

### デバッグ機能の活用:
- `CursorStateDebugger`: カーソル状態をリアルタイム表示
- `HintUITester`: キーボードショートカットでテスト実行
- `HintSceneInitializer`: 緊急時のUI修復機能

## ✅ 完了チェックリスト

- [ ] HintScene.unityを開けている
- [ ] 必要なスクリプトコンポーネントが配置されている  
- [ ] `HintSceneInitializer.InitializeHintScene()` を実行した
- [ ] Play Modeでカテゴリ選択が動作する
- [ ] ヒント詳細表示が動作する
- [ ] 戻る機能が動作する
- [ ] メインゲームへの遷移が動作する
- [ ] エラーログが出ていない

この設定により、ヒントシーンでの完全なUI操作が可能になります！
