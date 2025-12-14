using UnityEngine;

using TMPro;
using System;
public class MessageSystem : MonoBehaviour
{
    [SerializeField] TMP_Text messageText;

    void OnEnable()
    {
        ClearMessage();
        GameManager.OnUpdateMessage += ReceiveMessage;
        Player.OnUpdateMessage += ReceiveMessage;
        MonopolyNode.OnUpdateMessage += ReceiveMessage;
    }

    void OnDisable()
    {
        GameManager.OnUpdateMessage -= ReceiveMessage;
        Player.OnUpdateMessage -= ReceiveMessage;
        MonopolyNode.OnUpdateMessage -= ReceiveMessage;
    }

    void ReceiveMessage(String _message)
    {
        messageText.text = _message;
    }

    void ClearMessage()
    {
        messageText.text = "";
    }
}
