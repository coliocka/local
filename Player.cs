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
    private bool[] componentsEnabled;//�洢����Ƿ����
    private bool colliderEnabled;

    private readonly NetworkVariable<int> currentHealth = new();//ֻ���ڷ��������޸�
    private readonly NetworkVariable<bool> isDead = new();//�жϵ�ǰ����Ƿ�����

    public void Setup()
    {
        componentsEnabled = new bool[componentsToDisable.Length];
        for (int i=0; i< componentsToDisable.Length; ++i)
        {
            componentsEnabled[i] = componentsToDisable[i].enabled;
        }

        Collider col = GetComponent<Collider>();//��������
        colliderEnabled = col.enabled;
        SetDefaults();
    }

    private void SetDefaults()//��ʼ��
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

    public void TakeDamage(int damage)//�ܵ��˺���ֻ���ڷ������˵���
    {
        if (isDead.Value) return;//����������Ͳ����˺���

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

    private IEnumerator Respawn()//����
    {
        yield return new WaitForSeconds(GameManager.Singleton.MatchingSettings.respawnTime);//�ӳ�3s

        SetDefaults();//�ָ�����Ȩ
        GetComponentInChildren<Animator>().SetInteger("direction", 0);//�ָ�վ������
        GetComponent<Rigidbody>().useGravity = true;//��������Ч

        if (IsLocalPlayer)//�Ƿ��Ǳ������
        {
            transform.position = new Vector3(0f, 10f, 0f);//��������
        }
    }

    private void DieOnServer()//�ڷ�������ִ��
    {
        Die();
    }

    [ClientRpc]//�ڷ������˵��ã����Զ���ÿһ���ͻ��˶�ִ��һ�飬�������ڷ�������ִ��
    private void DieClientRpc()
    {
        Die();
    }

    private void Die()//�����ڷ������ϵ���
    {
        GetComponentInChildren<Animator>().SetInteger("direction", -1);//��������
        GetComponent<Rigidbody>().useGravity = false;//��������ʧ

        for (int i=0;i<componentsToDisable.Length; ++i)
        {
            componentsToDisable[i].enabled = false;
        }
        Collider col = GetComponent<Collider>();
        col.enabled = false;

        StartCoroutine(Respawn());//�����߳�
    }

    public int GetHealth()
    {
        return currentHealth.Value;
    }
}
