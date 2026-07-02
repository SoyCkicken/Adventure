# Phase 1 QA 테스트 인벤토리 및 Manifest 생성 보고서

작성일: 2026-06-27T07:23:57+09:00
대상: `Adventure/JsonFile`
기준 문서: `JsonFile/memory/QA_Responsibility_Boundary_Design.md`

## 1. 필수 확인 결과

| 항목 | 결과 | 근거 |
| --- | --- | --- |
| Unity MCP 연결 | 실행 중인 Unity Editor 인스턴스 0개 | `mcpforunity://instances` |
| Unity batchmode EditMode | 97 total / 97 passed / 0 failed / 0 skipped / 0 inconclusive | `07_Evidence/unity_editmode_phase1_2026-06-27.xml` |
| Approved 기준 총계 | 71 total / 71 passed / 0 failed / 0 skipped / 0 inconclusive | `P0DataCompatibilityTests` test-case만 Approved로 집계 |
| Candidate 관측값 | 26 executable candidate test cases observed | AutoGenTests는 Candidate라 Pass/Fail 총계에서 제외 |
| SaveManager 충돌 상태 | 충돌 마커 0개, 테스트 저장 경로 seam 존재: {'SetSavePathForTesting': True, 'ClearSavePathForTesting': True, 'savePathOverride': True} | `SaveManager.cs` 최신 파일 정적 확인 + Unity EditMode 97/97 Passed |

## 2. Manifest 요약

| 상태 | 개수 | Pass/Fail 집계 포함 여부 | 설명 |
| --- | --- | --- | --- |
| Approved | 71 | 포함 | `P0DataCompatibilityTests.cs` 기존 Adventure 기준 테스트 |
| Candidate | 26 | 제외 | `Assets/Editor/AutoGenTests` generated 후보 파일 26개 |
| Hold | 0 | 제외 | Phase 1에서는 AutoGen 파일을 모두 Candidate로 등록하고 Phase 2 권장 조치로 Hold 후보를 표시 |
| Rejected | 0 | 제외 | Phase 1에서는 삭제/폐기 확정하지 않음 |

## 3. P0DataCompatibilityTests 등록 목록

| TC ID | Test Case | Method | Risk Area | Status | Latest Result |
| --- | --- | --- | --- | --- | --- |
| ADV-P0-EDIT-001 | BattleStartOptionRegistrationUpdatesSameItemOptionInsteadOfDuplicating | BattleStartOptionRegistrationUpdatesSameItemOptionInsteadOfDuplicating | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-002 | BerserkBattleStartBuffIsNonStackingAndChangesCombatStats | BerserkBattleStartBuffIsNonStackingAndChangesCombatStats | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-003 | BerserkUsesExplicitPlayerFlagWhenPlayerStateIsMissing | BerserkUsesExplicitPlayerFlagWhenPlayerStateIsMissing | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-004 | ChoiceEvaluatorHandlesFormulaBoundaries | ChoiceEvaluatorHandlesFormulaBoundaries | Story/Event | Approved | Passed |
| ADV-P0-EDIT-005 | ConditionEvaluatorHandlesKnownRequirementTypes | ConditionEvaluatorHandlesKnownRequirementTypes | Story/Event | Approved | Passed |
| ADV-P0-EDIT-006 | DamageTakenIncreaseBuffsAreSummedAfterArmorReduction | DamageTakenIncreaseBuffsAreSummedAfterArmorReduction | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-007 | EditorBuildSettingsRequiredScenesStayEnabledAndOrdered | EditorBuildSettingsRequiredScenesStayEnabledAndOrdered | Scene | Approved | Passed |
| ADV-P0-EDIT-008 | EliteMonsterVariantsUseExistingPassiveOptions | EliteMonsterVariantsUseExistingPassiveOptions | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-009 | EquipmentOptionCanApplyDebuffOnBattleStart | EquipmentOptionCanApplyDebuffOnBattleStart | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-010 | EquipmentSystemAppliesResistanceStatsFromJsonArmorOptions | EquipmentSystemAppliesResistanceStatsFromJsonArmorOptions | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-011 | EquipmentSystemAutoCreatesOptionManagerAndAppliesArmorCriticalChance | EquipmentSystemAutoCreatesOptionManagerAndAppliesArmorCriticalChance | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-012 | EquipmentSystemRegistersWeaponOnHitOptionAndRuntimeEffectCanFire | EquipmentSystemRegistersWeaponOnHitOptionAndRuntimeEffectCanFire | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-013 | EternalFrostEquipmentIconsAreLoadableSprites("Images/Items/Weapon_046") | EternalFrostEquipmentIconsAreLoadableSprites | Item/Equipment | Approved | Passed |
| ADV-P0-EDIT-014 | EternalFrostEquipmentIconsAreLoadableSprites("Images/Items/Armor_044") | EternalFrostEquipmentIconsAreLoadableSprites | Item/Equipment | Approved | Passed |
| ADV-P0-EDIT-015 | EternalFrostEquipmentUsesExistingOptionsAndItemDataPath | EternalFrostEquipmentUsesExistingOptionsAndItemDataPath | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-016 | ExcelConverterUsesKoreanFallbackEncoding | ExcelConverterUsesKoreanFallbackEncoding | Data Pipeline | Approved | Passed |
| ADV-P0-EDIT-017 | ExcelGeneratedOptionDataPreservesKoreanForcedMissEffect | ExcelGeneratedOptionDataPreservesKoreanForcedMissEffect | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-018 | ForcedMissEffectConsumesOnlyTheNextAttack | ForcedMissEffectConsumesOnlyTheNextAttack | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-019 | ForcedMissOptionEffectAddsMissChargeToTarget | ForcedMissOptionEffectAddsMissChargeToTarget | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-020 | FreezeSlowsOwnerAndCanBlockAction | FreezeSlowsOwnerAndCanBlockAction | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-021 | HolyCanApplyNextAttackMissChance | HolyCanApplyNextAttackMissChance | Runtime Contract | Approved | Passed |
| ADV-P0-EDIT-022 | ImportantJsonRowCountsStayStable("Story_Master_Main",93) | ImportantJsonRowCountsStayStable | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-023 | ImportantJsonRowCountsStayStable("Main_Script_Master_Main",93) | ImportantJsonRowCountsStayStable | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-024 | ImportantJsonRowCountsStayStable("RandomEvents_Master_Event",46) | ImportantJsonRowCountsStayStable | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-025 | ImportantJsonRowCountsStayStable("Ran_Script_Master_Event",50) | ImportantJsonRowCountsStayStable | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-026 | ImportantJsonRowCountsStayStable("Weapon_Master",46) | ImportantJsonRowCountsStayStable | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-027 | ImportantJsonRowCountsStayStable("Armor_Master",44) | ImportantJsonRowCountsStayStable | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-028 | ImportantJsonRowCountsStayStable("BlackSmith",88) | ImportantJsonRowCountsStayStable | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-029 | ImportantJsonRowCountsStayStable("Item_Master",20) | ImportantJsonRowCountsStayStable | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-030 | ImportantJsonRowCountsStayStable("Option_Master",18) | ImportantJsonRowCountsStayStable | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-031 | ImportantJsonRowCountsStayStable("OptionEffect_Master",18) | ImportantJsonRowCountsStayStable | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-032 | ImportantJsonRowCountsStayStable("Mon_Master",13) | ImportantJsonRowCountsStayStable | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-033 | JsonManagerIndexesItemMastersById | JsonManagerIndexesItemMastersById | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-034 | MerchantItemsUseLoadedBlackSmithTable | MerchantItemsUseLoadedBlackSmithTable | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-035 | MissingJsonManagerListTablesReturnEmptyCollections | MissingJsonManagerListTablesReturnEmptyCollections | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-036 | MonsterBattleStartPassiveIsTemporaryAndRemovedAfterBattle | MonsterBattleStartPassiveIsTemporaryAndRemovedAfterBattle | Monster/Combat | Approved | Passed |
| ADV-P0-EDIT-037 | MonsterMasterReferencesKnownOptionOrMonsterEffectIds | MonsterMasterReferencesKnownOptionOrMonsterEffectIds | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-038 | MonsterOnHitOptionUsesOptionManagerWithSafeDefaultValue | MonsterOnHitOptionUsesOptionManagerWithSafeDefaultValue | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-039 | MonsterOptionCollectorSupportsNumberedSlotsAndSkipsNoOps | MonsterOptionCollectorSupportsNumberedSlotsAndSkipsNoOps | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-040 | OptionEffectAuthoringRowsTargetExistingRuntimeOptions | OptionEffectAuthoringRowsTargetExistingRuntimeOptions | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-041 | OptionEffectAuthoringSheetIsMergedIntoRuntimeOptionMaster | OptionEffectAuthoringSheetIsMergedIntoRuntimeOptionMaster | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-042 | OptionMasterEffectIdsResolveToRegisteredRuntimeEffects | OptionMasterEffectIdsResolveToRegisteredRuntimeEffects | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-043 | PeriodicBuffTicksApplyBurnAndHealingAfterInitialEffect | PeriodicBuffTicksApplyBurnAndHealingAfterInitialEffect | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-044 | ResourcesEventsRootKeysMatchFileNames | ResourcesEventsRootKeysMatchFileNames | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-045 | RuntimeItemCodeLookupUsesItemIds | RuntimeItemCodeLookupUsesItemIds | JSON/Data | Approved | Passed |
| ADV-P0-EDIT-046 | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths("Option_001",13) | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-047 | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths("Option_002",17) | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-048 | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths("Option_003",6) | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-049 | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths("Option_004",8) | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-050 | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths("Option_005",25) | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-051 | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths("Option_006",20) | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-052 | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths("Option_007",15) | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-053 | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths("Option_008",2) | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-054 | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths("Option_009",2) | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-055 | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths("Option_010",3) | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-056 | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths("Option_011",2) | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-057 | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths("Option_012",3) | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-058 | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths("Option_013",2) | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-059 | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths("Option_014",14) | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-060 | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths("Option_015",15) | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-061 | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths("Option_016",16) | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-062 | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths("Option_017",17) | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-063 | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths("Option_018",60) | RuntimeOptionsApplyInjectedValuesThroughActualEffectPaths | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-064 | SaveDataPublicFieldNamesStayBackwardCompatible | SaveDataPublicFieldNamesStayBackwardCompatible | Save | Approved | Passed |
| ADV-P0-EDIT-065 | StackingBleedDamagesAttackerOnAttackAndUsesResistance | StackingBleedDamagesAttackerOnAttackAndUsesResistance | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-066 | StackingRegenHealsOwnerOnAttack | StackingRegenHealsOwnerOnAttack | Runtime Contract | Approved | Passed |
| ADV-P0-EDIT-067 | StopBattleRemovesTemporaryCombatBuffs | StopBattleRemovesTemporaryCombatBuffs | Option/Combat | Approved | Passed |
| ADV-P0-EDIT-068 | StoryNavigatorKeepsRewardNodesEvenWhenTheyLookLikeLabels | StoryNavigatorKeepsRewardNodesEvenWhenTheyLookLikeLabels | Story/Event | Approved | Passed |
| ADV-P0-EDIT-069 | StoryNavigatorNormalizesMainScriptChoiceCodes | StoryNavigatorNormalizesMainScriptChoiceCodes | Story/Event | Approved | Passed |
| ADV-P0-EDIT-070 | StoryNavigatorSkipsTextLabelNodesWithoutRewards | StoryNavigatorSkipsTextLabelNodesWithoutRewards | Story/Event | Approved | Passed |
| ADV-P0-EDIT-071 | WeaponMasterPreservesOneHandedCompatibility | WeaponMasterPreservesOneHandedCompatibility | Item/Equipment | Approved | Passed |

## 4. AutoGenTests 26개 분류표

| TC ID | File | Fixture | Executable Tests | Omitted Cases | Status | Phase 2 Recommendation | Action |
| --- | --- | --- | --- | --- | --- | --- | --- |
| CAND-AUTOGEN-001 | test_BattleImageDoTween.cs | TestBattleImageDoTween | 0 | 0 | Candidate | ArchiveThenRemoveFromRunner | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| CAND-AUTOGEN-002 | test_BattleManager.cs | TestBattleManager | 0 | 0 | Candidate | ArchiveThenRemoveFromRunner | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| CAND-AUTOGEN-003 | test_BuffIconUI.cs | TestBuffIconUI | 0 | 0 | Candidate | ArchiveThenRemoveFromRunner | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| CAND-AUTOGEN-004 | test_BuffUI.cs | TestBuffUI | 0 | 0 | Candidate | ArchiveThenRemoveFromRunner | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| CAND-AUTOGEN-005 | test_Character.cs | TestCharacter | 0 | 4 | Candidate | HoldReview | 생략 주석만 후보로 보존한다. 수동 fixture와 명시 오라클 없이는 Adventure 테스트로 승격하지 않는다. |
| CAND-AUTOGEN-006 | test_ChoiceEvaluator.cs | TestChoiceEvaluator | 6 | 15 | Candidate | DedupeReview | P0DataCompatibilityTests.ChoiceEvaluatorHandlesFormulaBoundaries와 중복 제거 후 필요한 경계값만 승인 후보로 유지한다. |
| CAND-AUTOGEN-007 | test_CombatTest.cs | TestCombatTest | 0 | 0 | Candidate | ArchiveThenRemoveFromRunner | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| CAND-AUTOGEN-008 | test_ConfirmPopup.cs | TestConfirmPopup | 0 | 0 | Candidate | ArchiveThenRemoveFromRunner | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| CAND-AUTOGEN-009 | test_EffectProcessor.cs | TestEffectProcessor | 0 | 21 | Candidate | HoldReview | 생략 주석만 후보로 보존한다. 수동 fixture와 명시 오라클 없이는 Adventure 테스트로 승격하지 않는다. |
| CAND-AUTOGEN-010 | test_EquipmentSystem.cs | TestEquipmentSystem | 4 | 1 | Candidate | HoldReview | 현재 P0 장비/옵션 흐름 테스트와 중복 가능하다. 기대값과 fixture 보강 전까지 Hold 후보로 본다. |
| CAND-AUTOGEN-011 | test_EventDisplay.cs | TestEventDisplay | 0 | 0 | Candidate | ArchiveThenRemoveFromRunner | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| CAND-AUTOGEN-012 | test_FontSizeEditor.cs | TestFontSizeManager | 0 | 0 | Candidate | ArchiveThenRemoveFromRunner | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| CAND-AUTOGEN-013 | test_GameFlowManager.cs | TestGameFlowManager | 0 | 3 | Candidate | HoldReview | 생략 주석만 후보로 보존한다. 수동 fixture와 명시 오라클 없이는 Adventure 테스트로 승격하지 않는다. |
| CAND-AUTOGEN-014 | test_GoogleManager.cs | TestGoogleManager | 0 | 0 | Candidate | ArchiveThenRemoveFromRunner | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| CAND-AUTOGEN-015 | test_GotoGameScene.cs | TestGotoGameScene | 0 | 0 | Candidate | ArchiveThenRemoveFromRunner | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| CAND-AUTOGEN-016 | test_InventoryManager.cs | TestInventoryManager | 6 | 2 | Candidate | PromoteReview | 순수 로직 fixture가 유지되면 P1 EditMode 후보로 검토한다. UI/씬 의존성이 없는지 확인한다. |
| CAND-AUTOGEN-017 | test_JsonManager.cs | TestJsonManager | 0 | 76 | Candidate | HoldReview | 생략 주석만 후보로 보존한다. 수동 fixture와 명시 오라클 없이는 Adventure 테스트로 승격하지 않는다. |
| CAND-AUTOGEN-018 | test_MerchantManager.cs | TestMerchantManager | 0 | 0 | Candidate | ArchiveThenRemoveFromRunner | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| CAND-AUTOGEN-019 | test_OptionManager.cs | TestOptionManager | 2 | 4 | Candidate | HoldOrRejectReview | 표시 텍스트 계약 여부가 불명확하다. ID/Effect 연결은 P0 승인 테스트가 담당하므로 보강 가치 재검토가 필요하다. |
| CAND-AUTOGEN-020 | test_PatchNoteManager.cs | TestPatchNoteViewer | 0 | 0 | Candidate | ArchiveThenRemoveFromRunner | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| CAND-AUTOGEN-021 | test_PlayerState.cs | TestPlayerState | 4 | 2 | Candidate | PromoteReview | 스탯 계산식이 공개 계약인지 확인한 뒤 P1 EditMode 후보로 검토한다. |
| CAND-AUTOGEN-022 | test_SaveManager.cs | TestSaveManager | 4 | 5 | Candidate | PromoteReview | Candidate 보관 후 저장 경로 격리와 오라클을 재확인한다. P0-EDIT-SAVE-002 승격 후보로 검토한다. |
| CAND-AUTOGEN-023 | test_SkipOrScrollHandler.cs | TestSkipOrScrollHandler | 0 | 0 | Candidate | ArchiveThenRemoveFromRunner | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| CAND-AUTOGEN-024 | test_StoryDisplayManager.cs | TestStoryDisplayManager | 0 | 0 | Candidate | ArchiveThenRemoveFromRunner | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| CAND-AUTOGEN-025 | test_TextFragment.cs | TestTextFragment | 0 | 0 | Candidate | ArchiveThenRemoveFromRunner | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| CAND-AUTOGEN-026 | test_TouchCatcher.cs | TestTouchCatcher | 0 | 0 | Candidate | ArchiveThenRemoveFromRunner | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |

## 5. 중복 후보 목록

| ID | 중복 그룹 | Approved 기준 테스트 | Candidate 파일 | 처리 제안 |
| --- | --- | --- | --- | --- |
| DUP-001 | ChoiceEvaluator formula boundary | P0DataCompatibilityTests.ChoiceEvaluatorHandlesFormulaBoundaries | test_ChoiceEvaluator.cs | 중복 제거 후 필요한 경계값만 P1 후보로 유지 |
| DUP-002 | SaveManager save compatibility | P0DataCompatibilityTests.SaveDataPublicFieldNamesStayBackwardCompatible | test_SaveManager.cs | 저장 필드 계약은 승인 테스트 유지, round-trip은 저장 경로 격리 후 별도 P0 후보 |
| DUP-003 | Option contract/display | P0DataCompatibilityTests.OptionMasterEffectIdsResolveToRegisteredRuntimeEffects / OptionEffectAuthoringRowsTargetExistingRuntimeOptions | test_OptionManager.cs | 표시 텍스트 계약 여부가 불명확하므로 Hold 또는 폐기 검토 |
| DUP-004 | Equipment option application | P0DataCompatibilityTests.EquipmentSystemRegistersWeaponOnHitOptionAndRuntimeEffectCanFire 등 | test_EquipmentSystem.cs | 장비 옵션 실제 적용 흐름과 중복 가능. 단편 요구 조건 테스트는 Hold 후 재설계 |

## 6. Phase 2 이동/삭제 제안

Phase 1에서는 `Assets/Editor/AutoGenTests`를 삭제하거나 이동하지 않았다. Phase 2에서 먼저 후보 보관 위치로 복사/이동한 뒤 Unity Test Runner 경로에서 제거할 수 있다.

권장 후보 보관 위치: `JsonFile/memory/QA_Portfolio/Candidates/generated_test_drafts/`

### 6.1 우선 승격 검토

| File | Recommendation | Reason |
| --- | --- | --- |
| test_ChoiceEvaluator.cs | DedupeReview | P0DataCompatibilityTests.ChoiceEvaluatorHandlesFormulaBoundaries와 중복 제거 후 필요한 경계값만 승인 후보로 유지한다. |
| test_InventoryManager.cs | PromoteReview | 순수 로직 fixture가 유지되면 P1 EditMode 후보로 검토한다. UI/씬 의존성이 없는지 확인한다. |
| test_PlayerState.cs | PromoteReview | 스탯 계산식이 공개 계약인지 확인한 뒤 P1 EditMode 후보로 검토한다. |
| test_SaveManager.cs | PromoteReview | Candidate 보관 후 저장 경로 격리와 오라클을 재확인한다. P0-EDIT-SAVE-002 승격 후보로 검토한다. |

### 6.2 Hold 또는 재설계 검토

| File | Recommendation | Reason |
| --- | --- | --- |
| test_Character.cs | HoldReview | 생략 주석만 후보로 보존한다. 수동 fixture와 명시 오라클 없이는 Adventure 테스트로 승격하지 않는다. |
| test_EffectProcessor.cs | HoldReview | 생략 주석만 후보로 보존한다. 수동 fixture와 명시 오라클 없이는 Adventure 테스트로 승격하지 않는다. |
| test_EquipmentSystem.cs | HoldReview | 현재 P0 장비/옵션 흐름 테스트와 중복 가능하다. 기대값과 fixture 보강 전까지 Hold 후보로 본다. |
| test_GameFlowManager.cs | HoldReview | 생략 주석만 후보로 보존한다. 수동 fixture와 명시 오라클 없이는 Adventure 테스트로 승격하지 않는다. |
| test_JsonManager.cs | HoldReview | 생략 주석만 후보로 보존한다. 수동 fixture와 명시 오라클 없이는 Adventure 테스트로 승격하지 않는다. |
| test_OptionManager.cs | HoldOrRejectReview | 표시 텍스트 계약 여부가 불명확하다. ID/Effect 연결은 P0 승인 테스트가 담당하므로 보강 가치 재검토가 필요하다. |

### 6.3 후보 보관 후 Unity Runner에서 제거 가능

| File | Reason |
| --- | --- |
| test_BattleImageDoTween.cs | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| test_BattleManager.cs | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| test_BuffIconUI.cs | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| test_BuffUI.cs | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| test_CombatTest.cs | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| test_ConfirmPopup.cs | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| test_EventDisplay.cs | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| test_FontSizeEditor.cs | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| test_GoogleManager.cs | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| test_GotoGameScene.cs | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| test_MerchantManager.cs | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| test_PatchNoteManager.cs | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| test_SkipOrScrollHandler.cs | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| test_StoryDisplayManager.cs | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| test_TextFragment.cs | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |
| test_TouchCatcher.cs | 실행 테스트와 생략 후보가 없는 shell 파일이다. 후보 보관 후 Unity Test Runner 경로에서는 제거 가능하다. |

## 7. 운영 메모

- Candidate는 최신 Unity XML에서 실행 관측되더라도 승인 테스트 결과로 집계하지 않는다.
- MyHarnessProject는 이 manifest와 Unity NUnit XML/JSON을 읽어 리포트만 생성해야 한다.
- Phase 2 전에는 AutoGenTests 삭제/이동을 하지 않는다.
- SaveManager는 최신 파일 기준 충돌 마커가 없고, `SetSavePathForTesting` / `ClearSavePathForTesting` seam이 존재한다.
