using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance { get; private set; }

    public enum ParticleType
    {
        Pistoleffect,
        ShotGunEffect,
        RifleEffect,
        SMGEffect,
        BloodEffect,
        BrickImpact
    }

    public Dictionary<ParticleType, GameObject> particleDic = new Dictionary<ParticleType, GameObject>();

    public GameObject pistolEffect;
    public GameObject shotGunEffect;
    public GameObject RifleEffect;
    public GameObject SMGEffect;
    public GameObject bloodEffect;
    public GameObject BrickImpack;

    GameObject ParticleObj;

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

        particleDic.Add(ParticleType.Pistoleffect, pistolEffect);
        particleDic.Add(ParticleType.ShotGunEffect, shotGunEffect);
        particleDic.Add(ParticleType.RifleEffect, RifleEffect);
        particleDic.Add(ParticleType.SMGEffect, SMGEffect);
        particleDic.Add(ParticleType.BloodEffect, bloodEffect);
        particleDic.Add(ParticleType.BrickImpact, BrickImpack);

        pistolEffect.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
        shotGunEffect.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
        RifleEffect.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
        SMGEffect.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
        BrickImpack.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }

    public void PlayParticle(ParticleType type, Vector3 position)
    {
        //싱글톤으로 플레이어의 위치를 가져옴
        Transform playerTransform = PlayerManager.Instance.transform;

        //플레이어 방향을 기준으로 파티클이 회전하도록 설정
        Vector3 directionToPlayer = playerTransform.position - position;
        Quaternion rotation = Quaternion.LookRotation(directionToPlayer);


        if (particleDic.ContainsKey(type))
        {
            ParticleObj = Instantiate(particleDic[type],position, Quaternion.identity);
            StartCoroutine(particleEnd(ParticleObj));
        }

        
    }

    IEnumerator particleEnd(GameObject ParticleObj)
    {
        ParticleObj.SetActive(true);
        yield return new WaitForSeconds(1.0f);

        Destroy(ParticleObj);
    }
}
