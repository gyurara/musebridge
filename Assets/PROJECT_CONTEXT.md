# Bridge Melody — 프로젝트 작업 현황

> 이 파일은 Claude가 이전 대화 내용을 빠르게 파악하기 위한 작업 기록입니다.
> 마지막 업데이트: 2026-04-17 (v2 — 악기 21종, 스테이지 15개, 장애물 9종, 다리형태 7종)

---

## 게임 개요

| 항목 | 내용 |
|------|------|
| 장르 | 퍼즐 플랫포머 |
| 유니티 버전 | 2022.3.52f1 |
| 레퍼런스 | Poly Bridge + 리듬게임 조합 |

### 핵심 메커니즘
- **빌드 페이즈**: 왼→오로 이동하는 판정선(리듬게임 방식)에 맞춰 키를 눌러 다리 생성
- **악기 21종**: Piano, Guitar, Drum, Flute, Violin, Bass, Xylophone, Bell, Trumpet, Saxophone, Harp, Organ, Synth, Marimba, Clarinet, Harmonica, Banjo, Cello, Tuba, Accordion, Kalimba
- **다리 형태 7종**: Straight, Curved, Zigzag, Stepped, Bouncy, Slippery, Wide
- **장애물 9종**: Moving, Falling, Wind, Spike, Bouncer, GravityFlip, TimedGate, Laser, Ice
- **프로시저럴 오디오 20파형**: Sine, Square, Sawtooth, Triangle, Pluck, Noise, Bell, Breath, Organ, Brass, StringPad, Marimba, WoodBlock, Whistle, Sitar, Kalimba, SynthPad, Accordion, Harmonica, Pizzicato
- **음표 녹음**: 다리 생성 시 나는 소리를 순서대로 기록
- **플레이 페이즈**: 직접 캐릭터 조작해서 만든 다리를 건넘 + 악기 픽업
- **음악 감상**: 스테이지 클리어 후 자신이 만든 음악을 재생 + 음악 갤러리
- **효율 점수**: 악기 적게 쓸수록 점수 높음 → 15스테이지 클리어 후 3가지 엔딩 분기

---

## 씬 구성

| 씬 이름 | 역할 |
|---------|------|
| `MainMenu` | 타이틀 화면 |
| `GameScene` | 빌드 + 플레이 페이즈 (하나의 씬에서 페이즈 전환) |
| `EndingScene` | 누적 효율 점수에 따라 Best / Normal / Bad 엔딩 |

---

## 악기 목록 (21종)

| # | ID | 이름 | 키 | 다리형태 | 파형 | 색상 |
|---|-----|------|-----|---------|------|------|
| 1 | piano | Piano | Alpha1 | Straight | Sine | 흰색 |
| 2 | guitar | Guitar | Alpha2 | Curved | Pluck | 갈색 |
| 3 | drum | Drum | Alpha3 | Zigzag | Noise | 검회색 |
| 4 | flute | Flute | Alpha4 | Straight | Breath | 하늘색 |
| 5 | violin | Violin | Alpha5 | Curved | Sawtooth | 빨간색 |
| 6 | bass | Bass | Alpha6 | Stepped | Triangle | 보라색 |
| 7 | xylophone | Xylophone | Alpha7 | Zigzag | Square | 노란색 |
| 8 | bell | Bell | Alpha8 | Straight | Bell | 오렌지 |
| 9 | trumpet | Trumpet | Alpha9 | Bouncy | Brass | 금색 |
| 10 | saxophone | Saxophone | Alpha0 | Curved | Sawtooth | 황금색 |
| 11 | harp | Harp | Q | Slippery | StringPad | 연보라 |
| 12 | organ | Organ | W | Wide | Organ | 진갈색 |
| 13 | synth | Synth | E | Bouncy | SynthPad | 시안 |
| 14 | marimba | Marimba | R | Zigzag | Marimba | 갈색 |
| 15 | clarinet | Clarinet | T | Straight | Square | 검정 |
| 16 | harmonica | Harmonica | Y | Slippery | Harmonica | 은색 |
| 17 | banjo | Banjo | U | Curved | Sitar | 황갈색 |
| 18 | cello | Cello | I | Curved | StringPad | 짙은갈색 |
| 19 | tuba | Tuba | O | Stepped | Brass | 올리브 |
| 20 | accordion | Accordion | P | Wide | Accordion | 빨간색 |
| 21 | kalimba | Kalimba | F | Straight | Kalimba | 연초록 |

---

## 스테이지 목록 (15개)

| # | 이름 | 고유 기믹 | 획득 악기 |
|---|------|----------|----------|
| 1 | Tutorial - First Note | 없음 (튜토리얼) | Guitar |
| 2 | Strings Attached | Moving 장애물 입문 | Drum |
| 3 | Wind of Change | Wind 영역 첫 등장 | Flute |
| 4 | Crumbling Path | Falling 발판 첫 등장 | Violin |
| 5 | Spiked Rhythm | Spike 첫 등장 | Bass, Trumpet |
| 6 | Bounce House | **Bouncer 첫 등장** | Saxophone |
| 7 | Moving Melody | 복합 Moving | Xylophone, Harp |
| 8 | Gravity Shift | **GravityFlip 첫 등장** | Organ, Bell |
| 9 | Ticking Gates | **TimedGate 첫 등장** | Synth |
| 10 | Laser Symphony | **Laser 첫 등장** | Marimba, Clarinet |
| 11 | Frozen Melody | **Ice 첫 등장** | Harmonica, Banjo |
| 12 | Symphony of Traps | Bouncer+Laser+Wind 복합 | Cello |
| 13 | Gravity Maze | GravityFlip+TimedGate+Ice 복합 | Tuba, Accordion |
| 14 | Tight Tempo | 빠른 판정선 + 전체 기믹 | Kalimba |
| 15 | Grand Finale | 9종 전체 장애물 총출동 | — |

---

## 장애물 목록 (9종)

| 종류 | 클래스 | 치명 | 설명 |
|------|--------|------|------|
| Moving | `MovingObstacle` | O | Sin파 왕복 이동 |
| Falling | `FallingObstacle` | O | 밟으면 흔들린 후 낙하 |
| Wind | `WindZoneObstacle` | X | 방향 힘 적용 |
| Spike | `SpikeObstacle` | O | 고정 가시 |
| Bouncer | `BouncerObstacle` | X | 트램폴린 (눌림/복원 애니) |
| GravityFlip | `GravityZoneObstacle` | X | 중력 반전 (진입/퇴장 복원) |
| TimedGate | `TimedGateObstacle` | X | 주기적 개폐 (경고 깜빡임) |
| Laser | `LaserObstacle` | O | 주기적 레이저 (경고 후 활성화) |
| Ice | `IceZoneObstacle` | X | 미끄러운 바닥 (드래그 감소) |

---

## 다리 형태 (7종)

| 형태 | 특징 | 사용 악기 |
|------|------|----------|
| Straight | 표준 가로 직선 | Piano, Flute, Bell, Clarinet, Kalimba |
| Curved | 위치 기반 아치 회전 | Guitar, Violin, Saxophone, Banjo, Cello |
| Zigzag | 교대 오프셋 + 기울기 | Drum, Xylophone, Marimba |
| Stepped | 점진적 높이 상승 | Bass, Tuba |
| Bouncy | 탄성 다리 (플레이어 바운스) | Trumpet, Synth |
| Slippery | 마찰 0.02 (미끄러움) | Harp, Harmonica |
| Wide | 폭 1.5배 넓은 플랫폼 | Organ, Accordion |

---

## 생성된 스크립트 목록

### Scripts/Data/
| 파일 | 역할 |
|------|------|
| `InstrumentData.cs` | 악기 ScriptableObject (키, 다리형태 7종, 사운드, 색상) |
| `StageData.cs` | 스테이지 ScriptableObject (장애물 9종 + 픽업 배치) |
| `InstrumentRegistry.cs` | 런타임 악기 이름 검색 (Resources 로드) |

### Scripts/Managers/
| 파일 | 역할 |
|------|------|
| `GameManager.cs` | 싱글톤. 게임 페이즈 전환, TotalStages=15, 엔딩 분기 (평균 75+/45+) |
| `AudioManager.cs` | 싱글톤. 음표 재생 + 녹음 → 순서 재생 |
| `StageManager.cs` | 현재 스테이지, 보유 악기 (PlayerPrefs 영구 저장) |
| `UIManager.cs` | 페이즈별 패널 전환, 사이드바 악기 버튼 |
| `MainMenuController.cs` | 메인메뉴 버튼 (시작/종료) |
| `StageResultUI.cs` | 클리어 결과 패널 (등급, 점수, 음악감상, 다음/재도전) |

### Scripts/Gameplay/BuildPhase/
| 파일 | 역할 |
|------|------|
| `BuildPhaseManager.cs` | 빌드 페이즈 흐름 (키입력 → 악기선택 → 다리생성) |
| `RhythmLineController.cs` | 판정선 이동 (startX→endX), 끝 도달 이벤트 |
| `RhythmLineVisual.cs` | 판정선 시각화 (LineRenderer + 플래시 + Trail) |
| `BridgeBuilder.cs` | 판정선 위치에 다리 조각 생성, 사용 횟수 기록 |
| `BridgePiece.cs` | 개별 다리 조각 — 7종 형태 구현 (Bouncy 바운스, Slippery 마찰) |
| `CurvedBridgeRenderer.cs` | 곡선형 다리 LineRenderer + PolygonCollider2D |
| `InstrumentSidebarButton.cs` | 사이드바 악기 버튼 UI |
| `SheetMusicBackground.cs` | 오선지 프로시저럴 배경 |

### Scripts/Gameplay/PlayPhase/
| 파일 | 역할 |
|------|------|
| `PlayPhaseManager.cs` | 플레이 페이즈 흐름 (클리어/낙사 처리, StageResultUI 연동) |
| `PlayerController.cs` | 캐릭터 이동(A/D) + 점프(Space) + 접지판정 |
| `CameraFollow.cs` | 빌드=전체뷰, 플레이=플레이어 추적 |
| `InstrumentPickup.cs` | 악기 아이템 픽업 (충돌 시 영구 획득) |
| `GoalTrigger.cs` | 목적지 트리거 |
| `DeathZone.cs` | 낙사 구역 트리거 |
| `ObstacleBase.cs` | 장애물 추상 베이스 (isLethal 분기) |
| `MovingObstacle.cs` | Sin파 왕복 이동 장애물 |
| `FallingObstacle.cs` | 밟으면 흔들림 후 낙하 |
| `WindZoneObstacle.cs` | 바람 영역 (지속 힘) |
| `SpikeObstacle.cs` | 고정 가시 (즉사) |
| `BouncerObstacle.cs` | **[신규]** 트램폴린 (Squash&Stretch 애니) |
| `GravityZoneObstacle.cs` | **[신규]** 중력 반전 (원래 gravityScale 기억/복원) |
| `TimedGateObstacle.cs` | **[신규]** 주기적 개폐 문 (경고 깜빡임) |
| `LaserObstacle.cs` | **[신규]** 주기적 레이저 빔 (활성화 전 경고) |
| `IceZoneObstacle.cs` | **[신규]** 미끄러운 얼음 (드래그 0) |
| `EndingController.cs` | 엔딩 씬 점수 기반 UI 분기 |

### Scripts/Gameplay/Scoring/
| 파일 | 역할 |
|------|------|
| `EfficiencyScoreManager.cs` | 악기 사용 횟수 기반 점수 (0~100), S/A/B/C/D 등급 |

### Scripts/Gameplay/
| 파일 | 역할 |
|------|------|
| `StageBuilder.cs` | StageData에서 지형/장애물(9종)/픽업 스폰 |

### Scripts/SaveSystem/
| 파일 | 역할 |
|------|------|
| `MusicRecording.cs` | 녹음 데이터 (악기, 노트, 시간, 볼륨) |
| `MusicSaveSystem.cs` | JSON 파일로 저장/불러오기 (스테이지별 폴더) |

### Scripts/UI/
| 파일 | 역할 |
|------|------|
| `MusicGalleryUI.cs` | 스테이지별 저장 음악 브라우저 (폴리브릿지 스타일) |

### Editor/
| 파일 | 역할 |
|------|------|
| `AssetGenerator.cs` | 악기 21종 + 스테이지 15개 + 프리팹 18종 + 프로시저럴 오디오 일괄 생성 |
| `SceneSetupEditor.cs` | 씬 계층구조 자동 생성 + 새 장애물 프리팹 와이어링 |
| `ProceduralAudioGenerator.cs` | 20파형 지원 프로시저럴 WAV 합성 (44.1kHz 16-bit PCM) |

---

## 게임 흐름 요약

```
MainMenu
  └─→ GameScene
        ├─ [Build Phase]
        │    판정선 이동 중 숫자/문자 키로 악기 선택 → Space로 다리 생성
        │    다리 생성 시 음표 재생 + 녹음
        │    판정선 끝 도달 → Play Phase 자동 전환
        │
        ├─ [Play Phase]
        │    A/D 이동, Space 점프
        │    장애물 9종 회피 (Bouncer로 높이뛰기, GravityFlip 주의 등)
        │    악기 아이템 픽업 → 영구 보유
        │    목적지 도달 → 결과 UI + 음악 재생 → 다음 스테이지
        │    낙사 → 다리 초기화 후 Build Phase 재시작
        │
        └─ (15스테이지 모두 클리어)
              └─→ EndingScene
                    평균 효율 점수 기준:
                    ≥ 75점 → Best Ending
                    ≥ 45점 → Normal Ending
                    < 45점 → Bad Ending
```

---

## 핵심 클래스 관계

```
GameManager (페이즈 전환, TotalStages=15)
  ├── BuildPhaseManager
  │     ├── RhythmLineController  ← 판정선 이동
  │     ├── BridgeBuilder         ← 다리 생성 (7종 형태)
  │     └── AudioManager          ← 음표 녹음 (21종 악기)
  ├── PlayPhaseManager
  │     ├── PlayerController
  │     ├── EfficiencyScoreManager
  │     └── StageResultUI         ← 결과 패널 (이중호출 방지)
  ├── StageManager
  │     └── InstrumentData × 21종 (ScriptableObject)
  └── StageBuilder
        ├── 장애물 9종 프리팹 스폰
        └── InstrumentPickup 스폰
```

---

## 버그 수정 이력

### [2026-04-13] linearVelocity 컴파일 오류
- **파일**: `PlayerController.cs`
- **수정**: `rb.linearVelocity` → `rb.velocity` (Unity 2022.3 호환)

### [2026-04-13] WindZone 내장 컴포넌트 이름 충돌
- **파일**: `WindZone.cs` → `WindZoneObstacle.cs`로 클래스명 변경

### [2026-04-13] OnTriggerEnter2D 숨김 경고 (CS0114)
- **파일**: `FallingObstacle.cs`, `WindZoneObstacle.cs`
- **수정**: `private void` → `protected override void`

### [2026-04-17 v2] 신규 수정 사항
- **GravityZoneObstacle**: `Mathf.Abs(rb.gravityScale)` → 원래 gravityScale 값 기억/복원 (Player 프리팹의 gravityScale=3)
- **LaserObstacle**: `OnTriggerEnter2D`에서 `PlayerController.DisableControl()` 호출 후 사망 처리 (중복 호출 방지)
- **PlayPhaseManager**: `OnPlayerReachedGoal()`에서 `GameManager.OnStageClear()` 직접 호출 제거 → `StageResultUI.Show()` 통해 버튼 클릭 시 진행 (이중 스테이지 진행 방지)
- **AssetGenerator**: `GenerateAll()`에서 새 프리팹 5종이 `SaveAssets()` 뒤에 생성되던 문제 → 순서 수정

---

## 자동 생성 메뉴

### 한 번에 모든 자산 생성
```
BridgeRhythm → Generate All Assets
```
자동 생성 항목:
- `Assets/Sprites/Square.png` (32x32 흰색 플레이스홀더)
- 태그 `Player`, `Goal`, `DeathZone`, `Ground` + 레이어 `Ground`
- `Assets/ScriptableObjects/` — 악기 21종 + 스테이지 15개
- `Assets/Resources/InstrumentRegistry.asset`
- `Assets/Audio/` — 21 × 8 = 168개 프로시저럴 WAV
- `Assets/Prefabs/` — 18종 프리팹:
  - BridgePiece, Player, InstrumentSidebarButton
  - Goal, Ground, InstrumentPickup
  - MovingObstacle, FallingObstacle, WindZone, Spike
  - **Bouncer, GravityZone, TimedGate, Laser, IceZone** (신규)
  - StageTab, RecordEntry

### 부분 생성
- `BridgeRhythm → Generate ScriptableObjects` — 악기/스테이지만
- `BridgeRhythm → Generate Prefabs` — 프리팹만
- `BridgeRhythm → Regenerate Audio Clips` — 오디오만

### 씬 자동 설정
```
BridgeRhythm → Setup GameScene
```
- 매니저 계층 + UI + 판정선 + DeathZone + 플레이어 자동 생성
- StageBuilder에 장애물 프리팹 9종 + 픽업/지형/골 프리팹 자동 와이어링
- StageManager에 스테이지 15개 자동 연결

---

## 작업 절차 (새 환경 기준)

1. Unity 에디터 열기
2. `BridgeRhythm → Generate All Assets` 클릭 (168개 오디오 + 자산 일괄 생성)
3. 씬 3개 생성: `MainMenu.unity`, `GameScene.unity`, `EndingScene.unity`
   - Build Settings 순서: MainMenu(0), GameScene(1), EndingScene(2)
4. 각 씬 열고 `BridgeRhythm → Setup [씬이름]` 실행
5. Ctrl+S 로 저장
6. Play 테스트

---

## 미완성 / 추후 작업 필요 항목

### 비주얼
- [ ] 플레이어 스프라이트 / 애니메이션 (현재 파란 사각형)
- [ ] 다리 조각 스프라이트 (현재 흰 사각형 + 색상)
- [ ] 스테이지별 배경 / 지형 디자인
- [ ] 엔딩 연출 (이미지, 텍스트 확정)
- [ ] 장애물별 시각 효과 (WindZone 파티클, Laser 글로우 등)

### 게임플레이
- [ ] CurvedBridgeRenderer와 BridgePiece.ApplyCurved() 연동 (현재 회전으로 임시 처리)
- [ ] Zigzag/Stepped 다리의 더 정교한 비주얼
- [ ] 스테이지별 난이도 밸런싱 (실플레이 테스트 필요)
- [ ] 음악 갤러리를 메인메뉴에서 접근 가능하게

### 사운드
- [ ] BGM 추가
- [ ] UI 효과음 (버튼 클릭, 스테이지 클리어 등)

---

## 참고 파일
- `Assets/SETUP_GUIDE.txt` — 전체 세팅 가이드 (상세)
- `Assets/Editor/SceneSetupEditor.cs` — 씬 자동 생성 에디터 툴
- `Assets/Editor/AssetGenerator.cs` — 자산 일괄 생성
- `Assets/Editor/ProceduralAudioGenerator.cs` — 프로시저럴 WAV 합성
