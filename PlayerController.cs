using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private Rigidbody Rb;//获取刚体
    [SerializeField]
    private Camera cam;

    private Vector3 velocity = Vector3.zero;//每秒钟移动的距离
    private Vector3 yRotation = Vector3.zero;//旋转角色
    private Vector3 xRotation = Vector3.zero;//旋转摄像机
    private float recoiForce = 0f;//累加的后坐力

    private float cameraRotationTotal = 0f;//累计转了多少度
    [SerializeField]
    private float cameraRotationLimit = 85f;

    private Vector3 thrusterForce = Vector3.zero;//向上的推力

    private Vector3 lastFramePosition = Vector3.zero;//记录上一帧的位置

    private float eps = 0.01f;//误差

    private Animator animator;

    private float distToGround = 0f;


    private void Start()
    {
        lastFramePosition = transform.position;
        animator = GetComponentInChildren<Animator>();//获取动画，因为动画是子对象，所以用GetComponentInChildren

        distToGround = GetComponent<Collider>().bounds.extents.y;//中心点距离下一个碰撞体的距离
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
            Rb.MovePosition(Rb.position + velocity * Time.fixedDeltaTime);//fixUpdate上一次执行的时间
        }

        if(thrusterForce != Vector3.zero)
        {
            Rb.AddForce(thrusterForce);//作用Time.fixedDeltaTime:0.02秒
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
            Rb.transform.Rotate(yRotation + Rb.transform.up * Random.Range(-2f * recoiForce, 2 * recoiForce));//每一帧提供一个随机左右方向的摆动
        }
        if (xRotation != Vector3.zero || recoiForce > 0)//控制旋转角度
        {
            cameraRotationTotal += xRotation.x - recoiForce;
            cameraRotationTotal = Mathf.Clamp(cameraRotationTotal, -cameraRotationLimit, cameraRotationLimit);//将旋转角度限制在正负limit之间
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

        int direction = 0;//静止
        if (forward > eps)
        {
            direction = 1;//向前
        }
        else if(forward < -eps) 
        {
            if(right > eps)
            {
                direction = 4;//右后
            }
            else if(right < -eps)
            {
                direction = 6;//左后
            }
            else
            {
                direction = 5;//后
            }
        }
        else if (right > eps)
        {
            direction = 3;//向右
        }
        else if (right < -eps)
        {
            direction = 7;//向左
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
        if (IsLocalPlayer)//是本地玩家才支持手动控制移动
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
        //不放在FixedUpdate里是因为FixedUpdate在执行的时候一帧可能会调用多次，所以会出现远程玩家看到小碎步的情况
        //放在Update里就好了
        //远程玩家在Update里更改动画
        //本地玩家在FixedUpdate里更改动画
        //远程玩家跟本地玩家分开更改动画即可
        if (!IsLocalPlayer)
        {
            PerformAnimation();//不管是不是本地玩家都会有动作
        }

    }
}
