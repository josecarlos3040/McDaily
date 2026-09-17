using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FeaturedRewardUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image rewardImage;

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text descriptionText;

    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text buyButtonText;


    private RewardPrefabManager currentReward;
    private RewardShopManager shopManager;


    // =========================================================
    // MOSTRAR RECOMPENSA
    // =========================================================

    public void ShowReward(
        RewardPrefabManager reward,
        RewardShopManager manager
    )
    {
        if (reward == null)
        {
            return;
        }

        currentReward = reward;
        shopManager = manager;


        // NOME
        if (nameText != null)
        {
            nameText.text =
                reward.rewardName;
        }


        // PREÇO
        if (priceText != null)
        {
            priceText.text =
                reward.price + " pontos";
        }


        // DESCRIÇÃO
        if (descriptionText != null)
        {
            descriptionText.text =
                reward.description;
        }


        // IMAGEM
        if (
            rewardImage != null &&
            reward.rewardImage != null
        )
        {
            rewardImage.sprite =
                reward.rewardImage.sprite;

            rewardImage.preserveAspect =
                true;
        }


        // BOTÃO
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();

            buyButton.onClick.AddListener(
                BuyFeaturedReward
            );
        }


        UpdatePurchasedState();
    }


    // =========================================================
    // COMPRAR ITEM EM DESTAQUE
    // =========================================================

    private void BuyFeaturedReward()
    {
        if (currentReward == null)
        {
            return;
        }

        if (shopManager == null)
        {
            return;
        }

        shopManager.BuyReward(
            currentReward
        );
    }


    // =========================================================
    // ESTADO DO BOTÃO
    // =========================================================

    public void UpdatePurchasedState()
    {
        if (currentReward == null)
        {
            return;
        }


        if (currentReward.IsPurchased)
        {
            if (buyButton != null)
            {
                buyButton.interactable =
                    false;
            }


            if (buyButtonText != null)
            {
                buyButtonText.text =
                    "ADQUIRIDO";
            }
        }
        else
        {
            if (buyButton != null)
            {
                buyButton.interactable =
                    true;
            }


            if (buyButtonText != null)
            {
                buyButtonText.text =
                    "Comprar";
            }
        }
    }
}