using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimController : MonoBehaviour
{
    [Header("コンポーネント参照")]
    [SerializeField] private Transform cameraTarget;      // Cinemachineが追従する空のTarget
    [SerializeField] private Transform aimPointer;       // プレイヤーの周りを回る三角形スプライト

    [Header("エイム設定")]
    [SerializeField] private float orbitRadius = 1.5f;     // 三角形の周回半径
    [SerializeField] private float cameraOffsetDistance = 3.0f; // カメラをずらす距離
    [SerializeField] private float cameraLerpSpeed = 8.0f; // カメラターゲットの移動補正速度
    [SerializeField] private float aimSpeedMultiplier = 0.5f; // エイム中のプレイヤー移動速度倍率

    private PlayerInput playerInput;
    private Camera mainCamera;
    
    private Vector2 rawAimInput;       // スティック入力値
    private Vector2 currentAimDirection = Vector2.up; // 現在の狙い方向（初期値は上）
    private bool isAiming;             // エイムボタンを押しているか

    public bool IsAiming => isAiming;
    public float AimSpeedMultiplier => aimSpeedMultiplier;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        mainCamera = Camera.main;
        Debug.Log(new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f));
    }

    private void Update()
    {
        UpdateAimDirection();
        UpdatePointerTransform();
        UpdateCameraTargetPosition();
    }

    /// <summary>
    /// Input System: Aim アクション（右クリック / L2トリガー）の入力受取
    /// </summary>
    public void OnAim(InputValue value)
    {
        isAiming = value.isPressed;
    }

    /// <summary>
    /// Input System: Look アクション（マウス座標 / 右スティック）の入力受取
    /// </summary>
    public void OnLook(InputValue value)
    {
        rawAimInput = value.Get<Vector2>();
    }

    /// <summary>
    /// 操作デバイス（マウス / ガンプ）に応じてエイム方向（ベクトル）を統一計算
    /// </summary>
    private void UpdateAimDirection()
    {
        // 1. playerInput 自体の null チェック
        if (playerInput == null) return;

        // 2. playerInput.currentControlScheme の null チェックを含めた安全な判定
        string scheme = playerInput.currentControlScheme;
        bool isMouseControl = string.IsNullOrEmpty(scheme) || scheme.Contains("Keyboard") || scheme.Contains("Mouse");

        // マウス操作の場合
        if (isMouseControl && Mouse.current != null)
        {
            // 1. マウスの位置をビューポート座標（0.0 ~ 1.0）に変換
            Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 mouseViewportPos = mainCamera.ScreenToViewportPoint(mouseScreenPos);

            // 2. プレイヤーの位置もビューポート座標に変換
            Vector3 playerViewportPos = mainCamera.WorldToViewportPoint(transform.position);

            // 3. ビューポート空間での方向ベクトルを算出
            Vector2 direction = (Vector2)(mouseViewportPos - playerViewportPos);

            if (direction.sqrMagnitude > 0.0001f)
            {
                currentAimDirection = direction.normalized;
            }
        }
        // ゲームパッド（右スティック）操作の場合
        else
        {
            if (rawAimInput.sqrMagnitude > 0.1f) // デッドゾーン判定
            {
                currentAimDirection = rawAimInput.normalized;
            }
        }
    }

    /// <summary>
    /// 衛星インジケーター（三角形）の位置と向きを更新
    /// </summary>
    private void UpdatePointerTransform()
    {
        if (aimPointer == null) return;

        // 1. プレイヤー中心からエイム方向へ orbitRadius だけ離した位置に配置
        Vector3 targetPosition = transform.position + (Vector3)(currentAimDirection * orbitRadius);
        aimPointer.position = targetPosition;

        // 2. 三角形が常にエイム方向を向くようにZ軸回転（スプライトの上が正面の場合 -90度）
        float angle = Mathf.Atan2(currentAimDirection.y, currentAimDirection.x) * Mathf.Rad2Deg - 90f;
        aimPointer.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// Cinemachine追従用の CameraTarget をエイム方向へ移動させる
    /// </summary>
    private void UpdateCameraTargetPosition()
    {
        if (cameraTarget == null) return;

        // エイム中のみカメラターゲットをエイム方向へずらす
        Vector3 targetLocalPos = isAiming ? (Vector3)(currentAimDirection * cameraOffsetDistance) : Vector3.zero;

        // Smooth (Lerp) で滑らかに移動
        cameraTarget.localPosition = Vector3.Lerp(
            cameraTarget.localPosition, 
            targetLocalPos, 
            Time.deltaTime * cameraLerpSpeed
        );
    }
}