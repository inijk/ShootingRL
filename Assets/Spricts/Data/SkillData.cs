using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Combat/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("― 基本情報 ―")]
    public string skillName = "スキル名";
    [TextArea(2, 5)]
    public string description = "スキルの説明文";
    public Sprite icon;

    [Header("― コスト・属性 ―")]
    public float wpCost = 3f; // 消費WP

    [Header("― 戦闘パラメータ ―")]
    public float damageMultiplier = 1.0f; // 威力倍率（ステータスATKに対する乗数など）
    public GameObject hitboxPrefab;       // 生成する攻撃判定（Hitbox）のプレハブ

    // エディタ上で値の変更・アセット名の変更があった際に自動呼び出しされる
    private void OnValidate()
    {
        // アセット名（ファイル名）を自動的に skillName に代入
        if (!string.IsNullOrEmpty(name))
        {
            skillName = name;
        }
    }
}