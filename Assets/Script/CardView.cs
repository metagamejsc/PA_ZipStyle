using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [Header("UI")]
    public Image photo;
    public TMP_Text nameText;
    public TMP_Text ageLocationText;
    public TMP_Text distanceText;
    public TMP_Text tagText;
    public CanvasGroup infoGroup;

    public void Bind(CardData data)
    {
        if (data == null) return;

        if (photo != null) photo.sprite = data.photo;
        if (nameText != null) nameText.text = data.userName;
        if (ageLocationText != null) ageLocationText.text = data.ageLocation;
        if (distanceText != null) distanceText.text = data.distance;
        if (tagText != null) tagText.text = data.tag;

        if (infoGroup != null)
            infoGroup.alpha = 1f;
    }
}