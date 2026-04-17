using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스테이지 데이터 ScriptableObject
/// 각 스테이지의 구성 정보 + 기믹/픽업 배치 정의
/// </summary>
[CreateAssetMenu(fileName = "NewStage", menuName = "BridgeRhythm/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("스테이지 정보")]
    public int stageIndex;
    public string stageName;
    [TextArea] public string description;

    [Header("판정선 설정")]
    public float rhythmLineSpeed = 3f;
    public float stageWidth = 20f;
    public int maxBridgePieces = 30;

    [Header("효율 점수 기준")]
    public int perfectThreshold = 5;
    public int goodThreshold = 12;

    [Header("지형")]
    public float startGroundX = -11f;
    public float endGroundX = 11f;
    public float groundY = -2f;
    public float goalY = 0.5f;

    [Header("스테이지 물리")]
    public float gravityScale = 1f;

    [Header("픽업 (먹으면 영구 보유)")]
    public List<PickupPlacement> pickups = new();

    [Header("장애물")]
    public List<ObstaclePlacement> obstacles = new();
}

[System.Serializable]
public class PickupPlacement
{
    public InstrumentData instrument;
    public Vector2 position;
}

public enum ObstacleKind
{
    Moving,       // 왕복 이동
    Falling,      // 밟으면 떨어지는 발판
    Wind,         // 바람 영역 (비치명)
    Spike,        // 고정 가시 (치명)
    Bouncer,      // 트램폴린 (플레이어를 위로 튕김)
    GravityFlip,  // 중력 반전 영역
    TimedGate,    // 주기적으로 열리고 닫히는 문
    Laser,        // 주기적 레이저 빔 (치명)
    Ice,          // 미끄러운 바닥 영역
}

[System.Serializable]
public class ObstaclePlacement
{
    public ObstacleKind kind;
    public Vector2 position;
    public Vector2 size = Vector2.one;
    public Vector2 moveDirection = Vector2.right;
    public float moveDistance = 3f;
    public float moveSpeed = 2f;
    public Vector2 windForce = new(3f, 0f);

    // Bouncer
    public float bounceForce = 15f;
    // TimedGate
    public float gateOpenTime = 2f;
    public float gateCloseTime = 2f;
    // Laser
    public float laserOnTime = 1.5f;
    public float laserOffTime = 2f;
    public float laserLength = 5f;
}
