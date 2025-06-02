using UnityEngine;

public class RevealOnTouch : MonoBehaviour
{
    [Tooltip("プレイヤーと接触したときだけ表示されるようにする対象のRenderer")]
    public Renderer targetRenderer;

    private void Start()
    {
        if (targetRenderer != null)
        {
            targetRenderer.enabled = false; // 最初は非表示
        }
        else
        {
            Debug.LogWarning("Rendererが指定されていません");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && targetRenderer != null)
        {
            targetRenderer.enabled = true; // 接触したら表示
        }
    }
}
