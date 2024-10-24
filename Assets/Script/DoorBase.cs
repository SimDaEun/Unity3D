using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBase : MonoBehaviour
{
    public bool isOpen = false; //문이 열려있는지 상태를 나타내는 변수
    private Animator animator;
    public bool LastOpenForward = true; //마지막 문이 정방향으로 열렸는지 여부 

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    //문을 기준으로 플레이어가 앞에 있는지 뒤에있는지 판단해야함
    bool isPlayerInfront(Transform player)
    {
        //플레이어와 문 사이의 벡터값 계산
        Vector3 toPlayer = (player.position - transform.position).normalized;
        //문이 향하는 방향과 플레이어 방향을 비교 (내적 연산)
        float doProduct = Vector3.Dot(toPlayer, transform.position);
        //doProduct > 0 이면 플레이어가 문 앞에 있음 

        Debug.Log($"player : " + doProduct);
        return doProduct > 0;
    }

    public bool Open(Transform player)
    {
        if (!isOpen)
        {
            isOpen = true;

            //플레이어가 앞에 있으면 정방향 애니메이션 재생, 뒤에있으면 역방향 애니메이션 재생 
            if (!isPlayerInfront(player))
            {
                animator.SetTrigger("OpenForward");  //정방향 애니메이션 
                LastOpenForward = true;
            }
            else
            {
                animator.SetTrigger("OpenBackward");  //역방향 애니메이션 
                LastOpenForward = false;
            }

            return true;
        }

        return false;
    }

    public void CloseForward(Transform player)
    {
        if (isOpen)
        {
            isOpen = false;
            animator.SetTrigger("CloseForward");
        }
    }

    public void CloseBackward(Transform player)
    {
        if (isOpen)
        {
            isOpen = false;
            animator.SetTrigger("CloseBackward");
        }
    }

}
