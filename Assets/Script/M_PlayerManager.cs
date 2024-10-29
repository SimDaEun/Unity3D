using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Cinemachine.DocumentationSortingAttribute;

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

        Timer.SetActive(false);
        SuccessPanel.SetActive(false);
        FailPanel.SetActive(false);
        TimeOverPanel.SetActive(false);
        confetti.SetActive(false);
        JumpPanel.SetActive(false);
        MissionPanel.SetActive(false);
        AddTime.SetActive(false);
    }

    void OnLevelWasLoaded()
    {
        Time.timeScale = 1;
        if (SuccessPanel != null)
        {
            SuccessPanel.SetActive(false);
        }
        else
        {
            SuccessPanel = GameObject.Find("SuccessPanel");
            SuccessPanel.SetActive(false);
        }


        if (FailPanel != null)
        {
            FailPanel.SetActive(false);
        }
        else
        {
            FailPanel = GameObject.Find("FailPanel");
            FailPanel.SetActive(false);
        }

        if (confetti != null)
        {
            confetti.SetActive(false);
        }
        else
        {
            confetti = GameObject.Find("confetti");
            confetti.SetActive(false);
        }

        if (Timer != null)
        {
            Timer.SetActive(false);
        }
        else
        {
            Timer = GameObject.Find("Timer");
            Timer.SetActive(false);
        }

        if (TimeOverPanel != null)
        {
            TimeOverPanel.SetActive(false);
        }
        else
        {
            TimeOverPanel = GameObject.Find("TimeOverPanel");
            TimeOverPanel.SetActive(false);
        }

        if (JumpPanel != null)
        {
            JumpPanel.SetActive(false);
        }
        else
        {
            JumpPanel = GameObject.Find("JumpPanel");
            JumpPanel.SetActive(false);
        }

        if (MissionPanel != null)
        {
            MissionPanel.SetActive(false);
        }
        else
        {
            MissionPanel = GameObject.Find("MissionPanel");
            MissionPanel.SetActive(false);
        }

        if (AddTime != null)
        {
            AddTime.SetActive(false);
        }
        else
        {
            AddTime = GameObject.Find("AddTimePanel");
            AddTime.SetActive(false);
        }
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
            Cursor.lockState = CursorLockMode.None;   //���콺 Ŀ�� Ǯ�� ����
        }

    }

    IEnumerator ShowGameObj(GameObject gameObj,float seconds)  //���� ������ �Ծ����� n�ʰ� ȭ�鿡 ���
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

    //public IEnumerator Shake(float duration, float magnitude)
    //{
    //    Debug.Log("Camera Shake");
    //    if (mainCamera == null)
    //    {
    //        Debug.Log("MainCamera�� �����ϴ�.");
    //        yield break;
    //    }

    //    isShaking = true;

    //    float elapsed = 0f;  //��� �ð� �ʱ�ȭ

    //    while (elapsed < duration)
    //    {
    //        float x = Random.Range(-1f, 1f) * magnitude;
    //        float y = Random.Range(-1f, 1f) * magnitude;

    //        mainCamera.transform.localPosition = new Vector3(transform.position.x+x, transform.position.y+y,-10);

    //        elapsed += Time.deltaTime;
    //        yield return null;
    //    }

    //    mainCamera.transform.localPosition = transform.position;
    //    isShaking = false;
    //}


}