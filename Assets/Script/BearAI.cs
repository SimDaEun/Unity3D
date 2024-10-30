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

    public Transform[] patrolPoints; //���� ������
    GameObject Player; //�÷��̾��� Transform
    public float detectionRange = 7f;  //�÷��̾� ���� ����
    public float attackRange = 3.0f;   //���� ����
    public Transform handTransform;  //�� �� ��ġ
    private NavMeshAgent agent;
    public float shpereCastRadius = 0.5f;
    private bool isAttacking = false;  //���������� ��Ÿ���� ����
    private Animator animator;
    private bool patrollingForward = true;
    private int currentPatrolIndex = 0;
    public float idleTime = 2.0f;
    private float idleTimer = 0f;
    private float attackCooldown; //���� ��� �ð� 

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
        //�÷��̾���� �Ÿ� 
        float distanceToPlayer = Vector3.Distance(Player.transform.position, transform.position);

        //�÷��̾ ���� ���� �ȿ� �ְ� ���� ���� �ƴ� �� ���� 
        if (distanceToPlayer <= detectionRange && !isAttacking)
        {
            currentState = BearState.Chase;  //�÷��̾� ����
        }

        //�÷��̾ ���ݹ��� �ȿ� �ְ� ���� ���� �ƴ� �� ���� 
        if (distanceToPlayer <= attackRange && !isAttacking)
        {
            //�����̸� ����ؼ� ������ �� �ִ� ���°� �ƴϸ� �Լ��� ������� �ʵ��� ��.
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

        //���� ��ǥ �������� �̵�
        agent.destination = patrolPoints[currentPatrolIndex].position;

        //���� ������ �������� ��, Idle���·� ��ȯ 
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
        if (patrollingForward) //���� ���� �������� �ε��� �̵�
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

        currentState = BearState.Patrol;  // �ٽ� ���� ���� ��ȯ
    }
    void ChasePlayer()
    {
        Debug.Log("Bear Chase");
        agent.isStopped = false;  //isStopped : ������Ʈ�� �̵��� �Ͻ������� �̵� ���� (true��θ� ���󰡴� �� ���߰�,false �ٽ� ��� �̵�)
        agent.destination = Player.transform.position;  //Destination : ������Ʈ�� �̵��� ��ǥ ��ġ ���� �Ӽ� 
        animator.SetBool("isWalking", true);
        animator.SetBool("isIdle", false);
    }
    void AttackPlayer()
    {
        Debug.Log("Bear Attack");
        if (isAttacking)  //�������̶�� �ٽ� �������� ���� 
        {
            return;
        }

        isAttacking = true;

        agent.isStopped = true; //���� �߿��� ����

        animator.SetBool("isWalking", false); 
        animator.SetTrigger("Attack");

        //�÷��̾ �ٶ󺸰� �����ϵ��� �÷��̾� �������� ȸ��
        Vector3 direction = (Player.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        //Slerp : �� ���� Quaternion�� �ε巴�� �����ϴ� �Լ� 

        PerformAttack();
    }

    public void PerformAttack()
    {
        float sphereRadius = 1.0f; // �ݰ� 
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
                Cursor.lockState = CursorLockMode.None;   //���콺 Ŀ�� Ǯ�� ����
                Time.timeScale = 0;
                SoundManager.instance.StopBGM();
            }
        }
    }

    // ���� ���� �� ȣ��Ǵ� �Լ� (���� �ִϸ��̼��� ���� �� �ִϸ��̼� �̺�Ʈ�� ȣ��Ǵ� �Լ�)
    public void EndAttack()
    {
        Debug.Log("EndAttack");
        isAttacking = false;  //���� ���� ���� 
        agent.isStopped = false;

        float distanceToPlayer = Vector3.Distance(Player.transform.position, transform.position);

        if (distanceToPlayer <= attackRange)   //���� ���� ���� �ִٸ� �ٽ� ���� ���� 
        {
            AttackPlayer();
            StartCoroutine(AttackCooldown()); //���� �� ������
        }
        else
        {
            if (distanceToPlayer <= detectionRange)  //���� �������� ������� ���� �Ǵ� �������� 
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
