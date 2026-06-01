# Bridge Melody — 게임 기획서

> 최종 업데이트: 2026-06-01
> 프로젝트: musebridge (GitHub: gyurara/musebridge, develop 브랜치)
> 유니티 버전: 2022.3.52f1

---

## 1. 게임 개요

### 1-1. 한 줄 소개

"음악으로 다리를 짓고, 직접 건너보세요."

### 1-2. 장르 및 레퍼런스

퍼즐 플랫포머 × 리듬 게임. Poly Bridge의 건축 메커니즘과 리듬 게임의 판정선 시스템을 결합한 독창적 구조. 플레이어는 리듬에 맞춰 다리를 건설하고, 그 다리를 직접 캐릭터로 건너가며, 과정에서 만들어진 소리가 하나의 음악이 된다.

### 1-3. 핵심 경험

플레이어가 게임을 통해 얻는 핵심 경험은 세 가지다.

첫째, 다리를 만드는 행위 자체가 작곡이 된다. 어떤 악기를 선택하느냐에 따라 다리의 형태와 소리가 달라지고, 결과적으로 자신만의 고유한 음악이 탄생한다.

둘째, 효율의 딜레마가 있다. 악기를 많이 쓰면 다리는 튼튼하지만 점수는 낮아진다. 적게 쓰면 점수는 높지만 건너기 어렵다. 이 긴장 관계가 게임의 전략적 깊이를 만든다.

셋째, 성장의 실감이 있다. Stage 1에서 피아노 하나로 시작해 Stage 15에서 21가지 악기를 지휘하는 과정이 음악적 역량의 성장으로 느껴진다.

### 1-4. 타겟 플랫폼 및 조작

PC (키보드). 숫자/문자 키로 악기 선택, Space로 다리 생성 및 점프, A/D로 좌우 이동.

---

## 2. 게임 흐름

### 2-1. 전체 구조

```
MainMenu (타이틀)
  │
  ├── [음악 갤러리] ← 이전 플레이의 녹음 감상
  │
  └── START
        │
        └── GameScene (15 스테이지 순차 진행)
              │
              ├── Build Phase ← 판정선에 맞춰 다리 건설
              │     │
              │     └── (판정선 끝 도달)
              │
              ├── Play Phase ← 캐릭터 조작으로 다리 건너기
              │     │
              │     ├── 목적지 도달 → 결과 UI + 음악 감상
              │     │     └── 다음 스테이지 / 재도전
              │     │
              │     └── 낙사 → Build Phase 재시작
              │
              └── (15 스테이지 클리어)
                    │
                    └── EndingScene
                          ├── Best Ending   (평균 효율 ≥ 75)
                          ├── Normal Ending  (평균 효율 ≥ 45)
                          └── Bad Ending     (평균 효율 < 45)
```

### 2-2. 씬 구성

| 씬 | 파일 | Build Settings 순서 | 역할 |
|----|------|---------------------|------|
| MainMenu | `MainMenu.unity` | 0 | 타이틀 화면, 시작/종료 버튼, 음악 갤러리 |
| GameScene | `GameScene.unity` | 1 | Build + Play 페이즈 통합. 15스테이지 루프 |
| EndingScene | `EndingScene.unity` | 2 | 누적 점수 기반 3분기 엔딩 연출 |

---

## 3. Build Phase (빌드 페이즈)

### 3-1. 판정선 시스템

화면 왼쪽에서 오른쪽으로 판정선(수직 바)이 일정 속도로 이동한다. 판정선이 이동하는 동안 플레이어는 악기를 선택하고 Space 키를 눌러 판정선의 현재 위치에 다리 조각을 생성한다.

판정선 속도는 스테이지별로 다르며 (Stage 1: 3.0 → Stage 15: 5.5), 후반으로 갈수록 빠른 반응이 요구된다. 판정선이 스테이지 끝에 도달하면 자동으로 Play Phase로 전환된다.

### 3-2. 악기 선택 및 다리 생성

좌측 사이드바에 보유 중인 악기 목록이 표시된다. 숫자/문자 키(1~0, Q~F) 또는 사이드바 클릭으로 악기를 선택하고, Space를 누르면 판정선 위치에 해당 악기의 다리 조각이 배치된다.

다리 조각 배치 시 해당 악기의 음표가 동시에 재생되며, 이 소리가 순서대로 녹음된다. 스테이지 클리어 후 녹음된 음표들이 자동 재생되어 "자신이 만든 음악"을 감상할 수 있다.

### 3-3. 다리 조각 예산

각 스테이지에는 maxBridgePieces(최대 배치 수)가 정해져 있다. 많이 쓸수록 점수가 낮아지므로, 꼭 필요한 곳에만 배치하는 전략이 중요하다.

### 3-4. 실시간 HUD (StageProgressUI)

| 표시 항목 | 위치 | 설명 |
|-----------|------|------|
| Stage 번호 / 이름 | 좌상단 | "Stage 3 / 15 — Wind of Change" |
| 남은 조각 수 | 중앙 상단 | 5개 이하 시 빨간색 경고 |
| 효율 등급 게이지 | 중앙 상단 | S~D 색상 변화 (금/파/초/주/회) |
| 판정선 진행도 바 | 중앙 상단 | 0~100% 슬라이더 |
| 선택 악기 이름 | 중앙 상단 | "♪ Piano" |

---

## 4. Play Phase (플레이 페이즈)

### 4-1. 캐릭터 조작

| 입력 | 동작 |
|------|------|
| A / D | 좌 / 우 이동 |
| Space | 점프 (접지 상태에서만) |

캐릭터는 16×20 픽셀아트로 프로시저럴 생성되며, Idle(4프레임) / Run(4프레임) / Jump / Fall 상태별 애니메이션을 가진다. 착지 시 Squash & Stretch 연출이 적용되고, 악기 픽업 시 해당 악기 색상으로 플래시 이펙트가 발생한다.

### 4-2. 접지 판정

Player 오브젝트 하단의 GroundCheck 위치에서 groundCheckRadius(0.15) 반경 내에 Ground 레이어 충돌체가 있으면 접지 판정. 다리 조각은 모두 Ground 레이어에 속한다.

### 4-3. 카메라

Build Phase에서는 스테이지 전체를 조망하는 Overview 모드(orthographicSize: 7), Play Phase에서는 플레이어를 추적하는 Follow 모드(orthographicSize: 5)로 자동 전환.

### 4-4. 악기 픽업

스테이지 내 특정 위치에 악기 아이템이 배치되어 있으며, 캐릭터가 접촉하면 영구적으로 해당 악기를 획득한다. 획득한 악기는 PlayerPrefs에 저장되어 이후 스테이지에서도 사용 가능. 픽업 시 피치 상승 SFX + 캐릭터 플래시 연출.

### 4-5. 클리어 / 실패

목적지(Goal)에 도달하면 클리어. 효율 점수와 등급이 표시되며, 녹음된 음악이 자동 재생된다.
DeathZone(y=-12)에 낙하하면 실패. 다리가 전부 초기화되고 Build Phase로 되돌아간다.

---

## 5. 악기 시스템

### 5-1. 악기 목록 (21종)

시작 시 Piano 하나만 보유하며, 스테이지를 진행하며 하나씩 획득한다.

| # | 이름 | 키 | 다리 형태 | 파형 | 색상 |
|---|------|-----|-----------|------|------|
| 1 | Piano | 1 | Straight | Sine | 흰색 |
| 2 | Guitar | 2 | Curved | Pluck | 갈색 |
| 3 | Drum | 3 | Zigzag | Noise | 검회색 |
| 4 | Flute | 4 | Straight | Breath | 하늘색 |
| 5 | Violin | 5 | Curved | Sawtooth | 빨간색 |
| 6 | Bass | 6 | Stepped | Triangle | 보라색 |
| 7 | Xylophone | 7 | Zigzag | Square | 노란색 |
| 8 | Bell | 8 | Straight | Bell | 오렌지 |
| 9 | Trumpet | 9 | Bouncy | Brass | 금색 |
| 10 | Saxophone | 0 | Curved | Sawtooth | 황금색 |
| 11 | Harp | Q | Slippery | StringPad | 연보라 |
| 12 | Organ | W | Wide | Organ | 진갈색 |
| 13 | Synth | E | Bouncy | SynthPad | 시안 |
| 14 | Marimba | R | Zigzag | Marimba | 갈색 |
| 15 | Clarinet | T | Straight | Square | 검정 |
| 16 | Harmonica | Y | Slippery | Harmonica | 은색 |
| 17 | Banjo | U | Curved | Sitar | 황갈색 |
| 18 | Cello | I | Curved | StringPad | 짙은갈색 |
| 19 | Tuba | O | Stepped | Brass | 올리브 |
| 20 | Accordion | P | Wide | Accordion | 빨간색 |
| 21 | Kalimba | F | Straight | Kalimba | 연초록 |

### 5-2. 프로시저럴 사운드

모든 악기 음표는 프로시저럴 오디오로 생성된다 (44.1kHz, 16-bit PCM WAV). 20종 파형을 조합하며, 각 악기당 8개 음표(C4~C5 메이저 스케일)를 생성한다. 총 168개 WAV 파일이 에디터 메뉴 한 번으로 자동 생성된다.

---

## 6. 다리 형태 시스템 (7종)

각 악기는 고유한 다리 형태를 가지며, 형태별로 텍스처도 프로시저럴 생성된다(64×16 픽셀아트).

| 형태 | 시각 텍스처 | 물리 특성 | 사용 악기 |
|------|------------|-----------|----------|
| **Straight** | 나무판자 결 (수평 선무늬) | 표준 직선 플랫폼 | Piano, Flute, Bell, Clarinet, Kalimba |
| **Curved** | 아치 돌블록 (줄눈 패턴) | Catmull-Rom 스플라인 곡선 + PolygonCollider2D | Guitar, Violin, Saxophone, Banjo, Cello |
| **Zigzag** | 금속 격자 (대각 해칭, 리벳) | 5개 자식 조각으로 분할, 교대 기울기 | Drum, Xylophone, Marimba |
| **Stepped** | 계단 벽돌 (오프셋 모르타르) | 5개 자식 조각, 점진 높이 상승 | Bass, Tuba |
| **Bouncy** | 스프링 물결 (초록 코일) | 캐릭터가 밟으면 위로 바운스 (force: 14) | Trumpet, Synth |
| **Slippery** | 얼음 결정 (반짝임 크랙) | 마찰 0.01 미끄러운 표면 | Harp, Harmonica |
| **Wide** | 체커보드 타일 | 폭 1.6배 넓은 플랫폼 | Organ, Accordion |

---

## 7. 장애물 시스템 (9종)

### 7-1. 장애물 목록

| 종류 | 클래스 | 치명 | 동작 | VFX |
|------|--------|------|------|-----|
| **Moving** | `MovingObstacle` | ● | 지정 방향으로 Sin파 왕복 이동 | 궤적 잔상 (TrailRenderer) |
| **Falling** | `FallingObstacle` | ● | 밟으면 0.5초 진동 후 낙하 (Kinematic → Dynamic) | 먼지 파티클 버스트 |
| **Wind** | `WindZoneObstacle` | ○ | 영역 내 캐릭터에 지속 힘 적용 | 흐르는 바람 파티클 |
| **Spike** | `SpikeObstacle` | ● | 고정 위치. 접촉 즉시 사망 | 주기적 반짝 광택 |
| **Bouncer** | `BouncerObstacle` | ○ | 접촉 시 위로 튕김 (force: configurable) | 아이들 진동 + 눌림/복원 |
| **GravityFlip** | `GravityZoneObstacle` | ○ | 영역 내 중력 반전. 퇴장 시 복원 | HSV 색상 순환 펄스 |
| **TimedGate** | `TimedGateObstacle` | ○ | 열림/닫힘 주기 교대. 물리적 차단 | 개폐 상태별 투명도 변화 |
| **Laser** | `LaserObstacle` | ● | 활성/비활성 주기. 경고 깜빡임 후 활성화 | 박동 글로우 레이어 |
| **Ice** | `IceZoneObstacle` | ○ | 영역 내 캐릭터 drag 0 (미끄러짐) | 표면 반짝임 파티클 |

● = 치명(사망), ○ = 비치명(방해)

### 7-2. 장애물 등장 스케줄

Stage 1~4는 단일 기믹, Stage 5부터 복합. Stage 12~15는 전체 기믹 총동원.

---

## 8. 스테이지 구성 (15개)

| Stage | 이름 | 판정선 속도 | 스테이지 폭 | 최대 조각 | 기믹 | 획득 악기 |
|-------|------|-----------|-----------|----------|------|----------|
| 1 | Tutorial - First Note | 3.0 | 20 | 30 | 없음 | Guitar |
| 2 | Strings Attached | 3.2 | 22 | 30 | Moving | Drum |
| 3 | Wind of Change | 3.5 | 24 | 32 | Wind | Flute |
| 4 | Crumbling Path | 3.8 | 26 | 34 | Falling | Violin |
| 5 | Spiked Rhythm | 4.0 | 28 | 36 | Spike ×3 | Bass, Trumpet |
| 6 | Bounce House | 4.0 | 26 | 36 | Bouncer ×2 + Spike | Saxophone |
| 7 | Moving Melody | 4.2 | 28 | 38 | Moving ×2 (수직) | Xylophone, Harp |
| 8 | Gravity Shift | 4.3 | 30 | 38 | GravityFlip + Spike ×2 | Organ, Bell |
| 9 | Ticking Gates | 4.5 | 30 | 40 | TimedGate ×3 | Synth |
| 10 | Laser Symphony | 4.5 | 32 | 40 | Laser ×3 | Marimba, Clarinet |
| 11 | Frozen Melody | 4.8 | 32 | 40 | Ice ×2 + Wind | Harmonica, Banjo |
| 12 | Symphony of Traps | 5.0 | 34 | 42 | Bouncer + Laser + Wind + Spike | Cello |
| 13 | Gravity Maze | 5.0 | 34 | 42 | GravityFlip + TimedGate + Ice + Falling | Tuba, Accordion |
| 14 | Tight Tempo | 5.5 | 34 | 42 | Moving ×2 + Laser + TimedGate + Spike | Kalimba |
| 15 | Grand Finale | 5.5 | 38 | 48 | 9종 전체 장애물 | — |

---

## 9. 효율 점수 및 등급 시스템

### 9-1. 점수 계산

각 스테이지마다 다리 조각 사용 횟수를 기반으로 효율 점수(0~100)를 계산한다.

- **perfectThreshold** 이하 사용 → 100점
- **maxBridgePieces** 이상 사용 → 0점
- 그 사이: 선형 보간

### 9-2. 등급

| 등급 | 점수 범위 | 색상 |
|------|----------|------|
| S | 90~100 | 금색 |
| A | 70~89 | 파란색 |
| B | 50~69 | 초록색 |
| C | 30~49 | 주황색 |
| D | 0~29 | 회색 |

### 9-3. 엔딩 분기

15스테이지의 평균 효율 점수로 엔딩이 결정된다.

| 엔딩 | 조건 | 연출 |
|------|------|------|
| **Best — True Musician** | 평균 ≥ 75 | 금색 타이핑 + 팡파레 BGM + 200개 파티클 + 악보 노트 12초 플로팅 |
| **Normal — Bridge Complete** | 평균 ≥ 45 | 파란 타이핑 + 서정적 BGM + 80개 파티클 + 노트 6초 플로팅 |
| **Bad — Noise...** | 평균 < 45 | 빨간 타이핑 + 불협화음 드론 BGM + 파티클 없음 |

---

## 10. 사운드 시스템

### 10-1. 악기 음표 (프로시저럴)

20종 파형으로 21개 악기 × 8개 음표(C4~C5) = 168개 WAV 파일. `ProceduralAudioGenerator`가 에디터 메뉴에서 일괄 합성.

### 10-2. BGM (프로시저럴 런타임)

`BGMManager`가 게임 시작 시 6종 BGM을 런타임에 합성. 씬 전환 시 크로스페이드 자동 적용.

| 페이즈 | BGM | 특징 |
|--------|-----|------|
| Build Phase | D minor 오르간 아르페지오 | 6초 루프, 느린 하모닉스 |
| Play Phase | C major 피아노+마림바 | 4초 루프, 밝고 경쾌 |
| Music Review | C major 사인파 패드 | 8초 루프, 잔잔한 앰비언트 |
| Best Ending | G major 팡파레 | 6초, 상행 분산화음 + 트레몰로 |
| Normal Ending | 서정적 멜로디 | 6초, E-D-C 순차 하행 |
| Bad Ending | 불협화음 드론 | 6초, A-A#-B 반음 간격 |

### 10-3. UI SFX (프로시저럴)

| 효과음 | 설명 | 트리거 |
|--------|------|--------|
| Click | 800Hz 감쇠 (0.1초) | 버튼 클릭 |
| Clear | C5-E5-G5-C6 상행 (0.5초) | 스테이지 클리어 |
| Pickup | 440→880Hz 피치 상승 (0.25초) | 악기 획득 |
| Judge | 600+900Hz 듀얼톤 (0.125초) | 다리 배치 판정 |
| Fall | 400→80Hz 피치 하강 (0.33초) | 낙사 |
| Build | 350+700Hz 스텝 (0.083초) | 다리 생성 |

### 10-4. 음악 녹음 및 감상

다리 생성 시 재생되는 음표가 시간순으로 `MusicRecording`에 기록되며, 스테이지 클리어 후 순서대로 재생된다. JSON 형식으로 로컬 저장되어 음악 갤러리에서 언제든 다시 감상 가능.

---

## 11. 비주얼

### 11-1. 아트 스타일

프로시저럴 픽셀아트. 모든 스프라이트/텍스처가 코드로 런타임 또는 에디터에서 생성된다.

| 요소 | 방식 | 크기 |
|------|------|------|
| 플레이어 캐릭터 | `PlayerVisual.cs` 런타임 베이킹 | 16×20px |
| 다리 조각 (7종) | `BridgeSpriteGenerator.cs` 런타임 | 64×16px |
| 스테이지 배경 | `SheetMusicBackground.cs` 프로시저럴 | 월드스페이스 Canvas |
| 배경 이미지 | `sheet_music_background.png` | 1920×1080px |

### 11-2. 장애물 VFX

`ObstacleVisualEffect.cs`가 각 장애물 프리팹에 자동 부착되어, 종류를 감지하고 대응하는 시각 효과를 생성한다 (파티클, 글로우, 궤적, 광택 등).

### 11-3. 배경

크림색 종이 위의 오선지, 음자리표, 소절 구분선, 음표 장식이 배치된 악보 스타일. 왼쪽에 링노트 구멍, 빨간 마진선 등 노트 느낌의 디테일 포함.

---

## 12. UI 구성

### 12-1. MainMenu

- 타이틀 텍스트: "Bridge Melody"
- 서브타이틀: "음악으로 다리를 만들어 건너세요"
- START 버튼 → GameScene 로드
- QUIT 버튼 → 종료
- 음악 갤러리 버튼 → `MusicGalleryUI` 패널 오픈

### 12-2. GameScene

| 패널 | 위치 | 내용 |
|------|------|------|
| StageProgressUI HUD | 상단 바 | 스테이지 정보, 예산, 게이지, 악기 이름 |
| BuildPhasePanel | 좌측 사이드바 | 보유 악기 버튼 목록 (InstrumentSidebarButton) |
| PlayPhasePanel | 상단 바 | (Build 숨김, Play 표시) |
| StageResultUI | 중앙 모달 | 결과 패널 (등급, 점수, 음악감상, 다음/재도전 버튼) |

### 12-3. EndingScene

- 배경 페이드인 (Best: 심야 파랑 / Normal: 따뜻한 갈색 / Bad: 어두운 적갈)
- 타이핑 효과 타이틀
- 서브타이틀 페이드인
- 악보 노트 플로팅 + 파티클 버스트
- 점수 카운트업 애니메이션
- TITLE 버튼 → MainMenu 복귀

---

## 13. 데이터 구조

### 13-1. ScriptableObject

| 클래스 | 경로 | 용도 |
|--------|------|------|
| `InstrumentData` | Assets/ScriptableObjects/*.asset | 악기별 키/다리형태/색상/파형/음표클립 |
| `StageData` | Assets/ScriptableObjects/Stage*.asset | 스테이지별 판정선속도/폭/장애물배치/픽업 |
| `InstrumentRegistry` | Assets/Resources/ | 악기 이름 검색용 레지스트리 |

### 13-2. 저장 시스템

| 데이터 | 방식 | 위치 |
|--------|------|------|
| 보유 악기 | PlayerPrefs | "BridgeMelody.OwnedInstruments" |
| 음악 녹음 | JSON 파일 | Application.persistentDataPath/recordings/ |

---

## 14. 기술 아키텍처

### 14-1. 싱글톤 매니저 (DontDestroyOnLoad)

| 매니저 | 역할 |
|--------|------|
| `GameManager` | 페이즈 전환, 스테이지 인덱스, 엔딩 분기 |
| `AudioManager` | 악기 음표 재생/녹음, 순서 재생 |
| `BGMManager` | BGM 크로스페이드, UI SFX |
| `StageManager` | 현재 스테이지 데이터, 보유 악기 관리 |

### 14-2. 씬 종속 매니저

| 매니저 | 역할 |
|--------|------|
| `BuildPhaseManager` | 빌드 페이즈 흐름 |
| `PlayPhaseManager` | 플레이 페이즈 흐름 |
| `UIManager` | 패널 전환, 사이드바 갱신 |
| `BridgeBuilder` | 다리 조각 인스턴스화, 사용 카운트 |
| `EfficiencyScoreManager` | 점수 계산, 등급 판정 |
| `StageBuilder` | StageData 기반 지형/장애물/픽업 스폰 |

### 14-3. 에디터 자동화

| 메뉴 | 기능 |
|------|------|
| `BridgeRhythm → Generate All Assets` | 악기 21종 + 스테이지 15개 + 프리팹 18종 + WAV 168개 |
| `BridgeRhythm → Setup GameScene` | 매니저 계층 + UI + 판정선 + 와이어링 |
| `BridgeRhythm → Setup MainMenu` | 타이틀 UI 자동 생성 |
| `BridgeRhythm → Setup EndingScene` | 엔딩 UI 자동 생성 |
| `BridgeRhythm → Add ALL Missing TODO Items` | BGMManager + PlayerVisual + VFX + HUD 추가 |

---

## 15. 코드 규모

| 분류 | 파일 수 | 총 라인 수 |
|------|---------|-----------|
| Editor (에디터 전용) | 4 | ~1,700 |
| Scripts (런타임) | 46 | ~5,400 |
| 합계 | 50 | ~7,100 |

---

## 16. 세팅 절차 (새 환경)

1. Unity 2022.3.52f1로 프로젝트 열기
2. `BridgeRhythm → Generate All Assets` (자산 일괄 생성)
3. 씬 3개 생성: MainMenu / GameScene / EndingScene
4. Build Settings에 순서대로 등록 (0 / 1 / 2)
5. GameScene에서 `BridgeRhythm → Setup GameScene` → `Add ALL Missing TODO Items`
6. MainMenu에서 `BridgeRhythm → Setup MainMenu` → `Add Missing Components → Gallery to MainMenu`
7. EndingScene에서 `BridgeRhythm → Setup EndingScene` → `Add Missing Components → EndingVisual`
8. 각 씬 Ctrl+S 저장
9. Play 테스트

---

## 17. 향후 과제

| 분류 | 항목 | 우선순위 |
|------|------|---------|
| 밸런싱 | 15스테이지 난이도 실플레이 테스트 → threshold 조정 | 높음 |
| 비주얼 | 스테이지별 테마 배경 (숲/바다/우주 등) | 중간 |
| 비주얼 | 악기별 전용 아이콘 스프라이트 | 중간 |
| 게임플레이 | 튜토리얼 팝업 (Stage 1 조작 안내) | 높음 |
| 게임플레이 | 스테이지 선택 맵 (직선 진행 대신) | 낮음 |
| 사운드 | 장애물 전용 효과음 (바람 소리, 레이저 "비이이") | 중간 |
| 시스템 | 설정 메뉴 (볼륨, 키바인딩) | 중간 |
| 시스템 | 모바일 터치 UI 대응 | 낮음 |
