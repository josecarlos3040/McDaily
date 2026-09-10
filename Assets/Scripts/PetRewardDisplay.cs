using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

#if !UNITY_WEBGL || UNITY_EDITOR
using Firebase.Auth;
using Firebase.Firestore;
#endif

public class PetRewardDisplay : MonoBehaviour
{
    [Serializable]
    public class RewardObject
    {
        public string rewardId;
        public GameObject rewardObject;
    }

    [Header("Recompensas do bichinho")]
    public List<RewardObject> rewards =
        new List<RewardObject>();

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

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        ListenToRewards();

#else

        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        ListenToRewards();

#endif
    }

    private void ListenToRewards()
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        FirebaseWeb_GetPurchasedRewards(gameObject.name);

#else

        if (auth.CurrentUser == null)
        {
            Debug.LogError("Nenhum usuário está logado.");
            return;
        }

        string userId = auth.CurrentUser.UserId;

        DocumentReference userDocument =
            db.Collection("users").Document(userId);

        userListener = userDocument.Listen(snapshot =>
        {
            if (!snapshot.Exists)
            {
                return;
            }

            if (!snapshot.ContainsField("purchasedRewards"))
            {
                DisableAllRewards();
                return;
            }

            List<string> purchasedRewards =
                snapshot.GetValue<List<string>>(
                    "purchasedRewards"
                );

            UpdatePet(purchasedRewards);
        });

#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR

    public void OnWebPurchasedRewards(string json)
    {
        Debug.Log(
            "Recompensas recebidas do Firebase Web: " +
            json
        );

        if (string.IsNullOrEmpty(json))
        {
            DisableAllRewards();
            return;
        }

        try
        {
            string[] purchasedRewards =
                JsonUtility.FromJson<RewardList>(
                    json
                ).rewards;

            UpdatePet(
                new List<string>(purchasedRewards)
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Erro ao ler recompensas: " +
                e.Message
            );
        }
    }

    [Serializable]
    private class RewardList
    {
        public string[] rewards;
    }

#endif

    private void UpdatePet(
        List<string> purchasedRewards)
    {
        DisableAllRewards();

        foreach (string rewardId in purchasedRewards)
        {
            foreach (RewardObject reward in rewards)
            {
                if (reward.rewardId == rewardId)
                {
                    if (reward.rewardObject != null)
                    {
                        reward.rewardObject.SetActive(true);
                    }

                    break;
                }
            }
        }
    }

    private void DisableAllRewards()
    {
        foreach (RewardObject reward in rewards)
        {
            if (reward.rewardObject != null)
            {
                reward.rewardObject.SetActive(false);
            }
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