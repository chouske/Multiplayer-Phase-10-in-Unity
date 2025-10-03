using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
public class AIController : MonoBehaviour
{
    float WAIT_TIME = 2f;
    public int whichAi;
    public CardManager manager;
    Dictionary<string, Dictionary<string, int>> aicards = new Dictionary<string, Dictionary<string, int>>();
    List<int> runCards = new List<int>();//DO NOT DISCARD
    List<int> targetCards = new List<int>();
    List<string> targetColors = new List<string>();
    List<int> whichCardsToPlay = new List<int>();
    List<string> whichColorsToPlay = new List<string>();
    List<int> cardsAlreadyUsed = new List<int>();
    int currentPhase;
    enum aiState
    {
        Phase,
        End
    }
    aiState currentState;
    void Start()
    {
        currentPhase = manager.playerphases[whichAi];
        currentState = aiState.Phase;
        foreach (string color in manager.colors)
        {
            aicards[color] = new Dictionary<string, int>();
        }
        resetcurrentcards();
    }
    public float increasespeed()
    {
        WAIT_TIME = Mathf.Min(10f, WAIT_TIME + 0.5f);
        return WAIT_TIME;   
    }
    public float decreasespeed()
    {
        WAIT_TIME = Mathf.Max(0.5f, WAIT_TIME - 0.5f);
        return WAIT_TIME;
    }
    public void resetai()
    {
        currentPhase = manager.playerphases[whichAi];
        aicards.Clear();
        runCards.Clear();
        targetCards.Clear();
        targetColors.Clear();
        whichCardsToPlay.Clear();
        whichColorsToPlay.Clear();
        cardsAlreadyUsed.Clear();
        currentState = aiState.Phase;
        foreach (string color in manager.colors)
        {
            aicards[color] = new Dictionary<string, int>();
        }
        manager.dolog($"AI {whichAi} has been reset for new round.");
    }
    //IGNORE SKIP WHEN FINDING MOST OF WHAT CARDS
    IEnumerator MakeDecisionCoroutine()
    {
        manager.dolog("Hello from ai " + whichAi);
        yield return new WaitForSeconds(WAIT_TIME);
        findcardsanddraw();
        yield return new WaitUntil(() => !manager.isDrawingCard);
        if ((currentState == aiState.Phase) && isphaseplayable())
        {
            manager.dolog("Ai " + whichAi + " is attempting to play phase ");
            if (currentPhase == 8)
            {
                for (int counter = 0; counter < whichColorsToPlay.Count; counter++)
                {
                    for (int i = 0; i < manager.playerhands[whichAi].Count; i++)
                    {
                        GameObject thecard = manager.playerhands[whichAi][i];
                        if (thecard.GetComponent<card>().color == whichColorsToPlay[counter])
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
            }
            else
            {
                for (int counter = 0; counter < whichCardsToPlay.Count; counter++)
                {
                    for (int i = 0; i < manager.playerhands[whichAi].Count; i++)
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
            }
            currentState = aiState.End;
        }
        if (currentState == aiState.End)//Not else if, needs to be able to immediately play on itself
        {
            while (true)
            {
                bool found = false;
                List<GameObject> handCopy = new List<GameObject>(manager.playerhands[whichAi]);
                List<(GameObject card, CardManager.PhaseRule rule)> toPlayOnCopy = new List<(GameObject, CardManager.PhaseRule)>(manager.toPlayOn);
                foreach (GameObject cardtoplay in handCopy)
                {
                    foreach ((GameObject cardtoplayon, CardManager.PhaseRule rule) in toPlayOnCopy)
                    {
                        int toplayvalue = int.Parse(cardtoplay.GetComponent<card>().type);
                        int playon = int.Parse(cardtoplayon.GetComponent<card>().type);
                        string toplaycolor = cardtoplay.GetComponent<card>().color;
                        string playoncolor = cardtoplayon.GetComponent<card>().color;
                        bool canPlay = false;
                        if (rule == CardManager.PhaseRule.Set)
                        {
                            canPlay = (toplayvalue == playon);
                        }
                        else if (rule == CardManager.PhaseRule.Color)
                        {
                            canPlay = (toplaycolor == playoncolor);
                        }
                        else if (rule == CardManager.PhaseRule.Run)
                        {
                            canPlay = (toplayvalue == (playon + 1));
                        }
                        if (canPlay)
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
        manager.dolog("Ai " + whichAi + " chooses to discard: " + todiscard.GetComponent<card>().type);
        manager.discard();
        whichCardsToPlay.Clear();
        whichColorsToPlay.Clear();
        cardsAlreadyUsed.Clear();
        targetCards.Clear();
        targetColors.Clear();
        runCards.Clear();
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
            if (currentPhase == 1)
            {
                int card1 = findhighestandremove(3);
                int card2 = findhighestandremove(3);
                manager.dolog("For phase 1 ai " + whichAi + " wants a set of: " + card1);
                manager.dolog("Ai also wants a set of: " + card2);
                targetCards.Add(card1);
                targetCards.Add(card2);
            }
            else if (currentPhase == 2)//Set of 3 and run of 4
            {
                int setcard = findhighestandremove(3);
                targetCards.Add(setcard);
                manager.dolog("Phase 2 wants set of: " + setcard);
                var (startNum, missingCards) = findBestRunWindow(4);
                if (startNum != -1)
                {
                    foreach (int theMissingCard in missingCards)
                    {
                        targetCards.Add(theMissingCard);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        runCards.Add(startNum + i);
                    }
                }
                debugrun(missingCards);
            }
            else if (currentPhase == 3)//Set of 4 and run of 4
            {
                int setcard = findhighestandremove(4);
                targetCards.Add(setcard);
                manager.dolog("Phase 3 wants set of: " + setcard);
                var (startNum, missingCards) = findBestRunWindow(4);
                if (startNum != -1)
                {
                    foreach (int theMissingCard in missingCards)
                    {
                        targetCards.Add(theMissingCard);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        runCards.Add(startNum + i);
                    }
                }
                debugrun(missingCards);
            }
            else if (currentPhase== 4)//Run of 7
            {
                var (startNum, missingCards) = findBestRunWindow(7);
                if (startNum != -1)
                {
                    foreach (int theMissingCard in missingCards)
                    {
                        targetCards.Add(theMissingCard);
                    }
                    for (int i = 0; i < 7; i++)
                    {
                        runCards.Add(startNum + i);
                    }
                }
                debugrun(missingCards);
            }
            else if (currentPhase == 5)//Run of 8
            {
                var (startNum, missingCards) = findBestRunWindow(8);
                if (startNum != -1)
                {
                    foreach (int theMissingCard in missingCards)
                    {
                        targetCards.Add(theMissingCard);
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        runCards.Add(startNum + i);
                    }
                }
                debugrun(missingCards);
            }
            else if (currentPhase == 6)//Run of 9
            {
                var (startNum, missingCards) = findBestRunWindow(9);
                if (startNum != -1)
                {
                    foreach (int theMissingCard in missingCards)
                    {
                        targetCards.Add(theMissingCard);
                    }
                    for (int i = 0; i < 9; i++)
                    {
                        runCards.Add(startNum + i);
                    }
                }
                debugrun(missingCards);
            }
            if (currentPhase == 7)
            {
                int card1 = findhighestandremove(4);
                int card2 = findhighestandremove(4);
                manager.dolog("For phase 7 ai " + whichAi + " wants a set of: " + card1);
                manager.dolog("Ai also wants a set of: " + card2);
                targetCards.Add(card1);
                targetCards.Add(card2);
            }
            if (currentPhase == 8)
            {
                string color1 = findHighestColor();
                manager.dolog("For phase 8 ai " + whichAi + " wants to collect: " + color1);
                targetColors.Add(color1);
            }
            if (currentPhase == 9)
            {
                int card1 = findhighestandremove(5);
                int card2 = findhighestandremove(2);
                manager.dolog("For phase 9 ai " + whichAi + " wants a set of: " + card1);
                manager.dolog("Ai also wants a set of: " + card2);
                targetCards.Add(card1);
                targetCards.Add(card2);
            }
            if (currentPhase == 10)
            {
                int card1 = findhighestandremove(5);
                int card2 = findhighestandremove(3);
                manager.dolog("For phase 10 ai " + whichAi + " wants a set of: " + card1);
                manager.dolog("Ai also wants a set of: " + card2);
                targetCards.Add(card1);
                targetCards.Add(card2);
            }
        }
        else if (currentState == aiState.End)//Already played phase
        {
            for (int i = 0; i < manager.toPlayOn.Count; i++)
            {
                CardManager.PhaseRule rule = manager.toPlayOn[i].rule;
                GameObject card = manager.toPlayOn[i].card;
                
                if (rule == CardManager.PhaseRule.Color)
                {
                    string cardColor = card.GetComponent<card>().color;
                    if (!targetColors.Contains(cardColor))
                    {
                        targetColors.Add(cardColor);
                    }
                }
                else
                {
                    int cardValue = int.Parse(card.GetComponent<card>().type);
                    targetCards.Add(cardValue);
                }
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
                    manager.dolog($"toPlayOn[{i}]: {manager.toPlayOn[i].card.GetComponent<card>().type}");
                }
            }
            manager.dolog("=== End of toPlayOn list ===");
        }
        int tolookfor = int.Parse(manager.discardpile.GetComponent<card>().type);
        if (currentPhase == 8)
        {
            string discardColor = manager.discardpile.GetComponent<card>().color;
            if (targetColors.Contains(discardColor))
            {
                manager.dolog("Ai " + whichAi + " chooses to pick from discard (color match)");
                manager.discardpile.GetComponentInChildren<Button>().onClick.Invoke();
                manager.isDrawingCard = false;
                resetcurrentcards();
                return;
            }
            else
            {
                manager.dolog("Ai " + whichAi + " draws a random card");
                manager.isDrawingCard = true;
                manager.drawcreatecard();
            }
        }
        else if (targetCards.Contains(tolookfor))
        {
            manager.dolog("Ai " + whichAi + " chooses to pick from discard");
            manager.discardpile.GetComponentInChildren<Button>().onClick.Invoke();
            manager.isDrawingCard = false;
        }
        else
        {
            manager.dolog("Ai " + whichAi + " draws a random card");
            manager.isDrawingCard = true;
            manager.drawcreatecard();
        }
        resetcurrentcards();//IMPORTANT
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
            }
            if (currentPhase != 8)
            {
                int whatnumber = int.Parse(thecard.GetComponent<card>().type);
                if (whatnumber == -1)//is a skip
                {
                    return thecard;
                }
                if ((targetCards.Contains(whatnumber) == false) && (runCards.Contains(whatnumber) == false))
                {
                    todiscard = thecard;
                }
            }
            else
            {
                string whatcolor = thecard.GetComponent<card>().color;
                if (whatcolor == "skip")//is a skip
                {
                    return thecard;
                }
                if (targetColors.Contains(whatcolor) == false)
                {
                    todiscard = thecard;
                }
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
    string findHighestColor()//Modifies aicards
    {
        string highestcolor = "";
        int highestval = 0;
        foreach (string whatcolor in manager.colors)
        {
            int counter = 0;
            if (whatcolor != "skip")
            {
                foreach (int whatnumber in manager.numbers)
                {
                    int compare = aicards[whatcolor].GetValueOrDefault(whatnumber.ToString(), 0);
                    counter = counter + compare;
                }
            }
            if (counter > highestval)
            {
                highestval = counter;
                highestcolor = whatcolor;
            }
        }
        /*int howmanysubtracted = 0;
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

        }*/
        return highestcolor;
    }
    (int startNum, List<int>) findBestRunWindow(int runLength)
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
            return (-1, new List<int>()); //FULL HAND OF SKIPS!
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

        return (bestStartNum, missingNumbers);
    }
    Dictionary<string, Dictionary<string, int>> DeepCopyAICards(Dictionary<string, Dictionary<string, int>> original)
    {
        var copy = new Dictionary<string, Dictionary<string, int>>();
        foreach (var colorKVP in original)
        {
            copy[colorKVP.Key] = new Dictionary<string, int>();
            foreach (var numberKVP in colorKVP.Value)
            {
                copy[colorKVP.Key][numberKVP.Key] = numberKVP.Value;
            }
        }
        return copy;
    }
    bool isphaseplayable()
    {
        if (currentPhase == 1)
        {
            foreach (int num in manager.numbers)
            {
                resetcurrentcards();
                findcurrentcards();
                if (checkset(num, 3) == true)//First set exists
                {
                    Dictionary<string, Dictionary<string, int>> aicardsCopy = DeepCopyAICards(aicards);
                    for (int z = 0; z < 3; z++)
                    {
                        whichCardsToPlay.Add(num);
                    }
                    foreach (int num2 in manager.numbers)
                    {
                        if (checkset(num2, 3) == true)
                        {
                            for (int z = 0; z < 3; z++)
                            {
                                whichCardsToPlay.Add(num2);
                            }
                            return true;
                        }
                        aicards = DeepCopyAICards(aicardsCopy);
                    }
                    for (int z = 0; z < 3; z++)
                    {
                        whichCardsToPlay.RemoveAt(whichCardsToPlay.Count - 1);
                    }
                }
            }
        }
        else if (currentPhase == 2)
        {
            foreach (int num in manager.numbers)
            {
                resetcurrentcards();
                findcurrentcards();
                if (checkset(num, 3) == true)//First set exists
                {
                    Dictionary<string, Dictionary<string, int>> aicardsCopy = DeepCopyAICards(aicards);
                    for (int z = 0; z < 3; z++)
                    {
                        whichCardsToPlay.Add(num);
                    }
                    for (int startNum = 1; startNum <= manager.numbers.Length - 4 + 1; startNum++)
                    {
                        if (checkrun(startNum, 4) == true)
                        {
                            for (int i = startNum; i < (startNum + 4); i++)
                            {   
                                whichCardsToPlay.Add(i);
                            }
                            return true;
                        }
                        aicards = DeepCopyAICards(aicardsCopy);
                    }
                    for (int z = 0; z < 3; z++)
                    {
                        whichCardsToPlay.RemoveAt(whichCardsToPlay.Count - 1);
                    }
                }
            }
        }
        else if (currentPhase == 3)
        {
            foreach (int num in manager.numbers)
            {
                resetcurrentcards();
                findcurrentcards();
                if (checkset(num, 4) == true)//First set exists
                {
                    Dictionary<string, Dictionary<string, int>> aicardsCopy = DeepCopyAICards(aicards);
                    for (int z = 0; z < 4; z++)
                    {
                        whichCardsToPlay.Add(num);
                    }
                    for (int startNum = 1; startNum <= manager.numbers.Length - 4 + 1; startNum++)
                    {
                        if (checkrun(startNum, 4) == true)
                        {
                            for (int i = startNum; i < (startNum + 4); i++)
                            {
                                whichCardsToPlay.Add(i);
                            }
                            return true;
                        }
                        aicards = DeepCopyAICards(aicardsCopy);
                    }
                    for (int z = 0; z < 4; z++)
                    {
                        whichCardsToPlay.RemoveAt(whichCardsToPlay.Count - 1);
                    }
                }
            }
        }
        else if (currentPhase == 4)
        {
            for (int startNum = 1; startNum <= manager.numbers.Length - 7 + 1; startNum++)
            {
                resetcurrentcards();
                findcurrentcards();
                if (checkrun(startNum, 7) == true)
                {
                    for (int i = startNum; i < (startNum + 7); i++)
                    {
                        whichCardsToPlay.Add(i);
                    }
                    return true;
                }
            }
        }
        else if (currentPhase == 5)
        {
            for (int startNum = 1; startNum <= manager.numbers.Length - 8 + 1; startNum++)
            {
                resetcurrentcards();
                findcurrentcards();
                if (checkrun(startNum, 8) == true)
                {
                    for (int i = startNum; i < (startNum + 8); i++)
                    {
                        whichCardsToPlay.Add(i);
                    }
                    return true;
                }
            }
        }
        else if (currentPhase == 6)
        {
            for (int startNum = 1; startNum <= manager.numbers.Length - 9 + 1; startNum++)
            {
                resetcurrentcards();
                findcurrentcards();
                if (checkrun(startNum, 9) == true)
                {
                    for (int i = startNum; i < (startNum + 9); i++)
                    {
                        whichCardsToPlay.Add(i);
                    }
                    return true;
                }
            }
        }
        if (currentPhase == 7)
        {
            foreach (int num in manager.numbers)
            {
                resetcurrentcards();
                findcurrentcards();
                if (checkset(num, 4) == true)//First set exists
                {
                    Dictionary<string, Dictionary<string, int>> aicardsCopy = DeepCopyAICards(aicards);
                    for (int z = 0; z < 4; z++)
                    {
                        whichCardsToPlay.Add(num);
                    }
                    foreach (int num2 in manager.numbers)
                    {
                        if (checkset(num2, 4) == true)
                        {
                            for (int z = 0; z < 4; z++)
                            {
                                whichCardsToPlay.Add(num2);
                            }
                            return true;
                        }
                        aicards = DeepCopyAICards(aicardsCopy);
                    }
                    for (int z = 0; z < 4; z++)
                    {
                        whichCardsToPlay.RemoveAt(whichCardsToPlay.Count - 1);
                    }
                }
            }
        }
        if (currentPhase == 8)
        {
            foreach (string thecolor in manager.colors)
            {
                resetcurrentcards();
                findcurrentcards();
                if (checkcolor(thecolor, 7) == true)//First set exists
                {
                    for (int z = 0; z < 7; z++)
                    {
                        whichColorsToPlay.Add(thecolor);
                    }
                    return true;
                }
            }
        }
        if (currentPhase == 9)
        {
            foreach (int num in manager.numbers)
            {
                resetcurrentcards();
                findcurrentcards();
                if (checkset(num, 5) == true)//First set exists
                {
                    Dictionary<string, Dictionary<string, int>> aicardsCopy = DeepCopyAICards(aicards);
                    for (int z = 0; z < 5; z++)
                    {
                        whichCardsToPlay.Add(num);
                    }
                    foreach (int num2 in manager.numbers)
                    {
                        if (checkset(num2, 2) == true)
                        {
                            for (int z = 0; z < 2; z++)
                            {
                                whichCardsToPlay.Add(num2);
                            }
                            return true;
                        }
                        aicards = DeepCopyAICards(aicardsCopy);
                    }
                    for (int z = 0; z < 5; z++)
                    {
                        whichCardsToPlay.RemoveAt(whichCardsToPlay.Count - 1);
                    }
                }
            }
        }
        if (currentPhase == 10)
        {
            foreach (int num in manager.numbers)
            {
                resetcurrentcards();
                findcurrentcards();
                if (checkset(num, 5) == true)//First set exists
                {
                    Dictionary<string, Dictionary<string, int>> aicardsCopy = DeepCopyAICards(aicards);
                    for (int z = 0; z < 5; z++)
                    {
                        whichCardsToPlay.Add(num);
                    }
                    foreach (int num2 in manager.numbers)
                    {
                        if (checkset(num2, 3) == true)
                        {
                            for (int z = 0; z < 3; z++)
                            {
                                whichCardsToPlay.Add(num2);
                            }
                            return true;
                        }
                        aicards = DeepCopyAICards(aicardsCopy);
                    }
                    for (int z = 0; z < 5; z++)
                    {
                        whichCardsToPlay.RemoveAt(whichCardsToPlay.Count - 1);
                    }
                }
            }
        }
        return false;
    }
    bool checkcolor(string setofwhat, int setlength)
    {
            if (setofwhat == "skip")
            {
                return false;
            }
            int count = 0;
            foreach (GameObject thecard in manager.playerhands[whichAi])
            {
                string cardcolor = thecard.GetComponent<card>().color;
                if (cardcolor == setofwhat)
                {
                    count++;
                }
            }
            if (count < setlength)
            {
                return false;
            }
            int added = 0;
            foreach (GameObject thecard in manager.playerhands[whichAi])
            {
                if ((thecard.GetComponent<card>().color == setofwhat) && (added < setlength))
                {
                    whichCardsToPlay.Add(int.Parse(thecard.GetComponent<card>().type));
                    added++;
                }
            }
            return true;
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
    void debugrun(List<int> missingCards)
    {
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
