using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitionManager : MonoBehaviour
{
    public static LevelTransitionManager Instance { get; private set; }

    [SerializeField] private LevelState levelState;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    
    public void LoadNextLevel()
    {
        if (levelState != null) levelState.ResetState();
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        Debug.Log($"[LevelTransition] Sonraki level yükleniyor: {next}");
        SceneManager.LoadScene(next);
    }

    public void RestartCurrentLevel()
    {
        if (levelState != null) levelState.ResetState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}