using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("― 静的ステータス (Base Values) ―")]
    [SerializeField] private float baseMaxHP = 100f;
    [SerializeField] private float baseMaxMP = 50f;
    [SerializeField] private float baseMaxST = 100f;
    [SerializeField] private float baseSTRecoveryRate = 15f; // 毎秒の回復量

    // 静的ステータス
    public Stat MaxHP { get; private set; }
    public Stat MaxMP { get; private set; }
    public Stat MaxST { get; private set; }
    public Stat STRecoveryRate { get; private set; }

    // 動的リソース
    public ResourceGauge Health { get; private set; }
    public ResourceGauge Mana { get; private set; }
    public ResourceGauge Stamina { get; private set; }

    private void Awake()
    {
        // 1. 静的ステータスの初期化
        MaxHP = new Stat(baseMaxHP);
        MaxMP = new Stat(baseMaxMP);
        MaxST = new Stat(baseMaxST);
        STRecoveryRate = new Stat(baseSTRecoveryRate);

        // 2. 動的リソースの初期化
        Health = new ResourceGauge(MaxHP);
        Mana = new ResourceGauge(MaxMP);
        Stamina = new ResourceGauge(MaxST);

        Health.Initialize();
        Mana.Initialize();
        Stamina.Initialize();
    }

    private void Update()
    {
        // スタミナの自動回復処理
        if (Stamina.CurrentValue < MaxST.Value)
        {
            Stamina.Recover(STRecoveryRate.Value * Time.deltaTime);
        }
    }
}