using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class titlescreen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void goToScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    public void quitGame() {
        Application.Quit();
    }
}
