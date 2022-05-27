using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class door : MonoBehaviour
{
    public GameObject destination;
    public bool locked = false;

    public AudioClip unlockSound, openSound, closeSound;

    private GameObject player;

    public GameObject particle;

    private Animator animator;
    private door otherDoor;

    private bool inUse = false;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        otherDoor = destination.GetComponent<door>();
    }

    void findPlayer() {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (inUse) {
            return;
        }

        if (player == null) {
            findPlayer();
            if (player == null) {
                return;
            }
        }

        if (!player.GetComponent<Playerbetter>()) {  // dead mario doesn't have this
            return;
        }

        Vector2 playerPos = player.transform.position;
        Playerbetter playerScript = player.GetComponent<Playerbetter>();
        if (playerScript.powerupState == Playerbetter.PowerupState.small) {
            playerPos.y += 0.5f;
        }

        float xdist = Mathf.Abs(playerPos.x - transform.position.x);
        float ydist = Mathf.Abs(playerPos.y - transform.position.y);

        // if player is at the door
        if (xdist < 0.4 && ydist < 0.1 && playerScript.onGround) {
            // if pressing up
            if (Input.GetAxisRaw("Vertical") > 0)
            {
                //print("attempting to open door");
                // if the door is locked
                if (locked) {
                    // if player has the key
                    if (globalVariables.keys.Count > 0) {
                        inUse = true;
                        Unlock();
                    }
                } else {
                    inUse = true;
                    Open();
                }
            }
        }
    }

    void FreezePlayer() {
        player.GetComponent<Rigidbody2D>().simulated = false;
        player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        Animator playerAnimator = player.GetComponent<Animator>();
        playerAnimator.SetBool("isJumping", false);
        playerAnimator.SetBool("isRunning", false);
        playerAnimator.SetBool("isSkidding", false);

        // disable all scripts
        foreach (MonoBehaviour script in player.GetComponents<MonoBehaviour>()) {
            script.enabled = false;
        }
    }
    void UnfreezePlayer() {
        player.GetComponent<Rigidbody2D>().simulated = true;
        
        // enable all scripts
        foreach (MonoBehaviour script in player.GetComponents<MonoBehaviour>()) {
            script.enabled = true;
        }
    }

    void Unlock() {
        locked = false;
        GameObject usedKey = globalVariables.keys[0];
        globalVariables.keys.RemoveAt(0);
        Destroy(usedKey);
        GetComponent<AudioSource>().PlayOneShot(unlockSound);
        FreezePlayer();
        animator.SetTrigger("Unlock");
        spawnParticles();
        Invoke("Open", 0.5f);

        // Unlock other door if needed
        if (otherDoor) {
            if (otherDoor.locked) {
                otherDoor.locked = false;
                destination.GetComponent<Animator>().SetTrigger("Unlock");
            }
        }
    }

    void spawnParticles()
    {
        // spawn 2 of them at the top corners of the door
        GameObject newParticle1 = Instantiate(particle, transform.position + new Vector3(-0.5f, 1, 0), Quaternion.identity);
        GameObject newParticle2 = Instantiate(particle, transform.position + new Vector3(0.5f, 1, 0), Quaternion.identity);


        // make the particles move outwards at constant speed
        newParticle1.GetComponent<starMoveOutward>().direction = new Vector2(-1, 1);
        newParticle2.GetComponent<starMoveOutward>().direction = new Vector2(1, 1);
        newParticle1.GetComponent<starMoveOutward>().speed = 2f;
        newParticle2.GetComponent<starMoveOutward>().speed = 2f;


    }

    void Open() {
        GetComponent<AudioSource>().PlayOneShot(openSound);
        FreezePlayer();
        animator.SetTrigger("Open");
        Invoke("Teleport", 0.5f);
        Invoke("Close", 1);

        // Open and close the other door
        if (otherDoor) {
            otherDoor.inUse = true;
            destination.GetComponent<Animator>().SetTrigger("Open");
            otherDoor.Invoke("Close", 1);
        }

    }

    void Teleport() {
        player.transform.position = destination.transform.position;
        if (player.GetComponent<Playerbetter>().powerupState == Playerbetter.PowerupState.small) {
            player.transform.position -= new Vector3(0, 0.5f, 0);
        }
        UnfreezePlayer();
    }

    void Close() {
        animator.SetTrigger("Close");
        GetComponent<AudioSource>().PlayOneShot(closeSound);
        Invoke("Ready", 0.5f);
    }

    void Ready() {
        inUse = false;
    }
}
