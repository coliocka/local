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
        Cursor.lockState = CursorLockMode.Locked;//��ס���
        //joint = GetComponent<ConfigurableJoint>();
        distToGround = GetComponent<Collider>().bounds.extents.y;//���ĵ������һ����ײ��ľ���
    }

    // Update is called once per frame
    void Update()
    {
        float xMov = Input.GetAxisRaw("Horizontal");//��ȡ��������
        float yMov = Input.GetAxisRaw("Vertical");//��������

        Vector3 velocity = (transform.right * xMov + transform.forward * yMov).normalized * speed;
        controller.Move(velocity);

        float xMouse = Input.GetAxisRaw("Mouse X");
        float yMouse = Input.GetAxisRaw("Mouse Y");
        //print(xMouse.ToString() + yMouse.ToString());

        Vector3 yRotation = new Vector3(0f, xMouse, 0f) * lookSensitivity;
        Vector3 xRotation = new Vector3(-yMouse, 0f, 0f) * lookSensitivity;
        controller.Rotation(yRotation, xRotation);

        if (Input.GetButton("Jump"))//ÿһ֡���ж�,��ס��ť
        {
            if(Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f))
            {
                Vector3 force = Vector3.up * thrusterForce;//���ϵ���
                controller.Thrust(force);//�������һ����
            }
            /*joint.yDrive = new JointDrive//��ֵ��Ҫ������ֵһ��ֵ������ֻ�ı䵥��ֵ
            {
                positionSpring = 0f,
                positionDamper = 0f,
                maximumForce = 0f,
            };//ȡ������*/
        }
        /*else//�ɿ��ָ�����
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
