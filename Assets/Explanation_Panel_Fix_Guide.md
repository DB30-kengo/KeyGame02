# 🔧 不正解時の解説パネル修正ガイド

**問題**: 不正解時に解説パネルが表示されない  
**原因**: UI要素（explanationPanel, explanationText）がInspectorで未設定  
**解決**: 解説用UIパネルの作成と設定  

## 📋 必要なUI要素の作成

### 1. ExplanationPanel（解説パネル）作成

#### Unity エディタでの作成手順
```
1. Hierarchy → Canvas → 右クリック → UI → Panel
2. 名前を "ExplanationPanel" に変更
3. Inspector → RectTransform設定:
   ┌─────────────────────────────────────┐
   │ Anchor Presets: Center              │
   │ Position X: 0                       │
   │ Position Y: 0                       │
   │ Width: 700                          │
   │ Height: 300                         │
   └─────────────────────────────────────┘

4. Inspector → Image設定:
   ┌─────────────────────────────────────┐
   │ Color: rgba(40, 40, 60, 220)        │ ← 濃いブルー、半透明
   │ Image Type: Sliced                  │
   └─────────────────────────────────────┘

5. 初期状態で非表示に設定:
   ┌─────────────────────────────────────┐
   │ GameObject (上部) チェックを外す     │ ← SetActive(false)
   └─────────────────────────────────────┘
```

### 2. ExplanationText（解説テキスト）作成

#### Unity エディタでの作成手順
```
1. ExplanationPanel → 右クリック → UI → Text
2. 名前を "ExplanationText" に変更
3. Inspector → RectTransform設定:
   ┌─────────────────────────────────────┐
   │ Anchor Presets: Stretch-All          │ ← 全域に拡張
   │ Left: 30                            │
   │ Top: 30                             │
   │ Right: 30                           │
   │ Bottom: 30                          │
   └─────────────────────────────────────┘

4. Inspector → Text設定:
   ┌─────────────────────────────────────┐
   │ Text: "解説がここに表示されます"      │
   │ Font: Arial                         │
   │ Font Size: 24                       │
   │ Line Spacing: 1.2                   │
   │ Color: #FFFFFF (白)                 │
   │ Alignment: Middle Center             │
   │ Horizontal Wrap: チェックオン        │
   │ Vertical Overflow: Overflow         │
   └─────────────────────────────────────┘
```

## 🔗 CryptoGameManager への接続

### Inspector設定手順
```
1. Hierarchy → CryptoGameManager オブジェクトを選択
2. Inspector → CryptoGameManager (Script) 
3. UI References セクション:
   ┌─────────────────────────────────────┐
   │ Explanation Text: ExplanationText をドラッグ  │
   │ Explanation Panel: ExplanationPanel をドラッグ │
   └─────────────────────────────────────┘
```

## 🎨 視覚的レイアウト例

### 画面配置イメージ
```
┌─────────────────────────────────────────────┐ ← ゲーム画面
│                                             │
│             ゲームエリア                     │
│                                             │
│        ┌─────────────────────────┐          │ ← 不正解時に表示
│        │   📋 解説パネル          │          │
│        │                         │          │
│        │ ❌ 公開鍵は公開鍵暗号で │          │
│        │    使用されます。        │          │
│        │                         │          │
│        │ ✅ 正解は「共通鍵」です。│          │
│        │    共通鍵暗号では送信者  │          │
│        │    と受信者が同じ鍵を    │          │
│        │    共有します。          │          │
│        └─────────────────────────┘          │
│                                             │
└─────────────────────────────────────────────┘
```

## 💻 詳細スタイリング（オプション）

### ExplanationPanel の装飾強化
```
1. 影効果追加:
   Component → UI → Effects → Shadow
   ├─ Effect Color: #000000
   ├─ Effect Distance: (5, -5)
   └─ Use Graphic Alpha: チェックオン

2. 縁取り効果:
   Component → UI → Effects → Outline
   ├─ Effect Color: #FFFFFF
   └─ Effect Distance: (2, 2)
```

### ExplanationText の装飾強化
```
1. 影効果:
   Component → UI → Effects → Shadow
   ├─ Effect Color: #000000
   └─ Effect Distance: (2, -2)

2. 色分け対応:
   正解記号: #00FF88 (緑)
   不正解記号: #FF4444 (赤)
   説明文: #FFFFFF (白)
```

## 🐛 トラブルシューティング

### よくある問題と解決法

#### 1. パネルが表示されない
```
原因: 初期状態でアクティブ化されている
解決: ExplanationPanel の GameObject チェックを外す

確認方法:
1. ExplanationPanel を選択
2. Inspector 上部のチェックボックスが外れているか確認
3. ゲーム実行中に一時的にチェックしてパネル表示確認
```

#### 2. テキストが見えない
```
原因: Canvas の Order in Layer 設定
解決: Canvas の Sort Order を 10以上に設定

確認方法:
1. Canvas を選択  
2. Inspector → Canvas → Sort Order = 10
3. Explanation Panel が前面に表示されることを確認
```

#### 3. 解説テキストが切れる
```
原因: パネルサイズまたはフォントサイズが不適切
解決: パネルサイズ拡大またはフォント縮小

調整例:
- パネル Width: 700 → 800
- フォントサイズ: 24 → 20
- Line Spacing: 1.2 → 1.1
```

#### 4. UI要素の接続エラー
```
確認項目:
1. CryptoGameManager Inspector:
   - Explanation Text: "Missing (Text)" でない
   - Explanation Panel: "Missing (GameObject)" でない

2. Hierarchy 確認:
   - ExplanationPanel が存在する
   - ExplanationText が ExplanationPanel の子要素

3. コンソール確認:
   - "[解説パネル状態]" ログでUI要素の有無を確認
```

## 🧪 動作テスト

### テスト手順
```
1. ゲーム開始
2. 意図的に間違った回答を選択  
3. 以下を確認:
   ✅ "❌ 不正解" テキストが表示
   ✅ 解説パネルが3秒間表示
   ✅ 適切な解説テキストが表示
   ✅ パネルが自動で非表示
   ✅ 同じ問題が再出題
```

### デバッグログ確認
```
Console で以下のログを確認:
[解説表示開始] 解説内容: ❌ 公開鍵は公開鍵暗号で使用されます。
[解説パネル状態] explanationPanel: 存在, explanationText: 存在  
[解説表示] パネルアクティブ化完了 - テキスト設定: '❌ 公開鍵は...'
[解説表示] パネル非表示化完了
```

## ⚡ 即座解決手順（1分）

**最速で解決したい場合:**

1. **Canvas 右クリック → UI → Panel** → 名前「ExplanationPanel」
2. **ExplanationPanel 右クリック → UI → Text** → 名前「ExplanationText」  
3. **CryptoGameManager 選択** → Inspector で **UI References に両方をドラッグ**
4. **ExplanationPanel の GameObject チェックを外す**
5. **ゲーム実行** → 不正解を選択 → **解説表示確認** ✅

この手順で解説パネルが正常に表示されるようになります！
