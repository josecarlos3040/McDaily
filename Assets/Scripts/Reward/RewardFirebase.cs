using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

#if !UNITY_WEBGL || UNITY_EDITOR
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
#endif

public class RewardFirebase : MonoBehaviour
{
#if !UNITY_WEBGL || UNITY_EDITOR

    private FirebaseAuth auth;
    private FirebaseFirestore db;

#else

    [DllImport("__Internal")]
    private static extern void FirebaseWeb_BuyReward(
        string rewardId,
        int price,
        string gameObjectName
    );

    private Action<bool> purchaseCallback;

#endif


    private void Awake()
    {
#if !UNITY_WEBGL || UNITY_EDITOR

        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

#endif
    }


    public void BuyReward(
        string rewardId,
        int price,
        Action<bool> callback = null)
    {

#if UNITY_WEBGL && !UNITY_EDITOR

        purchaseCallback = callback;

        FirebaseWeb_BuyReward(
            rewardId,
            price,
            gameObject.name
        );

#else

        if (auth.CurrentUser == null)
        {
            Debug.LogError(
                "Nenhum usuário está logado."
            );

            callback?.Invoke(false);
            return;
        }

        string userId =
            auth.CurrentUser.UserId;

        DocumentReference userDocument =
            db.Collection("users")
              .Document(userId);

        userDocument.GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompletedSuccessfully)
                {
                    Debug.LogError(
                        "Erro ao buscar usuário: " +
                        task.Exception
                    );

                    callback?.Invoke(false);
                    return;
                }

                DocumentSnapshot snapshot =
                    task.Result;

                if (!snapshot.Exists)
                {
                    Debug.LogError(
                        "Documento do usuário não existe."
                    );

                    callback?.Invoke(false);
                    return;
                }

                int points = 0;

                if (snapshot.ContainsField("points"))
                {
                    points =
                        snapshot.GetValue<int>(
                            "points"
                        );
                }

                List<string> purchasedRewards =
                    new List<string>();

                if (snapshot.ContainsField(
                    "purchasedRewards"))
                {
                    purchasedRewards =
                        snapshot.GetValue<List<string>>(
                            "purchasedRewards"
                        );
                }

                if (purchasedRewards.Contains(
                    rewardId))
                {
                    Debug.LogWarning(
                        "Essa recompensa já foi comprada."
                    );

                    callback?.Invoke(false);
                    return;
                }

                if (points < price)
                {
                    Debug.LogWarning(
                        "Pontos insuficientes."
                    );

                    callback?.Invoke(false);
                    return;
                }

                int newPoints =
                    points - price;

                purchasedRewards.Add(
                    rewardId
                );

                Dictionary<string, object> data =
                    new Dictionary<string, object>
                    {
                        {
                            "points",
                            newPoints
                        },

                        {
                            "purchasedRewards",
                            purchasedRewards
                        }
                    };

                userDocument.UpdateAsync(data)
                    .ContinueWithOnMainThread(
                        updateTask =>
                        {
                            if (updateTask
                                .IsCompletedSuccessfully)
                            {
                                Debug.Log(
                                    "Recompensa comprada com sucesso!"
                                );

                                callback?.Invoke(true);
                            }
                            else
                            {
                                Debug.LogError(
                                    "Erro ao salvar compra: " +
                                    updateTask.Exception
                                );

                                callback?.Invoke(false);
                            }
                        }
                );
            });

#endif
    }


#if UNITY_WEBGL && !UNITY_EDITOR

    public void OnWebBuyRewardResult(
        string result)
    {
        bool success =
            result == "success";

        Debug.Log(
            "Resultado compra WebGL: " +
            result
        );

        purchaseCallback?.Invoke(
            success
        );

        purchaseCallback = null;
    }

#endif
}