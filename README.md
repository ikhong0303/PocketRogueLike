# 🐾 PocketRoguelike (냥코 로그라이크)

> **포켓로그(PokeRogue) 시스템 아키텍처 × 냥코대전쟁 300종 캐릭터 리스킨 × 캐주얼 오토 턴제 전투**
>
> Unity 6 (URP) 기반으로 제작된 턴제 로그라이트 게임 프로젝트입니다. 300종의 고양이 캐릭터 수집과 파티 빌딩, 100 스테이지 등반 및 10스테이지마다 등장하는 보스전을 포함합니다.

---

## 📋 목차
1. [프로젝트 개요](#1-프로젝트-개요)
2. [핵심 루프 & 게임 규칙](#2-핵심-루프--게임-규칙)
3. [시스템 아키텍처 & 게임 상태 머신](#3-시스템-아키텍처--게임-상태-머신)
4. [주요 클래스 및 코드 모듈 상세 분석](#4-주요-클래스-및-코드-모듈-상세-분석)
   - [Core 모듈](#41-core-모듈)
   - [Battle & Catch 모듈](#42-battle--catch-모듈)
   - [Party & Stage 모듈](#43-party--stage-모듈)
   - [Data & Progression 모듈](#44-data--progression-모듈)
   - [UI & Localization 모듈](#45-ui--localization-모듈)
   - [Sound 모듈](#46-sound-모듈)
   - [Editor & Utilities 모듈](#47-editor--utilities-모듈)
5. [에디터 자동화 & 생산성 툴](#5-에디터-자동화--생산성-툴)
6. [주요 버그 및 트러블슈팅 내역](#6-주요-버그-및-트러블슈팅-내역)
7. [조작 방법 & 플레이 가이드](#7-조작-방법--플레이-가이드)

---

## 1. 프로젝트 개요

* **엔진/환경**: Unity 6000.3.18f1 (Universal Render Pipeline)
* **주요 장르**: 턴제 오토배틀 로그라이트 (Roguelite)
* **승리 조건**: 100 스테이지 파티 전멸 없이 클리어
* **주요 특징**:
  - **300종 고양이 컬렉션**: 도감 번호(1~300) 기반 스탯, 스피드, 희귀도 및 코스트 관리.
  - **오토 턴제 전투**: 스피드(도감 번호) 순서대로 자동으로 실행되는 빠른 속도의 전투.
  - **실시간 타이밍 캐치 시스템**: [SPACE] 키 입력으로 야생 고양이 포획 (적 잔여 HP, 희귀도, 3-Shake 확률 적용).
  - **인게임 회복약 & 기력의 조각 (부활)**:
    - **[H] 키 회복약**: 전투 중 즉시 출전 냥코 체력 50% 회복 (전용 `Drink.mp3` 사운드 즉시 1회 재생).
    - **기력의 조각 (10% 드랍)**: 파티 관리(`P`)에서 쓰러진 냥코를 **체력 50%로 부활** (전용 `Heal.mp3` 사운드 즉시 1회 재생).
  - **생동감 있는 오디오 피드백 (Sound Effects)**:
    - **포획 성공 신호**: 3번째 Shake 판정이 성공하는 순간(`truetruetrue`) `Catch.mp3` 1회 재생.
    - **전투 타격/피격**: 공격 시 `slap.mp3`, 피격 시 `ouch.mp3` 원샷 사운드 재생.
  - **코스트 예산제 스타팅 선택**: 최대 10점 예산 내에서 해금된 고양이 3마리로 런 시작.
  - **무작위 100스테이지 백본**: 런 시작 시 300종 중 100종을 추출·정렬하여 스테이지별 난이도 상승 곡선 생성.
  - **100% 완전 다국어(한/영) 실시간 전환 지원**:
    - `LanguageManager` 기반 스킬명 사전 및 키워드 자동 영문 번역기 탑재.
    - 영어 모드 선택 시 한국어 글자 0% 완전 배제 보장.

---

## 2. 핵심 루프 & 게임 규칙

### 2.1 런 루프 (In-Run Loop)
```
[스타팅 3마리 선택 (코스트 10점 이하)]
                 │
                 ▼
[스테이지 진입 (1 ~ 100 Stage)] ◄──────────┐
                 │                          │
         [오토 턴제 전투]                   │
          ├─ [SPACE] ➔ 고양이 캐치 (3-Shake 성공 시 Catch.mp3 재생)
          ├─ [H] ➔ 회복약 (HP 50% 회복 & Drink.mp3 재생)
          ├─ [P] ➔ 파티 관리 & 기력의 조각 부활 (Heal.mp3 재생)
          └─ 승리 ➔ 몬스터볼/회복약/기력의조각 드랍 ─┘
                 │
           (파티 전멸) ➔ [게임 오버 (런 종료)]
                 │
        (100스테이지 격파) ➔ [최종 승리!]
```

### 2.2 메타 루프 (Meta-Progression)
* 런 진행 중 새로운 고양이를 포획하면 **스타팅 수록 목록에 영구 해금**됩니다.
* 해금 정보 및 포획 횟수는 `PlayerPrefs`에 지속 저장되며, 다음 런 시작 시 더 강력한 고양이를 스타팅으로 선택할 수 있습니다.

---

## 3. 시스템 아키텍처 & 게임 상태 머신

프로젝트는 **중앙 중재자(Mediator) 패턴**과 **단일 책임 원칙(SRP)**을 준수하며 구축되었습니다.

### 3.1 GameManager 상태 트랜지션 (`GameState`)

| 상태 (`GameState`) | 설명 |
| :--- | :--- |
| `StarterSelect` | 런 시작 전 스타팅 고양이 선택 패널 (예산 10점 제한) |
| `StageBattle` | 오토 배틀 진행 중. [SPACE]로 포획, [H]로 회복약 사용, [P]로 파티 관리 진입 |
| `Catching` | 몬스터볼 투척 연출 및 3-Shake 포획 확률 계산 모드 (3-Shake 성공 시 `Catch.mp3` 재생) |
| `PartyManage` | 파티(최대 6마리) 목록 확인, 고양이 교체/방출/부활 (부활 클릭 시 `Heal.mp3` 재생) |
| `StageClear` | 스테이지 승리 후 전리품 보상(몬스터볼, 회복약, 기력의 조각) 수령 |
| `NextStage` | 다음 스테이지 진행 처리 및 10스테이지 단위 보스 전원 완치 적용 |
| `GameOver` | 파티 전멸 시 결과 화면 표시 |
| `Victory` | 100스테이지 보스 격파 시 승리 연출 |

---

## 4. 주요 클래스 및 코드 모듈 상세 분석

### 4.1 Core 모듈

#### [`GameManager.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Core/GameManager.cs)
* **역할**: 게임 전체 라이프사이클 관리 및 씬/상태 매니저.
* **주요 기능**:
  - `ChangeState(GameState newState)`: 상태 변경 이벤트 `OnStateChanged` 전파.
  - `StartRun(IReadOnlyList<CatDataSO> starters)`: 스타팅 고양이 검증, 예산 체크 후 런 개시.
  - Global Input Handling: `New Input System` 및 Legacy `Input` 양방향 감지로 [SPACE], [H], [P], [ESC] 키 입력 처리.
  - `TryUsePotion()`: [H] 키 입력 시 출전 중인 냥코의 체력 50% 회복 및 `SoundManager.Instance?.PlayDrinkSfx()` 1회성 효과음 연동.
  - 전투/포획 결과 수신에 따른 파티 관리 및 다음 스테이지 전환 중계.

---

### 4.2 Battle & Catch 모듈

#### [`BattleManager.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Battle/BattleManager.cs)
* **역할**: 아군 고양이 vs 적 야생 고양이 간 턴제 오토 배틀 루틴.
* **주요 기능**:
  - `AutoBattleLoop()`: 스피드(`Speed`)가 높은 객체가 선공. 턴 간 딜레이(`turnDelay = 1.2s`) 적용.
  - `ExecuteAttackAndWait()`: 데미지 난수 계산(`0.85 ~ 1.15`), 피격 텍스트 로그 생성 및 UI 연출 대기 (`SoundManager` 타격 `slap.mp3` / 피격 `ouch.mp3` 재생).
  - `ResumeAfterPlayerSwitch()`: 전투 중 고양이 교체 시 내 턴 소모 여부에 따른 적 페널티 공격 루틴(`EnemyFreeAttackAfterSwitch`) 수행.

#### [`CatchManager.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Battle/CatchManager.cs)
* **역할**: 야생 고양이 포획 및 인벤토리(몬스터볼, 회복약, 기력의 조각) 드랍 관리.
* **주요 공식 & 수치**:
  - **Shake 통과 확률 (`CalculateShakeChance`)**:
    $$P_{\text{shake}} = \text{Lerp}(\text{fullHpChance}, \text{lowHpChance}, (1 - \text{HpRatio})^{0.55}) - (\text{Rarity} \times 0.025)$$
  - **3-Shake 연출 및 포획 신호 오디오**:
    - 3번의 확률 체크를 모두 통과해야 최종 포획 성공.
    - 3번째 Shake 판정이 성공하는 바로 그 순간(`shake == 3 && passed`), `SoundManager.Instance?.PlayCatchSfx()`로 `Catch.mp3` 오디오 피드백 연동.
  - **전리품 드랍 (`RollVictoryDrops`)**:
    - **몬스터볼**: 10% 확률 획득
    - **회복약**: 10% 확률 획득 (사용 시 HP 50% 회복)
    - **기력의 조각**: 10% 확률 획득 (파티 화면에서 쓰러진 냥코 **HP 50% 부활**)

---

### 4.3 Party & Stage 모듈

#### [`PartyManager.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Party/PartyManager.cs)
* **역할**: 플레이어 파티(최대 6마리 슬롯) 관리.
* **주요 기능**:
  - `AddCat()`, `SwapCat()`, `ReleaseCat()`: 파티 추가/교체/방출. (최소 1마리 보존)
  - `SetActiveCat(int index)`: 선두 출전 고양이 지정.
  - `FullHealAll()`: 보스 스테이지 클리어 시 파티 전체 HP 완치.

#### [`StageManager.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Stage/StageManager.cs)
* **역할**: 1 ~ 100 스테이지 난이도 백본 및 보스 생성.
* **주요 기능**:
  - `GenerateBackbone(seed)`: 300종 고양이 중 100종을 시드 기반 무작위 추출 후 도감 번호 오름차순 정렬.
  - `GenerateEnemyForStage()`: 해당 스테이지의 백본 고양이 스폰. 10스테이지 단위는 보스(`IsBoss = true`, **HP 2배** 적용).

---

### 4.4 Data & Progression 모듈

* **[`CatDataSO.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Data/CatDataSO.cs)**: 도감 번호(`dexNo`), 한/영 이름, 기본 HP/ATK/Speed, 희귀도, 스프라이트 ScriptableObject.
* **[`CatDatabaseSO.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Data/CatDatabaseSO.cs)**: 전체 100~300종 `CatDataSO` 에셋 저장소 및 빠른 조회 메서드.
* **[`CatInstance.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Data/CatInstance.cs)**: 런타임 개체 (레벨 스케일링, 보스 배율, 실시간 HP 관리).
* **[`CatRarity.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Data/CatRarity.cs)**: `Basic` ~ `Legend` 6개 등급, 스타팅 코스트(1~7) 및 UI 등급 색상 정의.
* **[`CatUnlockProgress.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Data/CatUnlockProgress.cs)**: `PlayerPrefs`를 활용한 스타팅 해금 목록 및 포획 횟수 영구 기록.

---

### 4.5 UI & Localization 모듈

* **[`BattleUI.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/UI/BattleUI.cs)**: 플레이어/적 스탠딩 스프라이트(플레이어 냥코 출전 시 X 스케일 -1 적용으로 적과 마주보기), 체력바, 전투 로그 텍스트 애니메이션 및 스페이스바 포획 / H 키 회복약 / V 키 보유 포켓몬 토글 안내 UI.
* **[`StarterSelectUI.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/UI/StarterSelectUI.cs)**: 예산 10점 제한 기반 6개 스타팅 카드의 자가 생성(Self-Building) 그리드 패널.
* **[`PartyUI.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/UI/PartyUI.cs)**: 전투 화면 좌측 상단 6개 파티 슬롯 수직(위에서 아래로) 정렬 패널.
* **[`PartyManageModalUI.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/UI/PartyManageModalUI.cs)**:
  - 6마리 슬롯 교체/방출 및 **[부활] (REVIVE)** 버튼 지원.
  - 보유 중인 기력의 조각 개수 실시간 표시 및 쓰러진 냥코 HP 50% 부활 처리 (`SoundManager.Instance?.PlayHealSfx()` 1회성 효과음 연동).
* **[`LanguageManager.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/UI/LanguageManager.cs)**:
  - 한국어/영어 동적 언어 전환 매니저.
  - `SkillTranslationMap` 및 `TranslateSkillNameToEnglish`: 300종 대표 스킬 및 키워드 자동 영문 번역.
  - `ContainsKorean`: 영어 모드에서 한국어 문자열 출력을 100% 완전 차단.
* **[`LocalizedText.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/UI/LocalizedText.cs)**: TextMeshProUGUI 컴포넌트에 자동 부착되어 언어 변경 시 텍스트 즉시 갱신.

---

### 4.6 Sound 모듈

#### [`SoundManager.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/SoundManager.cs)
* **역할**: 게임 내 배경음악(BGM) 및 효과음(SFX) 통합 관리.
* **주요 오디오 에셋 및 사운드 피드백 상세**:
  - `BGM.mp3`: 스테이지 오토 배틀 루핑 배경음악.
  - `slap.mp3` (`PlayAttackSfx`): 아군/적 고양이 일반 공격 시 원샷 피드백.
  - `ouch.mp3` (`PlayHurtSfx`): 피격 데미지 수령 시 원샷 피드백.
  - `Drink.mp3` (`PlayDrinkSfx`): **[H] 키 포션 사용** 시 체력 회복과 동시에 1회 재생.
  - `Heal.mp3` (`PlayHealSfx`): **파티 관리 모달에서 [부활] 버튼 클릭** 시 냥코 부활과 동시에 1회 재생.
  - `Catch.mp3` (`PlayCatchSfx`): **몬스터볼 3번째 Shake 성공 시 (`truetruetrue`)** 포획 성공 신호로 그 순간 1회 재생.
* **자동 에디터 바인딩 & 폴백**:
  - 에디터 실행 환경에서 오디오 클립 레퍼런스가 미할당되었을 시 `Assets/Sounds` 내 경로에서 자동으로 세팅.

---

### 4.7 Editor & Utilities 모듈

* **[`CatSpriteRenamerWindow.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Editor/CatSpriteRenamerWindow.cs)**:
  - 100종 스프라이트 시트 meta 파일(`internalIDToNameTable`, `nameFileIdTable`)의 서브 스프라이트 네이밍을 `cat_1 ~ cat_300`으로 일괄 동기화하는 커스텀 에디터 윈도우.
* **[`MainGameSceneBuilder.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Editor/MainGameSceneBuilder.cs)**:
  - 메인 게임 씬(`MainGame.unity`)의 Canvas, UI 패널(교체/부활/방출 3버튼 레이아웃 포함), EventSystem 및 `SoundManager` 5종 SFX 자동 연동 구성.
* **[`GameAudioSceneConfigurator.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Editor/GameAudioSceneConfigurator.cs)**:
  - `MainGame.unity` 씬 오디오 구성 자동 설정 및 5종 SFX 에셋 할당 상태 자동 검증기.
* **[`CatEncyclopediaImporter.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Editor/CatEncyclopediaImporter.cs)**:
  - 도감 데이터를 기반으로 `CatData_1.asset ~ CatData_300.asset` 에셋 자동 생성 및 깔끔한 영문 이름/스킬 세팅.

---

## 5. 에디터 자동화 & 생산성 툴

본 프로젝트는 에디터 툴을 통해 빌드 및 데이터 세팅을 자동화하였습니다.

1. **Cat Sprite Batch Renamer (`Tools -> Cat Sprite Renamer`)**:
   - `Assets/Image/Cats` 시트 메타데이터 자동 재구성 (Y축 높이 Top→Bottom, X축 좌→우 5×5 그리드 기반 26~50번 스프라이트 네이밍 정렬 적용).
2. **Main Game Scene Auto Builder (`Tools -> Build Main Game Scene`)**:
   - 클릭 한 번으로 메인 게임 씬의 모든 UI 패널, SoundManager 사운드 에셋 및 C# 매니저 컴포넌트 자동 배치.
3. **Configure Game Audio (`Tools -> PocketRoguelike -> Configure Game Audio`)**:
   - `BGM.mp3`, `slap.mp3`, `ouch.mp3`, `Drink.mp3`, `Heal.mp3`, `Catch.mp3` 에셋 바인딩 및 씬 저장/검증 일괄 실행.

---

## 6. 주요 버그 및 트러블슈팅 내역

| 버그 증상 | 원인 분석 | 해결 조치 내용 |
| :--- | :--- | :--- |
| **Active Input Handling Exception** | `ProjectSettings.asset`이 New Input System 전용으로 설정되어 Legacy `Input.GetKeyDown` 호출 시 예외 발생. | `activeInputHandler`를 `2 (Both)`로 수정하고, Keyboard/Legacy 이중 감지 로직 구현. |
| **Play Mode 진입 시 에디터 예외** | 에디터 스크립트의 `[InitializeOnLoad]` 속성으로 인해 플레이 진입 시 씬 재생성 시도. | `[InitializeOnLoad]` 자동 실행 구문 제거 및 메뉴 항목 기반 실행으로 전환. |
| **Duplicate Identifier 300000 씬 오류** | YAML 직접 편집 과정에서 FileID 중복 할당 발생. | C# 네이티브 씬 빌더(`MainGameSceneBuilder`)를 제작하여 표준 유니티 API로 씬 재구성. |
| **CLI 실행 시 유니티 에디터 강제 종료** | PowerShell 강제 종료 명령어 사용으로 에디터 닫힘. | 프로세스 종료 명령을 완전 제거하고 배경 파일 수정 방식으로 전환. |
| **영어 모드에서 한글 스킬명/이름 노출** | `catNameEnglish` 및 `skillNameEnglish` 데이터에 한글 원본이 포함되어 노출됨. | `ContainsKorean` 검증 및 `SkillTranslationMap` 영문 번역기를 도입하여 영어 모드 시 100% 영문으로만 표기되도록 수정. |
| **스프라이트 시트 노출 소실 오류** | `.meta` 파싱 시 `spriteSheet:` 하단의 `sprites:` 배열 키워드가 누락되어 서브 스프라이트가 비활성화됨. | YAML 파싱 스크립트에 `sprites:` 키워드 보존 및 25개 수량 검증(Safety Check) 구문 추가. |
| **스프라이트 그리드 순서 꼬임** | 행 내부 Y좌표의 미세한 오차(±10px)로 인해 단순 Y 내림차순 정렬 시 열(Column) 순서가 뒤섞임. | Y좌표 클러스터링(80px 높이 행 단위 그룹핑) 후 행 내부 X 오름차순 2차 정렬 알고리즘 적용하여 26~50번 그리드 완벽 정렬. |

---

## 7. 조작 방법 & 플레이 가이드

### 7.1 인게임 조작 키

| 상황 | 입력 키 | 동작 | 사운드 피드백 |
| :--- | :---: | :--- | :--- |
| **오토 배틀 중** | **`SPACE`** | 몬스터볼 투척 (야생 고양이 포획 시도) | 3-Shake 성공 시 `Catch.mp3` 재생 |
| **오토 배틀 중** | **`H`** | 회복약 사용 (출전 고양이 HP 50% 회복) | `Drink.mp3` 1회 재생 |
| **오토 배틀 중** | **`P`** | 파티 관리 패널 열기 | [부활] 클릭 시 `Heal.mp3` 1회 재생 |
| **오토 배틀 중** | **`V`** | 보유 포켓몬 패널 열기 / 닫기 (토글) | - |
| **파티 관리 / 모달** | **`ESC`** | 패널 닫기 / 이전 화면으로 돌아가기 | - |
| **스타팅 선택 / 결과** | **마우스 클릭** | 고양이 카드 선택 / 부활 및 교체 버튼 클릭 | - |

### 7.2 플레이 순서
1. `Assets/Scenes/MainGame.unity` 씬을 열고 유니티 에디터 상단 **Play(▶)** 버튼을 누릅니다.
2. **스타팅 선택 화면**에서 예산 10점 내로 원하는 3마리 고양이를 선택하고 `START RUN`을 클릭합니다.
3. 배틀 진행 중 야생 고양이 체력이 낮아졌을 때 **`SPACE`** 키를 눌러 포획하거나(3-Shake 포획 성공 시 **`Catch.mp3`**), 체력이 부족하면 **`H`** 키로 회복약을 사용합니다 (**`Drink.mp3`**).
4. 냥코가 기절했을 경우 **`P`** 키를 눌러 파티 화면에서 **기력의 조각**으로 부활시킵니다 (**`Heal.mp3`**).
5. 10스테이지마다 나타나는 보스를 격파하여 파티 전체 회복 혜택을 누르고 **100스테이지 최종 보스**에 도전하세요!

