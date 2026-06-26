# Adventure P0 안정성 기준 및 검증 보고서

작성일: 2026-06-27 KST
대상 프로젝트: `JsonFile` Unity 프로젝트
검증 환경: Unity 2022.3.62f3, WindowsEditor, Android 플랫폼 설정

## 1. P0 안정성 기준

P0는 게임 실행과 저장 데이터 호환성을 직접 깨뜨릴 수 있는 영역으로 한정한다. 대규모 구조 변경, 데이터 파이프라인 변경, 기존 ID 체계 변경은 P0 안정성 작업 범위에 포함하지 않는다.

### P0 기준

| 구분 | P0 기준 | 자동화 우선 유형 |
|---|---|---|
| 저장 데이터 호환성 | 기존 저장 파일을 읽을 수 있어야 하며 `SaveManager.SaveData`의 공개 필드명이 유지되어야 한다. | EditMode |
| JSON 로드 | `Resources/Events` JSON 루트 키와 행 수가 기대값을 유지하고, 런타임 파서가 필요한 테이블을 로드해야 한다. | EditMode |
| 옵션/효과 ID 연결 | `Option_ID`, `Effect_ID`, `Option_Type`, `MonPas_EffectN`, `EffectN_Stat`가 런타임 OptionManager/MonsterOptionManager 흐름에 연결되어야 한다. | EditMode |
| 빌드 씬 진입 | `EditorBuildSettings`의 필수 씬이 활성화된 상태로 순서를 유지해야 한다. | EditMode, PlayMode |
| 기본 전투 흐름 | 핵심 런타임 타입, 전투 옵션 적용, 몬스터 패시브, 전투 시작/적중 옵션이 예외 없이 동작해야 한다. | EditMode, PlayMode |

### 공개 계약

| 공개 계약 | 유지 기준 |
|---|---|
| `SaveManager.SaveData` 필드명 | 기존 public 필드명 삭제/이름 변경 금지. `savedCurrentEvetnGroupIndex` 오탈자 형태도 저장 호환성상 현재 계약으로 본다. |
| `Option_ID`, `Effect_ID`, `Option_Type`, `MonPas_EffectN`, `EffectN_Stat` | Excel/JSON 작성 필드명을 유지하고, 런타임에서 ID 조회 또는 트리거 해석이 가능해야 한다. |
| `JsonManager` 조회 API | `GetWeaponById`, `GetArmorById`, `GetItemDataFromCode`, `GetOptionById`, 테이블 리스트 조회 API의 의미를 유지한다. |
| `EditorBuildSettings` 빌드 씬 구성 | `LobbyScenes` → `GameScene` → `GameEndingScene` 순서와 enabled 상태를 유지한다. |

## 2. 수정 전 기준선

코드 수정 전에 Unity Test Runner와 빌드 씬, 콘솔 상태를 먼저 확인했다.

| 항목 | total | passed | failed | skipped | inconclusive | 결과 |
|---|---:|---:|---:|---:|---:|---|
| 수정 전 EditMode 전체 | 67 | 67 | 0 | 0 | 0 | Passed |
| 수정 전 PlayMode 전체 | 4 | 4 | 0 | 0 | 0 | Passed |
| 수정 전 콘솔 오류/경고 | 0 | 0 | 0 | 0 | 0 | 오류/경고 없음 |

수정 전 `EditorBuildSettings` 구성:

| buildIndex | 씬 | 상태 |
|---:|---|---|
| 0 | `Assets/Scenes/LobbyScenes.unity` | enabled |
| 1 | `Assets/Scenes/GameScene.unity` | enabled |
| 2 | `Assets/Scenes/GameEndingScene.unity` | enabled |

수정 전 위험 징후:

| 위험 | 기준선 분류 | 조치 |
|---|---|---|
| `SaveManager.SaveGame()`이 비게임 씬에서 `showPatchNoteToggle` null 접근 가능 | 잠재 코드 결함 | 최소 수정 |
| `SaveManager.EnterGameScene()`에서 `SceneFader`가 없으면 `GameScene` 로드가 주석 처리되어 있음 | 잠재 코드 결함 | 최소 수정 |
| 저장 파일 읽기/쓰기 예외가 명확히 처리되지 않음 | 잠재 코드 결함 | 최소 수정 |
| `Option_006`, `Option_007`의 `Option_Type`이 JSON에서 null | 기존 데이터 상태. 현재 기존 테스트는 허용 | 남은 위험/데이터 정리 후보 |

## 3. 최소 수정 내역

수정 파일:

| 파일 | 변경 내용 | P0 영향 |
|---|---|---|
| `Assets/Script/Save_Load/SaveManager.cs` | 기존 저장 데이터의 `showPatchNoteToggle` 값을 보존하고, 토글 참조가 null이어도 저장이 진행되도록 방어 | 저장 호환성 보호 |
| `Assets/Script/Save_Load/SaveManager.cs` | `ReadSaveFile`, `WriteLoadFile` 읽기 경로에 빈 파일/파싱/IO 실패 로그와 null 반환 처리 추가 | 저장 파일 손상 시 명확한 실패 처리 |
| `Assets/Script/Save_Load/SaveManager.cs` | `WriteSaveFile(null)` 방어 및 쓰기 예외 로그 처리 추가 | 저장 파일 손상 방지 |
| `Assets/Script/Save_Load/SaveManager.cs` | `SceneFader`가 없을 때 `SceneManager.LoadScene("GameScene")` fallback 복구 | 씬 의존성 방어 |
| `Assets/Tests/EditMode/P0DataCompatibilityTests.cs` | 저장 필드명, 빌드 씬 순서, Option Effect 연결, OptionEffect 작성 데이터 연결 TC 추가 | 공개 계약 자동 검증 |

변경하지 않은 항목:

- `SaveManager.SaveData` 필드명
- `Option_ID` / `Effect_ID` / `Option_Type` 계열 데이터 계약
- Excel → JSON → JsonManager → Runtime 흐름
- 기존 P0 테스트의 기대 의미
- 빌드 씬 구성

## 4. 무결성 테스트 결과

수정 후 첫 EditMode 실행에서 새 TC 1개가 실패했다. 원인은 현재 데이터의 `Option_006`, `Option_007`이 `Option_Type: null`인 기존 상태를 새 테스트가 P0 실패로 과도하게 본 테스트 결함이었다. 테스트 기준을 “필드명/Effect 연결 유지, 비어 있지 않은 Option_Type은 알려진 값이어야 함”으로 좁힌 뒤 재검증했다.

| 항목 | total | passed | failed | skipped | inconclusive | 결과 |
|---|---:|---:|---:|---:|---:|---|
| 수정 후 EditMode 전체 최종 | 71 | 71 | 0 | 0 | 0 | Passed |
| 수정 후 PlayMode 전체 최종 | 4 | 4 | 0 | 0 | 0 | Passed |
| 수정 후 콘솔 오류/경고 최종 | 0 | 0 | 0 | 0 | 0 | 오류/경고 없음 |

환경 이슈:

| 항목 | 분류 | 처리 |
|---|---|---|
| Unity MCP refresh 중 `Cannot access a disposed object` 콘솔 오류 1회 | 환경 문제 | 프로젝트 컴파일 오류가 아니며 콘솔 정리 후 최종 콘솔 0건 확인 |

## 5. 자동 TC 목록

이번 작업에서 새로 생성하거나 보강한 자동 TC만 상세 기록한다. 기존 67개 EditMode와 4개 PlayMode 기준선 TC는 수정 전/후 전체 실행 결과로 검증했다.

| TC ID | 목적 | 사전 조건 | 테스트 데이터 | 수행 절차 | 기대 결과 | 자동화 유형 |
|---|---|---|---|---|---|---|
| P0-EDIT-SAVE-001 | 저장 데이터 필드명 호환성 고정 | 테스트 어셈블리에서 런타임 타입 접근 가능 | `SaveManager.SaveData` public fields | reflection으로 public 필드명 집합을 읽고 필수 필드명이 모두 포함되는지 확인 | 기존 저장 필드명이 삭제/변경되지 않음 | EditMode |
| P0-EDIT-SCENE-001 | 빌드 씬 공개 계약 고정 | Editor 환경 | `EditorBuildSettings.scenes` | enabled 씬 앞 3개를 읽어 `LobbyScenes`, `GameScene`, `GameEndingScene` 순서 확인 | 필수 씬이 활성화되고 순서 유지 | EditMode |
| P0-EDIT-OPT-001 | Option Effect 연결 검증 | `Option_Master.json` 로드 가능 | `Option_ID`, `Effect_ID`, `Option_Type` | 런타임 파서로 Option rows를 읽고 `Effect_ID`가 OptionManager 등록 effect에 존재하는지 확인 | 잘못된 Effect_ID를 실패로 감지 | EditMode |
| P0-EDIT-OPT-002 | OptionEffect 작성 데이터와 런타임 Option 연결 검증 | `Option_Master.json`, `OptionEffect_Master.json` 로드 가능 | `OptionEffect_Master.Option_ID` | OptionEffect rows의 `Option_ID`가 Option_Master에 존재하는지 확인 | 작성 데이터가 없는 Option을 참조하면 실패 | EditMode |

## 6. 추가 TC 목록

1차 검증 후 보강이 필요한 항목은 아래와 같다. 이번 턴에서 실행하지 않은 항목은 “계획” 또는 “권장 검증”으로만 기록한다.

| TC ID | 우선순위 | 목적 | 사전 조건 | 테스트 데이터 | 수행 절차 | 기대 결과 | 자동화 유형 | 상태 |
|---|---|---|---|---|---|---|---|---|
| P0-EDIT-SAVE-002 | P0 | 저장 경계값 역직렬화 | 저장 경로 격리 seam 필요 | 최소 저장 JSON, 누락 필드 JSON, 알 수 없는 추가 필드 JSON | 격리된 저장 경로에서 `ReadSaveFile` 호출 | 누락 필드는 기본값, 추가 필드는 무시, 파싱 불가 파일은 명확히 null/오류 로그 | EditMode | 계획 |
| P0-EDIT-JSON-001 | P0 | 필수 JSON 루트 누락 감지 | Resources fixture 또는 임시 TextAsset 대체 필요 | 루트 키가 다른 JSON | `JsonRuntimeTableParser.TryParseList` 호출 | 루트 누락 오류 메시지 반환 | EditMode | 계획 |
| P0-PLAY-SAVE-001 | P0 | 로드 버튼의 `SceneFader` 없는 fallback 검증 | 저장 파일 경로 격리 필요 | 유효 저장 파일 | `OnClickLoadGame` 후 씬 전환 대기 | `GameScene` 진입 | PlayMode | 계획 |
| P0-PLAY-RUNTIME-001 | P0 | `GameScene` 초기화 후 JsonManager/OptionManager 준비 상태 확인 | PlayMode 씬 로드 가능 | 실제 `GameScene` | 씬 로드 후 핵심 매니저 존재/준비 상태 확인 | 필수 매니저가 null이 아니고 런타임 조회 가능 | PlayMode | 계획 |
| MANUAL-P0-001 | P0 보조 | 기본 전투 조작감과 시각적 이상 확인 | 실제 플레이 빌드 또는 Editor Play | 기본 장비/몬스터 | 수동 전투 진행 | 조작 불능, UI 깨짐, 체감 밸런스 이상 기록 | 수동 QA | 권장 검증 |

## 7. 검증 진행 및 실패 분류

| 발생 항목 | 감지 단계 | 분류 | 최종 처리 |
|---|---|---|---|
| 새 테스트가 `Option_006`의 null `Option_Type`을 실패로 판정 | 수정 후 첫 EditMode | 테스트 결함 | 현재 계약에 맞게 테스트 기준 조정 후 통과 |
| `Option_Master.Effect_ID`를 `Effect_999`로 임시 변경 | 의도적 오류 주입 | 테스트 신뢰성 검증용 임시 결함 | `P0-EDIT-OPT-001` 실패 감지 확인 후 원복, 재통과 |
| `OptionEffect_Master.Option_ID`를 `Option_999`로 임시 변경 | 의도적 오류 주입 | 테스트 신뢰성 검증용 임시 결함 | `P0-EDIT-OPT-002` 실패 감지 확인 후 원복, 재통과 |
| Unity MCP refresh disposed object 오류 | 에셋 refresh | 환경 문제 | 최종 콘솔 0건, 테스트 통과 |

기존 실패와 수정으로 생긴 실패 구분:

- 수정 전 EditMode/PlayMode 실패는 없었다.
- 수정 후 실제 코드 결함으로 확인된 실패는 없었다.
- 새 테스트 초기 실패는 테스트 기준 과잉으로 분류했고, 코드/데이터 변경 없이 테스트 설계를 조정했다.
- 의도적 오류 주입 실패는 품질 증명을 위한 기대 실패이며 최종 상태에 남기지 않았다.

## 8. 의도적 오류 주입 검증 결과

| 주입 ID | 임시 변경 | 실행 TC | 기대 실패 메시지 요약 | 원복 후 결과 |
|---|---|---|---|---|
| INJECT-OPT-001 | `Option_Master.json`의 `Option_001.Effect_ID`를 `Effect_999`로 변경 | `P0DataCompatibilityTests.OptionMasterEffectIdsResolveToRegisteredRuntimeEffects` | `Option_001:Effect_999`가 등록 effect에 없음 | 동일 TC 1/1 Passed |
| INJECT-OPT-002 | `OptionEffect_Master.json`의 첫 `Option_ID`를 `Option_999`로 변경 | `P0DataCompatibilityTests.OptionEffectAuthoringRowsTargetExistingRuntimeOptions` | `OptionEffect_Master references missing Option_ID: Option_999` | 동일 TC 1/1 Passed |

주의:

- 두 오류 주입은 실제 프로젝트에 남기지 않았다.
- 최종 전체 EditMode/PlayMode는 원복 후 실행했다.

## 9. 남은 위험과 수동 QA 필요 항목

| 위험 | 영향 | 권장 대응 |
|---|---|---|
| `Option_006`, `Option_007`의 `Option_Type`이 null | 현재 런타임은 기존 테스트상 허용하지만 데이터 의미가 불명확함 | Excel 원본 기준으로 one-shot 아이템의 공식 타입 정책 정리 |
| 저장 파일 경로가 `Application.persistentDataPath`에 고정 | 자동 TC에서 실제 사용자 저장 파일을 건드릴 위험 | Editor 전용 저장 경로 격리 seam 추가 검토 |
| `GameScene` 실제 UI/전투 체감 | 자동 테스트만으로 재미, 난이도, 시각적 이상 판단 불가 | 수동 QA 시나리오로 조작감/밸런스/화면 이상 기록 |
| Excel → JSON 생성기 인코딩/필드 누락 | 현재 JSON은 통과하지만 재생성 시 회귀 가능 | Excel 재생성 직후 P0 EditMode 전체 실행을 릴리스 게이트로 사용 |

## 10. 다음 우선순위

1. 저장 경로 격리 seam을 Editor 전용으로 추가한 뒤 `P0-EDIT-SAVE-002`, `P0-PLAY-SAVE-001`을 자동화한다.
2. Excel 원본에서 `Option_006`, `Option_007`의 `Option_Type` 정책을 결정하고, 결정 후 데이터/테스트 기준을 함께 고정한다.
3. 누락 JSON, 잘못된 ID, 옵션 조합, 초기화 순서에 대한 작은 fixture 기반 EditMode TC를 추가한다.
4. 실제 플레이 기준 수동 QA는 전투 1회, 로비 시작, 저장/로드, 패치노트 표시, 장비 옵션 체감 순서로 진행한다.
