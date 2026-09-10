using System;
using TMPro;
using UnityEngine;

public class DateDisplay : MonoBehaviour
{
    [Header("Textos")]
    public TMP_Text dateText;
    public TMP_Text dayText;

    private void Start()
    {
        DateTime today = DateTime.Now;

        // Exemplo: "3 de novembro"
        dateText.text =
            $"{today.Day} de {GetMonthName(today.Month)}";

        // Exemplo: "quinta-feira"
        dayText.text =
            GetDayName(today.DayOfWeek);
    }

    private string GetMonthName(int month)
    {
        string[] months =
        {
            "janeiro",
            "fevereiro",
            "março",
            "abril",
            "maio",
            "junho",
            "julho",
            "agosto",
            "setembro",
            "outubro",
            "novembro",
            "dezembro"
        };

        return months[month - 1];
    }

    private string GetDayName(DayOfWeek day)
    {
        switch (day)
        {
            case DayOfWeek.Monday:
                return "segunda-feira";

            case DayOfWeek.Tuesday:
                return "terça-feira";

            case DayOfWeek.Wednesday:
                return "quarta-feira";

            case DayOfWeek.Thursday:
                return "quinta-feira";

            case DayOfWeek.Friday:
                return "sexta-feira";

            case DayOfWeek.Saturday:
                return "sábado";

            case DayOfWeek.Sunday:
                return "domingo";

            default:
                return "";
        }
    }
}