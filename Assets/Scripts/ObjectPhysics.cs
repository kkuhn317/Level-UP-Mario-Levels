using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectPhysics : MonoBehaviour
{

    [Header("Object Physics")]

    public bool movingLeft = true;
    public Vector2 velocity = new Vector2(2,0);

    public float gravity = 60f;

    public float width = 1;
    public float height = 1;

    // THIS IS DISTANCE AWAY FROM SIDES
    public float floorRaycastSpacing = 0.2f;
    public float wallRaycastSpacing = 0.2f;

    public LayerMask floorMask;
    public LayerMask wallMask;


    // should mostly be true, except for things like moving koopa shells
    public bool checkObjectCollision = true;

    public bool DontFallOffLedges = false;


    // this is possible, but i'd say to just put these in the objects that actually need them
    //[SerializeField] UnityEvent onFloorTouch;
    //[SerializeField] UnityEvent onWallTouch;

    public bool flipObject = true;
    private Vector2 normalScale;

    private float adjDeltaTime;

    private bool firstframe = true;

    public enum ObjectState {
        falling,
        walking,
        knockedAway
    }

    public AudioClip knockAwaySound;

    public enum ObjectMovement {
        still,      // not moving at all
        sliding,    // falling and sliding
        bouncing    // falling and bouncing (never in walking objectState)

    }

    public ObjectState objectState = ObjectState.falling;
    public ObjectMovement movement = ObjectMovement.sliding;

    public float bounceHeight;

    protected virtual void Start()
    {
        normalScale = transform.localScale;
    }

    protected virtual void Update()
    {
        adjDeltaTime = Time.deltaTime;

        if (adjDeltaTime > 0.1f) {
            adjDeltaTime = 0f;  // lag spike fix
            //print("lagging!");
        }

        if ((!(movement == ObjectMovement.still) || objectState == ObjectState.knockedAway) && !firstframe)
            UpdatePosition ();
        firstframe = false;

    }

    public void UpdatePosition () {

        Vector3 pos = transform.position;
        Vector3 scale = transform.localScale;

        // check walls first
        if (objectState != ObjectState.knockedAway) {
            if (velocity.x != 0) {
                CheckWalls (pos, movingLeft ? -1 : 1);
            }
        }

        if (objectState == ObjectState.falling || objectState == ObjectState.knockedAway) {
            
            pos.y += velocity.y * adjDeltaTime;

            velocity.y -= gravity * adjDeltaTime;
        }

        if (movingLeft) {

            pos.x -= velocity.x * adjDeltaTime;

            if (flipObject)
                scale.x = normalScale.x;

        } else {

            pos.x += velocity.x * adjDeltaTime;

            if (flipObject)
                scale.x = -normalScale.x;
        }

        // fix bug where object has y velocity but walking
        // making it walk in the air
        if (objectState == ObjectState.walking) {
            velocity.y = 0;
        }

        if (objectState != ObjectState.knockedAway) {

            if (velocity.y <= 0) {
                pos = CheckGround (pos);
            }


            if (DontFallOffLedges && objectState == ObjectState.walking) {
                CheckLedges(pos);
            }
        }

        transform.position = pos;
        transform.localScale = scale;
    }

    Vector3 CheckGround (Vector3 pos) {

        float halfHeight = height / 2;
        float halfWidth = width / 2;

        Vector2 originLeft = new Vector2 (pos.x - halfWidth + floorRaycastSpacing, pos.y - halfHeight);
        Vector2 originMiddle = new Vector2 (pos.x, pos.y - halfHeight);
        Vector2 originRight = new Vector2 (pos.x + halfWidth - floorRaycastSpacing, pos.y - halfHeight);
        //print("adjDeltaTime is:" + (adjDeltaTime));
        //print("Velocity is:" + velocity);
        RaycastHit2D[] groundLeft = Physics2D.RaycastAll (originLeft, Vector2.down, -velocity.y * adjDeltaTime + .02f, floorMask);
        RaycastHit2D[] groundMiddle = Physics2D.RaycastAll (originMiddle, Vector2.down, -velocity.y * adjDeltaTime + .02f, floorMask);
        RaycastHit2D[] groundRight = Physics2D.RaycastAll (originRight, Vector2.down, -velocity.y * adjDeltaTime + .02f, floorMask);


        RaycastHit2D[][] groundCollides = {groundLeft, groundMiddle, groundRight};


        // get shortest distance
        float shortestDistance = float.MaxValue;
        RaycastHit2D shortestRay = new RaycastHit2D ();
        Collider2D thisCollider = GetComponent<Collider2D> ();

        foreach (RaycastHit2D[] groundCols in groundCollides) {
            foreach (RaycastHit2D hitRay in groundCols) {
                if (hitRay.collider != thisCollider) {
                    if (hitRay.collider.gameObject.GetComponent<ObjectPhysics> ()) {
                        if (!checkifObjectCollideValid(hitRay.collider.gameObject.GetComponent<ObjectPhysics> ())) {
                            continue;
                        }
                    }
                    if (hitRay.distance < shortestDistance) {
                        shortestDistance = hitRay.distance;
                        shortestRay = hitRay;
                    }
                }
            }
        }

        if (shortestRay) {

            pos.y = shortestRay.point.y + halfHeight;
            velocity.y = 0;

            if (movement == ObjectMovement.sliding) {
                objectState = ObjectState.walking;
            } else if (movement == ObjectMovement.bouncing) {
                velocity.y = bounceHeight;
            }

        } else {

            if (objectState != ObjectState.falling) {

                Fall ();
            }
        }
        return pos;
    }

    RaycastHit2D RaycastWalls (Vector3 pos, float direction) {
        // use raycast all and don't count itself
        float halfHeight = height / 2;
        float halfWidth = width / 2;

        Vector2 originTop = new Vector2 (pos.x + direction * halfWidth, pos.y + halfHeight - wallRaycastSpacing);
        Vector2 originMiddle = new Vector2 (pos.x + direction * halfWidth, pos.y);
        Vector2 originBottom = new Vector2 (pos.x + direction * halfWidth, pos.y - halfHeight + wallRaycastSpacing);

        RaycastHit2D[] wallTop = Physics2D.RaycastAll (originTop, new Vector2 (direction, 0), velocity.x * adjDeltaTime, wallMask);
        RaycastHit2D[] wallMiddle = Physics2D.RaycastAll (originMiddle, new Vector2 (direction, 0), velocity.x * adjDeltaTime, wallMask);
        RaycastHit2D[] wallBottom = Physics2D.RaycastAll (originBottom, new Vector2 (direction, 0), velocity.x * adjDeltaTime, wallMask);

        RaycastHit2D[][] wallCollides = {wallTop, wallMiddle, wallBottom};


        // get shortest distance
        float shortestDistance = float.MaxValue;
        RaycastHit2D shortestRay = new RaycastHit2D ();
        Collider2D thisCollider = GetComponent<Collider2D> ();

        foreach (RaycastHit2D[] wallCols in wallCollides) {
            foreach (RaycastHit2D hitRay in wallCols) {
                if (hitRay.collider != thisCollider) {
                    if (hitRay.collider.gameObject.GetComponent<ObjectPhysics> ()) {
                        if (!checkifObjectCollideValid(hitRay.collider.gameObject.GetComponent<ObjectPhysics> ())) {
                            continue;
                        }
                    }
                    if (hitRay.distance < shortestDistance) {
                        shortestDistance = hitRay.distance;
                        shortestRay = hitRay;
                    }
                }
            }
        }

        return shortestRay;
    }

    private bool checkifObjectCollideValid (ObjectPhysics other) {
        if (!checkObjectCollision) {
            //print("hit object physics while I am turned off");
            return false;
        }

        if (!other.checkObjectCollision) {
            //print("hit object physics while the other one is turned off");
            return false;
        }

        return true;
    }


    public virtual bool CheckWalls (Vector3 pos, float direction) {

        //RaycastHit2D hitRay = RaycastWalls (pos, direction);
        RaycastHit2D hitRay = RaycastWalls (pos, direction);

        if (hitRay.collider == null) {
            return false;
        }

        // check if the gameobject we hit is also an objectphysics
        if (hitRay.collider.gameObject.GetComponent<ObjectPhysics> () != null) {
            ObjectPhysics other = hitRay.collider.gameObject.GetComponent<ObjectPhysics> ();

            //probably this checking should come in the raycast walls function
            if (other.objectState == ObjectState.knockedAway) {
                // They are knocked away, so we can walk through them
                return false;
            }

            // TODO: Implement proper object collision
        }

        // hit something, so bounce off
        movingLeft = !movingLeft;
        return true;

    }

    void CheckLedges(Vector3 pos) {
        float halfHeight = height / 2;
        float halfWidth = width / 2;

        Vector2 origin;

        if (movingLeft) 
            origin = new Vector2 (pos.x - halfWidth, pos.y - halfHeight);
        else
            origin = new Vector2 (pos.x + halfWidth, pos.y - halfHeight);

        RaycastHit2D ground = Physics2D.Raycast (origin, Vector2.down, 0.5f, floorMask);

        if (!ground) {
            movingLeft = !movingLeft;
        }

    }

    void Fall () {

        velocity.y = 0;

        objectState = ObjectState.falling;

    }

    public void KnockAway(bool direction) {
        if (objectState != ObjectState.knockedAway) {
            GetComponent<SpriteRenderer>().flipY = true;
            objectState = ObjectState.knockedAway;
            velocity.y = 5;
            velocity.x = 5;
            movingLeft = direction;
            GetComponent<Collider2D>().enabled = false;
            transform.rotation = Quaternion.identity;
            // play sound
            if (knockAwaySound != null && GetComponent<AudioSource>() != null) {
                GetComponent<AudioSource>().PlayOneShot(knockAwaySound);
            }
        }
    }

    private void OnBecameInvisible() {
        // once the knocked away object is off screen, destroy it
        if (objectState == ObjectState.knockedAway) {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos() {
        Vector2 pos = transform.position;
        float halfHeight = height / 2;
        float halfWidth = width / 2;

        Vector2 originLeft = new Vector2 (pos.x - halfWidth + floorRaycastSpacing, pos.y - halfHeight);
        Vector2 originMiddle = new Vector2 (pos.x, pos.y - halfHeight);
        Vector2 originRight = new Vector2 (pos.x + halfWidth - floorRaycastSpacing, pos.y - halfHeight);
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(originLeft, originLeft + new Vector2(0,-1));
        Gizmos.DrawLine(originMiddle, originMiddle + new Vector2(0,-1));
        Gizmos.DrawLine(originRight, originRight + new Vector2(0,-1));

    }
}
