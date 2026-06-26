# MyHarnessProject와 Adventure QA 책임 경계 재정의 설계

작성일: 2026-06-27  
대상 프로젝트:

- Adventure: `C:\Users\pc\OneDrive\문서\Unity\Adventure\JsonFile`
- MyHarnessProject: `C:\Users\pc\OneDrive\문서\Unity\MyHarnessProject`

## 1. 목적

이 문서는 Adventure 내부 QA와 MyHarnessProject 기반 QA 자동화의 책임을 분리하기 위한 구조 개선안이다.

핵심 목표는 다음과 같다.

- Adventure 내부에는 사람이 승인한 P0/P1 기준 테스트만 유지한다.
- MyHarnessProject는 테스트 후보 생성, 정적 분석, 리포트 생성, TC 초안 생성 역할로 제한한다.
- MyHarnessProject가 생성한 테스트는 Adventure에 즉시 반영하지 않는다.
- 후보 테스트는 QA manifest에서 `Candidate` 상태로 관리한다.
- 사람이 승인한 테스트만 `Adventure/JsonFile/Assets/Tests/EditMode` 또는 `Adventure/JsonFile/Assets/Tests/PlayMode`로 승격한다.
- MyHarnessProject는 Adventure의 NUnit XML/JSON 결과를 읽어 리포트만 생성한다.

## 2. 현재 구조 문제점

### 2.1 승인 테스트와 생성 테스트의 경계가 흐림

현재 Adventure 안에는 승인된 P0 안정성 테스트와 generated test 성격의 파일이 동시에 존재한다.

확인된 주요 경로:

| 구분 | 경로 | 현재 의미 | 문제 |
|---|---|---|---|
| 승인 테스트 중심 | `JsonFile/Assets/Tests/EditMode/P0DataCompatibilityTests.cs` | 저장 호환성, JSON, 옵션, 빌드 씬, 전투 옵션 등 P0/P1 성격의 기준 테스트 | 유지 대상 |
| 생성 테스트 흔적 | `JsonFile/Assets/Editor/AutoGenTests` | MyHarnessProject 또는 자동 생성 계열 테스트 후보 | 승인 여부가 불명확한 상태로 Unity 프로젝트 안에 존재 |

현재 관찰 기준:

- `P0DataCompatibilityTests.cs`: `[Test]` 40개
- `AutoGenTests`: `.cs` 파일 26개, `[Test]` 26개

이 상태가 계속되면 "사람이 승인한 기준 테스트"와 "자동 생성 후보 테스트"가 같은 Unity 프로젝트 안에서 동일한 신뢰도를 가진 것처럼 보일 수 있다.

### 2.2 중복 검증과 결과 해석 혼선

일부 영역은 이미 P0 테스트에서 검증하고 있는데, AutoGenTests에도 비슷한 목적의 테스트가 존재한다.

예시:

- `ChoiceEvaluator`: P0DataCompatibilityTests와 AutoGenTests 양쪽에서 경계값/성공 판정 계열 검증 가능
- `SaveManager`: P0DataCompatibilityTests는 저장 필드명 호환성을 검증하고, AutoGenTests는 실제 저장 round-trip 후보를 포함
- `OptionManager`: P0DataCompatibilityTests는 Option ID와 Effect 연결성을 검증하고, AutoGenTests는 일부 표시 텍스트 중심 검증을 포함
- `EquipmentSystem`: P0DataCompatibilityTests는 실제 옵션 적용 흐름을 검증하고, AutoGenTests는 단편 메서드 반환값 검증을 포함

중복 자체가 문제는 아니지만, 소유권이 없으면 실패 원인을 분류하기 어려워진다.

### 2.3 MyHarnessProject 업데이트가 Adventure 품질 기준을 흔들 수 있음

MyHarnessProject가 계속 개선되면서 generated test의 양과 형태가 바뀌면 다음 문제가 생길 수 있다.

- Adventure 내부 테스트 수가 자동 생성 기준에 따라 계속 변동됨
- 실패가 실제 게임 코드 결함인지, 생성기 오라클 결함인지 구분하기 어려움
- 사람이 승인하지 않은 테스트가 P0/P1 게이트처럼 작동할 수 있음
- 테스트 리포트가 Unity 실제 결과보다 manifest 또는 Excel 집계 기준에 더 의존할 수 있음

### 2.4 manual oracle 영역이 자동 테스트처럼 보일 위험

AutoGenTests에는 "manual fixture", "manual oracle", "explicit expected values"가 필요한 테스트 후보 또는 생략 주석이 포함되어 있다.

이런 항목은 바로 Adventure 테스트로 들어오면 안 된다. 사람이 기대 결과를 명확히 정의하기 전까지는 `Candidate` 또는 `Hold` 상태로 남겨야 한다.

## 3. 목표 구조

### 3.1 책임 분리 원칙

| 영역 | 책임 | 허용 작업 | 금지 작업 |
|---|---|---|---|
| Adventure | 최종 품질 기준 테스트 보관 및 실행 | 승인된 P0/P1 EditMode/PlayMode 테스트 실행 | 미승인 generated test 자동 반영 |
| MyHarnessProject | 후보 생성, 정적 분석, TC 초안, 리포트 생성 | 후보 TC 생성, 코드 위험 분석, NUnit 결과 리포트화 | Adventure 테스트 파일 직접 생성/수정 |
| QA manifest | 테스트 상태와 소유권의 단일 기준 | Candidate/Approved/Hold/Rejected 관리 | 실제 테스트 결과를 임의로 대체 |
| QA 문서/보고서 | 검증 결과와 판단 기록 | 실행 결과 요약, 수동 QA 기록, 리스크 설명 | 승인되지 않은 후보를 통과 테스트처럼 표현 |

### 3.2 목표 흐름

```text
MyHarnessProject
  -> 정적 분석
  -> 테스트 후보 생성
  -> TC 초안 생성
  -> QA manifest에 Candidate 등록

사람 검토
  -> 기대 결과 명확화
  -> P0/P1 여부 판단
  -> EditMode/PlayMode/수동 QA 분류

Approved만 Adventure로 승격
  -> Assets/Tests/EditMode
  -> Assets/Tests/PlayMode

Adventure 테스트 실행
  -> NUnit XML/JSON 결과 생성

MyHarnessProject
  -> NUnit XML/JSON 읽기
  -> 리포트 생성
  -> 결과 기록
```

### 3.3 단일 기준 파일

권장 manifest 위치:

- `JsonFile/memory/QA_Portfolio/qa_manifest.json`

권장 필드:

| 필드 | 설명 |
|---|---|
| `tc_id` | 고유 TC ID |
| `title` | 테스트명 |
| `priority` | P0, P1, P2 등 |
| `status` | Candidate, Approved, Hold, Rejected |
| `owner` | Adventure, MyHarnessProject, ManualQA |
| `execution_target` | EditMode, PlayMode, StaticAnalysis, Manual |
| `source` | Human, MyHarnessGenerated, ExistingAdventureTest |
| `adventure_path` | 승격된 경우 실제 테스트 파일 경로 |
| `harness_source` | 후보 생성에 사용된 MyHarnessProject 산출물 |
| `oracle_status` | Explicit, NeedsFixture, NeedsManualOracle, Invalid |
| `risk_area` | Save, JSON, Option, Scene, Combat, UI 등 |
| `approval_note` | 승인 또는 보류 사유 |
| `last_verified_at` | 마지막 검증일 |
| `result_source` | NUnit XML, JSON, Manual Sheet 등 |

## 4. TC 상태 흐름

### 4.1 상태 정의

| 상태 | 의미 | Adventure 반영 여부 |
|---|---|---|
| `Candidate` | MyHarnessProject 또는 사람이 제안한 테스트 후보 | 반영하지 않음 |
| `Approved` | 사람이 기대 결과, 우선순위, 실행 위치를 승인한 테스트 | 반영 가능 |
| `Hold` | 필요성은 있지만 fixture, 오라클, 씬 격리, 저장 경로 격리 등이 부족한 테스트 | 반영하지 않음 |
| `Rejected` | 중복, 잘못된 기대 결과, 가치 부족, 자동화 부적합으로 폐기한 테스트 | 반영하지 않음 |

### 4.2 상태 전이 규칙

| 전이 | 조건 |
|---|---|
| Candidate -> Approved | 기대 결과가 명확하고, P0/P1 가치가 있으며, EditMode/PlayMode 위치가 결정됨 |
| Candidate -> Hold | 테스트 가치는 있지만 fixture 또는 수동 판단이 필요함 |
| Candidate -> Rejected | 기존 Approved 테스트와 중복되거나, 오라클이 틀렸거나, 검증 가치가 낮음 |
| Hold -> Approved | 부족했던 fixture/오라클/격리 조건이 해결됨 |
| Approved -> Hold | 유지 비용이 크거나 실행 안정성이 낮아져 임시 보류 필요 |
| Approved -> Rejected | 기능 삭제 또는 더 나은 테스트로 대체되어 폐기 |

### 4.3 승인 조건

Adventure 내부 테스트로 승격하려면 아래 조건을 모두 만족해야 한다.

1. P0 또는 P1 우선순위가 명확해야 한다.
2. 기대 결과가 사람이 읽어도 분명해야 한다.
3. 실패 시 실제 제품 리스크를 설명할 수 있어야 한다.
4. 테스트가 사용자 저장 파일, 실제 외부 계정, 실제 네트워크, 실제 결제 상태를 건드리지 않아야 한다.
5. EditMode 또는 PlayMode 중 실행 위치가 명확해야 한다.
6. 기존 Approved 테스트와 목적이 중복되면 기존 테스트 보강으로 처리해야 한다.
7. MyHarnessProject가 생성했다는 이유만으로 승인하면 안 된다.

## 5. Adventure 내부 테스트로 승격할 P0/P1 후보 목록

아래 목록은 현재 구조와 P0 안정성 기준을 기준으로 한 1차 후보이다. `Approved`가 아니라, manifest에 `Candidate`로 등록한 뒤 검토해야 한다.

### 5.1 P0 후보

| 후보 TC ID | 우선순위 | 권장 위치 | 목적 | 현재 근거 | 승인 조건 |
|---|---|---|---|---|---|
| `P0-EDIT-SAVE-002` | P0 | `Assets/Tests/EditMode` | 저장 파일 round-trip과 기본값 보존 검증 | AutoGenTests의 `SaveManager` round-trip 후보, 기존 P0 보고서의 추가 TC 계획 | 테스트 전용 저장 경로 격리와 명확한 fixture 확보 |
| `P0-EDIT-SAVE-003` | P0 | `Assets/Tests/EditMode` | 빈 파일, 손상 JSON, 추가 필드, 누락 필드 처리 검증 | 저장 스키마가 P0 계약 | 실제 사용자 저장 파일을 절대 건드리지 않는 격리 필요 |
| `P0-PLAY-SAVE-001` | P0 | `Assets/Tests/PlayMode` | 저장 로드 후 `GameScene` 진입 fallback 검증 | `SceneFader` 없는 환경 방어가 P0 위험 구간 | PlayMode 씬 로드 fixture와 저장 경로 격리 필요 |
| `P0-PLAY-RUNTIME-001` | P0 | `Assets/Tests/PlayMode` | `GameScene` 초기화 후 `JsonManager`, `OptionManager`, 필수 UI/전투 매니저 준비 상태 확인 | 초기화 순서와 싱글턴 의존성이 버그 위험 구간 | 씬 로드 후 대기 조건과 실패 메시지 명확화 |
| `P0-EDIT-JSON-001` | P0 | `Assets/Tests/EditMode` | 필수 JSON 루트 키 누락 감지 | Excel -> JSON -> JsonManager 파이프라인이 공개 계약 | 임시 JSON fixture 또는 parser 단위 fixture 필요 |
| `P0-EDIT-DATA-001` | P0 | `Assets/Tests/EditMode` | Excel 재생성 후 핵심 row count와 ID 연결 유지 | `Weapon_Master`, `Armor_Master`, `BlackSmith`, `Option_Master`가 런타임 진입점 | row count 정책과 데이터 변경 승인 절차 명시 |
| `P0-EDIT-OPT-003` | P0 | `Assets/Tests/EditMode` | `Option_Type` null/unknown 정책 고정 | `Option_006`, `Option_007`의 null 타입이 남은 위험 | Excel 원본 기준 정책 결정 후 승인 |
| `P0-PLAY-COMBAT-001` | P0 | `Assets/Tests/PlayMode` | 실제 PlayMode에서 기본 전투 1회가 예외 없이 완료되는지 검증 | EditMode의 효과 단위 검증만으로 런타임 씬 흐름을 완전히 보장하지 못함 | 씬, 캐릭터, 몬스터, UI 초기화 fixture 필요 |

### 5.2 P1 후보

| 후보 TC ID | 우선순위 | 권장 위치 | 목적 | 현재 근거 | 승인 조건 |
|---|---|---|---|---|---|
| `P1-EDIT-CHOICE-001` | P1 | `Assets/Tests/EditMode` | `ChoiceEvaluator` 공식 경계값과 성공/실패 분기 고정 | AutoGenTests와 P0DataCompatibilityTests에 유사 검증 존재 | 중복 제거 후 하나의 승인 테스트로 통합 |
| `P1-EDIT-INVENTORY-001` | P1 | `Assets/Tests/EditMode` | 인벤토리 수량 증가/감소/부족 처리 순수 로직 검증 | AutoGenTests에 InventoryManager 후보 존재 | UI 또는 씬 의존성이 없는 순수 fixture로 정리 |
| `P1-EDIT-PLAYERSTATE-001` | P1 | `Assets/Tests/EditMode` | PlayerState 기본 스탯 계산과 경계값 검증 | AutoGenTests에 PlayerState 후보 존재 | 게임 밸런스 변경과 테스트 기대값의 계약 여부 결정 |
| `P1-EDIT-EQUIP-001` | P1 | `Assets/Tests/EditMode` | 장비 착용/해제 시 옵션 중복 등록 방지 | 현재 P0 테스트가 일부 장비 옵션 흐름을 검증 | 기존 P0 테스트와 중복되지 않는 회귀 포인트 정의 |
| `P1-PLAY-UI-001` | P1 | `Assets/Tests/PlayMode` | 패치노트, 인벤토리, 전투 UI의 기본 표시 예외 검증 | UI 계열 AutoGenTests는 대부분 fixture 부족 | 시각 판단은 수동 QA로 남기고 예외/오브젝트 존재만 자동화 |
| `P1-MANUAL-STORY-001` | P1 | 수동 QA | 스토리 선택지 표시, 보상 노드, 라벨 노드 체감 확인 | StoryNavigator 단위 검증은 있으나 실제 표시 UX는 수동 판단 필요 | 자동화하지 않고 수동 시나리오로 관리 |

### 5.3 AutoGenTests 1차 분류 방향

| 파일/영역 | 권장 상태 | 이유 |
|---|---|---|
| `test_SaveManager.cs` | Candidate | 저장 경로 격리만 확실해지면 P0/P1로 승격 가치 있음 |
| `test_ChoiceEvaluator.cs` | Candidate 또는 Rejected | 일부는 이미 승인 테스트와 중복. 중복 제거 후 필요한 항목만 승격 |
| `test_InventoryManager.cs` | Candidate | 순수 로직이면 P1 EditMode로 승격 가능 |
| `test_PlayerState.cs` | Candidate | 스탯 기대값이 공식 계약이면 P1 가능 |
| `test_EquipmentSystem.cs` | Hold | 기대값이 단편적이고 현재 P0 장비 옵션 테스트와 중복 가능 |
| `test_OptionManager.cs` | Hold 또는 Rejected | 표시 텍스트 계약인지 불명확. ID/Effect 연결은 이미 P0에서 검증 |
| UI 계열 테스트 | Hold | PlayMode fixture 또는 수동 QA 판단 필요 |
| 빈 클래스 또는 생략 주석만 있는 파일 | Rejected 또는 Candidate 기록만 유지 | Adventure 내부 테스트 파일로 유지할 가치 낮음 |

## 6. MyHarnessProject에 남길 기능 목록

MyHarnessProject는 Adventure의 최종 테스트 저장소가 아니라 QA 보조 도구로 유지한다.

남길 기능:

| 기능 | 유지 여부 | 설명 |
|---|---|---|
| 정적 분석 | 유지 | 위험 코드, 중복 코드, 복잡도, 테스트 후보 영역 탐지 |
| 테스트 후보 생성 | 유지 | Adventure에 바로 쓰지 않고 manifest `Candidate`로만 등록 |
| TC 초안 생성 | 유지 | 사람이 검토할 수 있는 절차, 기대 결과, 사전 조건 작성 |
| QA manifest 생성/갱신 | 유지 | Candidate/Approved/Hold/Rejected 상태 관리 |
| NUnit XML/JSON 결과 파싱 | 유지 | Adventure 테스트 결과를 읽어 리포트 생성 |
| Excel/Markdown/DOCX 리포트 생성 | 유지 | 실행 결과, 실패 요약, 추세, 리스크 기록 |
| 중복 TC 감지 | 유지 | `target_class + target_function + test_function` 또는 TC ID 기준 중복 제거 |
| 의도적 오류 주입 검증 계획 생성 | 유지 | 실제 Adventure 파일에 장기 반영하지 않고 임시 변경/원복 절차만 작성 |
| Adventure 테스트 파일 직접 생성 | 제한 | 기본 금지. 승인된 TC 승격 작업에서만 사람이 요청한 경우 허용 |
| Adventure 런타임 코드 직접 수정 | 제한 | QA 경계 설계상 금지. 별도 리팩토링/버그 수정 작업으로 분리 |

## 7. 단계별 마이그레이션 계획

### Phase 0. 현 상태 동결

목표: 더 이상 generated test가 Adventure에 자동 유입되지 않도록 막는다.

작업:

1. MyHarnessProject의 Adventure 테스트 파일 직접 쓰기 경로를 비활성화한다.
2. 새 generated test는 Adventure가 아니라 MyHarnessProject 산출물 또는 QA manifest 후보로만 저장한다.
3. Adventure 내부의 `Assets/Editor/AutoGenTests`는 임시 동결 대상으로 표시한다.

완료 기준:

- MyHarnessProject 실행만으로 `Adventure/JsonFile/Assets` 아래 테스트 파일이 추가/수정되지 않는다.

### Phase 1. 현재 테스트 인벤토리 작성

목표: 현재 테스트를 Approved/Candidate/Hold/Rejected로 분류한다.

작업:

1. `P0DataCompatibilityTests.cs`의 기존 테스트를 manifest에 `Approved`로 등록한다.
2. `Assets/Editor/AutoGenTests`의 테스트와 생략 주석을 manifest에 `Candidate`, `Hold`, `Rejected`로 등록한다.
3. 중복 목적 테스트는 하나의 대표 TC에 연결하고 나머지는 중복 후보로 표시한다.

완료 기준:

- 모든 Adventure 내부 테스트 파일이 manifest에 등록된다.
- `source`와 `status`가 비어 있는 TC가 없다.

### Phase 2. Adventure 내부 테스트 경로 정리

목표: 승인 테스트와 후보 테스트의 물리적 위치를 분리한다.

권장 구조:

```text
JsonFile/Assets/Tests/EditMode
  P0DataCompatibilityTests.cs
  P1ApprovedEditModeTests.cs

JsonFile/Assets/Tests/PlayMode
  P0RuntimeSmokeTests.cs
  P1UiSmokeTests.cs

JsonFile/memory/QA_Portfolio/Candidates
  generated_tc_manifest.json
  generated_test_drafts/
```

작업:

1. `Assets/Editor/AutoGenTests`는 바로 삭제하지 않고 manifest 분류를 먼저 끝낸다.
2. `Approved`가 아닌 generated test는 Unity Test Runner에서 실행되지 않는 후보 보관 위치로 옮기는 계획을 세운다.
3. 승격된 테스트만 `Assets/Tests/EditMode` 또는 `Assets/Tests/PlayMode`에 둔다.

완료 기준:

- Adventure의 Unity Test Runner에는 승인된 테스트만 남는다.

### Phase 3. P0/P1 후보 승격

목표: 가치가 높은 후보만 Adventure 내부 기준 테스트로 편입한다.

작업:

1. `P0-EDIT-SAVE-002`, `P0-EDIT-SAVE-003`부터 검토한다.
2. 저장 경로 격리 seam이 충분한지 확인한다.
3. `P0-PLAY-SAVE-001`, `P0-PLAY-RUNTIME-001`은 PlayMode fixture가 준비된 뒤 진행한다.
4. P1 후보는 중복 제거 후 작은 단위로 승격한다.

완료 기준:

- 승격된 TC는 manifest 상태가 `Approved`로 변경된다.
- 실제 테스트 파일 경로가 `adventure_path`에 기록된다.
- 기존 generated test 원본은 `harness_source`로만 추적된다.

### Phase 4. MyHarnessProject 리포트 전용 연결

목표: MyHarnessProject가 Adventure 테스트 결과를 읽기만 하도록 한다.

작업:

1. Adventure 테스트 실행 산출물 위치를 고정한다.
2. MyHarnessProject는 NUnit XML/JSON 결과를 읽어 집계한다.
3. MyHarnessProject 리포트는 manifest의 `Approved` TC와 실행 결과를 매칭한다.
4. Candidate/Hold/Rejected는 실행 결과로 집계하지 않고 후보/보류/폐기 섹션에만 표시한다.

완료 기준:

- MyHarnessProject 리포트 총계가 Adventure NUnit XML/JSON의 test-case 수와 일치한다.
- Candidate 테스트가 통과 테스트 수에 섞이지 않는다.

### Phase 5. 중복 제거와 품질 증명

목표: 테스트가 실제 결함을 잡는지 확인하고, 중복/무효 테스트를 제거한다.

작업:

1. 승인 테스트 중 일부에 대해 의도적 오류 주입 검증을 수행한다.
2. 오류 주입은 임시 변경으로만 수행하고 즉시 원복한다.
3. 동일 목적 테스트는 하나로 합치고, 나머지는 Rejected 또는 보조 후보로 전환한다.
4. 결과를 QA 리포트에 기록한다.

완료 기준:

- 의도적 오류 주입 시 관련 Approved 테스트가 실패한다.
- 원복 후 전체 Approved 테스트가 다시 통과한다.
- 결과 기록에 기존 실패, 테스트 결함, 실제 코드 결함, 환경 문제를 구분해 남긴다.

## 8. 운영 규칙

### 8.1 Adventure에 둘 수 있는 테스트

Adventure 내부에 둘 수 있는 테스트는 다음 조건 중 하나를 만족해야 한다.

- P0 공개 계약을 보호한다.
- P1 회귀 위험을 낮춘다.
- 실패 시 실제 게임 품질 위험을 설명할 수 있다.
- 사람이 기대 결과를 승인했다.
- 실행 위치가 EditMode 또는 PlayMode로 확정됐다.

### 8.2 MyHarnessProject가 하지 말아야 할 일

MyHarnessProject는 다음 작업을 기본적으로 하지 않는다.

- Adventure `Assets/Tests` 아래에 generated test를 직접 추가
- Adventure 런타임 코드를 자동 수정
- Candidate 테스트를 통과/실패 결과로 집계
- manual oracle이 필요한 테스트를 자동 테스트처럼 표현
- NUnit XML/JSON 결과와 다른 총계를 최종 결과처럼 표시

### 8.3 결과 보고 규칙

리포트는 항상 아래 구분을 유지한다.

| 분류 | 의미 |
|---|---|
| Approved 실행 결과 | Adventure 내부 승인 테스트의 실제 NUnit XML/JSON 결과 |
| Candidate 후보 | 아직 Adventure에 반영하지 않은 테스트 후보 |
| Hold 보류 | fixture, 오라클, 씬 격리, 수동 판단이 필요한 항목 |
| Rejected 폐기 | 중복, 무효, 기대값 오류, 자동화 부적합 항목 |
| Manual QA | 조작감, 밸런스, 시각적 이상, 재미 판단 |

## 9. 우선 실행안

가장 안전한 첫 단계는 코드 수정이 아니라 분류와 동결이다.

1. `qa_manifest.json`을 만든다.
2. `P0DataCompatibilityTests.cs`의 현재 테스트를 `Approved`로 등록한다.
3. `Assets/Editor/AutoGenTests`의 26개 `.cs` 파일을 `Candidate/Hold/Rejected`로 분류한다.
4. MyHarnessProject의 Adventure 직접 쓰기 경로를 끊고, 후보 출력만 남긴다.
5. `test_SaveManager.cs`의 저장 round-trip 후보부터 P0 승격 검토를 시작한다.
6. 승격된 테스트만 `Assets/Tests/EditMode` 또는 `Assets/Tests/PlayMode`로 이동한다.
7. MyHarnessProject는 Adventure NUnit XML/JSON을 읽어 리포트만 생성하도록 전환한다.

## 10. 결론

분리 작업은 지금 시작하는 것이 맞다. 단, 목적은 MyHarnessProject를 제거하는 것이 아니라 역할을 명확히 제한하는 것이다.

최종 기준은 다음과 같이 둔다.

- Adventure는 승인된 P0/P1 테스트의 실행 장소다.
- MyHarnessProject는 후보 생성과 리포트 생성 도구다.
- QA manifest는 테스트 상태와 소유권의 단일 기준이다.
- Candidate는 테스트 결과가 아니라 검토 대상이다.
- Approved만 Adventure 내부 테스트로 승격한다.

이 구조를 적용하면 MyHarnessProject를 계속 개선해도 Adventure의 품질 기준은 흔들리지 않는다. 반대로 이 경계를 만들지 않으면, 자동 생성 테스트가 늘어날수록 실패 원인과 품질 책임을 분리하기 어려워진다.
