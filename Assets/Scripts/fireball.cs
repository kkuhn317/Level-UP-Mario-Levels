using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fireball : MonoBehaviour
{

    public firePower firePowerScript;

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
        print("fireball hit");
        if (other.gameObject.tag == "Enemy")
        {
            bool direction = GetComponent<ObjectPhysics>().movingLeft;
            other.gameObject.GetComponent<EnemyAi>().KnockAway(direction);
            deleteFireball();
        }
    }

    public void deleteFireball()
    {
        // TODO: make explosion animation
        if (firePowerScript)
            firePowerScript.onFireballDestroyed();
        Destroy(gameObject);
    }

    


}
