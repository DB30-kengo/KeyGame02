# スコア表示UI設定ガイド

## 必要なUI要素の設定

### 1. 現在のスコア表示
- **オブジェクト名**: `CurrentScoreText`
- **型**: Text (UI)
- **位置**: Canvas左上または右上
- **フォント**: Bold推奨
- **サイズ**: 24-32
- **カラー**: 白または黄色
- **テキスト**: "スコア: 0点"

### 2. 最終スコア表示パネル
- **オブジェクト名**: `FinalResultPanel`
- **型**: Panel (UI)
- **位置**: Canvas中央
- **サイズ**: 400x300
- **背景**: 半透明の黒またはダークブルー
- **子要素**: FinalScoreText

### 3. 最終スコアテキスト
- **オブジェクト名**: `FinalScoreText`
- **型**: Text (UI) - FinalResultPanelの子要素
- **位置**: パネル中央
- **フォント**: Bold
- **サイズ**: 20-24
- **カラー**: 白
- **テキスト**: "最終スコア: 0点\n正答率: 0% (0/0問正解)"
- **Text Alignment**: Center

## CryptoGameManagerへの設定手順

1. **Hierarchy**でCryptoGameManagerオブジェクトを選択
2. **Inspector**でCryptoGameManagerコンポーネントを確認
3. **Score UI - スコア表示**セクションで以下を設定：
   - **Current Score Text**: CurrentScoreTextオブジェクトをドラッグ&ドロップ
   - **Final Score Text**: FinalScoreTextオブジェクトをドラッグ&ドロップ
   - **Final Result Panel**: FinalResultPanelオブジェクトをドラッグ&ドロップ

## スコア設定のカスタマイズ

CryptoGameManagerスクリプトで以下の値を調整可能：
- `pointsPerCorrect = 10`: 正解時の獲得ポイント
- `pointsPerIncorrect = -2`: 不正解時の減点

## 動作確認

1. ゲーム開始時に「スコア: 0点」が表示される
2. 正解すると「+10点」でスコアが増加
3. 不正解すると「-2点」でスコアが減少（0点未満にはならない）
4. 3分終了時に最終結果パネルが表示される
5. 5秒後に自動で新しいゲームセットが開始される

## 注意点

- FinalResultPanelは初期状態で非アクティブ（SetActive(false)）にしておく
- CurrentScoreTextは常に表示状態にしておく
- スコア表示はゲーム中にリアルタイムで更新される
