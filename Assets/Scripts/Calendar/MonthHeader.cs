using System;
using TMPro;
using UnityEngine;

public class MonthHeader : MonoBehaviour
{
    public TMP_Text monthText;

    public void SetMonth(int year, int month)
    {
        DateTime date = new DateTime(year, month, 1);

        string monthName = date
            .ToString("MMMM")
            .ToUpper();

        monthText.text = monthName;
    }
}