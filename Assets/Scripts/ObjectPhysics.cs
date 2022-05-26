using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectPhysics : MonoBehaviour
{
    // IMPORTANT!!
    // TODO: Make enemies, and maybe mario and items use this to reduce code duplication

    public bool movingLeft;
    public Vector2 velocity;

    public float gravity;

    public float width;
    public float height;

    // THIS IS DISTANCE AWAY FROM SIDES
    public float floorRaycastSpacing;
    public float wallRaycastSpacing;

    public LayerMask floorMask;
    public LayerMask wallMask;

    [SerializeField] UnityEvent onFloorTouch;
    [SerializeField] UnityEvent onWallTouch;

    public bool checkEnemyCollision;

    public enum ObjectState {
        falling,
        walking
    }

    public enum ObjectMovement {
        still,      // not moving at all
        sliding,    // falling and sliding
        bouncing    // falling and bouncing (never in walking state)

    }

    public ObjectState state = ObjectState.falling;
    public ObjectMovement movement = ObjectMovement.sliding;

    public float bounceHeight;

    void Update()
    {
        if (movement != ObjectMovement.still)
            UpdatePosition();
    }

    public void UpdatePosition () {

        Vector3 pos = transform.position;
        Vector3 scale = transform.localScale;

        if (state == ObjectState.falling) {
            
            pos.y += velocity.y * Time.deltaTime;

            velocity.y -= gravity * Time.deltaTime;
        }

        if (movingLeft) {

            pos.x -= velocity.x * Time.deltaTime;

            scale.x = 1;

        } else {

            pos.x += velocity.x * Time.deltaTime;

            scale.x = -1;
        }


        if (velocity.y <= 0) {
            pos = CheckGround (pos);
        }

        CheckWalls (pos, -scale.x);

        transform.position = pos;
        transform.localScale = scale;
    }

    Vector3 CheckGround (Vector3 pos) {

        float halfHeight = height / 2;
        float halfWidth = width / 2;

        Vector2 originLeft = new Vector2 (pos.x - halfWidth + floorRaycastSpacing, pos.y - halfHeight);
        Vector2 originMiddle = new Vector2 (pos.x, pos.y - halfHeight);
        Vector2 originRight = new Vector2 (pos.x + halfWidth - floorRaycastSpacing, pos.y - halfHeight);
        //print("Time.deltaTime is:" + (Time.deltaTime));
        //print("Velocity is:" + velocity);
        RaycastHit2D groundLeft = Physics2D.Raycast (originLeft, Vector2.down, -velocity.y * Time.deltaTime, floorMask);
        RaycastHit2D groundMiddle = Physics2D.Raycast (originMiddle, Vector2.down, -velocity.y * Time.deltaTime, floorMask);
        RaycastHit2D groundRight = Physics2D.Raycast (originRight, Vector2.down, -velocity.y * Time.deltaTime, floorMask);

        if (groundLeft.collider != null || groundMiddle.collider != null || groundRight.collider != null) {

            RaycastHit2D hitRay = groundLeft;

            if (groundLeft) {
                hitRay = groundLeft;
            } else if (groundMiddle) {
                hitRay = groundMiddle;
            } else if (groundRight) {
                hitRay = groundRight;
            }

            //pos.y = hitRay.collider.bounds.center.y + hitRay.collider.bounds.size.y / 2 + halfHeight;
            pos.y = hitRay.point.y + halfHeight;
            velocity.y = 0;

            if (movement == ObjectMovement.sliding) {
                state = ObjectState.walking;
            } else if (movement == ObjectMovement.bouncing) {
                velocity.y = bounceHeight;
            }

            onFloorTouch.Invoke();

            //print("hit" + gameObject.name);

        } else {

            if (state != ObjectState.falling) {

                Fall ();
            }
        }
        return pos;
    }

    void CheckWalls (Vector3 pos, float direction) {

        float halfHeight = height / 2;
        float halfWidth = width / 2;

        Vector2 originTop = new Vector2 (pos.x + direction * halfWidth, pos.y + halfHeight - wallRaycastSpacing);
        Vector2 originMiddle = new Vector2 (pos.x + direction * halfWidth, pos.y);
        Vector2 originBottom = new Vector2 (pos.x + direction * halfWidth, pos.y - halfHeight + wallRaycastSpacing);

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

            // remove this part once enemies use this script for physics
            if (hitRay.collider.gameObject.tag == "Enemy") {
                EnemyAi otherEnemyAi = hitRay.collider.gameObject.GetComponent<EnemyAi>();
                if (otherEnemyAi.isWalkingLeft == movingLeft && !otherEnemyAi.stayStill && otherEnemyAi.velocity.x != 0)
                    // They are walking the same direction, so 
                    return;
            }

            ObjectPhysics otherObjectPhysics = hitRay.collider.gameObject.GetComponent<ObjectPhysics>();
            if (otherObjectPhysics != null) {
                if (otherObjectPhysics.movingLeft == movingLeft && otherObjectPhysics.movement != ObjectMovement.still && otherObjectPhysics.velocity.x != 0)
                    // They are moving the same direction, so 
                    return;
            }


            if (!(hitRay.collider.gameObject.tag == "Enemy" && !checkEnemyCollision)) {
                movingLeft = !movingLeft;
                onWallTouch.Invoke();
            }

        }
    }

    void Fall () {

        velocity.y = 0;

        state = ObjectState.falling;

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
