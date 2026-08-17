using UnityEngine;

public enum Direction { North, South, East, West }

public class Door : MonoBehaviour
{
    [Header("このドアの方向")]
    public Direction direction;

    [Header("参照コンポーネント")]
    public SpriteRenderer doorRenderer;
    public Collider2D doorCollider;

    [Header("見た目の設定（色で表現する場合）")]
    public Color openColor = Color.white;
    public Color closedColor = Color.gray; // 暗めの色や赤系など

    [Header("スプライト画像切替の場合（任意）")]
    public Sprite openSprite;
    public Sprite closedSprite;

    private void Reset()
    {
        // コンポーネントの自動取得
        doorRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // IsTrigger = true の時（有効なドア）だけ通過イベントが発生
        if (other.CompareTag("Player"))
        {
            DungeonManager.Instance.OnPlayerEnterDoor(direction);
        }
    }

    /// <summary>
    /// ドアの有効／無効（封鎖）状態をセットする
    /// </summary>
    public void SetDoorState(bool isOpen)
    {
        if (doorCollider != null)
        {
            // 有効ならTrigger（通り抜け可）、無効なら固体コライダー（壁として衝突）
            doorCollider.isTrigger = isOpen;
        }

        if (doorRenderer != null)
        {
            // スプライト画像を切り替える場合
            if (openSprite != null && closedSprite != null)
            {
                doorRenderer.sprite = isOpen ? openSprite : closedSprite;
            }

            // 色を塗りつぶす/変更する場合
            doorRenderer.color = isOpen ? openColor : closedColor;
        }
    }
}