using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public enum MonopolyNodeType
{
    Property,
    Utility,
    Station,
    Tax,
    OpKnock,
    Potluck,
    Go,
    Jail,
    FreeParking,
    GoToJail
}
public class MonopolyNode : MonoBehaviour
{

    public MonopolyNodeType monopolyNodeType;
    public Image propertyColorField;
    [Header("Property Name")]
    [SerializeField] internal new string name;
    [SerializeField] TMP_Text nameText;
    [Header("Property Price")]
    public int price;
    [SerializeField] TMP_Text priceText;
    public int houseCost;
    [Header("Property Rent")]
    [SerializeField] bool calculateRentAuto;
    [SerializeField] int currentRent;
    [SerializeField] internal int baseRent;
    [SerializeField] internal List<int> rentWithHouses = new List<int>();
    int numberOfHouses;
    public int NumberOfHouses => numberOfHouses;
    [SerializeField] GameObject[] houses;
    [SerializeField] GameObject hotel;
    [Header("Property Mortgage")]
    [SerializeField] GameObject mortgageImage;
    [SerializeField] GameObject propertyImage;
    [SerializeField] bool isMortgaged;
    [SerializeField] int mortgageValue;
    [Header("Property Owner")]
    [SerializeField] GameObject ownerBar;
    [SerializeField] TMP_Text ownerText;
    Player owner;

    //message system
    public delegate void UpdateMessage(String message);
    public static UpdateMessage OnUpdateMessage;

    // human input panel
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn);
    public static ShowHumanPanel OnShowHumanPanel;

    // property buy panel
    public delegate void ShowPropertyBuyPanel(MonopolyNode node, Player player);
    public static ShowPropertyBuyPanel OnShowPropertyBuyPanel;

    // station buy panel
    public delegate void ShowStationBuyPanel(MonopolyNode node, Player player);
    public static ShowStationBuyPanel OnShowStationBuyPanel;

    // utility buy panel
    public delegate void ShowUtilityBuyPanel(MonopolyNode node, Player player);
    public static ShowUtilityBuyPanel OnShowUtilityBuyPanel;

    //drag a putlock
    public delegate void DrawPotLuckCard(Player player);
    public static DrawPotLuckCard OnDrawPotLuckCard;
    
    //drag a Op knocks
    public delegate void DrawOpKnockCard(Player player);
    public static DrawOpKnockCard OnDrawOpKnockCard;
    public Player Owner => owner;
    public void SetOwner(Player newOwner)
    {
        owner = newOwner;
        OnOwnerUpdated();
    }

    void OnValidate()
    {
        if (nameText != null)
        {
            nameText.text = name;
        }
        //calculation
        if (calculateRentAuto)
        {
            if (monopolyNodeType == MonopolyNodeType.Property)
            {
                if (baseRent > 0)
                {
                    price = 3 * (baseRent * 10);
                    //mortgage price
                    mortgageValue = price / 2;
                    rentWithHouses.Clear();
                    rentWithHouses.Add(baseRent * 5);
                    rentWithHouses.Add(baseRent * 5 * 3);
                    rentWithHouses.Add(baseRent * 5 * 9);
                    rentWithHouses.Add(baseRent * 5 * 16);
                    rentWithHouses.Add(baseRent * 5 * 25);

                }
                else if (baseRent <= 0)
                {
                    price = 0;
                    baseRent = 0;
                    rentWithHouses.Clear();
                    mortgageValue = 0;
                }
            }
            if (monopolyNodeType == MonopolyNodeType.Utility)
            {
                mortgageValue = price / 2;
            }
            if (monopolyNodeType == MonopolyNodeType.Station)
            {
                mortgageValue = price / 2;
            }

        }
        if (priceText != null)
        {
            priceText.text = "£" + price;
        }
        //update the owner
        OnOwnerUpdated();
        UnMortgageProperty();
        //isMortgaged = false;
    }

    public void UpdateColorField(Color color)
    {
        if (propertyColorField != null)
        {
            propertyColorField.color = color;
        }
    }

    //everything about mortgage
    public int MortgageProperty()
    {
        isMortgaged = true;
        if (mortgageImage != null)
        {
            mortgageImage.SetActive(true);
        }

        if (propertyImage != null)
        {
            propertyImage.SetActive(false);
        }
        return mortgageValue;
    }

    public void UnMortgageProperty()
    {
        isMortgaged = false;
        if (mortgageImage != null)
        {
            mortgageImage.SetActive(false);
        }

        if (propertyImage != null)
        {
            propertyImage.SetActive(true);
        }
    }
    public bool IsMortgaged => isMortgaged;
    public int MortgageValue => mortgageValue;
    //update owner
    public void OnOwnerUpdated()
    {
        if (ownerBar != null)
        {
            if (owner != null)
            {
                ownerBar.SetActive(true);
                ownerText.text = owner.name;
            }
            else
            {
                ownerBar.SetActive(false);
                ownerText.text = "";
            }
        }
    }
    public void PlayerLandedOnNode(Player currentPlayer)
    {
        bool playerIsHuman = currentPlayer.playerType == Player.PlayerType.HUMAN;
        bool continueTurn = true;
        //check for node type and act
        switch (monopolyNodeType)
        {
            case MonopolyNodeType.Property:
                if (!playerIsHuman)//AI
                {
                    //if it owned and if we are notowner and is not mortgaged
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        //pay rent to somebody

                        //calculate the rent
                        int rentToPay = CalculatePropertyRent();
                        //pay the actual rent to the owner
                        currentPlayer.PayRent(rentToPay, owner);

                        //show a message about what happened
                        OnUpdateMessage.Invoke(currentPlayer.name + "pays rent to " + rentToPay + " to " + owner.name);
                    }
                    else if (owner == null && currentPlayer.CanAfford(price) && currentPlayer.wentOverGo)
                    {
                        //buy the node
                        //Debug.Log("Player can buy");
                        OnUpdateMessage.Invoke(currentPlayer.name + "buys " + this.name);
                        currentPlayer.BuyProperty(this);
                        //OnOwnerUpdated();
                        //show a message about what happened
                    }
                    else
                    {
                        //is unowned and we cant afford it

                        //show a message about what happened
                    }
                }
                else//human
                {
                    //if it owned and if we are notowner and is not mortgaged
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        //pay rent to somebody

                        //calculate the rent
                        int rentToPay = CalculatePropertyRent();
                        //pay the actual rent to the owner
                        currentPlayer.PayRent(rentToPay, owner);

                        //pay the actual rent to the owner

                        //show a message about what happened
                    }
                    else if (owner == null && currentPlayer.wentOverGo)
                    {
                        //show buy interface for the property
                        OnShowPropertyBuyPanel.Invoke(this, currentPlayer);
                    }
                    else
                    {
                        //is unowned and we cant afford it
                    }
                }
                break;
            case MonopolyNodeType.Utility:
                if (!playerIsHuman)//AI
                {
                    //if it owned and if we are notowner and is not mortgaged
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        //pay rent to somebody

                        //calculate the rent

                        int rentToPay = CalculateUtilityRent();
                        currentRent = rentToPay;
                        //pay the actual rent to the owner
                        currentPlayer.PayRent(rentToPay, owner);

                        //show a message about what happened
                        OnUpdateMessage.Invoke(currentPlayer.name + "pays Utility rent of " + rentToPay + " to " + owner.name);
                    }
                    else if (owner == null && currentPlayer.CanAfford(price) && currentPlayer.wentOverGo)
                    {
                        //buy the node
                        //Debug.Log("Player can buy");
                        OnUpdateMessage.Invoke(currentPlayer.name + "buys " + this.name);
                        currentPlayer.BuyProperty(this);
                        OnOwnerUpdated();
                        //show a message about what happened
                    }
                    else
                    {
                        //is unowned and we cant afford it

                        //show a message about what happened
                    }
                }
                else//human
                {
                    //if it owned and if we are notowner and is not mortgaged
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        //calculate the rent
                        int rentToPay = CalculateUtilityRent();
                        currentRent = rentToPay;
                        //pay the actual rent to the owner
                        currentPlayer.PayRent(rentToPay, owner);


                        //show a message about what happened
                    }
                    else if (owner == null && currentPlayer.wentOverGo)
                    {
                        //show buy interface for the property
                        OnShowUtilityBuyPanel.Invoke(this, currentPlayer);
                    }
                    else
                    {
                        //is unowned and we cant afford it
                    }
                }
                break;
            case MonopolyNodeType.Station:
                if (!playerIsHuman)//AI
                {
                    //if it owned and if we are notowner and is not mortgaged
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        //pay rent to somebody

                        //calculate the rent

                        int rentToPay = CalculateStationRent();
                        currentRent = rentToPay;
                        //pay the actual rent to the owner
                        currentPlayer.PayRent(rentToPay, owner);

                        //show a message about what happened
                        OnUpdateMessage.Invoke(currentPlayer.name + "pays station rent of " + rentToPay + " to " + owner.name);
                    }
                    else if (owner == null && currentPlayer.CanAfford(price) && currentPlayer.wentOverGo)
                    {
                        //buy the node
                        //Debug.Log("Player can buy");
                        OnUpdateMessage.Invoke(currentPlayer.name + "buys " + this.name);
                        currentPlayer.BuyProperty(this);
                        OnOwnerUpdated();
                        //show a message about what happened
                    }
                    else
                    {
                        //is unowned and we cant afford it

                        //show a message about what happened
                    }
                }
                else//human
                {
                    //if it owned and if we are notowner and is not mortgaged
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        int rentToPay = CalculateStationRent();
                        currentRent = rentToPay;
                        //pay the actual rent to the owner
                        currentPlayer.PayRent(rentToPay, owner);     

                        //show a message about what happened
                    }
                    else if (owner == null && currentPlayer.wentOverGo)
                    {
                        //show buy interface for the property
                        OnShowStationBuyPanel.Invoke(this, currentPlayer);
                    }
                    else
                    {
                        //is unowned and we cant afford it
                    }
                }
                break;
            case MonopolyNodeType.Tax:
                GameManager.instance.AddTaxToPool(price);
                currentPlayer.PayMoney(price);
                //show a message about what happened
                OnUpdateMessage.Invoke(currentPlayer.name + " pays tax of: " + price);
                break;
            case MonopolyNodeType.FreeParking:
                int tax = GameManager.instance.CollectFinesPool();
                currentPlayer.CollectMoney(tax);
                //show a message about what happened
                OnUpdateMessage.Invoke(currentPlayer.name + " gets tax of: " + tax);
                break;
            case MonopolyNodeType.GoToJail:
                int indexOnBoard = MonopolyBoard.instance.route.IndexOf(currentPlayer.MyMonopolyNode);
                currentPlayer.GoToJail(indexOnBoard);
                //Bug:
                OnUpdateMessage.Invoke(currentPlayer.name + " has to go to jail");
                continueTurn = false;
                break;
            case MonopolyNodeType.OpKnock:
                OnDrawOpKnockCard.Invoke(currentPlayer);
                continueTurn = false;
                break;
            case MonopolyNodeType.Potluck:
                OnDrawPotLuckCard.Invoke(currentPlayer);
                continueTurn = false;
                break;
        }
        //stop here if needed
        if (!continueTurn )
        {
            return;
        }

        //continue
        if (!playerIsHuman)
        {
            Invoke("ContinueGame", GameManager.instance.SecondsBetweenTurns);
        }
        else
        {
            OnShowHumanPanel.Invoke(true, GameManager.instance.RolledADouble, true);
        }
    }

    void ContinueGame()
    {
        //if the last roll was a double roll

        if (GameManager.instance.RolledADouble)
        {
            //roll the dice again anyhow
            GameManager.instance.RollDice();
        }
        
        else
        {
            //not a double
            //switch the player
            GameManager.instance.SwitchPlayer();
        }

    }

    int CalculatePropertyRent()
    {
        switch (numberOfHouses)
        {
            case 0:
                //check if the owner has the full set of this nodes
                var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);

                if (allSame)
                {
                    currentRent = baseRent * 2;
                }
                else
                {
                    currentRent = baseRent;
                }
                break;
            case 1:
                currentRent = rentWithHouses[0];
                break;
            case 2:
                currentRent = rentWithHouses[1];
                break;
            case 3:
                currentRent = rentWithHouses[2];
                break;
            case 4:
                currentRent = rentWithHouses[3];
                break;
            case 5://hotel
                currentRent = rentWithHouses[4];
                break;
        }
        return currentRent;
    }

    int CalculateUtilityRent()
    {
        var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);
        if (allSame)
        {
            return baseRent * 2;
        }
                  
        return baseRent;
    }
    int CalculateStationRent()
    {
        return baseRent;
    }
    //manually
    void BuildHouseOnMap()
    {
        switch(numberOfHouses)
        {
            case 0:
            //unknown bug if we delete this
            break;
            case 1:
                houses[0].SetActive(true);
            break;
            case 2:
                houses[0].SetActive(true);
                houses[1].SetActive(true);
            break;
            case 3:
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(true);
            break;
            case 4:
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(true);
                houses[3].SetActive(true);
            break;
            case 5:
                hotel.SetActive(true);
            break;
        }
    }
    public void BuildHouseOrHotel()
    {
        if(monopolyNodeType == MonopolyNodeType.Property)
        {
            numberOfHouses++;
            BuildHouseOnMap();
        }
    }
    public int SellHouseOrHotel()
    {
        if(monopolyNodeType == MonopolyNodeType.Property)
        {
            numberOfHouses--;
            BuildHouseOnMap();
        }
        return houseCost;
    }
    public void ResetNode()
    {
        //if Ismorgaged
        if(isMortgaged)
        {
            propertyImage.SetActive(true);
            mortgageImage.SetActive(false);
            isMortgaged = false;
        }
        //Reset houses and hotel
        if(monopolyNodeType == MonopolyNodeType.Property)
        {
            numberOfHouses = 0;
            BuildHouseOnMap();
        }
        //Reset the owner
        owner.RemoveProperty(this);
        owner.name = "";
        //Update the UI
        OnOwnerUpdated();
    }
}
