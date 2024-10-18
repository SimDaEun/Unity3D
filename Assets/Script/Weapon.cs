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
    public Transform UIImage; //해당 아이템 위에 뜨는 UI 


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
        Vector3 direction = targetCamera.transform.position - UIImage.position;  //카메라의 방향 계산 
        direction.y = 0;  //Y축 회전을 고정하여 UI가 위 아래로 기울어지지 않도록 함
        Quaternion rotation = Quaternion.LookRotation(-direction);  //UI가 카메라를 바라보도록 회전 
        UIImage.rotation = rotation;  //UIImage 회전 적용 
    }

    private void OnTriggerEnter(Collider other)  //Trigger 에 닿으면 UI 활성화 하는 함수
    {
        UIImage.gameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)  //Trigger에서 벗어나면 UI 비활성화 되도록 하는 함수
    {
        UIImage.gameObject.SetActive(false);
    }
}
