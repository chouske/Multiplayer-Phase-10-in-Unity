using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class CardManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    #region Card Prefabs
    /*public GameObject card1;
    public GameObject card2;
    public GameObject card3;
    public GameObject card4;
    public GameObject card5;
    public GameObject card6;
    public GameObject card7;
    public GameObject card8;
    public GameObject card9;
    public GameObject card10;
    public GameObject card11;
    public GameObject card12;*/
    #endregion
    string[] colors = { "red", "blue", "green", "yellow", "skip" };
    GameObject discardpile;
    //int[] numbers = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12};
    //string[] colors = {"blue"};
    int[] numbers = { 1, 2, 3, 4 };
    public TMP_Text turntext;
    public TMP_Text roundtext;
    public TMP_Text phasebuttontext;
    public TMP_Text eachphasetext;
    //float CARD_START_X = -10.15f;
    float CARD_START_X = -20.15f;
    float CARD_START_Y = -5.25f;
    float CARD_GAP_X = 1.5f;
    float CARD_GAP_Y = 3.0f;
    float PHASE_START_X = 0.0f;
    float PHASE_START_Y = 3.75f;
    float PHASE_GAP_X = 1.5f;
    float PHASE_GAP_Y = -3.0f;
    Vector3 DISCARD_POSITION = new Vector3(9.14f, 0.5f, 416.2204f);
    int max_players = 6;
    int actual_players = 4;
    int max_cards = 11;
    int round = 0;
    int playerturn = 0;
    bool hasdiscard = false;
    bool hasdraw = false;
    bool editingphase = false;
    List<GameObject> phasebuildercards;
    List<int> phasebuilderindices;
    //List<GameObject> allpossiblecards;
    Dictionary<string, Dictionary<string, GameObject>> allpossiblecards = new Dictionary<string, Dictionary<string, GameObject>>();
    List<GameObject>[] playerhands;
    int[] playerphases;
    int[] phaserequirements = {6, 7, 8, 7, 8, 9, 8, 7, 7, 8};
    void Start()
    {
        playerphases = new int[actual_players];
        for (int y = 0; y < actual_players; y++)
        {
            playerphases[y] = 1;
            eachphasetext.text += "Player " + (y+1).ToString() + ": Phase 1\n";
        }
        phasebuildercards = new List<GameObject>();
        phasebuilderindices = new List<int>();
        foreach (string color in colors)
        {
            allpossiblecards[color] = new Dictionary<string, GameObject>();
        }
        playerhands = new List<GameObject>[actual_players];
        for (int i = 0; i < actual_players; i++)
        {
            playerhands[i] = new List<GameObject>();
        }
        foreach (string color in colors)
        {
            if (color == "skip")
            {
                continue;
            }
            foreach (int num in numbers)
            {
                //Debug.Log(color + num.ToString());
                if ((num < 5))
                {
                    GameObject card = Resources.Load<GameObject>("Prefabs/" + color + num.ToString());
                    //allpossiblecards.Add(card);
                    allpossiblecards[color][num.ToString()] = card;

                }
            }
        }
        GameObject skipcard = Resources.Load<GameObject>("Prefabs/skip1");
        allpossiblecards["skip"]["1"] = skipcard;
        giveplayersstartingcards();
    }
    void giveplayersstartingcards()
    {
        for (int y = 0; y < actual_players; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                int index = x; //Needed because of weirdness with lambdas
                int owner = y;
                var randomcard = generaterandomcard();
                giveplayercard(owner, randomcard.Item1, randomcard.Item2);
            }
        }
    }
    public void playphase()//Current player
    {
        if (editingphase)
        {
            editingphase = false;
            phasebuttontext.text = "Play Phase";
            foreach (GameObject tempcard in playerhands[playerturn])
            {
                tempcard.GetComponent<SpriteRenderer>().color = Color.white;
            }
            phasebuildercards.Clear();
            phasebuilderindices.Clear();
            return;
        }
        phasebuttontext.text = "Cancel phase";
        editingphase = true;
        Dictionary<string, Dictionary<string, int>> cardcounts = new Dictionary<string, Dictionary<string, int>>();
        foreach (string color in colors)
        {
            cardcounts[color] = new Dictionary<string, int>();
            foreach (int num in numbers)
            {
                cardcounts[color][num.ToString()] = 0;
            }
        }
        foreach (GameObject tempcard in playerhands[playerturn])
        {
            card cardscript = tempcard.GetComponent<card>();
            cardcounts[cardscript.color][cardscript.type]++;
            //Debug.Log(cardscript.color + cardscript.type);
        }
        /*if (checkphase(phasebuildercards, 1) == true)
        {
            Debug.Log("phase 1 complete!");
        }*/
    }
    bool checkphase(List<GameObject> cardcounts, int whatphase)
    {
        if (whatphase == 1)//2 sets of 3
        {
            Debug.Log("Phase 1 check");
            for (int z = 0; z < 2; z++)
            {
                if (!checkset(cardcounts, 3))
                {
                    return false;
                }
                for (int x = 0; x < 3; x++)
                {
                    cardcounts.RemoveAt(0);
                }
            }
        }
        else if (whatphase == 2)
        {
            Debug.Log("Phase 2 check");
            if (!checkset(cardcounts, 3))
            {
                return false;
            }
            for (int x = 0; x < 3; x++)
            {
                cardcounts.RemoveAt(0);
            }
            if (!checkrun(cardcounts, 4))
            {
                return false;
            }
        }
        else if (whatphase == 3)
        {
            Debug.Log("Phase 3 check");
            if (!checkset(cardcounts, 3))
            {
                return false;
            }
            for (int x = 0; x < 3; x++)
            {
                cardcounts.RemoveAt(0);
            }
            if (!checkrun(cardcounts, 4))
            {
                return false;
            }
        }
        else if (whatphase == 4)
        {
            Debug.Log("Phase 4 check");
            if (!checkrun(cardcounts, 7))
            {
                return false;
            }
        }
        else if (whatphase == 5)
        {
            Debug.Log("Phase 5 check");
            if (!checkrun(cardcounts, 8))
            {
                return false;
            }
        }
        else if (whatphase == 6)
        {
            Debug.Log("Phase 6 check");
            if (!checkrun(cardcounts, 9))
            {
                return false;
            }
        }
        else if (whatphase == 7)
        {
            Debug.Log("Phase 7 check");
            for (int z = 0; z < 2; z++)
            {
                if (!checkset(cardcounts, 4))
                {
                    return false;
                }
                for (int x = 0; x < 4; x++)
                {
                    cardcounts.RemoveAt(0);
                }
            }
        }
        else if (whatphase == 8)
        {
            Debug.Log("Phase 8 check");
            if (!checkcolor(cardcounts, 7))
            {
                return false;
            }
        }
        else if (whatphase == 9)
        {
            Debug.Log("Phase 9 check");
            if (!checkset(cardcounts, 5))
            {
                return false;
            }
            for (int x = 0; x < 5; x++)
            {
                cardcounts.RemoveAt(0);
            }
            if (!checkrun(cardcounts, 2))
            {
                return false;
            }
        }
        else if (whatphase == 10)
        {
            Debug.Log("Phase 10 check");
            if (!checkset(cardcounts, 5))
            {
                return false;
            }
            for (int x = 0; x < 5; x++)
            {
                cardcounts.RemoveAt(0);
            }
            if (!checkrun(cardcounts, 3))
            {
                return false;
            }
        }
        return true;
    }
    /*bool checksetold(Dictionary<string, Dictionary<string, int>> cardcounts, int setofwhat, int setlength)
    {
        Dictionary<string, Dictionary<string, int>> copycounts = new Dictionary<string, Dictionary<string, int>>();
        foreach (string color in colors)
        {
            if (color == "skip")
            {
                continue;
            }
            copycounts[color] = new Dictionary<string, int>();
            foreach (int num in numbers)
            {
                copycounts[color][num.ToString()] = 0;
            }
        }
        int total = 0;
        foreach (string color in colors)
        {
            if (color == "skip")
            {
                continue;
            }
            int pretotal = total;
            total += cardcounts[color][setofwhat.ToString()];
            if (total >= setlength)
            {
                copycounts[color][setofwhat.ToString()] += setlength - pretotal;//total - pretotal;
                foreach (string newcolor in colors)
                {
                    if (newcolor == "skip")
                    {
                        continue;
                    }
                    cardcounts[newcolor][setofwhat.ToString()] -= copycounts[newcolor][setofwhat.ToString()];
                }
                return true;
            }
            copycounts[color][setofwhat.ToString()] += cardcounts[color][setofwhat.ToString()];
        }
        return false;
    }*/
    bool checkset(List<GameObject> cardcounts, int setlength)
    {
        bool foundfirst = false;
        string firstnum = "";
        int dictsize = cardcounts.Count;
        for (int z = 0; z < setlength; z++)
        {
            string cardtype = cardcounts[z].GetComponent<card>().type;
            if (!foundfirst)
            {
                firstnum = cardtype;
                foundfirst = true;
            }
            else
            {
                if (cardtype != firstnum)
                {
                    return false;
                }
            }
        }
        return true;
    }
    bool checkrun(List<GameObject> cardcounts, int runlength)
    {
        bool foundfirst = false;
        string firstnum = "";
        int dictsize = cardcounts.Count;
        /*if (runlength > dictsize)
        {
            return false;
        }*/
        for (int z = 0; z < runlength; z++)
        {
            string cardtype = cardcounts[z].GetComponent<card>().type;
            Debug.Log("card type: " + cardtype);
            if (!foundfirst)
            {
                firstnum = cardtype;
                foundfirst = true;
            }
            else
            {   
                if (cardtype != (int.Parse(firstnum) + 1).ToString())
                {
                    return false;
                }
                firstnum = (int.Parse(firstnum)+1).ToString();
            }
        }
        return true;
    }
    bool checkcolor(List<GameObject> cardcounts, int setlength)
    {
        bool foundfirst = false;
        string firstnum = "";
        int dictsize = cardcounts.Count;
        for (int z = 0; z < setlength; z++)
        {
            string cardtype = cardcounts[z].GetComponent<card>().color;
            if (!foundfirst)
            {
                firstnum = cardtype;
                foundfirst = true;
            }
            else
            {
                if (cardtype != firstnum)
                {
                    return false;
                }
            }
        }
        return true;
    }
    (string, string) generaterandomcard()
    {
        string randomColor = colors[Random.Range(0, colors.Length)];
        string randomNumber = numbers[Random.Range(0, numbers.Length)].ToString();
        if (randomColor == "skip")
        {
            randomNumber = "1";
        }
        return (randomColor, randomNumber);
    }
    /*void hideallplayercards(){
        for(int y = 0; y < actual_players; y++){
            for(int x = 0; x < playerhands[y].Count; x++){
                        playerhands[y][x].SetActive(false);
            }
        }
    }*/
    /*void displayplayercards(int playerid){
        for(int x = 0; x < playerhands[playerid].Count; x++){
                playerhands[playerid][x].SetActive(true);
        }
    }*/
    public void endround()
    {
        hasdiscard = false;
        hasdraw = false;
        playerturn = 0;
        round++;
        roundtext.text = "Round " + (round + 1).ToString();
        turntext.text = "Player 1's turn";
        for (int i = 0; i < actual_players; i++)
        {
            foreach (GameObject card in playerhands[i])
            {
                Destroy(card);
            }
            playerhands[i].Clear();
        }
        giveplayersstartingcards();
    }
    public void endturn()
    {
        if (!hasdiscard)
        {
            return;
        }
        editingphase = false;
        phasebuildercards.Clear();
        phasebuilderindices.Clear();
        phasebuttontext.text = "Play Phase";
        int loopcount = 1;
        if (discardpile.GetComponent<card>().color == "skip")
        {
            loopcount = 2;
        }
        if (!hasdraw)
        {
            return;
        }
        for (int i = 0; i < loopcount; i++)
        {
            if (playerturn == (actual_players - 1))
            {
                playerturn = 0;
            }
            else
            {
                playerturn++;
            }
        }
        turntext.text = "Player " + (playerturn + 1).ToString() + "'s turn";
        hasdiscard = false;
        hasdraw = false;
        //hideallplayercards();
        //displayplayercards(playerturn);
    }
    /*GameObject getcard(int playerid, int index)
    {
        return playerhands[playerturn][index];
    }*/     
    void drawcreatecard()
    {//Determines a random card, creates an object of it, and makes sure it has appeared
        if (!hasdraw)
        {
            var randomcard = generaterandomcard();
            giveplayercard(playerturn, randomcard.Item1, randomcard.Item2);
            hasdraw = true;
        }
    }
    void removecard(int owner, int whichcard)
    {
        if (owner != playerturn)
        {
            Debug.Log("Failed to remove card");
            return;
        }
        Debug.Log("owner: " + owner + " which: " + whichcard);
        if (!hasdraw)
        {
            return;
        }
        if (hasdiscard)
        {
            return;
        }
        //Debug.Log(whichcard);
        Destroy(discardpile);
        GameObject cardtomove = playerhands[playerturn][whichcard];
        cardtomove.transform.position = DISCARD_POSITION;
        discardpile = cardtomove;
        card cardscript = cardtomove.GetComponent<card>();
        cardtomove.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        cardtomove.GetComponentInChildren<Button>().onClick.AddListener(() =>
        {
            if (!hasdraw)
            {
                Destroy(cardtomove);
                giveplayercard(playerturn, cardscript.color, cardscript.type);
                hasdraw = true;
            }
        ;
        });
        playerhands[playerturn].RemoveAt(whichcard);
        for (int i = whichcard; i < playerhands[playerturn].Count; i++)
        {
            int newcardindex = i;
            GameObject tempcard = playerhands[playerturn][newcardindex];
            tempcard.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            tempcard.GetComponentInChildren<Button>().onClick.AddListener(() => interactwithcard(playerturn, newcardindex));
            tempcard.transform.position = new Vector3(tempcard.transform.position.x - CARD_GAP_X, tempcard.transform.position.y, tempcard.transform.position.z);//= new Vector3(-8.15f + (1.5f * (playerhands[playerturn].Count - 1)), -5.25f, 0f);
        }
        hasdiscard = true;
    }
    public void debugphasebuilder()
    {
        string res = "";
        foreach (int x in phasebuilderindices)
        {
            res += x.ToString();
        }
        Debug.Log("indices: " + res);
    }
    GameObject giveplayercard(int playerid, string color, string num)
    {
        GameObject newboardcard = Instantiate(allpossiblecards[color][num]);
        newboardcard.GetComponent<card>().owner = playerid;
        int beforecardcount = playerhands[playerid].Count;
        Vector3 cardpos = new Vector3(CARD_START_X + CARD_GAP_X * beforecardcount, CARD_START_Y + CARD_GAP_Y * playerid, 0f);
        newboardcard.transform.position = cardpos;
        playerhands[playerid].Add(newboardcard);
        int newcardindex = playerhands[playerid].Count - 1;
        newboardcard.GetComponentInChildren<Button>().onClick.AddListener(() => interactwithcard(playerid, newcardindex));

        return null;
    }
    void interactwithcard(int owner, int whichcard)
    {
        if (owner != playerturn)
        {
            return;
        }
        if (editingphase)
        {
            GameObject phasecard = playerhands[playerturn][whichcard];
            if (phasecard.GetComponent<card>().color == "skip")
            {
                return;
            }
            int containsindex = phasebuilderindices.IndexOf(whichcard);
            if (containsindex != -1)//The hand index of that card is already in the list
            {
                phasecard.GetComponent<SpriteRenderer>().color = Color.white;
                phasebuilderindices.RemoveAt(containsindex);
                phasebuildercards.RemoveAt(phasebuildercards.IndexOf(phasecard));
            }
            else//Doesn't exist
            {
                phasecard.GetComponent<SpriteRenderer>().color = Color.cyan;
                Debug.Log("Added a card");
                //int currentphase = 1;
                phasebuilderindices.Add(whichcard);
                phasebuildercards.Add(phasecard);
                if (phasebuildercards.Count == (phaserequirements[playerphases[playerturn]-1]))
                {
                    Debug.Log("Doing Check");
                    Debug.Log("How many in builder: " + phasebuildercards.Count);
                    Debug.Log("player turn: " + playerturn);
                    /*Debug.Log("requirements: " + phaserequirements[playerphases[playerturn]]);*/
                    List<GameObject> phasebuilderCopy = new List<GameObject>(phasebuildercards);
                    if (checkphase(phasebuildercards, playerphases[playerturn]) == true)
                    {
                        int winner = playerturn;
                        putdowncards(phasebuilderCopy, playerphases[playerturn]);
                        /*for (int x = 0; x < phasebuilderCopy.Count; x++)
                        {
                            phasebuilderCopy[x].transform.position = new Vector3(PHASE_START_X, PHASE_START_Y + (PHASE_GAP_Y * x), phasecard.transform.position.z);
                            phasebuilderCopy[x].GetComponent<SpriteRenderer>().color = Color.white;
                        }*/
                        /*
                        9/10/25 OLD
                        endround();
                        updatephasedata(winner);
                        */
                    }
                    foreach (GameObject tempcard in playerhands[playerturn])
                    {
                        tempcard.GetComponent<SpriteRenderer>().color = Color.white;
                    }
                    editingphase = false;
                    phasebuildercards.Clear();
                    phasebuilderindices.Clear();
                    phasebuttontext.text = "Play Phase";
                }
                
            }
        }
        else
        {
            removecard(owner, whichcard);
        }
    }
    void putdowncards(List<GameObject> cardcounts, int phaseno)
    {
        if (phaseno == 1)
        {
            for (int x = 0; x < 3; x++)
            {
                cardcounts[x].transform.position = new Vector3(PHASE_START_X, PHASE_START_Y + (PHASE_GAP_Y * x), cardcounts[x].transform.position.z);
                cardcounts[x].GetComponent<SpriteRenderer>().color = Color.white;
                //cardcounts.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                /*cardtomove.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    addphaseextension()
                ;
                });*/
            }
            for (int x = 3; x < 6; x++)
            {
                cardcounts[x].transform.position = new Vector3(PHASE_START_X + PHASE_GAP_X, PHASE_START_Y + (PHASE_GAP_Y * (x-3)), cardcounts[x].transform.position.z);
                cardcounts[x].GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
    }
    void updatephasedata(int winner)
    {
        playerphases[winner] += 1;
        eachphasetext.text = "";
        for(int x = 0; x < playerphases.Length; x++)
        {
            eachphasetext.text += "Player " + (x+1).ToString() + ": Phase " + playerphases[x].ToString() + "\n";
        }
    }
}
