using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class M_PlayerManager : MonoBehaviour
{

    float MoveSpeed = 5.0f;  //이동속도
    float gravity = -9.81f;   //중력
    Vector3 velocity;         //현재 속도 저장
    CharacterController characterController;  //캐릭터컨트롤러

    float mouseSensitivity = 100f;  //마우스 감도
    public Transform cameraTransfrom;  //카메라 Transform
    public Transform playerHead;  //플레이어 머리 위치(1인치 모드일 때 사용)
    public float thirdPersonDistance = 3.0f; //3인칭 모드에서 플레이어와 카메라 시야 거리
    public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, 0);  //3인칭 모드에서 카메라 오프셋 - 그냥 카메라의 위치를 지정해주는 것이라고 이해하자
    public Transform playerLookObj;

    public float zoomedDistance = 1.0f; //카메라가 확대될 때의 거리 (3인칭 모드일 때 사용)
    public float zoomSpeed = 5f;  //확대 축소가 되는 속도
    public float defaultFov = 60f;  //기본 카메라 시야각
    public float zoomFov = 30f; //확대 시 카메라 시야각

    float currentDistance;  //현재 카메라와의 거리
    float targetDistance;  //목표 카메라 거리
    float targetFov;  //목표Fov
    bool isZoomed = false; //확대 여부
    private Coroutine zoomCoroutine; //코루틴을 사용하여 확대, 축소
    Camera mainCamera; //카메라 컴포넌트

    float pitch = 0f;  //위 아래 회전값
    float yaw = 0f;   //좌우 회전값
    bool isFirstPerson = false; //1인칭 모드 여부
    bool rotaterAroundPlayer = true;  //카메라가 플레이어 주위를 회전하는 여부
    public float jumpHeight = 2f;   //점프 높이
    bool isGround = false;   //땅에 충돌 여부
    public LayerMask groundLayer;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;   //마우스 커서를 잠근 상태
        currentDistance = thirdPersonDistance;  //초기 카메라 거리 설정
        targetDistance = thirdPersonDistance;  //목표 카메라 거리 설정
        targetFov = defaultFov;   //초기 fov 설정
        mainCamera = cameraTransfrom.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;  //기본 fov 설정
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        //마우스 회전값 가져오기
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX; //좌우 회전값 + 마우스 회전값 (기준y)
                       //Debug.Log(yaw);
        pitch -= mouseY; //위 아래 회전값 + 마우스 위아래 회전값 (기준x)
                         //Debug.Log(pitch);
                         //카메라가 -y로 갈수록 윗쪽을 비추고, 카메라가 +y쪽으로 갈수록 플레이어의 아랫쪽을 비춤

        //위아래 보는 것을 제한하기 위함
        pitch = Mathf.Clamp(pitch, -45f, 45f);

        transform.rotation = Quaternion.Euler(0, yaw, 0);   //Player에 대한 로테이션
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);  //카메라에 대한 로테이션

        Jump();

        if (Input.GetKeyDown(KeyCode.V))   //v키 -> 3인칭 / 1인칭
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log(isFirstPerson ? "1인칭모드" : "3인칭모드");
        }

        if (isFirstPerson)
        {
            FirstPersonMovement();
        }
        else
        {
            ThirdPersonMovement();
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt) && !isFirstPerson)   //왼쪽 alt키 && 3인칭일 때만  -> 카메라 회전할지말지
        {
            rotaterAroundPlayer = !rotaterAroundPlayer;  //
            Debug.Log(isFirstPerson ? "카메라가 플레이어 주위를 회전" : "플레이어가 직접 회전");
        }

        if (Input.GetMouseButtonDown(1))   //오른쪽 마우스 버튼을 눌렀을 때
        {
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }
            if (isFirstPerson)   //1인칭일 때
            {
                SetTargetFov(zoomFov);
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov));
            }
            else   //3인칭일 때
            {
                SetTargetDistance(zoomedDistance);
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));
            }
        }

        if (Input.GetMouseButtonUp(1))    //오른쪽 마우스 버튼을 땠을 때
        {
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }

            if (isFirstPerson)
            {
                SetTargetFov(defaultFov);
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov));
            }
            else
            {
                SetTargetDistance(thirdPersonDistance);
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));
            }
        }

    }
    public void SetTargetDistance(float distance)
    {
        targetDistance = distance;
    }

    public void SetTargetFov(float fov)
    {
        targetFov = fov;
    }

    void ThirdPersonMovement() //3인칭
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");


        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        Debug.Log("move : " + move);
        characterController.Move(move * MoveSpeed * Time.deltaTime);

        UpdateCameraPosition();
    }
    void FirstPersonMovement()
    {
        cameraTransfrom.position = playerHead.transform.position;  //카메라 위치를 플레이어 머리의 위치로 바꾸기
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0); //카메라에 회전을 설정

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");


        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        Debug.Log("move : " + move);
        characterController.Move(move * MoveSpeed * Time.deltaTime);


    }
    void Jump()
    {
        isGround = CheckifGround(); //땅인지 판별

        if (Input.GetButtonDown("Jump") && isGround)  //isGround -> 무한점프 방지
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);  //스페이스를 누르면 중력보다 더 뛰어넘게 함
            SoundManager.instance.PlaySFX("Jump", transform.position);
        }
        velocity.y += gravity * Time.deltaTime;   //중력에 대한 코드
        characterController.Move(velocity * Time.deltaTime);


    }
    void UpdateCameraPosition()
    {
        if (rotaterAroundPlayer)
        {   //플레이어의 주위를 돌도록 함
            Vector3 direction = new Vector3(0, 0, -currentDistance);  //카메라 거리 설정
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);    //회전에 대한 설정

            //카메라를 플레이어의 오른쪽에서 고정된 위치로 이동
            cameraTransfrom.position = transform.position + thirdPersonOffset + rotation * direction;
            //                         플레이어 포지션 + (현재 플레이어보다 약간 위) +

            //카메라가 플레이어의 위치를 따라가도록 설정
            cameraTransfrom.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, yaw, 0);
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            //cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);

            //카메라를 플레이어의 오른쪽에서 고정된 위치로 이동
            cameraTransfrom.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;

            //카메라가 플레이어의 위치를 따라가도록 설정
            cameraTransfrom.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y, 0));
        }

    }
    bool CheckifGround()  //그라운드인지 체크하는 코드
    {
        RaycastHit hit;  //레이캐스트
        float rayDistance = 0.2f;  //레이캐스트 쏴주는 경로

        if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance, groundLayer))
        {
            return true;
        }
        return false;

    }

    IEnumerator ZoomCamera(float targetDistance)
    {
        while (Mathf.Abs(currentDistance - targetDistance) > 0.01f)
        {
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
            //currentD에서 targetD로 ()만큼 보간하며 이동
            yield return null;
        }

        currentDistance = targetDistance;  //목표 거리에 도달한 후 값을 고정
    }

    IEnumerator ZoomFieldOfView(float targetFov)  //카메라 - Field of View 에 대한 줌
    {
        while (Mathf.Abs(mainCamera.fieldOfView - targetFov) > 0.01f)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        mainCamera.fieldOfView = targetFov;
    }
}