using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class M_PlayerManager : MonoBehaviour
{
    float MoveWalkSpeed = 5.0f;  //일반 이동속도
    float MoveRunSpeed = 10.0f; //달리기 이동속도 
    float currentSpeed = 1.0f;  //변경 속도
    bool isRunning = false;    // 달리고 있는지 확인하기 
    float gravity = -9.81f;   //중력
    Vector3 velocity;         //현재 속도 저장
    CharacterController characterController;  //캐릭터컨트롤러

    float mouseSensitivity = 100f;  //마우스 감도
    public Transform cameraTransfrom;  //카메라 Transform
    public float thirdPersonDistance = 3.0f; //3인칭 모드에서 플레이어와 카메라 시야 거리
    public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, 0);  //3인칭 모드에서 카메라 오프셋 - 그냥 카메라의 위치를 지정해주는 것이라고 이해하자


    float pitch = 0f;  //위 아래 회전값
    float yaw = 0f;   //좌우 회전값
    bool isGround = false;   //땅에 충돌 여부
    public LayerMask groundLayer;
    Animator animator;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;   //마우스 커서를 잠근 상태
        
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        Debug.Log("move : " + move);
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
    }
}