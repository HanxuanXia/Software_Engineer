using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
public class OpKnockField : MonoBehaviour
{
    [SerializeField] List<Script_OpKnockCard> cards = new List<Script_OpKnockCard>();
    [SerializeField] Image cardImg;
    [SerializeField] GameObject cardHolderBackground;
    // the time that card shows(hide card automatically after 3 secs)
    // if it is AI player
    [SerializeField] float showTime = 3;
    [SerializeField] Button closeCardButton;
    List<Script_OpKnockCard> Deck = new List<Script_OpKnockCard>();
    List<Script_OpKnockCard> Graveyard = new List<Script_OpKnockCard>();
    // current card and current player
    Script_OpKnockCard pickedCard;
    Player currentPlayer;

    // human input panel
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn);
    public static ShowHumanPanel OnShowHumanPanel;

    void OnEnable()
    {
        MonopolyNode.OnDrawOpKnockCard += GetCard;
    }

    void OnDisable()
    {
        MonopolyNode.OnDrawOpKnockCard -= GetCard;
    }

    void Start()
    {
        cardHolderBackground.SetActive(false);
        // add all cards to the deck
        Deck.AddRange(cards);
        // shuffle these cards
        Shuffle();
    }

    void Shuffle()
    {
        for (int i = 0; i < Deck.Count; i++)
        {
            int idx = Random.Range(0, Deck.Count);
            Script_OpKnockCard tmpCard = Deck[idx];
            Deck[idx] = Deck[i];
            Deck[i] = tmpCard;
        }
    }

    void GetCard(Player cardUser)
    {
        //get a card
        pickedCard = Deck[0];
        Deck.RemoveAt(0);
        Graveyard.Add(pickedCard);
        if (Deck.Count == 0)
        {
            // return all cards
            Deck.AddRange(Graveyard);
            Graveyard.Clear();
            //shuffle all
            Shuffle();
        }
        //the current player
        currentPlayer = cardUser;
        // showing card
        cardHolderBackground.SetActive(true);
        // fill in the text
        //cardText.text = pickedCard.textOnCard;
        cardImg.sprite = pickedCard.cardImg;
        //cardText.gameObject.SetActive(false);
        // deactivate button if it is AI player
        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            closeCardButton.interactable = false;
            Invoke("ApplyCardEffect", showTime);
        }
        else
        {
            closeCardButton.interactable = true;
        }

    }
    public void ApplyCardEffect() // call the function when close button of the card
    {
        bool isMoving = false;
        if (pickedCard.rewardMoney != 0)
        {
            currentPlayer.CollectMoney(pickedCard.rewardMoney);
        }
        else if (pickedCard.penalityMoney != 0 && !pickedCard.payToPlayer)
        {
            currentPlayer.PayMoney(pickedCard.penalityMoney); // handle insuff funds
        }
        else if (pickedCard.moveToBoardIndex != -1)
        {
            isMoving = true;
            // steps to goal
            int currentIndex = MonopolyBoard.instance.route.IndexOf(currentPlayer.MyMonopolyNode);
            int lengthOfBoard = MonopolyBoard.instance.route.Count;
            int stepsToMove = 0;
            if (currentIndex < pickedCard.moveToBoardIndex)
            {
                stepsToMove = pickedCard.moveToBoardIndex - currentIndex;
            }
            else if (currentIndex > pickedCard.moveToBoardIndex)
            {
                stepsToMove = lengthOfBoard - currentIndex + pickedCard.moveToBoardIndex;
            }

            // start the move

            MonopolyBoard.instance.MovePlayerToken(stepsToMove, currentPlayer);

        }
        else if (pickedCard.payToPlayer)
        {
            int totalCollected = 0;
            List<Player> players = GameManager.instance.GetPlayers;

            foreach (var player in players)
            {
                if (player != currentPlayer)
                {
                    //prevent bankrupcy
                    int amount = Mathf.Min(currentPlayer.ReadMoney, pickedCard.penalityMoney);
                    player.CollectMoney(amount);
                    totalCollected += amount;
                }
            }
            currentPlayer.PayMoney(totalCollected);
        }
        else if (pickedCard.streetRepairs)
        {
            int[] allBuildings = currentPlayer.CountHousesAndHotels();
            int totalCosts = pickedCard.streetRepairsHousePrice * allBuildings[0] + pickedCard.streetRepairsHotelPrice * allBuildings[1];
            currentPlayer.PayMoney(totalCosts);
        }
        else if (pickedCard.goToJail)
        {
            isMoving = false; // because we don't have a effect to go to the jail, we did not implement it
            currentPlayer.GoToJail(MonopolyBoard.instance.route.IndexOf(currentPlayer.MyMonopolyNode));
        }
        else if (pickedCard.jailFreeCard) // jail free cards, neither
        {

        }
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //be care of these 3 isMoving = true, there are some potential bugs
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        else if (pickedCard.moveStepsBackwards != 0)
        {
            int steps = Mathf.Abs(pickedCard.moveStepsBackwards);
            MonopolyBoard.instance.MovePlayerToken(-steps, currentPlayer);
            isMoving = true;

        }
        else if (pickedCard.nextStation)
        {
            MonopolyBoard.instance.MovePlayerToken(MonopolyNodeType.Station, currentPlayer);
            isMoving = true;
        }
        else if (pickedCard.nextUtility)
        {
            MonopolyBoard.instance.MovePlayerToken(MonopolyNodeType.Utility, currentPlayer);
            isMoving = true;
        }
        cardHolderBackground.SetActive(false);
        ContinueGame(isMoving);
    }

    void ContinueGame(bool isMoving)
    {
        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            if (!isMoving && GameManager.instance.RolledADouble)
            {
                GameManager.instance.RollDice();
            }
            else if (!isMoving && !GameManager.instance.RolledADouble)
            {
                GameManager.instance.SwitchPlayer();
            }
        }
        else // human inputs
        {
            if (!isMoving)
            {
                OnShowHumanPanel.Invoke(true, GameManager.instance.RolledADouble, !GameManager.instance.RolledADouble);
            }
        }
    }
}
