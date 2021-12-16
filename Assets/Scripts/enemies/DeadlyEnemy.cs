using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadlyEnemy : MonoBehaviour
{

    public AudioClip kickSound;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            Playerbetter playerscript = other.gameObject.GetComponent<Playerbetter>();

            if (playerscript.starPower) {
                GetComponent<AudioSource>().PlayOneShot(kickSound);
                GetComponent<EnemyAi>().KnockAway(other.transform.position.x > transform.position.x);
            } else {
                playerscript.damageMario();
            }
            
        }

    }
}
