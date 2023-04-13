using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField]
    private Behaviour[] componentsToDisable;
    private Camera SceneCamera;
    
    //����Ϸ����ɹ����ӵ������ʱ��ͻ�ִ��һ��,ÿһ��player���ӽ����ʱ�򶼻ᴥ���˺���
    public override void OnNetworkSpawn()//��ʽ�������ӵ�ʱ�򱨴���ԭ����start��Ķ�ת�Ƶ�OnNetworkSpawn��
    {
        base.OnNetworkSpawn();

        if (!IsLocalPlayer)
        {
            SetLayerMaskForAllChildren(transform, LayerMask.NameToLayer("Remote Player"));
            DisableComponents();
        }
        else
        {
            SetLayerMaskForAllChildren(transform, LayerMask.NameToLayer("Player"));
            SceneCamera = Camera.main;
            if(SceneCamera != null)
            {
                SceneCamera.gameObject.SetActive(false);
            }
        }

        string name = "Player" + GetComponent<NetworkObject>().NetworkObjectId.ToString();
        Player player = GetComponent<Player>();
        player.Setup();//�ȸ�ֵ�ٴ�
        GameManager.Singleton.RegisterPlayer(name, player);//����RegisterPlayer����֮ǰ��û�����Ƶ�
        //�����ڵ���֮ǰ��Ҫ�Լ�д
    }

    private void SetLayerMaskForAllChildren(Transform transform, LayerMask layerMask)//�ݹ�������������layer
    {
        transform.gameObject.layer = layerMask;
        for (int i=0;i<transform.childCount;i++)
        {
            SetLayerMaskForAllChildren(transform.GetChild(i), layerMask);
        }
    }

    private void DisableComponents()
    {
        for (int i=0;i<componentsToDisable.Length;i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }

    //���ͻ��˶Ͽ�����֮ǰ��������������
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (SceneCamera != null)
        {
            SceneCamera.gameObject.SetActive(true);
        }

        //��ʱtransform�Ѿ�������������
        GameManager.Singleton.UnregisterPlayer(transform.name);//ɾ�����
    }
}
