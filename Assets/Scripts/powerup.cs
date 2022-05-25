using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class powerup : MonoBehaviour
{

    public float gravity;
    public Vector2 velocity;
    public bool isWalkingLeft = true;

    public LayerMask floorMask;
    public LayerMask wallMask;

    public GameObject newMarioState;
    public float starTime = -1;

    public GameObject starMusicOverride;

    public enum PowerupMovement {
        still,
        sliding,
        bouncing    // TODO
    }

    public PowerupMovement movement = PowerupMovement.sliding;

    private enum PowerupState {
        
        walking,
        falling
    }

    private PowerupState state = PowerupState.falling;

    // Start is called before the first frame update
    void Start()
    {
        enabled = false;

        if (movement != PowerupMovement.still)
            Fall();
    }

    // Update is called once per frame
    void Update()
    {
        if (movement != PowerupMovement.still)
            UpdatePosition ();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            if (starTime > 0) {
                other.GetComponent<Playerbetter>().startStarPower(starTime);
                GameObject starSong = Instantiate(starMusicOverride);
                starSong.GetComponent<musicOverride>().stopPlayingAfterTime(starTime);
            }
            if (newMarioState) {
                other.GetComponent<Playerbetter>().ChangePowerup(newMarioState);
            }
            GetComponent<AudioSource>().Play();
            GetComponent<Collider2D>().enabled = false;
            GetComponent<SpriteRenderer>().enabled = false;
            Destroy(gameObject, 2);
        }
    }

    void UpdatePosition () {

        Vector3 pos = transform.localPosition;
        Vector3 scale = transform.localScale;

        if (state == PowerupState.falling) {

            pos.y += velocity.y * Time.deltaTime;

            velocity.y -= gravity * Time.deltaTime;
        }

        if (isWalkingLeft) {

            pos.x -= velocity.x * Time.deltaTime;

            scale.x = -1;

        } else {

            pos.x += velocity.x * Time.deltaTime;

            scale.x = 1;
        }

        if (velocity.y <= 0)
            pos = CheckGround (pos);

        CheckWalls (pos, scale.x);

        transform.localPosition = pos;
    }

    Vector3 CheckGround (Vector3 pos) {

        Vector2 originLeft = new Vector2 (pos.x - 0.5f + 0.2f, pos.y - .5f);
        Vector2 originMiddle = new Vector2 (pos.x, pos.y - .5f);
        Vector2 originRight = new Vector2 (pos.x + 0.5f - 0.2f, pos.y - .5f);

        RaycastHit2D groundLeft = Physics2D.Raycast (originLeft, Vector2.down, velocity.y * Time.deltaTime, floorMask);
        RaycastHit2D groundMiddle = Physics2D.Raycast (originMiddle, Vector2.down, velocity.y * Time.deltaTime, floorMask);
        RaycastHit2D groundRight = Physics2D.Raycast (originRight, Vector2.down, velocity.y * Time.deltaTime, floorMask);

        if (groundLeft.collider != null || groundMiddle.collider != null || groundRight.collider != null) {

            RaycastHit2D hitRay = groundLeft;

            if (groundLeft) {
                hitRay = groundLeft;
            } else if (groundMiddle) {
                hitRay = groundMiddle;
            } else if (groundRight) {
                hitRay = groundRight;
            }

            //pos.y = hitRay.collider.bounds.center.y + hitRay.collider.bounds.size.y / 2 + .5f;
            pos.y = hitRay.point.y + .5f;
            velocity.y = 0;

            state = PowerupState.walking;

        } else {

            if (state != PowerupState.falling) {

                Fall ();
            }
        }

        return pos;
    }

    void CheckWalls (Vector3 pos, float direction) {

        Vector2 originTop = new Vector2 (pos.x + direction * 0.4f, pos.y + .5f - 0.2f);
        Vector2 originMiddle = new Vector2 (pos.x + direction * 0.4f, pos.y);
        Vector2 originBottom = new Vector2 (pos.x + direction * 0.4f, pos.y - .5f + 0.2f);

        RaycastHit2D wallTop = Physics2D.Raycast (originTop, new Vector2 (direction, 0), velocity.x * Time.deltaTime, wallMask);
        RaycastHit2D wallMiddle = Physics2D.Raycast (originMiddle, new Vector2 (direction, 0), velocity.x * Time.deltaTime, wallMask);
        RaycastHit2D wallBottom = Physics2D.Raycast (originBottom, new Vector2 (direction, 0), velocity.x * Time.deltaTime, wallMask);

        if (wallTop.collider != null || wallMiddle.collider != null || wallBottom.collider != null) {

            RaycastHit2D hitRay = wallTop;

            if (wallTop) {
                hitRay = wallTop;
            } else if (wallMiddle) {
                hitRay = wallMiddle;
            } else if (wallBottom) {
                hitRay = wallBottom;
            }

            isWalkingLeft = !isWalkingLeft;
        }
    }

    void OnBecameVisible() {
        
        enabled = true;
    }

    void Fall () {

        velocity.y = 0;

        state = PowerupState.falling;

    }
}
