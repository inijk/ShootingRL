using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(WeaponManager))]
public class PlayerSkillExecutor : MonoBehaviour
{
    [Header("― Input System アクション設定 ―")]
    [Tooltip("武器切り替え（トグル）用アクション")]
    [SerializeField] private InputActionReference switchWeaponAction; // 例: Qキーやボタン

    [Tooltip("スキル1（例: ライトスラッシュ等）用アクション")]
    [SerializeField] private InputActionReference skill1Action;       // 例: 1キーや攻撃ボタン1

    [Tooltip("スキル2（例: ヘビィスラッシュ等）用アクション")]
    [SerializeField] private InputActionReference skill2Action;       // 例: 2キーや攻撃ボタン2

    [SerializeField] private Transform attackPoint; // AimPointerの子にある AttackPoint を割り当て
    private WeaponManager weaponManager;

    private void Awake()
    {
        weaponManager = GetComponent<WeaponManager>();
    }

    private void OnEnable()
    {
        // アクションの有効化
        EnableAction(switchWeaponAction);
        EnableAction(skill1Action);
        EnableAction(skill2Action);
    }

    private void OnDisable()
    {
        // アクションの無効化
        DisableAction(switchWeaponAction);
        DisableAction(skill1Action);
        DisableAction(skill2Action);
    }

    private void Update()
    {
        // 武器トグル切り替え
        if (IsPressed(switchWeaponAction))
        {
            weaponManager.ToggleWeapon();
        }

        // スキル1発動
        if (IsPressed(skill1Action))
        {
            ExecuteSkill(0);
        }

        // スキル2発動
        if (IsPressed(skill2Action))
        {
            ExecuteSkill(1);
        }
    }

    /// <summary>
    /// 指定されたインデックスのスキルを実行
    /// </summary>
    private void ExecuteSkill(int skillIndex)
    {
        WeaponInstance currentWeapon = weaponManager.ActiveWeapon;
        if (currentWeapon == null || currentWeapon.Data == null) return;

        if (skillIndex < currentWeapon.Data.settableSkills.Count)
        {
            SkillData skillToUse = currentWeapon.Data.settableSkills[skillIndex];

            if (weaponManager.TryUseSkill(skillToUse))
            {
                Vector2 aimDirection = transform.right; 
                
                // AimController があれば現在のエイム方向を取得
                if (TryGetComponent<PlayerAimController>(out var aimController))
                {
                    // aimController から方向を取得する処理など
                }

                EntityStats playerStats = GetComponent<EntityStats>();

                // 変数名を attackPoint に統一
                Transform spawnParent = attackPoint != null ? attackPoint : transform;
                skillToUse.Execute(spawnParent, aimDirection, playerStats);
            }
        }
    }

    #region Helper Methods
    private void EnableAction(InputActionReference actionRef)
    {
        if (actionRef != null && actionRef.action != null)
        {
            actionRef.action.Enable();
        }
    }

    private void DisableAction(InputActionReference actionRef)
    {
        if (actionRef != null && actionRef.action != null)
        {
            actionRef.action.Disable();
        }
    }

    private bool IsPressed(InputActionReference actionRef)
    {
        return actionRef != null && actionRef.action != null && actionRef.action.WasPressedThisFrame();
    }
    #endregion
}