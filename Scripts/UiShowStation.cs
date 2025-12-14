using UnityEngine;

using TMPro; 
using UnityEngine.UI;

public class UiShowStation : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;

    [Header("Buy Station UI")]
    [SerializeField] GameObject StationUiPanel;
    [SerializeField] TMP_Text StationNameText;
    [SerializeField] Image colorField;
    [Space]
    [SerializeField] TMP_Text oneStationRentText;
    [SerializeField] TMP_Text twoStationRentText;
    [SerializeField] TMP_Text threeStationRentText;
    [SerializeField] TMP_Text fourStationRentText;
    [Space]
    [SerializeField] Button buyStationButton;
    [Space]
    [SerializeField] TMP_Text propertyPriceText;
    [SerializeField] TMP_Text playerMoneyText;

    void OnEnable()
    {
        MonopolyNode.OnShowStationBuyPanel += ShowBuyStationPanelUI;
    }

    void OnDisable()
    {
        MonopolyNode.OnShowStationBuyPanel -= ShowBuyStationPanelUI;
    }

    void Start()
    {
        StationUiPanel.SetActive(false);
    }

    void ShowBuyStationPanelUI(MonopolyNode node, Player currentPlayer)
    {
        nodeReference = node;
        playerReference = currentPlayer;

        // top
        StationNameText.text = node.name;
        // ------
        twoStationRentText.text = "£ " + node.rentWithHouses[1];
        threeStationRentText.text = "£ " + node.rentWithHouses[2];
        fourStationRentText.text = "£ " + node.rentWithHouses[3];

        // bottom
        propertyPriceText.text = "Price: £ " + node.price;
        playerMoneyText.text = "You have: £ " + currentPlayer.ReadMoney;

        // Buy Property Button
        if (currentPlayer.CanAfford(node.price))
        {
            buyStationButton.interactable = true;
        }
        else
        {
            buyStationButton.interactable = false;
        }
        //show ui panel
        StationUiPanel.SetActive(true);
    }

    public void BuyStationButton()
    {
        playerReference.BuyProperty(nodeReference);
        buyStationButton.interactable = false;
    }
    
    public void CloseStationButton()
    {
        StationUiPanel.SetActive(false);
        nodeReference = null;
        playerReference = null;
    }

}
