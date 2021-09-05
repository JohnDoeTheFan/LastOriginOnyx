using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> backgrounds;
    [SerializeField] private Vector2 size;

    private Bounds bounds;

    public void Construct(Bounds bounds, Vector2 position)
    {
        float xMin = bounds.min.x + size.x;
        float xMax = bounds.max.x - size.x;
        float yMin = bounds.min.y + size.y;
        float yMax = bounds.max.y - size.y;

        Vector2 newBoundSize = new Vector2(Mathf.Max(0, xMax - xMin), Mathf.Max(0, yMax - yMin));

        this.bounds = new Bounds(bounds.center, newBoundSize);
        ScrollBackground(position);
    }

    public void ScrollBackground(Vector2 position)
    {
        float newX = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
        float newY = Mathf.Clamp(position.y, bounds.min.y, bounds.max.y);
        Vector2 newPosition = new Vector2(newX, newY);

        transform.position = newPosition;
    }
}
