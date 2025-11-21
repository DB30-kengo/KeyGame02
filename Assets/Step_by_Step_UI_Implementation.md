# Keygame02 プロジェクト専用 UI配置実装ガイド

## 🎯 今すぐ実装できる具体的手順

### Phase 1: 基本Canvas準備（5分）

#### 1-1. メインゲームシーン選択
```
1. Unityエディタを開く
2. Project → Assets → Scenes → Chapter_game.unity をダブルクリック
3. シーンが読み込まれることを確認
```

#### 1-2. 既存UI構造の確認
```
1. Hierarchy ウィンドウで検索: "Canvas"
2. 既存のCanvasがあるか確認
   - ある場合: そのCanvasを使用
   - ない場合: 新しく作成
```

#### 1-3. メインCanvas作成・設定
```
既存Canvasがない場合:
1. Hierarchy 右クリック → UI → Canvas
2. 名前を "MainGameCanvas" に変更

Canvas設定:
1. MainGameCanvas選択
2. Inspector → Canvas コンポーネント:
   ✓ Render Mode: Screen Space - Overlay
   ✓ Pixel Perfect: チェック
3. Canvas Scaler コンポーネント:
   ✓ UI Scale Mode: Scale With Screen Size
   ✓ Reference Resolution: X=1920, Y=1080
   ✓ Screen Match Mode: Match Width Or Height
   ✓ Match: 0.5
```

### Phase 2: CurrentScoreText 実装（3分）

#### 2-1. スコア表示テキスト作成
```
1. MainGameCanvas 右クリック → UI → Text - TextMeshPro
   (初回時は TMP Essentials import を確認)
2. 名前を "CurrentScoreText" に変更
```

#### 2-2. 位置とサイズ設定
```
CurrentScoreText選択 → Inspector:

=== RectTransform ===
1. Anchor Presets → 右上 (top-right) をクリック
2. Pos X: -150
3. Pos Y: -50
4. Width: 250
5. Height: 60

=== TextMeshProUGUI ===
1. Text Input: "スコア: 0点"
2. Font Asset: LiberationSans SDF (デフォルト)
3. Font Size: 32
4. Color: #FFD700 (金色) または #FFFFFF (白)
5. Alignment: Right (右寄せ)
6. Auto Size: チェック
```

### Phase 3: FinalResultPanel 実装（5分）

#### 3-1. 最終結果パネル作成
```
1. MainGameCanvas 右クリック → UI → Panel
2. 名前を "FinalResultPanel" に変更
```

#### 3-2. パネル設定
```
FinalResultPanel選択 → Inspector:

=== RectTransform ===
1. Anchor Presets → Center-Middle をクリック
2. Pos X: 0
3. Pos Y: 0  
4. Width: 600
5. Height: 450

=== Image ===
1. Color: rgba(20, 25, 40, 220) ← 濃いダークブルー、やや透明
2. Image Type: Sliced
```

#### 3-3. パネルを初期非表示に
```
1. FinalResultPanel選択
2. Inspector上部のチェックボックスを外す（GameObject非アクティブ化）
```

### Phase 4: FinalScoreText 実装（3分）

#### 4-1. 最終スコアテキスト作成
```
1. FinalResultPanel 右クリック → UI → Text - TextMeshPro
2. 名前を "FinalScoreText" に変更
```

#### 4-2. テキスト設定
```
FinalScoreText選択 → Inspector:

=== RectTransform ===
1. Anchor Presets → Stretch-All (右下の四角いボタン)
2. Left: 30
3. Top: 30
4. Right: 30  
5. Bottom: 30

=== TextMeshProUGUI ===
1. Text Input: "最終スコア: 0点\n正答率: 0% (0/0問正解)"
2. Font Size: 28
3. Color: #FFFFFF (白)
4. Alignment: Center (中央揃え)
5. Vertical Alignment: Middle
6. Line Spacing: 10
7. Auto Size: チェック
```

### Phase 5: CryptoGameManager接続（2分）

#### 5-1. スクリプト接続
```
1. Hierarchy で CryptoGameManager オブジェクトを検索・選択
   (通常は GameManager や Main などの名前の可能性あり)
2. Inspector → CryptoGameManager スクリプト
3. Score UI セクション:
   ✓ Current Score Text: CurrentScoreText をドラッグ
   ✓ Final Score Text: FinalScoreText をドラッグ  
   ✓ Final Result Panel: FinalResultPanel をドラッグ
```

### Phase 6: テスト確認（1分）

#### 6-1. エディタでのテスト
```
1. Play ボタンを押してゲーム開始
2. 右上に "スコア: 0点" が表示されることを確認
3. 問題に答えてスコアが変動することを確認
4. 3分経過後に最終結果パネルが表示されることを確認
```

## 🎨 推奨カスタマイズ設定

### スタイリッシュな見た目にしたい場合

#### CurrentScoreText 強化版
```
=== 影効果追加 ===
Component → UI → Effects → Shadow
- Effect Color: #000000 (黒)
- Effect Distance: X=2, Y=-2

=== 縁取り効果 ===
TextMeshPro の Material Settings:
- Outline: 0.2
- Outline Color: #000000
```

#### FinalResultPanel 強化版
```
=== 背景画像使用 ===
1. Project で右クリック → Create → UI → Panel Background
2. Image コンポーネントで上記作成画像を設定

=== アニメーション効果 ===
1. Window → Animation → Animation
2. FinalResultPanel にアニメーション追加
3. Scale: 0→1 のアニメーション作成
```

### ゲーム画面に馴染ませる場合

#### 暗号ゲーム風デザイン
```
CurrentScoreText:
- Color: #00FFAA (サイバーグリーン)
- Font: 等幅フォント推奨

FinalResultPanel:
- Color: rgba(0, 20, 40, 240) (サイバーブルー)
- Border: #00FFAA の細いライン
```

## 🔧 トラブルシューティング

### よくある問題と解決法

#### ❌ テキストが見えない
```
解決法:
1. Canvas の Order in Layer を確認 (1以上)
2. Camera の Clear Flags 設定確認
3. UI要素の Color Alpha値が0でないか確認
```

#### ❌ 位置がおかしい  
```
解決法:
1. Canvas の Render Mode 確認
2. RectTransform の Anchor設定を再確認
3. Safe Area 設定がある場合は無効化テスト
```

#### ❌ スクリプト接続エラー
```
解決法:
1. CryptoGameManager スクリプトのコンパイルエラー確認
2. Inspector でMissing Scriptがないか確認  
3. UI要素の名前が正確か確認
```

## ⚡ 高速実装コマンド (上級者向け)

シーンファイル直接編集による一括設定も可能ですが、初回はGUI操作を推奨します。

この手順に従えば、**約20分**でスコア表示システムが完成します！
