using UnityEngine;
using TMPro;

public class PlayerStatusUI : MonoBehaviour
{
    [Header("― 参照コンポーネント ―")]
    [SerializeField] private EntityStats playerStats;
    [SerializeField] private WeaponManager weaponManager;

    [Header("― UIコンポーネント ―")]
    [SerializeField] private TextMeshProUGUI statusText;

    private void Start()
    {
        // 参照が未設定の場合は親や同一オブジェクトから自動取得[cite: 2]
        if (playerStats == null) playerStats = GetComponentInParent<EntityStats>();
        if (weaponManager == null) weaponManager = GetComponentInParent<WeaponManager>();
    }

    private void Update()
    {
        UpdateStatusDisplay();
    }

    /// <summary>
    /// ステータス、ST、装備中の武器・WP情報を毎フレーム取得して表示更新
    /// </summary>
    public void UpdateStatusDisplay()
    {
        if (statusText == null) return;

        // 1. HP・ST 情報取得（EntityStats から参照）[cite: 2]
        string hpText = "HP: --- / ---";
        string stText = "ST: --- / ---";

        if (playerStats != null)
        {
            // HP
            if (playerStats.HP != null)
            {
                hpText = $"HP: {playerStats.HP.CurrentValue:F0} / {playerStats.HP.MaxStat.Value:F0}";
            }

            // ST (リアルタイム表示)[cite: 2]
            if (playerStats.ST != null)
            {
                stText = $"ST: {playerStats.ST.CurrentValue:F1} / {playerStats.ST.MaxStat.Value:F0}";
            }
        }

        // 2. 装備中武器 & WP 情報取得（WeaponManager から参照）
        string weaponName = "なし";
        string wpText = "WP: --- / ---";

        if (weaponManager != null && weaponManager.ActiveWeapon != null)
        {
            WeaponInstance activeWeapon = weaponManager.ActiveWeapon;

            // 武器名
            if (activeWeapon.Data != null)
            {
                weaponName = activeWeapon.Data.weaponName;
            }

            // WP (リアルタイム表示)
            if (activeWeapon.WPGauge != null)
            {
                wpText = $"WP: {activeWeapon.WPGauge.CurrentValue:F1} / {activeWeapon.WPGauge.MaxStat.Value:F0}";
            }
        }

        // 3. テキスト描画の反映
        statusText.text = $"{hpText}\n{stText}\n装備: {weaponName}\n{wpText}";
    }
}