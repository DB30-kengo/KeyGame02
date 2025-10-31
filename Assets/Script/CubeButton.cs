using UnityEngine;

public class CubeButton : MonoBehaviour
{
    [Header("Button Settings")]
    public int buttonIndex; // 0 or 1 for answer buttons
    public Material normalMaterial;
    public Material highlightMaterial;
    public string buttonText;
    
    private CryptoGameManager gameManager;
    private Renderer cubeRenderer;
    private TextMesh textMesh;
    
    private void Start()
    {
        gameManager = FindObjectOfType<CryptoGameManager>();
        cubeRenderer = GetComponent<Renderer>();
        
        // テキスト表示用の子オブジェクト作成
        GameObject textObject = new GameObject("ButtonText");
        textObject.transform.SetParent(transform);
        textObject.transform.localPosition = Vector3.forward * 0.6f;
        textObject.transform.localScale = Vector3.one * 0.1f;
        
        textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = buttonText;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontSize = 50;
    }
    
    private void OnMouseDown()
    {
        if (gameManager != null)
        {
            gameManager.OnAnswerSelected(buttonIndex);
            
            // ボタンエフェクト
            var uiManager = FindObjectOfType<CryptoUIManager>();
            if (uiManager != null)
            {
                uiManager.AnimateButtonPress(null); // 3D用の別メソッドを作成可能
            }
        }
    }
    
    private void OnMouseEnter()
    {
        if (highlightMaterial != null)
            cubeRenderer.material = highlightMaterial;
    }
    
    private void OnMouseExit()
    {
        if (normalMaterial != null)
            cubeRenderer.material = normalMaterial;
    }
    
    public void SetButtonText(string newText)
    {
        buttonText = newText;
        if (textMesh != null)
            textMesh.text = newText;
    }
    
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}