using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class M_PlayerManager : MonoBehaviour
{

    float MoveSpeed = 5.0f;  //�̵��ӵ�
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

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;   //���콺 Ŀ���� ��� ����
        currentDistance = thirdPersonDistance;  //�ʱ� ī�޶� �Ÿ� ����
        targetDistance = thirdPersonDistance;  //��ǥ ī�޶� �Ÿ� ����
        targetFov = defaultFov;   //�ʱ� fov ����
        mainCamera = cameraTransfrom.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;  //�⺻ fov ����
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
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
            Debug.Log(isFirstPerson ? "ī�޶� �÷��̾� ������ ȸ��" : "�÷��̾ ���� ȸ��");
        }

        if (Input.GetMouseButtonDown(1))   //������ ���콺 ��ư�� ������ ��
        {
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

        if (Input.GetMouseButtonUp(1))    //������ ���콺 ��ư�� ���� ��
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

    void ThirdPersonMovement() //3��Ī
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
        cameraTransfrom.position = playerHead.transform.position;  //ī�޶� ��ġ�� �÷��̾� �Ӹ��� ��ġ�� �ٲٱ�
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0); //ī�޶� ȸ���� ����

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");


        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        Debug.Log("move : " + move);
        characterController.Move(move * MoveSpeed * Time.deltaTime);


    }
    void Jump()
    {
        isGround = CheckifGround(); //������ �Ǻ�

        if (Input.GetButtonDown("Jump") && isGround)  //isGround -> �������� ����
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);  //�����̽��� ������ �߷º��� �� �پ�Ѱ� ��
            SoundManager.instance.PlaySFX("Jump", transform.position);
        }
        velocity.y += gravity * Time.deltaTime;   //�߷¿� ���� �ڵ�
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
            //cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);

            //ī�޶� �÷��̾��� �����ʿ��� ������ ��ġ�� �̵�
            cameraTransfrom.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;

            //ī�޶� �÷��̾��� ��ġ�� ���󰡵��� ����
            cameraTransfrom.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y, 0));
        }

    }
    bool CheckifGround()  //�׶������� üũ�ϴ� �ڵ�
    {
        RaycastHit hit;  //����ĳ��Ʈ
        float rayDistance = 0.2f;  //����ĳ��Ʈ ���ִ� ���

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
}