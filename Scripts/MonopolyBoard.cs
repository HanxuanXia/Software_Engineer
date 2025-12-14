using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MonopolyBoard : MonoBehaviour
{

    public static MonopolyBoard instance;

    public List<MonopolyNode> route = new List<MonopolyNode>();
    
    [System.Serializable]
    public class NodeSet
    {
        public Color setColor = Color.white;
        public List<MonopolyNode> nodesInSetList = new List<MonopolyNode>();
    }
    
    public List<NodeSet> nodeSetList = new List<NodeSet>();

    void Awake()
    {
        instance = this;
    }
    //auto-filled 
    void OnValidate()
    {
        route.Clear();
        foreach (Transform node in transform.GetComponentInChildren<Transform>()) 
        {
            MonopolyNode mn = node.GetComponent<MonopolyNode>();
            if (mn != null)
            {
                route.Add(mn);
            }
        }
    }
    /*
        params:
        steps: how many steps should should the player move
        player: who is doing the actual step/turn(GameManager knows exactly who is the current player)
    */
    public void MovePlayerToken(int steps, Player player)

    {
        StartCoroutine(MovePlayerInStep(steps, player));
    }
    public void MovePlayerToken(MonopolyNodeType type, Player player)
    {
        int indexOfNextNodeType = -1; // index to find
        int indexOnBoard = route.IndexOf(player.MyMonopolyNode); // where is the player
        int startSearchIndex = (indexOnBoard + 1) % route.Count;
        int nodeSearches = 0; // amount of fields searched

        while(indexOfNextNodeType == -1 && nodeSearches< route.Count)
        {
            if(route[startSearchIndex].monopolyNodeType == type) // found the desired type
            {
                indexOfNextNodeType = startSearchIndex;
            }
            startSearchIndex = (startSearchIndex + 1) % route.Count;
            nodeSearches++;
        }
        if(indexOfNextNodeType == -1) // security exit
        {
            Debug.LogError("NO NODE FOUND");
            return;
        }

        StartCoroutine(MovePlayerInStep(nodeSearches, player));
        
    }
    IEnumerator MovePlayerInStep(int steps, Player player)
    {
        int stepsLeft = steps;
        GameObject tokenToMove = player.MyToken;
        int indexOnBoard = route.IndexOf(player.MyMonopolyNode);
        bool passOverGo = false;
        while(stepsLeft > 0)
        {
            indexOnBoard++;
            //is this over go
            if(indexOnBoard > route.Count - 1)
            {
                indexOnBoard = 0;
                passOverGo = true;
                player.wentOverGo = true;
            }
            //get start and end position
            Vector3 startPos = tokenToMove.transform.position;
            Vector3 endPos = route[indexOnBoard].transform.position;
            //perform this move
            while(MoveToNextNode(tokenToMove, endPos, 20))
            {
                //just keep the while loop works until MoveToNextNode equals true
                yield return null;
            }
            stepsLeft--;
        }
        //passed Go receive 200 
        if(passOverGo)
        {
            //collect money on the player
            player.CollectMoney(GameManager.instance.GetGoMoney);
        }
        //set new node on the current player
        player.SetMyCurrentNode(route[indexOnBoard]);
    }

    bool MoveToNextNode(GameObject tokenToMove, Vector3 endPos, float speed)
    {
        return endPos != (tokenToMove.transform.position = Vector3.MoveTowards(tokenToMove.transform.position, endPos, speed * Time.deltaTime));
    }
    public (List<MonopolyNode> list, bool allSame) PlayerHasAllNodesOfSet(MonopolyNode node)
{
    bool allSame = false;
    foreach (var nodeSet in nodeSetList)
    {
        if (nodeSet.nodesInSetList.Contains(node))
        {
            // LINQ
            allSame = nodeSet.nodesInSetList.All(_node => _node.Owner == node.Owner);
            return (nodeSet.nodesInSetList, allSame);
        }
    }
    return (null, allSame);
}



}

