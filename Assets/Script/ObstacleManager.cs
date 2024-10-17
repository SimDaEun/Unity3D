using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
public enum ObstacleType   //상수를 관리하는 열거형 자료형 
{
    Moving,  //움직이는 장애물
    Rotating, //회전하는 장애물
    Visibility, //투명 장애물
    ShrinkingPlatform, //크기가 점점 줄어드는 장애물 (충돌시)
    DamagingPlatform  //HP가 감소하는 장애물 (충돌시)
}
public class ObstacleManager : MonoBehaviour
{
    public ObstacleType ObstacleType;

    //이동 
    public List<Transform> points;  //이동 경로 리스트
    public float moveSpeed = 2.0f;  //이동 속도 
    private int currentPointIndex = 0;  //현재 목표 지점 인덱스
    //회전
    public Vector3 rotationAxis = Vector3.up;  //회전축 up으로 
    public float rotationSpeed = 50f;  //회전 속도 
    //투명
    private Renderer objectRenderer;
    public bool isVisible = true;
    //커지기
    private float shrinkRate = 0.1f;  //매 프레임 크기 감소 비율
    private Vector3 initialScale;  //초기 크기 
    //데미지
    public float damageRate = 10.0f;

    //Vector3 previousPosition;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();

        if (points.Count == 0)   //리스트가 비어있다면
        {
            transform.position = points[0].position;
        }

        initialScale = transform.localScale;
    }
    
    void Update()
    {
        switch (ObstacleType)
        {
            case ObstacleType.Moving:
                Movement();
                break;
            case ObstacleType.Rotating:
                Rotation();
                break;
            case ObstacleType.Visibility:
                Visiblity();
                break;
        }
    }

    void Movement()  //이동에 대한 함수 
    {
        if (points.Count == 0) return;

        //현재 목표 지점 
        Transform targetPoint = points[currentPointIndex];

        //현재 위치에서 목표 지점으로 이동 
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed*Time.deltaTime);

        //목표 지점에 도달하면 다음 지점으로 인덱스를 순환 
        if (transform.position == targetPoint.position)
        {
            currentPointIndex = (currentPointIndex + 1) % points.Count;
            Debug.Log(currentPointIndex);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Player")
        {
            switch (ObstacleType)
            {
                case ObstacleType.ShrinkingPlatform:
                    ShrinkingPlatform();
                    break;
                case ObstacleType.DamagingPlatform:
                    DamagingPlatform();
                    break;
                case ObstacleType.Rotating:
                    DamagingPlatform();
                    break;
            }
        }


    }

    void Rotation()  //회전에 대한 함수 
    {
        transform.Rotate(rotationAxis, rotationSpeed*Time.deltaTime);
    }

    public void Visiblity()
    {
        objectRenderer.enabled = isVisible;
        //방법 2 gameObject.SetActive(isVisible);
    }

    void ShrinkingPlatform()
    {
        if (transform.localScale.x > 0.1f)
        {
            transform.localScale -= Vector3.one * shrinkRate;  //점점 줄어들게 함 
        }
    }

    void DamagingPlatform()
    {
        PlayerManager.Instance.TakeDamage(damageRate);
        Debug.Log("Player의 현재 체력 : " + PlayerManager.Instance.HP);
    }
}
