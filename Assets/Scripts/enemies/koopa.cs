using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class koopa : MonoBehaviour
{

    public enum EnemyState {
        walking,
        inShell,
        movingShell
    }

    public EnemyState state = EnemyState.walking;
    public float walkingSpeed = 2;
    public float movingShellSpeed = 10;
    private Animator animator;
    private EnemyAi enemyAi;

    private AudioSource audioSource;

    public AudioClip kickSound;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        enemyAi = GetComponent<EnemyAi>();
        audioSource = GetComponent<AudioSource>();

        switch(state) {
            case EnemyState.walking:
                ToWalking();
                break;
            case EnemyState.inShell:
                ToInShell();
                break;
            case EnemyState.movingShell:
                ToMovingShell(enemyAi.isWalkingLeft);
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            Playerbetter playerscript = other.gameObject.GetComponent<Playerbetter>();
            Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();

            if (playerscript.starPower) {
                audioSource.PlayOneShot(kickSound);
                GetComponent<EnemyAi>().KnockAway(other.transform.position.x > transform.position.x);
                return;
            }

            if (playerscript.powerupState == Playerbetter.PowerupState.small) {
                if (rb.position.y - 0.2 > transform.position.y - 0.3) {
                    onStomped(other.gameObject);
                } else {
                   onhitPlayer(other.gameObject);
                }
            } else {
                if (rb.position.y - 0.7 > transform.position.y - 0.3) {
                    onStomped(other.gameObject);
                } else {
                    onhitPlayer(other.gameObject);
                }
            }
        } else if (other.gameObject.tag == "Enemy" && state == EnemyState.movingShell) {
            other.gameObject.GetComponent<EnemyAi>().KnockAway(enemyAi.isWalkingLeft);
            audioSource.PlayOneShot(kickSound);
        }
    }

    private void ToWalking() {
        state = EnemyState.walking;
        animator.SetBool("inShell", false);
        enemyAi.velocity = new Vector2(walkingSpeed, enemyAi.velocity.y);
        enemyAi.checkEnemyCollision = true;
    }

    private void ToInShell() {
        state = EnemyState.inShell;
        animator.SetBool("inShell", true);
        enemyAi.velocity = new Vector2(0, enemyAi.velocity.y);
        enemyAi.checkEnemyCollision = true;
    }

    private void ToMovingShell(bool direction) {
        state = EnemyState.movingShell;
        enemyAi.isWalkingLeft = direction;
        animator.SetBool("inShell", true);
        enemyAi.velocity = new Vector2(movingShellSpeed, enemyAi.velocity.y);
        enemyAi.checkEnemyCollision = false;
    }

    private void onStomped(GameObject player) {
        Playerbetter playerScript = player.GetComponent<Playerbetter>();
        switch (state) {
            case EnemyState.walking:
                playerScript.Jump();
                audioSource.Play();
                ToInShell();
                break;
            case EnemyState.inShell:
                audioSource.PlayOneShot(kickSound);
                ToMovingShell(player.transform.position.x > transform.position.x);
                break;
            case EnemyState.movingShell:
                playerScript.Jump();
                audioSource.Play();
                ToInShell();
                break;
        }

    }

    private void onhitPlayer(GameObject player) {
        Playerbetter playerScript = player.GetComponent<Playerbetter>();
        switch (state) {
            case EnemyState.walking:
                playerScript.damageMario();
                break;
            case EnemyState.inShell:
                ToMovingShell(player.transform.position.x > transform.position.x);
                break;
            case EnemyState.movingShell:
                playerScript.damageMario();
                break;
        }
    }
}
