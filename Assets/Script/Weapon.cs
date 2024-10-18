using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum WeaponType
    {
        Pistol, ShotGun, Rifle, SMG
    }

    public WeaponType weaponType;

    public Camera targetCamera;
    public Transform UIImage; //�ش� ������ ���� �ߴ� UI 


    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        UIImage.gameObject.SetActive(false);
    }

    void Update()
    {
        Vector3 direction = targetCamera.transform.position - UIImage.position;  //ī�޶��� ���� ��� 
        direction.y = 0;  //Y�� ȸ���� �����Ͽ� UI�� �� �Ʒ��� �������� �ʵ��� ��
        Quaternion rotation = Quaternion.LookRotation(-direction);  //UI�� ī�޶� �ٶ󺸵��� ȸ�� 
        UIImage.rotation = rotation;  //UIImage ȸ�� ���� 
    }

    private void OnTriggerEnter(Collider other)  //Trigger �� ������ UI Ȱ��ȭ �ϴ� �Լ�
    {
        UIImage.gameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)  //Trigger���� ����� UI ��Ȱ��ȭ �ǵ��� �ϴ� �Լ�
    {
        UIImage.gameObject.SetActive(false);
    }
}
