using UnityEngine;

[CreateAssetMenu(fileName = "New PotluckCard Card", menuName = "Monopoly/Cards/Potluck")]
public class Script_PotluckCard : ScriptableObject
{   
    public Sprite cardImg;
    public string textOnCard; //Description
    public int rewardMoney;//get money
    public int penalityMoney;//pay money
    public int moveToBoardIndex = -1;//how we move to board indices
    public bool collectFromPlayer;
    [Header("Jail Content")]
    public bool goToJail;
    public bool jailFreeCard;
    [Header("Street Repairs")]
    public bool streetRepairs;
    public int streetRepairsHousePrice = 40;
    public int streetRepairsHotelPrice = 115;
}
