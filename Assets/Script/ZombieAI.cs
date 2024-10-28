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
    public enum ZombieState  //������ ���¸� ������ 
    {
        Patrol, Chase, Attack, Idle, Damage, Die
    }

    public ZombieState currentState; //���� ���� 

    public enum ZombieType  //���� Ÿ��
    {
        ZombieType1, ZombieType2, ZombieType3
    }

    public ZombieType zombieType;  //���� Ÿ���� �������ִ� ���� 

    public Transform[] patrolPoints;  //���� ������ 
    private Transform player;  //�÷��̾��� Transform
    public float detectionRange = 10f; //�÷��̾ �����ϴ� ����
    public float attackRange = 2.0f; //���� ���� 
    public float hp = 100;  //�⺻ü��(��, ZombieType���� �ٸ�)
    public int damage = 1;  //�÷��̾�� �ִ� ������ 
    public Transform handTransform;  //������ �� ��ġ 
    private NavMeshAgent agent;
    public float sphereCastRadius = 0.5f;  //Cast�ݰ� 
    private int currentPatrolIndex = 0;  
    private bool isAttacking;  //���� �������� ��Ÿ���� ����
    private Animator animator;
    public LayerMask attackLayermask; //���� ��� ���̾� ���� 
    private bool patrollingForward = true;  //���� ���� �պ��� ���� ������� 
    public float idleTime = 2.0f; //���� �������� ����ϴ� �ð� 
    private float idleTimer = 0; //Idle �ִϸ��̼� ��� �ð� 
    private float attackCooldown = 0f;  //���� ��� �ð� 

    private bool isJumping = false;
    private Rigidbody rb;
    public float jumpHeight = 2.0f;  //���� ���� 
    public float jumpDuration = 1.0f;  //���� �ð� 
    private NavMeshLink[] navMeshLinks;

    void Start()
    {
        if (isJumping) return;  //���� �߿��� �ٸ� ������ ���� ���� 

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = PlayerManager.Instance.transform;

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
           rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;

        navMeshLinks = FindObjectsOfType<NavMeshLink>();  //��� NavMeshLink�� ã�´�
        //���� Ÿ�� ����
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
        //�÷��̾���� �Ÿ� 
        float distanceToPlayer = Vector3.Distance(player.position ,transform.position);

        //�÷��̾ ���� ���� �ȿ� �ְ� ���� ���� �ƴ� �� ���� 
        if (distanceToPlayer <= detectionRange && !isAttacking)
        {
            currentState = ZombieState.Chase; //�÷��̾� ����
        }

        //�÷��̾ ���� ���� �ȿ� �ְ� ���� ���� �ƴ� �� ���� 
        if (distanceToPlayer <= attackRange && !isAttacking) 
        {
            //�����̸� ����ؼ� ������ �� �ִ� ���°� �ƴϸ� �Լ��� ������� �ʵ��� ������ �߰��� �� 
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

        //���� ��ǥ �������� �̵� 
        agent.destination = patrolPoints[currentPatrolIndex].position;
        //distination : �������� �����ϴ� �Ӽ�, AI�� �� ��ǥ�� ��ǥ�� ��θ� ����ؼ� �̵� 

        //��ֹ��� �ְų� NavMeshLink�� ��������� ���� 
        if (agent.isOnOffMeshLink)
        {
            StartCoroutine(JumpAcrossLink());
        }

        //���������� �������� �� Idle���·� ��ȯ,remainingDistance : AI ��ǥ ���������� ���� �Ÿ�, �ǽð� ��� 
        if (agent.remainingDistance < 0.5f && !agent.pathPending)  //pathPending : ��ΰ� ��������� ���θ� 
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
        if (patrollingForward)  //���� ���� �������� �ε����� �̵� 
        {
            currentPatrolIndex++;
            if (currentPatrolIndex >= patrolPoints.Length)
            {
                currentPatrolIndex = patrolPoints.Length - 2;  //������ �������� ���ư� 
                patrollingForward = false;
            }
        }

        else
        {
            currentPatrolIndex--;
            if (currentPatrolIndex < 0)
            {
                currentPatrolIndex = 1;  // ù �������� �ٽ� ���� 
                patrollingForward = true;
            }
        }

        currentState = ZombieState.Patrol;  // �ٽ� ���� ���� ��ȯ
    }

    void ChasePlayer()
    {
        agent.isStopped = false;  //isStopped : ������Ʈ�� �̵��� �Ͻ������� �̵� ���� (true��θ� ���󰡴� �� ���߰�,false �ٽ� ��� �̵�)
        agent.destination = player.position;  //Destination : ������Ʈ�� �̵��� ��ǥ ��ġ ���� �Ӽ� 
        animator.SetBool("IsWalking", true);
        animator.SetBool("IsIdle", false);
    }

    void AttackPlayer()
    {
        if (isAttacking) return;  //���� ���̶�� �ٽ� �������� ����

        isAttacking = true;

        agent.isStopped = true; //���� �߿��� ���� 

        animator.SetBool("IsWalking", false);
        animator.SetTrigger("Attack");

        //�÷��̾ �ٶ󺸰� �����ϵ��� �÷��̾� �������� ȸ��
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        //Slerp : �� ���� Quaternion�� �ε巴�� �����ϴ� �Լ� 

        PerformAttack();
    }

    public void PerformAttack()
    {
        Vector3 attackDirection = player.position - handTransform.position;
        attackDirection.Normalize();

        RaycastHit hit;
        float sphereRadius = 1.0f; //�ݰ� 
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

    // ���� ���� �� ȣ��Ǵ� �Լ� (���� �ִϸ��̼��� ���� �� �ִϸ��̼� �̺�Ʈ�� ȣ��Ǵ� �Լ�)
    public void EndAttack()
    {
        Debug.Log("EndAttack");
        isAttacking = false;  //���� ���� ���� 
        agent.isStopped = false;

        float distanceToPlayer = Vector3.Distance(player.position, transform.position);

        if (distanceToPlayer <= attackRange)   //���� ���� ���� �ִٸ� �ٽ� ���� ���� 
        {
            AttackPlayer();
            StartCoroutine(AttackCooldown()); //���� �� ������
        }
        else
        {
            if (distanceToPlayer <= detectionRange)  //���� �������� ������� ���� �Ǵ� �������� 
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

        //NavMeshLink ���۰� �� ��ǥ �������� 
        OffMeshLinkData linkData = agent.currentOffMeshLinkData;
        Vector3 startPos = linkData.startPos;
        Vector3 endPos = linkData.endPos;


        //���� ��� ��� ( �������� �׸��� ���� )
        float elapsedTime = 0;
        while (elapsedTime < jumpDuration)
        {
            float t = elapsedTime / jumpDuration;
            Vector3 currentPosition = Vector3.Lerp(startPos, endPos, t);
            currentPosition.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;  //������ ���
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
