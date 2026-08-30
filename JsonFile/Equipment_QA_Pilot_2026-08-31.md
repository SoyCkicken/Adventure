# Adventure 장비 장착/해제 QA Pilot 분석

- 작성일: 2026-08-31 (Asia/Seoul)
- 대상 프로젝트: `Adventure/JsonFile`
- 대상 기능: 장비 장착 / 해제
- 분석 성격: Reconstructed / Inferred Requirement 초안
- 검토 상태: `Draft` — 사람의 승인 전까지 확정 Requirement 또는 Defect가 아님
- 수행 범위: 제품 코드, 데이터, 씬/프리팹, 기존 테스트 소스의 정적 분석
- 미수행 범위: Unity Editor 실행, Unity MCP 실행, Test Runner 실행, 제품 코드·테스트 코드 수정

## 1. 결론 요약

현재 구현은 인벤토리에서 무기 또는 방어구를 선택해 같은 종류의 장착 슬롯으로 이동하고, 기존 장비가 있으면 인벤토리로 돌려보낸 뒤 장비 ID를 기준으로 능력치와 옵션을 다시 계산하는 구조다. 장착 해제는 반대 방향으로 아이템을 이동하고 장비 원천 버프를 제거한 뒤 재계산한다.

정적 근거상 기본 장착·교체·해제 흐름은 설명 가능하며, 데이터도 무기 46개와 방어구 44개 모두 유효한 ID와 `ItemType`을 가지고 있다. 장비가 참조하는 옵션 84건은 모두 `Option_Master`에 존재한다.

다만 다음 항목은 사람의 우선 검토와 이후 격리된 Runtime 검증이 필요하다.

1. 같은 장비를 유지한 채 `EquipmentSystem.Init()`를 반복하면 `Character.AddBuff()`의 기존 버프 갱신 경로가 능력치 delta를 다시 적용하지 않아 치명타 확률·공격속도 등의 패시브 효과가 사라질 가능성이 있다.
2. 장착 해제 가능 여부는 현재 표시 가능한 인벤토리 칸 수가 아니라 고정 최대치 14개로 검사한다. 현재 칸이 가득 찼지만 14개 미만이면 해제 아이템이 목록에는 추가되고 UI에는 보이지 않을 가능성이 있다.
3. 장비가 없는 Save 데이터를 기존 장비 상태 위에 Load할 때 장착 슬롯과 장비 ID를 명시적으로 비우는 `else` 처리가 없어 이전 상태가 남을 가능성이 있다.
4. 프리팹에는 오른손·왼손·방어구 슬롯이 있으나 런타임 데이터와 `InventoryManager`는 단일 무기 ID/단일 무기 슬롯만 관리한다. `One-Handed` 데이터의 실제 정책은 구현만으로 확정할 수 없다.

이 문서의 Requirement, Risk, Test Condition은 위 현상을 곧바로 결함으로 확정하지 않고 승인 가능한 검증 기준으로 바꾸기 위한 초안이다.

## 2. 근거와 판단 등급

### 2.1 근거 종류

| 표기 | 의미 |
|---|---|
| Source | 현재 C# 제품 코드에서 직접 확인 |
| Data | 현재 JSON 및 C# 데이터 구조에서 직접 확인 |
| Scene | 현재 씬/프리팹 직렬화에서 직접 확인 |
| Existing Test Source | 기존 테스트 코드에 검증 의도가 존재하나 이번 Pilot에서 실행하지 않음 |
| Static Inference | 실행하지 않고 제어·데이터 흐름으로 추론 |
| Runtime Needed | 실제 Unity 상태에서 재현·관찰이 필요 |

### 2.2 분석 경계

- 코드가 현재 그렇게 동작한다는 사실과 정상 동작 요구사항은 분리했다.
- 기존 EditMode 테스트 소스의 존재는 Runtime 통과 근거로 사용하지 않았다.
- 과거 테스트 결과는 이 문서의 현재 실행 증거로 사용하지 않았다.
- Finding은 `Defect Candidate`, `Requirement Gap`, `Quality Concern`, `Expected Behavior`, `Need Review`로만 분류했다.

## 3. 현재 기능 구조

### 3.1 주요 구성요소

| 구성요소 | 현재 책임 | 주요 근거 |
|---|---|---|
| `InventoryManager` | 선택 아이템, 인벤토리 목록, 장착/해제 버튼, 장착 슬롯 UI, 저장/복원 연결 | `Assets/Script/UI_UX/InventoryManager.cs:11-60`, `187-233`, `401-481`, `532-567` |
| `EquipmentSystem` | 장비 교체·해제, 장비 ID 갱신, 기본/장비 능력치 재계산, 옵션 적용 | `Assets/Script/EquipmentSystem.cs:67-150`, `193-264`, `266-313` |
| `ItemSlotUI` | 슬롯의 `CurrentItem`, 아이콘, 클릭 콜백, 슬롯 초기화 | `Assets/Script/UI_UX/ItemSlotUI.cs:7-29`, `74-89`, `159-162` |
| `Character` | 실제 전투 능력치, 장비 ID, 장비 옵션 목록, 활성 버프와 원천 아이템 연결 | `Assets/Script/combat/Character.cs:621-688`, `873-949`, `1386-1479` |
| `PlayerState` | 성장 능력치와 씬 간 유지, Load 후 장비 재계산 호출 | `Assets/Script/PlayerState.cs:9-46`, `63-71`, `123-157` |
| `OptionManager` | 옵션 ID를 효과 구현으로 연결하고 장착/전투 트리거별로 등록 또는 적용 | `Assets/Script/combat/OptionManager.cs:683-735`, `795-813`, `839-912` |
| `ItemDataFactory` | 무기·방어구 Master 데이터를 런타임 `ItemData`로 변환·보정 | `Assets/Script/GamePlay/ItemDataFactory.cs:7-48`, `103-131` |
| `SaveManager` | `PlayerState`와 `InventoryManager`에 Save/Load를 위임 | `Assets/Script/Save_Load/SaveManager.cs:217-230`, `352-375`, `517-545` |
| JSON Master | 무기, 방어구, 옵션의 기준 데이터 | `Assets/Resources/Events/Weapon_Master.json`, `Armor_Master.json`, `Option_Master.json` |

### 3.2 현재 호출 흐름

```text
인벤토리 슬롯 클릭
  -> ItemSlotUI.OnClick()
  -> InventoryManager.ShowItemDetail(item)
  -> ItemDataFactory.ApplyMasterData(item)
  -> 장착 또는 해제 버튼 노출

장착 버튼
  -> InventoryManager.OnClickEquip()
  -> EquipmentSystem.EquipItem()
  -> 기존 같은 종류 장비를 인벤토리에 복귀
  -> 기존 장비 원천 버프 제거
  -> 새 아이템을 장착 슬롯에 설정
  -> 새 아이템을 인벤토리에서 제거
  -> Character.weapon_Name 또는 armor_Name 갱신
  -> EquipmentSystem.Init()
  -> 기본 능력치 재설정
  -> JSON Master 장비 능력치 계산
  -> OptionManager를 통한 옵션 적용/등록
  -> 인벤토리 및 DPS/HP UI 갱신

해제 버튼
  -> InventoryManager.OnClickUnequip()
  -> 고정 최대치 14개 기준 공간 검사
  -> EquipmentSystem.UnequipItem()
  -> 장착 아이템 clone을 인벤토리에 추가
  -> 해당 아이템 원천 버프 제거
  -> Character 장비 ID 제거
  -> 장착 슬롯 Clear
  -> EquipmentSystem.Init()
  -> 남은 장비 기준 능력치/옵션 재계산
  -> 인벤토리 및 DPS/HP UI 갱신
```

### 3.3 상태 소유권

| 상태 | 소유 위치 | 지속성 | 관찰 방법 |
|---|---|---|---|
| 인벤토리 아이템 목록 | `InventoryManager.inventoryItems` | Save 대상 | 목록 수, 슬롯 UI, Save JSON |
| 장착 슬롯 아이템 | `ItemSlotUI.CurrentItem` | clone이 Save 대상 | 장착 슬롯 UI, Save JSON |
| 장착 무기/방어구 ID | `Character.weapon_Name`, `armor_Name` | 장착 데이터에서 Load 시 재구성 | Component field |
| 공격력·방어력·최대 체력·속도·치명타 | `Character` | 직접 Save하지 않고 장비/스탯에서 재계산 | Component field, DPS/HP UI, 전투 결과 |
| 장착 원천 버프 | `Character.activeBuffs` | 직접 Save하지 않고 장비에서 재구성 | 디버그 요약, 버프 UI, 전투 결과 |
| OnHit/전투 시작 옵션 | `Character.OnHitOptions`, `OnBattleStartOptions` | 직접 Save하지 않고 장비에서 재구성 | Component field, 전투 트리거 결과 |
| 성장 스탯 | `PlayerState` | Save 대상 | Component field, Save JSON |

### 3.4 장착 전후 상태 예시

| State | Before | Trigger | After | Persistence |
|---|---|---|---|---|
| `inventoryItems` | 새 무기 포함 | 새 무기 장착 | 새 무기 제거, 교체된 무기가 있으면 그 clone 추가 | Save |
| `weaponEquipSlot.CurrentItem` | 비어 있음 또는 기존 무기 | 새 무기 장착 | 새 무기 | Save의 `equippedWeaponData` |
| `Character.weapon_Name` | null/빈 값 또는 기존 ID | 새 무기 장착 | 새 무기 ID | Load 시 장착 데이터에서 복원 |
| `Character.damage` | 기본 또는 기존 무기 계산값 | `Init()` | 새 무기 Master와 `PlayerState` scaling 계산값 | 재계산 |
| 원천 옵션 상태 | 없음 또는 기존 장비 원천 | 장비 교체 | 기존 원천 제거, 새 원천 적용/등록 | 재계산 |

### 3.5 데이터 연결 상태

정적 데이터 점검 결과:

- 무기: 46개, ID 누락 0, ID 중복 0, `ItemType`은 모두 `Weapon`
- 방어구: 44개, ID 누락 0, ID 중복 0, `ItemType`은 모두 `Armor`
- 옵션: 18개
- 장비 옵션 참조: 84건, `Option_Master`에 없는 참조 0
- 무기 계산은 `Weapon_DMG + PlayerState 스탯 × 각 scaling`을 정수로 변환한다.
- 방어구 계산은 `Armor_DEF`와 `Armor_HP`를 `Character.armor`, `Character.MaxHealth`에 직접 설정한다.
- 런타임 장비 효과 계산은 저장된 `ItemData` 수치보다 장비 ID로 다시 조회한 JSON Master를 기준으로 한다.

## 4. 기능 목적과 정상 흐름 초안

### 4.1 기능 목적

장비 장착/해제는 플레이어가 보유 아이템 중 무기와 방어구를 전투 상태에 반영하고, 다른 장비로 교체하거나 다시 인벤토리로 회수할 수 있게 한다. 이 기능이 없으면 획득한 장비가 전투 능력치와 옵션에 연결되지 않고, 장비 선택에 따른 플레이 변화가 성립하지 않는다.

### 4.2 정상 흐름

```text
Input
  인벤토리의 Weapon 또는 Armor 선택 후 장착 버튼 입력
Condition
  선택 아이템 존재, 지원 타입, 같은 아이템이 이미 같은 슬롯에 장착된 상태가 아님
Action
  기존 같은 종류 장비 복귀 -> 새 장비 이동 -> 장비 ID 갱신 -> 능력치/옵션 재계산
State Change
  inventoryItems, equipment slot, equipment ID, derived stats, option state 변경
Output
  장착 슬롯/인벤토리/UI/전투 계산이 동일한 장비 상태를 표시·사용
```

```text
Input
  장착 슬롯의 Weapon 또는 Armor 선택 후 해제 버튼 입력
Condition
  장착 아이템 존재, 반환 가능한 인벤토리 공간 존재
Action
  아이템 복귀 -> 원천 옵션 제거 -> 장비 ID/슬롯 제거 -> 남은 장비 기준 재계산
State Change
  inventoryItems 증가, equipment slot/ID 비움, derived stats/options 복원
Output
  해제 아이템을 인벤토리에서 다시 사용할 수 있고 전투에는 더 이상 영향을 주지 않음
```

## 5. Reconstructed Requirement 후보

### REQ-EQP-001 — 장비 타입별 슬롯 장착

- feature: Equipment
- description: 인벤토리의 `Weapon` 또는 `Armor` 아이템을 장착하면 각각 대응하는 장착 상태로 이동하고 인벤토리 목록에서는 제거되어야 한다.
- basis: `InventoryManager.ShowItemDetail()`, `OnClickEquip()`, `EquipmentSystem.EquipItem()`, 현재 Master 데이터의 일관된 `ItemType`
- confidence: High
- uncertainty: 프리팹의 오른손/왼손 중 어느 슬롯이 논리적 무기 슬롯인지, 양손 사용 정책은 불명확
- related_components: `InventoryManager`, `EquipmentSystem`, `ItemSlotUI`, `Character`
- related_requirements: `REQ-EQP-002`, `REQ-EQP-006`
- review_status: Draft

### REQ-EQP-002 — 같은 종류 장비 교체 시 보유 수량 보존

- feature: Equipment
- description: 같은 종류의 새 장비를 장착하면 기존 장비는 인벤토리로 1개 반환되고 새 장비는 인벤토리에서 1개 제거되어, 장비 인스턴스 총수와 다른 종류의 장착 상태가 보존되어야 한다.
- basis: `EquipmentSystem.EquipItem()`의 기존 슬롯 clone 추가 후 새 아이템 제거 순서
- confidence: High
- uncertainty: 아이템 인스턴스가 완전 비스택인지에 대한 공식 정책은 없으나 현재 `CountItemInstances()`와 clone 사용은 비스택 모델을 가리킴
- related_components: `EquipmentSystem`, `InventoryManager`, `ItemData`
- related_requirements: `REQ-EQP-001`, `REQ-EQP-004`
- review_status: Draft

### REQ-EQP-003 — 동일 장비 중복 장착의 멱등성

- feature: Equipment
- description: 이미 같은 슬롯에 장착된 동일 장비에 대한 장착 요청은 인벤토리, 슬롯, 능력치, 옵션 상태를 추가 변경하지 않아야 한다.
- basis: `InventoryManager.OnClickEquip()`과 `EquipmentSystem.EquipItem()`의 이중 동일 ID 방어
- confidence: High
- uncertainty: 서로 다른 인스턴스가 같은 `Item_ID`를 가질 때도 동일 장비로 볼지 사람의 확인 필요
- related_components: `InventoryManager`, `EquipmentSystem`, `Character`
- related_requirements: `REQ-EQP-005`
- review_status: Draft

### REQ-EQP-004 — 장착 해제와 인벤토리 복귀

- feature: Equipment
- description: 장착 아이템을 해제하면 해당 아이템 1개가 인벤토리로 복귀하고 대응 장착 슬롯과 장비 ID가 비워져야 한다. 반환 공간이 없으면 기존 장착 상태가 유지되어야 한다.
- basis: `InventoryManager.OnClickUnequip()`, `EquipmentSystem.UnequipItem()`
- confidence: High
- uncertainty: “공간”이 현재 활성 슬롯 수인지 절대 최대 14칸인지 공식 기준이 없음
- related_components: `InventoryManager`, `EquipmentSystem`, `ItemSlotUI`
- related_requirements: `REQ-EQP-002`, `REQ-EQP-006`
- review_status: Draft

### REQ-EQP-005 — 장비 능력치와 옵션의 단일 적용 및 복원

- feature: Equipment Effect
- description: 장착된 각 장비의 기본 능력치와 옵션은 현재 장비 상태를 기준으로 1회만 반영되고, 교체·해제·재초기화 후에는 이전 장비의 효과가 남거나 현재 장비의 효과가 사라지지 않아야 한다.
- basis: `EquipmentSystem.ClearInit()` 후 Master 재계산, 장비 원천 ID가 포함된 `BuffData`, `RemoveBuffByItem()`
- confidence: High
- uncertainty: 옵션별 중첩 정책은 `Option_Master`에 있으나 동일 Option을 서로 다른 장비가 제공할 때의 제품 정책은 별도 승인 필요
- related_components: `EquipmentSystem`, `Character`, `OptionManager`, JSON Master
- related_requirements: `REQ-EQP-003`, `REQ-EQP-007`
- review_status: Draft

### REQ-EQP-006 — 장비·인벤토리·표시 상태의 일관성

- feature: Equipment UI
- description: 장착/교체/해제 완료 후 인벤토리 목록, 장착 슬롯, 장비 ID, DPS/HP 표시가 동일한 장비 상태를 나타내야 한다.
- basis: 각 동작 후 `LoadInventory()`와 `UpdateDPS_MaxHealth()` 호출, `ItemSlotUI.CurrentItem`
- confidence: High
- uncertainty: 방어력과 개별 옵션 수치가 사용자 UI에 모두 표시되어야 하는지는 불명확
- related_components: `InventoryManager`, `ItemSlotUI`, `EquipmentSystem`, `Character`
- related_requirements: `REQ-EQP-001`, `REQ-EQP-004`, `REQ-EQP-005`
- review_status: Draft

### REQ-EQP-007 — 장비 상태의 Save/Load 재구성

- feature: Save / Load
- description: Save 후 Load하면 인벤토리 아이템, 장착 슬롯, 장비 ID, 파생 능력치와 장비 옵션 상태가 저장 시점과 논리적으로 동일하게 재구성되어야 한다. 저장 데이터에 장비가 없으면 기존 장비 상태가 남지 않아야 한다.
- basis: `SaveInventoryData()`, `LoadInventoryData()`, `SaveManager.ApplyPendingLoadDataOnce()`
- confidence: Medium
- uncertainty: Load가 항상 새 `GameScene` 인스턴스에서만 수행되는지, 동일 Runtime 객체에 적용될 수 있는지 수명주기 확인 필요
- related_components: `SaveManager`, `InventoryManager`, `EquipmentSystem`, `PlayerState`, `Character`
- related_requirements: `REQ-EQP-005`, `REQ-EQP-006`
- review_status: Draft

### REQ-EQP-008 — 잘못된 장비 데이터의 안전한 거부

- feature: Equipment Data
- description: null 아이템, 지원하지 않는 타입, 존재하지 않는 Master ID 또는 불완전한 장착 상태가 입력되면 아이템 손실이나 부분 장착 없이 요청이 거부되거나 복구 가능한 상태를 유지해야 한다.
- basis: null/타입 방어와 경고는 존재하지만 Master ID 검증은 상태 이동 뒤 `Init()`에서 수행됨
- confidence: Medium
- uncertainty: 잘못된 Save/구버전 데이터에 대한 제품 복구 정책이 없음
- related_components: `EquipmentSystem`, `InventoryManager`, `JsonManager`, `SaveManager`
- related_requirements: `REQ-EQP-001`, `REQ-EQP-007`
- review_status: Draft

## 6. 제품 Risk 후보

### RISK-EQP-001 — 반복 초기화 시 장비 패시브 능력치 소실

- condition: 치명타·공격속도·저항 등 패시브 능력치 옵션이 있는 장비를 유지한 채 `EquipmentSystem.Init()`가 다시 호출됨
- risk_event: `ClearInit()`가 일부 기본 능력치를 재설정하지만 `activeBuffs`의 동일 key는 유지된다. `AddBuff()`의 기존 key 갱신 경로는 `ApplyStatDelta(..., true)`를 다시 호출하지 않는다.
- impact: 장비는 장착 표시 상태인데 실제 전투 능력치가 장비 효과를 잃어 UI와 전투 결과가 왜곡될 수 있음
- likelihood: Medium
- impact_level: High
- priority: High
- related_requirement: `REQ-EQP-005`
- related_components: `EquipmentSystem`, `Character`, `OptionManager`, `PlayerStatsUI`
- mitigation: 동일 장비 상태에서 최초 장착, 2회 이상 `Init()`, 스탯 적용, Load 전후의 실제 능력치와 active buff를 비교
- remaining_risk: 옵션 종류마다 기존 key 갱신과 기본값 reset 범위가 달라 개별 검증 필요
- review_status: Draft

### RISK-EQP-002 — 현재 인벤토리 용량 초과로 해제 아이템이 UI에서 보이지 않음

- condition: `PlayerState.STR`에 따른 현재 활성 슬롯이 가득 찼지만 아이템 수가 절대 최대치 14보다 적을 때 장비 해제
- risk_event: 해제 전 검사가 `currnetSlotCount`가 아니라 `maxSlotCount`만 사용해 목록에 아이템을 추가하고, `LoadInventory()`는 활성 UI 슬롯 수까지만 표시
- impact: 해제한 아이템이 목록 내부에는 있으나 플레이어가 선택하거나 다시 장착하기 어려운 숨은 상태가 될 수 있음
- likelihood: High
- impact_level: High
- priority: High
- related_requirement: `REQ-EQP-004`, `REQ-EQP-006`
- related_components: `InventoryManager`, `EquipmentSystem`
- mitigation: 최소/중간 STR에서 활성 슬롯을 정확히 채운 후 장비 해제, 목록 수·표시 수·pending 목록·재장착 가능 여부 비교
- remaining_risk: 이후 STR 증가 시 아이템이 다시 보이는지, Save/Load 후 순서가 보존되는지 별도 확인
- review_status: Draft

### RISK-EQP-003 — 장비 없는 Save Load 후 이전 장비 상태 잔존

- condition: 현재 Runtime에 장비가 있는 상태에서 `equippedWeaponData` 또는 `equippedArmorData`가 null인 Save를 Load
- risk_event: `LoadInventoryData()`는 장비 데이터가 있을 때만 슬롯과 장비 ID를 설정하고, 없을 때 기존 슬롯/ID를 명시적으로 비우지 않음
- impact: Save 내용과 다른 장비 능력치·옵션이 유지되어 진행 상태와 전투 결과가 오염될 수 있음
- likelihood: Low 또는 Medium — 실제 객체 수명주기 확인 필요
- impact_level: High
- priority: High
- related_requirement: `REQ-EQP-007`
- related_components: `SaveManager`, `InventoryManager`, `EquipmentSystem`, `Character`
- mitigation: 동일 씬/씬 전환 양쪽에서 장비 있음 -> 장비 없음 Save Load를 수행하고 슬롯, ID, 능력치, 옵션을 비교
- remaining_risk: 신규 씬 객체에서는 나타나지 않고 특정 Load 경로에서만 나타날 수 있음
- review_status: Draft

### RISK-EQP-004 — 존재하지 않는 Master ID의 부분 장착

- condition: 구버전 Save, 손상 데이터 또는 외부 입력으로 ID는 있으나 현재 Master에 없는 Weapon/Armor `ItemData`가 장착됨
- risk_event: 슬롯 설정·인벤토리 제거·장비 ID 갱신은 먼저 수행되지만 `Init()`의 Master 조회는 null이 되어 장비 능력치/옵션만 적용되지 않음
- impact: 아이템은 장착 표시되나 효과가 없고 Save에 다시 기록될 수 있음
- likelihood: Low
- impact_level: Medium
- priority: Medium
- related_requirement: `REQ-EQP-008`
- related_components: `EquipmentSystem`, `JsonManager`, `InventoryManager`
- mitigation: missing ID 입력 시 상태 변경 전후와 경고, Save 재기록 여부 확인
- remaining_risk: 데이터 버전 마이그레이션 정책이 없으면 업데이트 시 재발 가능
- review_status: Draft

### RISK-EQP-005 — Save 시 장비 ID와 슬롯 데이터 불일치로 예외 또는 잘못된 저장

- condition: `Character.weapon_Name`/`armor_Name`은 존재하지만 대응 `CurrentItem`이 null인 부분 상태에서 Save
- risk_event: `SaveInventoryData()`가 장비 ID 존재 여부만 보고 `CurrentItem.Clone()`을 호출
- impact: Save 동작 중 예외가 발생하거나 장비 데이터가 저장되지 않아 진행 기록이 손상될 수 있음
- likelihood: Low
- impact_level: High
- priority: Medium
- related_requirement: `REQ-EQP-007`, `REQ-EQP-008`
- related_components: `InventoryManager`, `SaveManager`, `ItemSlotUI`, `Character`
- mitigation: 의도적으로 ID/슬롯 불일치 상태를 구성해 Save 실패 처리와 기존 Save 보존 여부 확인
- remaining_risk: 예외가 `SaveGame()` 전체를 중단시키는 범위 확인 필요
- review_status: Draft

### RISK-EQP-006 — 비무기 타입의 해제 요청이 방어구 슬롯으로 라우팅

- condition: UI 표시 제약을 우회하거나 stale selection으로 `Weapon`이 아닌 `selectedItem`에서 `OnClickUnequip()` 호출
- risk_event: 삼항 연산자가 모든 비무기 타입을 방어구 슬롯으로 선택
- impact: 예상하지 않은 방어구 해제 또는 무반응이 발생할 수 있음
- likelihood: Low
- impact_level: Medium
- priority: Low
- related_requirement: `REQ-EQP-008`
- related_components: `InventoryManager`
- mitigation: Armor 외 `Consumable`, `Item`, 알 수 없는 타입으로 해제 요청 시 상태 불변 확인
- remaining_risk: 정상 UI 흐름에서는 버튼이 노출되지 않아 재현되지 않을 수 있음
- review_status: Draft

### RISK-EQP-007 — 무기 슬롯/한손 데이터 정책 불일치

- condition: 오른손·왼손 슬롯과 `One-Handed` 데이터가 함께 존재하는 콘텐츠를 사용
- risk_event: 현재 런타임은 단일 `weapon_Name`과 단일 `weaponEquipSlot`만 관리하며 `One_Handed`는 표시용 데이터로만 전달됨
- impact: 왼손 슬롯이 비기능 UI가 되거나 한손/양손 장비 조합이 기획 의도와 다르게 동작할 수 있음
- likelihood: Medium
- impact_level: Medium
- priority: Medium
- related_requirement: `REQ-EQP-001`
- related_components: `InventoryManager`, `EquipmentSystem`, `ItemSlotUI`, `Weapon_Master`, 인벤토리 프리팹
- mitigation: 제품 소유자가 현재 목표를 “단일 무기 슬롯” 또는 “양손 슬롯” 중 하나로 승인한 뒤 조건 재작성
- remaining_risk: 승인 전에는 동작 차이를 Defect로 판정할 수 없음
- review_status: Draft

## 7. Test Condition 후보

| ID | Title | Requirement | Risk | Condition | Verification Target | Expected Behavior | Priority | Recommended Execution Method |
|---|---|---|---|---|---|---|---|---|
| COND-EQP-001 | 빈 슬롯에 무기 장착 | REQ-EQP-001 | - | 인벤토리 무기 1개를 빈 무기 상태에서 장착 | 목록 수, 무기 슬롯, `weapon_Name`, damage, UI | 무기 1개가 목록에서 슬롯으로 이동하고 ID·공격력·UI가 일치 | High | unity_mcp 또는 unity_test_runner |
| COND-EQP-002 | 빈 슬롯에 방어구 장착 | REQ-EQP-001 | - | 인벤토리 방어구 1개를 빈 방어구 상태에서 장착 | 목록 수, 방어구 슬롯, `armor_Name`, armor/MaxHealth, UI | 방어구 1개가 이동하고 ID·능력치·UI가 일치 | High | unity_mcp 또는 unity_test_runner |
| COND-EQP-003 | 무기 교체 시 인스턴스 보존 | REQ-EQP-002 | - | 무기 A 장착 중 인벤토리 무기 B 장착 | 두 ID별 총수, 슬롯, 목록, damage | A는 1개 반환되고 B는 1개 제거되며 B만 효과를 제공 | High | unity_test_runner |
| COND-EQP-004 | 방어구 교체 시 인스턴스 보존 | REQ-EQP-002 | - | 방어구 A 장착 중 인벤토리 방어구 B 장착 | 두 ID별 총수, 슬롯, 목록, armor/MaxHealth | A 반환, B 제거, B만 효과 제공 | High | unity_test_runner |
| COND-EQP-005 | 동일 ID 반복 장착 멱등성 | REQ-EQP-003 | RISK-EQP-001 | 같은 ID 장착 요청을 연속 수행 | 목록/슬롯 수, stats, active buffs, trigger option 수 | 최초 완료 후 추가 상태 변화나 중복 효과 없음 | High | unity_test_runner |
| COND-EQP-006 | 동일 장비 상태에서 Init 반복 | REQ-EQP-005 | RISK-EQP-001 | 패시브 옵션 장비 장착 후 `Init()` 2회 이상 | CitChance, speed, resistance, active buffs, UI | 각 호출 후 동일한 1회 적용 결과 유지 | High | unity_test_runner |
| COND-EQP-007 | 스탯 적용 후 장비 패시브 유지 | REQ-EQP-005 | RISK-EQP-001 | 패시브 장비 장착 후 PlayerStats 적용 | PlayerState, Character stats, active buffs | 성장 스탯과 장비 효과가 새 기준에서 정확히 1회 반영 | High | unity_mcp 또는 unity_test_runner |
| COND-EQP-008 | 공간이 있는 상태에서 무기 해제 | REQ-EQP-004 | - | 인벤토리에 현재 활성 빈칸이 있을 때 무기 해제 | 목록, 슬롯, `weapon_Name`, stats/options | 아이템 1개 복귀, 슬롯/ID 비움, 효과 제거 | High | unity_test_runner |
| COND-EQP-009 | 공간이 있는 상태에서 방어구 해제 | REQ-EQP-004 | - | 인벤토리에 현재 활성 빈칸이 있을 때 방어구 해제 | 목록, 슬롯, `armor_Name`, stats/options | 아이템 1개 복귀, 슬롯/ID 비움, 효과 제거 | High | unity_test_runner |
| COND-EQP-010 | 현재 활성 인벤토리가 가득 찬 상태에서 해제 | REQ-EQP-004, REQ-EQP-006 | RISK-EQP-002 | 활성 슬롯 수만큼 아이템을 채우고 14개 미만인 상태에서 해제 | 내부 목록 수, 표시 슬롯 수, pending, 장비 상태 | 승인 정책에 따라 해제 거부 또는 보이는/pending 위치로 안전 이동; 숨은 아이템 금지 | High | unity_mcp + 상태 캡처 |
| COND-EQP-011 | 절대 최대 14개 상태에서 해제 | REQ-EQP-004 | RISK-EQP-002 | 목록 14개 상태에서 장비 해제 | 목록, 슬롯, ID, stats | 해제 거부 및 기존 장비 상태 완전 유지 | High | unity_test_runner |
| COND-EQP-012 | 장비 상태 Save/Load 왕복 | REQ-EQP-007 | RISK-EQP-001 | 무기·방어구 장착 후 격리 경로에 Save하고 Load | Save JSON, 슬롯, ID, stats, options, inventory | 저장 시점과 논리적으로 같은 상태를 1회 효과로 복원 | High | unity_test_runner; 반드시 격리 Save 경로 |
| COND-EQP-013 | 장비 없음 Save로 기존 상태 제거 | REQ-EQP-007 | RISK-EQP-003 | 장비 있음 Runtime에 장비 없음 Save Load | 슬롯, ID, stats, active buffs, UI | 이전 장비 상태가 모두 제거되고 Save 상태와 일치 | High | unity_mcp 또는 playmode; 격리 Save 경로 |
| COND-EQP-014 | 존재하지 않는 장비 ID 입력 | REQ-EQP-008 | RISK-EQP-004 | 없는 Weapon/Armor ID의 ItemData 장착 | 목록, 슬롯, ID, stats, Console | 아이템 손실/부분 장착 없이 안전 거부 또는 승인된 복구 동작 | Medium | unity_test_runner |
| COND-EQP-015 | 장비 ID와 슬롯 불일치 Save | REQ-EQP-007, REQ-EQP-008 | RISK-EQP-005 | ID 존재/CurrentItem null 및 반대 조합 | Save 결과, 기존 파일, Console | 예외로 전체 저장을 손상시키지 않고 불일치를 탐지·처리 | High | unity_test_runner; 격리 Save 경로 |
| COND-EQP-016 | 비장비 타입 해제 요청 | REQ-EQP-008 | RISK-EQP-006 | Consumable/Item/unknown type으로 해제 요청 | armor slot, inventory, IDs, Console | 기존 장비 상태 불변, 요청 안전 거부 | Low | unity_test_runner |
| COND-EQP-017 | 서로 다른 장비의 동일 Option ID | REQ-EQP-005 | RISK-EQP-001 | 동일 옵션 ID를 가진 무기/방어구 동시 장착·한쪽 해제 | source item별 buff/trigger, stats | 승인된 중첩 규칙대로 분리되고 한쪽 해제 시 다른 원천 효과 유지 | High | unity_test_runner |
| COND-EQP-018 | 씬 전환 전후 장비 상태 | REQ-EQP-006, REQ-EQP-007 | RISK-EQP-003 | 장비 장착 후 씬 전환 및 GameScene 재진입 | PlayerState 참조, 슬롯, ID, stats/options | 승인된 지속성 정책과 일치하며 중복/잔존 없음 | Medium | unity_mcp 또는 playmode; Save 비사용/사용 분리 |

## 8. Requirement Gap

### GAP-EQP-001 — 무기 슬롯과 한손/양손 정책 부재

- description: 오른손·왼손 슬롯과 `One-Handed` 데이터는 존재하지만 단일 `weapon_Name`/`weaponEquipSlot`만 실제 장착 흐름에 연결됨
- reason: 단일 슬롯 게임인지, 양손 조합을 지원해야 하는지 공식 기준 없음
- potential_impact: UI와 실제 장착 가능 조합 불일치, 잘못된 TC 생성
- related_requirement: `REQ-EQP-001`
- review_status: Draft

### GAP-EQP-002 — 방어구 HP의 의미와 계산 규칙 부재

- description: `Armor_HP`가 기본 최대 체력에 더해지는 값인지, 최대 체력을 대체하는 값인지 불명확
- reason: 현재 코드는 `player.MaxHealth = armor.Armor_HP`로 대체하지만 공식 기획 근거 없음
- potential_impact: 방어구와 Health 성장 스탯의 관계 및 밸런스 판정 불가
- related_requirement: `REQ-EQP-005`
- review_status: Draft

### GAP-EQP-003 — 현재 인벤토리 용량과 절대 최대치의 우선순위 부재

- description: STR 기반 활성 슬롯 수와 고정 최대 14칸 중 장비 해제 시 어떤 값을 용량으로 사용할지 불명확
- reason: 아이템 획득 축소 경로에는 `pendingItems`가 있지만 장비 해제는 이를 사용하지 않음
- potential_impact: 해제 거부 또는 숨은 아이템 처리의 Expected Behavior를 확정할 수 없음
- related_requirement: `REQ-EQP-004`, `REQ-EQP-006`
- review_status: Draft

### GAP-EQP-004 — 동일 옵션의 다중 원천 중첩 정책

- description: 서로 다른 장비가 동일 Option ID를 제공할 때 합산, 독립, 최대값, 무시 중 어떤 규칙인지 기능 전체 기준이 없음
- reason: `StackPolicy`는 버프 데이터에 있으나 장비 조합에 대한 제품 규칙이 별도로 정의되지 않음
- potential_impact: 정상적인 중첩과 능력치 오염을 구분하기 어려움
- related_requirement: `REQ-EQP-005`
- review_status: Draft

### GAP-EQP-005 — 손상·구버전 Save의 장비 복구 정책

- description: 없는 ID, 장비 ID/슬롯 불일치, null 목록, 구버전 필드 누락 시 유지·제거·기본값·Load 중단 중 정책이 없음
- reason: 일부 null 방어는 있으나 장비 단위 복구 계약이 없음
- potential_impact: 데이터 손실 또는 잘못된 장비 상태를 Defect로 판정하기 어려움
- related_requirement: `REQ-EQP-007`, `REQ-EQP-008`
- review_status: Draft

### GAP-EQP-006 — 장착/해제 가능 시점

- description: 전투 중, 사망 처리 중, 씬 전환 중에도 장착/해제가 허용되는지 불명확
- reason: 현재 메서드에는 Game State 조건이 없음
- potential_impact: 전투 도중 능력치·옵션 재계산 순서와 결과가 달라질 수 있음
- related_requirement: `REQ-EQP-001`, `REQ-EQP-004`, `REQ-EQP-005`
- review_status: Draft

## 9. Finding 후보

| Finding ID | 분류 | 내용 | 근거 수준 | 다음 판단 |
|---|---|---|---|---|
| FIND-EQP-001 | Defect Candidate | 반복 `Init()`에서 기존 passive buff의 능력치 delta가 재적용되지 않을 가능성 | Static Inference, High signal | COND-EQP-006/007 Runtime 재현 후 사람 판정 |
| FIND-EQP-002 | Quality Concern | 해제 공간 검사가 활성 슬롯 수가 아닌 14개를 사용 | Source confirmed, 사용자 영향은 Runtime Needed | GAP-EQP-003 승인 후 COND-EQP-010 실행 |
| FIND-EQP-003 | Need Review | 장비 없는 Save Load 시 기존 장비가 남을 가능성 | Static Inference, lifecycle dependent | COND-EQP-013을 동일 씬/씬 전환으로 분리 실행 |
| FIND-EQP-004 | Requirement Gap | 오른손/왼손/One-Handed 정책이 구현과 데이터 사이에서 확정되지 않음 | Source + Scene + Data | 제품 소유자 승인 필요 |
| FIND-EQP-005 | Expected Behavior | 현재 Master 장비 ID와 옵션 참조는 정적으로 일관됨 | Data confirmed | 데이터 변경 시 회귀 조건 유지 |
| FIND-EQP-006 | Requirement Gap | Armor HP가 base에 더해지는지 대체하는지 불명확 | Source confirmed, intent unknown | GAP-EQP-002 승인 필요 |

## 10. 기존 테스트 소스와 현재 Coverage Gap

현재 `Assets/Tests/EditMode/P0DataCompatibilityTests.cs`에는 다음 단일 초기화 기반 검증 의도가 존재한다.

- 방어구 저항 옵션 적용과 `RemoveBuffByItem()` 복원
- 방어구 치명타 옵션 적용
- 무기 OnHit 옵션 등록과 전투 효과 호출

이번 Pilot에서는 이 테스트를 실행하지 않았다. 또한 현재 검색 범위에서는 다음 흐름을 직접 검증하는 기존 테스트를 확인하지 못했다.

- 실제 `EquipItem()`/`UnequipItem()`의 목록·슬롯·ID 원자성
- 같은 장비 상태에서 `Init()` 반복
- 현재 활성 인벤토리 용량이 가득 찬 상태의 해제
- 장비 있음/없음 Save 간 Load 전환
- ID/슬롯 불일치 Save
- 오른손/왼손/One-Handed 정책

이 Coverage Gap은 Adventure에서 새 TC를 바로 만들라는 의미가 아니다. 승인된 Test Condition을 MyHarnessProject가 구체적인 TC와 실행 방식으로 변환할 때 사용하는 입력이다.

## 11. 사람 검토 요청

다음 항목을 승인 또는 보류해야 Pilot의 Expected Behavior를 확정할 수 있다.

1. 현재 제품의 논리적 무기 슬롯은 1개인가, 오른손/왼손 2개인가?
2. `One-Handed`는 현재 릴리스 범위에서 기능 규칙인가, 미래용 데이터인가?
3. `Armor_HP`는 최대 체력 대체값인가, 기본/성장 체력에 대한 추가값인가?
4. 장비 해제 시 “인벤토리 공간”은 현재 활성 슬롯 수인가, 절대 최대 14개인가?
5. 현재 활성 슬롯이 가득 찬 경우 해제를 거부할지 `pendingItems`로 보낼지?
6. 서로 다른 장비가 같은 Option ID를 제공하면 합산/독립/최대값/무시 중 어느 규칙인가?
7. 전투 중 장착/해제가 허용되는가?
8. 손상·구버전 Save의 없는 장비 ID는 제거, 기본 장비 대체, Load 실패 중 어떻게 처리할 것인가?

## 12. Harness 전달용 추적성

```text
Equipment
  REQ-EQP-001 -> COND-EQP-001, 002, 014
  REQ-EQP-002 -> COND-EQP-003, 004
  REQ-EQP-003 -> RISK-EQP-001 -> COND-EQP-005
  REQ-EQP-004 -> RISK-EQP-002 -> COND-EQP-008, 009, 010, 011
  REQ-EQP-005 -> RISK-EQP-001 -> COND-EQP-005, 006, 007, 017
  REQ-EQP-006 -> RISK-EQP-002 -> COND-EQP-001, 002, 010, 018
  REQ-EQP-007 -> RISK-EQP-003, 005 -> COND-EQP-012, 013, 015, 018
  REQ-EQP-008 -> RISK-EQP-004, 005, 006 -> COND-EQP-014, 015, 016
```

후속 TC ID, Run ID, Finding ID는 MyHarnessProject에서 승인 후 연결한다. 이 문서의 `COND-*`는 TC가 아니며 실행 결과도 포함하지 않는다.

## 13. Pilot 성공 조건 평가

| 성공 조건 | 현재 상태 |
|---|---|
| 현재 구현 구조 설명 | 충족 — 정적 근거 기반 |
| 정상 동작 정의 | Draft 충족 — Gap 승인 필요 |
| 근거 포함 Requirement | 8개 Draft |
| Confidence/Uncertainty 구분 | 충족 |
| 주요 Product Risk | 7개 Draft |
| Risk-Requirement 연결 | 충족 |
| Test Condition 도출 | 18개 Draft |
| Harness용 ID 구조 | 충족 |
| 판단 불가 항목 분리 | 6개 Gap |

## 14. 다음 단계 제안

1. 사람 검토로 8개 Requirement와 6개 Gap의 정책을 승인/수정/보류한다.
2. High Priority인 `COND-EQP-006`, `010`, `013`을 우선 Harness 전달 대상으로 선정한다.
3. MyHarnessProject에서 구체 TC, fixture, oracle, 실행 경로를 설계한다.
4. Save/Load 조건은 실제 사용자 `save.json`과 분리된 경로가 증명된 뒤에만 실행한다.
5. 실제 실패가 재현되어도 Requirement·Test Data·Environment·Infrastructure를 분리한 뒤 사람이 Defect를 확정한다.
