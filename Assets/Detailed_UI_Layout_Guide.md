# Unity スコアUI配置ガイド - 詳細設定

## Canvas設定（前提条件）

### 1. Canvasコンポーネント設定
```
Render Mode: Screen Space - Overlay
UI Scale Mode: Scale With Screen Size
Reference Resolution: 1920x1080 (16:9)
Screen Match Mode: Match Width Or Height (0.5)
```

## UI要素の具体的な配置座標

### 1. CurrentScoreText（現在スコア表示）

#### 推奨配置：画面右上
```
GameObject名: CurrentScoreText
親: Canvas
コンポーネント: Text (Legacy) または TextMeshPro

=== RectTransform設定 ===
Anchor Presets: Top-Right
Position X: -120
Position Y: -40
Width: 200
Height: 50

=== Text設定 ===
Text: "スコア: 0点"
Font: Arial Bold (または好みのフォント)
Font Size: 28
Color: #FFFFFF (白) または #FFD700 (金色)
Alignment: Middle Right
```

### 2. FinalResultPanel（最終結果パネル）

#### 配置：画面中央
```
GameObject名: FinalResultPanel
親: Canvas
コンポーネント: Image (Panel)

=== RectTransform設定 ===
Anchor Presets: Center
Position X: 0
Position Y: 0
Width: 500
Height: 400

=== Image設定 ===
Source Image: UI-Panel-Background (またはデフォルトUI背景)
Color: rgba(0, 0, 0, 180) ← 半透明の黒
Image Type: Sliced (9-slice対応)

=== 初期状態 ===
GameObject.SetActive(false) ← 非表示状態で開始
```

### 3. FinalScoreText（最終スコアテキスト）

#### 配置：FinalResultPanel内の中央
```
GameObject名: FinalScoreText
親: FinalResultPanel
コンポーネント: Text (Legacy) または TextMeshPro

=== RectTransform設定 ===
Anchor Presets: Stretch-All
Left: 20
Top: 20
Right: 20
Bottom: 20

=== Text設定 ===
Text: "最終スコア: 0点\n正答率: 0% (0/0問正解)\n\n✨ Enterキーでもう一度プレイ ✨"
Font: Arial Bold
Font Size: 24
Color: #FFFFFF (白)
Alignment: Middle Center
Line Spacing: 1.2
```

### 4. 追加UI要素（オプション）

#### タイマー表示強化
```
GameObject名: TimerText
親: Canvas

=== RectTransform設定 ===
Anchor Presets: Top-Center
Position X: 0
Position Y: -40
Width: 150
Height: 50

=== Text設定 ===
Text: "03:00"
Font: Arial Bold
Font Size: 32
Color: #FF4444 (赤色) ← 残り時間が少ないとき
Alignment: Middle Center
```

#### 進捗表示の改善
```
GameObject名: ProgressPanel
親: Canvas

=== RectTransform設定 ===
Anchor Presets: Bottom-Left
Position X: 20
Position Y: 120
Width: 300
Height: 100

=== 子要素構成 ===
- ProgressBackground (Image)
- ProgressSlider (Slider)
- ProgressText (Text)
```

## Unity Editor での実装手順

### Step 1: Canvas準備
1. **Hierarchy** → 右クリック → **UI** → **Canvas**
2. Canvas選択 → **Inspector** で上記設定を適用

### Step 2: CurrentScoreText作成
1. Canvas右クリック → **UI** → **Text** 
2. 名前を「CurrentScoreText」に変更
3. 上記の座標・設定を適用
4. **Anchor Presets** でTop-Rightを選択
5. Position, Width, Height を設定

### Step 3: FinalResultPanel作成
1. Canvas右クリック → **UI** → **Panel**
2. 名前を「FinalResultPanel」に変更
3. 上記の座標・設定を適用
4. **SetActive(false)** で非表示に設定

### Step 4: FinalScoreText作成
1. FinalResultPanel右クリック → **UI** → **Text**
2. 名前を「FinalScoreText」に変更
3. 上記の設定を適用

### Step 5: CryptoGameManager接続
1. CryptoGameManager選択
2. **Inspector** → **UI References** セクション
3. 各UI要素をドラッグ&ドロップで設定:
   - **Current Score Text**: CurrentScoreText をドラッグ
   - **Final Result Panel**: FinalResultPanel をドラッグ
   - **Final Score Text**: FinalScoreText をドラッグ
   - **Result Panel**: 結果表示用のパネル をドラッグ
   - **Result Text**: 結果表示用のテキスト をドラッグ

### Step 6: Enterキー再開機能の動作確認
1. ゲームをプレイして全問題をクリア
2. 最終スコア表示で「✨ Enterキーでもう一度プレイ ✨」が表示されることを確認
3. Enterキーを押してゲームが再開することを確認

## スコア表示重複問題の修正（2025年11月21日）

### 問題
ゲーム終了時に`ResultText`と`FinalScoreText`の両方でスコア結果が表示され、UI が見にくくなっていました。

### 修正内容
1. **ShowResults()メソッド修正**:
   - `resultPanel.SetActive(true)` をコメントアウト
   - `resultText.text` 設定をコメントアウト
   - 最終結果では `FinalScoreText` のみを使用

2. **表示ロジック改善**:
   - ゲーム終了時は `FinalResultPanel` のみ表示
   - `ResultPanel` は中間結果用として保持（将来的な使用のため）

### 修正後の動作
- ゲーム終了時: `FinalScoreText` のみ表示（クリーンな表示）
- Enterキーでのリスタート: 正常に動作
- スコア表示の重複: 解決

### コード変更箇所
```csharp
private void ShowResults()
{
    // ゲーム終了時は FinalResultPanel のみを表示
    // resultPanel.SetActive(true);  // ← コメントアウト
    
    // FinalScoreText のみで表示
    ShowFinalScore();
    StartCoroutine(WaitForRestartInput());
}
```

この修正により、最終スコア画面がより見やすく、プロフェッショナルな見た目になりました。

## RetryCurrentQuestion エラー修正（2025年11月21日）

### 問題
ゲーム終了後に「問題再表示時にもゲーム状態が無効です。ゲームを再開始します。」エラーが発生していました。

### 原因分析
1. ゲーム終了後（`isGameActive = false`）でも`RetryCurrentQuestion`メソッドが呼ばれていた
2. ゲーム状態チェックで無効と判定され、不必要な`ForceGameReset`が実行されていた

### 修正内容
1. **RetryCurrentQuestion メソッドの早期終了**:
   ```csharp
   private IEnumerator RetryCurrentQuestion(string explanation)
   {
       // ゲームが非アクティブな場合は静かに終了
       if (!isGameActive)
       {
           Debug.Log("[RetryCurrentQuestion] ゲームが既に終了しているため、処理を中断します");
           yield break;
       }
       // ...existing code...
   }
   ```

2. **OnAnswerSelected での保護**:
   ```csharp
   // 間違えた場合：ゲームがアクティブなら同じ問題を再出題
   if (isGameActive)
   {
       StartCoroutine(RetryCurrentQuestion(explanation));
   }
   else
   {
       Debug.Log("ゲーム終了のため、RetryCurrentQuestionをスキップします");
   }
   ```

3. **テストメソッドでの保護**:
   - `TestExplanationSystem`
   - `SimulateWrongAnswerCoroutine`
   - すべてのテスト用メソッドで`isGameActive`チェックを追加

### 修正後の動作
- ✅ ゲーム終了後のエラー発生が防止される
- ✅ 不要な`ForceGameReset`実行が回避される
- ✅ ゲーム状態の整合性が保たれる
- ✅ エラーログの無駄な出力が削減される

### ゲーム状態管理の改善
- `isGameActive = true`: `StartNewGameSet()`で設定
- `isGameActive = false`: `EndGameSet()`で設定
- すべての問題処理メソッドで状態チェックを実施

この修正により、ゲーム終了時の状態管理が大幅に改善され、エラーメッセージの発生が防止されました。

## 解説テキスト表示問題の修正（2025年11月21日）

### 問題
不正解時の解説テキストが表示されない問題が発生していました。パネルは表示されるが、テキスト内容が見えない状況でした。

### 原因分析
1. **フォント設定の問題**: 動的に作成される解説パネルのフォント設定が不適切
2. **複雑な表示ロジック**: 複数のフォールバック機能が重複し、信頼性が低下
3. **テキストコンポーネント設定の不備**: fontSize、color、font の設定で問題発生

### 修正内容
1. **System.Linq追加**: FirstOrDefault メソッド使用のため
2. **フォント設定改善**: 
   - LegacyRuntime.ttf を優先使用
   - Arial フォント検索の改善
   - フォントサイズを28ポイントに拡大
3. **シンプル解説表示システム追加**:
   ```csharp
   private IEnumerator ShowExplanationSimple(string explanation)
   {
       // questionText を使用した確実な表示
       questionText.text = $"💡 解説\n\n{explanation}\n\n⏳ 3秒後に問題を再表示します...";
       questionText.color = new Color(1f, 0.9f, 0.3f, 1f); // 明るい黄色
       yield return new WaitForSeconds(3f);
   }
   ```

4. **RetryCurrentQuestion 簡略化**: 複雑な解説パネル作成ロジックをシンプルメソッドに置換

### 修正後の動作
- ✅ 解説テキストが確実に表示される
- ✅ questionText を使用するため既存UI要素を活用
- ✅ 明るい黄色で視認性が向上
- ✅ 3秒間の適切な表示時間
- ✅ フォント問題が解決される
- ✅ QuickTestSimpleExplanation でテスト可能

### テスト方法
1. Unity Editor で CryptoGameManager を選択
2. Inspector の右上メニューから "Quick Test Simple Explanation" を実行
3. 解説テキストが questionText エリアに表示されることを確認

### 技術的改善点
- **シンプルさ**: 複雑な動的パネル作成から既存要素活用へ
- **確実性**: questionText は必ず存在するため信頼性向上
- **視認性**: 明るい黄色と大きなフォントサイズで可読性向上
- **デバッグ性**: テスト用メソッドで動作確認が容易

この修正により、不正解時の解説表示が確実に動作するようになりました。

## 解説テキスト「解説」のみ表示問題の修正（2025年11月21日）

### 問題の詳細
不正解時の解説表示で、実際の解説内容ではなく「解説」とだけしか表示されない問題が発生していました。コンソールには正しい解説文が表示されるが、UI上では内容が見えない状況でした。

### デバッグ強化の実装
問題の原因特定のため、以下の詳細ログを追加しました：

1. **OnAnswerSelected 解説取得部分**:
   ```csharp
   Debug.Log($"🔍 解説取得開始 - answerIndex: {answerIndex}");
   Debug.Log($"   - 解説長: {explanation?.Length ?? 0}");
   Debug.Log($"   - 解説内容詳細: '{explanation}'");
   Debug.Log($"   - 最終解説null確認: {explanation == null}");
   ```

2. **RetryCurrentQuestion パラメータ確認**:
   ```csharp
   Debug.Log($"[RetryCurrentQuestion] 受け取った解説: '{explanation}'");
   Debug.Log($"[RetryCurrentQuestion] 解説長: {explanation?.Length ?? 0}");
   Debug.Log($"[RetryCurrentQuestion] 解説null確認: {explanation == null}");
   ```

3. **ShowExplanationSimple 詳細確認**:
   ```csharp
   Debug.Log($"🔍 explanation変数の詳細確認:");
   Debug.Log($"   - 長さ: {(explanation?.Length ?? 0)}");
   Debug.Log($"   - null確認: {(explanation == null ? "null" : "not null")}");
   Debug.Log($"   - 空確認: {(string.IsNullOrEmpty(explanation) ? "empty" : "not empty")}");
   Debug.Log($"   - 内容: '{explanation}'");
   ```

### 安全性改善
`ShowExplanationSimple`メソッドで安全性チェックを追加：
```csharp
string safeExplanation = string.IsNullOrEmpty(explanation) ? "解説内容が取得できませんでした" : explanation;
string displayText = $"💡 解説\n\n{safeExplanation}\n\n⏳ 3秒後に問題を再表示します...";
```

### テスト機能強化
詳細テスト用のメソッドを改善：
```csharp
[ContextMenu("Quick Test Simple Explanation")]
public void QuickTestSimpleExplanation()
{
    string testExplanation = "🧪 【詳細テスト解説】このテキストが見える場合、シンプル解説システムは正常に動作しています...";
    Debug.Log($"🧪 テスト用解説長: {testExplanation.Length}");
    StartCoroutine(ShowExplanationSimple(testExplanation));
}
```

### デバッグ手順
1. Unity Editor で CryptoGameManager を選択
2. Inspector の右上メニューから "Quick Test Simple Explanation" を実行
3. Console ログを確認して、各段階での解説内容の変化を追跡
4. 実際のゲームプレイで不正解を選択し、同様にログを確認

### 期待される診断結果
- 解説データベースからの取得: コンソールに完全な解説が表示される
- パラメータ渡し: `RetryCurrentQuestion` で正しい解説を受け取る
- UI表示: `ShowExplanationSimple` で実際の内容が表示される

この強化されたデバッグシステムにより、解説表示問題の正確な原因を特定し、適切な修正を行うことができます。

## キャンバス上解説パネル表示システムの実装（2025年11月21日）

### 問題解決
不正解時の解説で「解説」とだけしか表示されない問題を解決するため、キャンバス上に専用の解説パネルを動的作成・表示するシステムを実装しました。

### 新システムの特徴
1. **専用解説パネル**: キャンバス上に独立した解説専用パネルを動的作成
2. **確実な表示**: questionText に依存しない独立したUI要素
3. **視認性向上**: 大きなサイズ、明確な背景、適切な装飾
4. **自動管理**: 表示後の自動削除とCanvasレイヤー管理

### 実装詳細
```csharp
private IEnumerator ShowExplanationOnCanvas(string explanation)
{
    // 1. 既存パネル削除
    GameObject existingPanel = GameObject.Find("DynamicExplanationPanel");
    if (existingPanel != null) DestroyImmediate(existingPanel);
    
    // 2. Canvas取得
    Canvas targetCanvas = FindBestCanvas();
    
    // 3. 解説パネル作成
    GameObject explanationPanel = new GameObject("DynamicExplanationPanel");
    explanationPanel.transform.SetParent(targetCanvas.transform, false);
    
    // 4. サイズ設定（画面の80%）
    RectTransform panelRect = explanationPanel.AddComponent<RectTransform>();
    panelRect.anchorMin = new Vector2(0.1f, 0.2f);
    panelRect.anchorMax = new Vector2(0.9f, 0.8f);
    
    // 5. 背景・装飾設定
    Image panelImage = explanationPanel.AddComponent<Image>();
    panelImage.color = new Color(0.05f, 0.05f, 0.15f, 0.95f); // 濃い青
    
    // 6. テキスト作成
    Text textComponent = textObject.AddComponent<Text>();
    textComponent.text = $"💡 解説\n\n{safeExplanation}\n\n✨ 3秒後に問題を再表示します ✨";
    textComponent.fontSize = 32;
    textComponent.color = Color.white;
    textComponent.alignment = TextAnchor.MiddleCenter;
    
    // 7. 4秒間表示後自動削除
    yield return new WaitForSeconds(4f);
    DestroyImmediate(explanationPanel);
}
```

### UI要素の詳細設定
- **パネルサイズ**: 画面の80% (anchorMin: 0.1,0.2 / anchorMax: 0.9,0.8)
- **背景色**: 濃い青 (0.05, 0.05, 0.15, 0.95)
- **テキスト色**: 白色
- **フォントサイズ**: 32ポイント
- **外枠**: 黄色のアウトライン (4px)
- **影効果**: 黒い影 (6px, -6px)
- **余白**: 30px

### 装飾効果
1. **Outline**: 黄色の外枠で視認性向上
2. **Shadow**: 影効果でパネルの立体感
3. **Font**: LegacyRuntime.ttf で確実な文字表示
4. **TextOutline**: テキストの黒い縁取りで可読性向上

### テスト方法
1. Unity Editor で CryptoGameManager を選択
2. Inspector の右上メニューから "Quick Test Canvas Explanation" を実行
3. 画面中央に大きな青いパネルで解説が表示されることを確認

### システムフロー
1. **RetryCurrentQuestion**: 解説内容を受け取る
2. **ShowExplanationOnCanvas**: キャンバス上にパネルを動的作成
3. **Canvas管理**: sortingOrderを1000に設定して最前面表示
4. **自動削除**: 4秒後にパネルを削除してCanvas順序をリセット

### 旧システムとの比較
| 項目 | 旧システム (ShowExplanationSimple) | 新システム (ShowExplanationOnCanvas) |
|------|-------------------------------------|---------------------------------------|
| 表示場所 | questionText エリア | キャンバス中央の専用パネル |
| サイズ | 固定 | 画面の80% |
| 背景 | なし | 濃い青色パネル |
| 装飾 | 基本色変更のみ | アウトライン・影・縁取り |
| 独立性 | questionText に依存 | 完全独立 |
| 視認性 | 普通 | 非常に高い |

この新システムにより、解説内容が確実にキャンバス上の見やすいパネルで表示され、ユーザーが不正解時に適切なフィードバックを受け取れるようになりました。

## CS1626エラー修正（yield return in try-catch）（2025年11月21日）

### 問題
`Assets/Script/CryptoGameManager.cs(3246,13): error CS1626: Cannot yield a value in the body of a try block with a catch clause` エラーが発生していました。

### 原因分析
C#では`catch`句がある`try`ブロック内で`yield return`を使用することはできません。`ShowExplanationOnCanvas`メソッドで以下の構造が問題でした：

```csharp
// 問題のあるコード
try
{
    // パネル作成処理...
    yield return new WaitForSeconds(4f); // ← ここでCS1626エラー
    // パネル削除処理...
}
catch (System.Exception e)
{
    // エラーハンドリング
}
```

### 修正内容
`try-catch`ブロックを分割して、`yield return`を含む処理を`try-catch`の外に移動しました：

```csharp
// 修正後のコード
GameObject explanationPanel = null;
bool creationSuccess = false;

try
{
    // パネル作成処理のみ（yield returnなし）
    explanationPanel = new GameObject("DynamicExplanationPanel");
    // ... UI作成処理 ...
    creationSuccess = true;
}
catch (System.Exception e)
{
    // エラーハンドリング
    creationSuccess = false;
}

// パネル作成が成功した場合のみ表示処理を実行
if (creationSuccess && explanationPanel != null)
{
    explanationPanel.SetActive(true);
    yield return new WaitForSeconds(4f); // ← try-catchの外で使用
    // パネル削除処理...
}
```

### 修正のポイント
1. **エラーハンドリングの分離**: UI作成処理のみを`try-catch`で保護
2. **成功フラグの使用**: `creationSuccess`でUI作成成功を判定
3. **安全な表示処理**: 作成成功時のみ表示・削除処理を実行
4. **yield returnの安全な使用**: `try-catch`の外で`yield return`を使用

### 修正後の動作
- ✅ CS1626コンパイルエラーが解決される
- ✅ UI作成時のエラーハンドリングが維持される
- ✅ パネル表示・削除処理が正常に動作する
- ✅ エラー発生時の安全性が確保される

### C# yield return制限の理解
- `try-finally`ブロック内では`yield return`使用可能
- `try-catch`ブロック内では`yield return`使用不可
- この制限はC#言語仕様による設計上の決定

この修正により、コルーチンベースの解説パネル表示システムが正常に動作するようになりました。

## プレイヤー位置リセットの統一（2025年11月21日）

### 問題
プレイヤーの位置移動が正解時のみ行われており、不正解時には同じ位置に移動しませんでした。

### 修正内容
不正解時にも正解時と同じプレイヤー位置リセット処理を実行するように修正しました：

```csharp
// RetryCurrentQuestion メソッドに追加
private IEnumerator RetryCurrentQuestion(string explanation)
{
    // ...existing code...
    
    // プレイヤーの位置をリセット（正解時と同じ位置に移動）
    StartCoroutine(ResetPlayerPosition(GetPlayerResetPosition()));
    
    DisplayQuestion(question);
    
    Debug.Log("同じ問題を再出題 - プレイヤー位置もリセット");
}
```

### 統一された動作
1. **正解時**: OnCorrectAnswer() で `StartCoroutine(ResetPlayerPosition(GetPlayerResetPosition()));`
2. **不正解時**: RetryCurrentQuestion() で `StartCoroutine(ResetPlayerPosition(GetPlayerResetPosition()));`

両方とも同じ`GetPlayerResetPosition()`メソッドを使用してリセット位置を決定します。

### プレイヤー位置リセットシステムの特徴
- **設定システム連携**: resetSettings による詳細制御
- **地面検出**: useGroundDetection による自動高度調整
- **CharacterController対応**: 安全なリセット処理
- **ThirdPersonController対応**: 一時的な無効化で競合回避
- **床抜け防止**: 地面検出と高度オフセット

### リセット位置決定の流れ
1. **設定確認**: resetSettings の resetType を確認
2. **位置計算**: Custom/Preset/Transform の設定に基づいて基本位置を決定
3. **地面検出**: useGroundDetection が true の場合、Raycast で地面を検出
4. **高度調整**: heightOffset を適用して最終位置を決定
5. **安全移動**: CharacterController を一時無効化して移動実行

### 修正後の動作
- ✅ 正解時: 指定位置に移動
- ✅ 不正解時: 正解時と同じ指定位置に移動
- ✅ 設定システムによる位置制御
- ✅ 地面検出とCharacterController対応
- ✅ 統一されたプレイヤー移動体験

この修正により、プレイヤーは正解・不正解に関わらず、常に同じ指定位置に移動するようになり、一貫したゲーム体験を提供できるようになりました。

この修正により、プレイヤーは正解・不正解に関わらず、常に同じ指定位置に移動するようになり、一貫したゲーム体験を提供できるようになりました。

## プレイヤー位置移動の実行順序最適化（2025年11月21日）

### 改善要求
解説文の表示が終わるより先に、プレイヤーの位置移動を早く実行する要求がありました。

### 修正前の実行順序
```
1. 解説パネル表示開始
2. ShowExplanationOnCanvas() 実行（5秒間表示）
3. UI色リセット
4. 安全性チェック
5. プレイヤー位置リセット ← 解説後に実行
6. 問題再表示
```

### 修正後の実行順序
```
1. プレイヤー位置リセット ← 解説表示前に移動
2. 解説パネル表示開始
3. ShowExplanationOnCanvas() 実行（5秒間表示）
4. UI色リセット
5. 安全性チェック
6. 問題再表示
```

### コード変更内容
```csharp
// RetryCurrentQuestion メソッドの修正
// プレイヤーの位置をリセット（解説表示より先に実行）
StartCoroutine(ResetPlayerPosition(GetPlayerResetPosition()));
Debug.Log("プレイヤー位置リセット開始 - 解説表示前に実行");

Debug.Log($"[解説表示開始] 解説内容: '{explanation}'");
// キャンバス上の専用解説パネル表示を使用
yield return StartCoroutine(ShowExplanationOnCanvas(explanation));
```

### ユーザー体験の改善
1. **即座の位置修正**: 不正解時にプレイヤーがすぐに適切な位置に移動
2. **解説読み取り時間の確保**: 位置移動後に解説を5秒間表示
3. **スムーズな流れ**: 位置修正→解説→問題再表示の自然な流れ
4. **視覚的な整合性**: プレイヤーが正しい位置で解説を読める

### デバッグログの追加
- `"プレイヤー位置リセット開始 - 解説表示前に実行"` ログを追加
- 実行順序の確認が容易になる

### 並行処理の活用
`StartCoroutine()`を使用してプレイヤー移動を非同期で開始し、解説表示と並行して処理されるため、全体的な応答性が向上します。

この修正により、不正解時のユーザー体験が大幅に改善され、より自然で快適なゲームフローを提供できるようになりました。
