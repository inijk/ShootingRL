using UnityEngine;

public enum ObjectState
{
    Unopened, // 未開封・未使用
    Opened,   // 開封済み・使用済み
    Locked    // 鍵がかかっている・ロック中（敵未倒しなど）
}

public abstract class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("オブジェクト基本設定")]
    public int objectID; // 部屋内での一意のID（0, 1, 2...）
    [SerializeField] protected ObjectState currentState = ObjectState.Unopened;
    [SerializeField] protected string objectName = "オブジェクト";

    public ObjectState CurrentState => currentState;

    // 外部から初期状態を設定するメソッド
    public virtual void Setup(bool isRoomCleared, bool isOpened)
    {
        if (isOpened)
        {
            currentState = ObjectState.Opened;
        }
        else if (!isRoomCleared)
        {
            // 敵が全滅していない場合はロック状態にする
            currentState = ObjectState.Locked;
        }
        else
        {
            currentState = ObjectState.Unopened;
        }
        
        UpdateAppearance();
    }

    // 部屋がクリアされた時に外部（DungeonManager等）から呼ばれる
    public virtual void OnRoomCleared()
    {
        // まだ開けられていないロック中オブジェクトのロックを解除する
        if (currentState == ObjectState.Locked)
        {
            currentState = ObjectState.Unopened;
            Debug.Log($"[{objectName}] のロックが解除された！");
            UpdateAppearance();
        }
    }

    public virtual bool CanInteract()
    {
        // 未開封(Unopened)の時だけインタラクト可能
        return currentState == ObjectState.Unopened;
    }

    public virtual void Interact()
    {
        if (currentState == ObjectState.Locked)
        {
            Debug.Log($"[{objectName}] はロックされている！（部屋の敵を全滅させよう）");
            return;
        }

        if (currentState == ObjectState.Opened)
        {
            Debug.Log($"[{objectName}] は既に調査済みだ。");
            return;
        }

        OnInteract();
    }

    protected abstract void OnInteract();

    // 見た目の更新（必要に応じてオーバーライドして色を変える等）
    protected virtual void UpdateAppearance() { }
}