using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("ダッシュ/ブリンク設定")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashStaminaCost = 25f;

    private Rigidbody2D rb;
    private PlayerStats playerStats;    

    private Vector2 moveInput;
    private Vector2 dashDirection;
    
    private bool isDashing;
    private float dashTimer;

    private void Awake()
    {
        // Rigidbody2Dのコンポーネントを取得
        rb = GetComponent<Rigidbody2D>();
        playerStats = GetComponent<PlayerStats>();
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

    public void OnDash_Blink(InputValue value)
    {
        // ボタンが押された瞬間、かつ現在ダッシュ中でない場合
        if (value.isPressed && !isDashing)
        {
            TryPerformDash();
        }
    }
    
    private void TryPerformDash()
    {
        // 1. スタミナを消費できるかチェック
        if (playerStats.Stamina.Consume(dashStaminaCost))
        {
            // 2. ダッシュ開始処理
            isDashing = true;
            dashTimer = dashDuration;

            // 入力方向があればその方向へ、入力がなければ現在向いている方向へ
            dashDirection = moveInput != Vector2.zero ? moveInput : transform.up;
        }
        else
        {
            Debug.Log("スタミナが足りません！");
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            // ダッシュ中の移動
            rb.linearVelocity = dashDirection * dashSpeed;

            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
        else
        {
            // 通常移動
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }
}