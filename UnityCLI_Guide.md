# Unity CLI Guide & Usage Reference

Unity CLI는 Unity Hub 없이 독립 실행형 바이너리로 에디터 설치, 모듈 관리, 프로젝트 실행 및 빌드를 자동화할 수 있는 차세대 명령줄 도구(Beta)입니다.

---

## 1. Unity CLI 주요 특징 및 차이점 (기존 Hub CLI 대비)

| 구분 | 기존 Hub CLI (`-- --headless`) | 신규 Unity CLI (`unity`) |
| :--- | :--- | :--- |
| **의존성** | Unity Hub 데스크톱 앱 필수 | **독립형 단일 바이너리 (Hub 불필요)** |
| **명령어 형식** | `"Unity Hub.exe" -- --headless <cmd>` | `unity <cmd>` |
| **출력 형식** | Plain Text 고정 | 터미널 대화형: `human`<br>파이프/자동화: `tsv`, `json` |
| **에러 출력** | `stdout`으로 출력 | `stderr`로 분리 출력 (CI/CD 파이프라인 친화적) |
| **추가 기능** | 기본 설치 기능 위주 | `auth` 로그인/아웃, `projects` 프로젝트 관리, `upgrade` 자가 업데이트 등 |

---

## 2. 설치 및 업데이트 방법

### Windows (PowerShell)
```powershell
$env:UNITY_CLI_CHANNEL='beta'
irm https://public-cdn.cloud.unity3d.com/hub/prod/cli/install.ps1 | iex
```
* 설치 위치: `%LOCALAPPDATA%\Unity\bin\unity.exe`

### macOS / Linux (Bash)
```bash
curl -fsSL https://public-cdn.cloud.unity3d.com/hub/prod/cli/install.sh | UNITY_CLI_CHANNEL=beta bash
```

### CLI 자가 업데이트
```shell
unity upgrade
```

---

## 3. 핵심 명령어 요약

### (1) 설치된 에디터 조회
```shell
# 설치된 에디터 목록 확인
unity editors -i

# 출시된 에디터 버전 목록 확인
unity editors -r

# JSON 형식으로 에디터 목록 출력
unity editors -i --format json
```

### (2) 에디터 및 모듈 설치
```shell
# 최신 LTS 에디터 설치
unity install lts

# 특정 에디터 버전 및 모듈(iOS, Android, WebGL) 동시 설치
unity install 6000.3.7f1 -m ios android webgl

# 기존 설치된 에디터에 모듈 추가
unity install-modules -e 6000.3.7f1 -m android --cm
```

### (3) 프로젝트 조회 및 실행
```shell
# Hub에 등록된 프로젝트 목록 보기
unity projects list

# 특정 프로젝트 상세 정보 (에디터 버전, 패키지 목록, Render Pipeline 등)
unity projects info ./PocketRoguelike

# 프로젝트 열기 (해당 프로젝트의 Unity 에디터 버전 자동 감지)
unity open ./PocketRoguelike
# 또는 줄여서
unity ./PocketRoguelike
```

### (4) 계정 인증 (Auth)
```shell
unity auth login     # 브라우저 연동 로그인
unity auth status    # 로그인 상태 확인
unity auth logout    # 로그아웃
```

---

## 4. 로컬 설치 검증 결과

- **설치 버전**: `Unity CLI 1.0.0-beta.2` (win32-x64)
- **설치 경로**: `C:\Users\PC\AppData\Local\Unity\bin\unity.exe`
- **감지된 에디터**:
  - `6000.3.18f1` (Platforms: Web)
  - `6000.0.59f2`
- **프로젝트 상세 정보 검증**: `PocketRoguelike` (Unity `6000.3.18f1`, URP, 52개 패키지 확인 완료)

---

## 5. 2026-07-24 CLI 실행 검증

공식 Unity CLI 문서의 설치·에디터 조회·프로젝트 조회 흐름을 이 프로젝트에서 실제로 실행해 확인했다.

```powershell
$unity = "$env:LOCALAPPDATA\Unity\bin\unity.exe"

& $unity --version
& $unity editors -i --format json
& $unity projects info "C:\UnityProject\PocketRoguelike" --format json
```

- **CLI 버전**: `1.0.0-beta.2`
- **프로젝트 에디터**: `6000.3.18f1` (`C:\Program Files\Unity\Hub\Editor\6000.3.18f1\Editor\Unity.exe`)
- **프로젝트**: `PocketRoguelike`, URP, `StandaloneWindows64` 타겟
- **설치된 추가 모듈**: Web

CLI는 실험적 기능이므로 자동화에서는 `--format json`을 사용하고, 설치·업데이트 명령(`unity install`, `unity upgrade`, `unity install-modules`)은 에디터와 모듈을 변경하므로 실행 전에 필요한 버전과 모듈을 확인한다.
