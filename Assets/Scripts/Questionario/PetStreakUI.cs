using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetStreakUI : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Image fireIcon;
    [SerializeField] private TMP_Text streakText;

    [Header("Cores")]
    [SerializeField] private Color zeroColor = Color.white;
    [SerializeField] private Color maxStreakColor = new Color(1f, 0.45f, 0f);

    [Header("Config")]
    [SerializeField] private int maxVisualStreak = 7;

    public void UpdateStreak(int streak)
    {
        if (streak < 0)
        {
            streak = 0;
        }

        if (streakText != null)
        {
            streakText.text = streak.ToString();
        }

        float t =
            Mathf.Clamp01(
                (float)streak / maxVisualStreak
            );

        Color currentColor =
            Color.Lerp(
                zeroColor,
                maxStreakColor,
                t
            );

        if (fireIcon != null)
        {
            fireIcon.color = currentColor;
        }

        if (streakText != null)
        {
            streakText.color = currentColor;
        }
    }
}