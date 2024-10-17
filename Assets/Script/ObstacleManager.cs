using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
public enum ObstacleType   //����� �����ϴ� ������ �ڷ��� 
{
    Moving,  //�����̴� ��ֹ�
    Rotating, //ȸ���ϴ� ��ֹ�
    Visibility, //���� ��ֹ�
    ShrinkingPlatform, //ũ�Ⱑ ���� �پ��� ��ֹ� (�浹��)
    DamagingPlatform  //HP�� �����ϴ� ��ֹ� (�浹��)
}
public class ObstacleManager : MonoBehaviour
{
    public ObstacleType ObstacleType;

    //�̵� 
    public List<Transform> points;  //�̵� ��� ����Ʈ
    public float moveSpeed = 2.0f;  //�̵� �ӵ� 
    private int currentPointIndex = 0;  //���� ��ǥ ���� �ε���
    //ȸ��
    public Vector3 rotationAxis = Vector3.up;  //ȸ���� up���� 
    public float rotationSpeed = 50f;  //ȸ�� �ӵ� 
    //����
    private Renderer objectRenderer;
    public bool isVisible = true;
    //Ŀ����
    private float shrinkRate = 0.1f;  //�� ������ ũ�� ���� ����
    private Vector3 initialScale;  //�ʱ� ũ�� 
    //������
    public float damageRate = 10.0f;

    //Vector3 previousPosition;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();

        if (points.Count == 0)   //����Ʈ�� ����ִٸ�
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

    void Movement()  //�̵��� ���� �Լ� 
    {
        if (points.Count == 0) return;

        //���� ��ǥ ���� 
        Transform targetPoint = points[currentPointIndex];

        //���� ��ġ���� ��ǥ �������� �̵� 
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed*Time.deltaTime);

        //��ǥ ������ �����ϸ� ���� �������� �ε����� ��ȯ 
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

    void Rotation()  //ȸ���� ���� �Լ� 
    {
        transform.Rotate(rotationAxis, rotationSpeed*Time.deltaTime);
    }

    public void Visiblity()
    {
        objectRenderer.enabled = isVisible;
        //��� 2 gameObject.SetActive(isVisible);
    }

    void ShrinkingPlatform()
    {
        if (transform.localScale.x > 0.1f)
        {
            transform.localScale -= Vector3.one * shrinkRate;  //���� �پ��� �� 
        }
    }

    void DamagingPlatform()
    {
        PlayerManager.Instance.TakeDamage(damageRate);
        Debug.Log("Player�� ���� ü�� : " + PlayerManager.Instance.HP);
    }
}
