using UnityEngine;

/// <summary>
/// 플레이어를 부드럽게 따라가는 카메라
/// 빌드 페이즈엔 전체 스테이지 뷰, 플레이 페이즈엔 플레이어 추적
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("타겟")]
    [SerializeField] private Transform target;

    [Header("이동 설정")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(2f, 1f, -10f);

    [Header("경계 (스테이지 안에서만 이동)")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = -5f;
    [SerializeField] private float maxY = 5f;

    private bool isFollowing = false;
    private Vector3 overviewPosition;

    private void Start()
    {
        overviewPosition = transform.position;
    }

    /// <summary>
    /// 빌드 페이즈: 전체 뷰로 복귀
    /// </summary>
    public void SetOverviewMode()
    {
        isFollowing = false;
    }

    /// <summary>
    /// 플레이 페이즈: 플레이어 추적 시작
    /// </summary>
    public void SetFollowMode(Transform followTarget)
    {
        target = followTarget;
        isFollowing = true;
    }

    private void LateUpdate()
    {
        if (!isFollowing || target == null) return;

        Vector3 desired = target.position + offset;

        if (useBounds)
        {
            desired.x = Mathf.Clamp(desired.x, minX, maxX);
            desired.y = Mathf.Clamp(desired.y, minY, maxY);
        }
        desired.z = offset.z;

        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
