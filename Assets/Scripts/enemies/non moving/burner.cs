using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class burner : MonoBehaviour
{

    public float offtime = 0.5f;
    public float ontime = 0.5f;


    // Start is called before the first frame update
    void Start()
    {
        Invoke("TurnOn", offtime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TurnOn()
    {
        GetComponent<Animator>().SetTrigger("On");
        GetComponent<AudioSource>().Play();
        Invoke("TurnOff", ontime);
    }

    void TurnOff()
    {
        GetComponent<Animator>().SetTrigger("Off");
        Invoke("TurnOn", offtime);
    }
}
