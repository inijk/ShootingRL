using System.Collections.Generic;

[System.Serializable]
public class RoomData
{
    public string roomType;          // 部屋の種類（Start, Normal, Boss など）
    public bool isCleared = false;   // 敵全滅フラグ（初期値は未クリア）
    public List<int> openedChestIDs = new List<int>(); // 開封済み宝箱のIDリスト
}