using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CalendarDay : MonoBehaviour
{
    public TMP_Text numberText;
    public Image background;
    public Button button;

    public Color normalColor = Color.white;
    public Color todayColor = new Color(1f, 0.4f, 0.25f);

    public void Setup(int day, bool isToday)
    {
        numberText.text = day.ToString();

        if (isToday)
        {
            background.color = todayColor;
            button.interactable = true;
        }
        else
        {
            background.color = normalColor;
            button.interactable = false;
        }
    }

    public void OnDayClicked()
    {
        SceneManager.LoadScene("Questionario");
    }


}