﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SpriteTile;

public class BasicEnemyAI : MonoBehaviour {

    public int attackDistance = 7;
    public int chargeDistance = 12;
    public float speed = 2f;
    public float nearbyEnemyRadius = .01f;
    public float pathFindingRate = 2f;
    public bool debug = false;
    public float chaseTime = 3f;
    public float attackDelay = .5f;

    private GameObject player;
    private Wander wanderScript;
    private BasicEnemyFire fireScript;
    private Rigidbody2D rb2d;

    private int wallLayerMask = 1 << 8; // Layer 8 is the wall layer.
    private int enemyLayerMask = 1 << 9;
    private float lastPathfindTime = 0;
    private bool chasing = false;
    private bool readyToAttack = false;
    private bool attackInvoked = false;

    private bool isFirstFrame = true;

    void Awake()
    {
        player = GameObject.Find("Soldier");
        wanderScript = GetComponent<Wander>();
        fireScript = GetComponent<BasicEnemyFire>();
        rb2d = GetComponent<Rigidbody2D>();
    }
	
	void Update () {
        if (isFirstFrame)
        {
            isFirstFrame = false;
            return;
        }

        Vector2 enemyPosition = gameObject.transform.position;
        Vector2 playerPosition = player.transform.position;

        float distanceFromPlayer = Vector3.Distance(playerPosition, enemyPosition);

        if (CanSeePlayer())
        {
            chasing = true;
            CancelInvoke("DeactivateChase");
            if (distanceFromPlayer <= attackDistance)
            {
                if (readyToAttack)
                {
                    rb2d.velocity = CalculateVelocity(enemyPosition);
                    fireScript.Fire();
                }
                else if (!attackInvoked)
                {
                    attackInvoked = true;
                    Invoke("ActivateAttack", attackDelay);
                }
            }
            else if (distanceFromPlayer <= chargeDistance)
            {
                readyToAttack = false;
                if (PathToPlayerIsNotBlocked())
                {
                    rb2d.velocity = CalculateVelocity(player.transform.position);
                }
                else
                {
                    ExecuteAStar(enemyPosition, playerPosition);
                }
            }
            else
            {
                readyToAttack = false;
                rb2d.velocity = CalculateVelocity(enemyPosition);
            }
        }
        else {
            readyToAttack = false;
            if (chasing)
            {
                Invoke("DeactivateChase", chaseTime);
            }

            if (distanceFromPlayer <= chargeDistance && chasing)
            {
                ExecuteAStar(enemyPosition, playerPosition);
            }
            else
            {
                chasing = false;
                rb2d.velocity = CalculateVelocity(enemyPosition);
            }
        } 
	}

    void ActivateAttack()
    {
        readyToAttack = true;
        attackInvoked = false;
    }

    void DeactivateChase()
    {
        chasing = false;
    }

    void StartFiring()
    {
        rb2d.velocity = CalculateVelocity(gameObject.transform.position);
        fireScript.Fire();
    }

    void ExecuteAStar(Vector2 enemyPosition, Vector2 playerPosition)
    {
        // Do A*
        if (Time.time > lastPathfindTime + pathFindingRate)
        {
            lastPathfindTime = Time.time;
            List<AStar.Node> list = AStar.calculatePath(AStar.positionToArrayIndices(enemyPosition),
                AStar.positionToArrayIndices(playerPosition));

            rb2d.velocity = CalculateVelocity(AStar.arrayIndicesToPosition(list[1].point));
        }
    }


    Vector2 CalculateVelocity(Vector2 target)
    {
        Vector2 pullVector = new Vector2(target.x - gameObject.transform.position.x,
            target.y - gameObject.transform.position.y).normalized * speed;
        Vector2 pushVector = Vector2.zero;

        // Find all nearby enemies
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(gameObject.transform.position, nearbyEnemyRadius, enemyLayerMask);
        int contenders = 0;

        for (int i = 0; i < nearbyEnemies.Length; i++)
        {
            if (nearbyEnemies[i].transform == gameObject.transform)
            {
                continue;
            }

            Vector2 push = gameObject.transform.position - nearbyEnemies[i].transform.position;
            pushVector += push / push.sqrMagnitude;

            contenders++;
        }


        pullVector *= Mathf.Max(1, 4 * contenders);
        pullVector += pushVector;

        return pullVector.normalized * speed;
    }

    bool CanSeePlayer()
    {
        RaycastHit2D linecastHit = Physics2D.Linecast(transform.position, player.transform.position, wallLayerMask);
        

        return linecastHit.transform == null;
    }

    bool PathToPlayerIsNotBlocked()
    {
        Vector2 colliderSize = GetComponent<BoxCollider2D>().size;
        Vector2 boxCastSize = new Vector2(colliderSize.x * 1.25f, colliderSize.y * 1.25f);
        RaycastHit2D boxHit = Physics2D.BoxCast(transform.position, boxCastSize, 0f, player.transform.position - transform.position,
            1, wallLayerMask);

        return boxHit.transform == null;
    }
}
