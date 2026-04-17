using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 스테이지 데이터에 정의된 장애물/픽업을 씬에 스폰
/// 새 스테이지 진입 시 StageBuildFromData() 호출
/// </summary>
public class StageBuilder : MonoBehaviour
{
    public static StageBuilder Instance { get; private set; }

    [Header("프리팹 - 기본 장애물")]
    [SerializeField] private GameObject movingObstaclePrefab;
    [SerializeField] private GameObject fallingObstaclePrefab;
    [SerializeField] private GameObject windZonePrefab;
    [SerializeField] private GameObject spikePrefab;

    [Header("프리팹 - 확장 장애물")]
    [SerializeField] private GameObject bouncerPrefab;
    [SerializeField] private GameObject gravityZonePrefab;
    [SerializeField] private GameObject timedGatePrefab;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private GameObject iceZonePrefab;

    [Header("프리팹 - 기타")]
    [SerializeField] private GameObject instrumentPickupPrefab;
    [SerializeField] private GameObject groundPrefab;
    [SerializeField] private GameObject goalPrefab;

    [Header("런타임")]
    [SerializeField] private Transform levelRoot;

    private readonly List<GameObject> spawned = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void BuildFromData(StageData data)
    {
        ClearSpawned();
        if (data == null) return;

        if (levelRoot == null)
        {
            var rootObj = GameObject.Find("--- LEVEL ---") ?? new GameObject("--- LEVEL ---");
            levelRoot = rootObj.transform;
        }

        // 시작/도착 지형
        SpawnGround(data.startGroundX, data.groundY);
        SpawnGround(data.endGroundX, data.groundY);

        // 목적지
        if (goalPrefab != null)
        {
            var goal = Instantiate(goalPrefab, new Vector3(data.endGroundX, data.goalY, 0), Quaternion.identity, levelRoot);
            Track(goal);
        }

        // 픽업
        foreach (var p in data.pickups)
        {
            if (p?.instrument == null) continue;
            if (StageManager.Instance != null && StageManager.Instance.Owns(p.instrument)) continue;
            SpawnPickup(p);
        }

        // 장애물
        foreach (var o in data.obstacles)
            SpawnObstacle(o);
    }

    public void ClearSpawned()
    {
        foreach (var go in spawned)
            if (go != null) Destroy(go);
        spawned.Clear();
    }

    private void Track(GameObject go) => spawned.Add(go);

    private void SpawnGround(float x, float y)
    {
        if (groundPrefab == null) return;
        var g = Instantiate(groundPrefab, new Vector3(x, y, 0), Quaternion.identity, levelRoot);
        Track(g);
    }

    private void SpawnPickup(PickupPlacement p)
    {
        if (instrumentPickupPrefab == null) return;
        var go = Instantiate(instrumentPickupPrefab, new Vector3(p.position.x, p.position.y, 0),
            Quaternion.identity, levelRoot);
        var pickup = go.GetComponent<InstrumentPickup>();
        if (pickup != null) pickup.SetInstrument(p.instrument);
        Track(go);
    }

    private void SpawnObstacle(ObstaclePlacement o)
    {
        GameObject prefab = o.kind switch
        {
            ObstacleKind.Moving      => movingObstaclePrefab,
            ObstacleKind.Falling     => fallingObstaclePrefab,
            ObstacleKind.Wind        => windZonePrefab,
            ObstacleKind.Spike       => spikePrefab,
            ObstacleKind.Bouncer     => bouncerPrefab,
            ObstacleKind.GravityFlip => gravityZonePrefab,
            ObstacleKind.TimedGate   => timedGatePrefab,
            ObstacleKind.Laser       => laserPrefab,
            ObstacleKind.Ice         => iceZonePrefab,
            _ => null,
        };
        if (prefab == null) return;

        var go = Instantiate(prefab, new Vector3(o.position.x, o.position.y, 0),
            Quaternion.identity, levelRoot);
        go.transform.localScale = new Vector3(o.size.x, o.size.y, 1f);

        // kind 별 파라미터 적용
        switch (o.kind)
        {
            case ObstacleKind.Moving:
                var mv = go.GetComponent<MovingObstacle>();
                if (mv != null) mv.Configure(o.moveDirection, o.moveDistance, o.moveSpeed);
                break;
            case ObstacleKind.Wind:
                var wind = go.GetComponent<WindZoneObstacle>();
                if (wind != null) wind.Configure(o.windForce);
                break;
            case ObstacleKind.Bouncer:
                var bouncer = go.GetComponent<BouncerObstacle>();
                if (bouncer != null) bouncer.Configure(o.bounceForce);
                break;
            case ObstacleKind.TimedGate:
                var gate = go.GetComponent<TimedGateObstacle>();
                if (gate != null) gate.Configure(o.gateOpenTime, o.gateCloseTime);
                break;
            case ObstacleKind.Laser:
                var laser = go.GetComponent<LaserObstacle>();
                if (laser != null) laser.Configure(o.laserOnTime, o.laserOffTime);
                break;
        }

        Track(go);
    }
}
