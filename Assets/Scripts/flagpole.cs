using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class flagpole : MonoBehaviour
{
    public GameObject cutsceneMario;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            cutsceneMario.transform.localPosition = new Vector3(-0.25f, 0, 0);
            cutsceneMario.transform.position = new Vector3(cutsceneMario.transform.position.x, other.transform.position.y, 0);
            Destroy(other.gameObject);
            foreach(GameObject music in GameObject.FindGameObjectsWithTag("Music")) {
                Destroy(music);
            }
            foreach(GameObject music in GameObject.FindGameObjectsWithTag("MusicOverride")) {
                Destroy(music);
            }
            GetComponent<PlayableDirector>().Play();
        }
    }
}
