using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[System.Serializable]
public class Player
{
    //Define player's type
    public enum PlayerType
    {
        HUMAN,
        AI
    }
    public PlayerType playerType;
    public string name;
    int money;
    //record which node the player is currently at
    MonopolyNode currentNode;
    bool isInJail;
    public bool wentOverGo;
    

    int numTurnsInJail = 0;

    //store the player that can use into show the player in Unity
    [SerializeField] GameObject myToken;
    //store all properties that player owned
    [SerializeField] List<MonopolyNode> myMonopolyNodes = new List<MonopolyNode>();
    //playerINFO(it just knows how much cash the actual player currently has and what a name of current player is)

    PlayerInfo myInfo;

    //AI
    //the variable setting for high IQ AI but we don't implement it
    int aiMoneySavity = 200;

    //return some infos
    public bool IsInJail => isInJail;
    public GameObject MyToken => myToken;
    public MonopolyNode MyMonopolyNode => currentNode;
    public int ReadMoney => money;
    //message system
    public delegate void UpdateMessage(String message);
    public static UpdateMessage OnUpdateMessage;

    // human input panel
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn);
    public static ShowHumanPanel OnShowHumanPanel;

    //function to access the actual player script and pass over all the necessary content
    public void Initialize(MonopolyNode startNode, int startMoney, PlayerInfo info, GameObject token)
    {
        currentNode = startNode;
        money = startMoney;
        myInfo = info;
        myInfo.SetPlayerNameAndMoney(name, money);
        myToken = token;
    }
    public void SetMyCurrentNode(MonopolyNode newNode) // turn is over
    {
        currentNode = newNode;
        //player landed on node so let's 
        newNode.PlayerLandedOnNode(this);
        //if it is AI player
        if (playerType == PlayerType.AI)
        {
            CheckIfPlayerHasASet();

        }
        //check if can build houses
        CheckIfPlayerHasASet();
        //check for unmortgaged properties
        UnMortgageProperties();
        //check if he could trade for missing properties
    }
    public void CollectMoney(int amount)
    {
        money += amount;
        myInfo.SetPlayerMoney(money);
    }

    internal bool CanAfford(int price)
    {
        return price <= money;
    }

    public void BuyProperty(MonopolyNode node)
    {

        money -= node.price;
        node.SetOwner(this);
        // UpdateUI
        myInfo.SetPlayerMoney(money);
        // Set Ownership
        myMonopolyNodes.Add(node);
        // sort all nodes by price
        SortPropertiesByPrice();

    }

    void SortPropertiesByPrice()
    {
        myMonopolyNodes.OrderBy(_node => _node.price).ToList();
    }

    internal void PayRent(int amount, Player owner)
    {
        // Don`t have enough money
        if (money < amount)
        {
            if(playerType == PlayerType.AI)
            {
                // handle insufficient finds > AI
                HandleInsufficientFunds(amount);
            }
            else
            {
            // disable human turn and roll dice
            OnShowHumanPanel.Invoke(true, false, false);
            }
        }
        money -= amount;
        owner.CollectMoney(amount);
        //Update UI
        myInfo.SetPlayerMoney(money);
    }

    internal void PayMoney(int amount)
    {
        // Don`t have enough money
        if (money < amount)
        {
            if(playerType == PlayerType.AI)
            {
                // handle insufficient finds > AI
                HandleInsufficientFunds(amount);
            }
            else
            {
            // disable human turn and roll dice
            OnShowHumanPanel.Invoke(true, false, false);
            }
        }
        money -= amount;
        //Update UI
        myInfo.SetPlayerMoney(money);
    }
    //-----------------------------jail---------------------------------
    public void GoToJail(int indexOnBoard)
    {
        isInJail = true;
        //reposition player
        myToken.transform.position = MonopolyBoard.instance.route[10].transform.position;
        currentNode = MonopolyBoard.instance.route[10];
        //MonopolyBoard.instance.MovePlayerToken(CalculateDistanceFromJail(indexOnBoard), this);
        GameManager.instance.ResetRolledADouble();
    }

    public void OutOfJail()
    {
        isInJail = false;
        numTurnsInJail = 0;
    }


    public int NumTurnsInJail => numTurnsInJail;

    public void IncreaseCountTurnsInJail()
    {
        numTurnsInJail++;
    }
    //----------------------street repairs------------------------------------
    public int[] CountHousesAndHotels()
    {
        int houses = 0; // goes to index 0
        int hotels = 0; // goes to index 1

        foreach (var node in myMonopolyNodes)
        {
            if (node.NumberOfHouses != 5)
            {
                houses += node.NumberOfHouses;
            }
            else
            {
                hotels += 1;
            }
        }

        int[] allBuildings = new int[] { houses, hotels };
        return allBuildings;
    }
    //----------------------handle insufficient funds-------------------------
    void HandleInsufficientFunds(int amountToPay)
    {
        int housesToSell = 0; //  available houses to sell
        int allHouses = 0;
        int propertiesToMortgage = 0;
        int allPropertiesToMortgage = 0;

        // count all houses
        foreach (var node in myMonopolyNodes)
        {
            allHouses += node.NumberOfHouses;
        }

        //loop through the actual properties and try to sell as much as needed
        while (money < amountToPay && allHouses > 0)
        {
            foreach (var node in myMonopolyNodes)
            {
                housesToSell = node.NumberOfHouses;
                if (housesToSell > 0)
                {
                    CollectMoney(node.SellHouseOrHotel());
                    allHouses--;
                    // do we need more money?
                    if(money >= amountToPay)
                    {
                        return;
                    }
                }
            }
        }
        // mortgage
        foreach (var node in myMonopolyNodes)
        {
            allPropertiesToMortgage += (!node.IsMortgaged) ? 1 : 0;
        }
        //loop through the actual properties and try to sell as much as needed

        while (money < amountToPay && allPropertiesToMortgage > 0)
        {
            foreach (var node in myMonopolyNodes)
            {
                propertiesToMortgage += (!node.IsMortgaged) ? 1 : 0;
                if (propertiesToMortgage > 0)
                {
                    CollectMoney(node.MortgageProperty());
                    allPropertiesToMortgage--;
                    // do we need more money?
                    if(money >= amountToPay)
                    {
                        return;
                    }
                }
            }
        }
        // we go bankrupt if we reach
        Bankrupt();
    }
    //----------------------bankrupt game over-------------------------
    public void Bankrupt()
    {
        // take out the player of the game

        // send a message to message system
        OnUpdateMessage.Invoke(name + " is bankrupt");
        // clear all what the player has owned
        for (int i = myMonopolyNodes.Count - 1; i >= 0; i--)
        {
            myMonopolyNodes[i].ResetNode();
        }
        myToken.SetActive(false);
        myInfo.SetPlayerName("Bankrupt");
        // remove the player 
        GameManager.instance.RemovePlayer(this);
    }

    public void RemoveProperty(MonopolyNode node)
    {
        myMonopolyNodes.Remove(node);
    }

    void UnMortgageProperties()
    {
        // for AI only
        foreach (var node in myMonopolyNodes)
        {
            int cost = node.MortgageValue + (int)(node.MortgageValue * 0.1f); // 10% interest
            // can we affort to unmortgage
            if (money >= aiMoneySavity + cost) 
            {
                PayMoney(cost);
                node.UnMortgageProperty();
            }
        }
    }

    //----------------------check if player has a property set-------------------------
    void CheckIfPlayerHasASet()
    {
        List<MonopolyNode> processedSet = null;

        foreach (var node in myMonopolyNodes)
        {
            var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);
            if (!allSame)
            {
                continue;
            }
            List<MonopolyNode> nodeSet = list;
            if (nodeSet != null && nodeSet != processedSet)
            {
                bool hasMorgdagedNode = nodeSet.Any(node => node.IsMortgaged) ? true : false;
                if (!hasMorgdagedNode)
                {
                    if (nodeSet[0].monopolyNodeType == MonopolyNodeType.Property)
                    {
                        // WE COULD BUILD A HOUSE ON THE SET
                        BuildHouseOrHotelEvenly(nodeSet);
                        // update processed set over here
                        processedSet = nodeSet;
                    }
                }
            }
        }
    }
    //----------------------build houses evenly on node sets-------------------------
    void BuildHouseOrHotelEvenly(List<MonopolyNode> nodesToBuildOn)
    {
        int minHouses = int.MaxValue;
        int maxHouses = int.MinValue;

        // GET MIN AND MAX NUMBERS OF HOUSE CURRENTLY ON THE PROPERTYS
        foreach (var node in nodesToBuildOn)
        {
            int numOfHouses = node.NumberOfHouses;
            if (numOfHouses < minHouses)
            {
                minHouses = numOfHouses;
            }

            if (numOfHouses > maxHouses && numOfHouses < 5)
            {
                maxHouses = numOfHouses;
            }
        }

        // BUY HOUSES ON THE PROPERTIES FOR MYAX ALLOWED ON THE PROPERTIES
        foreach (var node in nodesToBuildOn)
        {
            if (node.NumberOfHouses == minHouses && node.NumberOfHouses < 5 && CanAffordHouse(node.houseCost))
            {
                node.BuildHouseOrHotel();
                PayMoney(node.houseCost);
                //stop the loop if it only shuld run once
                break;
            }
        }
    }

    bool CanAffordHouse(int price)
    {
        //AI only
        if (playerType == PlayerType.AI)
        {
            return (money - aiMoneySavity) >= price;
        }
        //Human only
        return money >= price;
    }
}
