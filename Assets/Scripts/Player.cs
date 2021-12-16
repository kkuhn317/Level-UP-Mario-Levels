using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{

    public float jumpVelocity;
    public float goombabounce;

    public float springbounce;

    public Vector2 velocity;
    public float gravity;
    public LayerMask wallMask;
    public LayerMask floorMask;

    private bool InsideEnemy = false;

    private float bounceamount = 0;

    private bool inCrouchState = false;

    private bool walk, walk_left, walk_right, jump, run, crouch;

    BoxCollider2D m_Collider;

    public enum PlayerState {

        jumping,
        idle,
        walking,
        bouncing,

    }

    public enum PowerupState {

        small,
        big
    }

    private PlayerState playerState = PlayerState.idle;
    private PowerupState powerupState = PowerupState.small;

    private bool grounded = false;
    private bool bounce = false;

    // Start is called before the first frame update
    void Start()
    {
        m_Collider = GetComponent<BoxCollider2D>();
        Fall ();
    }

    // Update is called once per frame
    void Update()
    {
        CheckPlayerInput();

        UpdatePlayerPosition();

        UpdateAnimationStates();

        UpdateBoxCollider();
    }

    void UpdatePlayerPosition () {

        Vector3 pos = transform.localPosition;
        Vector3 scale = transform.localScale;

        if ((walk && grounded && !inCrouchState) || (walk && !grounded)) {
            if (walk_left) {

                if(run)
                    pos.x -= velocity.x * Time.deltaTime;
                else
                    pos.x -= velocity.x/2 * Time.deltaTime;

                scale.x = -1;
            }
            
            if (walk_right) {

                if(run)
                    pos.x += velocity.x * Time.deltaTime;
                else
                    pos.x += velocity.x/2 * Time.deltaTime;

                scale.x = 1;
            }       

            pos = CheckWallRays (pos, scale.x);
        }

        if (jump && playerState != PlayerState.jumping && playerState != PlayerState.bouncing) {

            playerState = PlayerState.jumping;

            grounded = false;

            velocity = new Vector2(velocity.x, jumpVelocity);
        }

        if (playerState == PlayerState.jumping) {

            pos.y += velocity.y * Time.deltaTime;

            velocity.y -= gravity * Time.deltaTime;
        }

        if (bounce && playerState != PlayerState.bouncing) {

            playerState = PlayerState.bouncing;
            grounded = false;

            if (Input.GetKey(KeyCode.Space)) {
                velocity = new Vector2 (velocity.x, (bounceamount * 2));
            } else {
                velocity = new Vector2 (velocity.x, bounceamount);
            }
            
        }

        if (playerState == PlayerState.bouncing) {

            pos.y += velocity.y * Time.deltaTime;

            velocity.y -= gravity * Time.deltaTime;
        }


        if (velocity.y <= 0)
            pos = CheckFloorRays (pos);

        if (velocity.y >= 0)
            pos = CheckCeilingRays (pos);

        transform.position = pos;
        transform.localScale = scale;
    }

    void UpdateAnimationStates () {

        if (grounded && !walk && !bounce) {

            GetComponent<Animator>().SetBool("isJumping", false);
            GetComponent<Animator>().SetBool("isRunning", false);

        }

        if (grounded && walk) {

            GetComponent<Animator>().SetBool("isJumping", false);
            GetComponent<Animator>().SetBool("isRunning", true);

        }

        if (playerState == PlayerState.jumping || playerState == PlayerState.bouncing) {

            GetComponent<Animator>().SetBool("isJumping", true);
            GetComponent<Animator>().SetBool("isRunning", false);
        }

        if (powerupState == PowerupState.big) {

            GetComponent<Animator>().SetBool("isBig", true);

        } else if (powerupState == PowerupState.small) {

            GetComponent<Animator>().SetBool("isBig", false);

        }

        if (crouch && powerupState == PowerupState.big && grounded) {

            GetComponent<Animator>().SetBool("isDucking", true);
            inCrouchState = true;

        } else if ((!crouch && grounded) || (powerupState == PowerupState.small)) {

            GetComponent<Animator>().SetBool("isDucking", false);
            inCrouchState = false;

        }

        if(run)
            GetComponent<Animator>().SetFloat("Speed", 1f);
        else
            GetComponent<Animator>().SetFloat("Speed", .5f);

    }

    void CheckPlayerInput () {
        bool input_left = Input.GetKey(KeyCode.LeftArrow);
        bool input_right = Input.GetKey(KeyCode.RightArrow);
        bool input_space = Input.GetKeyDown(KeyCode.Space);
        bool input_shift = Input.GetKey(KeyCode.LeftShift);
        bool input_down = Input.GetKey(KeyCode.DownArrow);

        walk = input_left || input_right;

        walk_left = input_left && !input_right;

        walk_right = !input_left && input_right;
        
        jump = input_space;

        run = input_shift;

        crouch = input_down;
    }

    Vector3 CheckWallRays (Vector3 pos, float direction) {
        Vector2 originTop, originMiddle, originBottom;
        
        if (powerupState == PowerupState.small) {
            originTop = new Vector2 (pos.x + direction *.4f, pos.y + .5f - 0.2f);
            originMiddle = new Vector2 (pos.x + direction * .4f, pos.y);
            originBottom = new Vector2 (pos.x + direction * .4f, pos.y - .5f + 0.2f);
        } else if (inCrouchState) {
            originTop = new Vector2 (pos.x + direction *.4f, pos.y - 0.5f + 0.2f);
            originMiddle = new Vector2 (pos.x + direction * .4f, pos.y - 0.5f);
            originBottom = new Vector2 (pos.x + direction * .4f, pos.y - 1f + 0.2f);
        } else {
            originTop = new Vector2 (pos.x + direction *.4f, pos.y + 1f - 0.2f);
            originMiddle = new Vector2 (pos.x + direction * .4f, pos.y);
            originBottom = new Vector2 (pos.x + direction * .4f, pos.y - 1f + 0.2f);
        }

        RaycastHit2D wallTop = Physics2D.Raycast (originTop, new Vector2 (direction, 0), velocity.x * Time.deltaTime, wallMask);
        RaycastHit2D wallMiddle = Physics2D.Raycast (originMiddle, new Vector2 (direction, 0), velocity.x * Time.deltaTime, wallMask);
        RaycastHit2D wallBottom = Physics2D.Raycast (originBottom, new Vector2 (direction, 0), velocity.x * Time.deltaTime, wallMask);

        if (wallTop.collider != null || wallMiddle.collider != null || wallBottom.collider != null) {

            pos.x -= velocity.x * Time.deltaTime * direction;
        }

        return pos;
    }

    Vector3 CheckFloorRays (Vector3 pos) {

        Vector2 originLeft, originMiddle, originRight;

        if (powerupState == PowerupState.small) {
            originLeft = new Vector2 (pos.x - 0.5f + 0.2f, pos.y - .5f);
            originMiddle = new Vector2 (pos.x, pos.y - .7f);
            originRight = new Vector2 (pos.x + 0.5f - 0.2f, pos.y - .5f);
        } else {
            originLeft = new Vector2 (pos.x - 0.5f + 0.2f, pos.y - 1f);
            originMiddle = new Vector2 (pos.x, pos.y - 1.2f);
            originRight = new Vector2 (pos.x + 0.5f - 0.2f, pos.y - 1f);
        }

        RaycastHit2D floorLeft = Physics2D.Raycast (originLeft, Vector2.down, velocity.y * Time.deltaTime, floorMask);
        RaycastHit2D floorMiddle = Physics2D.Raycast (originMiddle, Vector2.down, velocity.y * Time.deltaTime, floorMask);
        RaycastHit2D floorRight = Physics2D.Raycast (originRight, Vector2.down, velocity.y * Time.deltaTime, floorMask);

        if (floorLeft.collider != null || floorMiddle.collider != null || floorRight.collider != null) {
            
            RaycastHit2D hitRay = floorRight;

            if (floorLeft) {
                hitRay = floorLeft;
            } else if (floorMiddle) {
                hitRay = floorMiddle;
            } else if (floorRight) {
                hitRay = floorRight;
            }

            //if (hitRay.collider.tag == "Enemy" && !InsideEnemy) {
                
            //    bounce = true;

            //    bounceamount = goombabounce;

            //    hitRay.collider.GetComponent<EnemyAi>().Crush();

            //}

            if (hitRay.collider.tag == "Spring") {
                
                bounce = true;

                bounceamount = springbounce;

                pos.y -= .5f;

                hitRay.collider.GetComponent<Spring>().Bounce();

            }

            if (hitRay.collider.tag != "Enemy" || (hitRay.collider.tag == "Enemy" && !InsideEnemy)) {

                playerState = PlayerState.idle;

                grounded = true;

                velocity.y = 0;

                if (powerupState == PowerupState.small) {
                    pos.y = hitRay.collider.bounds.center.y + hitRay.collider.bounds.size.y / 2 + .5f;
                } else {
                    pos.y = hitRay.collider.bounds.center.y + hitRay.collider.bounds.size.y / 2 + 1;
                }

            }

        } else {

            if (playerState != PlayerState.jumping) {

                Fall ();
            }
        }

        return pos;
    }

    Vector3 CheckCeilingRays (Vector3 pos) {

        Vector2 originLeft, originMiddle, originRight;

        if (powerupState == PowerupState.small) {
            originLeft = new Vector2 (pos.x - 0.5f + 0.2f, pos.y + .5f);
            originMiddle = new Vector2 (pos.x, pos.y + .5f);
            originRight = new Vector2 (pos.x + 0.5f - 0.2f, pos.y + .5f);
        } else  if (inCrouchState) {
            originLeft = new Vector2 (pos.x - 0.5f + 0.2f, pos.y);
            originMiddle = new Vector2 (pos.x, pos.y);
            originRight = new Vector2 (pos.x + 0.5f - 0.2f, pos.y);
        } else {
            originLeft = new Vector2 (pos.x - 0.5f + 0.2f, pos.y + 1f);
            originMiddle = new Vector2 (pos.x, pos.y + 1f);
            originRight = new Vector2 (pos.x + 0.5f - 0.2f, pos.y + 1f);
        }

        RaycastHit2D ceilLeft = Physics2D.Raycast (originLeft, Vector2.up, velocity.y * Time.deltaTime, floorMask);
        RaycastHit2D ceilMiddle = Physics2D.Raycast (originMiddle, Vector2.up, velocity.y * Time.deltaTime, floorMask);
        RaycastHit2D ceilRight = Physics2D.Raycast (originRight, Vector2.up, velocity.y * Time.deltaTime, floorMask);

        if (ceilLeft.collider != null || ceilMiddle.collider != null || ceilRight.collider != null) {

            RaycastHit2D hitRay = ceilLeft;

            if (ceilLeft) {
                hitRay = ceilLeft;
            } else if (ceilMiddle) {
                hitRay = ceilMiddle;
            } else if (ceilRight) {
                hitRay = ceilRight;
            }

            if (hitRay.collider.tag == "QuestionBlock") {

                hitRay.collider.GetComponent<QuestionBlock>().QuestionBlockBounce();
            } else if (hitRay.collider.tag == "BrickBlock") {

            //    if (powerupState == PowerupState.small)
            //        hitRay.collider.GetComponent<BrickBlock>().BrickBlockBounce();
            //    else
            //        hitRay.collider.GetComponent<BrickBlock>().BrickBlockBreak();
            }

            if (hitRay.collider.tag != "Enemy" && hitRay.collider.tag != "Spring") {
                if (powerupState == PowerupState.small) {
                    pos.y = hitRay.collider.bounds.center.y - hitRay.collider.bounds.size.y / 2 - .5f;
                } else if (inCrouchState) {
                    pos.y = hitRay.collider.bounds.center.y - hitRay.collider.bounds.size.y / 2;
                } else {
                    pos.y = hitRay.collider.bounds.center.y - hitRay.collider.bounds.size.y / 2 - 1;
                }
                

                Fall ();
            }
        }

        return pos;
    }

    void Fall () {

        velocity.y = 0;
        
        playerState = PlayerState.jumping;

        bounce = false;
        grounded = false;
    }

    void UpdateBoxCollider() {
        if (powerupState == PowerupState.small) {
            m_Collider.size = new Vector2(0.8f, 1f);
            m_Collider.offset = new Vector2(0, 0);
        } else {
            m_Collider.size = new Vector2(.8f, 1.910833f);
            m_Collider.offset = new Vector2(0, -0.04458332f);
        }

    }
    private void OnTriggerEnter2D(Collider2D other) {
        //print("enter" + other.gameObject.name);
        Vector3 pos = transform.localPosition;
        if (other.gameObject.tag == "Powerup") {
            Destroy(other.gameObject);
            if (powerupState == PowerupState.small)
                pos = GetBig(pos);
        } else if (other.gameObject.tag == "Enemy") {
            if (powerupState == PowerupState.small) {
                SceneManager.LoadScene("GameOver");
            } else if (powerupState == PowerupState.big) {
                pos = GetSmall(pos);
                InsideEnemy = true;
            }
        }
        transform.position = pos;
    }

    private void OnTriggerExit2D(Collider2D other) {
        //print("left" + other.gameObject.name);
        if (other.gameObject.tag == "Enemy") {
            InsideEnemy = false;
        }
    }

    Vector3 GetBig(Vector3 pos) {
        powerupState = PowerupState.big;
        pos.y += .5f;
        return pos;
    }

    Vector3 GetSmall(Vector3 pos) {
        powerupState = PowerupState.small;
        pos.y -= .5f;
        return pos;
    }
}
