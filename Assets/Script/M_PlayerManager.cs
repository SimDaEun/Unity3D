using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class M_PlayerManager : MonoBehaviour
{
    public static M_PlayerManager instance;
    //-----------------Player Move---------------------//
    float MoveWalkSpeed = 4.6f;  //�Ϲ� �̵��ӵ�
    float MoveRunSpeed = 8.0f; //�޸��� �̵��ӵ� 
    float JumpHeight = 10.0f; //���� ����
    float currentSpeed = 1.0f;  //���� �ӵ�
    bool isRunning = false;    // �޸��� �ִ��� Ȯ���ϱ� 
    float gravity = -9.81f;   //�߷�
    Vector3 velocity;         //���� �ӵ� ����
    CharacterController characterController;  //ĳ������Ʈ�ѷ�


    //-----------------------Camera Settings------------------------//
    Camera mainCamera;
    float mouseSensitivity = 100f;  //���콺 ����
    public Transform cameraTransfrom;  //ī�޶� Transform
    public float thirdPersonDistance = 3.0f; //3��Ī ��忡�� �÷��̾�� ī�޶� �þ� �Ÿ�
    public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, 0);  //3��Ī ��忡�� ī�޶� ������ - �׳� ī�޶��� ��ġ�� �������ִ� ���̶�� ��������
    float currentDistance;  //���� ī�޶���� �Ÿ�
    float targetDistance;  //��ǥ ī�޶� �Ÿ�
    float targetFov;  //��ǥFov
    public float defaultFov = 60f;  //�⺻ ī�޶� �þ߰�
    float pitch = 0f;  //�� �Ʒ� ȸ����
    float yaw = 0f;   //�¿� ȸ����
    bool isGround = false;   //���� �浹 ����
    public LayerMask groundLayer;


    //----------------FootStep-------------------------//
    public Transform leftFoot; //���� ��
    public Transform rightFoot; //������ �� 
    public float minMoveDistance = 0.01f; //�ּ� �Ÿ�
    Vector3 previousPosition;
    bool isLeftFootGround = false;
    bool isRightFootGround = false;

    Animator animator;


    //-------------GameObject---------------------//
    public GameObject confetti;
    public GameObject SuccessPanel;
    public GameObject FailPanel;
    public GameObject TimeOverPanel;
    public GameObject Timer;
    public GameObject MissionPanel;
    public GameObject AddTime;
    public GameObject JumpItem;

    private bool hasJumpItem = false;
    private bool isUsedAddTime = false;
    public bool isFloating = false;  //���߿� �� �ִ��� Ȯ���ϴ� ����
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;   //���콺 Ŀ���� ��� ����
        currentDistance = thirdPersonDistance;  //�ʱ� ī�޶� �Ÿ� ����
        targetDistance = thirdPersonDistance;  //��ǥ ī�޶� �Ÿ� ����
        targetFov = defaultFov;   //�ʱ� fov ����
        mainCamera = cameraTransfrom.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;  //�⺻ fov ����
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        confetti.SetActive(false);
        SuccessPanel.SetActive(false);
        FailPanel.SetActive(false);
        TimeOverPanel.SetActive(false);
        Timer.SetActive(false);
        MissionPanel.SetActive(false);
        AddTime.SetActive(false);
        JumpItem.SetActive(false);
    }

    void Update()
    {

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        //���콺 ȸ�� �� ��������
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;  //�¿� ȸ���� + ���콺 ȸ���� (���� y)
        pitch = 20f;  //���Ʒ� ȸ���� + ���콺 ȸ���� (����x)

        //pitch = Mathf.Clamp(pitch, -20f, 20f);  //���Ʒ� ȸ�� ���� 

        transform.rotation = Quaternion.Euler(0, yaw, 0); //�÷��̾ ���� �����̼�
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);



        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);

        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        if (!isFloating)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        characterController.Move(velocity * Time.deltaTime); //�߰� (�߷�)
        characterController.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.LeftShift))  //ShiftŰ ������ �޸��� �ִϸ��̼�
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

        currentSpeed = isRunning ? MoveRunSpeed : MoveWalkSpeed;  //�޸��� ���̸� currentSpeed�� �޸��� �ӵ�, �ƴϸ� �ȴ� �ӵ� ����
        animator.SetBool("isRunning", isRunning);


        CameraPosition();



        //���� ��ġ�� ���� ��ġ�� ���̸� ����ϴ� �Լ�
        float distanceMoved = Vector3.Distance(transform.position, previousPosition);

        bool isMoving = distanceMoved > minMoveDistance;//�̵� �߿� ���� ����
        if (isMoving)
        {
            bool leftHit = CheckGround(leftFoot);
            bool rightHit = CheckGround(rightFoot);

            if ((leftHit && !isLeftFootGround))
            {
                if (SoundManager.instance.sfxSource.isPlaying) return;
                SoundManager.instance.PlaySFX("Step", transform.position);
            }

            if ((rightHit && !isRightFootGround))
            {
                if (SoundManager.instance.sfxSource.isPlaying) return;
                SoundManager.instance.PlaySFX("Step", transform.position);
            }

            //���� ���¸� ���� �����Ӱ� ���ϱ� ���� ����
            isLeftFootGround = leftHit;
            isRightFootGround = rightHit;
        }


    }


    void CameraPosition()
    {
        transform.rotation = Quaternion.Euler(0, yaw, 0);
        Vector3 direction = new Vector3(0, 0, -currentDistance);
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);

        cameraTransfrom.position = transform.position + thirdPersonOffset + Quaternion.Euler(pitch,yaw,0) * direction;

        cameraTransfrom.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
    }

    void FloatingCameraPosition()
    { 

    }

    bool CheckGround(Transform foot)
    {
        Vector3 rayStart = foot.position + Vector3.down * 0.1f;

        bool hit = Physics.Raycast(rayStart, Vector3.down, 0.1f);

        Debug.DrawRay(rayStart, Vector3.down * 0.1f, hit ? Color.green : Color.red);

        return hit;
    }



    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.name == "Clock" && !isUsedAddTime)
        {
            //Sound ���
            hit.gameObject.SetActive(false);
            AddTime.SetActive(true);
            isUsedAddTime = true;
            StartCoroutine(ShowGameObj(AddTime, 1.0f));
            ItemClock();
        }

        if (hit.gameObject.name.StartsWith("soccer"))
        {
            hasJumpItem = true;
            hit.gameObject.SetActive(false);
            ItemBoots();
        }
    }

    private void OnTriggerExit(Collider other)  //Trigger���� ����� UI ��Ȱ��ȭ �ǵ��� �ϴ� �Լ�
    {
        if (other.tag == "StartEnd")
        {
            other.gameObject.SetActive(false);
        }
        if (other.name == "Start")
        {
            Timer.SetActive(true);
            StartCoroutine(ShowGameObj(MissionPanel,2.0f));
        }
        if(other.name == "End")
        {
            confetti.SetActive(true);
            SuccessPanel.SetActive(true);
        }

    }

    IEnumerator ShowGameObj(GameObject gameObj,float seconds)
    {
        gameObj.SetActive(true);

        yield return new WaitForSeconds(seconds);

        gameObj.SetActive(false);
    }

    private void ItemBoots()
    {
        if (!isFloating)
        {
            StartCoroutine(Floating(5.0f));
        }

    }

    private void ItemClock()
    {
        TimerManager.instance.LimitTime += 30;
    }
    IEnumerator Floating(float duration)
    {
        Debug.Log("���߿� �ߴ� �ڷ�ƾ ����");
        isFloating = true;
        float jumpMultiplier = 1.5f;

        Vector3 jump = new Vector3(0, JumpHeight, 0);
        velocity.y = Mathf.Sqrt(JumpHeight * -2f * gravity * Time.deltaTime) * jumpMultiplier;

        //������ �Ͻ������� ������ ���߿� �ӹ������� ��
        float originalGravity = gravity;
        gravity = 0;

        yield return new WaitForSeconds(duration);  

        gravity = originalGravity;
        isFloating = false;
    }
}