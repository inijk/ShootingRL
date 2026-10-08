using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Combat/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("― 基本情報 ―")]
    public string weaponName = "武器名";
    [TextArea(2, 5)]
    public string description = "武器の説明文";
    public Sprite icon;

    [Header("― WP・回復パラメータ ―")]
    public float maxWP = 20f;              // 最大WP
    public float wpRecoveryRate = 2f;      // 通常回復速度（毎秒）
    public float offHandRecoveryMultiplier = 2.0f; // 非装備（裏武器）時の回復倍率

    [Header("― スキル・アビリティ枠 ―")]
    public List<SkillData> settableSkills = new List<SkillData>(); // セット可能なスキル一覧

    // エディタ上で値の変更・アセット名の変更があった際に自動呼び出しされる
    private void OnValidate()
    {
        // アセット名（ファイル名）を自動的に skillName に代入
        if (!string.IsNullOrEmpty(name))
        {
            weaponName = name;
        }
    }
}