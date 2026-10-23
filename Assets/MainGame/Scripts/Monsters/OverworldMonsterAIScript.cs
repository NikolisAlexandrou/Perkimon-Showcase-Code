
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldMonsterAI : MonoBehaviour
{
    public BattleMonster monster;
    [SerializeField] float chanceToSpawn = 100;
    [SerializeField] GameObject ItemToSpawn;
    [SerializeField] List<Vector2> route = new List<Vector2>();
    [SerializeField] bool randomRoute;
    [SerializeField] float randomMovementOffset; // eg from 1 to 5
    [SerializeField] Vector2 StartPos;
    [SerializeField] float farAwayValue;

    [SerializeField] float timeBetweenMoves;
    [SerializeField] float moveTime;

    [SerializeField] float moveAnimationSpeed;
    [SerializeField] bool playIdleAnim;
    [SerializeField] bool interrupt;
    [SerializeField] bool followPlayer;
    float animTimer;
    Rigidbody2D rb;
    bool isMovingToNextPosition;
    bool movingAnim;
    float timer;
    float timer2;
    bool stopped;
    int currentRoute;
    int currentAnimationFrame;
    private void Start()
    {
        if (followPlayer == true)
        {
            monster = PlayerStats.i.PlayerParty[0];
        }
        else
        {
            Spawn();
        }
        rb = GetComponent<Rigidbody2D>();
        GetComponent<SpriteRenderer>().sprite = monster.Base.overworldSprites[0];
        timer = moveTime;
        timer2 = 0;
        StartPos = transform.position;
    }

    void Spawn()
    {
        if (Random.Range(0, 100) <= chanceToSpawn)
        {
            gameObject.SetActive(true);
        }
        else
        {
            Instantiate(ItemToSpawn, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
    private void FixedUpdate()
    {
        if (GameUtils.i.battleSystem.CurrentState is FreezeState)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            if (interrupt == true)
                return;

            MoveToNextPosition();
            PlayWalkAnimation();
            if (playIdleAnim == true)
            {
                PlayIdleAnimation();
            }
        }
    }
    public void SetInterrupt(bool set)
    {
        interrupt = set;
    }
    public bool GetInterrupt()
    {
        return interrupt;
    }
    public void PlayIdleAnimation()
    {
        if (animTimer > 0)
        {
            GetComponent<SpriteRenderer>().sprite = monster.Base.overworldWalkSpritesFront[currentAnimationFrame];
            animTimer -= Time.deltaTime;
        }
        else
        {
            animTimer = moveAnimationSpeed;
            if (currentAnimationFrame + 1 < monster.Base.overworldWalkSpritesFront.Count)
                currentAnimationFrame++;
            else
                currentAnimationFrame = 0;
        }
    }
    public void PlayWalkAnimation()
    {
        if (movingAnim == true)
        {
            if (animTimer > 0)
            {
                if (route[currentRoute].x > 0)
                    GetComponent<SpriteRenderer>().sprite = monster.Base.overworldWalkSpritesRight[currentAnimationFrame];
                else if (route[currentRoute].x < 0)
                    GetComponent<SpriteRenderer>().sprite = monster.Base.overworldWalkSpritesLeft[currentAnimationFrame];
                if (route[currentRoute].y > 0)
                    GetComponent<SpriteRenderer>().sprite = monster.Base.overworldWalkSpritesBack[currentAnimationFrame];
                else if (route[currentRoute].y < 0)
                    GetComponent<SpriteRenderer>().sprite = monster.Base.overworldWalkSpritesFront[currentAnimationFrame];


                animTimer -= Time.deltaTime;
            }
            else
            {
                animTimer = moveAnimationSpeed;
                if (currentAnimationFrame + 1 < monster.Base.overworldWalkSpritesFront.Count)
                    currentAnimationFrame++;
                else
                    currentAnimationFrame = 0;
            }
        }
    }

    public void getSpriteFromMovement()
    {
        if (route[currentRoute].x > 0)
            GetComponent<SpriteRenderer>().sprite = monster.Base.overworldSprites[2];
        else if (route[currentRoute].x < 0)
            GetComponent<SpriteRenderer>().sprite = monster.Base.overworldSprites[1];
        if (route[currentRoute].y > 0)
            GetComponent<SpriteRenderer>().sprite = monster.Base.overworldSprites[3];
        else if (route[currentRoute].y < 0)
            GetComponent<SpriteRenderer>().sprite = monster.Base.overworldSprites[0];
    }
    public void assignNewRandomRoute()
    {
        route.Add(new Vector2((int)Random.Range(-randomMovementOffset, randomMovementOffset), (int)Random.Range(-randomMovementOffset, randomMovementOffset)));
    }
    public void assignNewRoute(int x, int y)
    {
        route.Add(new Vector2(x, y));
    }
    int getXFromInitialPosition()
    {
        return (int)(StartPos.x - transform.position.x);
    }
    int getYFromInitialPosition()
    {
        return (int)(StartPos.y - transform.position.y);
    }

    public void FollowPlayer()
    {
        if (followPlayer == false)
            return;


        if (Vector2.Distance(transform.position, GameUtils.i.player.transform.position) <= farAwayValue)
        {
            assignNewRoute((int)transform.position.x, (int)transform.position.y);
        }
        else
        {
            assignNewRoute((int)GameUtils.i.player.transform.position.x, (int)GameUtils.i.player.transform.position.y);
        }
    }

    public void MoveToNextPosition()
    {
        if (isMovingToNextPosition == false)
        {
            //(new Vector2(transform.position.x + route[currentRoute].x, transform.position.y + route[currentRoute].y));
            if (randomRoute == true)
            {
                currentRoute = 0;

                if (Vector2.Distance(transform.position, StartPos) >= farAwayValue)
                {
                    assignNewRoute(getXFromInitialPosition(), getYFromInitialPosition());
                }
                else
                {
                    assignNewRandomRoute();
                }
            }
            else
            {
                FollowPlayer();
            }
            rb.linearVelocity = route[currentRoute];
            getSpriteFromMovement();
            isMovingToNextPosition = true;
            movingAnim = true;
        }
        else
        {
            if (timer > 0)
                timer -= Time.deltaTime;
            else
            {

                if (stopped == false)
                {
                    stopped = true;
                    rb.linearVelocity = Vector2.zero;
                    timer2 = timeBetweenMoves;
                    movingAnim = false;
                }
                if (timer2 > 0)
                    timer2 -= Time.deltaTime;
                else
                {
                    if (currentRoute + 1 < route.Count)
                        currentRoute++;
                    else
                        currentRoute = 0;
                    if (randomRoute == true)
                    {
                        route.Clear();
                    }
                    isMovingToNextPosition = false;
                    currentAnimationFrame = 0;
                    animTimer = moveAnimationSpeed;
                    timer = moveTime;
                    stopped = false;
                }
            }

        }


    }
}