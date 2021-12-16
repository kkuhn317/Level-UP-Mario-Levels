using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingBlock : MonoBehaviour
{

    public float gravity;
    public LayerMask floorMask;

    public Vector2 velocity;

    private enum ObjectState {
        
        standing,
        falling
    }

    private ObjectState state = ObjectState.falling;

    // Start is called before the first frame update
    void Start()
    {
        enabled = false;

        Fall();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePosition ();
    }

    void UpdatePosition () {

        Vector3 pos = transform.localPosition;
        Vector3 scale = transform.localScale;

        if (state == ObjectState.falling) {

            pos.y += velocity.y * Time.deltaTime;

            velocity.y -= gravity * Time.deltaTime;
        }

        if (velocity.y <= 0)
            pos = CheckGround (pos);

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

            pos.y = hitRay.point.y + 0.5f;


            velocity.y = 0;

            state = ObjectState.standing;

        } else {

            if (state != ObjectState.falling) {

                Fall ();
            }
        }

        return pos;
    }

    void OnBecameVisible() {
        
        enabled = true;
    }

    void Fall () {

        velocity.y = 0;

        state = ObjectState.falling;

    }
}
