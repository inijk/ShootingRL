using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        //Rigidbody2Dのコンポーネントを取得
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Input System (Player Inputコンポーネント) からの入力を受け取るメソッド
    /// </summary>
    public void OnMove(InputValue value)
    {
        // WASDなどの入力値をVector2(x, y)として取得
        moveInput = value.Get<Vector2>();
        
        // 斜め移動時に移動速度が速くならないよう正規化（ベクトルの長さを最大1にする）
        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }
    }

    private void FixedUpdate()
    {
        // 物理更新タイミングでRigidbody2Dを用いて移動
        // Unity 6では linearVelocity を使用（旧 velocity）
        rb.linearVelocity = moveInput * moveSpeed;
    }
}