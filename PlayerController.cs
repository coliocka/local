using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private Rigidbody Rb;//��ȡ����
    [SerializeField]
    private Camera cam;

    private Vector3 velocity = Vector3.zero;//ÿ�����ƶ��ľ���
    private Vector3 yRotation = Vector3.zero;//��ת��ɫ
    private Vector3 xRotation = Vector3.zero;//��ת�����
    private float recoiForce = 0f;//�ۼӵĺ�����

    private float cameraRotationTotal = 0f;//�ۼ�ת�˶��ٶ�
    [SerializeField]
    private float cameraRotationLimit = 85f;

    private Vector3 thrusterForce = Vector3.zero;//���ϵ�����

    private Vector3 lastFramePosition = Vector3.zero;//��¼��һ֡��λ��

    private float eps = 0.01f;//���

    private Animator animator;

    private float distToGround = 0f;


    private void Start()
    {
        lastFramePosition = transform.position;
        animator = GetComponentInChildren<Animator>();//��ȡ��������Ϊ�������Ӷ���������GetComponentInChildren

        distToGround = GetComponent<Collider>().bounds.extents.y;//���ĵ������һ����ײ��ľ���
    }

    public void Move(Vector3 _velocity)
    {
        velocity= _velocity;
    }

    public void Rotation(Vector3 _yRotation, Vector3 _xRotation)
    {
        yRotation= _yRotation;
        xRotation= _xRotation;
    }

    public void Thrust(Vector3 _thrusterForce)
    {
        thrusterForce= _thrusterForce;
    }

    public void AddRecoiForce(float newRecoiForce)
    {
        recoiForce += newRecoiForce;
    }
    private void PerformMovement()
    {
        if (velocity != Vector3.zero)
        {
            Rb.MovePosition(Rb.position + velocity * Time.fixedDeltaTime);//fixUpdate��һ��ִ�е�ʱ��
        }

        if(thrusterForce != Vector3.zero)
        {
            Rb.AddForce(thrusterForce);//����Time.fixedDeltaTime:0.02��
            thrusterForce = Vector3.zero;
        }
    }

    private void PerformRotation()
    {
        if (recoiForce < 0.1)
        {
            recoiForce = 0;
        }

        if (yRotation != Vector3.zero || recoiForce > 0) 
        {
            Rb.transform.Rotate(yRotation + Rb.transform.up * Random.Range(-2f * recoiForce, 2 * recoiForce));//ÿһ֡�ṩһ��������ҷ���İڶ�
        }
        if (xRotation != Vector3.zero || recoiForce > 0)//������ת�Ƕ�
        {
            cameraRotationTotal += xRotation.x - recoiForce;
            cameraRotationTotal = Mathf.Clamp(cameraRotationTotal, -cameraRotationLimit, cameraRotationLimit);//����ת�Ƕ�����������limit֮��
            cam.transform.localEulerAngles = new Vector3(cameraRotationTotal, 0f, 0f);
        }

        recoiForce *= 0.5f;
    }

    private void PerformAnimation()
    {
        Vector3 delaPosition = transform.position - lastFramePosition;
        lastFramePosition= transform.position;

        float forward = Vector3.Dot(delaPosition, transform.forward);
        float right = Vector3.Dot(delaPosition, transform.right);

        int direction = 0;//��ֹ
        if (forward > eps)
        {
            direction = 1;//��ǰ
        }
        else if(forward < -eps) 
        {
            if(right > eps)
            {
                direction = 4;//�Һ�
            }
            else if(right < -eps)
            {
                direction = 6;//���
            }
            else
            {
                direction = 5;//��
            }
        }
        else if (right > eps)
        {
            direction = 3;//����
        }
        else if (right < -eps)
        {
            direction = 7;//����
        }

        if (GetComponent<Player>().IsDead())
        {
            direction = -1;
        }

        if(!Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f))
        {
            direction = 8;
        }
        animator.SetInteger("direction", direction);
    }

    private void FixedUpdate()
    {
        if (IsLocalPlayer)//�Ǳ�����Ҳ�֧���ֶ������ƶ�
        {
            PerformMovement();
            PerformRotation();
        }

        if (IsLocalPlayer)
        {
            PerformAnimation();
        }
    }

    private void Update()
    {
        //������FixedUpdate������ΪFixedUpdate��ִ�е�ʱ��һ֡���ܻ���ö�Σ����Ի����Զ����ҿ���С�鲽�����
        //����Update��ͺ���
        //Զ�������Update����Ķ���
        //���������FixedUpdate����Ķ���
        //Զ����Ҹ�������ҷֿ����Ķ�������
        if (!IsLocalPlayer)
        {
            PerformAnimation();//�����ǲ��Ǳ�����Ҷ����ж���
        }

    }
}
