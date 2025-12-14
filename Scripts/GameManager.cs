using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TMP_Text gameOverText;
    [SerializeField] MonopolyBoard gameBoard;
    [SerializeField] List<Player> playerList = new List<Player>();
    [SerializeField] int currentPlayer;
    [Header("Global Game Settings")]
    [SerializeField] int maxTurnsInJail = 2;//setting for how long in jail
    [SerializeField] int startMoney = 1500;//setting for how much money
    [SerializeField] int goMoney = 200;
    [SerializeField] float secondsBetweenTurns = 2;
    [Header("PlayerInfo")]
    [SerializeField] GameObject playerInfoPrefab;
    [SerializeField] Transform playerPanel;//for the playerInfo prefabs to become parented to
    [SerializeField] List<GameObject> tokenList = new List<GameObject>();

    //about the rolling dice
    int[] rolledDice;
    bool rolledDouble;
    public bool RolledADouble => rolledDouble;
    public void ResetRolledADouble() => rolledDouble = false;
    // fine pool also tax pool
    int finesPool = 0; 
    //how often do we actually have rolled a double
    int doubleRollCount;
    //pass over go to get the money
    public int GetGoMoney => goMoney;
    public float SecondsBetweenTurns => secondsBetweenTurns;
    public List<Player> GetPlayers => playerList;

    //message system
    public delegate void UpdateMessage(String message);
    public static UpdateMessage OnUpdateMessage;

    // human input panel
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn);
    public static ShowHumanPanel OnShowHumanPanel;
    
    private bool gameOver = false;

    // These for debugging
    [SerializeField] bool forceDiceRolls;
    //------------------------------------------------
    [SerializeField] int dice1;
    [SerializeField] int dice2;


    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        Initialized();
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            RollDice();
        }
        else
        {
            //show UI for human inputs
            //we don't have the specific UI for human to roll the dice, we just put the panel and button on the buttom fo the window
        }

    }
    void Initialized()
    {   
        List<GameObject> tempTokenList = new List<GameObject>(tokenList);
        for (int i = 0; i < playerList.Count; i++)
        {   
            GameObject infoObject = Instantiate(playerInfoPrefab, playerPanel, false);
            PlayerInfo info = infoObject.GetComponent<PlayerInfo>();
            //Random token
            int randomIndex = UnityEngine.Random.Range(0, tempTokenList.Count);
            //Instatiate
            GameObject newToken = Instantiate(tempTokenList[randomIndex], gameBoard.route[0].transform.position, Quaternion.identity);
            tempTokenList.RemoveAt(randomIndex);
            playerList[i].Initialize(gameBoard.route[0], startMoney, info, newToken);
        }

        if(playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
        {
            OnShowHumanPanel.Invoke(true, true, false);
        }
        else
        {
            OnShowHumanPanel.Invoke(false, false, false);
        }

    }
    public void RollDice() //press button from human inputs or auto from AI
    {   
        if(playerList[currentPlayer].ReadMoney <= 0)
        {
            playerList[currentPlayer].Bankrupt();
        }
        bool allowedToMove = true;
        //reset last roll
        rolledDice = new int[2];
        //roll dice randomly 1-6 and store them in variable
        rolledDice[0] = UnityEngine.Random.Range(1, 7);
        rolledDice[1] = UnityEngine.Random.Range(1, 7);
        Debug.Log("Rolled dice are: " + rolledDice[0] + "&" + rolledDice[1]);

        //debug
        if(forceDiceRolls)
        {
            rolledDice[0] = dice1;
            rolledDice[1] = dice2;
        }
        //check for double
        rolledDouble = rolledDice[0] == rolledDice[1];
        //throw 3 times in a row -> go to jail -> end turn

        //if is in jail already
        if (playerList[currentPlayer].IsInJail)
        {
            playerList[currentPlayer].IncreaseCountTurnsInJail();

            if (rolledDouble)
            {
                playerList[currentPlayer].OutOfJail();
                OnUpdateMessage.Invoke(playerList[currentPlayer].name + " <color=green>can leave jail</color>, because a double was rolled");
                doubleRollCount++;
                //move the player

            }
            else if (playerList[currentPlayer].NumTurnsInJail >= maxTurnsInJail)
            {
                //we have been long enough here 
                playerList[currentPlayer].OutOfJail();
                OnUpdateMessage.Invoke(playerList[currentPlayer].name + " <color=green>can leave jail</color>, because it is based on rules");
            }
            else
            {
                allowedToMove = false;
            }
        }
        else // not in jail
        {
            //reset double rolls follow requirements
            if (!rolledDouble)
            {
                doubleRollCount = 0;
            }
            else
            {
                doubleRollCount++;
                if (doubleRollCount >= 3)
                {
                    // move to jail
                    OnUpdateMessage.Invoke(playerList[currentPlayer].name + " has rolled <b>3 times a double</b>, and has to <b><color=red>go to jail!</color></b>");
                    int indexOnBoard = MonopolyBoard.instance.route.IndexOf(playerList[currentPlayer].MyMonopolyNode);
                    playerList[currentPlayer].GoToJail(indexOnBoard);
                    rolledDouble = false; // reset
                    SwitchPlayer();
                }
            }
        }
        //Can anyone in jail leave jail

        if (allowedToMove)
        {
            OnUpdateMessage.Invoke(playerList[currentPlayer].name + " has rolled" + rolledDice[0] + "&" + rolledDice[1]);
            StartCoroutine(DelayBeforeMove(rolledDice[0] + rolledDice[1]));
        }
        else
        {
            //maybe switch player or smth
            OnUpdateMessage.Invoke(playerList[currentPlayer].name + " has to stay in jail!");
            StartCoroutine(DelayBetweenSwitchPlayer());
        }

        //show or hide UI
        if(playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
        {
            OnShowHumanPanel.Invoke(true, false, true);
        }

        //check gameover for debug
        if(gameOver)
        {
            return; // end the game(not allowed to roll any dice)
        }
    }
    //function, need some time to show the act of dice rolling before player's move
    IEnumerator DelayBeforeMove(int rolledDice)
    {
        yield return new WaitForSeconds(secondsBetweenTurns);
        //if we are allowed to move we do so
        gameBoard.MovePlayerToken(rolledDice, playerList[currentPlayer]);
        //else we switch
    }

    IEnumerator DelayBetweenSwitchPlayer()
    {
        yield return new WaitForSeconds(secondsBetweenTurns);
        SwitchPlayer();
    }

    public void SwitchPlayer()
    {
        if(playerList[currentPlayer].ReadMoney <= 0)
        {
            playerList[currentPlayer].Bankrupt();
        }
        currentPlayer++;
        //check rolled double or not
        doubleRollCount = 0;

        //overflow check meanlessness
        if (currentPlayer >= playerList.Count)
        {
            currentPlayer = 0;
        }
        //is player AI
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            RollDice();
            OnShowHumanPanel.Invoke(false, false, false); // because it is AI player 
        }

        if(gameOver)
        {
            return; // for debug that gameover
        }

        //if it is human player - show the UI
        else
        {
            OnShowHumanPanel.Invoke(true, true, false);
        }
    }
    // record last dice 
    public int[] LastRolledDice()
    {
        return rolledDice;
    }
    //about tax paying
    public void AddTaxToPool(int amount)
    {
        finesPool += amount;
    }

    public int CollectFinesPool()
    {
        int temp = finesPool;
        finesPool = 0;
        return temp;
    }

    //!!!!!!!!!!!!GAME OVER PART
    public void RemovePlayer(Player player)
    {
        playerList.Remove(player);
        // check for game over
        CheckForGameOver();
    }

    void CheckForGameOver()
    {
        if (playerList.Count == 1)
        {
            // we have a winner then
            Debug.Log(playerList[0].name + " is the WINNER");
            //did not work
            OnUpdateMessage.Invoke(playerList[0].name + " is the WINNER"); // for the Invoke, smth need to be dubug
            gameOverPanel.SetActive(true);
            //work
            gameOverText.text = playerList[0].name + " is the WINNER!";
            gameOver = true;
        }
    }

}
