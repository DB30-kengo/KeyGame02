using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TouchActivatedLabel : MonoBehaviour
{
    [Header("表示するテキスト内容")]
    [TextArea]
    public string labelText = "ここに表示する内容を入力";

    [Header("ラベルのプレハブ（World Space Canvas + Text付き）")]
    public GameObject labelPrefab;

    [Header("ラベルの表示位置オフセット")]
    public Vector3 labelOffset = new Vector3(0, 2f, 0);

    [Header("表示時間（秒）")]
    public float displayDuration = 5f;

    private GameObject currentLabel;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && currentLabel == null && labelPrefab != null)
        {
            ShowFloatingLabel();
        }
    }

    private void ShowFloatingLabel()
    {
        currentLabel = Instantiate(labelPrefab, transform.position + labelOffset, Quaternion.identity);
        currentLabel.transform.SetParent(transform); // ラベルをこのオブジェクトの子に

        // テキスト設定
        Text textComponent = currentLabel.GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            textComponent.text = labelText;
        }

        // 一定時間後に削除
        StartCoroutine(HideLabelAfterDelay());
    }

    private IEnumerator HideLabelAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        if (currentLabel != null)
        {
            Destroy(currentLabel);
        }
    }
}
