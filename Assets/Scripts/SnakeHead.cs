using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SnakeHead : BodyPart
{
    Vector2 movement;

    private BodyPart tail = null;

    const float TIMETOADDBODYPART = 0.1f;
    float addTimer = TIMETOADDBODYPART;

    public int partsToAdd = 0;

    List<BodyPart> bodyParts = new List<BodyPart>();

    public AudioSource[] gulpSounds = new AudioSource[3];
    public AudioSource dieSound = null;

    //0: up, 1: right, 2: down, 3: left
    private int currentDirection = 0;

    // Start is called before the first frame update
    void Start()
    {
        SwipeControls.OnSwipe += SwipeDetection;
    }

    // Update is called once per frame
    override public void Update()
    {
        if (!GameController.instance.alive) return;
        base.Update();

        SetMovement(movement * Time.deltaTime);
        UpdateDirection();
        UpdatePosition();

        if (partsToAdd > 0)
        {
            addTimer -= Time.deltaTime;
            if(addTimer <= 0)
            {
                addTimer = TIMETOADDBODYPART;
                AddBodyPart();
                partsToAdd--;
            }
        }
    }

    void AddBodyPart()
    {
        if (tail == null)
        {
            Vector3 newPosition = transform.position;
            newPosition.z = newPosition.z + 0.01f; // to avoid rendering clashes.

            BodyPart newPart = Instantiate(GameController.instance.bodyPrefab, newPosition, Quaternion.identity);
            newPart.following = this;
            tail = newPart;
            newPart.TurnIntoTail();

            bodyParts.Add(newPart);
        }
        else
        {
            Vector3 newPosition = tail.transform.position;
            newPosition.z = newPosition.z + 0.01f; // to avoid rendering clashes.
            
            // create new part
            BodyPart newPart = Instantiate(GameController.instance.bodyPrefab, newPosition, tail.transform.rotation);
            // make it follow the old tail.
            newPart.following = tail;
            // turn this into the new tail.
            newPart.TurnIntoTail();
            // turn the old tail into a normal body.
            tail.TurnIntoBody();
            // set the reference to the new tail.
            tail = newPart;

            bodyParts.Add(newPart);
        }
    }

    void SwipeDetection(SwipeControls.SwipeDirection direction)
    {
        switch(direction)
        {
            case SwipeControls.SwipeDirection.Up:
                if (currentDirection == 2) break;
                MoveUp();
                currentDirection = 0;
                break;
            case SwipeControls.SwipeDirection.Down:
                if(currentDirection == 0) break;
                MoveDown();
                currentDirection = 2;
                break;
            case SwipeControls .SwipeDirection.Left:
                if(currentDirection == 1) break;
                MoveLeft();
                currentDirection = 3;
                break;
            case SwipeControls.SwipeDirection.Right:
                if(currentDirection == 3) break;
                MoveRight();
                currentDirection = 1;
                break;
        }
    }

    void MoveUp()
    {
        movement = Vector2.up * GameController.instance.snakeSpeed;
    }

    void MoveDown()
    {
        movement = Vector2.down * GameController.instance.snakeSpeed;
    }

    void MoveLeft()
    {
        movement = Vector2.left * GameController.instance.snakeSpeed;
    }

    void MoveRight()
    {
        movement = Vector2.right * GameController.instance.snakeSpeed;
    }

    internal void ResetSnake()
    {
        foreach (BodyPart part in bodyParts)
        {
            Destroy(part.gameObject);
        }
        bodyParts.Clear();

        ResetMemory();

        tail = null;
        MoveUp();
        currentDirection = 0;

        gameObject.transform.localEulerAngles = new Vector3(0, 0, 0);
        gameObject.transform.position = new Vector3(0, 0, -8);

        partsToAdd = 5;
        addTimer = TIMETOADDBODYPART;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Egg egg = collision.GetComponent<Egg>();
        if(egg)
        {
            Debug.Log("Hit egg: " + egg.gameObject.transform.position);
            EatEgg(egg);
        }
        else
        {
            Debug.Log("hit obstacle");
            dieSound.Play();
            GameController.instance.GameOver();
        }
    }

    private void EatEgg(Egg egg)
    {
        partsToAdd = 5;
        addTimer = 0;
        int rand = Random.Range(0, 3);
        gulpSounds[rand].Play();
        GameController.instance.EggEaten(egg);
    }
}
