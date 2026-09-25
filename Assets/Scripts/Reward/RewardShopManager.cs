using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardShopManager : MonoBehaviour
{
    [Header("Firebase")]
    public RewardFirebase firebase;

    [Header("Container")]
    public Transform rewardsContainer;

    [Header("Rewards")]
    public List<RewardPrefabManager> rewards =
        new List<RewardPrefabManager>();


    // =========================================================
    // DESTAQUE GRANDE DA LOJA
    // =========================================================

    [Header("Destaque da Loja")]
    [SerializeField]
    private FeaturedRewardUI featuredRewardUI;


    // =========================================================
    // DESTAQUE DO MENU
    // =========================================================

    [Header("Destaque no Menu")]
    [SerializeField]
    private TMP_Text menuFeaturedName;

    [SerializeField]
    private Image menuFeaturedImage;


    private RewardPrefabManager featuredReward;

    private const string FeaturedRewardKey =
        "FeaturedRewardId";


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        CreateRewards();
    }


    // =========================================================
    // CRIAR RECOMPENSAS
    // =========================================================

    private void CreateRewards()
    {
        string savedFeaturedId =
            PlayerPrefs.GetString(
                FeaturedRewardKey,
                ""
            );


        RewardPrefabManager rewardToRestore =
            null;


        foreach (
            RewardPrefabManager reward
            in rewards
        )
        {
            RewardPrefabManager item =
                Instantiate(
                    reward,
                    rewardsContainer
                );


            item.Setup(
                this
            );


            // Verifica se esse era o reward em destaque
            if (
                !string.IsNullOrEmpty(
                    savedFeaturedId
                ) &&
                item.rewardId ==
                savedFeaturedId
            )
            {
                rewardToRestore =
                    item;
            }
        }


        // Restaura o destaque depois de criar todos
        if (
            rewardToRestore !=
            null
        )
        {
            SetFeaturedReward(
                rewardToRestore,
                false
            );
        }
    }


    // =========================================================
    // COMPRAR RECOMPENSA
    // =========================================================

    public void BuyReward(
        RewardPrefabManager reward
    )
    {
        Debug.Log(
            "Tentando comprar: "
            + reward.rewardName
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


                    if (
                        featuredReward ==
                        reward
                    )
                    {
                        if (
                            featuredRewardUI !=
                            null
                        )
                        {
                            featuredRewardUI
                                .UpdatePurchasedState();
                        }
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
    // DEFINIR DESTAQUE
    // =========================================================

    public void SetFeaturedReward(
        RewardPrefabManager reward
    )
    {
        SetFeaturedReward(
            reward,
            true
        );
    }


    private void SetFeaturedReward(
        RewardPrefabManager reward,
        bool save
    )
    {
        if (reward == null)
        {
            return;
        }


        // Remove destaque anterior
        if (
            featuredReward !=
            null
        )
        {
            featuredReward
                .UpdateFeatured(
                    false
                );
        }


        // Define novo destaque
        featuredReward =
            reward;


        featuredReward
            .UpdateFeatured(
                true
            );


        // =====================================================
        // SALVAR DESTAQUE
        // =====================================================

        if (save)
        {
            PlayerPrefs.SetString(
                FeaturedRewardKey,
                reward.rewardId
            );

            PlayerPrefs.Save();
        }


        // =====================================================
        // DESTAQUE GRANDE DA LOJA
        // =====================================================

        if (
            featuredRewardUI !=
            null
        )
        {
            featuredRewardUI
                .ShowReward(
                    featuredReward,
                    this
                );
        }


        // =====================================================
        // MENU
        // =====================================================

        UpdateMenuFeatured();


        Debug.Log(
            "Recompensa em destaque: "
            + reward.rewardName
        );
    }


    // =========================================================
    // ATUALIZAR MENU
    // =========================================================

    private void UpdateMenuFeatured()
    {
        if (
            featuredReward ==
            null
        )
        {
            return;
        }


        // NOME
        if (
            menuFeaturedName !=
            null
        )
        {
            menuFeaturedName.text =
                featuredReward.rewardName;
        }


        // IMAGEM
        if (
            menuFeaturedImage !=
            null &&
            featuredReward.rewardImage !=
            null
        )
        {
            menuFeaturedImage.sprite =
                featuredReward
                    .rewardImage
                    .sprite;


            menuFeaturedImage.enabled =
                true;
        }
    }


    // =========================================================
    // PEGAR ID DO DESTAQUE
    // =========================================================

    public static string GetFeaturedRewardId()
    {
        return PlayerPrefs.GetString(
            FeaturedRewardKey,
            ""
        );
    }


    // =========================================================
    // REMOVER DESTAQUE SALVO
    // =========================================================

    public static void ClearFeaturedReward()
    {
        PlayerPrefs.DeleteKey(
            FeaturedRewardKey
        );

        PlayerPrefs.Save();
    }
}