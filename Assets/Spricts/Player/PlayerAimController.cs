using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimController : MonoBehaviour
{
    [Header("コンポーネント参照")]
    [SerializeField] private Transform cameraTarget;      // Cinemachineが追従する空のTarget
    [SerializeField] private Transform aimPointer;       // プレイヤーの周りを回る指示スプライト

    [Header("エイム＆カメラ設定")]
    [SerializeField] private float orbitRadius = 1.5f;            // 三角形の周回半径
    [SerializeField] private float cameraOffsetDistance = 3.0f;        // カメラをずらす距離
    [SerializeField] private float cameraLerpSpeed = 8.0f;        // カメラターゲットの移動補正速度
    [SerializeField] private float mouseDeadzoneRatio = 0.15f;       // マウス用: 画面中央からの不感帯領域（0.0〜0.5）
    [SerializeField] private float mouseMaxRangeRatio = 0.40f;       // マウス用: 最大カメラ移動に達する領域（0.0〜0.5）

    private PlayerInput playerInput;
    private Camera mainCamera;
    
    private Vector2 rawAimInput;                       // スティック入力値
    private Vector2 currentAimDirection = Vector2.up; // 現在の狙い方向（初期値は上）
    private float currentCameraOffsetFactor = 0f;     // カメラ移動の適用割合 (0.0 ～ 1.0)

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        UpdateAimDirectionAndCameraFactor();
        UpdatePointerTransform();
        UpdateCameraTargetPosition();
    }

    /// <summary>
    /// Input System: Look アクション（マウス座標 / 右スティック）の入力受取
    /// </summary>
    public void OnLook(InputValue value)
    {
        rawAimInput = value.Get<Vector2>();
    }

    /// <summary>
    /// 操作デバイスに応じてエイム方向とカメラシフト率を同時計算
    /// </summary>
    private void UpdateAimDirectionAndCameraFactor()
    {
        if (playerInput == null || mainCamera == null) return;

        // 現在使用中の操作スキームを確認（またはデバイス判定）
        string scheme = playerInput.currentControlScheme;
        bool isGamepad = (scheme != null && scheme.Contains("Gamepad"));

        // -----------------------------------------------------------------
        // 【1. ゲームパッド操作時】
        // -----------------------------------------------------------------
        if (isGamepad)
        {
            if (rawAimInput.sqrMagnitude > 0.05f)
            {
                currentAimDirection = rawAimInput.normalized;
                currentCameraOffsetFactor = Mathf.Clamp01(rawAimInput.magnitude);
            }
            else
            {
                // スティックを戻したらカメラのみ中心へ戻す
                currentCameraOffsetFactor = 0f;
            }
            return;
        }

        // -----------------------------------------------------------------
        // 【2. マウス操作時】
        // -----------------------------------------------------------------
        if (Mouse.current != null)
        {
            // 1. マウスの位置を取得してビューポート座標（0.0 ~ 1.0）に変換
            Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 mouseViewportPos = mainCamera.ScreenToViewportPoint(mouseScreenPos);

            // 2. プレイヤーの位置をビューポート座標に変換
            Vector3 playerViewportPos = mainCamera.WorldToViewportPoint(transform.position);

            // 3. 画面中央（自機）からの差分と距離（distance）を計算
            Vector2 diff = (Vector2)(mouseViewportPos - playerViewportPos);
            float distance = diff.magnitude; // ※画面端で約0.5

            // 方向の更新
            if (distance > 0.001f)
            {
                currentAimDirection = diff.normalized;
            }

            // 4. distance の値に応じてカメラ移動割合（0.0 ~ 1.0）を算出
            if (distance <= mouseDeadzoneRatio)
            {
                // 不感帯の内側ならカメラはズレない（中心に戻る）
                currentCameraOffsetFactor = 0f;
            }
            else
            {
                // 不感帯の外側なら mouseMaxRangeRatio に向かって 0.0 -> 1.0 へ補間
                float range = mouseMaxRangeRatio - mouseDeadzoneRatio;
                if (range > 0.0001f)
                {
                    currentCameraOffsetFactor = Mathf.Clamp01((distance - mouseDeadzoneRatio) / range);
                }
            }
        }
    }

    /// <summary>
    /// 衛星インジケーター（三角形）の位置と向きを更新
    /// </summary>
    private void UpdatePointerTransform()
    {
        if (aimPointer == null) return;

        Vector3 targetPosition = transform.position + (Vector3)(currentAimDirection * orbitRadius);
        aimPointer.position = targetPosition;

        float angle = Mathf.Atan2(currentAimDirection.y, currentAimDirection.x) * Mathf.Rad2Deg - 90f;
        aimPointer.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// Cinemachine追従用の CameraTarget を移動させる
    /// </summary>
    private void UpdateCameraTargetPosition()
    {
        if (cameraTarget == null) return;

        // 計算された移動割合 (currentCameraOffsetFactor) を乗算してターゲット位置を決定
        Vector3 targetLocalPos = (Vector3)(currentAimDirection * (cameraOffsetDistance * currentCameraOffsetFactor));

        cameraTarget.localPosition = Vector3.Lerp(
            cameraTarget.localPosition, 
            targetLocalPos, 
            Time.deltaTime * cameraLerpSpeed
        );
    }
}