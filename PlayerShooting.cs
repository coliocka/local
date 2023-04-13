using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour//ʹ��ServerRpc��Ҫʹ��NetworkBehaviour
{
    private const string PLAYER_TAG = "Player";//������ǩ

    private WeaponManager weaponManager;//��������Ϣ����
    private PlayerWeapon currentWeapon;//����������

    private float shootCoolDownTime = 0f;//�����ϴο�ǹ�����˶��
    private int autoShootCount = 0;//��ǰһ�������˶���ǹ

    [SerializeField]
    private LayerMask mask;

    private Camera cam;
    private PlayerController playerController;

    enum HitEffectMaterial//��Ч���
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
        shootCoolDownTime += Time.deltaTime;//��ȴʱ��

        if (!IsLocalPlayer) return;

        currentWeapon = weaponManager.GetCurrentWeapon();//��ȡ��ǰ��������Ϣ

        if (currentWeapon.shootRate <= 0)
        {
            if (Input.GetButtonDown("Fire1") && shootCoolDownTime >= currentWeapon.shootCoolDownTime)//����ģʽ,ֻ�����갴��
            {
                Shoot();
                shootCoolDownTime = 0f;//������ȴʱ��
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                autoShootCount = 0;
                InvokeRepeating(nameof(Shoot), 0f, 1f / currentWeapon.shootRate);//�����Դ���
                //��ʼʱ�䣬ʱ����
            }
            else if (Input.GetButtonUp("Fire1") || Input.GetKeyDown(KeyCode.Q))
            {
                autoShootCount = 0;
                CancelInvoke(nameof(Shoot));
            }
        }
    }

    private void OnHit(Vector3 pos, Vector3 normal, HitEffectMaterial meterial)//���е����Ч
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
    private void OnShoot(float recoiForce)//ÿ�������ص��߼���������Ч��������
    {
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();//������Ч
        weaponManager.GetCurrentAudioSource().Play();//������Ч

        if (IsLocalPlayer)//ֻ������������Ӻ�����
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
        OnShootClientRpc(recoiForce);//����ClientRpcʱ�����ظ�����ÿһ��ClientRpc
    }
    private void Shoot()
    {
        autoShootCount++;
        float recoiForce = currentWeapon.recoiForce;//��¼��ǰ������

        if (autoShootCount <= 3)
        {
            recoiForce *= 0.2f;
        }

        OnShootServerRpc(recoiForce);//���÷������˵ĺ�������������ĺ���

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, currentWeapon.range, mask))//�����ж�
        {
            if (hit.collider.CompareTag(PLAYER_TAG))
            {
                ShootServerRpc(hit.collider.name, currentWeapon.damage);//�������
                OnHitServerRpc(hit.point, hit.normal, HitEffectMaterial.Metal);
            }
            else
            {
                OnHitServerRpc(hit.point, hit.normal, HitEffectMaterial.Stone);
            }
        }
    }

    [ServerRpc]//ʹ��ServerRpc�ĺ���������ServerRpcΪ��׺
    private void ShootServerRpc(string name, int damage)//������Ŀ�����Ϣ���͸����������ڷ�����������
    {
        Player player = GameManager.Singleton.GetPlayer(name);
        player.TakeDamage(damage);
    }
}
