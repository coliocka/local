using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]//能够串行化
public class PlayerWeapon
{
    //需要公有化
    public string name = "M16A1";//枪名
    public int damage = 10;//伤害
    public float range = 100f;//射击距离

    public float shootRate = 10f;//1s可以发多少子弹，小于等于0表示单发
    public float shootCoolDownTime = 0.75f;//单发模式冷却时间
    public float recoiForce = 2f;//后坐力

    public GameObject graphics;
}
