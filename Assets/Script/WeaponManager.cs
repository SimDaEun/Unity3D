using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager instance;

    [Serializable] //인스펙터창에서 보이고 싶을 때 (private)
    public class WeaponSpawnPoint
    {
        public Weapon.WeaponType weaponType;  //무기 타입
        public Transform spawnPoint;  //해당 무기의 생성 위치 
    }

    public List<WeaponSpawnPoint> spawnPoints = new List<WeaponSpawnPoint>();  //인스펙터에서 관리할 스폰 리스트 
    private Dictionary<Weapon.WeaponType, Transform> weaponSpawnPoints = new Dictionary<Weapon.WeaponType, Transform>();

    //리스트의 접근을 쉽게 하기 위한 딕셔너리 생성
    //딕셔너리의 큰 단점: 인스펙터 창에서 안보인다.

    public List<GameObject> weaponPrefabs;  //무기 프리펩 리스트 
    private Dictionary<Weapon.WeaponType, GameObject> weaponInventory  = new Dictionary<Weapon.WeaponType, GameObject>();

    private GameObject currentWeapon; //현재 장착된 무기 
    private Weapon.WeaponType currentWeaponType;  //현재 무기 타입 
    private Weapon currentWeaponComponent;  //현재 무기의 Weapon 컴포넌트(EffectPos를 가져오기 위해 사용) 
    

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

        foreach (var spawnPoint in spawnPoints)  //리스트를 딕셔너리로 변환하는 코드 
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
            Debug.Log("무기가 인벤토리에 없습니다.");
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
        Debug.Log($"{weaponType} 무기 장착");
    }

    public void AddWeapon(GameObject weapon)
    {
        Weapon weaponComponent = weapon.GetComponent<Weapon>();
        SphereCollider sphereCollider = weaponComponent.GetComponent<SphereCollider>();


        if (weaponComponent != null && !weaponInventory.ContainsKey(weaponComponent.weaponType) && sphereCollider)
        {
            sphereCollider.enabled = false;
            weaponInventory.Add(weaponComponent.weaponType, weapon);
            Debug.Log($"{weaponComponent.weaponType} 무기 획득");
        }
    }


    public Weapon.WeaponType GetCurrentWeaponType()  //weapon type 반환해주는 함수 
    {
        return currentWeaponType;
    }

    public Weapon GetCurrentWeaponComponent()
    {
        return currentWeaponComponent;
    }
}
