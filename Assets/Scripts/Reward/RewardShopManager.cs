using System.Collections.Generic;
using UnityEngine;

public class RewardShopManager : MonoBehaviour
{
    [Header("Firebase")]
    public RewardFirebase firebase;

    [Header("Container")]
    public Transform rewardsContainer;

    [Header("Rewards")]
    public List<RewardPrefabManager> rewards =
        new List<RewardPrefabManager>();

    private RewardPrefabManager featuredReward;


    private void Start()
    {
        CreateRewards();
    }


    // =========================================================
    // CRIAR RECOMPENSAS
    // =========================================================

    private void CreateRewards()
    {
        foreach (RewardPrefabManager reward in rewards)
        {
            RewardPrefabManager item =
                Instantiate(
                    reward,
                    rewardsContainer
                );

            item.Setup(this);
        }
    }


    // =========================================================
    // COMPRAR
    // =========================================================

    public void BuyReward(
        RewardPrefabManager reward)
    {
        Debug.Log(
            "Tentando comprar: " +
            reward.rewardName
        );

        firebase.BuyReward(
            reward.rewardId,
            reward.price,
            success =>
            {
                if (success)
                {
                    Debug.Log(
                        "Compra realizada!"
                    );

                    reward.SetPurchased();
                }
                else
                {
                    Debug.LogWarning(
                        "Não foi possível comprar a recompensa."
                    );
                }
            }
        );
    }


    // =========================================================
    // DESTAQUE
    // =========================================================

    public void SetFeaturedReward(
        RewardPrefabManager reward)
    {
        if (featuredReward != null)
        {
            featuredReward.UpdateFeatured(false);
        }

        featuredReward = reward;

        featuredReward.UpdateFeatured(true);

        Debug.Log(
            "Recompensa em destaque: " +
            reward.rewardName
        );
    }
}