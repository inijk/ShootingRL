using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("― 参照設定 ―")]
    [SerializeField] private GameObject attackHitboxPrefab; // 斬撃のプレハブ
    [SerializeField] private Transform attackSpawnPoint;     // 生成位置 (Playerの子要素)
    
    [Header("― 攻撃パラメータ ―")]
    [SerializeField] private float attackCooldown = 0.3f;   // 攻撃のクールタイム
    private float lastAttackTime;
    
    private EntityStats myStats;

    private void Awake()
    {
        myStats = GetComponent<EntityStats>();
    }

    private void Update()
    {
        // Fire1 ボタン/キー（Mouse Left Click / Ctrl / Gamepad Button）が押されたか判定
        if (Mouse.current.leftButton.wasPressedThisFrame) 
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
        }
    }

    private void PerformAttack()
    {
        // マウスカーソルへの方向・角度を計算して回転させる
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 attackDir = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;

        // 扇状判定の生成 (角度を合わせる)
        Quaternion spawnRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        GameObject hitboxObj = Instantiate(attackHitboxPrefab, attackSpawnPoint.position, spawnRotation, transform);

        // 初期化処理
        if (hitboxObj.TryGetComponent<MeleeHitbox>(out var meleeHitbox))
        {
            meleeHitbox.Setup(myStats);
        }
    }
}