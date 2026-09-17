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

    [Header("Destaque")]
    [SerializeField] private FeaturedRewardUI featuredRewardUI;

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
        RewardPrefabManager reward
    )
    {
        if (reward == null)
        {
            return;
        }

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

                    // Se o item comprado também for
                    // o item grande em destaque,
                    // atualiza o botão dele.
                    if (
                        featuredReward == reward &&
                        featuredRewardUI != null
                    )
                    {
                        featuredRewardUI
                            .UpdatePurchasedState();
                    }
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
        RewardPrefabManager reward
    )
    {
        if (reward == null)
        {
            return;
        }


        // Remove o destaque do item anterior
        if (featuredReward != null)
        {
            featuredReward.UpdateFeatured(
                false
            );
        }


        // Novo item em destaque
        featuredReward = reward;


        // Liga o indicador do card pequeno
        featuredReward.UpdateFeatured(
            true
        );


        // Atualiza o card grande de cima
        if (featuredRewardUI != null)
        {
            featuredRewardUI.ShowReward(
                featuredReward,
                this
            );
        }


        Debug.Log(
            "Recompensa em destaque: " +
            reward.rewardName
        );
    }
}