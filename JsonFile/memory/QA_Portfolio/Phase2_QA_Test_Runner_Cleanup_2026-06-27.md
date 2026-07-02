# Phase 2 QA 테스트 러너 정리 보고서

작성일: 2026-06-27
대상: `Adventure/JsonFile`
기준 문서: `JsonFile/memory/QA_Responsibility_Boundary_Design.md`, `JsonFile/memory/QA_Portfolio/Phase1_QA_Test_Inventory_2026-06-27.md`, `JsonFile/memory/QA_Portfolio/qa_manifest.json`

## 1. 목표

Adventure Unity Test Runner 경로에는 Approved 테스트(`P0DataCompatibilityTests.cs`, 71개)만 남기고, `Assets/Editor/AutoGenTests`의 generated 후보 26개(.cs 26 + .meta 26 = 52 파일)를 `QA_Portfolio/Candidates`로 보관한다.

## 2. 작업 절차 및 결과

### 2-1. 후보 보관 (삭제 전 복사)

- 보관 위치: `JsonFile/memory/QA_Portfolio/Candidates/generated_test_drafts/`
- 복사 파일: `Assets/Editor/AutoGenTests`의 `.cs` 26개 + `.cs.meta` 26개 = 52개
- 검증: `diff -rq` 로 원본과 보관본이 **완전히 동일**함을 확인 후에만 원본 제거 진행

### 2-2. Unity Test Runner 경로에서 제거

- 제거 대상: `Assets/Editor/AutoGenTests/` (디렉터리 전체) + `Assets/Editor/AutoGenTests.meta`
- Approved 테스트(`Assets/Tests/EditMode/P0DataCompatibilityTests.cs`)는 건드리지 않음

### 2-3. qa_manifest.json 갱신

- Candidate 26개 엔트리(`CAND-AUTOGEN-001`~`026`) 각각에 추가/변경:
  - `current_workspace_path`: `null` (더 이상 Unity 워크스페이스에 없음)
  - `candidate_archive_path`: `JsonFile/memory/QA_Portfolio/Candidates/generated_test_drafts/<파일명>`
  - `phase2_status`: `"ArchivedAndRemovedFromRunner"`
  - `phase2_completed_at`: 실행 시각
  - `approval_note`에 Phase 2 처리 내역 추가
- 최상위 `policy.autogen_files_archived_and_removed_in_phase2`: `true` 추가
- `phase2_verification_snapshot` 신규 섹션 추가(보관/검증/실행 결과 전체 기록)
- Approved 71건 엔트리는 **일절 수정하지 않음**

### 2-4. Unity EditMode 실행 (제거 후)

- 결과 XML: `JsonFile/memory/QA_Portfolio/07_Evidence/unity_editmode_phase2_2026-06-27.xml`
- 로그: `JsonFile/memory/QA_Portfolio/07_Evidence/unity_editmode_phase2_2026-06-27.log`

| 항목 | total | passed | failed | skipped | inconclusive | 결과 |
|---|---:|---:|---:|---:|---:|---|
| Phase 2 (AutoGenTests 제거 후) | 71 | 71 | 0 | 0 | 0 | Passed |
| Phase 1 베이스라인(Approved만) | 71 | 71 | 0 | 0 | 0 | Passed |

- 관측된 fixture: `P0DataCompatibilityTests` **1개뿐** — Candidate 클래스(`TestChoiceEvaluator`, `TestEquipmentSystem`, `TestInventoryManager`, `TestOptionManager`, `TestPlayerState`, `TestSaveManager` 등)는 결과에 전혀 나타나지 않음
- 컴파일 에러 0건, 경고 0건 (`Tundra build success, 6 items updated, 733 evaluated`)
- 실패 테스트 0건

## 3. 전/후 비교

| 항목 | Phase 1 (AutoGenTests 존재) | Phase 2 (AutoGenTests 제거 후) |
|---|---|---|
| Unity Test Runner 총 케이스 | 97 (71 Approved + 26 Candidate) | **71 (Approved만)** |
| Approved pass/fail | 71/71 Passed | 71/71 Passed (변화 없음) |
| Candidate 노출 여부 | Test Runner에 26개 노출(집계는 제외) | **Test Runner에 전혀 노출 안 됨** |
| Candidate 원본 위치 | `Assets/Editor/AutoGenTests/` | `memory/QA_Portfolio/Candidates/generated_test_drafts/` (보관) |

**결론**: AutoGenTests 제거가 Approved 테스트 결과에 어떤 영향도 주지 않았다(71/71 Passed 동일). Candidate는 더 이상 Unity Test Runner에서 보이지 않으며, 원본은 완전히 보존되어 있다.

## 4. 완료 기준 충족 확인

| 완료 기준 | 충족 여부 | 근거 |
|---|---|---|
| Unity Test Runner 결과에 Candidate가 섞이지 않음 | ✅ | Phase 2 XML에 `P0DataCompatibilityTests` fixture만 존재, 71/71 |
| Approved 테스트만 실행 대상으로 남음 | ✅ | `Assets/Editor/AutoGenTests` 제거, `Assets/Tests/EditMode/P0DataCompatibilityTests.cs`만 남음 |
| qa_manifest.json에서 Candidate 원본 보관 경로 추적 가능 | ✅ | 26개 엔트리 모두 `candidate_archive_path` 필드로 추적 가능 |
| 삭제/이동 전후 결과가 명확히 기록됨 | ✅ | `phase2_verification_snapshot`(manifest) + 본 보고서 + 2-1·2-4절 비교표 |

## 5. 운영 메모

- Candidate 후보의 Phase 2 권장 처리(`ArchiveThenRemoveFromRunner` / `HoldReview` / `DedupeReview` / `PromoteReview` / `HoldOrRejectReview`, Phase 1 보고서 4절·6절)는 이번 작업으로 **변경되지 않았다** — 이번 Phase 2는 "Unity Runner 경로에서 분리"만 수행했고, 개별 파일의 승격/폐기 판단(PromoteReview 등)은 그대로 보류 상태다.
- `test_ChoiceEvaluator.cs`, `test_InventoryManager.cs`, `test_PlayerState.cs`, `test_SaveManager.cs`(PromoteReview 대상)도 이번에 함께 보관 폴더로 옮겨졌다 — 승격 검토는 별도로 진행해야 하며, 이번 작업으로 자동 승격되지 않았다.
- MyHarnessProject 쪽 `run_unity_tests.py`/`run_pipeline.py`는 Phase 0(2026-06-27)부터 `--allow-adventure-temp-copy` 없이는 Adventure에 어떤 것도 복사하지 않으므로, 이번에 비운 `Assets/Editor/AutoGenTests` 경로가 의도치 않게 다시 채워질 위험은 낮다.
