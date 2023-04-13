using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton;//����ģʽ,���õ�ʵ����

    [SerializeField]
    public MatchingSettings MatchingSettings;

    //��һ���ֵ�ά��ÿһ�����ָ�player֮��Ĺ�ϵ
    private static readonly Dictionary<string, Player> players = new();

    private void Awake()
    {
        //��ʼ���Ὣ�����Ӧ�ø�ֵ��������
        Singleton = this;
    }

    //����һ�����
    public void RegisterPlayer(string name, Player player)
    {
        player.transform.name = name;//������
        players.Add(name, player);//�����ֵ���
    }

    //ɾ�����
    public void UnregisterPlayer(string name)
    {
        if (players.ContainsKey(name))
        {
            players.Remove(name);
        }
    }

    public Player GetPlayer(string name)//�����������
    {
        return players[name];
    }

    private void OnGUI()//ÿһ֡����һ�Σ��滭
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
