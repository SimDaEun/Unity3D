using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager instance;

    [Serializable] //�ν�����â���� ���̰� ���� �� (private)
    public class WeaponSpawnPoint
    {
        public Weapon.WeaponType weaponType;  //���� Ÿ��
        public Transform spawnPoint;  //�ش� ������ ���� ��ġ 
    }

    public List<WeaponSpawnPoint> spawnPoints = new List<WeaponSpawnPoint>();  //�ν����Ϳ��� ������ ���� ����Ʈ 
    private Dictionary<Weapon.WeaponType, Transform> weaponSpawnPoints = new Dictionary<Weapon.WeaponType, Transform>();

    //����Ʈ�� ������ ���� �ϱ� ���� ��ųʸ� ����
    //��ųʸ��� ū ����: �ν����� â���� �Ⱥ��δ�.

    public List<GameObject> weaponPrefabs;  //���� ������ ����Ʈ 
    private Dictionary<Weapon.WeaponType, GameObject> weaponInventory  = new Dictionary<Weapon.WeaponType, GameObject>();

    private GameObject currentWeapon; //���� ������ ���� 
    private Weapon.WeaponType currentWeaponType;  //���� ���� Ÿ�� 
    private Weapon currentWeaponComponent;  //���� ������ Weapon ������Ʈ(EffectPos�� �������� ���� ���) 
    

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        foreach (var spawnPoint in spawnPoints)  //����Ʈ�� ��ųʸ��� ��ȯ�ϴ� �ڵ� 
        {
            if (!weaponSpawnPoints.ContainsKey(spawnPoint.weaponType))
            {
                weaponSpawnPoints.Add(spawnPoint.weaponType, spawnPoint.spawnPoint);
            }
        }
    }

    public void EquipWeapon(Weapon.WeaponType weaponType)
    {
        if (!weaponInventory.ContainsKey(weaponType))
        {
            Debug.Log("���Ⱑ �κ��丮�� �����ϴ�.");
        }

        //
        foreach (Transform child in weaponSpawnPoints[weaponType])
        {
            Destroy(child.gameObject);
        }

        GameObject newWeapon = Instantiate(weaponInventory[weaponType], weaponSpawnPoints[weaponType]);

        newWeapon.transform.localPosition = Vector3.zero;

        currentWeapon = newWeapon;
        currentWeaponType = weaponType;

        currentWeaponComponent = newWeapon.GetComponent<Weapon>();

        currentWeapon.SetActive(true);
        Debug.Log($"{weaponType} ���� ����");
    }

    public void AddWeapon(GameObject weapon)
    {
        Weapon weaponComponent = weapon.GetComponent<Weapon>();
        SphereCollider sphereCollider = weaponComponent.GetComponent<SphereCollider>();


        if (weaponComponent != null && !weaponInventory.ContainsKey(weaponComponent.weaponType) && sphereCollider)
        {
            sphereCollider.enabled = false;
            weaponInventory.Add(weaponComponent.weaponType, weapon);
            Debug.Log($"{weaponComponent.weaponType} ���� ȹ��");
        }
    }


    public Weapon.WeaponType GetCurrentWeaponType()  //weapon type ��ȯ���ִ� �Լ� 
    {
        return currentWeaponType;
    }

    public Weapon GetCurrentWeaponComponent()
    {
        return currentWeaponComponent;
    }
}
