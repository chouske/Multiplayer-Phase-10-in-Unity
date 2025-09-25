using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class winnertext : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TMP_Text thetext = GetComponent<TMP_Text>();
        thetext.text = GameData.winnerName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
