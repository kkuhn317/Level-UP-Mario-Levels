using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class deadMario : MonoBehaviour
{

    public float gravity;
    public Vector2 velocity;

    public GameObject gameOverScreen;

    // Start is called before the first frame update
    void Start()
    {
        globalVariables.lives -= 1;

        if (globalVariables.lives == 0)
        {
            Invoke("showGameOver", 1);
        }
        else
        {
            globalVariables.levelscene = SceneManager.GetActiveScene().buildIndex;
            Invoke("ToLivesScene", 1);
        }
        
    }



    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.localPosition;
        Vector3 scale = transform.localScale;

        pos.y += velocity.y * Time.deltaTime;

        velocity.y -= gravity * Time.deltaTime;

        transform.localPosition = pos;
    }

    void showGameOver()
    {
        // kill all music
        GameObject[] music = GameObject.FindGameObjectsWithTag("Music");
        foreach (GameObject m in music)
        {
            Destroy(m);
        }
        // instantiate the game over screen
        gameOverScreen = Instantiate(gameOverScreen, transform.position, Quaternion.identity);
        // go to title screen after 3 seconds
        Invoke("ToTitleScene", 5);
    }

    void ToTitleScene()
    {
        SceneManager.LoadScene("Title");
    }

    void ToLivesScene() {
        SceneManager.LoadScene("Lives");
    }
}
