# 🐾 PocketRoguelike 전체 개발 작업 & 버그 해결 종합 보고서

본 문서는 **PocketRoguelike** 프로젝트 진행 과정에서 수행한 Unity CLI 도입, SoundManager 구축, 고양이 100종 스프라이트 툴 제작, 냥코 포켓로그 데모 게임 제작 및 발생했던 각종 버그와 해결 내역을 상세히 정리한 종합 문서입니다.

---

## 1. ⚙️ 유니티 CLI (Unity CLI) 설치 및 환경 구축

### 1.1 설치 및 가이던스 작성
* **설치 경로**: `%LOCALAPPDATA%\Unity\bin\unity.exe` (v1.0.0-beta.2)
* **환경 검증 명령어**:
  - `unity --version` : 설치 버전 확인
  - `unity editors -i` : 컴퓨터에 설치된 유니티 에디터 목록 조회 (`6000.3.18f1` 감지)
  - `unity projects list` & `unity projects info .` : 프로젝트 정보 검사
  - `unity open .` : 현재 CLI 디렉토리 프로젝트를 유니티 에디터로 실행

### 1.2 산출물
* [UnityCLI_Guide.md](file:///c:/UnityProject/PocketRoguelike/UnityCLI_Guide.md): 개발자를 위한 Unity CLI 전체 사용 가이드 문서 작성.
* [.agents/skills/unity-cli/SKILL.md](file:///c:/UnityProject/PocketRoguelike/.agents/skills/unity-cli/SKILL.md): 에이전트 전용 Unity CLI 커스텀 스킬 등록.

---

## 2. 🎵 사운드 관리자 (SoundManager) & 테스트 씬 구축

### 2.1 주요 기능 및 스크립트 구현
* **[SoundManager.cs](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/SoundManager.cs)**: 
  - `Assets/Sounds` 내의 5종 BGM 사운드 파일(`BGM.mp3`, `frog.mp3`, `n2kstudio...mp3`, `ouch...mp3`, `tasty...mp3`)을 슬롯 배열로 관리.
  - 사운드 재생/일시정지/음량 조절/트랙 변경 기능 연동.
  - 플레이 버튼 클릭 시 오디오 클립 미할당으로 소리가 나지 않던 현상 해결 (`m_PlayOnAwake: 1` 및 슬롯 자동 연결).

### 2.2 테스트 씬 제작
* **[Soundmanager.unity](file:///c:/UnityProject/PocketRoguelike/Assets/Scenes/Soundmanager.unity)**: 
  - `soundmanager` 앰티 오브젝트 생성 후 `AudioSource` 및 `SoundManager` 컴포넌트 바인딩 및 오디오 클립 5종 연결 완료.

---

## 3. 🐱 고양이 100종 스프라이트 시트 네이밍 수정 툴 및 세팅

### 3.1 에디터 툴 제작 ([CatSpriteRenamerWindow.cs](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Editor/CatSpriteRenamerWindow.cs))
* `Assets/Image/Cats` 폴더 내 4장의 스프라이트 시트(`1-25.png`, `26-50.png`, `51-75.png`, `76-100.png`) 총 100개 고양이 서브 스프라이트 네이밍 관리.
* **주요 기능**:
  - `Tools -> Cat Sprite Renamer` 에디터 윈도우 메뉴 제공.
  - 100개 크기의 Inspector 배열 지원, 붙여넣기 기능 및 `cat_1` ~ `cat_100` 접두사 자동 생성.
  - `.meta` 파일의 `spritesheet:`, `internalIDToNameTable:`, `nameFileIdTable:` 3개 YAML 섹션 자동 동기화.

### 3.2 고양이 스프라이트 순서 변경 (83번 ➔ 92번 밀림 재정의)
* **요구사항**: 83번 고양이를 92번(`cat_92`)으로 지정하고, 밀린 84~92번 고양이를 `cat_83` ~ `cat_91`로 재정의.
* **적용 결과**: `76-100.png.meta`의 서브 스프라이트 테이블을 수정하여 8번째 스프라이트 ➔ `cat_92`, 9~17번째 스프라이트 ➔ `cat_83` ~ `cat_91`로 업데이트 및 Git 커밋 완료.

---

## 4. 🎮 냥코 포켓로그 시스템 기획서 기반 게임 제작

### 4.1 SOLID 원칙 기반 시스템 아키텍처
* **Data System (`Assets/Scripts/Data/`)**:
  - `CatRarity`: 6개 등급(`Basic`, `EX`, `Rare`, `Unique`, `Epic`, `Legend`) 및 등급별 코스트/색상.
  - `CatDataSO`: 고양이 1종 ScriptableObject (도감번호 1~100, 스탯, 스프라이트 `cat_1~100`).
  - `CatDatabaseSO`: 100종 `CatDataSO` 레지스트리.
  - `CatInstance`: 런타임 인스턴스 (HP, 레벨 스케일링, 보스 2배 HP 적용).
* **Party & Stage (`Assets/Scripts/Party/`, `Assets/Scripts/Stage/`)**:
  - `PartyManager`: 파티(최대 6마리) 관리, 방생/교체, 전체 풀 회복.
  - `StageManager`: 1~100 스테이지 진행, 10단계마다 보스전, 100스테이지 클리어 조건.
* **Battle & Catch (`Assets/Scripts/Battle/`)**:
  - `BattleManager`: Speed(도감 번호 순) 기반 턴 정렬 및 오토 배틀.
  - `CatchManager`: [SPACE] 키 입력 시 움직이는 타이밍 게이지 작동 ➔ Green Zone 정확도 및 적 HP% 기반 포획 성공률 계산.
* **UI System (`Assets/Scripts/UI/`)**:
  - `UIManager`: 전체 UI 패널 중재자(Mediator).
  - `BattleUI`: 아군/야생 고양이 스탠딩 대치, HP바, 레벨, 스테이지 표시.
  - `CatchTimingUI`: 스페이스바 포획 타이밍 게이지 및 팝업.
  - `StarterSelectUI`: 코스트 10점 제한 내 3마리 스타팅 선택 (1, 2, 3번 기본 선택).
  - `PartyManageModalUI`: 6마리 초과 시 방생/교체 모달.
  - `ResultUI`: 100스테이지 클리어 승리 및 패배 화면.

### 4.2 자동화 빌더 스크립트
* **[CatDataAutoGenerator.cs](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Editor/CatDataAutoGenerator.cs)**: `cat_1`~`cat_100` 스프라이트를 스캔하여 `CatData_1.asset`~`CatData_100.asset` 및 `CatDatabase.asset` 자동 생성.
* **[MainGameSceneBuilder.cs](file:///c:/UnityProject/PocketRoguelike/Assets/Scripts/Editor/MainGameSceneBuilder.cs)**: 메인 게임 씬 `Assets/Scenes/MainGame.unity` 생성 및 UI Canvas 구조 자동 빌드.

---

## 5. 🐛 발생했던 주요 버그, 원인 분석 및 해결 현황

| 버그 증상 | 원인 분석 | 해결 조치 내용 | 상태 |
| :--- | :--- | :--- | :---: |
| **`InvalidOperationException: Active Input Handling`** | `ProjectSettings.asset`이 `New Input System` 전용으로 되어있으나 C# 및 UGUI가 구형 `UnityEngine.Input`을 호출함. | `ProjectSettings.asset`의 `activeInputHandler`를 **`2 (Both)`** 로 변경하고, C# 스크립트에 `Keyboard.current` 및 `Input.GetKeyDown` 이중 지원 코드 추가. | **완료** |
| **`InvalidOperationException: This cannot be used during play mode`** | 에디터 스크립트의 `[InitializeOnLoad]` 속성 때문에 플레이 모드 진입 시 씬 생성을 시도함. | `MainGameSceneBuilder` 및 `SoundManagerAutoBuilder`에서 `[InitializeOnLoad]` 자동 실행 제거. | **완료** |
| **스페이스바 입력 시 유니티 에디터가 일시정지(Pause)됨** | 키보드 포커스가 Game View가 아닌 에디터 상단 Toolbar Pause 버튼에 맞춰져 있음. | Game View 마우스 클릭 안내 및 UI EventSystem 포커스 해제 조치. | **완료** |
| **스타팅 선택 화면 카드 목록 비어있음 (아무것도 안나옴)** | `StarterSelectUI`가 Inspector 렌더링에 의존하여 카드 슬롯이 생기지 않음. | `StarterSelectUI.cs`를 자가 생성(Self-Building) 구조로 변경하여 6개 선택 카드 그리드 및 1, 2, 3번 스타팅 자동 선택 적용. | **완료** |
| **`Duplicate Identifier 300000` 씬 로딩 오류 (씬 데이터 없어짐 현상)** | 외부 YAML 텍스트 생성 스크립트에서 ID `300000`을 복수 컴포넌트에 중복 사용하여 유니티가 씬 로딩을 거부하고 빈 `SampleScene`으로 대체함. | 씬 YAML의 모든 FileID를 고유한 정수(101, 201, 301...)로 수정하고 유니티 C# 네이티브 빌더(`MainGameSceneBuilder`)를 구동하여 정상 씬으로 완전 복원. | **완료** |
| **작업 시 유니티 에디터가 강제 종료(크래시)되는 현상** | CLI 작업 과정에서 `Stop-Process -Name Unity -Force` 구문이 실행되어 사용자 에디터가 닫힘. | 유니티 강제 종료 명령어를 완전히 제거하고, 에디터가 열려있는 상태에서 C# 스크립트만 배경 수정하도록 정책 변경. | **완료** |

---

## 📌 현재 상태 요약

* **메인 게임 씬**: **[Assets/Scenes/MainGame.unity](file:///c:/UnityProject/PocketRoguelike/Assets/Scenes/MainGame.unity)**
* **데이터 베이스**: `Assets/Resources/CatDatabase.asset` (100종 고양이 에셋 포함)
* 모든 스크립트 및 씬 데이터가 정상적으로 빌드 및 Git 저장소에 커밋되었습니다. 유니티 에디터에서 `MainGame.unity` 씬을 열고 플레이(▶)를 누르시면 스타팅 선택부터 배틀 및 스페이스바 포획까지 정상 플레이가 가능합니다.
