using UnityEngine;

/// <summary>
/// 플레이어를 부드럽게 따라가는 카메라
/// [FIX] BuildPhaseManager/PlayPhaseManager에서 명시적으로 모드 전환 호출하도록 연결
/// [FIX] GameManager 페이즈 변경 시 자동 감지 (Update 폴링)
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("타겟")]
    [SerializeField] private Transform target;

    [Header("이동 설정")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(2f, 1f, -10f);

    [Header("경계")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private float minX = -15f;
    [SerializeField] private float maxX = 15f;
    [SerializeField] private float minY = -5f;
    [SerializeField] private float maxY = 5f;

    [Header("오버뷰 (빌드 페이즈)")]
    [SerializeField] private float overviewOrthographicSize = 7f;
    [SerializeField] private float playOrthographicSize     = 5f;

    private bool isFollowing = false;
    private Vector3 overviewPosition;
    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>() ?? Camera.main;
        overviewPosition = transform.position;
        if (overviewPosition == Vector3.zero)
            overviewPosition = new Vector3(0, 0, -10f);
    }

    public void SetOverviewMode()
    {
        isFollowing = false;
        if (cam != null) cam.orthographicSize = overviewOrthographicSize;
    }

    public void SetFollowMode(Transform followTarget)
    {
        target = followTarget;
        isFollowing = true;
        if (cam != null) cam.orthographicSize = playOrthographicSize;
    }

    private void LateUpdate()
    {
        if (!isFollowing || target == null)
        {
            // 빌드 페이즈: 오버뷰 위치로 부드럽게 복귀
            transform.position = Vector3.Lerp(transform.position,
                new Vector3(0, 0, overviewPosition.z), smoothSpeed * Time.deltaTime);
            return;
        }

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
