using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [Header("部屋のプレハブ（テスト用）")]
    public GameObject roomPrefab; // Aseet/sprites/room にある BasicRoom プレハブ

    [Header("プレイヤーの参照")]
    public Transform playerTransform;

    // ダンジョンのグリッド上の部屋データ構造
    private Dictionary<Vector2Int, string> mapData = new Dictionary<Vector2Int, string>();
    private Vector2Int currentGridCoord = Vector2Int.zero;
    private GameObject currentRoomInstance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GenerateMainRoute(); // 1. 主経路マップデータの生成
        SpawnRoom(currentGridCoord); // 2. 初期部屋の生成
    }

    // 1. 仮の主経路データ作成（例: (0,0) -> (1,0) -> (1,1)）
    private void GenerateMainRoute()
    {
        mapData.Clear();
        mapData.Add(new Vector2Int(0, 0), "Start");
        mapData.Add(new Vector2Int(1, 0), "Normal");
        mapData.Add(new Vector2Int(1, 1), "Boss");

        Debug.Log("主経路マップを作成しました。");
    }

    // 部屋の動的生成
    private void SpawnRoom(Vector2Int coord)
    {
        // 既存の部屋を破棄
        if (currentRoomInstance != null)
        {
            Destroy(currentRoomInstance);
        }

        // 新しい部屋を原点(0,0,0)に生成
        currentRoomInstance = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity);

        // 未接続のドアを塞ぐ/非表示にする
        SetupDoors(coord);

        Debug.Log($"現在地: {coord} ({mapData[coord]} 部屋)");
    }

    // マップデータに沿って不要なドアを非アクティブ化
    private void SetupDoors(Vector2Int coord)
    {
        Door[] doors = currentRoomInstance.GetComponentsInChildren<Door>();

        foreach (var door in doors)
        {
            Vector2Int targetCoord = coord + GetDirectionOffset(door.direction);
            
            // 隣の座標に部屋が存在しない場合はドアを消す（壁扱い）
            if (!mapData.ContainsKey(targetCoord))
            {
                door.gameObject.SetActive(false);
            }
        }
    }

    // ドアに触れたときの処理
    public void OnPlayerEnterDoor(Direction enteredDirection)
    {
        Vector2Int nextCoord = currentGridCoord + GetDirectionOffset(enteredDirection);

        if (mapData.ContainsKey(nextCoord))
        {
            currentGridCoord = nextCoord;
            SpawnRoom(currentGridCoord);

            // プレイヤーの位置を反対側のドア付近へ移動
            RelocatePlayer(enteredDirection);
        }
    }

    // 入ってきた方向の「反対側」にプレイヤーを配置
    private void RelocatePlayer(Direction enteredDirection)
    {
        // 入ってきたドアの反対側のドアを探す（東から入ったなら西ドア）
        Direction spawnDoorDir = GetOppositeDirection(enteredDirection);
        Door[] doors = currentRoomInstance.GetComponentsInChildren<Door>();

        foreach (var door in doors)
        {
            if (door.direction == spawnDoorDir)
            {
                // ドアの方向（部屋の中心から外側へ向かうベクトル）
                Vector2Int dirVector = GetDirectionOffset(spawnDoorDir);
                Vector3 outerVector = new Vector3(dirVector.x, dirVector.y, 0f);

                // ドアの位置から「部屋の内側（outerVectorの逆方向）」へ 1.5 ユニット移動した位置に配置
                float pushInDistance = 1.5f; // プレイヤーがすぐドアに再接触しない距離（適宜調整）
                Vector3 spawnPos = door.transform.position - (outerVector * pushInDistance);

                playerTransform.position = spawnPos;
                break;
            }
        }
    }

    // 方向のベクトル変換
    private Vector2Int GetDirectionOffset(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return new Vector2Int(0, 1);
            case Direction.South: return new Vector2Int(0, -1);
            case Direction.East:  return new Vector2Int(1, 0);
            case Direction.West:  return new Vector2Int(-1, 0);
            default: return Vector2Int.zero;
        }
    }

    // 反対方向の取得
    private Direction GetOppositeDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return Direction.South;
            case Direction.South: return Direction.North;
            case Direction.East:  return Direction.West;
            case Direction.West:  return Direction.East;
            default: return Direction.North;
        }
    }
}