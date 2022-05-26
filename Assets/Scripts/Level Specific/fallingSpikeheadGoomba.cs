using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fallingSpikeheadGoomba : MonoBehaviour
{
    public bool deadly = true;

    public AudioClip kickSound;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if (!GetComponent<EnemyAi>().stayStill && GetComponent<EnemyAi>().velocity.y == 0) {
            deadly = false;
        } else {
            deadly = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            Playerbetter playerscript = other.gameObject.GetComponent<Playerbetter>();

            if (playerscript.starPower || !deadly) {
                GetComponent<AudioSource>().PlayOneShot(kickSound);
                GetComponent<EnemyAi>().KnockAway(other.transform.position.x > transform.position.x);
            } else {
                playerscript.damageMario();
            }
            
        }

    }

}
