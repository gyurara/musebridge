using UnityEngine;

/// <summary>
/// 왕복 이동하는 장애물
/// 벽, 가시 등 움직이는 방해물에 사용
/// </summary>
public class MovingObstacle : ObstacleBase
{
    [Header("이동 설정")]
    [SerializeField] private Vector3 moveDirection = Vector3.right;
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private float moveSpeed = 2f;

    private Vector3 originPosition;
    private float moveTimer = 0f;

    public void Configure(Vector2 direction, float distance, float speed)
    {
        moveDirection = direction;
        moveDistance = distance;
        moveSpeed = speed;
    }

    private void Start()
    {
        originPosition = transform.position;
    }

    private void Update()
    {
        moveTimer += Time.deltaTime * moveSpeed;
        float offset = Mathf.Sin(moveTimer) * moveDistance;
        transform.position = originPosition + moveDirection.normalized * offset;
    }
}
