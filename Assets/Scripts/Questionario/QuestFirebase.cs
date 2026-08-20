using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestFirebase : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseFirestore db;

    private void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }


    // SALVAR AS RESPOSTAS DO USUÁRIO


    public void SaveQuestionnaire(
        Dictionary<string, int> answers,
        Action<bool> callback = null)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("Nenhum usuário está logado.");
            callback?.Invoke(false);
            return;
        }

        string userId = auth.CurrentUser.UserId;

        string date = DateTime.Now.ToString("yyyy-MM-dd");

        Dictionary<string, object> data =
            new Dictionary<string, object>();

        foreach (var answer in answers)
        {
            data[answer.Key] = answer.Value;
        }

        data["timestamp"] =
            Timestamp.GetCurrentTimestamp();

        db.Collection("users")
            .Document(userId)
            .Collection("questionnaires")
            .Document(date)
            .SetAsync(data)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log(
                        "Questionário salvo com sucesso!"
                    );

                    callback?.Invoke(true);
                }
                else
                {
                    Debug.LogError(
                        "Erro ao salvar questionário: " +
                        task.Exception
                    );

                    callback?.Invoke(false);
                }
            });
    }



    // VERIFICAR SE O USUÁRIO JÁ RESPONDEU HOJE


    public void HasAnsweredToday(
        Action<bool> callback)
    {
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
    }


    // BUSCAR AS PERGUNTAS DO DIA

    public void GetDailyQuestions(
        Action<List<string>> callback)
    {
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
    }

    // CRIAR AS PERGUNTAS DO DIA


    public void CreateDailyQuestions(
        List<string> questionIds,
        Action<bool> callback = null)
    {
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
    }

    public void CreateDailyQuestionsIfNeeded(
    QuestPrefabManager[] availablePrefabs,
    int amount,
    System.Action<List<string>> callback)
    {
        string date =
            DateTime.Now.ToString("yyyy-MM-dd");

        DocumentReference document =
            db.Collection("dailyQuestionnaires")
              .Document(date);

        document.GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompletedSuccessfully)
                {
                    Debug.LogError(
                        "Erro ao verificar perguntas do dia: " +
                        task.Exception
                    );

                    callback?.Invoke(null);
                    return;
                }

                DocumentSnapshot snapshot =
                    task.Result;


                // Já existem perguntas

                if (snapshot.Exists)
                {
                    List<string> existingQuestions =
                        snapshot.GetValue<List<string>>("questions");

                    Debug.Log(
                        "PERGUNTAS DO FIREBASE: " +
                        string.Join(", ", existingQuestions)
                    );

                    callback?.Invoke(existingQuestions);

                    return;
                }


                // Criar novas perguntas

                List<QuestPrefabManager> available =
                    new List<QuestPrefabManager>(
                        availablePrefabs
                    );

                List<string> selected =
                    new List<string>();


                // Seed baseado na data

                int seed =
                    DateTime.Now.Year * 10000 +
                    DateTime.Now.Month * 100 +
                    DateTime.Now.Day;

                System.Random random =
                    new System.Random(seed);


                for (int i = 0;
                     i < amount && available.Count > 0;
                     i++)
                {
                    int index =
                        random.Next(available.Count);

                    selected.Add(
                        available[index].questionId
                    );

                    available.RemoveAt(index);
                }


                Dictionary<string, object> data =
                    new Dictionary<string, object>
                    {
                    {
                        "questions",
                        selected
                    },

                    {
                        "createdAt",
                        Timestamp.GetCurrentTimestamp()
                    }
                    };


                document.SetAsync(data)
                    .ContinueWithOnMainThread(saveTask =>
                    {
                        if (saveTask.IsCompletedSuccessfully)
                        {
                            Debug.Log(
                                "Perguntas do dia criadas."
                            );

                            callback?.Invoke(selected);
                        }
                        else
                        {
                            Debug.LogError(
                                "Erro ao criar perguntas: " +
                                saveTask.Exception
                            );

                            callback?.Invoke(null);
                        }
                    });
            });
    }
}