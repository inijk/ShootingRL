using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public enum WeaponSlot
    {
        Main,
        Sub,
        Starter
    }

    [Header("― 初期装備設定 ―")]
    [SerializeField] private WeaponData starterWeaponData;
    [SerializeField] private WeaponData mainWeaponData;
    [SerializeField] private WeaponData subWeaponData;

    // スロットごとの武器インスタンスデータ
    private Dictionary<WeaponSlot, WeaponInstance> weaponSlots = new Dictionary<WeaponSlot, WeaponInstance>();

    // 現在装備中のスロット
    public WeaponSlot CurrentSlot { get; private set; } = WeaponSlot.Main;

    // 現在装備中の武器インスタンス
    public WeaponInstance ActiveWeapon => weaponSlots.ContainsKey(CurrentSlot) ? weaponSlots[CurrentSlot] : null;

    // イベント通知（UI更新やエフェクト用）
    public event Action<WeaponSlot, WeaponInstance> OnWeaponSwitched;
    public event Action<WeaponSlot, float, float> OnWPChanged;

    private void Awake()
    {
        InitializeWeapons();
    }

    /// <summary>
    /// 各スロットの武器初期化
    /// </summary>
    public void InitializeWeapons()
    {
        weaponSlots.Clear();

        if (starterWeaponData != null) SetWeapon(WeaponSlot.Starter, starterWeaponData);
        if (mainWeaponData != null) SetWeapon(WeaponSlot.Main, mainWeaponData);
        if (subWeaponData != null) SetWeapon(WeaponSlot.Sub, subWeaponData);

        // 各武器のWP変更イベントを購読・リレー
        foreach (var pair in weaponSlots)
        {
            // ★重要: ループ変数のキャプチャによるバグを防ぐためローカル変数にコピー
            WeaponSlot targetSlot = pair.Key; 
            WeaponInstance weapon = pair.Value;

            if (weapon != null && weapon.WPGauge != null)
            {
                weapon.WPGauge.OnValueChanged += (current, max) =>
                {
                    OnWPChanged?.Invoke(targetSlot, current, max);
                };
            }
        }

        // 初期選択はMain（Mainが無ければStarter）
        CurrentSlot = weaponSlots.ContainsKey(WeaponSlot.Main) ? WeaponSlot.Main : WeaponSlot.Starter;
        
        // 初期状態の通知
        OnWeaponSwitched?.Invoke(CurrentSlot, ActiveWeapon);
    }

    private void Update()
    {
        // 1. 全武器のWP回復処理 (毎フレーム)[cite: 1]
        foreach (var pair in weaponSlots)
        {
            WeaponSlot slot = pair.Key;
            WeaponInstance weapon = pair.Value;

            if (weapon != null)
            {
                bool isEquipped = (slot == CurrentSlot);
                weapon.UpdateWP(Time.deltaTime, isEquipped); // 非装備時は倍率回復[cite: 1]
            }
        }
    }

    /// <summary>
    /// 武器の切り替え（トグル形式）
    /// 例: Main -> Sub -> Starter -> Main
    /// </summary>
    public void ToggleWeapon()
    {
        WeaponSlot nextSlot = CurrentSlot;

        switch (CurrentSlot)
        {
            case WeaponSlot.Main:
                nextSlot = weaponSlots.ContainsKey(WeaponSlot.Sub) ? WeaponSlot.Sub : WeaponSlot.Starter;
                break;
            case WeaponSlot.Sub:
                nextSlot = WeaponSlot.Starter;
                break;
            case WeaponSlot.Starter:
                nextSlot = weaponSlots.ContainsKey(WeaponSlot.Main) ? WeaponSlot.Main : WeaponSlot.Sub;
                break;
        }

        SwitchToSlot(nextSlot);
    }

    /// <summary>
    /// 指定スロットへ直接切り替え
    /// </summary>
    public void SwitchToSlot(WeaponSlot slot)
    {
        if (!weaponSlots.ContainsKey(slot) || weaponSlots[slot] == null) return;
        if (CurrentSlot == slot) return;

        CurrentSlot = slot;
        OnWeaponSwitched?.Invoke(CurrentSlot, ActiveWeapon);
    }

    /// <summary>
    /// スキル発動時のチェック＆WP消費ロジック[cite: 1, 2]
    /// </summary>
    /// <param name="skill">発動しようとしているスキル</param>
    /// <returns>発動成功ならtrue、WP不足などで失敗ならfalse</returns>
    public bool TryUseSkill(SkillData skill)
    {
        if (skill == null || ActiveWeapon == null) return false;

        // WP不足チェック[cite: 1, 2]
        if (!ActiveWeapon.HasEnoughWP(skill.wpCost))
        {
            // WP不足時の警告通知（効果音鳴動やUI点滅など）を入れる場所
            Debug.LogWarning($"[WP不足] {skill.skillName} の必要WP: {skill.wpCost} / 現在WP: {ActiveWeapon.WPGauge.CurrentValue}");
            return false;
        }

        // WPを消費して実行[cite: 2]
        return ActiveWeapon.ConsumeWP(skill.wpCost);
    }

    /// <summary>
    /// 新しい武器をセット（入手・変更時）
    /// </summary>
    public void SetWeapon(WeaponSlot slot, WeaponData data)
    {

        weaponSlots[slot] = new WeaponInstance(data);
    }
}