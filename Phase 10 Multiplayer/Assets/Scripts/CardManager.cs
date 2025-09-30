using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
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
    int STARTING_PHASE = 2;
    public List<GameObject> aiPlayers = new List<GameObject>();
    public string[] colors = { "red", "blue", "green", "yellow", "skip"};
    public GameObject discardpile;
    //string[] colors = {"blue"};
    public int[] numbers = {1, 2, 3, 4};
    public TMP_Text turntext;
    public TMP_Text roundtext;
    public TMP_Text phasebuttontext;
    public TMP_Text eachphasetext;
    //float CARD_START_X = -10.15f;
    float CARD_START_X = -20.15f;
    float CARD_START_Y = -5.25f;
    float CARD_GAP_X = 1.6f;
    float CARD_GAP_Y = 3.2f;
    float PHASE_START_X = -2.0f;
    float PHASE_START_Y = 3.75f;
    float PHASE_GAP_X = 1.6f;
    float PHASE_GAP_Y = -1.6f;
    float SET_GAP_X = 6.4f;
    public int errno = 1;

    enum PhaseRule
    {
        Set,
        Run,
        Color
    }
    //Vector3 DISCARD_POSITION = new Vector3(9.14f, 0.5f, 416.2204f);
    Vector3 DISCARD_POSITION = new Vector3(-15.8f, -10.44f, 416.2204f);
    int max_players = 6;
    int actual_players = 4;
    int max_cards = 11;
    int round = 0;
    int playerturn = 0;
    bool hasdraw = false;
    bool editingphase = false;
    List<GameObject> phasebuildercards;
    Dictionary<string, Dictionary<string, GameObject>> allpossiblecards = new Dictionary<string, Dictionary<string, GameObject>>();
    public List<GameObject>[] playerhands;
    public int[] playerphases;
    int[] phaserequirements = {6, 7, 8, 7, 8, 9, 8, 7, 7, 8};
    public bool[] hasplayedphase;
    public List<GameObject> playedCards = new List<GameObject>();
    public List<GameObject> toPlayOn = new List<GameObject>();
    public bool isProcessingCardPlay = false;
    public void dolog(string themsg)
    {
        Debug.Log(errno.ToString() + ": " + themsg);
        errno++;
    }
    void Start()
    {
        playerphases = new int[actual_players];
        hasplayedphase = new bool[actual_players];
        for (int y = 0; y < actual_players; y++)
        {
            hasplayedphase[y] = false;
            playerphases[y] = STARTING_PHASE;
            eachphasetext.text += "Player " + (y + 1).ToString() + ": Phase 1\n";
        }
        phasebuildercards = new List<GameObject>();
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
                GameObject card = Resources.Load<GameObject>("Prefabs/" + color + num.ToString());
                //allpossiblecards.Add(card);
                allpossiblecards[color][num.ToString()] = card;
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
            //Debug.Log("Phase 1 check");
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
            //Debug.Log("Phase 2 check");
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
            //Debug.Log("Phase 3 check");
            if (!checkset(cardcounts, 4))
            {
                return false;
            }
            for (int x = 0; x < 4; x++)
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
            //Debug.Log("Phase 4 check");
            if (!checkrun(cardcounts, 7))
            {
                return false;
            }
        }
        else if (whatphase == 5)
        {
            //Debug.Log("Phase 5 check");
            if (!checkrun(cardcounts, 8))
            {
                return false;
            }
        }
        else if (whatphase == 6)
        {
            //Debug.Log("Phase 6 check");
            if (!checkrun(cardcounts, 9))
            {
                return false;
            }
        }
        else if (whatphase == 7)
        {
           // Debug.Log("Phase 7 check");
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
            //Debug.Log("Phase 8 check");
            if (!checkcolor(cardcounts, 7))
            {
                return false;
            }
        }
        else if (whatphase == 9)
        {
            //Debug.Log("Phase 9 check");
            if (!checkset(cardcounts, 5))
            {
                return false;
            }
            for (int x = 0; x < 5; x++)
            {
                cardcounts.RemoveAt(0);
            }
            if (!checkset(cardcounts, 2))
            {
                return false;
            }
        }
        else if (whatphase == 10)
        {
            //Debug.Log("Phase 10 check");
            if (!checkset(cardcounts, 5))
            {
                return false;
            }
            for (int x = 0; x < 5; x++)
            {
                cardcounts.RemoveAt(0);
            }
            if (!checkset(cardcounts, 3))
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
            //Debug.Log("card type: " + cardtype);
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
            hasplayedphase[i] = false;
            playerhands[i].Clear();
        }
        foreach (GameObject card in playedCards)
        {
            Destroy(card);
        }
        playedCards.Clear();
        phasebuildercards.Clear();
        Destroy(discardpile);
        giveplayersstartingcards();
        updatephasedata();
    }
    public void endturn()
    {
        editingphase = false;
        phasebuildercards.Clear();
        phasebuttontext.text = "Play Phase";
        int loopcount = 1;
        if (discardpile.GetComponent<card>().color == "skip")
        {
            loopcount = 2;
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
        hasdraw = false;
        if (playerturn != 0)
        {
             aiPlayers[playerturn - 1].GetComponent<AIController>().MakeDecision();
        }
        //hideallplayercards();
        //displayplayercards(playerturn);
    }   
    public void drawcreatecard()
    {//Determines a random card, creates an object of it, and makes sure it has appeared
        if (!hasdraw)
        {
            var randomcard = generaterandomcard();
            giveplayercard(playerturn, randomcard.Item1, randomcard.Item2);
            hasdraw = true;
        }
    }
    /*void oldremovecard(int owner, int whichcard)
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
    }*/
    /*public void debugphasebuilder()
    {
        string res = "";
        foreach (int x in phasebuilderindices)
        {
            res += x.ToString();
        }
        Debug.Log("indices: " + res);
    }*/
    GameObject giveplayercard(int playerid, string color, string num)
    {
    
        GameObject newboardcard = Instantiate(allpossiblecards[color][num]);
        newboardcard.GetComponent<card>().owner = playerid;
        int beforecardcount = playerhands[playerid].Count;
        Vector3 cardpos = new Vector3(CARD_START_X + CARD_GAP_X * beforecardcount, CARD_START_Y + CARD_GAP_Y * playerid, 0f);
        newboardcard.transform.position = cardpos;
        playerhands[playerid].Add(newboardcard);
        newboardcard.GetComponentInChildren<Button>().onClick.AddListener(() => interactwithcard(playerid, newboardcard));

        return null;
    }
    void interactwithcard(int owner, GameObject whichcard)
    {
        //Changed to gameobject instead of index whichcard
        if (owner != playerturn)
        {
            return;
        }
        GameObject phasecard = whichcard;
        bool containscard = phasebuildercards.Contains(whichcard);
        if (containscard == true)
        {
            phasecard.GetComponent<SpriteRenderer>().color = Color.white;
            phasebuildercards.Remove(phasecard);
        }
        else//Doesn't exist
        {
            phasecard.GetComponent<SpriteRenderer>().color = Color.cyan;
            //int currentphase = 1;
            phasebuildercards.Add(phasecard);
            if (phasebuildercards.Count == (phaserequirements[playerphases[playerturn]-1]))
            {
                dolog("Doing Check");
                dolog("How many in builder: " + phasebuildercards.Count);
                dolog("player turn: " + playerturn);
                /*Debug.Log("requirements: " + phaserequirements[playerphases[playerturn]]);*/
                List<GameObject> phasebuilderCopy = new List<GameObject>(phasebuildercards);
                if (checkphase(phasebuilderCopy, playerphases[playerturn]) == true)
                {
                    putdowncards(phasebuildercards, playerphases[playerturn]);
                    hasplayedphase[playerturn] = true;
                    playerphases[playerturn] += 1;//Winner is playerturn
                    if (playerphases[playerturn] == 11)
                    {
                        GameData.winnerName = "Player " + (playerturn+1) + " Wins!";
                        UnityEngine.SceneManagement.SceneManager.LoadScene("WinScene");
                    }
                }
                foreach (GameObject tempcard in playerhands[playerturn])
                {
                    tempcard.GetComponent<SpriteRenderer>().color = Color.white;
                }
                editingphase = false;
                phasebuildercards.Clear();
                phasebuttontext.text = "Play Phase";
            }
            
        }
    }
    void removecardfromhand(GameObject thecard, int whichplayer)
    {
        if (playerhands[whichplayer].Contains(thecard))
        {
            playerhands[whichplayer].Remove(thecard);
            compacthand(whichplayer); // keep hand layout tidy
        }
        else
        {
            Debug.LogWarning($"Tried to remove {thecard.name} from player {whichplayer}'s hand, but it wasn't there!");

        }
    }
    void compacthand(int whichplayer)
    {
        for (int i = 0; i < playerhands[whichplayer].Count; i++)
        {
            int newcardindex = i;
            GameObject tempcard = playerhands[whichplayer][newcardindex];
            tempcard.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            tempcard.GetComponentInChildren<Button>().onClick.AddListener(() => interactwithcard(whichplayer, tempcard));
            //tempcard.transform.position = new Vector3(tempcard.transform.position.x - CARD_GAP_X, tempcard.transform.position.y, tempcard.transform.position.z);//= new Vector3(-8.15f + (1.5f * (playerhands[playerturn].Count - 1)), -5.25f, 0f);
            tempcard.transform.position = new Vector3(CARD_START_X + CARD_GAP_X * i, CARD_START_Y + CARD_GAP_Y * whichplayer, tempcard.transform.position.z);
        }
    }
    public void discard()
    {
        if (!hasdraw)
        {
            return;
        }
        if (phasebuildercards.Count != 1)
        {
            return;
        }
        Destroy(discardpile);
        GameObject cardtomove = phasebuildercards[0];
        cardtomove.GetComponent<SpriteRenderer>().color = Color.white;
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
        removecardfromhand(cardtomove, playerturn);
        if (playerhands[playerturn].Count == 0)
        {
            endround();
            return;
        }
        compacthand(playerturn);
        endturn();
    }
    void placecards(List<GameObject> cardcounts, int start, int end, PhaseRule therule)
    {
        float whatz = 0.0f;
        float startx = PHASE_START_X + PHASE_GAP_X;
        if (start == 0)
        {
            startx = PHASE_START_X;
        }
        for (int z = start; z < (end + 1); z++)
        {
            int x = z;
            GameObject thiscard = cardcounts[x];
            thiscard.transform.position = new Vector3(startx + (SET_GAP_X * playerturn), PHASE_START_Y + (PHASE_GAP_Y * (x - start)), whatz - (x*0.01f));
            thiscard.GetComponent<SpriteRenderer>().color = Color.white;
            thiscard.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            if (x == end)
            {
                toPlayOn.Add(thiscard);
                thiscard.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    addphaseextension(thiscard, therule);
                ;
                });
            }
            playedCards.Add(thiscard);
            removecardfromhand(thiscard, playerturn);
        }
    }
    void putdowncards(List<GameObject> cardcounts, int phaseno)
    {
        dolog("Put down cards");
        if (phaseno == 1)
        {
             placecards(cardcounts, 0, 2, PhaseRule.Set);
             placecards(cardcounts, 3, 5, PhaseRule.Set);
                        
        }
        if (phaseno == 2)
        {
            placecards(cardcounts, 0, 2, PhaseRule.Set);
            placecards(cardcounts, 3, 6, PhaseRule.Run);
        }
        if (phaseno == 3)
        {
            placecards(cardcounts, 0, 3, PhaseRule.Set);
            placecards(cardcounts, 4, 7, PhaseRule.Run);
        }
        if (phaseno == 4)
        {
            placecards(cardcounts, 0, 6, PhaseRule.Run);
        }
        if (phaseno == 5)
        {
            placecards(cardcounts, 0, 7, PhaseRule.Run);
        }
        if (phaseno == 6)
        {
            placecards(cardcounts, 0, 8, PhaseRule.Run);
        }
        if (phaseno == 7)
        {
             placecards(cardcounts, 0, 3, PhaseRule.Set);
             placecards(cardcounts, 4, 7, PhaseRule.Set);         
        }
        if (phaseno == 8)
        {
             placecards(cardcounts, 0, 6, PhaseRule.Color); 
        }
        if (phaseno == 9)
        {
             placecards(cardcounts, 0, 4, PhaseRule.Set);
             placecards(cardcounts, 5, 6, PhaseRule.Set);
        }
        if (phaseno == 10)
        {
             placecards(cardcounts, 0, 4, PhaseRule.Set);
             placecards(cardcounts, 5, 7, PhaseRule.Set);
        }
        compacthand(playerturn);
        
    }
    void addphaseextension(GameObject thecard, PhaseRule therule)
    {
        dolog("Phase extension");
        if ((phasebuildercards.Count != 1) || (hasplayedphase[playerturn] == false))
        {
            dolog("Not 1 or hasn't played phase yet");
            return;
        }
        GameObject basecard = phasebuildercards[0];
        int handValue = int.Parse(phasebuildercards[0].GetComponent<card>().type);
        int boardValue = int.Parse(thecard.GetComponent<card>().type);
        string handColor = phasebuildercards[0].GetComponent<card>().color;
        string boardColor = thecard.GetComponent<card>().color;
        if (therule == PhaseRule.Set)
        {
            if (handValue != boardValue)
            {
                return;
            }
        }
        else if (therule == PhaseRule.Run)
        {
            if (handValue != (boardValue + 1))
            {
                return;
            }
        }
        else
        {
            if (handColor != boardColor)
            {
                return;
            }
        }
        toPlayOn.Add(basecard);
        toPlayOn.Remove(thecard);
        thecard.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        phasebuildercards[0].GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        phasebuildercards[0].GetComponentInChildren<Button>().onClick.AddListener(() =>
        {
            addphaseextension(basecard, therule)
        ;
        });
        playedCards.Add(basecard);
        removecardfromhand(basecard, playerturn);
        if (playerhands[playerturn].Count == 0)
        {
            endround();
            return;
        }
        compacthand(playerturn);
        phasebuildercards[0].transform.position = new Vector3(thecard.transform.position.x, thecard.transform.position.y + PHASE_GAP_Y, thecard.transform.position.z);
        phasebuildercards[0].GetComponent<SpriteRenderer>().color = Color.white;
        phasebuildercards.Clear();
        
        isProcessingCardPlay = false;


    }
    void updatephasedata()
    {
        //playerphases[winner] += 1;
        eachphasetext.text = "";
        for(int x = 0; x < playerphases.Length; x++)
        {
            eachphasetext.text += "Player " + (x+1).ToString() + ": Phase " + playerphases[x].ToString() + "\n";
        }
    }
}
