using UnityEngine;

public class Chest : InteractableObject
{
    [Header("宝箱の設定")]
    [SerializeField] private string itemInside = "ハンドガンの弾";

    protected override void OnInteract()
    {
        currentState = ObjectState.Opened;
        Debug.Log($"宝箱を開けた！ 中から [{itemInside}] を入手した！");

        // DungeonManagerに開封状態を保存
        DungeonManager.Instance.RecordChestOpened(objectID);

        UpdateAppearance();
    }

    protected override void UpdateAppearance()
    {
        // ログ出力や、見た目のスプライト変更など
        if (currentState == ObjectState.Locked)
        {
            // ロック中の見た目（例: 暗くする、鍵アイコンを出すなど）
        }
        else if (currentState == ObjectState.Opened)
        {
            // 開封済みの見た目（例: 箱が開いたスプライトにする）
        }
    }
}