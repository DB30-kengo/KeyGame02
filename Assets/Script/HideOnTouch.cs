using UnityEngine;
using System.Collections;

public class HideOnTouch : MonoBehaviour
{
    [Header("非表示にするまでの時間（秒）")]
    public float delayBeforeHide = 10f;

    private bool hasTouched = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTouched && other.CompareTag("Player"))
        {
            hasTouched = true;
            StartCoroutine(HideAfterDelay());
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeHide);
        gameObject.SetActive(false);
    }
}
