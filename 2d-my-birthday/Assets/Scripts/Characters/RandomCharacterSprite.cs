using UnityEngine;

public class RandomCharacterSprite : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] sprites;

    private static int _lastIndex = -1;

    private void Awake()
    {
        if (sprites == null || sprites.Length == 0) return;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        int index;
        if (sprites.Length == 1)
        {
            index = 0;
        }
        else
        {
            do { index = Random.Range(0, sprites.Length); }
            while (index == _lastIndex);
        }

        _lastIndex = index;
        spriteRenderer.sprite = sprites[index];
    }
}