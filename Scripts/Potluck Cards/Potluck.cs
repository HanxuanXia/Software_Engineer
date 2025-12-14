using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class Potluck : MonoBehaviour
{
    [SerializeField] List<Script_PotluckCard> cards = new List<Script_PotluckCard>();
    [SerializeField] Image cardImg;
    [SerializeField] GameObject cardHolderBackground;
    [SerializeField] float showTime = 3; // the time that card shows(hide card automatic after 3 secs)
    //[SerializeField] float moveDelay = 0.5f;// maybe not used later
    [SerializeField] Button closeCardButton;
    List<Script_PotluckCard> Deck = new List<Script_PotluckCard>();
    List<Script_PotluckCard> Graveyard = new List<Script_PotluckCard>();
    // current card and current player
    Script_PotluckCard pickedCard;
    Player currentPlayer;

    // human input panel
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn);
    public static ShowHumanPanel OnShowHumanPanel;

    void OnEnable()
    {
        MonopolyNode.OnDrawPotLuckCard += GetCard;
    }

    void OnDisable()
    {
        MonopolyNode.OnDrawPotLuckCard -= GetCard;
    }

    void Start()
    {
        cardHolderBackground.SetActive(false);
        // add all cards to the deck
        Deck.AddRange(cards);
        // shuffle
        Shuffle();
    }

    void Shuffle()
    {
        for (int i = 0; i < Deck.Count; i++)
        {
            int idx = Random.Range(0, Deck.Count);
            Script_PotluckCard tmpCard = Deck[idx];
            Deck[idx] = Deck[i];
            Deck[i] = tmpCard;
        }
    }

    void GetCard(Player cardTaker)
    {
        //get a card
        pickedCard = Deck[0];
        Deck.RemoveAt(0);
        Graveyard.Add(pickedCard);
        if(Deck.Count == 0)
        {
            // put back all cards
            Deck.AddRange(Graveyard);
            Graveyard.Clear();
            //shuffle all
            Shuffle();
        }
        //who is current player
        currentPlayer = cardTaker;
        // show card
        cardHolderBackground.SetActive(true);
        // fill in the text
        cardImg.sprite = pickedCard.cardImg;
        // deactivate button if it is an AI player
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
        if (pickedCard.rewardMoney != 0 && !pickedCard.collectFromPlayer)
        {
            currentPlayer.CollectMoney(pickedCard.rewardMoney);
        }
        else if (pickedCard.penalityMoney != 0)
        {
            currentPlayer.PayMoney(pickedCard.penalityMoney); // handle insuff
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
        else if (pickedCard.collectFromPlayer)
        {
            int totalCollected = 0;
            List<Player> players = GameManager.instance.GetPlayers;

            foreach (var player in players)
            {
                if (player != currentPlayer)
                {
                    //prevent bankrupcy
                    int amount = Mathf.Min(player.ReadMoney, pickedCard.rewardMoney);
                    player.PayMoney(amount);
                    totalCollected += amount;
                }
            }
            currentPlayer.CollectMoney(totalCollected);
        }
        else if (pickedCard.streetRepairs)
        {
            int[] allBuildings = currentPlayer.CountHousesAndHotels();
            int totalCosts = pickedCard.streetRepairsHousePrice * allBuildings[0] + pickedCard.streetRepairsHotelPrice * allBuildings[1];
            currentPlayer.PayMoney(totalCosts);
        }
        else if (pickedCard.goToJail)
        {
            isMoving = false; 
            currentPlayer.GoToJail(MonopolyBoard.instance.route.IndexOf(currentPlayer.MyMonopolyNode));
        }
        else if (pickedCard.jailFreeCard) // jail free cards , we don't implement it(to be implemented)
        {

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
            if(!isMoving)
            {
                OnShowHumanPanel.Invoke(true, GameManager.instance.RolledADouble, !GameManager.instance.RolledADouble);
            }
        }
    }

}
