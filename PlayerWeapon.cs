using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]//�ܹ����л�
public class PlayerWeapon
{
    //��Ҫ���л�
    public string name = "M16A1";//ǹ��
    public int damage = 10;//�˺�
    public float range = 100f;//�������

    public float shootRate = 10f;//1s���Է������ӵ���С�ڵ���0��ʾ����
    public float shootCoolDownTime = 0.75f;//����ģʽ��ȴʱ��
    public float recoiForce = 2f;//������

    public GameObject graphics;
}
