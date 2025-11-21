# UI設定が元に戻る問題の解決ガイド

## 🎯 問題の原因
- **CryptoUILayout.cs**スクリプトが`Start()`メソッドでUI要素を強制的に上書き
- エディタで設定したフォントサイズや位置が無視される

## ✅ 解決済み修正

### 1. 自動レイアウト機能を無効化
```csharp
// CryptoUILayout.cs
public bool autoSetupLayout = false;  // true から false に変更
```

### 2. カスタム設定保持機能を追加
```csharp
// 新しく追加されたフィールド
public bool preserveCustomSettings = true;
public int customQuestionFontSize = 32;
public int customProgressFontSize = 20;
public int customTimerFontSize = 24;
```

## 🚀 Unity エディタでの使用方法

### 方法A: 完全に自動レイアウトを無効化（簡単）

1. **Hierarchy** で `CryptoUILayout` スクリプトがアタッチされたオブジェクトを選択
2. **Inspector** → `CryptoUILayout` → `Auto Setup Layout` を **チェックオフ**
3. 好みのUI設定（フォントサイズ、位置など）をエディタで調整
4. プレイモード開始 → 設定が保持される ✅

### 方法B: カスタム設定を使用（推奨・柔軟）

1. エディタでUI要素を理想的な状態に調整
2. **CryptoUILayout** オブジェクト選択 → **Inspector**
3. 右クリック → **"Capture Current UI Settings"** を実行
   ```
   コンソールに "QuestionText FontSize captured: XX" などが表示される
   ```
4. **Preserve Custom Settings** を **チェックオン**
5. **Custom Question Font Size**, **Custom Progress Font Size** などの値が自動設定される
6. プレイモード開始 → カスタム設定が適用される ✅

### 方法C: 設定値の手動調整

**Inspector** で直接値を調整：
```
CryptoUILayout (Script)
├─ Auto Setup Layout: ☐ false
├─ Preserve Custom Settings: ☑ true
├─ Custom Question Font Size: 32
├─ Custom Progress Font Size: 20
└─ Custom Timer Font Size: 24
```

## 🔧 デバッグ機能

### エディタでのテスト
**Inspector** 右クリックメニュー:
- **"Capture Current UI Settings"**: 現在の設定を取得
- **"Apply Custom Settings"**: カスタム設定を即座に適用

### 設定確認
```csharp
// Console出力例
QuestionText FontSize captured: 28
ProgressText FontSize captured: 18
TimerText FontSize captured: 22
現在のUI設定をキャプチャしました。preserveCustomSettings = true にしてください。
```

## 📱 各UI要素の推奨設定

### QuestionText (質問文)
- **推奨フォントサイズ**: 28-36
- **位置**: 画面中央やや上
- **色**: 白（#FFFFFF）

### ProgressText (進捗表示)
- **推奨フォントサイズ**: 18-24
- **位置**: 画面上部中央
- **色**: 黄色（#FFFF00）

### TimerText (タイマー)
- **推奨フォントサイズ**: 22-28
- **位置**: 画面右上
- **色**: 白（#FFFFFF）/ 残り時間少ないとき赤（#FF4444）

## 🎨 ベストプラクティス

### 1. 段階的な調整
1. まず `Auto Setup Layout = false` で基本動作確認
2. UI配置を細かく調整
3. `Capture Current UI Settings` でバックアップ
4. `Preserve Custom Settings = true` で完全カスタム化

### 2. 解像度対応
- Canvas Scaler設定を確認
- 複数解像度でテストプレイ
- 必要に応じて追加調整

### 3. パフォーマンス
- 不要なスクリプトは無効化
- 動的UI変更は最小限に

## ⚠️ 注意点

1. **シーン保存を忘れずに** - UI設定変更後は必ずシーン保存
2. **Prefab更新** - UI要素がPrefabの場合は適用を忘れずに
3. **バックアップ** - 大きな変更前にシーンのバックアップ推奨

## 🔄 元に戻す場合

元の自動レイアウトに戻したい場合：
1. `Auto Setup Layout = true`
2. `Preserve Custom Settings = false`
3. プレイモード開始でデフォルト設定に復帰

これで、エディタでのUI設定がプレイモードでも保持されます！
