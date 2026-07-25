using UnityEngine;

public class LevelMusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] tracks;

    private static int _lastIndex = -1;

    private void Start()
    {
        if (audioSource == null || tracks == null || tracks.Length == 0) return;

        int index;
        if (tracks.Length == 1)
        {
            index = 0;
        }
        else
        {
            do { index = Random.Range(0, tracks.Length); }
            while (index == _lastIndex);
        }

        _lastIndex = index;
        audioSource.clip = tracks[index];
        audioSource.Play();
    }
}