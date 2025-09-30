using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
public class AIController : MonoBehaviour
{
    float WAIT_TIME = 6f;
    public int whichAi;
    public CardManager manager;
    Dictionary<string, Dictionary<string, int>> aicards = new Dictionary<string, Dictionary<string, int>>();
    List<int> targetCards = new List<int>();
    List<int> whichCardsToPlay = new List<int>();
    List<int> cardsAlreadyUsed = new List<int>();
    enum aiState
    {
        Phase,
        End
    }
    aiState currentState;
    void Start()
    {
        currentState = aiState.Phase;
        foreach (string color in manager.colors)
        {
            aicards[color] = new Dictionary<string, int>();
        }
        resetcurrentcards();
    }
    //IGNORE SKIP WHEN FINDING MOST OF WHAT CARDS
    IEnumerator MakeDecisionCoroutine()
    {
        manager.dolog("hello from ai " + whichAi);
        yield return new WaitForSeconds(WAIT_TIME);
        findcardsanddraw();
        //todiscard.GetComponentInChildren<Button>().onClick.Invoke();
        if ((currentState == aiState.Phase) && isphaseplayable())
        {
            manager.dolog("Ai " + whichAi + " is attempting to play phase ");
            for (int counter = 0; counter < whichCardsToPlay.Count; counter++)
            {
                for (int i = 0; i < 10; i++)
                {
                    GameObject thecard = manager.playerhands[whichAi][i];
                    if (thecard.GetComponent<card>().type == whichCardsToPlay[counter].ToString())
                    {//Select it 
                        if (!cardsAlreadyUsed.Contains(i))
                        {
                            thecard.GetComponentInChildren<Button>().onClick.Invoke();
                            cardsAlreadyUsed.Add(i);
                            break;
                        }
                    }

                }

            }
            currentState = aiState.End;
        }
        else if (currentState == aiState.End)
        {
            while (true)
            {
                bool found = false;
                List<GameObject> handCopy = new List<GameObject>(manager.playerhands[whichAi]);
                List<GameObject> toPlayOnCopy = new List<GameObject>(manager.toPlayOn);
                foreach (GameObject cardtoplay in handCopy)
                {
                    foreach (GameObject cardtoplayon in toPlayOnCopy)
                    {
                        int toplayvalue = int.Parse(cardtoplay.GetComponent<card>().type);
                        int playon = int.Parse(cardtoplayon.GetComponent<card>().type);
                        if (toplayvalue == playon)
                        {
                            manager.isProcessingCardPlay = true;
                            cardtoplay.GetComponentInChildren<Button>().onClick.Invoke();
                            cardtoplayon.GetComponentInChildren<Button>().onClick.Invoke();
                            found = true;
                            yield return new WaitUntil(() => !manager.isProcessingCardPlay);
                            //yield return new WaitForSeconds(WAIT_TIME);
                            break;
                        }
                    }
                    if (found == true)
                    {
                        break;
                    }
                }
                if (found == false)
                {
                    break;
                }
            }        
        }
        yield return new WaitForSeconds(WAIT_TIME);
        GameObject todiscard = findwhattodiscard();
        todiscard.GetComponentInChildren<Button>().onClick.Invoke();
        manager.dolog("to discard: " + todiscard.GetComponent<card>().type);
        //manager.discard();
        whichCardsToPlay.Clear();
        cardsAlreadyUsed.Clear();
        targetCards.Clear();
    }
    void findcurrentcards()//Populates aicards
    {
        foreach (GameObject thecard in manager.playerhands[whichAi])
        {
            string whatcolor = thecard.GetComponent<card>().color;
            string whatnumber = thecard.GetComponent<card>().type;
            //manager.dolog(thecard.GetComponent<card>().type);
            if (!aicards[whatcolor].ContainsKey(whatnumber))
            {
                (aicards[whatcolor])[whatnumber] = 0;
            }
            (aicards[whatcolor])[whatnumber]++;
        }
    }
    void findcardsanddraw()
    {
        findcurrentcards();
        if (currentState == aiState.Phase)
        {
            if (manager.playerphases[whichAi] == 1)
            {
                int card1 = findhighestandremove(3);
                int card2 = findhighestandremove(3);
                //manager.dolog("highest: " + card1);
                //manager.dolog("secondhighest: " + card2);
                targetCards.Add(card1);
                targetCards.Add(card2);
            }
            else if (manager.playerphases[whichAi] == 2)//Set of 3 and run of 4
            {
                int setcard = findhighestandremove(3);
                targetCards.Add(setcard);
                manager.dolog("highest: " + setcard);
                List<int> missingCards = findBestRunWindow(4); // or whatever runLength you need\
                foreach (int theMissingCard in missingCards)
                {
                    targetCards.Add(theMissingCard);
                }
                manager.dolog("=== Run Window Results ===");
                manager.dolog("Missing cards needed for run: " + missingCards.Count);

                if (missingCards.Count == 0)
                {
                    manager.dolog("No missing cards - either have complete run or hand full of skips");
                }
                else
                {
                    string missingStr = "";
                    foreach (int card in missingCards)
                    {
                        missingStr += card.ToString() + " ";
                    }
                    manager.dolog("Missing card numbers: " + missingStr);
                }

                manager.dolog("=== End Run Window Results ===");
            }
        }
        else if (currentState == aiState.End)//Already played phase
        {
            for (int i = 0; i < manager.toPlayOn.Count; i++)
            {
                int cardValue = int.Parse(manager.toPlayOn[i].GetComponent<card>().type);
                targetCards.Add(cardValue);
            }
            manager.dolog("=== Cards that can be played on ===");
            if (manager.toPlayOn.Count == 0)
            {
                manager.dolog("toPlayOn list is empty");
            }
            else
            {
                for (int i = 0; i < manager.toPlayOn.Count; i++)
                {
                    manager.dolog($"toPlayOn[{i}]: {manager.toPlayOn[i].GetComponent<card>().type}");
                }
            }
            manager.dolog("=== End of toPlayOn list ===");
        }
        int tolookfor = int.Parse(manager.discardpile.GetComponent<card>().type);
        if (targetCards.Contains(tolookfor))
        {
            manager.dolog("get from discard");
            manager.discardpile.GetComponentInChildren<Button>().onClick.Invoke();
        }
        else
        {
            manager.dolog("draw create card");
            manager.drawcreatecard();
        }
        resetcurrentcards();
    }
    void resetcurrentcards()
    {
        foreach (var kvp in aicards)
        {
            kvp.Value.Clear();
        }
        foreach (string color in manager.colors)
        {
            aicards[color] = new Dictionary<string, int>();
            foreach (int num in manager.numbers)
            {
                aicards[color][num.ToString()] = 0;
            }
        }
    }
    GameObject findwhattodiscard()
    {
        GameObject todiscard = null;
        foreach (GameObject thecard in manager.playerhands[whichAi])
        {
            if (todiscard == null)
            {
                todiscard = thecard;
                continue;
            }
            int whatnumber = int.Parse(thecard.GetComponent<card>().type);
            if (whatnumber == -1)//is a skip
            {
                return thecard;
            }
            if (targetCards.Contains(whatnumber) == false)
            {
                todiscard = thecard;
            }
        }
        return todiscard;
    }
    int findhighestandremove(int howmanytoremove)//Modifies aicards
    {
        int highestindex = 0;
        int highestval = 0;
        foreach (int whatnumber in manager.numbers)
        {
            int counter = 0;
            foreach (string whatcolor in manager.colors)
            {
                if (whatcolor != "skip")
                {
                    int compare = aicards[whatcolor].GetValueOrDefault(whatnumber.ToString(), 0);
                    counter = counter + compare;
                }
            }
            if (counter > highestval)
            {
                highestval = counter;
                highestindex = whatnumber;
            }
        }
        int howmanysubtracted = 0;
        foreach (string whatcolor in manager.colors)
        {
            if (whatcolor == "skip") continue;

            if (!aicards[whatcolor].ContainsKey(highestindex.ToString())) continue;

            while (aicards[whatcolor][highestindex.ToString()] > 0)
            {
                aicards[whatcolor][highestindex.ToString()]--;
                howmanysubtracted++;
                if (howmanysubtracted == howmanytoremove)
                {
                    return highestindex;
                }
            }

        }
        //manager.dolog("What number? " + highestindex + " How many? " + highestval);
        return highestindex;
    }
    List<int> findBestRunWindow(int runLength)
    {
        int maxnumbers = manager.numbers.Length;
        int highestCount = 0;
        int bestStartNum = 0;
        List<int> bestWindowAlreadyHave = new List<int>();
        for (int startNum = 1; startNum <= (maxnumbers - runLength + 1); startNum++)
        {
            int positionsFilled = 0;
            List<int> alreadyHave = new List<int>();
            for (int i = 0; i < runLength; i++)
            {
                foreach (string whatColor in manager.colors)
                {
                    if (whatColor != "skip")
                    {
                        int howmany = aicards[whatColor].GetValueOrDefault((startNum + i).ToString(), 0);
                        if (howmany > 0)
                        {
                            positionsFilled++;
                            alreadyHave.Add(startNum + i);
                            break;
                        }
                    }
                }
            }
            if (positionsFilled > highestCount)
            {
                highestCount = positionsFilled;
                bestStartNum = startNum;
                bestWindowAlreadyHave = alreadyHave;
            }
        }
        if (highestCount == 0)
        {
            return new List<int>(); //FULL HAND OF SKIPS!
        }
        List<int> missingNumbers = new List<int>();
        for (int i = 0; i < runLength; i++)
        {
            int cardNum = bestStartNum + i;
            if (!bestWindowAlreadyHave.Contains(cardNum))
            {
                missingNumbers.Add(cardNum);
            }
        }
        
        return missingNumbers;
    }
    bool isphaseplayable()
    {
        if (manager.playerphases[whichAi] == 1)
        {
            int totalsets = 0;
            foreach (int num in manager.numbers)
            {
                resetcurrentcards();
                findcurrentcards();
                if (checkset(num, 3) == true)
                {
                    //manager.dolog(num + " has a set of 3");
                    for (int z = 0; z < 3; z++)
                    {
                        whichCardsToPlay.Add(num);
                    }
                    totalsets++;
                    if (totalsets == 2)
                    {
                        return true;
                    }
                    if (checkset(num, 3) == true)
                    {
                        //manager.dolog(num + " Bonus set of 3");]
                        for (int z = 0; z < 3; z++)
                        {
                            whichCardsToPlay.Add(num);
                        }
                        totalsets++;
                        if (totalsets == 2)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        if (manager.playerphases[whichAi] == 2)
        {
            int totalsets = 0;
            foreach (int num in manager.numbers)
            {
                resetcurrentcards();
                findcurrentcards();
                if (checkset(num, 3) == true)
                {
                    //manager.dolog(num + " has a set of 3");
                    for (int z = 0; z < 3; z++)
                    {
                        whichCardsToPlay.Add(num);
                    }
                    totalsets++;
                    if (totalsets == 2)
                    {
                        return true;
                    }
                    if (checkset(num, 3) == true)
                    {
                        //manager.dolog(num + " Bonus set of 3");]
                        for (int z = 0; z < 3; z++)
                        {
                            whichCardsToPlay.Add(num);
                        }
                        totalsets++;
                        if (totalsets == 2)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    bool checkset(int setofwhat, int setlength)
    {
        Dictionary<string, Dictionary<string, int>> copycounts = new Dictionary<string, Dictionary<string, int>>();
        foreach (string color in manager.colors)
        {
            copycounts[color] = new Dictionary<string, int>();
            foreach (int num in manager.numbers)
            {
                copycounts[color][num.ToString()] = 0;
            }
        }
        int total = 0;
        foreach (string color in manager.colors)
        {
            int pretotal = total;
            total += aicards[color][setofwhat.ToString()];
            if (total >= setlength)
            {
                copycounts[color][setofwhat.ToString()] += setlength - pretotal;//total - pretotal;
                foreach (string newcolor in manager.colors)
                {
                    aicards[newcolor][setofwhat.ToString()] -= copycounts[newcolor][setofwhat.ToString()];
                }
                return true;
            }
            copycounts[color][setofwhat.ToString()] += aicards[color][setofwhat.ToString()];
        }
        return false;
    }
    bool checkrun(int start, int runLength)
    {
        int runcounter = 0;
        for (int i = 0; i < runLength; i++)
        {
            int cardNum = start + i;
            bool found = false;
            foreach (string color in manager.colors)
            {
                if (color == "skip")
                {
                    continue;
                }
                if (aicards[color].GetValueOrDefault(cardNum.ToString(), 0) > 0)
                {
                    aicards[color][cardNum.ToString()]--;
                    whichCardsToPlay.Add(cardNum);
                    found = true;
                    break;
                }
            }
            if (found == false)
            {
                return false;
            }
        }
        return true; 
    }
    public void MakeDecision()
    {
        StartCoroutine(MakeDecisionCoroutine());
    }
}
