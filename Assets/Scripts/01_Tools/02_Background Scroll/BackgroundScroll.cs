using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private SpriteRenderer[] backgrounds;

    [Header("Settings")]
    [SerializeField] private float scrollSpeed = 1.0f;

    [Header("Status")]
    [SerializeField] private Vector2 startPoint;
    [SerializeField] private Vector2 endPoint;

    private void Awake()
    {
        float sizeX = backgrounds[0].bounds.size.x;

        startPoint = new Vector2(-sizeX, 0);
        endPoint = new Vector2(sizeX, 0);
    }

    private void Update()
    {
        Move();
        LoopBackgrounds();
    }

    private void Move()
    {
        Vector2 moveVector = scrollSpeed * Time.deltaTime * Vector2.right;

        for (int i = 0; i < backgrounds.Length; i++)
            backgrounds[i].transform.Translate(moveVector);
    }

    private void LoopBackgrounds()
    {
        for (int i = 0; i < backgrounds.Length; i++)
        {
            Vector2 bgPos = backgrounds[i].transform.position;

            if (bgPos.x >= endPoint.x)
            {
                float marginX = bgPos.x - endPoint.x;

                Vector2 pos = bgPos;
                pos.x = startPoint.x + marginX;
                backgrounds[i].transform.position = pos;
            }
        }
    }
}
