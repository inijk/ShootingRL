using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    [SerializeField] private float lifeTime = 0.2f; // 斬撃の持続時間
    private EntityStats attackerStats;

    public void Setup(EntityStats stats)
    {
        attackerStats = stats;
        Destroy(gameObject, lifeTime); // 指定時間後に消滅
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 攻撃を受けた対象から EntityStats を取得
        if (other.TryGetComponent<EntityStats>(out var targetStats))
        {
            // 簡易ダメージ計算 (自分のATK - 相手のDEF)
            float attackerAtk = attackerStats != null ? attackerStats.Attack.Value : 10f;
            float targetDef = targetStats.Defense.Value;
            float finalDamage = Mathf.Max(1f, attackerAtk - targetDef);

            // HP減算
            targetStats.HP.Consume(finalDamage);
            Debug.Log($"{other.name} に {finalDamage} ダメージを与えた！");
        }
    }
}