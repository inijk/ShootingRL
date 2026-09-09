using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("敵プレハブリスト")]
    [SerializeField] private List<GameObject> enemyPrefabs; // 出現させたい敵のプレハブ

    private List<EnemySpawner> spawners = new List<EnemySpawner>();

    private void Awake()
    {
        // 部屋の中にある全 EnemySpawner を自動取得
        spawners.AddRange(GetComponentsInChildren<EnemySpawner>());
    }

    // 第1ウェーブ（事前出現）の敵を生成するメソッド
    public void SpawnInitialEnemies(bool isCleared, RoomType roomType)
    {
        if (isCleared) return; // すでにクリア済みなら出さない[cite: 3]

        // 「Enemy」か「Boss」タグが含まれていない部屋なら敵を出さない
        bool hasEnemy = (roomType & (RoomType.Enemy | RoomType.Boss)) != 0;
        if (!hasEnemy) return;

        // --- 敵のスポーン処理を行う ---
        foreach (var spawner in spawners)
        {
            if (spawner.isInitialWave)
            {
                GameObject selectedPrefab = GetRandomEnemyPrefab();
                if (selectedPrefab != null)
                {
                    spawner.Spawn(selectedPrefab);
                }
            }
        }
    }

    // 敵プレハブをランダムに1つ選ぶ
    private GameObject GetRandomEnemyPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0) return null;
        int randomIndex = Random.Range(0, enemyPrefabs.Count);
        return enemyPrefabs[randomIndex];
    }
}