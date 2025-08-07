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
    string[] colors = {"red", "blue", "green", "yellow"};
    //int[] numbers = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12};
     //string[] colors = {"blue"};
    int[] numbers = {1, 2, 3, 4};
    public TMP_Text turntext;
    public TMP_Text roundtext;
    float CARD_START_X = -10.15f;
    float CARD_START_Y = -5.25f;
    float CARD_GAP_X = 1.5f;
    float CARD_GAP_Y = 3.0f;
    int max_players = 6;
    int actual_players = 4;
    int max_cards = 11;
    int round = 0;
    int playerturn = 0;
    bool hasdiscard = false;
    bool hasdraw = false;
    List<GameObject> allpossiblecards;
    List<GameObject>[] playerhands;
    void Start()
    {
        allpossiblecards = new List<GameObject>();
        playerhands  = new List<GameObject>[actual_players];
        for (int i = 0; i < actual_players; i++)
            {
                playerhands[i] = new List<GameObject>();
            }
            foreach(string color in colors){
                foreach(int num in numbers){
                    //Debug.Log(color + num.ToString());
                    if((num < 5)){
                        GameObject card = Resources.Load<GameObject>("Prefabs/" + color + num.ToString());
                        allpossiblecards.Add(card);
                    }
                }
            }
            giveplayersstartingcards();
            displayplayercards(0);
    }
    void giveplayersstartingcards(){
        Vector3 cardpos = new Vector3(CARD_START_X,  CARD_START_Y, 0f);
        for (int y = 0; y < actual_players; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                int index = x; //Needed because of weirdness with lambdas
                int owner = y;
                GameObject newboardcard = Instantiate(generaterandomcard());
                newboardcard.GetComponent<card>().owner = owner;
                //Debug.Log("y: " + y + " x: " + x);
                newboardcard.GetComponentInChildren<Button>().onClick.AddListener(() => removecard(owner, index));
                newboardcard.transform.position = cardpos;
                cardpos.x = cardpos.x + CARD_GAP_X;
                playerhands[y].Add(newboardcard);
                //  newboardcard.SetActive(false);
            }
            cardpos.x = CARD_START_X;
            cardpos.y = cardpos.y + CARD_GAP_Y;
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
            Debug.Log(cardscript.color + cardscript.type);
        }
        if (checkphase(cardcounts, 2) == true)
        {
            Debug.Log("phase 2 complete!");
        }
    }
    bool checkphase(Dictionary<string, Dictionary<string, int>> cardcounts, int whatphase)
    {
        if (whatphase == 1)//2 sets of 3
        {
            int totalsets = 0;
            foreach (string color in colors)
            {
                foreach (int num in numbers)
                {
                    if (checkset(cardcounts, color, num, 3) == true)
                    {
                        totalsets++;
                        if (totalsets == 2)
                        {
                            return true;
                        }
                    }
                }
            }

        }
        if (whatphase == 2)//1 set of 3 + 1 run of 4
        {
            bool hasrun = checkrun(cardcounts, 4);
            if (hasrun == false)
            {
                return false;
            }
            int totalsets = 0;
            foreach (string color in colors)
            {
                foreach (int num in numbers)
                {
                    if (checkset(cardcounts, color, num, 3) == true)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    bool checkset(Dictionary<string, Dictionary<string, int>> cardcounts, string color, int num, int quantity)
    {
        if (cardcounts[color][num.ToString()] >= quantity)
        {
            return true;
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
    GameObject generaterandomcard(){
        return allpossiblecards[Random.Range(0, allpossiblecards.Count)];
    }
    void hideallplayercards(){
        for(int y = 0; y < actual_players; y++){
            for(int x = 0; x < playerhands[y].Count; x++){
                        playerhands[y][x].SetActive(false);
            }
        }
    }
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
    GameObject getcard(int playerid, int index)
    {
        return playerhands[playerturn][index];
    }
    void drawcreatecard(){//Determines a random card, creates an object of it, and makes sure it has appeared
        if(!hasdraw){
            //playerhands[playerturn].Add(Instantiate(allpossiblecards[Random.Range(0, allpossiblecards.Count)]));
            playerhands[playerturn].Add(Instantiate(generaterandomcard()));
            int newcardindex = playerhands[playerturn].Count - 1;
            GameObject createdcard = playerhands[playerturn][newcardindex]; // This just gets what was just made
            createdcard.GetComponentInChildren<Button>().onClick.AddListener(() => removecard(playerturn, newcardindex));
            createdcard.GetComponent<card>().owner = playerturn;
            hasdraw = true;
            createdcard.transform.position = new Vector3(CARD_START_X + (CARD_GAP_X * (playerhands[playerturn].Count - 1)), CARD_START_Y + CARD_GAP_Y*playerturn, 0f);
        }
    }                                               
    void removecard(int owner, int whichcard){
        if (owner != playerturn)
        {
            Debug.Log("return!");
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
        Destroy(playerhands[playerturn][whichcard]);
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
    void printcardstatus(){
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
    }
}
