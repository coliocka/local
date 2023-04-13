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
    
    //当游戏对象成功连接到网络的时候就会执行一遍,每一个player连接进入的时候都会触发此函数
    public override void OnNetworkSpawn()//方式网络连接的时候报错，将原本在start里的都转移到OnNetworkSpawn里
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
        player.Setup();//先赋值再存
        GameManager.Singleton.RegisterPlayer(name, player);//调用RegisterPlayer函数之前是没有名称的
        //所以在调用之前需要自己写
    }

    private void SetLayerMaskForAllChildren(Transform transform, LayerMask layerMask)//递归更改所有组件的layer
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

    //当客户端断开连接之前，会调用这个函数
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (SceneCamera != null)
        {
            SceneCamera.gameObject.SetActive(true);
        }

        //此时transform已经创建了名字了
        GameManager.Singleton.UnregisterPlayer(transform.name);//删除玩家
    }
}
