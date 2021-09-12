using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//https://forum.unity.com/threads/moving-object-in-random-position-in-scence-like-fly.328050/

public class flyAroundSM : MonoBehaviour
{
    public float speed = 1.5f;
    public float rotateSpeed = 5.0f;
    public float maxMove = 1;

    Vector3 newPosition;
    //bool isMoving = false;

    public bool delayAnim = true;
    Animator anim;
    Animation animA;

    public bool flyAround = false;

    void Start()
    {
        if (flyAround)
        {
            PositionChange();
        }

        anim = GetComponent<Animator>();
        animA = GetComponent<Animation>();

        if (delayAnim && anim != null)
        {
            anim.enabled = false;
        }
        if (delayAnim && animA != null)
        {
            animA.enabled = false;
        }
    }

    public float minDelay = 1;
    public float maxDelay = 4;

    void Update()
    {

        if(delayAnim && anim != null)
        {
            if (!anim.enabled && Time.fixedTime > Random.Range(minDelay, maxDelay))
            {
                anim.enabled = true;
            }
        }
        if (delayAnim && animA != null)
        {
            if (!animA.enabled && Time.fixedTime > Random.Range(minDelay, maxDelay))
            {
                animA.enabled = true;
                animA.playAutomatically = true;
            }
        }

        if (flyAround)
        {
            if (Vector2.Distance(transform.position, newPosition) < 1)
            {
                PositionChange();
            }

            if (LookAt2D(newPosition))
            {
                transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * speed);
            }
        }
    }

    void PositionChange()
    {
        //newPosition = new Vector2(Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f));

        newPosition = new Vector2(Random.Range(-maxMove, maxMove), Random.Range(-maxMove, maxMove));

        //This check if the new position is to the right of the flying object.
        //if (newPosition.x > transform.position.x)
        //    transform.localScale = new Vector3(-1, 1, 1);
        //else
        //    transform.localScale = Vector3.one;
    }

    bool LookAt2D(Vector3 lookAtPosition)
    {
        float distanceX = lookAtPosition.x - transform.position.x;
        float distanceY = lookAtPosition.y - transform.position.y;
        float angle = Mathf.Atan2(distanceX, distanceY) * Mathf.Rad2Deg;

        Quaternion endRotation = Quaternion.AngleAxis(angle, Vector3.back);
        transform.rotation = Quaternion.Slerp(transform.rotation, endRotation, Time.deltaTime * rotateSpeed);

        if (Quaternion.Angle(transform.rotation, endRotation) < 1f)
            return true;

        return false;
    }
    //public float speed = 1;
    //// Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    transform.position = Vector3.Lerp(transform.position,
    //                 transform.position + new Vector3((Random.value - 0.5f) * speed, 0,
    //                 (Random.value - 0.5f) * speed), Time.time);
    //}
}
