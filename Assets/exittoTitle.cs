using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class exittoTitle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if pressing escape, go to title screen
        if (Input.GetKeyDown(KeyCode.Escape)) {
            // kill music
            GameObject[] musics = GameObject.FindGameObjectsWithTag("Music");
            foreach(GameObject music in musics) {
                Destroy(music);
            }
            SceneManager.LoadScene("Title");
        }
    }
}
