using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public enum WeaponSlot { Main, Sub, Starter }

    [Header("― 初期装備設定 ―")]
    [SerializeField] private WeaponData starterWeaponData;
    [SerializeField] private WeaponData mainWeaponData;
    [SerializeField] private WeaponData subWeaponData;

    private Dictionary<WeaponSlot, WeaponInstance> weaponSlots = new Dictionary<WeaponSlot, WeaponInstance>();
    
    public WeaponSlot CurrentSlot { get; private set; } = WeaponSlot.Main;
    public WeaponInstance ActiveWeapon => weaponSlots.ContainsKey(CurrentSlot) ? weaponSlots[CurrentSlot] : null;

    // --- イベント定義 ---
    // 武器切り替えイベント (新しい武器インスタンスを渡す)
    public event Action<WeaponInstance> OnWeaponSwitched;
    // 現在装備中武器のWP変動イベント (現在値, 最大値)
    public event Action<float, float> OnActiveWeaponWPChanged;

    private void Awake()
    {
        InitializeWeapons();
    }

    public void InitializeWeapons()
    {
        // 既存の登録を解除
        UnsubscribeWPEvents();

        weaponSlots.Clear();

        if (starterWeaponData != null) SetWeapon(WeaponSlot.Starter, starterWeaponData);
        if (mainWeaponData != null) SetWeapon(WeaponSlot.Main, mainWeaponData);
        if (subWeaponData != null) SetWeapon(WeaponSlot.Sub, subWeaponData);

        CurrentSlot = weaponSlots.ContainsKey(WeaponSlot.Main) ? WeaponSlot.Main : WeaponSlot.Starter;

        // WP変更イベントの購読登録
        SubscribeWPEvents();

        // 初期通知
        OnWeaponSwitched?.Invoke(ActiveWeapon);
    }

    private void SubscribeWPEvents()
    {
        foreach (var pair in weaponSlots)
        {
            WeaponSlot slot = pair.Key;
            WeaponInstance weapon = pair.Value;

            if (weapon != null && weapon.WPGauge != null)
            {
                // クロージャ問題対策のためスロットをローカル変数に固定
                WeaponSlot targetSlot = slot;
                weapon.WPGauge.OnValueChanged += (current, max) =>
                {
                    // 変動した武器が「現在装備中」の武器である場合のみ、外部（UI等）に通知
                    if (targetSlot == CurrentSlot)
                    {
                        OnActiveWeaponWPChanged?.Invoke(current, max);
                    }
                };
            }
        }
    }

    private void UnsubscribeWPEvents()
    {
        // クリーンアップ処理
        foreach (var weapon in weaponSlots.Values)
        {
            if (weapon != null && weapon.WPGauge != null)
            {
                weapon.WPGauge.OnValueChanged -= (current, max) => { };
            }
        }
    }

    private void Update()
    {
        // WP自動回復[cite: 3]
        foreach (var pair in weaponSlots)
        {
            bool isEquipped = (pair.Key == CurrentSlot);
            pair.Value?.UpdateWP(Time.deltaTime, isEquipped);
        }
    }

    public void SwitchToSlot(WeaponSlot slot)
    {
        if (!weaponSlots.ContainsKey(slot) || weaponSlots[slot] == null) return;
        if (CurrentSlot == slot) return;

        CurrentSlot = slot;

        // 武器が切り替わったことを通知
        OnWeaponSwitched?.Invoke(ActiveWeapon);

        // 切り替え時点でのWP初期値を通知
        if (ActiveWeapon != null && ActiveWeapon.WPGauge != null)
        {
            OnActiveWeaponWPChanged?.Invoke(ActiveWeapon.WPGauge.CurrentValue, ActiveWeapon.WPGauge.MaxStat.Value);
        }
    }

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

    public bool TryUseSkill(SkillData skill)
    {
        if (skill == null || ActiveWeapon == null) return false;
        return ActiveWeapon.ConsumeWP(skill.wpCost);
    }

    public void SetWeapon(WeaponSlot slot, WeaponData data)
    {
        weaponSlots[slot] = new WeaponInstance(data);
    }
}