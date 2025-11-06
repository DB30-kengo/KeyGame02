# ゲームヒントシーン作成ガイド

## 📋 シーン作成手順

### 1. 新しいシーンの作成
1. Unity エディターで `File → New Scene` を選択
2. シーン名を `HintScene` として保存（Assets/Scenes/フォルダーに）

### 2. 基本UI要素の作成

#### Canvas の設定
1. `GameObject → UI → Canvas` でCanvas作成
2. Canvas Scaler コンポーネントで：
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1920 x 1080`
   - Screen Match Mode: `Match Width Or Height`

#### EventSystem の確認
- 自動で作成されているEventSystemが存在することを確認

### 3. GameHintManager の設定

#### スクリプトアタッチ
1. 空のGameObjectを作成し、名前を `HintManager` に変更
2. `GameHintManager` スクリプトをアタッチ

#### UI自動生成（推奨）
1. 空のGameObjectを作成し、名前を `UIGenerator` に変更
2. `HintUIGenerator` スクリプトをアタッチ
3. Inspectorで `Generate UI` チェックボックスをオンにするか
4. 右クリックメニューから `Generate Hint UI` を選択

### 4. 手動UI作成（自動生成を使わない場合）

#### メインパネル構造
```
Canvas
└── HintMainPanel (Image: 半透明の背景)
    ├── CategoryPanel (カテゴリ選択画面)
    │   ├── Title (Text: "ヒントカテゴリ選択")
    │   ├── CategoryButton1 (Button: "共通鍵暗号")
    │   ├── CategoryButton2 (Button: "公開鍵暗号")
    │   ├── CategoryButton3 (Button: "ハイブリッド暗号")
    │   ├── CategoryButton4 (Button: "ゲーム操作")
    │   └── CategoryButton5 (Button: "一般ヒント")
    │
    ├── HintSelectionPanel (ヒント選択画面・初期非表示)
    │   ├── SelectionTitle (Text: "ヒント選択")
    │   ├── HintButton1 (Button)
    │   ├── HintButton2 (Button)
    │   ├── HintButton3 (Button)
    │   ├── HintButton4 (Button)
    │   ├── HintButton5 (Button)
    │   └── HintButton6 (Button)
    │
    ├── HintDisplayPanel (ヒント表示画面・初期非表示)
    │   ├── HintTitle (Text: タイトル表示)
    │   └── ContentArea (Image: 背景付き)
    │       └── ScrollView
    │           └── Viewport (Mask付き)
    │               └── HintContent (Text: 内容表示)
    │
    ├── BackButton (Button: "← 戻る")
    └── MainMenuButton (Button: "メインメニュー")
```

### 5. GameHintManager の設定

#### Inspector設定
```
[UI References]
- Hint Content Text: HintContent オブジェクト
- Hint Title Text: HintTitle オブジェクト
- Category Buttons: CategoryButton1~5 の配列
- Hint Selection Buttons: HintButton1~6 の配列
- Back Button: BackButton オブジェクト
- Main Menu Button: MainMenuButton オブジェクト

[UI Panels]
- Category Panel: CategoryPanel オブジェクト
- Hint Display Panel: HintDisplayPanel オブジェクト
- Hint Selection Panel: HintSelectionPanel オブジェクト

[Visual Settings]
- Selected Button Color: 黄色 (255, 255, 0, 255)
- Normal Button Color: 白 (255, 255, 255, 255)
```

### 6. メインゲームシーンでの設定

#### CryptoGameManager への追加
1. CryptoGameManager オブジェクトを選択
2. `HintSceneTransition` スクリプトをアタッチ
3. Inspector で：
   - Hint Scene Name: `HintScene`
   - Current Scene Name: 現在のシーン名を入力

#### ヒントボタンの追加（自動 or 手動）
- **自動**: CryptoGameManagerが起動時に自動生成
- **手動**: UIにヒントボタンを作成してCryptoGameManagerの`Hint Button`に設定

### 7. シーン遷移の設定

#### Build Settings
1. `File → Build Settings` を開く
2. 以下のシーンを追加：
   - MainMenu シーン（存在する場合）
   - ゲームメインシーン（現在のゲームシーン）
   - HintScene

### 8. テスト手順

#### 基本動作確認
1. ゲームメインシーンでPlay
2. ヒントボタンをクリック
3. HintSceneに遷移することを確認
4. 各カテゴリボタンの動作確認
5. ヒント表示の確認
6. 戻るボタンの動作確認

#### 詳細テスト項目
- [ ] カテゴリ選択画面の表示
- [ ] 各カテゴリボタンのクリック
- [ ] ヒント選択画面への遷移
- [ ] ヒント内容の表示
- [ ] スクロールの動作
- [ ] 戻るボタンの階層遷移
- [ ] メインメニューへの遷移

## 🎨 UI デザインのカスタマイズ

### 色設定の推奨値
```csharp
// 背景色
メインパネル: Color(0.1f, 0.1f, 0.2f, 0.9f)  // 濃紺

// ボタン色
カテゴリボタン: Color(0.3f, 0.6f, 1f, 0.8f)   // 青系
選択ボタン: Color(0.4f, 0.8f, 0.4f, 0.8f)      // 緑系
ヒントボタン: Color(1f, 0.8f, 0.2f, 0.9f)      // 黄系

// テキスト色
タイトル: Color.white
ヒントタイトル: Color.yellow
内容: Color.white
```

### フォント設定
- タイトル: 28-36px
- ボタンテキスト: 16-20px
- 内容テキスト: 18-22px

## 🔧 トラブルシューティング

### よくある問題
1. **ボタンが反応しない**
   → EventSystemが存在するか確認

2. **シーン遷移ができない**
   → Build Settingsにシーンが追加されているか確認

3. **ヒント内容が表示されない**
   → GameHintManagerのUI参照設定を確認

4. **戻るボタンが機能しない**
   → PlayerPrefsで元のシーン名が保存されているか確認

### デバッグ方法
- Console ウィンドウでエラーメッセージを確認
- GameHintManager の各メソッドにDebug.Logを追加
- UI要素のアクティブ状態を確認

## 📝 拡張可能な機能

### 追加可能な機能
1. **アニメーション効果**
   - パネル切り替え時のフェード
   - ボタンホバー効果

2. **音響効果**
   - ボタンクリック音
   - ページ遷移音

3. **検索機能**
   - ヒント内容の検索
   - キーワードハイライト

4. **お気に入り機能**
   - よく参照するヒントの保存
   - 履歴機能

5. **進捗連動**
   - ゲーム進捗に応じたヒント解放
   - 達成度表示

この設定により、直感的で使いやすいヒントシステムが完成します！
