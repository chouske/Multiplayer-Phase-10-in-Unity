using UnityEngine;
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
    int[] numbers = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12};
    public TMP_Text turntext;
    int max_players = 6;
    int actual_players = 4;
    int max_cards = 11;
    int round = 0;
    int playerturn = 0;
    bool hasdrew = false;
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
        Vector3 cardpos = new Vector3(-8.15f, -5.25f, 0f);
        for(int y = 0; y < actual_players; y++){
            for(int x = 0; x < 10; x++){
                    GameObject newboardcard = Instantiate(generaterandomcard());
                    newboardcard.transform.position = cardpos;
                    cardpos.x = cardpos.x + 1.5f;
                    playerhands[y].Add(newboardcard);
                    newboardcard.SetActive(false);
            }
            cardpos.x = -8.15f;
        }
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
    void endround(){
        playerturn = 0;
        round++;
    }
    void endturn(){
        if(playerturn == (actual_players-1)){   
            playerturn = 0;
        }
        else{
            playerturn++;
        }
        turntext.text = "Player " + (playerturn + 1).ToString() + "'s turn";
        hasdrew = false;
        hideallplayercards();
        displayplayercards(playerturn);
    }
    void drawcreatecard(){
        if(!hasdrew){
            //playerhands[playerturn].Add(Instantiate(allpossiblecards[Random.Range(0, allpossiblecards.Count)]));
            playerhands[playerturn].Add(Instantiate(generaterandomcard()));
            GameObject createdcard = playerhands[playerturn][playerhands[playerturn].Count-1];
            createdcard.GetComponent<card>().owner = playerturn;
            hasdrew = true;
            createdcard.transform.position = new Vector3(-8.15f + (1.5f * (playerhands[playerturn].Count - 1)), -5.25f, 0f);
        }
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
