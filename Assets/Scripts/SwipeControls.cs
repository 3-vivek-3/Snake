using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeControls : MonoBehaviour
{
    Vector2 swipeStart;
    Vector2 swipeEnd;
    float minimumSwipeDistance = 10;

    public static event System.Action<SwipeDirection> OnSwipe = delegate { };

    public enum SwipeDirection
    {
        Up, Down, Left, Right
    };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach(Touch touch in Input.touches)
        {
            // Check if a finger has begun a touch.
            if(touch.phase == TouchPhase.Began)
            {
                swipeStart = touch.position;
            }
            // Check if a finger has just ended a touch.
            else if(touch.phase == TouchPhase.Ended)
            {
                swipeEnd = touch.position;
                ProcessSwipe();
            }
        }

        // mouse touch simulation. Allows testing on pc instead of relying solely on phone.
        if(Input.GetMouseButtonDown(0))
        {
            swipeStart = Input.mousePosition;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            swipeEnd = Input.mousePosition;
            ProcessSwipe();
        }
    }

    void ProcessSwipe()
    {
        float distance = Vector2.Distance(swipeStart, swipeEnd);
        if(distance > minimumSwipeDistance)
        {
            // vertical
            if(IsVerticalSwipe())
            {
                //up
                if(swipeEnd.y > swipeStart.y)
                {
                    OnSwipe(SwipeDirection.Up);
                }
                //down
                else
                {
                    OnSwipe(SwipeDirection.Down);
                }
            }
            // horizontal
            else
            {
                // right
                if(swipeEnd.x > swipeStart.x)
                {
                    OnSwipe(SwipeDirection.Right);
                }
                // left
                else
                {
                    OnSwipe(SwipeDirection.Left);
                }
            }
        }
    }

    bool IsVerticalSwipe()
    {
        float verticalDistance = Mathf.Abs(swipeEnd.y - swipeStart.y);
        float horizontalDistance = Mathf.Abs(swipeEnd.x - swipeStart.x);
        
        if (verticalDistance > horizontalDistance) return true;
        return false;

    }
}
