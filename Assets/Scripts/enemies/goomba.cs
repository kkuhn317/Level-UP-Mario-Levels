﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class goomba : EnemyAi
{

    public enum EnemyState {
        walking,
        crushed
    }

    public EnemyState state = EnemyState.walking;

    public bool stompable = true;

    private bool shouldDie = false;
    private float deathTimer = 0;
    public float timeBeforeDestroy = 1.0f;

    protected override void Update() {
        base.Update();
        CheckCrushed();
    }

    protected override void hitByStomp(GameObject player)
    {
        Playerbetter playerscript = player.GetComponent<Playerbetter>();
        playerscript.Jump();
        Crush();
    }

    public void Crush () {

        GetComponent<AudioSource>().Play();
        
        if (!stompable) {
            return;
        }

        state = EnemyState.crushed;
        movement = ObjectMovement.still;

        GetComponent<Animator>().SetBool("isCrushed", true);

        GetComponent<Collider2D>().enabled = false;

        shouldDie = true;

    }

    void CheckCrushed () {

        if (shouldDie) {
            
            if (deathTimer <= timeBeforeDestroy) {

                deathTimer += Time.deltaTime;

            } else {
                
                shouldDie = false;

                Destroy(this.gameObject);
            }
        }
    }

}
