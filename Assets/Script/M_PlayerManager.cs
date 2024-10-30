using System.Collections;

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
    float gravity = -20f;   //�߷�
    Vector3 velocity;         //���� �ӵ� ����
    CharacterController characterController;  //ĳ������Ʈ�ѷ�
    public GameObject PlayerHead;

    float mouseX;
    float mouseY;

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
    public GameObject JumpPanel;
    public GameObject MissionPanel;
    public GameObject AddTime;
    public GameObject HelpPanel;

    private bool isUsedAddTime = false;
    public bool isFloating = false;  //���߿� �� �ִ��� Ȯ���ϴ� ����
    public bool isShaking = false;

    [Header("CameraShake Setting")]
    public float shakeDuration = 0.3f;  //��鸲 ���� �ð�
    public float shakeMagnitude = 0.2f; //��鸲 ����
    

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if(instance != null)
        {
            Destroy(gameObject);
            //instance = this;
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

        //SuccessPanel.SetActive(false);
        //FailPanel.SetActive(false);
        //TimeOverPanel.SetActive(false);
        //confetti.SetActive(false);
        //JumpPanel.SetActive(false);
        //MissionPanel.SetActive(false);
        //AddTime.SetActive(false);
    }

    void OnLevelWasLoaded()
    {
        Debug.Log("OnLevelWasLoaded");
        CharacterController();
        if(!SuccessPanel)
        {
            SuccessPanel = GameObject.Find("Canvas").transform.GetChild(3).gameObject;
            SuccessPanel.SetActive(false);
        }


        if (!FailPanel)
        {
            FailPanel = GameObject.Find("Canvas").transform.GetChild(4).gameObject;
            FailPanel.SetActive(false);
        }

     

        if (!confetti)
        {
            confetti = GameObject.Find("MapItem").transform.GetChild(2).gameObject;
            confetti.SetActive(false);
        }


        if (!Timer)
        {
            Timer = GameObject.Find("Canvas").transform.GetChild(0).gameObject;
        }


        if (!TimeOverPanel)
        {
            TimeOverPanel = GameObject.Find("Canvas").transform.GetChild(1).gameObject;
            TimeOverPanel.SetActive(false);
        }


        if (!JumpPanel)
        {
            JumpPanel = GameObject.Find("Canvas").transform.GetChild(5).gameObject;
            JumpPanel.SetActive(false);
        }


        if (!MissionPanel)
        {
            MissionPanel = GameObject.Find("Canvas").transform.GetChild(6).gameObject;
            MissionPanel.SetActive(false);
        }

        if (!HelpPanel)
        {
            HelpPanel = GameObject.Find("Canvas").transform.GetChild(7).gameObject;
            HelpPanel.SetActive(false);
        }

        if (!AddTime)
        {
            AddTime = GameObject.Find("Canvas").transform.GetChild(8).gameObject;
            AddTime.SetActive(false);   
        }

        Time.timeScale = 1;
    }

    void Update()
    {

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        //���콺 ȸ�� �� ��������
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;



        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);

        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        if (!isFloating)
        {
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(move * currentSpeed * Time.deltaTime);
        }
        characterController.Move(velocity * Time.deltaTime); //�߰� (�߷�)



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

        if (!isFloating)
        {
            CameraPosition();
        }
        else
        {
            FloatingCameraPosition();
        }

        if (!isFloating && CheckGround(leftFoot))
        {
            isFloating = false;
        }

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
        if (cameraTransfrom == null)
        {
            Debug.LogWarning("cameraTransfrom is null. CameraPosition not updated.");
            return;
        }
        yaw += mouseX;  //�¿� ȸ���� + ���콺 ȸ���� (���� y)
        pitch = 20f;  //���Ʒ� ȸ���� + ���콺 ȸ���� (����x)
        //pitch = Mathf.Clamp(pitch, -20f, 20f);  //���Ʒ� ȸ�� ���� 

        transform.rotation = Quaternion.Euler(0, yaw, 0); //�÷��̾ ���� �����̼�
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);


        transform.rotation = Quaternion.Euler(0, yaw, 0);
        Vector3 direction = new Vector3(0, 0, -currentDistance);
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);

        cameraTransfrom.position = transform.position + thirdPersonOffset + Quaternion.Euler(pitch,yaw,0) * direction;

        cameraTransfrom.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
    }

    void FloatingCameraPosition()
    {
        yaw += mouseX;  //�¿� ȸ���� + ���콺 ȸ���� (���� y)
        pitch = 30f;  //���Ʒ� ȸ���� + ���콺 ȸ���� (����x)
        //pitch = Mathf.Clamp(pitch, -45f, 45f);  //���Ʒ� ȸ�� ���� 

        transform.rotation = Quaternion.Euler(0, yaw, 0); //�÷��̾ ���� �����̼�
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);


        transform.rotation = Quaternion.Euler(0, yaw, 0);
        Vector3 direction = new Vector3(0, 0, -currentDistance);
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);

        //cameraTransfrom.position = transform.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;
        cameraTransfrom.position = PlayerHead.transform.position; //�÷��̾� �Ӹ� ��ġ�� ī�޶� �̵�
        //cameraTransfrom.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
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
            hit.gameObject.SetActive(false);
            StartCoroutine(ShowGameObj(JumpPanel, 1.0f));
            ItemBoots();
        }

        if (hit.gameObject.tag == "Enemy")
        {
            Time.timeScale = 0;
            FailPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;   //���콺 Ŀ�� Ǯ�� ����
            //StartCoroutine(Shake(shakeDuration, shakeMagnitude));
            SoundManager.instance.StopBGM();
            
        }
    }

    public void CharacterController()
    {
        if (gameObject.GetComponent<CharacterController>() != null)
        {
            gameObject.GetComponent<CharacterController>().enabled = false;
            gameObject.transform.position = GameManager.instance.DefaultPos.transform.position;
            gameObject.GetComponent<CharacterController>().enabled = true;
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
            Debug.Log("Start���� ����");
            if (Timer != null)
            {
                Timer.SetActive(true);
                StartCoroutine(ShowGameObj(MissionPanel, 2.0f));
                SoundManager.instance.PlayBGM("GameBGM");
            }
            else
            {
                Debug.Log("Timer is null");
            }

        }
        if(other.name == "End")
        {
            confetti.SetActive(true);
            SuccessPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;   //���콺 Ŀ�� Ǯ�� ����
            SoundManager.instance.StopBGM();
        }

    }

    IEnumerator ShowGameObj(GameObject gameObj,float seconds)  //���� ������ �Ծ����� n�ʰ� ȭ�鿡 ���
    {
        if (gameObj == null)
        {
            Debug.Log("ShowGameObj: gameObj is null or has been destroyed.");
            yield break;
        }

        gameObj.SetActive(true);
        yield return new WaitForSeconds(seconds);

        if (gameObj != null)  // �ٽ� �ѹ� null Ȯ��
        {
            gameObj.SetActive(false);
        }
        //if (gameObj != null)
        //{
        //    gameObj.SetActive(true);

        //    yield return new WaitForSeconds(seconds);

        //    gameObj.SetActive(false);
        //}
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
        FloatingCameraPosition();
        Debug.Log("���߿� �ߴ� �ڷ�ƾ ����");
        isFloating = true;
        float jumpMultiplier = 1.5f;

        Vector3 jump = new Vector3(0, JumpHeight, 0);
        velocity.y = Mathf.Sqrt(JumpHeight * -2f * gravity * Time.deltaTime) * jumpMultiplier;
        velocity.x = 0;
        velocity.z = 0;

        //������ �Ͻ������� ������ ���߿� �ӹ������� ��
        float originalGravity = gravity;
        gravity = 0;

        yield return new WaitForSeconds(duration);  

        gravity = originalGravity;
        isFloating = false;
    }


}