using UnityEngine;

[CreateAssetMenu(fileName = "New Op Knock Card", menuName = "Monopoly/Cards/OpKnock")]
public class Script_OpKnockCard : ScriptableObject
{
    public Sprite cardImg;
    public string textOnCard; //Description
    public int rewardMoney;//get money
    public int penalityMoney;//pay money
    public int moveToBoardIndex = -1;//how we move to board indices
    public bool payToPlayer;
    [Header("MoveToLocations")]
    public bool nextStation;
    public bool nextUtility;
    public int moveStepsBackwards;
    [Header("Jail Content")]
    public bool goToJail;
    public bool jailFreeCard;// we did not implement it yet
    [Header("Street Repairs")]
    public bool streetRepairs;
    public int streetRepairsHousePrice = 25;
    public int streetRepairsHotelPrice = 100;
}
