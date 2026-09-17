using System;
using System.Runtime.InteropServices;
using UnityEngine;

#if !UNITY_WEBGL || UNITY_EDITOR
using Firebase.Auth;
using Firebase.Firestore;
#endif

public class PetStatusUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private PetCalendarUI calendarUI;
    [SerializeField] private PetStreakUI streakUI;


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


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Gera calendário
        if (calendarUI != null)
        {
            calendarUI.GenerateCalendar();
        }


#if UNITY_WEBGL && !UNITY_EDITOR

        // Pega dados reais do usuário no WebGL
        FirebaseWeb_GetUserInfo(
            gameObject.name
        );

#else

        auth =
            FirebaseAuth.DefaultInstance;

        db =
            FirebaseFirestore.DefaultInstance;


        ListenToUser();

#endif
    }


    // =========================================================
    // FIREBASE NORMAL / EDITOR
    // =========================================================

#if !UNITY_WEBGL || UNITY_EDITOR

    private void ListenToUser()
    {
        if (auth == null)
        {
            Debug.LogError(
                "Firebase Auth não inicializado."
            );

            return;
        }


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
            db
                .Collection("users")
                .Document(userId);


        userListener =
            userDocument.Listen(
                snapshot =>
                {
                    if (!snapshot.Exists)
                    {
                        return;
                    }


                    int streak = 0;


                    if (
                        snapshot.ContainsField(
                            "streak"
                        )
                    )
                    {
                        streak =
                            snapshot.GetValue<int>(
                                "streak"
                            );
                    }


                    UpdateStreak(
                        streak
                    );
                }
            );
    }

#endif


    // =========================================================
    // WEBGL
    // =========================================================

#if UNITY_WEBGL && !UNITY_EDITOR

    [Serializable]
    private class UserInfoData
    {
        public string username;

        public int streak;

        public int points;
    }


    public void OnWebUserInfo(
        string json
    )
    {
        Debug.Log(
            "Dados do usuário recebidos: "
            + json
        );


        if (string.IsNullOrEmpty(json))
        {
            UpdateStreak(0);

            return;
        }


        try
        {
            UserInfoData data =
                JsonUtility.FromJson<UserInfoData>(
                    json
                );


            if (data == null)
            {
                return;
            }


            UpdateStreak(
                data.streak
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Erro ao ler streak: "
                + e.Message
            );
        }
    }

#endif


    // =========================================================
    // ATUALIZAR STREAK
    // =========================================================

    private void UpdateStreak(
        int streak
    )
    {
        Debug.Log(
            "Streak real do usuário: "
            + streak
        );


        if (streakUI != null)
        {
            streakUI.UpdateStreak(
                streak
            );
        }
    }


    // =========================================================
    // DESTROY
    // =========================================================

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