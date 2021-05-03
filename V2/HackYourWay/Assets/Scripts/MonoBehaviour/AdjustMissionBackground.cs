using TMPro;
using UnityEngine;

public class AdjustMissionBackground : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI adjustAfter;

    // Update is called once per frame
    private void Update()
    {
        RectTransform rect = GetComponent<RectTransform>();

        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, adjustAfter.preferredWidth);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, adjustAfter.preferredHeight);

        float topY = adjustAfter.rectTransform.position.y + (adjustAfter.rectTransform.rect.height / 2);
        float posY = topY - (rect.rect.height / 2);

        Vector3 newPos = rect.position;
        newPos.y = posY;
        rect.SetPositionAndRotation(newPos, rect.rotation);
    }
}
