using UnityEngine;
using UnityEngine.InputSystem; // New Input System使用の場合

public class PlayerInteractor : MonoBehaviour
{
    private IInteractable currentTarget;

    private void OnInteract(InputValue value)
    {
        // ボタンが押された瞬間（isPressed）かつ対象が存在するとき
        if (value.isPressed && currentTarget != null)
        {
            currentTarget.Interact();
        }
    }

    // コライダー範囲に入ったオブジェクトを記録
    private void OnTriggerEnter2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentTarget = interactable;
            Debug.Log($"[E] キーで調べる: {other.name}");
        }
    }

    // 範囲外に出たら解除
    private void OnTriggerExit2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null && interactable == currentTarget)
        {
            currentTarget = null;
        }
    }
}