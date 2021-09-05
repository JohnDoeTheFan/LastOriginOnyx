using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> backgrounds;
    [SerializeField][Range(0, 1)] private float minBackgroundScrollAlpha = 0.5f;

    private Bounds cameraMovementBounds;
    private Bounds tileMapBounds;

    // Minimum background scroll maximum;
    // Maximum background scroll minimum;
    private Vector2 minimumBackgroundSize;
    private Vector2 maximumBackgroundSize;
    private Vector2 maximumScrollSize;
    private Vector2 minimumScrollSize;

    public void Construct(Bounds cameraMovementBounds, Vector2 cameraPosition, Bounds tileMapBounds)
    {
        this.cameraMovementBounds = cameraMovementBounds;
        this.tileMapBounds = tileMapBounds;

        CacheMinimumAndMaximumBackgroundSize();
        CacheMinimumAndMaximumScrollSize();

        ScrollBackground(cameraPosition);
    }

    /// <summary>
    /// 카메라 위치 정보를 참고하여, 배경을 스크롤합니다. 이 함수를 호출하기 전에 카메라 위치를 업데이트하세요.
    /// </summary>
    /// <param name="cameraPosition">카메라 위치</param>
    public void ScrollBackground(Vector2 cameraPosition)
    {
        float newX = Mathf.Clamp(cameraPosition.x, cameraMovementBounds.min.x, cameraMovementBounds.max.x);
        float newY = Mathf.Clamp(cameraPosition.y, cameraMovementBounds.min.y, cameraMovementBounds.max.y);
        Vector2 clampedPosition = new Vector2(newX, newY);

        Vector2 boundCenterVector2 = cameraMovementBounds.center;
        Vector2 relativePosition = clampedPosition - boundCenterVector2;

        Vector2 positionPercentage;
        positionPercentage.x = (cameraMovementBounds.size.x > 0)? relativePosition.x / cameraMovementBounds.size.x : 0;
        positionPercentage.y = (cameraMovementBounds.size.y > 0) ? relativePosition.y / cameraMovementBounds.size.y : 0;

        foreach (SpriteRenderer background in backgrounds )
        {
            Scroll(background, positionPercentage);
        }
    }

    private void ScrollOlder(SpriteRenderer background, Vector2 positionPercentage)
    {
        Vector2 newLocalPosition = new Vector2();
        if(background.size.x < tileMapBounds.size.x)
        {
            float scrollSize = tileMapBounds.size.x - background.size.x;
            newLocalPosition.x = scrollSize * positionPercentage.x;
        }
        else
        {
            float sizePercentage = (background.size.x - minimumBackgroundSize.x) / (maximumBackgroundSize.x - minimumBackgroundSize.x);
            float scrollSize = cameraMovementBounds.size.x * positionPercentage.x * (1 - sizePercentage);
            newLocalPosition.x = scrollSize;
        }

        if(background.size.y < tileMapBounds.size.y)
        {
            float scrollSize = tileMapBounds.size.y - background.size.y;
            newLocalPosition.y = scrollSize * positionPercentage.y;
        }
        else
        {
            float scrollSize = maximumBackgroundSize.y - background.size.y;
            newLocalPosition.y = scrollSize * positionPercentage.y;
        }

        background.transform.localPosition = newLocalPosition;
    }

    private void Scroll(SpriteRenderer background, Vector2 positionPercentage)
    {
        Vector2 newLocalPosition = new Vector2();

        float scrollSizeX = CalcScrollSize(background.size.x, minimumBackgroundSize.x, maximumBackgroundSize.x, maximumScrollSize.x, minimumScrollSize.x);
        newLocalPosition.x = scrollSizeX * positionPercentage.x;

        float scrollSizeY = CalcScrollSize(background.size.y, minimumBackgroundSize.y, maximumBackgroundSize.y, maximumScrollSize.y, minimumScrollSize.y);
        newLocalPosition.y = scrollSizeY * positionPercentage.y;

        background.transform.localPosition = newLocalPosition;
    }

    private float CalcScrollSize(float backgroundSize, float minimumBackgroundSize, float maximumBackgroundSize, float maximumScrollSize, float minimumScrollSize)
    {
        if (maximumBackgroundSize == minimumBackgroundSize)
            return minimumScrollSize;
        else
        {
            float sizePercentage = (backgroundSize - minimumBackgroundSize) / (maximumBackgroundSize - minimumBackgroundSize);
            return maximumScrollSize * (1 - sizePercentage) + minimumScrollSize * sizePercentage;
        }
    }

    private void CacheMinimumAndMaximumBackgroundSize()
    {
        minimumBackgroundSize = new Vector2(float.MaxValue, float.MaxValue);
        maximumBackgroundSize = new Vector2(0, 0);
        foreach (SpriteRenderer background in backgrounds)
        {
            if (background.size.x < minimumBackgroundSize.x)
                minimumBackgroundSize.x = background.size.x;
            if (background.size.y < minimumBackgroundSize.y)
                minimumBackgroundSize.y = background.size.y;

            if (background.size.x > maximumBackgroundSize.x)
                maximumBackgroundSize.x = background.size.x;
            if (background.size.y > maximumBackgroundSize.y)
                maximumBackgroundSize.y = background.size.y;
        }
    }

    private void CacheMinimumAndMaximumScrollSize()
    {
        minimumScrollSize.x = CalcMinimumScrollSize(maximumBackgroundSize.x, tileMapBounds.size.x, cameraMovementBounds.size.x);
        minimumScrollSize.y = CalcMinimumScrollSize(maximumBackgroundSize.y, tileMapBounds.size.y, cameraMovementBounds.size.y);

        maximumScrollSize.x = CalcMaximumScrollSize(minimumBackgroundSize.x, tileMapBounds.size.x, cameraMovementBounds.size.x, minimumScrollSize.x);
        maximumScrollSize.y = CalcMaximumScrollSize(minimumBackgroundSize.y, tileMapBounds.size.y, cameraMovementBounds.size.y, minimumScrollSize.y);
    }

    private float CalcMinimumScrollSize(float maximumBackgroundSize, float tileMapSize, float cameraMovementBoundsSize)
    {
        float scrollSize = tileMapSize - maximumBackgroundSize;

        scrollSize = Mathf.Min(scrollSize, cameraMovementBoundsSize);
        scrollSize = Mathf.Max(scrollSize, 0);

        return scrollSize;
    }

    private float CalcMaximumScrollSize(float minimumBackgroundSize, float tileMapSize, float cameraMovementBoundsSize)
    {
        if (minimumBackgroundSize < tileMapSize)
            return tileMapSize - minimumBackgroundSize;
        else
            return cameraMovementBoundsSize;
    }

    private float CalcMaximumScrollSize(float minimumBackgroundSize, float tileMapSize, float cameraMovementBoundsSize, float minimumScrollSize)
    {
        float scrollSize = tileMapSize - minimumBackgroundSize;
        return Mathf.Max(minimumScrollSize, scrollSize) * minBackgroundScrollAlpha + cameraMovementBoundsSize * (1 - minBackgroundScrollAlpha);
    }
}
