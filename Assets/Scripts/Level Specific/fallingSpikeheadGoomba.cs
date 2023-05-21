using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fallingSpikeheadGoomba : EnemyAi
{
    public bool deadly = true;

    protected override void Update()
    {
        base.Update();

        if (movement != ObjectMovement.still && velocity.y == 0) {
            deadly = false;
        } else {
            deadly = true;
        }
    }

    protected override void hitByPlayer(GameObject player)
    {
        Playerbetter playerscript = player.GetComponent<Playerbetter>();

        if (playerscript.starPower || !deadly) {
            KnockAway(player.transform.position.x > transform.position.x);
        } else {
            playerscript.damageMario();
        }
    }

    public void fallDown() {
        movement = ObjectMovement.sliding;
    }

}
