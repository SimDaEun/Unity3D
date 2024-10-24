using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class M_PlayerManager : MonoBehaviour
{
    //-----------------Player Move---------------------//
    float MoveWalkSpeed = 3.0f;  //일반 이동속도
    float MoveRunSpeed = 6.0f; //달리기 이동속도 
    float currentSpeed = 1.0f;  //변경 속도
    bool isRunning = false;    // 달리고 있는지 확인하기 
    float gravity = -9.81f;   //중력
    Vector3 velocity;         //현재 속도 저장
    CharacterController characterController;  //캐릭터컨트롤러


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

    public GameObject confetti;

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
        confetti.SetActive(false);
    }

    void Update()
    {

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        //마우스 회전 값 가져오기
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;  //좌우 회전값 + 마우스 회전값 (기준 y)
        pitch = 20f;  //위아래 회전값 + 마우스 회전값 (기준x)

        //pitch = Mathf.Clamp(pitch, -20f, 20f);  //위아래 회전 제한 

        transform.rotation = Quaternion.Euler(0, yaw, 0); //플레이어에 대한 로테이션
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);



        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(move * currentSpeed * Time.deltaTime);

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

        CameraPosition();

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
        transform.rotation = Quaternion.Euler(0, yaw, 0);
        Vector3 direction = new Vector3(0, 0, -currentDistance);
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);

        cameraTransfrom.position = transform.position + thirdPersonOffset + Quaternion.Euler(pitch,yaw,0) * direction;

        cameraTransfrom.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
    }

    bool CheckGround(Transform foot)
    {
        Vector3 rayStart = foot.position + Vector3.down * 0.1f;

        bool hit = Physics.Raycast(rayStart, Vector3.down, 0.1f);

        Debug.DrawRay(rayStart, Vector3.down * 0.1f, hit ? Color.green : Color.red);

        return hit;
    }


    private void OnTriggerExit(Collider other)  //Trigger에서 벗어나면 UI 비활성화 되도록 하는 함수
    {
            Debug.Log("Start/End 오브젝트 비활성화");
        if (other.tag == "StartEnd")
        {
            other.gameObject.SetActive(false);
        }

        if(other.name == "End")
        {
            confetti.SetActive(true);
        }
    }

}