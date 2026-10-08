using System;
using UnityEngine;

[Serializable]
public class WeaponInstance
{
    public WeaponData Data { get; private set; }
    public ResourceGauge WPGauge { get; private set; }

    public WeaponInstance(WeaponData data)
    {
        Data = data;
        if (Data != null)
        {
            // StatクラスとResourceGaugeクラス（既存設計）を活用[cite: 2]
            Stat maxWPStat = new Stat(Data.maxWP);
            WPGauge = new ResourceGauge(maxWPStat);
            WPGauge.Initialize();
        }
    }

    /// <summary>
    /// 指定した時間(deltaTime)分、WPを回復する
    /// </summary>
    public void UpdateWP(float deltaTime, bool isEquipped)
    {
        if (Data == null || WPGauge == null) return;

        // 非装備（裏武器）時は回復倍率を適用[cite: 1]
        float multiplier = isEquipped ? 1.0f : Data.offHandRecoveryMultiplier;
        float recoveryAmount = Data.wpRecoveryRate * multiplier * deltaTime;

        WPGauge.Recover(recoveryAmount); //[cite: 2]
    }

    /// <summary>
    /// スキル発動に必要なWPが足りているかチェック[cite: 1, 2]
    /// </summary>
    public bool HasEnoughWP(float cost)
    {
        if (WPGauge == null) return false;
        return WPGauge.CurrentValue >= cost; //[cite: 2]
    }

    /// <summary>
    /// WPを消費する[cite: 2]
    /// </summary>
    public bool ConsumeWP(float cost)
    {
        if (!HasEnoughWP(cost)) return false;
        return WPGauge.Consume(cost); //[cite: 2]
    }
}