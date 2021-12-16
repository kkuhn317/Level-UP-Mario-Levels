﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{

    public bool enemyBounce = false;
    public float playerBouncePower = 10;
    public float enemyBouncePower = 25;

    public void Bounce () {

        GetComponent<Animator>().SetTrigger("Bounce");
        GetComponent<AudioSource>().Play();

    }

    private void OnCollisionEnter2D(Collision2D other) {
        
        Vector2 impulse = Vector2.zero;

        int contactCount = other.contactCount;
        for(int i = 0; i < contactCount; i++) {
            var contact = other.GetContact(i);
            impulse += contact.normal * contact.normalImpulse;
            impulse.x += contact.tangentImpulse * contact.normal.y;
            impulse.y -= contact.tangentImpulse * contact.normal.x;
        }

        if (impulse.y < 0) {
            if (other.gameObject.tag == "Player") {
                Playerbetter playerScript = other.gameObject.GetComponent<Playerbetter>();
                playerScript.Jump();
                other.gameObject.GetComponent<Rigidbody2D>().velocity += new Vector2(0, playerBouncePower);
                Bounce();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Enemy" && enemyBounce && other.transform.position.y > transform.position.y && other.transform.position.x > transform.position.x - 1 && other.transform.position.x < transform.position.x  + 1) {
            other.GetComponent<EnemyAi>().velocity = new Vector2(other.GetComponent<EnemyAi>().velocity.x, enemyBouncePower);
            Bounce();
        }


    }
}
