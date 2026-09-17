using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CalendarDayUI : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text dayText;

    public void Setup(
        int dayNumber,
        Color backgroundColor,
        Color textColor
    )
    {
        if (dayText != null)
        {
            dayText.text = dayNumber.ToString();
            dayText.color = textColor;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }
    }
}