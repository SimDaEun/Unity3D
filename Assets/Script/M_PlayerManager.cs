using System.Collections;

using UnityEngine;


public class M_PlayerManager : MonoBehaviour
{
    public static M_PlayerManager instance;
    //-----------------Player Move---------------------//
    float MoveWalkSpeed = 4.6f;  //일반 이동속도
    float MoveRunSpeed = 8.0f; //달리기 이동속도 
    float JumpHeight = 10.0f; //점프 높이
    float currentSpeed = 1.0f;  //변경 속도
    bool isRunning = false;    // 달리고 있는지 확인하기 
    float gravity = -20f;   //중력
    Vector3 velocity;         //현재 속도 저장
    CharacterController characterController;  //캐릭터컨트롤러
    public GameObject PlayerHead;

    float mouseX;
    float mouseY;

    //-----------------------Camera Settings------------------------//
    Camera mainCamera;
    float mouseSensitivity = 100f;  //마우스 감도
    public Transform cameraTransfrom;  //카메라 Transform
    public float thirdPersonDistance = 3.0f; //3인칭 모드에서 플레이어와 카메라 시야 거리
    public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, 0);  //3인칭 모드에서 카메라 오프셋 - 그냥 카메라의 위치를 지정해주는 것이라고 이해하자
    float currentDistance;  //현재 카메라와의 거리
    float targetDistance;  //목표 카메라 거리
    float targetFov;  //목표Fov
    public float defaultFov = 60f;  //기본 카메라 시야각
    float pitch = 0f;  //위 아래 회전값
    float yaw = 0f;   //좌우 회전값
    bool isGround = false;   //땅에 충돌 여부
    public LayerMask groundLayer;


    //----------------FootStep-------------------------//
    public Transform leftFoot; //왼쪽 발
    public Transform rightFoot; //오른쪽 발 
    public float minMoveDistance = 0.01f; //최소 거리
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
    public bool isFloating = false;  //공중에 떠 있는지 확인하는 변수
    public bool isShaking = false;

    [Header("CameraShake Setting")]
    public float shakeDuration = 0.3f;  //흔들림 지속 시간
    public float shakeMagnitude = 0.2f; //흔들림 강도
    

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
        Cursor.lockState = CursorLockMode.Locked;   //마우스 커서를 잠근 상태
        currentDistance = thirdPersonDistance;  //초기 카메라 거리 설정
        targetDistance = thirdPersonDistance;  //목표 카메라 거리 설정
        targetFov = defaultFov;   //초기 fov 설정
        mainCamera = cameraTransfrom.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;  //기본 fov 설정
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

        //마우스 회전 값 가져오기
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
        characterController.Move(velocity * Time.deltaTime); //추가 (중력)



        if (Input.GetKey(KeyCode.LeftShift))  //Shift키 누르면 달리는 애니메이션
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

        currentSpeed = isRunning ? MoveRunSpeed : MoveWalkSpeed;  //달리는 중이면 currentSpeed에 달리는 속도, 아니면 걷는 속도 대입
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

        //현재 위치와 이전 위치의 차이를 계산하는 함수
        float distanceMoved = Vector3.Distance(transform.position, previousPosition);

        bool isMoving = distanceMoved > minMoveDistance;//이동 중에 대한 여부
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

            //현재 상태를 다음 프레임과 비교하기 위해 저장
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
        yaw += mouseX;  //좌우 회전값 + 마우스 회전값 (기준 y)
        pitch = 20f;  //위아래 회전값 + 마우스 회전값 (기준x)
        //pitch = Mathf.Clamp(pitch, -20f, 20f);  //위아래 회전 제한 

        transform.rotation = Quaternion.Euler(0, yaw, 0); //플레이어에 대한 로테이션
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);


        transform.rotation = Quaternion.Euler(0, yaw, 0);
        Vector3 direction = new Vector3(0, 0, -currentDistance);
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);

        cameraTransfrom.position = transform.position + thirdPersonOffset + Quaternion.Euler(pitch,yaw,0) * direction;

        cameraTransfrom.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
    }

    void FloatingCameraPosition()
    {
        yaw += mouseX;  //좌우 회전값 + 마우스 회전값 (기준 y)
        pitch = 30f;  //위아래 회전값 + 마우스 회전값 (기준x)
        //pitch = Mathf.Clamp(pitch, -45f, 45f);  //위아래 회전 제한 

        transform.rotation = Quaternion.Euler(0, yaw, 0); //플레이어에 대한 로테이션
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);


        transform.rotation = Quaternion.Euler(0, yaw, 0);
        Vector3 direction = new Vector3(0, 0, -currentDistance);
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);

        //cameraTransfrom.position = transform.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;
        cameraTransfrom.position = PlayerHead.transform.position; //플레이어 머리 위치로 카메라 이동
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
            //Sound 재생
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
            Cursor.lockState = CursorLockMode.None;   //마우스 커서 풀린 상태
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


    private void OnTriggerExit(Collider other)  //Trigger에서 벗어나면 UI 비활성화 되도록 하는 함수
    {
        if (other.tag == "StartEnd")
        {
            other.gameObject.SetActive(false);
        }
        if (other.name == "Start")
        {
            Debug.Log("Start라인 지남");
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
            Cursor.lockState = CursorLockMode.None;   //마우스 커서 풀린 상태
            SoundManager.instance.StopBGM();
        }

    }

    IEnumerator ShowGameObj(GameObject gameObj,float seconds)  //무슨 아이템 먹었는지 n초간 화면에 띄움
    {
        if (gameObj == null)
        {
            Debug.Log("ShowGameObj: gameObj is null or has been destroyed.");
            yield break;
        }

        gameObj.SetActive(true);
        yield return new WaitForSeconds(seconds);

        if (gameObj != null)  // 다시 한번 null 확인
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
        Debug.Log("공중에 뜨는 코루틴 실행");
        isFloating = true;
        float jumpMultiplier = 1.5f;

        Vector3 jump = new Vector3(0, JumpHeight, 0);
        velocity.y = Mathf.Sqrt(JumpHeight * -2f * gravity * Time.deltaTime) * jumpMultiplier;
        velocity.x = 0;
        velocity.z = 0;

        //증력을 일시적으로 무시해 공중에 머무르도록 함
        float originalGravity = gravity;
        gravity = 0;

        yield return new WaitForSeconds(duration);  

        gravity = originalGravity;
        isFloating = false;
    }


}