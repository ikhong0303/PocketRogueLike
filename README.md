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
    - **[H] 키 회복약**: 전투 중 출전 냥코의 **최대 HP 50% 회복** (마시는 소리 `Drink.mp3` 재생과 동시에 HP바 즉시 상승).
    - **기력의 조각 (10% 드랍)**: 파티 관리(`P`)에서 쓰러진 냥코를 **체력 50%로 부활** (전용 `Heal.mp3` 사운드 즉시 1회 재생).
  - **사용자 편의 인터페이스**:
    - **[V] 키 파티 창 토글 기억**: 한번 닫으면 다음 적 출현 시에도 계속 닫힘 유지 (인벤토리/스킬 창 방식).
    - **스타팅 0마리 자율 선택**: 시작 시 기본 자동 선택을 없애고 유저가 원하는 냥코를 0~6마리(예산 10점 내) 직접 조합.
  - **생동감 있는 오디오 피드백 (Sound Effects)**:
    - **포획 성공 신호**: 3번째 Shake 판정이 성공하는 순간(`truetruetrue`) `Catch.mp3` 1회 재생.
    - **전투 타격/피격**: 공격 시 `slap.mp3`, 피격 시 `ouch.mp3` 원샷 사운드 재생.
  - **무작위 100스테이지 백본**: 런 시작 시 300종 중 100종을 추출·정렬하여 스테이지별 난이도 상승 곡선 생성.
  - **100% 완전 다국어(한/영) 실시간 전환 지원**:
    - `LanguageManager` 기반 스킬명 사전 및 키워드 자동 영문 번역기 탑재.
    - 영어 모드 선택 시 한국어 글자 0% 완전 배제 보장.

---

## 2. 핵심 루프 & 게임 규칙

### 2.1 런 루프 (In-Run Loop)
```
[스타팅 고양이 직접 선택 (0~6마리, 예산 10점 이하)]
                 │
                 ▼
[스테이지 진입 (1 ~ 100 Stage)] ◄──────────┐
                 │                          │
         [오토 턴제 전투]                   │
          ├─ [SPACE] ➔ 고양이 캐치 (3-Shake 성공 시 Catch.mp3 재생)
          ├─ [H] ➔ 회복약 (최대 HP 50% 즉시 회복 & Drink.mp3 재생)
          ├─ [V] ➔ 파티 정보 창 (토글 상태 기억)
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
| `StarterSelect` | 런 시작 전 스타팅 고양이 선택 패널 (유저 자율 선택, 예산 10점 제한) |
| `StageBattle` | 오토 배틀 진행 중. [SPACE] 포획, [H] 회복약 사용, [V] 파티 창 토글, [P] 파티 관리 진입 |
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
  - Global Input Handling: `New Input System` 및 Legacy `Input` 양방향 감지로 [SPACE], [H], [P], [V], [ESC] 키 입력 처리.
  - `TryUsePotion()`: [H] 키 입력 시 출전 중인 냥코의 최대 체력 50% 회복 및 사운드(`Drink.mp3`) 재생과 동시에 화면 UI 즉시 갱신 (`BattleUI.RefreshPlayerHp()`).

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
  - **3-Shake 연출 및 포획 신호 오디오**: 3번째 Shake 판정이 성공하는 순간(`shake == 3 && passed`), `Catch.mp3` 피드백 연동.
  - **회복약 (`UsePotion`)**: 고양이 최대 HP의 정확히 50% 양 회복 (예: 1,000 HP ➔ +500, 20,000 HP ➔ +10,000).

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

* **[`BattleUI.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/UI/BattleUI.cs)**: 플레이어/적 스탠딩 스프라이트, 체력바, 전투 로그 텍스트 애니메이션, `RefreshPlayerHp()`를 통한 즉시 체력 바 갱신.
* **[`StarterSelectUI.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/UI/StarterSelectUI.cs)**: 예산 10점 제한 기반 유저 자율 0~6마리 선택 스타팅 패널.
* **[`PartyUI.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/UI/PartyUI.cs)**: 전투 화면 좌측 상단 6개 파티 슬롯 패널. `IsUserOpen` 토글 상태 기억 지원.
* **[`PartyManageModalUI.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/UI/PartyManageModalUI.cs)**: 6마리 슬롯 교체/방출 및 **[부활] (REVIVE)** 버튼 지원 (쓰러진 냥코 HP 50% 부활 & `Heal.mp3` 재생).
* **[`LanguageManager.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/UI/LanguageManager.cs)**: 한국어/영어 동적 언어 전환 매니저. 영어 모드 시 한국어 문자열 100% 완전 차단.

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

---

### 4.7 Editor & Utilities 모듈

* **[`CatSpriteMappingWindow.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Editor/CatSpriteMappingWindow.cs)**:
  - 300개 고양이 스프라이트를 25개씩 12개 그룹으로 한눈에 보고 직관적으로 드래그 앤 드롭 매핑할 수 있는 커스텀 에디터 윈도우 (`Tools > Cat Sprite Direct Mapping Window`).
  - `cat_1 ~ cat_300` 숫자 이름 자동 파싱 매칭 및 일괄 반영 지원.
* **[`CatSpriteRenamerWindow.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Editor/CatSpriteRenamerWindow.cs)**:
  - 전체 스프라이트 시트 내 서브 스프라이트 네이밍 일괄 수정 윈도우.
* **[`CatEncyclopediaImporter.cs`](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Editor/CatEncyclopediaImporter.cs)**:
  - 도감 데이터를 기반으로 `CatData_1.asset ~ CatData_300.asset` 에셋 자동 생성 및 유연한 커스텀 스프라이트 수동 지정 보존.

---

## 5. 에디터 자동화 & 생산성 툴

본 프로젝트는 에디터 툴을 통해 빌드 및 데이터 세팅을 자동화하였습니다.

1. **Cat Sprite Direct Mapping Window (`Tools -> Cat Sprite Direct Mapping Window`)**:
   - 300마리 고양이의 이미지 스프라이트를 12개 시트 섹션별로 시각적 검증 및 드래그 앤 드롭 매핑 적용.
2. **Cat Sprite Batch Renamer (`Tools -> Cat Sprite Renamer`)**:
   - `Assets/Image/Cats` 시트 메타데이터 서브 스프라이트 네이밍 정렬 적용.
3. **Main Game Scene Auto Builder (`Tools -> Build Main Game Scene`)**:
   - 클릭 한 번으로 메인 게임 씬의 모든 UI 패널, SoundManager 사운드 에셋 및 C# 매니저 컴포넌트 자동 배치.
4. **Configure Game Audio (`Tools -> PocketRoguelike -> Configure Game Audio`)**:
   - `BGM.mp3`, `slap.mp3`, `ouch.mp3`, `Drink.mp3`, `Heal.mp3`, `Catch.mp3` 에셋 바인딩 및 씬 저장/검증 일괄 실행.

---

## 6. 주요 버그 및 트러블슈팅 내역

| 버그 증상 | 원인 분석 | 해결 조치 내용 |
| :--- | :--- | :--- |
| **회복약 사용 시 소리만 나고 HP바 안 올라감** | 포션 사용 시 데이터 수치(HP)는 상승하나 `BattleUI` 및 `PartyUI`의 갱신 함수가 호출되지 않음. | `BattleUI.RefreshPlayerHp()` 및 `PartyUI.RefreshPartyUI()`를 포션 마시는 사운드 재생 시점에 즉시 동시 호출하도록 연동. |
| **스테이지 변경 시 [V] 키로 닫은 파티 창이 재오픈됨** | `UIManager`가 스테이지 전환 시 `partyPanel.SetActive(true)`를 강제로 호출함. | `PartyUI.IsUserOpen` 유저 토글 상태를 저장하도록 하여 닫아둔 경우 다음 적이 나와도 계속 닫힘 유지. |
| **게임 시작 시 고양이 3마리 기본 자동 선택됨** | `StarterSelectUI` 진입 시 `SelectDefaultStarters()`가 구동하여 3마리를 강제 선택함. | 기본 자동 선택 구문을 제거하여 0마리 상태에서 유저가 직접 1~6마리 자율 선택 가능하도록 수정. |
| **CS0618 obsolete 경고** | `TextureImporter.spritesheet` 구버전 API 사용 경고. | `#pragma warning disable 0618` 구문으로 차단하여 에디터 콘솔 깔끔하게 유지. |
| **MenuItem 중복 등록 경고** | `SoundManagerBuilder`와 `SoundManagerAutoBuilder`에 동일 메뉴 경로 존재. | `Tools/Build SoundManager Scene Auto`로 경로 분리하여 메뉴 중복 경고 해결. |
| **Active Input Handling Exception** | `ProjectSettings.asset`이 New Input System 전용으로 설정되어 Legacy `Input.GetKeyDown` 호출 시 예외 발생. | `activeInputHandler`를 `2 (Both)`로 수정하고, Keyboard/Legacy 이중 감지 로직 구현. |
| **영어 모드에서 한글 스킬명/이름 노출** | `catNameEnglish` 및 `skillNameEnglish` 데이터에 한글 원본이 포함되어 노출됨. | `ContainsKorean` 검증 및 `SkillTranslationMap` 영문 번역기를 도입하여 영어 모드 시 100% 영문으로만 표기되도록 수정. |

---

## 7. 조작 방법 & 플레이 가이드

### 7.1 인게임 조작 키

| 상황 | 입력 키 | 동작 | 사운드 피드백 |
| :--- | :---: | :--- | :--- |
| **오토 배틀 중** | **`SPACE`** | 몬스터볼 투척 (야생 고양이 포획 시도) | 3-Shake 성공 시 `Catch.mp3` 재생 |
| **오토 배틀 중** | **`H`** | 회복약 사용 (출전 고양이 최대 HP 50% 즉시 회복) | `Drink.mp3` 재생과 동시에 HP바 상승 |
| **오토 배틀 중** | **`P`** | 파티 관리 패널 열기 | [부활] 클릭 시 `Heal.mp3` 1회 재생 |
| **오토 배틀 중** | **`V`** | 보유 고양이 파티 창 토글 (열기/닫기 상태 기억) | - |
| **파티 관리 / 모달** | **`ESC`** | 패널 닫기 / 이전 화면으로 돌아가기 | - |
| **스타팅 선택 / 결과** | **마우스 클릭** | 고양이 카드 선택 (0~6마리 자율) / 부활 및 교체 버튼 클릭 | - |

### 7.2 플레이 순서
1. `Assets/Scenes/MainGame.unity` 씬을 열고 유니티 에디터 상단 **Play(▶)** 버튼을 누릅니다.
2. **스타팅 선택 화면**에서 예산 10점 내로 원하는 고양이를 0~6마리 자유롭게 선택하고 `START RUN`을 클릭합니다.
3. 배틀 진행 중 야생 고양이 체력이 낮아졌을 때 **`SPACE`** 키를 눌러 포획하거나(3-Shake 포획 성공 시 **`Catch.mp3`**), 체력이 부족하면 **`H`** 키로 회복약을 사용하여 최대 HP의 50%를 회복합니다 (**`Drink.mp3`**).
4. 냥코가 기절했을 경우 **`P`** 키를 눌러 파티 화면에서 **기력의 조각**으로 부활시킵니다 (**`Heal.mp3`**).
5. 10스테이지마다 나타나는 보스를 격파하여 파티 전체 회복 혜택을 누르고 **100스테이지 최종 보스**에 도전하세요!
