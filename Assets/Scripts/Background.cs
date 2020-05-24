using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Background : MonoBehaviour
{
    private void Start()
    {
        Image backgroundImage = GetComponentInChildren<Image>();
        RectTransform backgroundRectTransform = backgroundImage.GetComponent<RectTransform>();
        Rect canvasRect = transform.GetComponent<RectTransform>().rect;

        backgroundRectTransform.offsetMax = new Vector2(canvasRect.xMax, canvasRect.yMax);
        backgroundRectTransform.offsetMin = new Vector2(canvasRect.xMin, canvasRect.yMin);
    }
}
