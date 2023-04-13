using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton;//单列模式,调用的实例，

    [SerializeField]
    public MatchingSettings MatchingSettings;

    //开一个字典维护每一个名字跟player之间的关系
    private static readonly Dictionary<string, Player> players = new();

    private void Awake()
    {
        //初始化会将对象的应用赋值到对象上
        Singleton = this;
    }

    //加入一个玩家
    public void RegisterPlayer(string name, Player player)
    {
        player.transform.name = name;//重命名
        players.Add(name, player);//加入字典中
    }

    //删除玩家
    public void UnregisterPlayer(string name)
    {
        if (players.ContainsKey(name))
        {
            players.Remove(name);
        }
    }

    public Player GetPlayer(string name)//返回玩家名字
    {
        return players[name];
    }

    private void OnGUI()//每一帧调用一次，绘画
    {
        GUILayout.BeginArea(new Rect(200f, 200f, 400f, 400f));
        GUILayout.BeginVertical();

        GUI.color = Color.red;
        foreach (string name in players.Keys)
        {
            Player player = GetPlayer(name);
            GUILayout.Label(name + "-" + player.GetHealth());
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
