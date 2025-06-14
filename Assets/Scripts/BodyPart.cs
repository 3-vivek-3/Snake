using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPart : MonoBehaviour
{
    Vector2 deltaPosition;

    // reference to the body part it is following.
    public BodyPart following = null;

    private SpriteRenderer spriteRenderer = null;

    // circular buffer stuff
    int PARTSREMEMBERED;
    public Vector3[] previousPositions;
    public int setIndex;
    public int getIndex;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        PARTSREMEMBERED = 10;
        previousPositions = new Vector3[PARTSREMEMBERED];
        setIndex = 0;
        getIndex = -(PARTSREMEMBERED - 1);

}

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    virtual public void Update()
    {
        if (!GameController.instance.alive) return;

        // the position of the body part 'this' is following(only for normal body part).
        Vector3 followPosition;

        if (following != null) // not a head
        {
            // if the followed body part's buffer is filled, then getIndex will be > -1.
            if (following.getIndex > -1) followPosition = following.previousPositions[following.getIndex];
            else followPosition = following.transform.position;
        }
        else followPosition = gameObject.transform.position;


        // values are rewritten on. Thus, garbage collection won't kick in.
        previousPositions[setIndex].x = gameObject.transform.position.x;
        previousPositions[setIndex].y = gameObject.transform.position.y;
        previousPositions[setIndex].z = gameObject.transform.position.z;

        setIndex = (setIndex + 1) % PARTSREMEMBERED;
        getIndex = (getIndex + 1) % PARTSREMEMBERED;

        if (following != null) // not the head
        {
            // the position of 'this' body part.
            Vector3 newPosition;

            if(following.getIndex > -1) newPosition = followPosition;
            else newPosition = following.transform.position;

            newPosition.z = newPosition.z + 0.01f;

            SetMovement(newPosition - gameObject.transform.position);
            UpdateDirection();
            UpdatePosition();
        }
    }

    // set the distance moved.
    public void SetMovement(Vector2 movement)
    {
        deltaPosition = movement;
    }

    // update the body part's position based on the movement.
    public void UpdatePosition()
    {
        gameObject.transform.position += (Vector3)deltaPosition;
    }

    public void UpdateDirection()
    {
        //up
        if (deltaPosition.y > 0) gameObject.transform.localEulerAngles = new Vector3(0, 0, 0);
        // down
        else if (deltaPosition.y < 0) gameObject.transform.localEulerAngles = new Vector3(0, 0, 180);
        // left
        else if (deltaPosition.x < 0) gameObject.transform.localEulerAngles = new Vector3(0, 0, 90);
        // right
        else if (deltaPosition.x > 0) gameObject.transform.localEulerAngles = new Vector3(0, 0, -90);
    }

    internal void TurnIntoTail()
    {
        spriteRenderer.sprite = GameController.instance.tailSprite;
    }

    internal void TurnIntoBody()
    {
        spriteRenderer.sprite = GameController.instance.bodySprite;
    }

    public void ResetMemory()
    {
        setIndex = 0;
        getIndex = -(PARTSREMEMBERED - 1);
    }
}
