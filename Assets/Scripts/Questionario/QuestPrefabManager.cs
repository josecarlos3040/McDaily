using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestPrefabManager : MonoBehaviour
{
    [Header("Question")]
    public string questionId;
    public TMP_Text questionText;

    [Header("Answers")]
    public Toggle[] answers;

    public int GetAnswer()
    {
        for (int i = 0; i < answers.Length; i++)
        {
            if (answers[i].isOn)
            {
                return i + 1;
            }
        }

        return 0;
    }

    public bool IsAnswered()
    {
        return GetAnswer() != 0;
    }
}