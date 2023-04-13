using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour//使用ServerRpc需要使用NetworkBehaviour
{
    private const string PLAYER_TAG = "Player";//常量标签

    private WeaponManager weaponManager;//将武器信息引用
    private PlayerWeapon currentWeapon;//引用武器库

    private float shootCoolDownTime = 0f;//距离上次开枪，过了多久
    private int autoShootCount = 0;//当前一共连开了多少枪

    [SerializeField]
    private LayerMask mask;

    private Camera cam;
    private PlayerController playerController;

    enum HitEffectMaterial//特效类别
    {
        Metal,
        Stone,
    }

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        weaponManager = GetComponent<WeaponManager>();
        playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        shootCoolDownTime += Time.deltaTime;//冷却时间

        if (!IsLocalPlayer) return;

        currentWeapon = weaponManager.GetCurrentWeapon();//获取当前的武器信息

        if (currentWeapon.shootRate <= 0)
        {
            if (Input.GetButtonDown("Fire1") && shootCoolDownTime >= currentWeapon.shootCoolDownTime)//单发模式,只检测鼠标按下
            {
                Shoot();
                shootCoolDownTime = 0f;//重置冷却时间
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                autoShootCount = 0;
                InvokeRepeating(nameof(Shoot), 0f, 1f / currentWeapon.shootRate);//周期性触发
                //开始时间，时间间隔
            }
            else if (Input.GetButtonUp("Fire1") || Input.GetKeyDown(KeyCode.Q))
            {
                autoShootCount = 0;
                CancelInvoke(nameof(Shoot));
            }
        }
    }

    private void OnHit(Vector3 pos, Vector3 normal, HitEffectMaterial meterial)//击中点的特效
    {
        GameObject hitEffectPrefab;
        if (meterial == HitEffectMaterial.Metal)
        {
            hitEffectPrefab = weaponManager.GetCurrentGraphics().metalHitEffectPrefab;
        }
        else
        {
            hitEffectPrefab = weaponManager.GetCurrentGraphics().stoneHitEffectPrefab;
        }

        GameObject hitEffectObject = Instantiate(hitEffectPrefab, pos, Quaternion.LookRotation(normal));
        ParticleSystem particleSystem = hitEffectObject.GetComponent<ParticleSystem>();
        particleSystem.Emit(1);
        particleSystem.Play();
        Destroy(hitEffectObject, 2f);
    }

    [ClientRpc]
    private void OnHitClientRpc(Vector3 pos, Vector3 normal, HitEffectMaterial meterial)
    {
        OnHit(pos, normal, meterial);
    }

    [ServerRpc]
    private void OnHitServerRpc(Vector3 pos, Vector3 normal, HitEffectMaterial meterial)
    {
        if (!IsHost)
        {
            OnHit(pos, normal, meterial);
        }
        OnHitClientRpc(pos, normal, meterial);
    }
    private void OnShoot(float recoiForce)//每次射击相关的逻辑，包含特效、声音等
    {
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();//播放特效
        weaponManager.GetCurrentAudioSource().Play();//播放音效

        if (IsLocalPlayer)//只会给本地玩家添加后座力
        {
            playerController.AddRecoiForce(recoiForce);
        }
    }

    [ClientRpc]
    private void OnShootClientRpc(float recoiForce)
    {
        OnShoot(recoiForce);
    }

    [ServerRpc]
    private void OnShootServerRpc(float recoiForce)
    {
        if (IsHost)
        {
            OnShoot(recoiForce);
        }
        OnShootClientRpc(recoiForce);//调用ClientRpc时，会重复告诉每一个ClientRpc
    }
    private void Shoot()
    {
        autoShootCount++;
        float recoiForce = currentWeapon.recoiForce;//记录当前后座力

        if (autoShootCount <= 3)
        {
            recoiForce *= 0.2f;
        }

        OnShootServerRpc(recoiForce);//调用服务器端的函数，正在射击的函数

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, currentWeapon.range, mask))//射线判断
        {
            if (hit.collider.CompareTag(PLAYER_TAG))
            {
                ShootServerRpc(hit.collider.name, currentWeapon.damage);//击中玩家
                OnHitServerRpc(hit.point, hit.normal, HitEffectMaterial.Metal);
            }
            else
            {
                OnHitServerRpc(hit.point, hit.normal, HitEffectMaterial.Stone);
            }
        }
    }

    [ServerRpc]//使用ServerRpc的函数必须以ServerRpc为后缀
    private void ShootServerRpc(string name, int damage)//将击中目标的信息发送给服务器，在服务器上运行
    {
        Player player = GameManager.Singleton.GetPlayer(name);
        player.TakeDamage(damage);
    }
}
