using System;
using UnityEngine;

// 静的ステータス（MaxHP, MaxST, 回復率など）
[Serializable]
public class Stat
{
    [SerializeField] private float baseValue;
    private float addModifier;
    private float multModifier = 1f;

    public float Value => (baseValue + addModifier) * multModifier;

    public Stat(float defaultValue)
    {
        baseValue = defaultValue;
    }

    public void AddModifier(float amount) => addModifier += amount;
    public void RemoveModifier(float amount) => addModifier -= amount;
}

// 動的リソース（現在HP, 現在STなど）
[Serializable]
public class ResourceGauge
{
    public float CurrentValue { get; private set; }
    public Stat MaxStat { get; private set; }

    // UI更新などに使えるイベント
    public event Action<float, float> OnValueChanged; // (現在値, 最大値)

    public ResourceGauge(Stat maxStat)
    {
        MaxStat = maxStat;
        CurrentValue = MaxStat.Value;
    }

    // 初期化用
    public void Initialize()
    {
        CurrentValue = MaxStat.Value;
        OnValueChanged?.Invoke(CurrentValue, MaxStat.Value);
    }

    // 消費 / ダメージ処理
    public bool Consume(float amount)
    {
        if (CurrentValue < amount) return false; // コスト不足

        CurrentValue = Mathf.Max(0, CurrentValue - amount);
        OnValueChanged?.Invoke(CurrentValue, MaxStat.Value);
        return true;
    }

    // 回復処理
    public void Recover(float amount)
    {
        CurrentValue = Mathf.Min(MaxStat.Value, CurrentValue + amount);
        OnValueChanged?.Invoke(CurrentValue, MaxStat.Value);
    }
}