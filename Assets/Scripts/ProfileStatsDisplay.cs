using System;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

#if !UNITY_WEBGL || UNITY_EDITOR
using Firebase.Auth;
using Firebase.Firestore;
#endif

public class ProfileStatsDisplay : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text streakText;
    [SerializeField] private TMP_Text pointsText;


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


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
#if !UNITY_WEBGL || UNITY_EDITOR

        auth =
            FirebaseAuth.DefaultInstance;

        db =
            FirebaseFirestore.DefaultInstance;

#endif
    }


    // =========================================================
    // SEMPRE QUE A TELA FOR ATIVADA
    // =========================================================

    private void OnEnable()
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        RefreshWebGL();

#else

        StartNativeListener();

#endif
    }


    // =========================================================
    // QUANDO A TELA FOR DESATIVADA
    // =========================================================

    private void OnDisable()
    {
#if !UNITY_WEBGL || UNITY_EDITOR

        StopNativeListener();

#endif
    }


    // =========================================================
    // WEBGL
    // =========================================================

#if UNITY_WEBGL && !UNITY_EDITOR

    private void RefreshWebGL()
    {
        Debug.Log(
            "Atualizando dados do perfil..."
        );

        FirebaseWeb_GetUserInfo(
            gameObject.name
        );
    }


    public void OnWebUserInfo(
        string json
    )
    {
        Debug.Log(
            "Dados recebidos no perfil: "
            + json
        );


        if (string.IsNullOrEmpty(json))
        {
            UpdateUI(
                0,
                0
            );

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
                UpdateUI(
                    0,
                    0
                );

                return;
            }


            UpdateUI(
                data.streak,
                data.points
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Erro ao ler dados do perfil: "
                + e.Message
            );


            UpdateUI(
                0,
                0
            );
        }
    }

#endif


    // =========================================================
    // FIREBASE NORMAL / EDITOR
    // =========================================================

#if !UNITY_WEBGL || UNITY_EDITOR

    private void StartNativeListener()
    {
        StopNativeListener();


        if (auth == null)
        {
            auth =
                FirebaseAuth.DefaultInstance;
        }


        if (db == null)
        {
            db =
                FirebaseFirestore.DefaultInstance;
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
                    int points = 0;


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


                    if (
                        snapshot.ContainsField(
                            "points"
                        )
                    )
                    {
                        points =
                            snapshot.GetValue<int>(
                                "points"
                            );
                    }


                    UpdateUI(
                        streak,
                        points
                    );
                }
            );
    }


    private void StopNativeListener()
    {
        if (userListener != null)
        {
            userListener.Stop();

            userListener =
                null;
        }
    }

#endif


    // =========================================================
    // ATUALIZAR UI
    // =========================================================

    private void UpdateUI(
        int streak,
        int points
    )
    {
        Debug.Log(
            "Atualizando perfil | Streak: "
            + streak
            + " | Pontos: "
            + points
        );


        if (streakText != null)
        {
            if (streak == 1)
            {
                streakText.text =
                    "Sequência: 1 dia";
            }
            else
            {
                streakText.text =
                    "Sequência: "
                    + streak
                    + " dias";
            }
        }


        if (pointsText != null)
        {
            if (points == 1)
            {
                pointsText.text =
                    "1 Ponto";
            }
            else
            {
                pointsText.text =
                    points
                    + " Pontos";
            }
        }
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
#if !UNITY_WEBGL || UNITY_EDITOR

        StopNativeListener();

#endif
    }
}