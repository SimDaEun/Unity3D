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

    public float recoilStrenght = 2f; //�ݵ��� ���� 
    public float maxRecoilAngle = 10.0f;  //�ݵ��� �ִ� ���� 
    private float currentRecoil = 0f;  //���� �ݵ� ���� �����ϴ� ���� 

    public LayerMask hitLayer;  //�浹�� ���̾��ũ 
    public float rayDistance = 100;

    private int shotGunRayCount = 5;  //���� �Ѿ��� ������ �� 
    private float shotGunSpreadAngle = 10f; //�Ѿ� ���� 

    float MoveWalkSpeed = 5.0f;  //�Ϲ� �̵��ӵ�
    float MoveRunSpeed = 10.0f; //�޸��� �̵��ӵ� 
    float currentSpeed = 1.0f;  //���� �ӵ�
    bool isRunning = false;    // �޸��� �ִ��� Ȯ���ϱ� 
    bool isJumping = false;    //�����ϰ� �ִ��� Ȯ���ϱ� 
    float gravity = -9.81f;   //�߷�
    Vector3 velocity;         //���� �ӵ� ����
    CharacterController characterController;  //ĳ������Ʈ�ѷ� 

    float mouseSensitivity = 100f;  //���콺 ����
    public Transform cameraTransfrom;  //ī�޶� Transform
    public Transform playerHead;  //�÷��̾� �Ӹ� ��ġ(1��ġ ����� �� ���)
    public float thirdPersonDistance = 3.0f; //3��Ī ��忡�� �÷��̾�� ī�޶� �þ� �Ÿ�
    public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, 0);  //3��Ī ��忡�� ī�޶� ������ - �׳� ī�޶��� ��ġ�� �������ִ� ���̶�� ��������
    public Transform playerLookObj;

    public float zoomedDistance = 1.0f; //ī�޶� Ȯ��� ���� �Ÿ� (3��Ī ����� �� ���)
    public float zoomSpeed = 5f;  //Ȯ�� ��Ұ� �Ǵ� �ӵ�
    public float defaultFov = 60f;  //�⺻ ī�޶� �þ߰�
    public float zoomFov = 30f; //Ȯ�� �� ī�޶� �þ߰�

    float currentDistance;  //���� ī�޶���� �Ÿ�
    float targetDistance;  //��ǥ ī�޶� �Ÿ�
    float targetFov;  //��ǥFov
    bool isZoomed = false; //Ȯ�� ����
    private Coroutine zoomCoroutine; //�ڷ�ƾ�� ����Ͽ� Ȯ��, ���
    Camera mainCamera; //ī�޶� ������Ʈ 

    float pitch = 0f;  //�� �Ʒ� ȸ���� 
    float yaw = 0f;   //�¿� ȸ���� 
    bool isFirstPerson = false; //1��Ī ��� ����
    bool rotaterAroundPlayer = true;  //ī�޶� �÷��̾� ������ ȸ���ϴ� ���� 
    public float jumpHeight = 2f;   //���� ����
    bool isGround = false;   //���� �浹 ���� 
    public LayerMask groundLayer;

    float horizontal;
    float vertical;

    public Transform leftFoot;  //���� ��
    public Transform rightFoot; //������ ��

    public float minMoveDistance = 0.01f; //�ּ� �Ÿ�
    bool IsLeftFootGround = false; //���ʹ��� ���� ��Ҵ��� üũ�ϴ� ���� 
    bool IsRightFootGround = false; //������ ���� ���� ��Ҵ��� üũ

    Vector3 previousPosition;

    Animator animator;

    public float fallThreshhold = -10f;
    bool isGameOver = false;

    public float HP = 100;

    bool isAiming = false;
    public bool isFiring = false;

    public Transform upperBody;  //��ü ���� �Ҵ�(Spine, UpperChest)
    public float upperBodyRotationAngle = -30f; //��ü Aim��忡���� ȸ���� �Ѵ�.
    private Quaternion originalUpperBodyRotation;  //dnjsfo tkdcp ghlwjs rkqt 

    public Transform aimTarget;

    public float slowMotionScale = 0.5f;
    private float defaultTimeScale = 1.0f; //�⺻ �ӵ�
    private bool isSlowMotion = false;  //���ο������� Ȯ���ϴ� ���� 

    //BoxCast
    public Vector3 boxSize = new Vector3(1, 1, 1);
    public float castDistance = 5.0f; //BoxCast ���� �Ÿ� 
    public LayerMask itemLayer;  //������ ���̾�
    public Transform itemGetPos; //BoxCast ��ġ - �÷��̾��� �� ��ġ���� �����ϱ� ���� ���� ����
    public float debugDuration = 2.0f; //����� ���� ����

    //public GameObject NoveCrossHair;
    //public GameObject CrossHair;

    private bool isFire = true;  //�߻� ���� 
    public float pistolFireDelay = 0.5f; //���� �߻� ������
    public float shotGunFireDelay = 1.0f;//���� �߻� ������
    public float rifleFireDelay = 1.2f;//������ �߻� ������
    public float SMGFireDelay = 0.1f;//������� �߻� ������

    bool isGetItemAction = false;

    public float FireDelay = 0.1f;

    public Light flashLight;
    private bool isFlashLightOn;


    private Rigidbody[] ragdollbodies;
    private Collider[] ragdollcollider;

    public GameObject GunUI;
    public GameObject HPUI;

    private bool lastOpenedForward = true;  //���������� ���� ���������� ���ȴ��� ���� 
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
        Cursor.lockState = CursorLockMode.Locked;   //���콺 Ŀ���� ��� ���� 
        currentDistance = thirdPersonDistance;  //�ʱ� ī�޶� �Ÿ� ����
        targetDistance = thirdPersonDistance;  //��ǥ ī�޶� �Ÿ� ����
        targetFov = defaultFov;   //�ʱ� fov ����
        mainCamera = cameraTransfrom.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;  //�⺻ fov ����
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        previousPosition = transform.position; //���� �� ���� ��ġ�� ���� ��ġ�� �ʱ�ȭ

        SoundManager.instance.PlayBGM("BGM1");

        animator.SetLayerWeight(1, 0);  //���̾� 1(fire) - ��Ȱ��ȭ

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

        //if (isGetItemAction)  //�˾Ƽ� 
        //{
        //    return;
        //}

        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);

        //���콺 ȸ���� �������� 
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX; //�¿� ȸ���� + ���콺 ȸ���� (����y)
        //Debug.Log(yaw);
        pitch -= mouseY; //�� �Ʒ� ȸ���� + ���콺 ���Ʒ� ȸ���� (����x)
        //Debug.Log(pitch);
        //ī�޶� -y�� ������ ������ ���߰�, ī�޶� +y������ ������ �÷��̾��� �Ʒ����� ���� 

        //���Ʒ� ���� ���� �����ϱ� ����
        pitch = Mathf.Clamp(pitch, -45f, 45f);

        transform.rotation = Quaternion.Euler(0, yaw, 0);   //Player�� ���� �����̼�
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);  //ī�޶� ���� �����̼� 

        Jump();

        if (Input.GetKeyDown(KeyCode.V))   //vŰ -> 3��Ī / 1��Ī
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log(isFirstPerson ? "1��Ī���" : "3��Ī���");
        }

        if (isFirstPerson)
        {
            FirstPersonMovement();
        }
        else
        {
            ThirdPersonMovement();
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt) && !isFirstPerson)   //���� altŰ && 3��Ī�� ����  -> ī�޶� ȸ����������
        {
            rotaterAroundPlayer = !rotaterAroundPlayer;  //
            Debug.Log(rotaterAroundPlayer ? "ī�޶� �÷��̾� ������ ȸ��" : "�÷��̾ ���� ȸ��");
        }

        if (Input.GetMouseButtonDown(1) && !isAiming)   //������ ���콺 ��ư�� ������ ��
        {
            isAiming = true;
            GunUI.SetActive(true);
            AimWeapon();

            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }
            if (isFirstPerson)   //1��Ī�� ��
            {
                SetTargetFov(zoomFov);
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov));
            }
            else   //3��Ī�� ��
            {
                SetTargetDistance(zoomedDistance);
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));
            }

        }

        if (Input.GetMouseButtonDown(0) && isAiming && isFire)  //���� ��ư ������ ��
        {
            
            FireWeapon();
        }
        if (Input.GetMouseButtonUp(1) && isAiming)    //������ ���콺 ��ư�� ���� ��
        {
            GunUI.SetActive(false);
            isAiming = false; //������ false
            animator.SetLayerWeight(1, 0); //���̾�1 ��Ȱ��ȭ 

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
        if (currentRecoil > 0)  //���� �ݵ����� 0���� ũ�� 
        {
            currentRecoil -= recoilStrenght * Time.deltaTime;

            currentRecoil = Mathf.Clamp(currentRecoil, 0, maxRecoilAngle);
        }

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

        //���� ��ġ�� ���� ��ġ�� ���̸� ����ϴ� �Լ�
        float distanceMoved = Vector3.Distance(transform.position, previousPosition);

        bool isMoving = distanceMoved > minMoveDistance;//�̵� �߿� ���� ����
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

            //���� ���¸� ���� �����Ӱ� ���ϱ� ���� ����
            IsLeftFootGround = leftHit;
            IsRightFootGround = rightHit;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            isSlowMotion = !isSlowMotion;
        }
        ToggleSlowMotion();

        previousPosition = transform.position; //���� �� ���� ��ġ�� ���� ��ġ�� �ʱ�ȭ

        if (Input.GetKeyDown(KeyCode.E))
        {
            GetItem();
            Debug.Log("EŰ�� �������ϴ�.");
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
        if (isAiming)//������ �� ��
        {
            if (upperBody != null) //����ó�� 
            {
                upperBody.localRotation = Quaternion.Euler(upperBodyRotationAngle, 0, 0);  //���� ȸ���ϵ���
            }
        }
        else //������ ���� ���� ��
        {
            if (upperBody != null)
            {
                upperBody.localRotation = originalUpperBodyRotation;  //���� ȸ�������� ���ư����� 
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

    void ThirdPersonMovement() //3��Ī
    {
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        //Debug.Log("move : "+move);
        characterController.Move(move * currentSpeed * Time.deltaTime);
        UpdateCameraPosition();

    }
    void FirstPersonMovement()
    {
        cameraTransfrom.position = playerHead.transform.position;  //ī�޶� ��ġ�� �÷��̾� �Ӹ��� ��ġ�� �ٲٱ�
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0); //ī�޶� ȸ���� ����

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        //Debug.Log("move : " + move);
        characterController.Move(move * currentSpeed * Time.deltaTime);

    }
    void Jump()
    {
        isGround = CheckIfGrounded();

        // "JumpUp" �ִϸ��̼��� �Ϸ�Ǿ����� Ȯ�� (normalizedTime >= 1)
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
        {   //�÷��̾��� ������ ������ ��
            Vector3 direction = new Vector3(0, 0, -currentDistance);  //ī�޶� �Ÿ� ���� 
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);    //ȸ���� ���� ����

            //ī�޶� �÷��̾��� �����ʿ��� ������ ��ġ�� �̵�
            cameraTransfrom.position = transform.position + thirdPersonOffset + rotation * direction;
            //                         �÷��̾� ������ + (���� �÷��̾�� �ణ ��) +

            //ī�޶� �÷��̾��� ��ġ�� ���󰡵��� ���� 
            cameraTransfrom.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, yaw, 0);
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);

            //ī�޶� �÷��̾��� �����ʿ��� ������ ��ġ�� �̵�
            cameraTransfrom.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;

            //ī�޶� �÷��̾��� ��ġ�� ���󰡵��� ���� 
            cameraTransfrom.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y, 0));

            UpdateAimTarget();
        }

    }
    bool CheckIfGrounded()
    {
        // ĳ���� �� �Ʒ��� �߽� ��ġ ���� (�ణ ���� �÷��� ����)
        Vector3 boxCenter = transform.position + Vector3.up * 0.1f;

        // BoxCast�� ũ�� ���� (ĳ������ �� ũ��� �°� ����)
        Vector3 boxSize = new Vector3(0.5f, 0.1f, 0.5f);

        // BoxCast �߻�
        RaycastHit hit;
        bool isGrounded = Physics.BoxCast(boxCenter, boxSize / 2, Vector3.down, out hit, Quaternion.identity, 0.2f, groundLayer);

        // ������ BoxCast �ð�ȭ
        Color castColor = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(boxCenter, Vector3.down * 0.2f, castColor);

        return isGrounded;
    }

    IEnumerator ZoomCamera(float targetDistance)
    {
        while (Mathf.Abs(currentDistance - targetDistance) > 0.01f)
        {
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
            //currentD���� targetD�� ()��ŭ �����ϸ� �̵�
            yield return null;
        }
        currentDistance = targetDistance;  //��ǥ �Ÿ��� ������ �� ���� ����
    }

    IEnumerator ZoomFieldOfView(float targetFov)  //ī�޶� - Field of View �� ���� �� 
    {
        while (Mathf.Abs(mainCamera.fieldOfView - targetFov) > 0.01f)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        mainCamera.fieldOfView = targetFov;
    }

    //�߼Ҹ� ��� 1 (����ĳ��Ʈ ���� ������ ����)
    bool CheckGround(Transform foot)
    {
        Vector3 rayStart = foot.position + Vector3.up * 0.05f;

        bool hit = Physics.Raycast(rayStart, Vector3.down, 0.1f);

        Debug.DrawRay(rayStart, Vector3.down * 0.1f, hit ? Color.green : Color.red);

        return hit;
    }


    //���� ���� �Լ�
    void GameOver()
    {
        isGameOver = true;
        Invoke("RestartGame", 2.0f);
    }

    //���� ����ŸƮ �Լ� 
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
    //        Debug.Log("���� ü�� : " + HP);
    //    }
    //}

    void UpdateAimTarget() //ī�޶󿡼� �����ɽ�Ʈ�� ���� Ÿ����..
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        aimTarget.position = ray.GetPoint(10);
    }

    void ToggleSlowMotion()
    {
        if (!isSlowMotion)  //���ο����� �ƴϸ� 
        {
            Time.timeScale = defaultTimeScale;  //Ÿ�ӽ������� �⺻����
            Debug.Log("���ο��� ����");
        }
        else
        {
            Time.timeScale = slowMotionScale;  //Ÿ�ӽ������� ���ο��� �����Ϸ� ����
            //currentSpeed *= 2;   //���ο����� ������ �÷��̾�� �������� �ʵ��� �ӵ��� ������ 
            Debug.Log("���ο� ��� Ȱ��ȭ");
        }
    }



    //Raycast�� ī�޶󿡼� ����.  ī�޶�� �÷��̾� �ڿ� ����. �÷��̾ ���� ���ɼ��� ����. ���� ������ ����
    //�� ������ ����ĳ��Ʈ�� �ö󰡴� �� (�ݵ�) -�� �������� �ݵ��� �޶����. + �Ҹ��� �ٸ� 

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

    void ApplyRecoil()  //�ݵ��� ���� �Լ� 
    {
        //���� ī�޶��� ���� ȸ���� ������
        Quaternion currentRotation = Camera.main.transform.rotation;

        //�ݵ��� ����Ͽ� X��(����) ȸ���� �߰�(���� �ö󰡴� �ݵ�)
        Quaternion recoilRotation = Quaternion.Euler(-currentRecoil, 0, 0);

        //���� ȸ�� ���� �ݵ��� ���Ͽ� ���ο� ȸ������ ����
        Camera.main.transform.rotation = currentRotation * recoilRotation;

        //�ݵ� ���� ������Ŵ
        currentRecoil += recoilStrenght;

        //�ݵ� ���� Max�� ���缭 ����
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
            Debug.LogError("���� ���Ⱑ �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        // ��ƼŬ ��� (�Ѿ� ȿ��)
        if (ParticleManager.instance != null)
        {
            ParticleManager.instance.PlayParticle(ParticleManager.ParticleType.PistolEffect, currenWeapon.effectPos.position);
        }
        else
        {
            Debug.LogError("ParticleManager �ν��Ͻ��� �����ϴ�.");
            return;
        }
        RaycastHit hit;

        Vector3 origin = Camera.main.transform.position;
        Vector3 direction = Camera.main.transform.forward;

        rayDistance = 100f;//�� �����Ÿ�

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
                        //�Լ� ȣ��
                        zombieAI.TakeDamage(5, hit.collider.tag);
                    }
                }
                else if (hit.collider.tag == "Head")
                {
                    ZombieAI zombieAI = hit.collider.GetComponentInParent<ZombieAI>();

                    if (zombieAI != null)
                    {
                        //�Լ� ȣ��
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

        rayDistance = 1000f;//�� �����Ÿ�

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

        rayDistance = 200f;//�� �����Ÿ�

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
            GameObject item = hit.collider.gameObject;   //hit�� �ݶ��̴��� ���ӿ�����Ʋ�� ������ item�̶�� ������ ���� 
            Debug.Log(hit.collider.name);

            DoorBase door = item.GetComponent<DoorBase>();  //DoorBase��� ������Ʈ�� ������ ������ 

            if (door != null)   //door�� null�� �ƴϸ� ���� 
            {
                if (door.isOpen)  //���� ���������� 
                {
                    if (lastOpenedForward)  //���� ���������� ���������� 
                    {
                        door.CloseForward(transform);  //���������� ���� 
                    }
                    else
                    {
                        door.CloseBackward(transform); //�ݴ� �������� ���� 
                    }
                }
                else  //���� �ȿ��������� 
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
                Debug.Log($"������ ���� : {item.name}");
                item.SetActive(false);
            }
            else
            {
                return;
            }
        }
        bool isPickUp = animator.GetCurrentAnimatorStateInfo(0).IsName("PickUp") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f; ;

        if ((!isPickUp))  //�˾Ƽ� �ϱ� 
        {
            isGetItemAction = false;
        }
    }

    void DebugBoxCast(Vector3 origin, Vector3 direction)
    {
        Vector3 enPoint = origin + direction * castDistance;
        //BoxCast�� ����� �׸��� ���� 8���� �𼭸� ��ǥ ���
        Vector3[] corners = new Vector3[8];
        corners[0] = origin + new Vector3(-boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[1] = origin + new Vector3(boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[2] = origin + new Vector3(-boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[3] = origin + new Vector3(boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[4] = origin + new Vector3(-boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[5] = origin + new Vector3(boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[6] = origin + new Vector3(-boxSize.x, boxSize.y, boxSize.z) / 2;
        corners[7] = origin + new Vector3(boxSize.x, boxSize.y, boxSize.z) / 2;

        //������ �ڽ��� 12�� �𼭱⸦ �׸���
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
        //BoxCast�� ������ �ð������� ǥ��
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
        isFire = false;  //false�� ���� ���� ������ ���� 

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

