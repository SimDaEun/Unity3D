using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    enum WeaponMode
    {
        None,
        Pistol,
        ShotGun,
        Rifile,
        SMG
    }

    private WeaponMode currentWeaponMode = WeaponMode.None;

    public float recoilStrenght = 2f; //반동의 세기 
    public float maxRecoilAngle = 10.0f;  //반동의 최대 각도 
    private float currentRecoil = 0f;  //현재 반동 값을 저장하는 변수 

    public LayerMask hitLayer;  //충돌할 레이어마스크 
    public float rayDistance = 100;

    private int shotGunRayCount = 5;  //샷건 총알이 퍼지는 수 
    private float shotGunSpreadAngle = 10f; //총알 각도 

    float MoveWalkSpeed = 5.0f;  //일반 이동속도
    float MoveRunSpeed = 10.0f; //달리기 이동속도 
    float currentSpeed = 1.0f;  //변경 속도
    bool isRunning = false;    // 달리고 있는지 확인하기 
    bool isJumping = false;    //점프하고 있는지 확인하기 
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

    float horizontal;
    float vertical;

    public Transform leftFoot;  //왼쪽 발
    public Transform rightFoot; //오른쪽 발

    public float minMoveDistance = 0.01f; //최소 거리
    bool IsLeftFootGround = false; //왼쪽발이 땅에 닿았는지 체크하는 상태 
    bool IsRightFootGround = false; //오른쪽 발이 땅에 닿았는지 체크

    Vector3 previousPosition;

    Animator animator;

    public float fallThreshhold = -10f;
    bool isGameOver = false;

    public float HP = 100;

    bool isAiming = false;
    public bool isFiring = false;

    public Transform upperBody;  //상체 본을 할당(Spine, UpperChest)
    public float upperBodyRotationAngle = -30f; //상체 Aim모드에서만 회전을 한다.
    private Quaternion originalUpperBodyRotation;  //dnjsfo tkdcp ghlwjs rkqt 

    public Transform aimTarget;

    public float slowMotionScale = 0.5f;
    private float defaultTimeScale = 1.0f; //기본 속도
    private bool isSlowMotion = false;  //슬로우모션인지 확인하는 변수 

    //BoxCast
    public Vector3 boxSize = new Vector3(1, 1, 1);
    public float castDistance = 5.0f; //BoxCast 감지 거리 
    public LayerMask itemLayer;  //아이템 레이어
    public Transform itemGetPos; //BoxCast 위치 - 플레이어의 손 위치에서 감지하기 위해 따로 만듦
    public float debugDuration = 2.0f; //디버그 라인 여부

    //public GameObject NoveCrossHair;
    //public GameObject CrossHair;

    private bool isFire = true;  //발사 여부 
    public float pistolFireDelay = 0.5f; //권총 발사 딜레이
    public float shotGunFireDelay = 1.0f;//샷건 발사 딜레이
    public float rifleFireDelay = 1.2f;//라이플 발사 딜레이
    public float SMGFireDelay = 0.1f;//기관단총 발사 딜레이

    bool isGetItemAction = false;

    public float FireDelay = 0.1f;

    public Light flashLight;
    private bool isFlashLightOn;


    private Rigidbody[] ragdollbodies;
    private Collider[] ragdollcollider;

    public GameObject GunUI;
    public GameObject HPUI;

    private bool lastOpenedForward = true;  //마지막으로 문이 정방향으로 열렸는지 여부 
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
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

        previousPosition = transform.position; //시작 시 현재 위치를 이전 위치로 초기화

        SoundManager.instance.PlayBGM("BGM1");

        animator.SetLayerWeight(1, 0);  //레이어 1(fire) - 비활성화

        if (upperBody != null)
        {
            originalUpperBodyRotation = upperBody.localRotation;
        }

        flashLight.enabled = false;

        ragdollbodies = GetComponentsInChildren<Rigidbody>();
        ragdollcollider = GetComponentsInChildren<Collider>();
        GunUI.SetActive(false);
        HPUI.SetActive(true);
    }

    void Update()
    {
        if (transform.position.y < fallThreshhold && !isGameOver)
        {
            SoundManager.instance.PlaySFX("Scream", transform.position);
            GameOver();
        }

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        //if (isGetItemAction)  //알아서 
        //{
        //    return;
        //}

        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);

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
            Debug.Log(rotaterAroundPlayer ? "카메라가 플레이어 주위를 회전" : "플레이어가 직접 회전");
        }

        if (Input.GetMouseButtonDown(1) && !isAiming)   //오른쪽 마우스 버튼을 눌렀을 때
        {
            isAiming = true;
            GunUI.SetActive(true);
            AimWeapon();

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

        if (Input.GetMouseButtonDown(0) && isAiming && isFire)  //왼쪽 버튼 눌렀을 때
        {
            
            FireWeapon();
        }
        if (Input.GetMouseButtonUp(1) && isAiming)    //오른쪽 마우스 버튼을 땠을 때
        {
            GunUI.SetActive(false);
            isAiming = false; //조준중 false
            animator.SetLayerWeight(1, 0); //레이어1 비활성화 

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

        ChangeWeapon();
        if (currentRecoil > 0)  //현재 반동값이 0보다 크면 
        {
            currentRecoil -= recoilStrenght * Time.deltaTime;

            currentRecoil = Mathf.Clamp(currentRecoil, 0, maxRecoilAngle);
        }

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

        //현재 위치와 이전 위치의 차이를 계산하는 함수
        float distanceMoved = Vector3.Distance(transform.position, previousPosition);

        bool isMoving = distanceMoved > minMoveDistance;//이동 중에 대한 여부
        if (isMoving)
        {
            bool leftHit = CheckGround(leftFoot);
            bool rightHit = CheckGround(rightFoot);

            if ((leftHit && !IsLeftFootGround))
            {
                if (SoundManager.instance.sfxSource.isPlaying) return;
                SoundManager.instance.PlaySFX("Step", transform.position);
            }

            if ((rightHit && !IsRightFootGround))
            {
                if (SoundManager.instance.sfxSource.isPlaying) return;
                SoundManager.instance.PlaySFX("Step", transform.position);
            }

            //현재 상태를 다음 프레임과 비교하기 위해 저장
            IsLeftFootGround = leftHit;
            IsRightFootGround = rightHit;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            isSlowMotion = !isSlowMotion;
        }
        ToggleSlowMotion();

        previousPosition = transform.position; //시작 시 현재 위치를 이전 위치로 초기화

        if (Input.GetKeyDown(KeyCode.E))
        {
            GetItem();
            Debug.Log("E키를 눌렀습니다.");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashLight();
        }

        if (HP <= 0)
        {
            ActivateRagdoll();
        }
    }

    private void ToggleFlashLight()
    {
        isFlashLightOn = !isFlashLightOn;
        flashLight.enabled = isFlashLightOn;
    }

    private void LateUpdate()
    {
        if (isAiming)//조준을 할 때
        {
            if (upperBody != null) //예외처리 
            {
                upperBody.localRotation = Quaternion.Euler(upperBodyRotationAngle, 0, 0);  //몸이 회전하도록
            }
        }
        else //조준을 하지 않을 때
        {
            if (upperBody != null)
            {
                upperBody.localRotation = originalUpperBodyRotation;  //원래 회전값으로 돌아가도록 
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
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        //Debug.Log("move : "+move);
        characterController.Move(move * currentSpeed * Time.deltaTime);
        UpdateCameraPosition();

    }
    void FirstPersonMovement()
    {
        cameraTransfrom.position = playerHead.transform.position;  //카메라 위치를 플레이어 머리의 위치로 바꾸기
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0); //카메라에 회전을 설정

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        //Debug.Log("move : " + move);
        characterController.Move(move * currentSpeed * Time.deltaTime);

    }
    void Jump()
    {
        isGround = CheckIfGrounded();

        // "JumpUp" 애니메이션이 완료되었는지 확인 (normalizedTime >= 1)
        bool jumpUpCompleted = animator.GetCurrentAnimatorStateInfo(0).IsName("JumpingUp") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f;

        if (isGround && isJumping && jumpUpCompleted)
        {

            animator.SetTrigger("JumpDown");
            isJumping = false;

        }

        if (Input.GetButtonDown("Jump") && isGround)
        {
            SoundManager.instance.PlaySFX("Jump", transform.position);
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("JumpUp");
            isJumping = true;
        }

        velocity.y += gravity * Time.deltaTime;

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
            cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);

            //카메라를 플레이어의 오른쪽에서 고정된 위치로 이동
            cameraTransfrom.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;

            //카메라가 플레이어의 위치를 따라가도록 설정 
            cameraTransfrom.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y, 0));

            UpdateAimTarget();
        }

    }
    bool CheckIfGrounded()
    {
        // 캐릭터 발 아래의 중심 위치 설정 (약간 위로 올려서 시작)
        Vector3 boxCenter = transform.position + Vector3.up * 0.1f;

        // BoxCast의 크기 설정 (캐릭터의 발 크기와 맞게 조정)
        Vector3 boxSize = new Vector3(0.5f, 0.1f, 0.5f);

        // BoxCast 발사
        RaycastHit hit;
        bool isGrounded = Physics.BoxCast(boxCenter, boxSize / 2, Vector3.down, out hit, Quaternion.identity, 0.2f, groundLayer);

        // 디버깅용 BoxCast 시각화
        Color castColor = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(boxCenter, Vector3.down * 0.2f, castColor);

        return isGrounded;
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

    //발소리 방법 1 (레이캐스트 쏴서 땅인지 인지)
    bool CheckGround(Transform foot)
    {
        Vector3 rayStart = foot.position + Vector3.up * 0.05f;

        bool hit = Physics.Raycast(rayStart, Vector3.down, 0.1f);

        Debug.DrawRay(rayStart, Vector3.down * 0.1f, hit ? Color.green : Color.red);

        return hit;
    }


    //게임 오버 함수
    void GameOver()
    {
        isGameOver = true;
        Invoke("RestartGame", 2.0f);
    }

    //게임 리스타트 함수 
    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TakeDamage(float damage)
    {
        HP -= damage;
        HP = Mathf.Max(HP, 0);
    }


    //private void OnControllerColliderHit(ControllerColliderHit hit)
    //{
    //    if (hit.collider.tag == "Obstacle")
    //    {
    //        TakeDamage(3f);
    //        Debug.Log("현재 체력 : " + HP);
    //    }
    //}

    void UpdateAimTarget() //카메라에서 레이케스트를 쏴서 타겟을..
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        aimTarget.position = ray.GetPoint(10);
    }

    void ToggleSlowMotion()
    {
        if (!isSlowMotion)  //슬로우모션이 아니면 
        {
            Time.timeScale = defaultTimeScale;  //타임스케일을 기본으로
            Debug.Log("슬로우모션 해제");
        }
        else
        {
            Time.timeScale = slowMotionScale;  //타임스케일을 슬로우모션 스케일로 변경
            //currentSpeed *= 2;   //슬로우모션일 때에도 플레이어는 느려지지 않도록 속도를 곱해줌 
            Debug.Log("슬로우 모션 활성화");
        }
    }



    //Raycast는 카메라에서 나감.  카메라는 플레이어 뒤에 있음. 플레이어가 맞을 가능성이 있음. 누가 맞을지 생각
    //쏠 때마다 레이캐스트가 올라가는 것 (반동) -총 종류마다 반동이 달라야함. + 소리도 다름 

    void AimWeapon()
    {
        
        animator.SetLayerWeight(1, 1);
        if ((WeaponManager.instance.GetCurrentWeaponType() == Weapon.WeaponType.Pistol))
        {
            AimPistol();
        }
        else if ((WeaponManager.instance.GetCurrentWeaponType() == Weapon.WeaponType.ShotGun))
        {
            AimShotGun();
        }
        else if ((WeaponManager.instance.GetCurrentWeaponType() == Weapon.WeaponType.Rifle))
        {
            AimRifle();
        }
        else if ((WeaponManager.instance.GetCurrentWeaponType() == Weapon.WeaponType.SMG))
        {
            AimSMG();
        }
        ApplyRecoil();
    }

    void FireWeapon()
    {
        StartCoroutine(FireWithDelay());
        
    }

    void ApplyRecoil()  //반동에 대한 함수 
    {
        //현재 카메라의 월드 회전을 가져옴
        Quaternion currentRotation = Camera.main.transform.rotation;

        //반동을 계산하여 X축(상하) 회전에 추가(위로 올라가는 반동)
        Quaternion recoilRotation = Quaternion.Euler(-currentRecoil, 0, 0);

        //현재 회전 값에 반동을 곱하여 새로운 회전값을 적용
        Camera.main.transform.rotation = currentRotation * recoilRotation;

        //반동 값을 증가시킴
        currentRecoil += recoilStrenght;

        //반동 값을 Max에 맞춰서 제한
        currentRecoil = Mathf.Clamp(currentRecoil,0,maxRecoilAngle);
    }

    void AimPistol()
    {
        animator.Play("PistolAim");
    }

    void AimShotGun() 
    {
        animator.Play("ShotGunAim");
    }

    void AimRifle()
    {
        animator.Play("RifleAim");
    }

    void AimSMG()
    {
        animator.Play("SMGAim");
    }


    void FirePistol()
    {
        animator.SetTrigger("FirePistol");
        SoundManager.instance.PlaySFX("FirePistol", transform.position);

        Weapon currenWeapon = WeaponManager.instance.GetCurrentWeaponComponent();

        if (currenWeapon == null)
        {
            Debug.LogError("현재 무기가 할당되지 않았습니다.");
            return;
        }

        // 파티클 재생 (총알 효과)
        if (ParticleManager.instance != null)
        {
            ParticleManager.instance.PlayParticle(ParticleManager.ParticleType.PistolEffect, currenWeapon.effectPos.position);
        }
        else
        {
            Debug.LogError("ParticleManager 인스턴스가 없습니다.");
            return;
        }
        RaycastHit hit;

        Vector3 origin = Camera.main.transform.position;
        Vector3 direction = Camera.main.transform.forward;

        rayDistance = 100f;//총 사정거리

        Debug.DrawRay(origin, direction * rayDistance, Color.red, 1.0f);

        if (Physics.Raycast(origin, direction * rayDistance, out hit))
        {
            if (hit.collider != null)
            {
                if (hit.collider.tag == "Ground")
                {
                    ParticleManager.instance.PlayParticle(ParticleManager.ParticleType.BrickImpact, hit.point);
                    return;
                }

                if (hit.collider.tag == "Zombie")
                {
                    ZombieAI zombieAI = hit.collider.GetComponent<ZombieAI>();

                    if (zombieAI != null)
                    {
                        //함수 호출
                        zombieAI.TakeDamage(5, hit.collider.tag);
                    }
                }
                else if (hit.collider.tag == "Head")
                {
                    ZombieAI zombieAI = hit.collider.GetComponentInParent<ZombieAI>();

                    if (zombieAI != null)
                    {
                        //함수 호출
                        zombieAI.TakeDamage(5, hit.collider.tag);
                    }
                }
            }
        }

    }

    void FireShotGun()
    {
        animator.SetTrigger("FireShotGun");
        //SoundManager.instance.PlaySFX("FireShotGun");

        rayDistance = 250f;

        for (int i = 0; i < shotGunRayCount; i++)
        {
            RaycastHit hit;

            Vector3 origin = Camera.main.transform.position;

            float spreadX = Random.Range(-shotGunSpreadAngle, shotGunSpreadAngle);
            float spreadY = Random.Range(-shotGunSpreadAngle, shotGunSpreadAngle);

            Vector3 spreadDirection = Quaternion.Euler(spreadX, spreadY, 0) * Camera.main.transform.forward;

            Debug.DrawRay(origin, spreadDirection * rayDistance, Color.red, 1.0f);

            if (Physics.Raycast(origin, spreadDirection, out hit, rayDistance, hitLayer))
            {
                Debug.Log("Hit : " + hit.collider.name);
            }

        }
    }

    void FireRifle()
    {
        animator.SetTrigger("FireRifle");
        //SoundManager.instance.PlaySFX("FireRifle");

        RaycastHit hit;

        Vector3 origin = Camera.main.transform.position;
        Vector3 direction = Camera.main.transform.forward;

        rayDistance = 1000f;//총 사정거리

        Debug.DrawRay(origin, direction * rayDistance, Color.red, 1.0f);

        if (Physics.Raycast(origin, direction * rayDistance, out hit, hitLayer))
        {
            Debug.Log("Hit : " + hit.collider.name);
        }
    }

    void FireSMG()
    {
        animator.SetTrigger("FireSMG");
        // SoundManager.instance.PlaySFX("FireSMG");

        RaycastHit hit;

        Vector3 origin = Camera.main.transform.position;
        Vector3 direction = Camera.main.transform.forward;

        rayDistance = 200f;//총 사정거리

        Debug.DrawRay(origin, direction * rayDistance, Color.red, 1.0f);

        if (Physics.Raycast(origin, direction * rayDistance, out hit, hitLayer))
        {
            Debug.Log("Hit : " + hit.collider.name);
        }
    }


    void ChangeWeapon()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            WeaponManager.instance.EquipWeapon(Weapon.WeaponType.Pistol);
            Debug.Log("Pistol Change");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            WeaponManager.instance.EquipWeapon(Weapon.WeaponType.ShotGun);
            Debug.Log("ShotGun Change");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            WeaponManager.instance.EquipWeapon(Weapon.WeaponType.Rifle);
            Debug.Log("Rifile Change");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            WeaponManager.instance.EquipWeapon(Weapon.WeaponType.SMG);
            Debug.Log("SMG Change");
        }
    }

    void GetItem()
    {
        isGetItemAction = true;
        Debug.Log("GetItem");
        animator.SetTrigger("PickUp");
        Vector3 origin = itemGetPos.position;
        Vector3 direction = itemGetPos.forward;

        RaycastHit[] hits;
        hits = Physics.BoxCastAll(origin, boxSize / 2, direction, Quaternion.identity, castDistance, itemLayer);

        DebugBoxCast(origin, direction);

        foreach (RaycastHit hit in hits)
        {
            GameObject item = hit.collider.gameObject;   //hit의 콜라이더의 게임오브젝틀르 가져와 item이라는 변수에 넣음 
            Debug.Log(hit.collider.name);

            DoorBase door = item.GetComponent<DoorBase>();  //DoorBase라는 컴포넌트가 있으면 가져옴 

            if (door != null)   //door이 null이 아니면 실행 
            {
                if (door.isOpen)  //문이 열려있으면 
                {
                    if (lastOpenedForward)  //문이 정방향으로 열려있으면 
                    {
                        door.CloseForward(transform);  //정방향으로 닫음 
                    }
                    else
                    {
                        door.CloseBackward(transform); //반대 방향으로 닫음 
                    }
                }
                else  //문이 안열려있으면 
                {
                    if (door.Open(transform))
                    {
                        lastOpenedForward = door.LastOpenForward;
                    }
                }

                return;
            }

            if (item.CompareTag("Weapon"))
            {
                WeaponManager.instance.AddWeapon(item);
                item.SetActive(false);
            }
            else if (item.CompareTag("Item"))
            {
                Debug.Log($"아이템 감지 : {item.name}");
                item.SetActive(false);
            }
            else
            {
                return;
            }
        }
        bool isPickUp = animator.GetCurrentAnimatorStateInfo(0).IsName("PickUp") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f; ;

        if ((!isPickUp))  //알아서 하기 
        {
            isGetItemAction = false;
        }
    }

    void DebugBoxCast(Vector3 origin, Vector3 direction)
    {
        Vector3 enPoint = origin + direction * castDistance;
        //BoxCast의 모양을 그리기 위한 8개의 모서리 좌표 계산
        Vector3[] corners = new Vector3[8];
        corners[0] = origin + new Vector3(-boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[1] = origin + new Vector3(boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[2] = origin + new Vector3(-boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[3] = origin + new Vector3(boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[4] = origin + new Vector3(-boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[5] = origin + new Vector3(boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[6] = origin + new Vector3(-boxSize.x, boxSize.y, boxSize.z) / 2;
        corners[7] = origin + new Vector3(boxSize.x, boxSize.y, boxSize.z) / 2;

        //시작점 박스의 12개 모서기를 그리기
        Debug.DrawLine(corners[0], corners[1], Color.green, debugDuration);
        Debug.DrawLine(corners[1], corners[3], Color.green, debugDuration);
        Debug.DrawLine(corners[3], corners[2], Color.green, debugDuration);
        Debug.DrawLine(corners[2], corners[0], Color.green, debugDuration);
        Debug.DrawLine(corners[4], corners[5], Color.green, debugDuration);
        Debug.DrawLine(corners[5], corners[7], Color.green, debugDuration);
        Debug.DrawLine(corners[7], corners[6], Color.green, debugDuration);
        Debug.DrawLine(corners[6], corners[4], Color.green, debugDuration);
        Debug.DrawLine(corners[0], corners[4], Color.green, debugDuration);
        Debug.DrawLine(corners[1], corners[5], Color.green, debugDuration);
        Debug.DrawLine(corners[2], corners[6], Color.green, debugDuration);
        Debug.DrawLine(corners[3], corners[7], Color.green, debugDuration);
        //BoxCast의 끝점을 시각적으로 표시
        Debug.DrawRay(origin, direction * castDistance, Color.green, debugDuration);
    }


    public IEnumerator effectActive()
    {
        GameObject effectPos = GameObject.Find("EffectPos");
        if (effectPos != null)
        {
            effectPos.SetActive(true);
        }
        yield return new WaitForSeconds(1.0f);
        effectPos.SetActive(false);
    }

    IEnumerator FireWithDelay()
    {
        isFire = false;  //false일 때는 총이 쏴지지 않음 

        if (WeaponManager.instance.GetCurrentWeaponType() == Weapon.WeaponType.Pistol)
        {
            FireDelay = 0.5f;
            FirePistol();
        }
        else if (WeaponManager.instance.GetCurrentWeaponType() == Weapon.WeaponType.ShotGun)
        {
            FireDelay = 1.0f;
            FireShotGun();
        }
        else if (WeaponManager.instance.GetCurrentWeaponType() == Weapon.WeaponType.Rifle)
        {
            FireDelay = 1.2f; 
            FireRifle();
        }
        else if (WeaponManager.instance.GetCurrentWeaponType() == Weapon.WeaponType.SMG)
        {
            FireDelay = 0.1f;
            FireSMG();
        }
        ApplyRecoil();

        yield return new WaitForSeconds(FireDelay);

        isFire = true;
    }

    private void SetRagdollState(bool state)
    {
        foreach (Rigidbody body in ragdollbodies)
        {
            body.isKinematic = !state;
        }
        foreach (Collider collider in ragdollcollider)
        {
            collider.enabled = state;
        }
    }

    public void ActivateRagdoll()
    {
        animator.enabled = false;
        SetRagdollState(true);
    }
}

