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
    string[] colors = { "red", "blue", "green", "yellow" };
    GameObject discardpile;
    //int[] numbers = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12};
    //string[] colors = {"blue"};
    int[] numbers = { 1, 2, 3, 4 };
    public TMP_Text turntext;
    public TMP_Text roundtext;
    float CARD_START_X = -10.15f;
    float CARD_START_Y = -5.25f;
    float CARD_GAP_X = 1.5f;
    float CARD_GAP_Y = 3.0f;
    Vector3 DISCARD_POSITION = new Vector3(9.14f, 0.5f, 416.2204f);
    int max_players = 6;
    int actual_players = 4;
    int max_cards = 11;
    int round = 0;
    int playerturn = 0;
    bool hasdiscard = false;
    bool hasdraw = false;
    //List<GameObject> allpossiblecards;
    Dictionary<string, Dictionary<string, GameObject>> allpossiblecards = new Dictionary<string, Dictionary<string, GameObject>>();
    List<GameObject>[] playerhands;
    void Start()
    {
        foreach (string color in colors)
        {
            allpossiblecards[color] = new Dictionary<string, GameObject>();
        }
        playerhands  = new List<GameObject>[actual_players];
        for (int i = 0; i < actual_players; i++)
            {
                playerhands[i] = new List<GameObject>();
            }
            foreach(string color in colors){
                foreach(int num in numbers){
                    //Debug.Log(color + num.ToString());
                    if ((num < 5))
                    {
                        GameObject card = Resources.Load<GameObject>("Prefabs/" + color + num.ToString());
                        //allpossiblecards.Add(card);
                        allpossiblecards[color][num.ToString()] = card;
                            
                    }
                    }
            }
            giveplayersstartingcards();
            //displayplayercards(0);
    }
    void giveplayersstartingcards(){
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
        //Debug.Log("player going down: " + playerturn);
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
        if (checkphase(cardcounts, 10) == true)
        {
            Debug.Log("phase 10 complete!");
        }
    }
    bool checkphase(Dictionary<string, Dictionary<string, int>> cardcounts, int whatphase)
    {
        if (whatphase == 1)//2 sets of 3
        {
            int totalsets = 0;
            foreach (int num in numbers)
            {
                if (checkset(cardcounts, num, 3) == true)
                {
                    Debug.Log(num + " has a set of 3");
                    totalsets++;
                    if (totalsets == 2)
                    {
                        foreach (string color in colors)
                        {
                            foreach (int curnum in numbers){
                                if (cardcounts[color][curnum.ToString()] > 0)
                                {
                                    Debug.Log(color + " " + curnum.ToString() + ": " + cardcounts[color][curnum.ToString()]);
                                }
                            }
                        }
                        return true;
                    }
                    if (checkset(cardcounts, num, 3) == true)
                    {
                        Debug.Log(num + " Bonus set of 3");
                        totalsets++;
                        if (totalsets == 2)
                        {
                            foreach (string color in colors)
                            {
                                foreach (int curnum in numbers){
                                    if (cardcounts[color][curnum.ToString()] > 0)
                                    {
                                        Debug.Log(color + " " + curnum.ToString() + ": " + cardcounts[color][curnum.ToString()]);
                                    }
                                }
                            }
                            return true;
                        }
                    }
                }
            }
        }
        if (whatphase == 2)//1 set of 3 + 1 run of 4
        {
            Dictionary<string, Dictionary<string, int>> copydict = new Dictionary<string, Dictionary<string, int>>();
            foreach (int outernum in numbers)//Test for all numbers, make a set and then a run
            {
                foreach (string color in colors)//Finish making copydict
                {
                    copydict[color] = new Dictionary<string, int>();
                    foreach (int num in numbers)
                    {
                        copydict[color][num.ToString()] = cardcounts[color][num.ToString()];
                    }   
                }
                if (checkset(copydict,outernum, 3) == true)
                {
                    if (checkrun(copydict, 4) == true)
                    {
                        Debug.Log("Built with a set of " + outernum.ToString());
                        return true;
                    }
                }

            }
            /*bool hasrun = checkrun(cardcounts, 4);
            if (hasrun == false)
            {
                return false;
            }
            foreach (int num in numbers)
            {
                if (checkset(cardcounts,num, 3) == true)
                {
                    return true;
                }
            }*/
        }
        if (whatphase == 3)//1 set of 4 + 1 run of 4
        {
            Dictionary<string, Dictionary<string, int>> copydict = new Dictionary<string, Dictionary<string, int>>();
            foreach (int outernum in numbers)//Test for all numbers, make a set and then a run
            {
                foreach (string color in colors)//Finish making copydict
                {
                    copydict[color] = new Dictionary<string, int>();
                    foreach (int num in numbers)
                    {
                        copydict[color][num.ToString()] = cardcounts[color][num.ToString()];
                    }   
                }
                if (checkset(copydict,outernum, 4) == true)
                {
                    Debug.Log("remaining when set " + outernum.ToString() + " exists: ");
                    foreach (string color in colors)
                    {
                        foreach (int curnum in numbers)
                        {
                            if (copydict[color][curnum.ToString()] > 0)
                            {
                                Debug.Log(color + " " + curnum.ToString() + ": " + copydict[color][curnum.ToString()]);
                            }
                        }
                    }
                    if (checkrun(copydict, 4) == true)
                    {
                        Debug.Log("Built with a set of " + outernum.ToString());
                        return true;
                    }
                }

            }
        }
        if (whatphase == 4)//1 run of 7
        {
            bool hasrun = checkrun(cardcounts, 7);
            if (hasrun == false)
            {
                return false;
            }
        }
        if (whatphase == 5)//1 run of 8
        {
            bool hasrun = checkrun(cardcounts, 8);
            if (hasrun == false)
            {
                return false;
            }
        }
        if (whatphase == 6)//1 run of 9
        {
            bool hasrun = checkrun(cardcounts, 9);
            if (hasrun == false)
            {
                return false;
            }
        }
        if (whatphase == 7)//2 sets of 4
        {
            int totalsets = 0;
            foreach (int num in numbers)
            {
                if (checkset(cardcounts, num, 4) == true)
                {
                    totalsets++;
                    if (totalsets == 2)
                    {
                        foreach (string color in colors)
                        {
                            foreach (int curnum in numbers){
                                if (cardcounts[color][curnum.ToString()] > 0)
                                {
                                    Debug.Log(color + " " + curnum.ToString() + ": " + cardcounts[color][curnum.ToString()]);
                                }
                            }
                        }
                        return true;
                    }
                    if (checkset(cardcounts, num, 4) == true)
                    {
                        totalsets++;
                        if (totalsets == 2)
                        {
                            foreach (string color in colors)
                            {
                                foreach (int curnum in numbers){
                                    if (cardcounts[color][curnum.ToString()] > 0)
                                    {
                                        Debug.Log(color + " " + curnum.ToString() + ": " + cardcounts[color][curnum.ToString()]);
                                    }
                                }
                            }
                            return true;
                        }
                    }
                }
            }
        }
        if (whatphase == 8)//7 cards of 1 color
        {
            foreach (string color in colors)
            {
                int colorcount = 0;
                foreach (int num in numbers)
                {
                    colorcount += cardcounts[color][num.ToString()];
                    if(colorcount >= 7){
                        return true;
                    }
                }
            }

        }
        if (whatphase == 9)
        {
            bool foundbig = false;
            bool foundsmall = false;
            foreach (int num in numbers)
            {
                if (checkset(cardcounts, num, 7) == true)
                {
                    return true;
                }
                if (checkset(cardcounts, num, 5))
                {
                    if ((foundbig == true) || (foundsmall == true))
                    {
                        return true;
                    }
                    foundbig = true;
                }
                else if (checkset(cardcounts, num, 2))
                {
                    if (foundbig == true)
                    {
                        return true;
                    }
                    foundsmall = true;
                }                
            }
        }
        if (whatphase == 10)
        {
            bool foundbig = false;
            bool foundsmall = false;
            foreach (int num in numbers)
            {
                if (checkset(cardcounts, num, 8) == true)
                {
                    return true;
                }
                if (checkset(cardcounts, num, 5))
                {
                    if ((foundbig == true) || (foundsmall == true))
                    {
                        return true;
                    }
                    foundbig = true;
                }
                else if (checkset(cardcounts, num, 3))
                {
                    if (foundbig == true)
                    {
                        return true;
                    }
                    foundsmall = true;
                }                
            }
        }
        else//1 set of 5 + 1 set of 3
        {

        }
        return false;
    }
    bool checkset(Dictionary<string, Dictionary<string, int>> cardcounts, int setofwhat, int setlength)
    {
        Dictionary<string, Dictionary<string, int>> copycounts = new Dictionary<string, Dictionary<string, int>>();
        foreach (string color in colors)
        {
            copycounts[color] = new Dictionary<string, int>();
            foreach (int num in numbers)
            {
                copycounts[color][num.ToString()] = 0;
            }
        }
        int total = 0;
        foreach (string color in colors)
        {
            int pretotal = total;
            total += cardcounts[color][setofwhat.ToString()];
            if (total >= setlength)
            {
                copycounts[color][setofwhat.ToString()] += setlength - pretotal;//total - pretotal;
                foreach (string newcolor in colors)
                {
                    cardcounts[newcolor][setofwhat.ToString()] -= copycounts[newcolor][setofwhat.ToString()];
                }
                return true;
            }   
            copycounts[color][setofwhat.ToString()] += cardcounts[color][setofwhat.ToString()];
        }
        return false;
    }
    bool checkrun(Dictionary<string, Dictionary<string, int>> cardcounts, int quantity)
    {
        int runcounter = 0;
        foreach (int num in numbers)
        {
            bool found = false;
            foreach (string color in colors)
            {
                if (cardcounts[color][num.ToString()] > 0)
                {
                    cardcounts[color][num.ToString()] -= 1;
                    found = true;
                    break;
                }
            }
            if (found == true)
            {
                runcounter++;
                if (runcounter == quantity)
                {
                    return true;
                }
            }
        }
        return false;
    }
    //GameObject generaterandomcard()
    (string, string) generaterandomcard()
    {
        string randomColor = colors[Random.Range(0, colors.Length)];
        string randomNumber = numbers[Random.Range(0, numbers.Length)].ToString();
        return (randomColor, randomNumber);
    }
    /*void hideallplayercards(){
        for(int y = 0; y < actual_players; y++){
            for(int x = 0; x < playerhands[y].Count; x++){
                        playerhands[y][x].SetActive(false);
            }
        }
    }*/
    void displayplayercards(int playerid){
        for(int x = 0; x < playerhands[playerid].Count; x++){
                playerhands[playerid][x].SetActive(true);
        }
    }
    public void endround(){
        hasdiscard = false;
        hasdraw = false;
        playerturn = 0;
        roundtext.text = "Round " + (round + 1).ToString();
        turntext.text = "Player " + (playerturn + 1).ToString() + "'s turn";
        round++;
    }
    public void endturn(){
        if(!hasdraw){
            return;
        }
        if(!hasdiscard){
            return;
        }
        if(playerturn == (actual_players-1)){   
            playerturn = 0;
        }
        else{
            playerturn++;
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
    void drawcreatecard(){//Determines a random card, creates an object of it, and makes sure it has appeared
        if(!hasdraw){
            var randomcard = generaterandomcard();
            giveplayercard(playerturn, randomcard.Item1, randomcard.Item2);
            hasdraw = true;
        }
    }                                               
    void removecard(int owner, int whichcard){
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
        if(hasdiscard){
            return;
        }
        //Debug.Log(whichcard);
        Destroy(discardpile);
        GameObject cardtomove = playerhands[playerturn][whichcard];
        cardtomove.transform.position = DISCARD_POSITION;
        discardpile = cardtomove;
        card cardscript = cardtomove.GetComponent<card>();
        cardtomove.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        cardtomove.GetComponentInChildren<Button>().onClick.AddListener(() => {
            if (!hasdraw) {
                Destroy(cardtomove);
                giveplayercard(playerturn, cardscript.color, cardscript.type);
                hasdraw = true;
            }
        ;
        });
        playerhands[playerturn].RemoveAt(whichcard);
        for(int i = whichcard; i < playerhands[playerturn].Count; i++){
            int newcardindex = i;
            GameObject tempcard = playerhands[playerturn][newcardindex];
            tempcard.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            tempcard.GetComponentInChildren<Button>().onClick.AddListener(() => removecard(playerturn, newcardindex));
            tempcard.transform.position = new Vector3(tempcard.transform.position.x - CARD_GAP_X, tempcard.transform.position.y, tempcard.transform.position.z);//= new Vector3(-8.15f + (1.5f * (playerhands[playerturn].Count - 1)), -5.25f, 0f);
        }
        hasdiscard = true;
    }
    GameObject giveplayercard(int playerid, string color, string num)
    {
        GameObject newboardcard = Instantiate(allpossiblecards[color][num]);
        newboardcard.GetComponent<card>().owner = playerid;
        int beforecardcount = playerhands[playerid].Count;
        Vector3 cardpos = new Vector3(CARD_START_X + CARD_GAP_X*beforecardcount,  CARD_START_Y + CARD_GAP_Y * playerid, 0f);
        newboardcard.transform.position = cardpos;
        playerhands[playerid].Add(newboardcard);
        int newcardindex = playerhands[playerid].Count - 1;
        newboardcard.GetComponentInChildren<Button>().onClick.AddListener(() => removecard(playerid, newcardindex));
        
        return null;
    }
    /*void printcardstatus(){
        string response = "";
        for(int y = 0; y < actual_players; y++){
            response += "Player " + (y+1) + ":";
            for(int x = 0; x < playerhands[y].Count; x++){
                    //if(playerhands[y][x] != null){
                        card tempcard = playerhands[y][x].GetComponent<card>();
                        response += tempcard.color + " " + tempcard.type;
                        if(x != (playerhands[y].Count-1)){
                            response += ", ";
                        }
                //}   
            }
            response += "\n";
        }
        Debug.Log(response);
    }*/
}
