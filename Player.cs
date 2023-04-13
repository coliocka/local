using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private int MaxHealth = 100;
    [SerializeField]
    private Behaviour[] componentsToDisable;
    private bool[] componentsEnabled;//存储组件是否禁用
    private bool colliderEnabled;

    private readonly NetworkVariable<int> currentHealth = new();//只能在服务器端修改
    private readonly NetworkVariable<bool> isDead = new();//判断当前玩家是否死亡

    public void Setup()
    {
        componentsEnabled = new bool[componentsToDisable.Length];
        for (int i=0; i< componentsToDisable.Length; ++i)
        {
            componentsEnabled[i] = componentsToDisable[i].enabled;
        }

        Collider col = GetComponent<Collider>();//单独特判
        colliderEnabled = col.enabled;
        SetDefaults();
    }

    private void SetDefaults()//初始化
    {
        for (int i=0;i< componentsToDisable.Length; ++i)
        {
            componentsToDisable[i].enabled = componentsEnabled[i];
        }
        Collider col = GetComponent<Collider>();
        col.enabled = colliderEnabled;

        if (IsServer)
        {
            currentHealth.Value = MaxHealth;
            isDead.Value = false;
        }
    }

    public bool IsDead()
    {
        return isDead.Value;
    }

    public void TakeDamage(int damage)//受到伤害，只会在服务器端调用
    {
        if (isDead.Value) return;//如果死亡，就不受伤害了

        currentHealth.Value -= damage;

        if(currentHealth.Value <= 0)
        {
            currentHealth.Value = 0;
            isDead.Value = true;

            if(!IsHost)
            {
                DieOnServer();
            }
            DieClientRpc();
        }
    }

    private IEnumerator Respawn()//重生
    {
        yield return new WaitForSeconds(GameManager.Singleton.MatchingSettings.respawnTime);//延迟3s

        SetDefaults();//恢复控制权
        GetComponentInChildren<Animator>().SetInteger("direction", 0);//恢复站立动画
        GetComponent<Rigidbody>().useGravity = true;//让重力生效

        if (IsLocalPlayer)//是否是本地玩家
        {
            transform.position = new Vector3(0f, 10f, 0f);//重生坐标
        }
    }

    private void DieOnServer()//在服务器上执行
    {
        Die();
    }

    [ClientRpc]//在服务器端调用，会自动在每一个客户端都执行一遍，但不会在服务器端执行
    private void DieClientRpc()
    {
        Die();
    }

    private void Die()//不会在服务器上调用
    {
        GetComponentInChildren<Animator>().SetInteger("direction", -1);//死亡动画
        GetComponent<Rigidbody>().useGravity = false;//让重力消失

        for (int i=0;i<componentsToDisable.Length; ++i)
        {
            componentsToDisable[i].enabled = false;
        }
        Collider col = GetComponent<Collider>();
        col.enabled = false;

        StartCoroutine(Respawn());//调用线程
    }

    public int GetHealth()
    {
        return currentHealth.Value;
    }
}
