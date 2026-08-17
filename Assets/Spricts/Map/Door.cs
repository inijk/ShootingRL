using UnityEngine;

public enum Direction { North, South, East, West }

public class Door : MonoBehaviour
{
    [Header("このドアの方向")]
    public Direction direction;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーが触れたらDungeonManagerに通知
        if (other.CompareTag("Player"))
        {
            DungeonManager.Instance.OnPlayerEnterDoor(direction);
        }
    }
}