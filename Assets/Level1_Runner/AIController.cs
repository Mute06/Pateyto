using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class AIController : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;

    [Header("Movement")]
    public float startWaitTime = 4;
    public float timeToRotate = 2;
    public float speedWalk = 6;
    public float speedRun = 9;

    [Header("View Settings")]
    public float viewRadius = 15;
    public float viewAngle = 90;
    public LayerMask playerMask;
    public LayerMask obstacleMask;

    [Header("Patrol")]
    public Transform[] waypoints;

    int m_CurrentWaypointIndex;

    Vector3 m_PlayerPosition;
    Vector3 playerLastPosition;

    float m_WaitTime;
    float m_TimeToRotate;

    bool m_PlayerInRange;
    bool m_PlayerNear;
    bool m_IsPatrol;
    bool m_CaughtPlayer;

    Transform player;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        player = GameObject.FindGameObjectWithTag("Player").transform;

        m_IsPatrol = true;
        m_CaughtPlayer = false;
        m_PlayerInRange = false;

        m_WaitTime = startWaitTime;
        m_TimeToRotate = timeToRotate;

        m_CurrentWaypointIndex = 0;

        navMeshAgent.isStopped = false;
        navMeshAgent.speed = speedWalk;
        navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
    }

    void Update()
    {
        EnvironmentView();

        if (m_IsPatrol)
            Patrolling();
        else
            Chasing();
    }

    // ===============================
    // DETECTION SYSTEM
    // ===============================

    void EnvironmentView()
    {
        Collider[] playerInRange =
            Physics.OverlapSphere(transform.position, viewRadius, playerMask);

        m_PlayerInRange = false;

        for (int i = 0; i < playerInRange.Length; i++)
        {
            Transform target = playerInRange[i].transform;

            Vector3 dirToPlayer =
                (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
            {
                float dstToPlayer =
                    Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dirToPlayer,
                    dstToPlayer, obstacleMask))
                {
                    m_PlayerInRange = true;
                    m_IsPatrol = false;
                    m_PlayerPosition = target.position;
                }
            }
        }

        if (!m_PlayerInRange && !m_CaughtPlayer)
        {
            m_IsPatrol = true;
        }
    }

    // ===============================
    // PATROL
    // ===============================

    void Patrolling()
    {
        if (m_PlayerNear)
        {
            if (m_TimeToRotate <= 0)
            {
                Move(speedWalk);
                LookingPlayer(playerLastPosition);
            }
            else
            {
                Stop();
                m_TimeToRotate -= Time.deltaTime;
            }
        }
        else
        {
            navMeshAgent.SetDestination(
                waypoints[m_CurrentWaypointIndex].position);

            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                if (m_WaitTime <= 0)
                {
                    NextPoint();
                    Move(speedWalk);
                    m_WaitTime = startWaitTime;
                }
                else
                {
                    Stop();
                    m_WaitTime -= Time.deltaTime;
                }
            }
        }
    }

    // ===============================
    // CHASE
    // ===============================

    void Chasing()
    {
        m_PlayerNear = false;
        playerLastPosition = Vector3.zero;

        if (!m_CaughtPlayer)
        {
            Move(speedRun);
            navMeshAgent.SetDestination(m_PlayerPosition);
        }

        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            if (Vector3.Distance(transform.position, player.position) <= 2.5f)
            {
                m_CaughtPlayer = true;
                Stop();
            }
            else
            {
                m_IsPatrol = true;
                m_WaitTime = startWaitTime;
                m_TimeToRotate = timeToRotate;
                navMeshAgent.SetDestination(
                    waypoints[m_CurrentWaypointIndex].position);
            }

        }

        if (Vector3.Distance(transform.position, player.position) <= 2f)
    {
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    }

    // ===============================
    // MOVEMENT HELPERS
    // ===============================

    void Move(float speed)
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = speed;
    }

    void Stop()
    {
        navMeshAgent.isStopped = true;
        navMeshAgent.speed = 0;
    }

    void NextPoint()
    {
        m_CurrentWaypointIndex =
            (m_CurrentWaypointIndex + 1) % waypoints.Length;

        navMeshAgent.SetDestination(
            waypoints[m_CurrentWaypointIndex].position);
    }

    void LookingPlayer(Vector3 player)
    {
        navMeshAgent.SetDestination(player);

        if (Vector3.Distance(transform.position, player) <= 0.3f)
        {
            if (m_WaitTime <= 0)
            {
                m_PlayerNear = false;
                Move(speedWalk);
                navMeshAgent.SetDestination(
                    waypoints[m_CurrentWaypointIndex].position);
                m_WaitTime = startWaitTime;
                m_TimeToRotate = timeToRotate;
            }
            else
            {
                Stop();
                m_WaitTime -= Time.deltaTime;
            }
        }
    }



    void OnControllerColliderHit(ControllerColliderHit hit)
{
    if (hit.gameObject.CompareTag("Player"))
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

}

