using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class M_PlayerManager : MonoBehaviour
{
    float MoveWalkSpeed = 5.0f;  //�Ϲ� �̵��ӵ�
    float MoveRunSpeed = 10.0f; //�޸��� �̵��ӵ� 
    float currentSpeed = 1.0f;  //���� �ӵ�
    bool isRunning = false;    // �޸��� �ִ��� Ȯ���ϱ� 
    float gravity = -9.81f;   //�߷�
    Vector3 velocity;         //���� �ӵ� ����
    CharacterController characterController;  //ĳ������Ʈ�ѷ�

    float mouseSensitivity = 100f;  //���콺 ����
    public Transform cameraTransfrom;  //ī�޶� Transform
    public float thirdPersonDistance = 3.0f; //3��Ī ��忡�� �÷��̾�� ī�޶� �þ� �Ÿ�
    public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, 0);  //3��Ī ��忡�� ī�޶� ������ - �׳� ī�޶��� ��ġ�� �������ִ� ���̶�� ��������


    float pitch = 0f;  //�� �Ʒ� ȸ����
    float yaw = 0f;   //�¿� ȸ����
    bool isGround = false;   //���� �浹 ����
    public LayerMask groundLayer;
    Animator animator;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;   //���콺 Ŀ���� ��� ����
        
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
    }
}