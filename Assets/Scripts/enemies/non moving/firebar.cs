using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class firebar : MonoBehaviour
{
    public GameObject fireball;
    public float speed = 10f;

    public bool rotateLeft = false;

    public int length = 5;

    private GameObject[] fireballs;

    public float angle = 0;

    // Start is called before the first frame update
    void Start()
    {
        Vector2 fireballpos = calcFireballpos();

        fireballs = new GameObject[length];
        for (int i = 0; i < length; i++)
        {
            fireballs[i] = Instantiate(fireball, transform.position, Quaternion.identity);
            fireballs[i].transform.parent = transform;
            fireballs[i].transform.localPosition = fireballpos * i * 0.5f;
        }
    }

    Vector2 calcFireballpos() {
        return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
    }

    // Update is called once per frame
    void Update()
    {
        if (!rotateLeft)
        {
            angle -= Time.deltaTime * speed;
        }
        else
        {
            angle += Time.deltaTime * speed;
        }
        if (angle > 360)
        {
            angle -= 360;
        }
        else if (angle < 0)
        {
            angle += 360;
        }

        Vector2 fireballpos = calcFireballpos();

        // place fireballs
        for (int i = 0; i < length; i++)
        {
            fireballs[i].transform.localPosition = fireballpos * i * 0.5f;
        }


    }
}
