using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq.Expressions;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.GlobalIllumination;
public class ZombieAI : MonoBehaviour
{
    public enum ZombieState  //좀비의 상태를 관리함 
    {
        Patrol, Chase, Attack, Idle, Damage, Die
    }

    public ZombieState currentState; //현재 상태 

    public enum ZombieType  //좀비 타입
    {
        ZombieType1, ZombieType2, ZombieType3
    }

    public ZombieType zombieType;  //좀비 타입을 지정해주는 변수 

    public Transform[] patrolPoints;  //순찰 지점들 
    private Transform player;  //플레이어의 Transform
    public float detectionRange = 10f; //플레이어를 감지하는 범위
    public float attackRange = 2.0f; //공격 범위 
    public float hp = 100;  //기본체력(단, ZombieType마다 다름)
    public int damage = 1;  //플레이어에게 주는 데미지 
    public Transform handTransform;  //좀비의 손 위치 
    private NavMeshAgent agent;
    public float sphereCastRadius = 0.5f;  //Cast반경 
    private int currentPatrolIndex = 0;  
    private bool isAttacking;  //공격 중인지를 나타내는 변수
    private Animator animator;
    public LayerMask attackLayermask; //공격 대상 레이어 설정 
    private bool patrollingForward = true;  //순찰 지점 왕복을 위한 방향관리 
    public float idleTime = 2.0f; //순찰 지점에서 대기하는 시간 
    private float idleTimer = 0; //Idle 애니메이션 대기 시간 
    private float attackCooldown = 0f;  //공격 대기 시간 

    private bool isJumping = false;
    private Rigidbody rb;
    public float jumpHeight = 2.0f;  //점프 높이 
    public float jumpDuration = 1.0f;  //점프 시간 
    private NavMeshLink[] navMeshLinks;

    void Start()
    {
        if (isJumping) return;  //점프 중에는 다른 동작을 하지 않음 

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = PlayerManager.Instance.transform;

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
           rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;

        navMeshLinks = FindObjectsOfType<NavMeshLink>();  //모든 NavMeshLink를 찾는다
        //좀비 타입 구분
        if (zombieType == ZombieType.ZombieType1)
        {
            agent.speed = 1.0f;
            attackCooldown = 1.0f;
            damage = 1;
            hp = 100;
        }
        else if (zombieType == ZombieType.ZombieType2)
        {
            agent.speed = 2.0f;
            attackCooldown = 1.5f;
            damage = 2;
            hp = 150;
        }
        else if (zombieType != ZombieType.ZombieType3)
        {
            agent.speed = 3.0f;
            attackCooldown = 2.0f;
            damage = 3;
            hp = 200;
        }
        currentState = ZombieState.Patrol;
    }


    void Update()
    {
        //플레이어와의 거리 
        float distanceToPlayer = Vector3.Distance(player.position ,transform.position);

        //플레이어가 감지 범위 안에 있고 공격 중이 아닐 때 추적 
        if (distanceToPlayer <= detectionRange && !isAttacking)
        {
            currentState = ZombieState.Chase; //플레이어 추적
        }

        //플레이어가 공격 범위 안에 있고 공격 중이 아닐 때 공격 
        if (distanceToPlayer <= attackRange && !isAttacking) 
        {
            //딜레이를 고려해서 공격할 수 있는 상태가 아니면 함수가 실행되지 않도록 조건을 추가한 것 
            if (!isAttacking)
            {
                currentState = ZombieState.Attack;
            }
        }

        switch (currentState)
        {
            case ZombieState.Patrol:
                Patrol();
                break;
            case ZombieState.Chase:
                ChasePlayer();
                break;
            case ZombieState.Attack:
                break;
            case ZombieState.Idle:
                Idle();
                break;
        }
    }

    void Patrol()
    {
        Debug.Log("Zombie Patrol");
        if (patrolPoints.Length == 0)
        {
            return;
        }

        agent.isStopped = false;
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsWalking", true);

        //현재 목표 지점으로 이동 
        agent.destination = patrolPoints[currentPatrolIndex].position;
        //distination : 목적지를 설정하는 속성, AI가 그 좌표를 목표로 경로를 계산해서 이동 

        //장애물이 있거나 NavMeshLink에 가까워지면 점프 
        if (agent.isOnOffMeshLink)
        {
            StartCoroutine(JumpAcrossLink());
        }

        //순찰지점에 도착했을 때 Idle상태로 전환,remainingDistance : AI 목표 목적지까지 남은 거리, 실시간 계산 
        if (agent.remainingDistance < 0.5f && !agent.pathPending)  //pathPending : 경로가 계산중인지 여부를 
        {
            currentState = ZombieState.Idle;
        }
    }

    void Idle()
    {
        Debug.Log("Zombie Idle");
        animator.SetBool("IsWaling", false);
        animator.SetBool("IsIdle", true);

        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTime)
        {
            idleTimer = 0;
        }

        MoveToNextPatrolPoint();
    }

    void MoveToNextPatrolPoint()
    {
        if (patrollingForward)  //다음 순찰 지점으로 인덱스를 이동 
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

        currentState = ZombieState.Patrol;  // 다시 순찰 상태 전환
    }

    void ChasePlayer()
    {
        agent.isStopped = false;  //isStopped : 에이전트의 이동을 일시적으로 이동 제어 (true경로를 따라가는 걸 멈추고,false 다시 경로 이동)
        agent.destination = player.position;  //Destination : 에이전트가 이동할 목표 위치 지정 속성 
        animator.SetBool("IsWalking", true);
        animator.SetBool("IsIdle", false);
    }

    void AttackPlayer()
    {
        if (isAttacking) return;  //공격 중이라면 다시 공격하지 않음

        isAttacking = true;

        agent.isStopped = true; //공격 중에는 멈춤 

        animator.SetBool("IsWalking", false);
        animator.SetTrigger("Attack");

        //플레이어를 바라보고 공격하도록 플레이어 방향으로 회전
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        //Slerp : 두 개의 Quaternion을 부드럽게 보간하는 함수 

        PerformAttack();
    }

    public void PerformAttack()
    {
        Vector3 attackDirection = player.position - handTransform.position;
        attackDirection.Normalize();

        RaycastHit hit;
        float sphereRadius = 1.0f; //반경 
        float castDistance = attackRange;

        if (Physics.SphereCast(handTransform.position, sphereRadius, attackDirection, out hit, castDistance, attackLayermask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("Player Hit");
            }
        }
        Debug.DrawRay(handTransform.position, attackDirection * castDistance, Color.red, 1.0f);

    }

    // 공격 종료 시 호출되는 함수 (공격 애니메이션이 끝날 때 애니메이션 이벤트로 호출되는 함수)
    public void EndAttack()
    {
        Debug.Log("EndAttack");
        isAttacking = false;  //공격 상태 해제 
        agent.isStopped = false;

        float distanceToPlayer = Vector3.Distance(player.position, transform.position);

        if (distanceToPlayer <= attackRange)   //공격 범위 내에 있다면 다시 공격 시작 
        {
            AttackPlayer();
            StartCoroutine(AttackCooldown()); //공격 후 딜레이
        }
        else
        {
            if (distanceToPlayer <= detectionRange)  //공격 범위에서 벗어났으면 추적 또는 순찰모드로 
            {
                currentState = ZombieState.Chase;
            }
            else
            {
                currentState = ZombieState.Idle;
            }
            animator.SetBool("IsWalking", true);
        }
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    IEnumerator JumpAcrossLink()
    {
        Debug.Log("Zombie Jump");
        isJumping = true;
        agent.isStopped = true;

        //NavMeshLink 시작과 끝 좌표 가져오기 
        OffMeshLinkData linkData = agent.currentOffMeshLinkData;
        Vector3 startPos = linkData.startPos;
        Vector3 endPos = linkData.endPos;


        //점프 경로 계산 ( 포물선을 그리며 점프 )
        float elapsedTime = 0;
        while (elapsedTime < jumpDuration)
        {
            float t = elapsedTime / jumpDuration;
            Vector3 currentPosition = Vector3.Lerp(startPos, endPos, t);
            currentPosition.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;  //포물선 경로
            transform.position = currentPosition;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;

        agent.CompleteOffMeshLink();
        agent.isStopped = false;

        isJumping = false;
    }

    public void TakeDamage(float amount, string hitPart)
    {
        if (hitPart == "Head")
        {
            amount *= 2.0f;
            animator.SetTrigger("HeadHit");
            Debug.Log("HeadHit");
        }
        else if (hitPart == "Zombie")
        {
            animator.SetTrigger("BodyHit");
            Debug.Log("BodyHit");
        }
        else
        {
            animator.SetTrigger("Hit");
            Debug.Log("Hit");
        }

        hp -= amount;
        if (hp < 0)
        {
            //Die
        }
    }

    void Die()
    {
        animator.SetTrigger("Die");
        agent.isStopped = true;
    }
}
