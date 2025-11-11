using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI関連の共通ユーティリティクラス
/// </summary>
public static class UIUtility
{
    /// <summary>
    /// 安全なデフォルトフォント取得
    /// Unity 2023.2以降でArial.ttfが廃止されたため、LegacyRuntime.ttfを使用
    /// </summary>
    /// <returns>利用可能なデフォルトフォント</returns>
    public static Font GetDefaultFont()
    {
        Font font = null;
        
        // まずLegacyRuntime.ttfを試行
        try
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                return font;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[UIUtility] LegacyRuntime.ttfの取得に失敗: {e.Message}");
        }
        
        // フォールバック：Arial.ttfを試行（古いUnityバージョン対応）
        try
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null)
            {
                Debug.LogWarning("[UIUtility] Arial.ttfを使用しています。Unity 2023.2以降では非推奨です。");
                return font;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[UIUtility] Arial.ttfの取得に失敗: {e.Message}");
        }
        
        // 最終フォールバック：Resourcesからフォントを検索
        try
        {
            Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
            if (fonts != null && fonts.Length > 0)
            {
                foreach (Font f in fonts)
                {
                    if (f != null && !string.IsNullOrEmpty(f.name))
                    {
                        Debug.Log($"[UIUtility] 代替フォントを使用: {f.name}");
                        return f;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[UIUtility] 代替フォント検索に失敗: {e.Message}");
        }
        
        Debug.LogError("[UIUtility] 利用可能なフォントが見つかりません。null を返します。");
        return null;
    }
    
    /// <summary>
    /// TextMesh Proが利用可能かチェック
    /// </summary>
    /// <returns>TextMesh Proが使用可能な場合true</returns>
    public static bool IsTextMeshProAvailable()
    {
        try
        {
            // TextMeshProUGUIの型が存在するかチェック
            System.Type tmpType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            return tmpType != null;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// 安全なText コンポーネント作成
    /// TextMesh Proが利用可能な場合は推奨メッセージを表示
    /// </summary>
    /// <param name="gameObject">TextコンポーネントをアタッチするGameObject</param>
    /// <param name="text">表示テキスト</param>
    /// <param name="fontSize">フォントサイズ</param>
    /// <param name="alignment">テキスト配置</param>
    /// <param name="color">テキスト色</param>
    /// <returns>作成されたTextコンポーネント</returns>
    public static Text CreateSafeText(GameObject gameObject, string text = "", int fontSize = 14, 
        TextAnchor alignment = TextAnchor.MiddleCenter, Color? color = null)
    {
        if (gameObject == null)
        {
            Debug.LogError("[UIUtility] CreateSafeText: gameObjectがnullです。");
            return null;
        }
        
        Text textComponent = gameObject.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = GetDefaultFont();
        textComponent.fontSize = fontSize;
        textComponent.alignment = alignment;
        textComponent.color = color ?? Color.white;
        
        // TextMesh Proが利用可能な場合は推奨メッセージ（1回のみ）
        if (IsTextMeshProAvailable() && !_tmpRecommendationShown)
        {
            Debug.Log("[UIUtility] TextMesh Proが利用可能です。より高品質なテキスト表示のためTextMeshProUGUIの使用を推奨します。");
            _tmpRecommendationShown = true;
        }
        
        return textComponent;
    }
    
    private static bool _tmpRecommendationShown = false;
}
