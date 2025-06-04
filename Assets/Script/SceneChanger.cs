using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneChanger : MonoBehaviour
{
    [Tooltip("切り替え先のシーン名（Build Settingsに登録されている必要があります）")]
    public string sceneToLoad;

    [Tooltip("シーン切り替えまでの待機時間（秒）")]
    public float delaySeconds = 0.2f;

    private bool isTransitioning = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isTransitioning && other.CompareTag("Player"))
        {
            isTransitioning = true;
            StartCoroutine(LoadSceneWithDelay());
        }
    }

    private IEnumerator LoadSceneWithDelay()
    {
        yield return new WaitForSeconds(delaySeconds);

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("Scene name is not set in the Inspector.");
        }
    }
}
