# 옵션 효과 작성 가이드

이 문서는 `Assets/ExcelFiles/AllItem_Master.xlsx`의 `Option_Master` 시트에 옵션 효과를 추가할 때 넣어야 하는 값을 정리한다.

현재 런타임 기준 파일:
- 엑셀 원본: `Assets/ExcelFiles/AllItem_Master.xlsx`
- 생성 JSON: `Assets/Resources/Events/Option_Master.json`
- 생성 자료형: `Assets/Json/Option_Master.cs`
- 효과 처리 코드: `Assets/Script/combat/OptionManager.cs`, `Assets/Script/combat/Character.cs`

## 기본 컬럼

| 컬럼 | 의미 | 예시 |
| --- | --- | --- |
| `Option_ID` | 옵션 고유 ID. 장비의 `Option_1_ID`, `Option_2_ID`에서 참조한다. | `Option_009` |
| `Option_Description` | 사람이 읽는 설명. 게임 내 표시나 확인용 설명이다. | `출혈: 공격 시 스택 기반 확률로 최대 체력 비례 고정 피해` |
| `Effect_ID` | 실제 처리할 효과 ID. `OptionManager`의 효과 딕셔너리에 등록되어 있어야 한다. | `Effect_009` |
| `Option_Type` | 옵션 발동 분류. 장비 옵션은 보통 `Passive` 또는 `OnHit`를 쓴다. | `OnHit` |

주의:
- 새로운 `Effect_ID`를 완전히 새로 만들면 `OptionManager` 코드에도 효과 클래스를 추가해야 한다.
- 기존 효과 처리기를 재사용하려면 아래 등록된 `Effect_ID` 중 하나를 사용한다.

## 상태 효과 메타 컬럼

| 컬럼 | 의미 | 예시 |
| --- | --- | --- |
| `StatusType` | 상태 종류. 같은 스택형 상태는 이 값을 기준으로 중첩된다. | `Bleed`, `Poison`, `Holy`, `Regen`, `Freeze` |
| `ApplyMode` | 적용 방식. 패시브, 즉시, 스택형, 틱형 등을 구분한다. | `Passive`, `Instant`, `Stacking`, `TimedTick`, `OneShotState` |
| `StackPolicy` | 같은 상태가 다시 들어왔을 때 처리 방식. | `Ignore`, `Refresh`, `Stack`, `Independent` |
| `MaxStack` | 최대 스택 수. 스택형이 아니면 보통 `1`. | `99` |
| `Duration` | 지속 시간 초. `0`이면 현재 구현에서는 전투 종료 전까지 유지되는 상태로 본다. | `5`, `0` |
| `TickInterval` | 틱 간격 초. 현재 화상/회복 틱은 기존 루틴 기준 1초 단위다. | `1` |
| `TriggerType` | 언제 효과가 발동되는지 나타내는 값. | `OnEquip`, `OnHit`, `OnTick`, `OnAttack`, `BeforeAttack` |
| `ValueMode` | 수치 해석 방식. | `Flat`, `Percent`, `PercentMaxHP`, `StackCount` |
| `BaseChance` | 기본 발동 확률. 스택형 공식에 사용된다. | `7` |
| `ChancePerStack` | 스택이 늘 때 추가되는 확률. | `7` |
| `BaseValue` | 기본 효과 수치. | `2` |
| `ValuePerStack` | 스택이 늘 때 추가되는 효과 수치. | `2` |
| `StatType` | 스탯형 효과가 건드리는 스탯 이름. | `Speed`, `CritChance` |
| `ResistanceType` | 저항 타입. 현재 출혈/중독/빙결 쪽에서 사용한다. | `BleedResist`, `PoisonResist`, `FreezeResist` |
| `MaxRemoveCount` | 디버프 제거 효과의 최대 제거 개수. 현재 신성 정화에서 사용한다. | `3` |

## 장비의 Option_Value 해석

장비 데이터의 `Option_Value1`, `Option_Value2`는 효과 종류에 따라 다르게 해석된다.

| 효과 종류 | `Option_Value` 의미 |
| --- | --- |
| `Passive` 스탯 효과 | 증가/감소 수치. 예: 크리티컬 확률 `20`, 공격속도 `50` |
| `TimedTick` 화상/회복 | 대상 최대 체력 대비 퍼센트. 예: `10`이면 10% |
| `Stacking` 출혈/중독/신성/재생/빙결 | 한 번 적용할 때 추가할 스택 수. 예: `2`면 적중 시 2스택 부여 |
| `OneShotState` 다음 공격 실패 | 추가할 1회성 차지 수. 예: `1`이면 다음 공격 1회 실패 |
| `Instant` 즉시 효과 | 즉시 적용할 고정값 |

예시:
- 화상 무기의 `Option_Value1 = 10`이면 즉시 10%, 이후 틱마다 10% 피해.
- 출혈 무기의 `Option_Value1 = 2`이면 적중할 때마다 출혈 2스택 추가.

## 스택 공식

현재 스택형 상태는 아래 공식을 사용한다.

```text
발동 확률 = BaseChance + (StackCount - 1) * ChancePerStack
효과 수치 = BaseValue + (StackCount - 1) * ValuePerStack
```

예시: 출혈

```text
BaseChance = 7
ChancePerStack = 7
BaseValue = 2
ValuePerStack = 2
```

결과:
- 1스택: 7% 확률, 최대 체력 2% 피해
- 2스택: 14% 확률, 최대 체력 4% 피해
- 3스택: 21% 확률, 최대 체력 6% 피해

저항이 있으면 최종 피해에서 감소한다.

```text
최종 피해 = 계산 피해 * (100 - DebuffDamageResist - 특정 저항) / 100
```

예시:
- 출혈 피해 4%
- `BleedResist = 50`
- 최종 피해 2%

## 현재 등록된 효과

### 추가 공격력

| 컬럼 | 값 |
| --- | --- |
| `Option_ID` | `Option_001` |
| `Effect_ID` | `Effect_001` |
| `ApplyMode` | `Instant` |
| `Option_Type` | `OnHit` |
| `ValueMode` | `Flat` |

적중 시 `Option_Value`만큼 피해를 추가로 준다.

### 크리티컬 확률 증가

| 컬럼 | 값 |
| --- | --- |
| `Option_ID` | `Option_002` |
| `Effect_ID` | `Effect_002` |
| `ApplyMode` | `Passive` |
| `StackPolicy` | `Ignore` |
| `StatType` | `CritChance` |

장비 착용 중 유지된다. 같은 장비에서 중복 부여하지 않는 패시브형으로 본다.

### 화상

| 컬럼 | 값 |
| --- | --- |
| `Option_ID` | `Option_003` |
| `Effect_ID` | `Effect_003` |
| `ApplyMode` | `TimedTick` |
| `StackPolicy` | `Refresh` |
| `TriggerType` | `OnTick` |
| `ValueMode` | `PercentMaxHP` |
| `Duration` | `5` |
| `TickInterval` | `1` |

적중 시 대상에게 즉시 `Option_Value%` 피해를 주고, 지속 중 틱마다 같은 비율의 피해를 준다.

### 회복

| 컬럼 | 값 |
| --- | --- |
| `Option_ID` | `Option_004` |
| `Effect_ID` | `Effect_004` |
| `ApplyMode` | `TimedTick` |
| `StackPolicy` | `Refresh` |
| `TriggerType` | `OnTick` |
| `ValueMode` | `PercentMaxHP` |
| `Duration` | `5` |
| `TickInterval` | `1` |

사용자에게 즉시 `Option_Value%` 회복을 주고, 지속 중 틱마다 같은 비율로 회복한다.

### 공격 속도 증가

| 컬럼 | 값 |
| --- | --- |
| `Option_ID` | `Option_005` |
| `Effect_ID` | `Effect_005` |
| `ApplyMode` | `Passive` |
| `StackPolicy` | `Ignore` |
| `StatType` | `Speed` |
| `ValueMode` | `Percent` |

장비 착용 중 공격 속도를 `Option_Value%`만큼 증가시킨다.

### 다음 공격 실패

| 컬럼 | 값 |
| --- | --- |
| `Option_ID` | `Option_008` |
| `Effect_ID` | `Effect_008` |
| `ApplyMode` | `OneShotState` |
| `StackPolicy` | `Stack` |
| `TriggerType` | `BeforeAttack` |

적중 시 대상에게 다음 공격 실패 차지를 추가한다. `Option_Value`는 추가 차지 수다.

### 출혈

| 컬럼 | 값 |
| --- | --- |
| `Option_ID` | `Option_009` |
| `Effect_ID` | `Effect_009` |
| `StatusType` | `Bleed` |
| `ApplyMode` | `Stacking` |
| `StackPolicy` | `Stack` |
| `TriggerType` | `OnAttack` |
| `ValueMode` | `PercentMaxHP` |
| `ResistanceType` | `BleedResist` |

공격자가 출혈 상태라면, 공격할 때 확률로 자기 최대 체력 비례 피해를 받는다.

추천 기본값:

```text
MaxStack = 99
BaseChance = 7
ChancePerStack = 7
BaseValue = 2
ValuePerStack = 2
```

### 중독

| 컬럼 | 값 |
| --- | --- |
| `Option_ID` | `Option_010` |
| `Effect_ID` | `Effect_010` |
| `StatusType` | `Poison` |
| `ApplyMode` | `Stacking` |
| `StackPolicy` | `Stack` |
| `TriggerType` | `OnAttack` |
| `ValueMode` | `PercentMaxHP` |
| `ResistanceType` | `PoisonResist` |

공격자가 중독 상태라면, 공격할 때 확률로 자기 최대 체력 비례 피해를 받는다.

### 신성

| 컬럼 | 값 |
| --- | --- |
| `Option_ID` | `Option_011` |
| `Effect_ID` | `Effect_011` |
| `StatusType` | `Holy` |
| `ApplyMode` | `Stacking` |
| `StackPolicy` | `Stack` |
| `TriggerType` | `OnAttack` |
| `ValueMode` | `Percent` |
| `MaxRemoveCount` | `3` |

공격자가 신성 상태라면:
- 확률로 적의 다음 공격 실패 확률을 올린다.
- 별도 확률로 자기 디버프를 제거한다.

현재 정화 확률과 제거 개수는 코드에 아래처럼 고정되어 있다.

```text
정화 확률 = 2 * max(1, StackCount / 2)
제거 개수 = min(MaxRemoveCount, 1 + StackCount / 3)
```

정수 나눗셈 기준이라 `StackCount / 2`, `StackCount / 3`은 소수점을 버린다.

### 재생

| 컬럼 | 값 |
| --- | --- |
| `Option_ID` | `Option_012` |
| `Effect_ID` | `Effect_012` |
| `StatusType` | `Regen` |
| `ApplyMode` | `Stacking` |
| `StackPolicy` | `Stack` |
| `TriggerType` | `OnAttack` |
| `ValueMode` | `PercentMaxHP` |

공격자가 재생 상태라면, 공격할 때 확률로 자기 최대 체력 비례 회복을 받는다.

### 빙결

| 컬럼 | 값 |
| --- | --- |
| `Option_ID` | `Option_013` |
| `Effect_ID` | `Effect_013` |
| `StatusType` | `Freeze` |
| `ApplyMode` | `Stacking` |
| `StackPolicy` | `Stack` |
| `TriggerType` | `BeforeAttack` |
| `ValueMode` | `Percent` |
| `StatType` | `Speed` |
| `ResistanceType` | `FreezeResist` |

빙결 상태라면:
- 행동 전 확률로 행동이 막힌다.
- 속도가 스택 수에 따라 감소한다.

추천 기본값:

```text
MaxStack = 10
BaseChance = 10
ChancePerStack = 5
BaseValue = 10
ValuePerStack = 5
```

예시:
- 1스택: 행동 불가 10%, 속도 10% 감소
- 2스택: 행동 불가 15%, 속도 15% 감소
- 3스택: 행동 불가 20%, 속도 20% 감소

## 새 옵션 추가 절차

1. `AllItem_Master.xlsx`의 `Option_Master` 시트에 새 행을 추가한다.
2. 기존 효과 처리기를 재사용할 경우 `Effect_ID`는 기존 값을 사용한다.
3. 완전히 새 효과라면 `OptionManager`에 새 `IOptionEffect` 구현과 딕셔너리 등록을 추가한다.
4. 장비 시트에서 `Option_1_ID` 또는 `Option_2_ID`에 새 `Option_ID`를 넣는다.
5. 장비의 `Option_Value1` 또는 `Option_Value2`를 효과에 맞게 넣는다.
6. Unity 메뉴 `Tools/Excel Auto Generator`로 JSON과 자료형을 재생성한다.
7. EditMode 테스트와 간단한 런타임 스모크를 확인한다.

## 작성 예시

### 출혈 1스택 부여 무기

`Weapon_Master`

```text
Option_1_ID = Option_009
Option_Value1 = 1
```

의미:
- 적중 시 대상에게 출혈 1스택 부여.

### 강한 출혈 3스택 부여 무기

`Weapon_Master`

```text
Option_1_ID = Option_009
Option_Value1 = 3
```

의미:
- 적중 시 대상에게 출혈 3스택 부여.

### 10% 화상 무기

`Weapon_Master`

```text
Option_1_ID = Option_003
Option_Value1 = 10
```

의미:
- 적중 시 대상 최대 체력의 10% 즉시 피해.
- 이후 지속 틱마다 10% 피해.

## 현재 구현상 주의점

- `Option_Value`는 효과 종류마다 의미가 다르다. 스택형은 스택 수, 화상/회복은 퍼센트다.
- `BaseChance`, `BaseValue`는 스택형 상태에서 주로 사용한다.
- `Duration`, `TickInterval`은 현재 모든 상태에 완전히 일반화된 것은 아니다. 화상/회복 틱형은 기존 루틴 중심으로 동작한다.
- `DebuffDamageResist`, `BleedResist`, `PoisonResist`, `FreezeResist`는 현재 `Character` 필드로 존재하지만, 아직 엑셀 스탯 데이터나 장비 스탯과 완전히 연결된 것은 아니다.
- `Effect_ID`가 `OptionManager` 딕셔너리에 없으면 효과는 실행되지 않는다.
