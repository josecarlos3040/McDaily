using System;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

#if !UNITY_WEBGL || UNITY_EDITOR
using Firebase.Auth;
using Firebase.Firestore;
#endif

public class UserInfoDisplay : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text usernameText;
    public TMP_Text streakText;
    public TMP_Text pointsText;

#if !UNITY_WEBGL || UNITY_EDITOR

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private ListenerRegistration userListener;

#else

    [DllImport("__Internal")]
    private static extern void FirebaseWeb_GetUserInfo(
        string gameObjectName
    );

#endif

    [Serializable]
    private class UserInfoData
    {
        public string username;
        public int streak;
        public int points;
    }

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        FirebaseWeb_GetUserInfo(gameObject.name);

#else

        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        ListenToUserInfo();

#endif
    }

#if !UNITY_WEBGL || UNITY_EDITOR

    private void ListenToUserInfo()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError(
                "Nenhum usuário está logado."
            );

            return;
        }

        string userId =
            auth.CurrentUser.UserId;

        DocumentReference userDocument =
            db.Collection("users")
              .Document(userId);

        userListener =
            userDocument.Listen(snapshot =>
            {
                if (!snapshot.Exists)
                {
                    Debug.LogWarning(
                        "Documento do usuário não existe."
                    );

                    return;
                }

                UpdateUI(
                    snapshot.ContainsField("username")
                        ? snapshot.GetValue<string>("username")
                        : "",

                    snapshot.ContainsField("streak")
                        ? snapshot.GetValue<int>("streak")
                        : 0,

                    snapshot.ContainsField("points")
                        ? snapshot.GetValue<int>("points")
                        : 0
                );
            });
    }

#endif

#if UNITY_WEBGL && !UNITY_EDITOR

    public void OnWebUserInfo(string json)
    {
        Debug.Log(
            "Informações atualizadas: " + json
        );

        if (string.IsNullOrEmpty(json))
            return;

        try
        {
            UserInfoData data =
                JsonUtility.FromJson<UserInfoData>(
                    json
                );

            UpdateUI(
                data.username,
                data.streak,
                data.points
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Erro ao processar informações: " +
                e.Message
            );
        }
    }

#endif

    private void UpdateUI(
        string username,
        int streak,
        int points
    )
    {
        if (usernameText != null)
        {
            usernameText.text =
                username;
        }

        if (streakText != null)
        {
            streakText.text =
                streak + "!!";
        }

        if (pointsText != null)
        {
            pointsText.text =
                points + " pnts";
        }
    }

    private void OnDestroy()
    {
#if !UNITY_WEBGL || UNITY_EDITOR

        if (userListener != null)
        {
            userListener.Stop();
            userListener = null;
        }

#endif
    }
}