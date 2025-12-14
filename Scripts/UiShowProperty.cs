using UnityEngine;

using TMPro; 
using UnityEngine.UI;

public class UiShowProperty : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player palyerReference;

    [Header("Buy Property UI")]
    [SerializeField] GameObject propertyUiPanel;
    [SerializeField] TMP_Text propertyNameText;
    [SerializeField] Image colorField;
    [Space]
    [SerializeField] TMP_Text rentPriceText; // WITHOUT A HOUSE
    [SerializeField] TMP_Text oneHouseRentText;
    [SerializeField] TMP_Text twoHouseRentText;
    [SerializeField] TMP_Text threeHouseRentText;
    [SerializeField] TMP_Text fourHouseRentText;
    [SerializeField] TMP_Text hotelRentText;
    [Space]
    [SerializeField] TMP_Text housePriceText;
    [SerializeField] TMP_Text hotelPriceText;
    [Space]
    [SerializeField] Button buyPropertyButton;
    [Space]
    [SerializeField] TMP_Text propertyPriceText;
    [SerializeField] TMP_Text playerMoneyText;

    void OnEnable()
    {
        MonopolyNode.OnShowPropertyBuyPanel += ShowBuyPropertyUi;
    }

    void OnDisable()
    {
        MonopolyNode.OnShowPropertyBuyPanel -= ShowBuyPropertyUi;
    }

    void Start()
    {
        propertyUiPanel.SetActive(false);
    }

    void ShowBuyPropertyUi(MonopolyNode node, Player currentPlayer)
    {
        nodeReference = node;
        palyerReference = currentPlayer;

        // top
        propertyNameText.text = node.name;
        colorField.color = node.propertyColorField.color;
        //-----
        rentPriceText.text = "£ " + node.baseRent;
        oneHouseRentText.text = "£ " + node.rentWithHouses[0];
        twoHouseRentText.text = "£ " + node.rentWithHouses[1];
        threeHouseRentText.text = "£ " + node.rentWithHouses[2];
        fourHouseRentText.text = "£ " + node.rentWithHouses[3];
        hotelRentText.text = "£ " + node.rentWithHouses[4];

        // how much it cost
        housePriceText.text = "£ " + node.houseCost;
        hotelPriceText.text = "£ " + node.houseCost;

        // bottom
        propertyPriceText.text = "Price: £ " + node.price;
        playerMoneyText.text = "You have: £ " + currentPlayer.ReadMoney;

        // Buy Property Button
        if (currentPlayer.CanAfford(node.price))
        {
            buyPropertyButton.interactable = true;
        }
        else
        {
            buyPropertyButton.interactable = false;
        }
        //show the corresponding UI
        propertyUiPanel.SetActive(true);
    }

    public void BuyPropertyButton()
    {
        palyerReference.BuyProperty(nodeReference);
        buyPropertyButton.interactable = false;
    }
    
    public void ClosePropertyButton()
    {
        propertyUiPanel.SetActive(false);
        nodeReference = null;
        palyerReference = null;
    }

}

