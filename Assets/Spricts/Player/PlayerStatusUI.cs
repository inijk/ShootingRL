using UnityEngine;
using TMPro; // TextMeshProを使用するために必要

public class PlayerStatusUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private EntityStats playerStats; // ステータス管理スクリプトへの参照
    [SerializeField] private TextMeshProUGUI statusText; // 表示用テキスト

    private void Update()
    {
        if (playerStats == null || statusText == null) return;

        // HP / MP / ST の現在値と最大値を文字列にして表示
        // 例: HP: 80/100  MP: 30/50  ST: 100/100
        statusText.text = $"HP: {playerStats.HP.CurrentValue:F0}/{playerStats.HP.MaxStat.Value:F0}\n" +
                          $"MP: {playerStats.FP.CurrentValue:F0}/{playerStats.FP.MaxStat.Value:F0}\n" +
                          $"ST: {playerStats.ST.CurrentValue:F0}/{playerStats.ST.MaxStat.Value:F0}";
    }
}