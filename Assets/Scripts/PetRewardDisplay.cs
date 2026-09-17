using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

#if !UNITY_WEBGL || UNITY_EDITOR
using Firebase.Auth;
using Firebase.Firestore;
#endif

public class PetRewardDisplay : MonoBehaviour
{
    // =========================================================
    // RECOMPENSAS
    // =========================================================

    [Serializable]
    public class RewardObject
    {
        public string rewardId;
        public GameObject rewardObject;
    }


    [Header("Recompensas do bichinho")]
    public List<RewardObject> rewards =
        new List<RewardObject>();


    // =========================================================
    // HUMOR DO PET
    // =========================================================

    [Header("Humor do Pet")]
    [SerializeField] private Image petImage;

    [SerializeField] private Sprite sadSprite;
    [SerializeField] private Sprite neutralSprite;
    [SerializeField] private Sprite happySprite;


#if !UNITY_WEBGL || UNITY_EDITOR

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private ListenerRegistration userListener;

#else

    [DllImport("__Internal")]
    private static extern void FirebaseWeb_GetPurchasedRewards(
        string gameObjectName
    );

#endif


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        ListenToRewards();

#else

        auth =
            FirebaseAuth.DefaultInstance;

        db =
            FirebaseFirestore.DefaultInstance;

        ListenToRewards();

#endif
    }


    // =========================================================
    // FIREBASE
    // =========================================================

    private void ListenToRewards()
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        FirebaseWeb_GetPurchasedRewards(
            gameObject.name
        );

#else

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


                    // =========================================
                    // HUMOR
                    // =========================================

                    string lastQuestionnaireDate =
                        "";


                    if (
                        snapshot.ContainsField(
                            "lastQuestionnaireDate"
                        )
                    )
                    {
                        lastQuestionnaireDate =
                            snapshot.GetValue<string>(
                                "lastQuestionnaireDate"
                            );
                    }


                    UpdateMood(
                        lastQuestionnaireDate
                    );


                    // =========================================
                    // ROUPINHAS
                    // =========================================

                    if (
                        !snapshot.ContainsField(
                            "purchasedRewards"
                        )
                    )
                    {
                        DisableAllRewards();

                        return;
                    }


                    List<string> purchasedRewards =
                        snapshot.GetValue<List<string>>(
                            "purchasedRewards"
                        );


                    UpdatePet(
                        purchasedRewards
                    );
                }
            );

#endif
    }


    // =========================================================
    // WEBGL
    // =========================================================

#if UNITY_WEBGL && !UNITY_EDITOR

    public void OnWebPurchasedRewards(
        string json
    )
    {
        Debug.Log(
            "Dados do pet recebidos: "
            + json
        );


        if (string.IsNullOrEmpty(json))
        {
            DisableAllRewards();

            UpdateMood("");

            return;
        }


        try
        {
            RewardList data =
                JsonUtility.FromJson<RewardList>(
                    json
                );


            if (data == null)
            {
                return;
            }


            // HUMOR
            UpdateMood(
                data.lastQuestionnaireDate
            );


            // ROUPINHAS
            if (data.rewards == null)
            {
                DisableAllRewards();

                return;
            }


            UpdatePet(
                new List<string>(
                    data.rewards
                )
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Erro ao ler dados do pet: "
                + e.Message
            );
        }
    }


    [Serializable]
    private class RewardList
    {
        public string[] rewards;

        public string lastQuestionnaireDate;
    }

#endif


    // =========================================================
    // HUMOR
    // =========================================================

    private void UpdateMood(
        string lastQuestionnaireDate
    )
    {
        if (petImage == null)
        {
            return;
        }


        // =====================================================
        // NUNCA RESPONDEU
        // =====================================================

        if (
            string.IsNullOrEmpty(
                lastQuestionnaireDate
            )
        )
        {
            petImage.sprite =
                neutralSprite;

            Debug.Log(
                "Pet neutro: nenhum questionário respondido."
            );

            return;
        }


        // =====================================================
        // CONVERTER DATA
        // =====================================================

        DateTime lastDate;


        bool validDate =
            DateTime.TryParse(
                lastQuestionnaireDate,
                out lastDate
            );


        if (!validDate)
        {
            petImage.sprite =
                neutralSprite;

            Debug.LogWarning(
                "Data do último questionário inválida: "
                + lastQuestionnaireDate
            );

            return;
        }


        // =====================================================
        // QUANTOS DIAS DESDE O ÚLTIMO QUESTIONÁRIO
        // =====================================================

        DateTime today =
            DateTime.Today;


        int daysWithoutQuestionnaire =
            (today - lastDate.Date).Days;


        // =====================================================
        // MAIS DE 1 DIA SEM RESPONDER
        // =====================================================

        if (
            daysWithoutQuestionnaire > 1
        )
        {
            petImage.sprite =
                sadSprite;

            Debug.Log(
                "Pet triste. Dias sem questionário: "
                + daysWithoutQuestionnaire
            );

            return;
        }


        // =====================================================
        // RESPONDEU HOJE OU ONTEM
        // =====================================================

        petImage.sprite =
            happySprite;


        Debug.Log(
            "Pet feliz."
        );
    }


    // =========================================================
    // ATUALIZAR ROUPINHAS
    // =========================================================

    private void UpdatePet(
        List<string> purchasedRewards
    )
    {
        DisableAllRewards();


        foreach (
            string rewardId
            in purchasedRewards
        )
        {
            foreach (
                RewardObject reward
                in rewards
            )
            {
                if (
                    reward.rewardId ==
                    rewardId
                )
                {
                    if (
                        reward.rewardObject !=
                        null
                    )
                    {
                        reward
                            .rewardObject
                            .SetActive(
                                true
                            );
                    }


                    break;
                }
            }
        }
    }


    // =========================================================
    // DESATIVAR ROUPINHAS
    // =========================================================

    private void DisableAllRewards()
    {
        foreach (
            RewardObject reward
            in rewards
        )
        {
            if (
                reward.rewardObject !=
                null
            )
            {
                reward
                    .rewardObject
                    .SetActive(
                        false
                    );
            }
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