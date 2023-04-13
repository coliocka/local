using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField]
    private float speed = 5.0f;
    [SerializeField]
    private PlayerController controller;
    [SerializeField]
    private float lookSensitivity = 8f;
    [SerializeField]
    private float thrusterForce = 20f;

    private float distToGround = 0f;

    //private ConfigurableJoint joint;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;//锁住鼠标
        //joint = GetComponent<ConfigurableJoint>();
        distToGround = GetComponent<Collider>().bounds.extents.y;//中心点距离下一个碰撞体的距离
    }

    // Update is called once per frame
    void Update()
    {
        float xMov = Input.GetAxisRaw("Horizontal");//获取左右输入
        float yMov = Input.GetAxisRaw("Vertical");//上下输入

        Vector3 velocity = (transform.right * xMov + transform.forward * yMov).normalized * speed;
        controller.Move(velocity);

        float xMouse = Input.GetAxisRaw("Mouse X");
        float yMouse = Input.GetAxisRaw("Mouse Y");
        //print(xMouse.ToString() + yMouse.ToString());

        Vector3 yRotation = new Vector3(0f, xMouse, 0f) * lookSensitivity;
        Vector3 xRotation = new Vector3(-yMouse, 0f, 0f) * lookSensitivity;
        controller.Rotation(yRotation, xRotation);

        if (Input.GetButton("Jump"))//每一帧都判断,按住按钮
        {
            if(Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f))
            {
                Vector3 force = Vector3.up * thrusterForce;//向上的力
                controller.Thrust(force);//赋予刚体一个力
            }
            /*joint.yDrive = new JointDrive//赋值需要把整个值一起赋值，不能只改变单个值
            {
                positionSpring = 0f,
                positionDamper = 0f,
                maximumForce = 0f,
            };//取消弹力*/
        }
        /*else//松开恢复弹力
        {
            joint.yDrive = new JointDrive
            {
                positionSpring = 20f,
                positionDamper = 0f,
                maximumForce = 40f,
            };
        }*/
    }
}
