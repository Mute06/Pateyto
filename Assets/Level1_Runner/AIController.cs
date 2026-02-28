using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class AIController : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;

    [Header("Audio")]
    public AudioSource audioSource;     // TEK Audio Source
    public AudioClip patrolClip;
    public AudioClip chaseClip;
    public AudioClip hitClip;

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
    float m_WaitTime;
    float m_TimeToRotate;

    bool m_PlayerInRange;
    bool m_IsPatrol;
    bool m_CaughtPlayer;

    Transform player;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        // Eğer AudioSource inspector’dan atanmadıysa otomatik ekle
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        m_IsPatrol = true;
        m_CaughtPlayer = false;

        m_WaitTime = startWaitTime;
        m_TimeToRotate = timeToRotate;

        m_CurrentWaypointIndex = 0;

        navMeshAgent.speed = speedWalk;
        navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);

        PlayPatrolSound();
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
    // SOUND SYSTEM
    // ===============================

    void PlayPatrolSound()
    {
        if (audioSource.clip == patrolClip) return;

        audioSource.loop = true;
        audioSource.clip = patrolClip;
        audioSource.Play();
    }

    void PlayChaseSound()
    {
        if (audioSource.clip == chaseClip) return;

        audioSource.loop = true;
        audioSource.clip = chaseClip;
        audioSource.Play();
    }

    void PlayHitSound()
    {
        audioSource.loop = false;
        audioSource.PlayOneShot(hitClip);
    }

    // ===============================
    // DETECTION
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
                    PlayChaseSound();
                }
            }
        }

        if (!m_PlayerInRange && !m_CaughtPlayer)
        {
            m_IsPatrol = true;
            PlayPatrolSound();
        }
    }

    // ===============================
    // PATROL
    // ===============================

    void Patrolling()
    {
        navMeshAgent.speed = speedWalk;
        navMeshAgent.SetDestination(
            waypoints[m_CurrentWaypointIndex].position);

        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            if (m_WaitTime <= 0)
            {
                NextPoint();
                m_WaitTime = startWaitTime;
            }
            else
            {
                m_WaitTime -= Time.deltaTime;
            }
        }
    }

    // ===============================
    // CHASE
    // ===============================

    void Chasing()
{
    if (!m_CaughtPlayer)
    {
        navMeshAgent.speed = speedRun;
        navMeshAgent.SetDestination(m_PlayerPosition);

        if (Vector3.Distance(transform.position, player.position) <= 2f)
        {
            m_CaughtPlayer = true;

            navMeshAgent.isStopped = true;   // Hareketi durdur
            PlayHitSound();
            P_SceneManager.Instance.ReloadLevelWithFade(3);
        }
    }
}

    void NextPoint()
    {
        m_CurrentWaypointIndex =
            (m_CurrentWaypointIndex + 1) % waypoints.Length;
    }
}