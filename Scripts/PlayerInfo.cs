using UnityEngine;
using TMPro;
public class PlayerInfo : MonoBehaviour
{
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] TMP_Text playerMoneyText;

    public void SetPlayerName(string newName)
    {
        playerNameText.text = newName;
    }
    public void SetPlayerMoney(int currentMoney)
    {
        playerMoneyText.text = "£" + currentMoney;
    }

    public void SetPlayerNameAndMoney(string newName, int currentMoney) 
    {
        SetPlayerName(newName);
        SetPlayerMoney(currentMoney);
    }
}