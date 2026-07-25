using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroSceneController : MonoBehaviour
{
    [SerializeField] private TypewriterPlayer typewriter;
    [SerializeField] private string nextSceneName;
    [SerializeField] private float delayAfterSequence = 1f;

    void Start()
    {
        if (typewriter != null)
        {
            typewriter.OnSequenceComplete += HandleSequenceComplete;
            typewriter.Play();
        }
    }

    void OnDestroy()
    {
        if (typewriter != null)
            typewriter.OnSequenceComplete -= HandleSequenceComplete;
    }

    void HandleSequenceComplete()
    {
        Debug.Log("[Intro] Narrative bitti, bir sonraki sahneye geçiliyor.");
        Invoke(nameof(LoadNextScene), delayAfterSequence);
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}