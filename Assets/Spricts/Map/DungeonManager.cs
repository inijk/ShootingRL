using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [Header("参照")]
    public GameObject roomPrefab;
    public Transform playerTransform;

    // マップデータ（座標ごとのRoomDataを保持）
    private Dictionary<Vector2Int, RoomData> mapData = new Dictionary<Vector2Int, RoomData>();
    private Vector2Int currentGridCoord = Vector2Int.zero;
    private GameObject currentRoomInstance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GenerateMainRoute();
        SpawnRoom(currentGridCoord);
    }

    private void GenerateMainRoute()
    {
        mapData.Clear();
        // スタート部屋（最初からクリア扱い）
        mapData.Add(new Vector2Int(0, 0), new RoomData { roomType = "Start", isCleared = true });
        // 通常部屋（未クリア）
        mapData.Add(new Vector2Int(1, 0), new RoomData { roomType = "Normal", isCleared = false });
        mapData.Add(new Vector2Int(1, 1), new RoomData { roomType = "Boss", isCleared = false });
    }

    private void SpawnRoom(Vector2Int coord)
    {
        if (currentRoomInstance != null) Destroy(currentRoomInstance);

        currentRoomInstance = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity);
        SetupDoors(coord);

        // --- 部屋内オブジェクトの状態復元 ---
        RoomData currentRoomData = mapData[coord];
        InteractableObject[] interactables = currentRoomInstance.GetComponentsInChildren<InteractableObject>();

        foreach (var obj in interactables)
        {
            bool isOpened = currentRoomData.openedChestIDs.Contains(obj.objectID);
            // 部屋がクリア済みか、開封済みかを渡して初期化
            obj.Setup(currentRoomData.isCleared, isOpened);
        }

        // ※スタート部屋など元から敵がいない部屋の場合のチェック
        CheckRoomClearCondition();
    }

    // 部屋内の敵が全滅した時に呼び出すメソッド
    public void OnEnemyDefeated()
    {
        // 部屋の中に残っている敵（"Enemy"タグなど）をチェック
        GameObject[] remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        // 最後の1体を倒した瞬間（Destroyされる前なので1以下で判定、あるいは敵側で数値を管理）
        if (remainingEnemies.Length <= 1) 
        {
            SetRoomCleared();
        }
    }

    // 部屋クリア処理
    public void SetRoomCleared()
    {
        RoomData data = mapData[currentGridCoord];
        if (!data.isCleared)
        {
            data.isCleared = true;
            Debug.Log($"部屋 {currentGridCoord} の敵を全滅させた！オブジェクトのロックが解除されます。");

            // 現在の部屋にある全オブジェクトのロックを解除
            InteractableObject[] interactables = currentRoomInstance.GetComponentsInChildren<InteractableObject>();
            foreach (var obj in interactables)
            {
                obj.OnRoomCleared();
            }
        }
    }

    // 宝箱が開けられたことを記録
    public void RecordChestOpened(int objectID)
    {
        RoomData data = mapData[currentGridCoord];
        if (!data.openedChestIDs.Contains(objectID))
        {
            data.openedChestIDs.Add(objectID);
        }
    }

    private void CheckRoomClearCondition()
    {
        // 敵タグのオブジェクトが無ければ自動的にクリア扱いにする
        if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            SetRoomCleared();
        }
    }
    // マップデータに沿って不要なドアを非アクティブ化
    // マップデータに沿ってドアの状態（開・閉）を更新
    private void SetupDoors(Vector2Int coord)
    {
        Door[] doors = currentRoomInstance.GetComponentsInChildren<Door>();

        foreach (var door in doors)
        {
            Vector2Int targetCoord = coord + GetDirectionOffset(door.direction);
            
            // 隣の座標に部屋が存在すれば「開く」、存在しなければ「閉じる（塗りつぶし＋壁化）」
            bool isOpen = mapData.ContainsKey(targetCoord);
            
            door.SetDoorState(isOpen);
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