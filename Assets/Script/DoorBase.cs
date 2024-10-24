using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBase : MonoBehaviour
{
    public bool isOpen = false; //���� �����ִ��� ���¸� ��Ÿ���� ����
    private Animator animator;
    public bool LastOpenForward = true; //������ ���� ���������� ���ȴ��� ���� 

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    //���� �������� �÷��̾ �տ� �ִ��� �ڿ��ִ��� �Ǵ��ؾ���
    bool isPlayerInfront(Transform player)
    {
        //�÷��̾�� �� ������ ���Ͱ� ���
        Vector3 toPlayer = (player.position - transform.position).normalized;
        //���� ���ϴ� ����� �÷��̾� ������ �� (���� ����)
        float doProduct = Vector3.Dot(toPlayer, transform.position);
        //doProduct > 0 �̸� �÷��̾ �� �տ� ���� 

        Debug.Log($"player : " + doProduct);
        return doProduct > 0;
    }

    public bool Open(Transform player)
    {
        if (!isOpen)
        {
            isOpen = true;

            //�÷��̾ �տ� ������ ������ �ִϸ��̼� ���, �ڿ������� ������ �ִϸ��̼� ��� 
            if (!isPlayerInfront(player))
            {
                animator.SetTrigger("OpenForward");  //������ �ִϸ��̼� 
                LastOpenForward = true;
            }
            else
            {
                animator.SetTrigger("OpenBackward");  //������ �ִϸ��̼� 
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
