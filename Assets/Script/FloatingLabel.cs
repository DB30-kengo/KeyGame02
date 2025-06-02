using UnityEngine;
using UnityEngine.UI;

public class FloatingLabel : MonoBehaviour
{
    [Header("UI表示内容")]
    [TextArea]
    public string labelText = "ここに表示するテキスト";

    [Header("UI CanvasとTextへの参照")]
    public Canvas worldCanvas;
    public Text label;

    [Header("表示位置のオフセット")]
    public Vector3 offset = new Vector3(0, 2, 0); // オブジェクトの上に表示

    void Start()
    {
        if (worldCanvas == null || label == null)
        {
            Debug.LogWarning("Canvas または Text の参照が設定されていません。");
            return;
        }

        label.text = labelText;
    }

    void LateUpdate()
    {
        if (worldCanvas != null)
        {
            // オブジェクトの上に表示し続ける
            worldCanvas.transform.position = transform.position + offset;

            // カメラの方向を向かせる
            worldCanvas.transform.forward = Camera.main.transform.forward;
        }
    }
}
