using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class powerup : ObjectPhysics
{

    public GameObject newMarioState;
    public int powerLevel = 1;
    public float starTime = -1;

    public GameObject starMusicOverride;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        enabled = false;

    }

    void OnBecameVisible() {
        
        enabled = true;
    }


    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            if (starTime > 0) {
                other.GetComponent<Playerbetter>().startStarPower(starTime);
                GameObject starSong = Instantiate(starMusicOverride);
                starSong.GetComponent<musicOverride>().stopPlayingAfterTime(starTime);
            }
            if (newMarioState) {
                Playerbetter player = other.GetComponent<Playerbetter>();
                Playerbetter.PowerupState playerState = player.powerupState;
                if (canGetPowerup(playerState)) 
                    other.GetComponent<Playerbetter>().ChangePowerup(newMarioState);
            }
            GetComponent<AudioSource>().Play();
            GetComponent<Collider2D>().enabled = false;
            GetComponent<SpriteRenderer>().enabled = false;
            Destroy(gameObject, 2);
        }
    }

    private bool canGetPowerup(Playerbetter.PowerupState state) {
        if (state == Playerbetter.PowerupState.small)
            return true;
        else {
            if (powerLevel >= 2)
                return true;
            else
                return false;
        }
    }

}
