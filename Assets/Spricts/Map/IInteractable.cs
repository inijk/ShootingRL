public interface IInteractable
{
    // インタラクト可能かどうか（ロック中や開封済みなら false）
    bool CanInteract();

    // 実際にEキーが押された時の処理
    void Interact();
}