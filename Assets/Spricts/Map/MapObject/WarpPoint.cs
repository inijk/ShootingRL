using UnityEngine;

public class WarpPoint : InteractableObject
{
    [Header("ワープ先の設定")]
    [SerializeField] private Vector2Int targetRoomCoord;

    protected override void OnInteract()
    {
        // ワープポイントは何度も使えるようにするか、1回切りかを選択可能
        // 今回は「使用済み」にする例
        currentState = ObjectState.Opened;

        Debug.Log($"ワープポイント作動！ 座標 {targetRoomCoord} へ移動します。(状態: {currentState})");

        // ※将来的に DungeonManager にワープ通知を送る処理を入れる
    }
}