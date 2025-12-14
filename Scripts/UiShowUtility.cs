using UnityEngine;

using TMPro; 
using UnityEngine.UI;

public class UiShowUtility : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;

    [Header("Buy Utility UI")]
    [SerializeField] GameObject utilityUiPanel;
    [SerializeField] TMP_Text utilityNameText;
    [SerializeField] Image colorField;
    [Space]
    [SerializeField] Button buyUtilityButton;
    [Space]
    [SerializeField] TMP_Text propertyPriceText;
    [SerializeField] TMP_Text playerMoneyText;

    void OnEnable()
    {
        MonopolyNode.OnShowUtilityBuyPanel += ShowBuyUtilityPanel;
    }

    void OnDisable()
    {
        MonopolyNode.OnShowUtilityBuyPanel -= ShowBuyUtilityPanel;
    }

    void Start()
    {
        utilityUiPanel.SetActive(false);
    }

    void ShowBuyUtilityPanel(MonopolyNode node, Player currentPlayer)
    {
        nodeReference = node;
        playerReference = currentPlayer;

        // top
        utilityNameText.text = node.name;


        // bottom
        propertyPriceText.text = "Price: £ " + node.price;
        playerMoneyText.text = "You have: £ " + currentPlayer.ReadMoney;

        // Buy Property Button
        if (currentPlayer.CanAfford(node.price))
        {
            buyUtilityButton.interactable = true;
        }
        else
        {
            buyUtilityButton.interactable = false;
        }
        //show ui panel
        utilityUiPanel.SetActive(true);
    }

    public void BuyUtilityButton()
    {
        playerReference.BuyProperty(nodeReference);
        buyUtilityButton.interactable = false;
    }
    
    public void CloseUtilityButton()
    {
        utilityUiPanel.SetActive(false);
        nodeReference = null;
        playerReference = null;
    }

}
