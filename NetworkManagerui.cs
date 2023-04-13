using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerui : MonoBehaviour
{
    [SerializeField]
    private Button hostBtn;
    [SerializeField]
    private Button serverBtn;
    [SerializeField]
    private Button clientBtn;
    // Start is called before the first frame update
    void Start()
    {
        //µ÷ÓÃ°´Å¥
        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            DEStoryALLButtons();
        });
        serverBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
            DEStoryALLButtons();
        });
        clientBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            DEStoryALLButtons();
        });
    }

    private void DEStoryALLButtons()
    {
        Destroy(hostBtn.gameObject);
        Destroy(serverBtn.gameObject); 
        Destroy(clientBtn.gameObject);
    }
}
