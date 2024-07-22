using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class FrogAI : MonoBehaviour
{
    [Header("LOS")]
    [SerializeField] private float noticeRadius;

    [Header("PlayerProperties")]
    [SerializeField] private Transform player;

    [Header("Enemy Idle")]
    private Vector3 mainIdlePos; // Spawn Position
    [SerializeField] private float jumpForce; 
    [SerializeField] private float maxWanderDist; // Max radius to free roam
    private float minWanderDist;
    private float moveDuration = 1;
    private float idleSpeed;
    private float idleMovingTimer = 0; // timer for idle movement
    private float idleBreakTimer = 0; // timer for idle break
    private bool returningToStart;

    private Vector3 idleDirection;
    Rigidbody rb;

    private float runHopTimer = 0; // timer for running/LOS movement

    public float enemySpeed;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>(); // gets rigidbody
        mainIdlePos = transform.position; // gets spawn position
        idleSpeed = enemySpeed / 2; // idle speed is half the running speed. change if you want slower or faster
        minWanderDist = maxWanderDist / 3; // min distance for wandering is 1/3 of max.
    }

    // Update is called once per frame
    void Update()
    {
        if (InLineOfSight())
        {
            RunFromPlayer();
        }
        else
        {
            Idle();
        }
    }

    private bool InLineOfSight()
    {
        Vector3 direction = (player.position - transform.position).normalized;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, noticeRadius))
        {
            if (hit.transform.gameObject.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    private void RunFromPlayer()
    {
        Vector3 direction = (transform.position - player.position).normalized;
        float newSpeed = enemySpeed * 1.5f;
        if (runHopTimer <= 0)
        {
            rb.MovePosition(transform.position + direction * newSpeed * Time.deltaTime);
            Hop(direction);
            runHopTimer = 1;
        }
        runHopTimer -= Time.deltaTime;

        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, enemySpeed * Time.deltaTime);
    }

    private void Idle()
    {
        isReturning();

        if (!returningToStart)
        {
            IdleWander();
        }
        else
        {
            ReturnToStart();
        }

    }

    private void isReturning()
    {
        if (Vector3.Distance(transform.position, mainIdlePos) > maxWanderDist)
        {
            returningToStart = true;
        }
        else if (Vector3.Distance(transform.position, mainIdlePos) < minWanderDist)
        {
            returningToStart = false;
        }
    }

    private void ReturnToStart()
    {
        Vector3 direction = (mainIdlePos - transform.position).normalized;
        if (idleBreakTimer <= 0)
        {
            rb.MovePosition(transform.position + direction * idleSpeed * Time.deltaTime);
            Hop(direction);
            idleBreakTimer = 3;
        }
        else
        {
            idleBreakTimer -= Time.deltaTime;
        }

        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, enemySpeed * Time.deltaTime);
    }

    private void IdleWander()
    {
        // Not resting or currently moving? Try moving!
        if (idleBreakTimer <= 0)
        {
            // Not moving? Get a new direction to move
            if (idleMovingTimer <= 0)
            {
                idleMovingTimer = moveDuration;
                GetIdleDirection();
            }
            else
            {
                rb.MovePosition(transform.position + idleDirection * idleSpeed * Time.deltaTime);
                Hop(idleDirection);
                idleBreakTimer = Random.Range(4, 12);
            }
        }
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(idleDirection.x, 0, idleDirection.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, enemySpeed * Time.deltaTime);
        idleMovingTimer -= Time.deltaTime;
        idleBreakTimer -= Time.deltaTime;
    }

    private void GetIdleDirection()
    {
        float angle = Random.Range(0f, 360f);
        Vector3 randomDirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized;
        Vector3 targetPosition = transform.position + randomDirection * enemySpeed * idleMovingTimer;

        idleDirection = randomDirection;
    }

    private void Hop(Vector3 direction)
    {
        rb.AddForce(new Vector3(direction.x, 1, direction.z) * jumpForce, ForceMode.Impulse);
    }
}
