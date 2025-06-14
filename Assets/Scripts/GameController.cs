using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public SnakeHead snakeHead = null;

    public float snakeSpeed = 1;
    int level = 0;
    int requiredEggsToNextLevel = 0;

    public BodyPart bodyPrefab = null;
    public GameObject rockPrefab = null;
    public GameObject eggPrefab = null;
    public GameObject goldenEggPrefab = null;
    public GameObject spikePrefab = null;

    // a dictionary that stores grid positions and the objects in that grid.
    Dictionary<Vector2Int, List<Spike>> grid = new Dictionary<Vector2Int, List<Spike>>();
    List<Egg> eggs = new List<Egg>();

    public Sprite tailSprite = null;
    public Sprite bodySprite = null;

    public float width = 3.9375f;
    public float height = 7f;

    public bool alive = true;
    public bool waitingForPlayer = true;

    public int score = 0;
    public int highscore = 0;

    public TextMeshProUGUI scoreText = null;
    public TextMeshProUGUI highscoreText = null;
    public TextMeshProUGUI gameOverText = null;
    public TextMeshProUGUI tapToPlayText = null;


    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        instance = this;
        alive = false;
        // determine screen width and height
        
        Camera cam = Camera.main;
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
        width = (topRight.x - bottomLeft.x) / 2;
        height = (topRight.y - bottomLeft.y) / 2;
        
        Debug.Log(width + " " + height);

        CreateWalls();
    }

    // Update is called once per frame
    void Update()
    {
        if (waitingForPlayer)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Ended) StartGame();
            }

            // mouse touch simulation. Allows testing on pc instead of relying solely on phone.
            if (Input.GetMouseButtonUp(0)) StartGame();
        }
    }
    void StartGame()
    {
        waitingForPlayer = false;
        alive = true;

        foreach (Egg egg in eggs)
        {
            Destroy(egg.gameObject);
        }
        eggs.Clear();

        LevelUp();

        // text setup
        score = 0;
        highscore = PlayerPrefs.GetInt("highscore", 0);
        scoreText.text = "Score: " + score;
        highscoreText.text = "Highscore: " + highscore;
        gameOverText.gameObject.SetActive(false);
        tapToPlayText.gameObject.SetActive(false);
    }
    public void GameOver()
    {
        // reset initial game parameters
        alive = false;
        waitingForPlayer = true;
        level = 0;
        snakeSpeed = 1;

        gameOverText.gameObject.SetActive(true);

        // save highscore with PlayerPrefs
        PlayerPrefs.SetInt("highscore", highscore);
        //PlayerPrefs.Save();
    }

    void LevelUp()
    {
        level++;
        requiredEggsToNextLevel = 2 + level * 2;

        snakeSpeed = 1.5f + level / 4;

        snakeHead.ResetSnake();
        // clear old spikes
        ClearSpikes();
        // add spikes
        // beyond level 10, number of spikes remains the same.
        if(level <= 10) CreateSpikes((level - 1) * 2);
        else CreateSpikes(20);
        // add egg
        CreateEgg();
    }

    void CreateWalls()
    {
        Vector3 topLeftCorner = new Vector3(-width, height, 0);
        Vector3 bottomLeftCorner = new Vector3(-width, -height, 0);
        Vector3 topRightCorner = new Vector3(width, height, 0);
        Vector3 bottomRightCorner = new Vector3(width, -height, 0);
        CreateWall(topLeftCorner, topRightCorner);
        CreateWall(topRightCorner, bottomRightCorner);
        CreateWall(bottomRightCorner, bottomLeftCorner);
        CreateWall(bottomLeftCorner, topLeftCorner);
    }

    void CreateWall(Vector3 startPosition, Vector3 endPosition)
    {
        float distance = Vector3.Distance(startPosition, endPosition);
        int numberOfRocks = (int)(distance * 3f);
        Vector3 delta = (endPosition - startPosition) / numberOfRocks;

        Vector3 rockPosition = startPosition;

        for (int i = 0; i < numberOfRocks; i++)
        {
            float rotation = Random.Range(0, 360f);
            float scale = Random.Range(1.5f, 2f);
            CreateRock(rockPosition, scale, rotation);

            rockPosition += delta;
        }
    }

    void CreateRock(Vector3 position, float scale, float rotation)
    {
        GameObject rock = Instantiate(rockPrefab, position, Quaternion.Euler(0, 0, rotation));
        rock.transform.localScale = new Vector3(scale, scale, 1);
    }

    Vector2Int GetGridPosition(Vector3 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x / 1f), Mathf.FloorToInt(position.y / 1f));
    }

    bool CanCreateObject(Vector3 objectPosition, float minDistance)
    {
        Vector2Int gridPosition = GetGridPosition(objectPosition);

        // check if dictionary contains current cell and the 8 surrounding cells.
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int checkGridPosition = new Vector2Int(gridPosition.x + x, gridPosition.y + y);
                if (grid.ContainsKey(checkGridPosition)) // if the dictionary contains a key(cell coords), then some object is already there.
                {
                    // for each object in the cell
                    foreach (Spike spike in grid[checkGridPosition])
                    {
                        // an object existing in cell is too close to new location.
                        if (Vector3.Distance(objectPosition, spike.gameObject.transform.position) < minDistance) return false;
                    }
                }
            }
        }
        return true;
    }

    void CreateSpike()
    {
        Vector3 position;
        position.z = 0;
        // run do-while loop until valid location is found.
        do
        {
            position.x = (Random.Range(0, 2) == 0) ? Random.Range(-(width - 0.75f), -0.5f) : Random.Range(0.5f, width - 0.75f);
            position.y = Random.Range(-(height - 0.75f), height - 0.75f);
        } while (!CanCreateObject(position, 1));

        Spike spike = Instantiate(spikePrefab, position, Quaternion.identity).GetComponent<Spike>();
        Debug.Log("x: " + position.x + " y: " + position.y + " z: " + position.z);

        // add spike to appropriate grid cell. 
        Vector2Int gridPosition = GetGridPosition(position);
        if (!grid.ContainsKey(gridPosition)) grid[gridPosition] = new List<Spike>();
        grid[gridPosition].Add(spike);
    }

    void CreateSpikes(int num)
    {
        for(int i = 0; i < num; i++)
        {
            CreateSpike();
        }
    }

    void ClearSpikes()
    {
        foreach (KeyValuePair<Vector2Int, List<Spike>> cell in grid)
        {
            foreach(Spike spike in cell.Value)
            {
                Destroy(spike.gameObject);
            }
        }
        grid.Clear();
    }

    void CreateEgg(bool isGolden = false)
    {
        Vector3 position;
        position.z = 0;

        do
        {
            position.x = Random.Range(-(width - 0.75f), width - 0.75f);
            position.y = Random.Range(-(height - 0.75f), height - 0.75f);
        } while (!CanCreateObject(position, 0.5f));

        Egg egg = null;
        if (!isGolden) egg = Instantiate(eggPrefab, position, Quaternion.identity).GetComponent<Egg>();
        else egg = Instantiate(goldenEggPrefab, position, Quaternion.identity).GetComponent<Egg>();
        // don't need to add the egg to grid cell.
        eggs.Add(egg);
    }

    public void EggEaten(Egg egg)
    {
        score++;
        scoreText.text = "Score: " + score;

        eggs.Remove(egg);

        requiredEggsToNextLevel--;
        // last egg is consumed
        if (requiredEggsToNextLevel == 0)
        {
            score += 10;
            scoreText.text = "Score: " + score;
            LevelUp();
        }
        // last egg
        else if (requiredEggsToNextLevel == 1) CreateEgg(true);
        // not the last egg
        else CreateEgg(false);

        if (score > highscore)
        {
            highscore = score;
            highscoreText.text = "Highscore: " + highscore;
        }
        Destroy(egg.gameObject);
    }
}
