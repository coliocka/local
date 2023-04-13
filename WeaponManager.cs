using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField] private PlayerWeapon primaryWeapon;//主武器
    [SerializeField] private PlayerWeapon secondaryWeapon;//副武器

    [SerializeField] private PlayerWeapon primaryWeapon2;//主武器2
    [SerializeField] private PlayerWeapon secondaryWeapon2;

    [SerializeField] private PlayerWeapon primaryWeapon3;
    [SerializeField] private PlayerWeapon secondaryWeapon3;

    [SerializeField]
    private GameObject weaponHolder;//武器库

    private PlayerWeapon currentWeapon;
    private WeaponGraphics currentGraphics;//武器特效
    private AudioSource currentAudioSource;//武器音效

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

        GameObject weaponObject = Instantiate(currentWeapon.graphics, weaponHolder.transform.position, weaponHolder.transform.rotation);//实例化
        weaponObject.transform.SetParent(weaponHolder.transform);//挂在对象上

        currentGraphics = weaponObject.GetComponent<WeaponGraphics>();//取出特效
        currentAudioSource = weaponObject.GetComponent<AudioSource>();

        if (IsLocalPlayer)
        {
            currentAudioSource.spatialBlend = 0f;//自己开枪变为2D音效
        }
    }

    public PlayerWeapon GetCurrentWeapon()//返回当前的武器
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

    private void ToggleWeapon()//切换武器
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
    private void ToggleWeaponClientRpc()//服务器端同步，服务器执行
    {
        ToggleWeapon();
    }

    [ServerRpc]
    private void ToggleWeaponServerRpc()//服务器调用，客户端执行，服务器不执行
    {
        if (!IsHost)
        {
            ToggleWeapon();//同步到服务器端
        }
        ToggleWeaponClientRpc();
    }
    // Update is called once per frame
    void Update()
    {
        if (IsLocalPlayer)//如果是本地玩家
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ToggleWeaponServerRpc();
            }
        }
    }
}