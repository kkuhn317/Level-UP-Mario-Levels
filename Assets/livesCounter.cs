using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class livesCounter : MonoBehaviour
{

    public bool oldNumber = false;


    // Start is called before the first frame update
    void Start()
    {
        int livesNum = globalVariables.lives;
        if (oldNumber) livesNum += 1;
        GetComponent<Text>().text = livesNum.ToString();
        if (oldNumber) {
            Invoke("PlaySound", 0.5f);
            Invoke("GoBackToLevel", 2);
        }
    }

    void GoBackToLevel() {
        SceneManager.LoadScene(globalVariables.levelscene);
    }

    void PlaySound() {
        GetComponent<AudioSource>().Play();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
