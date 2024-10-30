using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.AI;

public class BearAI : MonoBehaviour
{
    //public static BearAI instance;
    public enum BearState
    {
       Idle, Patrol,Chase, Attack
    }

    public BearState currentState;

    public Transform[] patrolPoints; //순찰 지점들
    GameObject Player; //플레이어의 Transform
    public float detectionRange = 7f;  //플레이어 감지 범위
    public float attackRange = 3.0f;   //공격 범위
    public Transform handTransform;  //곰 손 위치
    private NavMeshAgent agent;
    public float shpereCastRadius = 0.5f;
    private bool isAttacking = false;  //공격중인지 나타내는 변수
    private Animator animator;
    private bool patrollingForward = true;
    private int currentPatrolIndex = 0;
    public float idleTime = 2.0f;
    private float idleTimer = 0f;
    private float attackCooldown; //공격 대기 시간 

/*    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            instance = this;
        }

    }*/
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        this.Player = GameObject.Find("Player");

        agent.speed = 3.0f;
        attackCooldown = 0.5f;

        currentState = BearState.Patrol;
    }

    // Update is called once per frame
    void Update()
    {
        //플레이어와의 거리 
        float distanceToPlayer = Vector3.Distance(Player.transform.position, transform.position);

        //플레이어가 감지 범위 안에 있고 공격 중이 아닐 때 추적 
        if (distanceToPlayer <= detectionRange && !isAttacking)
        {
            currentState = BearState.Chase;  //플레이어 추적
        }

        //플레이어가 공격범위 안에 있고 공격 중이 아닐 때 공격 
        if (distanceToPlayer <= attackRange && !isAttacking)
        {
            //딜레이를 고려해서 공격할 수 있는 상태가 아니면 함수가 실행되지 않도록 함.
            if (!isAttacking)
            {
                currentState = BearState.Attack;
            }
        }

        switch (currentState)
        {
            case BearState.Idle:
                Idle();
                break;
            case BearState.Patrol:
                Patrol();
                break;
            case BearState.Chase:
                ChasePlayer();
                break;
            case BearState.Attack:
                AttackPlayer();
                break;

        }
        
    }


    private void OnLevelWasLoaded()
    {
        Debug.Log("Bear OnLevelWasLoaded");
        this.Player = GameObject.Find("Player");
    }
    void Patrol()
    {
        Debug.Log("Bear Patrol");
        if (patrolPoints.Length == 0)
        {
            return;
        }

        agent.isStopped = false;
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", true);

        //현재 목표 지점으로 이동
        agent.destination = patrolPoints[currentPatrolIndex].position;

        //순찰 지점에 도착했을 때, Idle상태로 전환 
        if (agent.remainingDistance < 0.5f && !agent.pathPending)
        {
            currentState = BearState.Idle;
        }
    }
    void Idle()
    {
        Debug.Log("Bear Idle");
        animator.SetBool("isWalking", false);
        animator.SetBool("isIdle", true);

        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTime)
        {
            idleTimer = 0;
        }

        MoveToNextPatrolPoint();
    }

    void MoveToNextPatrolPoint()
    {
        if (patrollingForward) //다음 순찰 지점으로 인덱스 이동
        {
            currentPatrolIndex++;
            if (currentPatrolIndex >= patrolPoints.Length)
            {
                currentPatrolIndex = patrolPoints.Length - 2;  //마지막 지점에서 돌아감 
                patrollingForward = false;
            }
        }
        else
        {
            currentPatrolIndex--;
            if (currentPatrolIndex < 0)
            {
                currentPatrolIndex = 1;  // 첫 지점에서 다시 전진 
                patrollingForward = true;
            }
        }

        currentState = BearState.Patrol;  // 다시 순찰 상태 전환
    }
    void ChasePlayer()
    {
        Debug.Log("Bear Chase");
        agent.isStopped = false;  //isStopped : 에이전트의 이동을 일시적으로 이동 제어 (true경로를 따라가는 걸 멈추고,false 다시 경로 이동)
        agent.destination = Player.transform.position;  //Destination : 에이전트가 이동할 목표 위치 지정 속성 
        animator.SetBool("isWalking", true);
        animator.SetBool("isIdle", false);
    }
    void AttackPlayer()
    {
        Debug.Log("Bear Attack");
        if (isAttacking)  //공격중이라면 다시 공격하지 않음 
        {
            return;
        }

        isAttacking = true;

        agent.isStopped = true; //공격 중에는 멈춤

        animator.SetBool("isWalking", false); 
        animator.SetTrigger("Attack");

        //플레이어를 바라보고 공격하도록 플레이어 방향으로 회전
        Vector3 direction = (Player.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        //Slerp : 두 개의 Quaternion을 부드럽게 보간하는 함수 

        PerformAttack();
    }

    public void PerformAttack()
    {
        float sphereRadius = 1.0f; // 반경 
        Vector3 attackCenter = handTransform.position;

        Collider[] hits = Physics.OverlapSphere(attackCenter, sphereRadius);
        bool playerHit = false;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log("Player Hit");
                playerHit = true;
                M_PlayerManager.instance.FailPanel.SetActive(true);
                Cursor.lockState = CursorLockMode.None;   //마우스 커서 풀린 상태
                Time.timeScale = 0;
                SoundManager.instance.StopBGM();
            }
        }
    }

    // 공격 종료 시 호출되는 함수 (공격 애니메이션이 끝날 때 애니메이션 이벤트로 호출되는 함수)
    public void EndAttack()
    {
        Debug.Log("EndAttack");
        isAttacking = false;  //공격 상태 해제 
        agent.isStopped = false;

        float distanceToPlayer = Vector3.Distance(Player.transform.position, transform.position);

        if (distanceToPlayer <= attackRange)   //공격 범위 내에 있다면 다시 공격 시작 
        {
            AttackPlayer();
            StartCoroutine(AttackCooldown()); //공격 후 딜레이
        }
        else
        {
            if (distanceToPlayer <= detectionRange)  //공격 범위에서 벗어났으면 추적 또는 순찰모드로 
            {
                currentState = BearState.Chase;
            }
            else
            {
                currentState = BearState.Idle;
            }
            animator.SetBool("isWalking", true);
        }
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }
}
