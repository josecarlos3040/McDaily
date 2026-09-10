using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

#if !UNITY_WEBGL || UNITY_EDITOR
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
#endif

public class QuestFirebase : MonoBehaviour
{
#if !UNITY_WEBGL || UNITY_EDITOR

    private FirebaseAuth auth;
    private FirebaseFirestore db;

#else

    [DllImport("__Internal")]
    private static extern void FirebaseWeb_SaveQuestionnaire(
        string answersJson,
        string gameObjectName
    );

    [DllImport("__Internal")]
    private static extern void FirebaseWeb_HasAnsweredToday(
        string gameObjectName
    );

    [DllImport("__Internal")]
    private static extern void FirebaseWeb_GetDailyQuestions(
        string gameObjectName
    );

    [DllImport("__Internal")]
    private static extern void FirebaseWeb_CreateDailyQuestions(
        string questionsJson,
        string gameObjectName
    );

    private Action<bool> saveCallback;
    private Action<bool> answeredCallback;
    private Action<List<string>> questionsCallback;

#endif

    // =========================================================
    // ESTRUTURA PARA CONVERTER RESPOSTAS EM JSON
    // =========================================================

    [Serializable]
    private class AnswerData
    {
        public string key;
        public int value;
    }

    [Serializable]
    private class AnswerList
    {
        public AnswerData[] answers;
    }

    [Serializable]
    private class QuestionList
    {
        public string[] questions;
    }


    private void Awake()
    {
#if !UNITY_WEBGL || UNITY_EDITOR

        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

#endif
    }


    // =========================================================
    // SALVAR AS RESPOSTAS + DAR PONTOS
    // =========================================================

    public void SaveQuestionnaire(
        Dictionary<string, int> answers,
        Action<bool> callback = null)
    {

#if UNITY_WEBGL && !UNITY_EDITOR

        saveCallback = callback;

        List<AnswerData> answerList =
            new List<AnswerData>();

        foreach (var answer in answers)
        {
            answerList.Add(
                new AnswerData
                {
                    key = answer.Key,
                    value = answer.Value
                }
            );
        }

        AnswerList wrapper =
            new AnswerList
            {
                answers = answerList.ToArray()
            };

        string json =
            JsonUtility.ToJson(wrapper);

        FirebaseWeb_SaveQuestionnaire(
            json,
            gameObject.name
        );

#else

        if (auth.CurrentUser == null)
        {
            Debug.LogError("Nenhum usuário está logado.");
            callback?.Invoke(false);
            return;
        }

        string userId =
            auth.CurrentUser.UserId;

        string date =
            DateTime.Now.ToString("yyyy-MM-dd");

        DocumentReference userDocument =
            db.Collection("users")
              .Document(userId);

        DocumentReference questionnaireDocument =
            userDocument
                .Collection("questionnaires")
                .Document(date);

        userDocument.GetSnapshotAsync()
            .ContinueWithOnMainThread(userTask =>
            {
                if (!userTask.IsCompletedSuccessfully)
                {
                    Debug.LogError(
                        "Erro ao buscar dados do usuário: " +
                        userTask.Exception
                    );

                    callback?.Invoke(false);
                    return;
                }

                DocumentSnapshot userSnapshot =
                    userTask.Result;

                int currentStreak = 0;
                string lastDate = "";

                if (userSnapshot.Exists)
                {
                    if (userSnapshot.ContainsField("streak"))
                    {
                        currentStreak =
                            userSnapshot.GetValue<int>("streak");
                    }

                    if (userSnapshot.ContainsField(
                        "lastQuestionnaireDate"))
                    {
                        lastDate =
                            userSnapshot.GetValue<string>(
                                "lastQuestionnaireDate"
                            );
                    }
                }

                DateTime today =
                    DateTime.Now.Date;

                DateTime lastQuestionnaireDate;

                bool isConsecutive = false;

                if (DateTime.TryParse(
                    lastDate,
                    out lastQuestionnaireDate))
                {
                    TimeSpan difference =
                        today -
                        lastQuestionnaireDate.Date;

                    if (difference.TotalDays == 1)
                    {
                        isConsecutive = true;
                    }
                }

                if (isConsecutive)
                {
                    currentStreak++;
                }
                else
                {
                    currentStreak = 1;
                }

                int pointsEarned =
                    Mathf.Min(
                        20 +
                        (currentStreak * 10),
                        100
                    );

                Dictionary<string, object>
                    questionnaireData =
                    new Dictionary<string, object>();

                foreach (var answer in answers)
                {
                    questionnaireData[answer.Key] =
                        answer.Value;
                }

                questionnaireData["timestamp"] =
                    Timestamp.GetCurrentTimestamp();

                questionnaireData["pointsEarned"] =
                    pointsEarned;

                Dictionary<string, object> userData =
                    new Dictionary<string, object>
                    {
                        {
                            "points",
                            FieldValue.Increment(
                                pointsEarned
                            )
                        },

                        {
                            "streak",
                            currentStreak
                        },

                        {
                            "lastQuestionnaireDate",
                            date
                        }
                    };

                questionnaireDocument
                    .SetAsync(questionnaireData)
                    .ContinueWithOnMainThread(
                        questionnaireTask =>
                        {
                            if (!questionnaireTask
                                .IsCompletedSuccessfully)
                            {
                                Debug.LogError(
                                    "Erro ao salvar questionário: " +
                                    questionnaireTask.Exception
                                );

                                callback?.Invoke(false);
                                return;
                            }

                            userDocument
                                .SetAsync(
                                    userData,
                                    SetOptions.MergeAll
                                )
                                .ContinueWithOnMainThread(
                                    userSaveTask =>
                                    {
                                        if (userSaveTask
                                        .IsCompletedSuccessfully)
                                        {
                                            Debug.Log(
                                            "Questionário salvo com sucesso!"
                                        );

                                            Debug.Log(
                                            "Pontos ganhos: " +
                                            pointsEarned
                                        );

                                            Debug.Log(
                                            "Streak: " +
                                            currentStreak
                                        );

                                            callback?.Invoke(true);
                                        }
                                        else
                                        {
                                            Debug.LogError(
                                            "Erro ao atualizar pontos: " +
                                            userSaveTask.Exception
                                        );

                                            callback?.Invoke(false);
                                        }
                                    }
                            );
                        }
                );
            });

#endif
    }


    // =========================================================
    // CALLBACK WEBGL - SALVAR QUESTIONÁRIO
    // =========================================================

#if UNITY_WEBGL && !UNITY_EDITOR

    public void OnWebSaveQuestionnaire(string result)
    {
        bool success =
            result == "success";

        Debug.Log(
            "Resultado questionário WebGL: " +
            result
        );

        saveCallback?.Invoke(success);

        saveCallback = null;
    }

#endif


    // =========================================================
    // VERIFICAR SE O USUÁRIO JÁ RESPONDEU HOJE
    // =========================================================

    public void HasAnsweredToday(
        Action<bool> callback)
    {

#if UNITY_WEBGL && !UNITY_EDITOR

        answeredCallback = callback;

        FirebaseWeb_HasAnsweredToday(
            gameObject.name
        );

#else

        if (auth.CurrentUser == null)
        {
            callback?.Invoke(false);
            return;
        }

        string userId =
            auth.CurrentUser.UserId;

        string date =
            DateTime.Now.ToString("yyyy-MM-dd");

        db.Collection("users")
            .Document(userId)
            .Collection("questionnaires")
            .Document(date)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    callback?.Invoke(
                        task.Result.Exists
                    );
                }
                else
                {
                    Debug.LogError(
                        "Erro ao verificar questionário: " +
                        task.Exception
                    );

                    callback?.Invoke(false);
                }
            });

#endif
    }


#if UNITY_WEBGL && !UNITY_EDITOR

    public void OnWebHasAnsweredToday(string result)
    {
        bool answered =
            result == "true";

        answeredCallback?.Invoke(answered);

        answeredCallback = null;
    }

#endif


    // =========================================================
    // BUSCAR AS PERGUNTAS DO DIA
    // =========================================================

    public void GetDailyQuestions(
        Action<List<string>> callback)
    {

#if UNITY_WEBGL && !UNITY_EDITOR

        questionsCallback = callback;

        FirebaseWeb_GetDailyQuestions(
            gameObject.name
        );

#else

        string date =
            DateTime.Now.ToString("yyyy-MM-dd");

        db.Collection("dailyQuestionnaires")
            .Document(date)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompletedSuccessfully)
                {
                    Debug.LogError(
                        "Erro ao buscar perguntas: " +
                        task.Exception
                    );

                    callback?.Invoke(null);
                    return;
                }

                DocumentSnapshot snapshot =
                    task.Result;

                if (!snapshot.Exists)
                {
                    Debug.LogWarning(
                        "Não existem perguntas cadastradas para hoje."
                    );

                    callback?.Invoke(null);
                    return;
                }

                List<string> questions =
                    snapshot.GetValue<List<string>>(
                        "questions"
                    );

                callback?.Invoke(questions);
            });

#endif
    }


#if UNITY_WEBGL && !UNITY_EDITOR

    public void OnWebDailyQuestions(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            questionsCallback?.Invoke(null);
            questionsCallback = null;
            return;
        }

        try
        {
            QuestionList data =
                JsonUtility.FromJson<QuestionList>(
                    json
                );

            List<string> questions =
                new List<string>(
                    data.questions
                );

            questionsCallback?.Invoke(
                questions
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Erro ao converter perguntas: " +
                e.Message
            );

            questionsCallback?.Invoke(null);
        }

        questionsCallback = null;
    }

#endif


    // =========================================================
    // CRIAR AS PERGUNTAS DO DIA
    // =========================================================

    public void CreateDailyQuestions(
        List<string> questionIds,
        Action<bool> callback = null)
    {

#if UNITY_WEBGL && !UNITY_EDITOR

        questionsCallback = null;

        string json =
            JsonUtility.ToJson(
                new QuestionList
                {
                    questions =
                        questionIds.ToArray()
                }
            );

        saveCallback = callback;

        FirebaseWeb_CreateDailyQuestions(
            json,
            gameObject.name
        );

#else

        string date =
            DateTime.Now.ToString("yyyy-MM-dd");

        Dictionary<string, object> data =
            new Dictionary<string, object>
            {
                {
                    "questions",
                    questionIds
                },

                {
                    "createdAt",
                    Timestamp.GetCurrentTimestamp()
                }
            };

        db.Collection("dailyQuestionnaires")
            .Document(date)
            .SetAsync(data)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log(
                        "Perguntas do dia criadas!"
                    );

                    callback?.Invoke(true);
                }
                else
                {
                    Debug.LogError(
                        "Erro ao criar perguntas: " +
                        task.Exception
                    );

                    callback?.Invoke(false);
                }
            });

#endif
    }


#if UNITY_WEBGL && !UNITY_EDITOR

    public void OnWebCreateDailyQuestions(
        string result)
    {
        bool success =
            result == "success";

        Debug.Log(
            "Criar perguntas WebGL: " +
            result
        );

        saveCallback?.Invoke(success);

        saveCallback = null;
    }

#endif


    // =========================================================
    // CRIAR PERGUNTAS SE NECESSÁRIO
    // =========================================================

    public void CreateDailyQuestionsIfNeeded(
        QuestPrefabManager[] availablePrefabs,
        int amount,
        Action<List<string>> callback)
    {
        GetDailyQuestions(existingQuestions =>
        {
            // Já existem perguntas
            if (existingQuestions != null &&
                existingQuestions.Count > 0)
            {
                Debug.Log(
                    "PERGUNTAS DO FIREBASE: " +
                    string.Join(
                        ", ",
                        existingQuestions
                    )
                );

                callback?.Invoke(
                    existingQuestions
                );

                return;
            }

            // Não existem → criar

            List<QuestPrefabManager> available =
                new List<QuestPrefabManager>(
                    availablePrefabs
                );

            List<string> selected =
                new List<string>();

            int seed =
                DateTime.Now.Year * 10000 +
                DateTime.Now.Month * 100 +
                DateTime.Now.Day;

            System.Random random =
                new System.Random(seed);

            for (
                int i = 0;
                i < amount &&
                available.Count > 0;
                i++
            )
            {
                int index =
                    random.Next(
                        available.Count
                    );

                selected.Add(
                    available[index].questionId
                );

                available.RemoveAt(index);
            }

            CreateDailyQuestions(
                selected,
                success =>
                {
                    if (success)
                    {
                        callback?.Invoke(
                            selected
                        );
                    }
                    else
                    {
                        callback?.Invoke(null);
                    }
                }
            );
        });
    }
}