using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardPrefabManager : MonoBehaviour
{
    [Header("Reward")]
    public string rewardId;
    public string rewardName;
    public int price;

    [TextArea]
    public string description;

    [Header("UI")]
    public TMP_Text nameText;
    public TMP_Text priceText;

    public Image rewardImage;

    public Button buyButton;
    public Button featuredButton;

    public TMP_Text buyButtonText;

    [Header("Featured")]
    public GameObject featuredIndicator;


    private RewardShopManager shopManager;
    public bool IsPurchased { get; private set; }


    // =========================================================
    // SETUP
    // =========================================================

    public void Setup(
        RewardShopManager manager)
    {
        shopManager = manager;

        nameText.text =
            rewardName;

        priceText.text =
            price + " pontos";


        buyButton.onClick.RemoveAllListeners();

        buyButton.onClick.AddListener(
            Buy
        );


        featuredButton.onClick.RemoveAllListeners();

        featuredButton.onClick.AddListener(
            SetFeatured
        );


        UpdateFeatured(false);
    }


    // =========================================================
    // COMPRAR
    // =========================================================

    private void Buy()
    {
        if (shopManager == null)
            return;

        shopManager.BuyReward(this);
    }


    // =========================================================
    // DESTAQUE
    // =========================================================

    private void SetFeatured()
    {
        if (shopManager == null)
            return;

        shopManager.SetFeaturedReward(
            this
        );
    }


    // =========================================================
    // ATUALIZAR DESTAQUE
    // =========================================================

    public void UpdateFeatured(
        bool featured)
    {
        if (featuredIndicator != null)
        {
            featuredIndicator.SetActive(
                featured
            );
        }
    }


    // =========================================================
    // RECOMPENSA COMPRADA
    // =========================================================

    public void SetPurchased()
    {
        IsPurchased = true;

        buyButton.interactable = false;

        if (buyButtonText != null)
        {
            buyButtonText.text =
                "ADQUIRIDO";
        }
    }
}