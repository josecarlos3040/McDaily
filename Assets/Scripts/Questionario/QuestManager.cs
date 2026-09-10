using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [Header("Question Prefabs")]
    public QuestPrefabManager[] questionPrefabs;

    [Header("UI")]
    public Transform questionsContainer;
    public GameObject questionnairePanel;
    public GameObject alreadyAnsweredPanel;

    [Header("Settings")]
    public int questionsAmount = 5;

    [Header("Firebase")]
    public QuestFirebase firebase;

    private List<QuestPrefabManager> currentQuestions =
        new List<QuestPrefabManager>();


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        CheckTodayQuestionnaire();
    }


    // =========================================================
    // VERIFICAR QUESTIONÁRIO DE HOJE
    // =========================================================

    private void CheckTodayQuestionnaire()
    {
        firebase.HasAnsweredToday(
            alreadyAnswered =>
            {
                if (alreadyAnswered)
                {
                    ShowAlreadyAnswered();
                }
                else
                {
                    LoadQuestions();
                }
            }
        );
    }


    // =========================================================
    // MOSTRAR QUE JÁ RESPONDEU
    // =========================================================

    private void ShowAlreadyAnswered()
    {
        questionnairePanel.SetActive(false);

        alreadyAnsweredPanel.SetActive(true);
    }


    // =========================================================
    // CARREGAR PERGUNTAS
    // =========================================================

    private void LoadQuestions()
    {
        firebase.CreateDailyQuestionsIfNeeded(
            questionPrefabs,
            questionsAmount,
            questionIds =>
            {
                if (questionIds == null)
                {
                    Debug.LogError(
                        "Não foi possível carregar as perguntas."
                    );

                    return;
                }

                CreateQuestionPrefabs(questionIds);
            }
        );
    }


    // =========================================================
    // CRIAR OS PREFABS
    // =========================================================

    private void CreateQuestionPrefabs(List<string> questionIds)
    {
        ClearQuestions();

        Debug.Log("Quantidade de IDs recebidos: " + questionIds.Count);

        foreach (string questionId in questionIds)
        {
            Debug.Log("ID recebido: " + questionId);

            QuestPrefabManager prefab = FindQuestionPrefab(questionId);

            if (prefab == null)
            {
                Debug.LogError("Prefab não encontrado para: " + questionId);
                continue;
            }

            QuestPrefabManager question =
                Instantiate(prefab, questionsContainer);

            currentQuestions.Add(question);
            Debug.Log("CRIANDO PERGUNTA: " + questionId);
        }
    }


    // =========================================================
    // ENCONTRAR PREFAB PELO ID
    // =========================================================

    private QuestPrefabManager FindQuestionPrefab(
        string questionId)
    {
        foreach (QuestPrefabManager prefab in questionPrefabs)
        {
            if (prefab.questionId == questionId)
            {
                return prefab;
            }
        }

        return null;
    }


    // =========================================================
    // LIMPAR PERGUNTAS
    // =========================================================

    private void ClearQuestions()
    {
        foreach (Transform child in questionsContainer)
        {
            Destroy(child.gameObject);
        }

        currentQuestions.Clear();
    }


    // =========================================================
    // ENVIAR QUESTIONÁRIO
    // =========================================================

    public void SubmitQuestionnaire()
    {
        Dictionary<string, int> answers =
            new Dictionary<string, int>();


        // Verificar se todas foram respondidas

        foreach (QuestPrefabManager question
                 in currentQuestions)
        {
            if (!question.IsAnswered())
            {
                Debug.LogWarning(
                    "A pergunta " +
                    question.questionId +
                    " não foi respondida."
                );

                return;
            }
        }


        // Coletar respostas

        foreach (QuestPrefabManager question
                 in currentQuestions)
        {
            answers.Add(
                question.questionId,
                question.GetAnswer()
            );
        }


        // Enviar para Firebase

        firebase.SaveQuestionnaire(
            answers,
            success =>
            {
                if (success)
                {
                    Debug.Log(
                        "Questionário enviado!"
                    );

                    questionnairePanel.SetActive(false);

                    alreadyAnsweredPanel.SetActive(true);
                }
            }
        );
    }

    public void ReturnHome()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("InitialScreen");
    }
}