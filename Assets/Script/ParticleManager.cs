using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance { get; private set; }

    public enum ParticleType
    {
        PistolEffect,
        ShotGunEffect,
        RifleEffect,
        SMGEffect,
        BloodEffect,
        BrickImpact
    }

    public Dictionary<ParticleType, GameObject> particleDic = new Dictionary<ParticleType, GameObject>();

    public Dictionary<ParticleType,Queue<GameObject>> particlePools = new Dictionary<ParticleType, Queue<GameObject>>();    

    public GameObject pistolEffect;
    public GameObject shotGunEffect;
    public GameObject RifleEffect;
    public GameObject SMGEffect;
    public GameObject bloodEffect;
    public GameObject BrickImpack;

    GameObject ParticleObj;

    //������Ʈ Ǯ��
    public int poolSize = 20;


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

        particleDic.Add(ParticleType.PistolEffect, pistolEffect);
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

    private void Start()
    {
        foreach (var particleType in particleDic.Keys)
        {
            Queue<GameObject> pool = new Queue<GameObject> ();   //ť : ���Լ��� 
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(particleDic[particleType]);
                obj.SetActive(false);
                pool.Enqueue(obj); //ť�� �߰��ϴ� �Լ� 

            }
            particlePools.Add(particleType, pool);
        }
    }

    public void PlayParticle(ParticleType type, Vector3 position)
    {
        if (particlePools.ContainsKey(type))
        {
            GameObject particleObj = particlePools[type].Dequeue();

            if (particleObj != null)
            {
                particleObj.transform.position = position;
                ParticleSystem particleSystem = particleObj.GetComponentInChildren<ParticleSystem>();

                if (particleSystem.isPlaying)
                {
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }

                particleObj.SetActive(true);
                particleSystem.Play();

                StartCoroutine(particleEnd(type, particleObj, particleSystem));
            }
        }

        ////�̱������� �÷��̾��� ��ġ�� ������
        //Transform playerTransform = PlayerManager.Instance.transform;

        ////�÷��̾� ������ �������� ��ƼŬ�� ȸ���ϵ��� ����
        //Vector3 directionToPlayer = playerTransform.position - position;
        //Quaternion rotation = Quaternion.LookRotation(directionToPlayer);


        //if (particleDic.ContainsKey(type))
        //{
        //    ParticleObj = Instantiate(particleDic[type],position, Quaternion.identity);
        //    StartCoroutine(particleEnd(ParticleObj));
        //}

    }

    IEnumerator particleEnd(ParticleType type, GameObject particleObj, ParticleSystem particleSystem)
    {
        while (particleSystem.isPlaying)
        {
            yield return null;
        }
        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particleObj.SetActive(false);
        particlePools[type].Enqueue(particleObj);  //�����͸� Queue�� �߰��ϴ� �Լ�. ���ο� ��Ҹ� ť�� ���� �߰� 
    }


}
