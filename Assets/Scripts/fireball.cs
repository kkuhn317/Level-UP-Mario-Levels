using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fireball : MonoBehaviour
{

    public firePower firePowerScript;

    public AudioClip kickSound;
    public AudioClip hitWallSound;

    bool hitEnemy = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.gameObject.tag == "Enemy" && !hitEnemy)
        {
            if (other.gameObject.GetComponent<EnemyAi>().canBeFireballed) {
                hitEnemy = true;
                bool direction = GetComponent<ObjectPhysics>().movingLeft;
                other.gameObject.GetComponent<EnemyAi>().KnockAway(direction);
                GetComponent<AudioSource>().PlayOneShot(kickSound);
                deleteFireball();
            } else {
                hitWall();
            }
        }
    }

    public void hitWall()
    {
        GetComponent<AudioSource>().PlayOneShot(hitWallSound);
        deleteFireball();
    }

    public void deleteFireball()
    {
        // TODO: make explosion animation
        if (firePowerScript)
            firePowerScript.onFireballDestroyed();
        
        // let sounds play before deleting
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<ObjectPhysics>().enabled = false;
        Destroy(gameObject, 2);
    }

    


}
