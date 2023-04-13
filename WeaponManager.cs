using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField] private PlayerWeapon primaryWeapon;//������
    [SerializeField] private PlayerWeapon secondaryWeapon;//������

    [SerializeField] private PlayerWeapon primaryWeapon2;//������2
    [SerializeField] private PlayerWeapon secondaryWeapon2;

    [SerializeField] private PlayerWeapon primaryWeapon3;
    [SerializeField] private PlayerWeapon secondaryWeapon3;

    [SerializeField]
    private GameObject weaponHolder;//������

    private PlayerWeapon currentWeapon;
    private WeaponGraphics currentGraphics;//������Ч
    private AudioSource currentAudioSource;//������Ч

    // Start is called before the first frame update
    void Start()
    {
        EquipWeapon(primaryWeapon);
    }

    public void EquipWeapon(PlayerWeapon _weapon)
    {
        currentWeapon = _weapon;

        if(weaponHolder.transform.childCount > 0)
        {
            Destroy(weaponHolder.transform.GetChild(0).gameObject);
        }

        GameObject weaponObject = Instantiate(currentWeapon.graphics, weaponHolder.transform.position, weaponHolder.transform.rotation);//ʵ����
        weaponObject.transform.SetParent(weaponHolder.transform);//���ڶ�����

        currentGraphics = weaponObject.GetComponent<WeaponGraphics>();//ȡ����Ч
        currentAudioSource = weaponObject.GetComponent<AudioSource>();

        if (IsLocalPlayer)
        {
            currentAudioSource.spatialBlend = 0f;//�Լ���ǹ��Ϊ2D��Ч
        }
    }

    public PlayerWeapon GetCurrentWeapon()//���ص�ǰ������
    {
        return currentWeapon;
    }

    public WeaponGraphics GetCurrentGraphics()
    {
        return currentGraphics;
    }

    public AudioSource GetCurrentAudioSource()
    {
        return currentAudioSource;
    }

    private void ToggleWeapon()//�л�����
    {
        if (currentWeapon == primaryWeapon)
        {
            EquipWeapon(secondaryWeapon);
        }else if(currentWeapon == secondaryWeapon)
        {
            EquipWeapon(primaryWeapon2);
        }else if(currentWeapon == primaryWeapon2)
        {
            EquipWeapon(secondaryWeapon2);
        }else if(currentWeapon== secondaryWeapon2)
        {
            EquipWeapon(primaryWeapon3);
        }else if(currentWeapon== primaryWeapon3)
        {
            EquipWeapon(secondaryWeapon3);
        }else if (currentWeapon == secondaryWeapon3)
        {
            EquipWeapon(primaryWeapon);
        }
    }

    [ClientRpc]
    private void ToggleWeaponClientRpc()//��������ͬ����������ִ��
    {
        ToggleWeapon();
    }

    [ServerRpc]
    private void ToggleWeaponServerRpc()//���������ã��ͻ���ִ�У���������ִ��
    {
        if (!IsHost)
        {
            ToggleWeapon();//ͬ������������
        }
        ToggleWeaponClientRpc();
    }
    // Update is called once per frame
    void Update()
    {
        if (IsLocalPlayer)//����Ǳ������
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ToggleWeaponServerRpc();
            }
        }
    }
}