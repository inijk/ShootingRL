using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerMovement : MonoBehaviour
{
    [Header("通常移動設定")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Blink（瞬間回避）設定")]
    [SerializeField] private float blinkSpeed = 18f;          // Blink中の速度
    [SerializeField] private float blinkDuration = 0.15f;       // Blink本体（無敵等）の持続時間
    [SerializeField] private float blinkInterval = 0.10f;      // Blink後のインターバル（硬直/受付時間）
    [SerializeField] private float blinkStaminaCost = 25f;     // Blink発動時の消費スタミナ

    [Header("長押しダッシュ設定")]
    [SerializeField] private float dashSpeedMultiplier = 1.4f; // 通常速度に対するダッシュ倍率
    [SerializeField] private float dashStaminaCostPerSec = 10f; // ダッシュ中の毎秒消費スタミナ

    private Rigidbody2D rb;
    private PlayerStats playerStats;

    private Vector2 moveInput;
    private Vector2 blinkDirection;

    // フラグ・タイマー管理
    [SerializeField] private bool isDashButtonPressed;   // ダッシュボタンが現在押されているか
    [SerializeField] private bool isBlinking;            // Blink（高速移動）中か
    [SerializeField] private bool isInBlinkInterval;     // Blink後のインターバル中か
    [SerializeField] private bool isContinuousDashing;   // 長押しダッシュ状態か

    private float blinkTimer;
    private float intervalTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerStats = GetComponent<PlayerStats>();
    }

    /// <summary>
    /// WASD/スティック移動入力
    /// </summary>
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }
    }

    /// <summary>
    /// Dash / Blink ボタン入力（Input System）
    /// </summary>
    public void OnDash_Blink(InputValue value)
    {
        // 現在のボタンの押しっぱなし状態を保持
        isDashButtonPressed = value.isPressed;

        // ボタンが押された瞬間 ＆ アクション可能な状態（通常時）であればBlink発動
        if (isDashButtonPressed && !isBlinking && !isInBlinkInterval && !isContinuousDashing)
        {
            TryPerformBlink();
        }

        // ボタンが離されたらダッシュ状態を即座に解除
        if (!isDashButtonPressed)
        {
            isContinuousDashing = false;
        }
    }

    private void TryPerformBlink()
    {
        // スタミナ消費チェック
        if (playerStats != null && playerStats.Stamina.Consume(blinkStaminaCost))
        {
            isBlinking = true;
            isInBlinkInterval = false;
            isContinuousDashing = false;

            blinkTimer = blinkDuration;
            
            // 8方向補正したベクトルを取得（入力がなければ自機正面）
            blinkDirection = Get8WayDirection(moveInput);
        }
        else
        {
            Debug.Log("スタミナが足りません！");
        }
    }

    private void FixedUpdate()
    {
        // 1. Blink（高速移動）中の処理
        if (isBlinking)
        {
            rb.linearVelocity = blinkDirection * blinkSpeed;

            blinkTimer -= Time.fixedDeltaTime;
            if (blinkTimer <= 0f)
            {
                // Blink終了 ➔ インターバル期間へ移行
                isBlinking = false;
                isInBlinkInterval = true;
                intervalTimer = blinkInterval;
            }
            return;
        }

        // 2. Blink後のインターバル中の処理
        if (isInBlinkInterval)
        {
            // インターバル中も慣性や通常移動速度に抑える（ここでは一瞬減速）
            rb.linearVelocity = moveInput * moveSpeed;

            intervalTimer -= Time.fixedDeltaTime;

            // インターバル時間内であっても、ボタンが押されていればダッシュへ移行
            if (isDashButtonPressed)
            {
                isInBlinkInterval = false;
                isContinuousDashing = true;
            }
            else if (intervalTimer <= 0f)
            {
                // インターバル終了（ボタンが押されていなければ通常状態へ戻る）
                isInBlinkInterval = false;
            }
            return;
        }

        // 3. 長押しダッシュ中の処理
        if (isContinuousDashing)
        {
            // ダッシュ中のスタミナ消費
            if (playerStats != null && playerStats.Stamina.Consume(dashStaminaCostPerSec * Time.fixedDeltaTime))
            {
                rb.linearVelocity = moveInput * (moveSpeed * dashSpeedMultiplier);
            }
            else
            {
                // スタミナ切れになったらダッシュを解除して通常移動へ
                isContinuousDashing = false;
                rb.linearVelocity = moveInput * moveSpeed;
            }
            return;
        }

        // 4. 通常移動
        rb.linearVelocity = moveInput * moveSpeed;
    }

    /// <summary>
    /// 入力ベクトルを8方向にスナップ（補正）するメソッド
    /// </summary>
    private Vector2 Get8WayDirection(Vector2 input)
    {
        if (input == Vector2.zero)
        {
            return transform.up;
        }

        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        float snappedAngle = Mathf.Round(angle / 45f) * 45f * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(snappedAngle), Mathf.Sin(snappedAngle)).normalized;
    }
}