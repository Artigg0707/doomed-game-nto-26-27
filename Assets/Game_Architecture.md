# Архитектура игры «Doomed Game» — Полный архитектурный граф

Документ описывает модульную архитектуру Unity-проекта: назначение каждого модуля, входы/выходы, обмен данными, события и ветвления логики. Основано на анализе всех C#-скриптов и иерархии сцены.

---

## 1. Общая схема (High-Level)

Игра построена по паттерну **«Менеджеры-синглтоны + данные-ScriptableObject + UI-событийная модель»**. Все ключевые системы — синглтоны (`Instance`), доступны глобально. Данные хранятся в сериализуемых классах (`ResourceData`, `GeneratedCharacter`, `NightReportData`) и `ScriptableObject`-ах (`ExpeditionLocationData`, `CharaterObject`). Взаимодействие — через C# `events` (`Action`) и прямые вызовы менеджеров.

**Главный цикл:**
```
Игрок → Interactor (луч) → IInteractable (дверь, ПК, кровать, щиток, еда)
    → Открытие UI-панели → Игрок меняет состояние
    → Менеджеры обрабатывают
    → Ночной цикл (DayCycleManager.StartNextDay)
    → Утренняя сводка (MorningReportPanel)
    → Победа/Поражение (DefeatScreenPanel)
```

---

## 2. Ядро: Менеджеры (Singletons)

| Менеджер | Ответственность | Ключевые поля/свойства | События (выходы) | Входы (зависимости) | Ветвления/логика |
|----------|-----------------|------------------------|------------------|---------------------|------------------|
| **ResourceManager** | Единый источник истины по ресурсам: еда, вода, топливо, температура, состояние генератора. | `foodCans`, `waterBottles`, `fuelCans`, `temperatureLevel` (0–3), `generatorBroken`, `generatorIsOn`, `GeneratorStatus` (строка), `CanTurnOnGenerator` | `OnResourcesChanged (Action)` | `ResourceData` (ScriptableObject) | Консьюмерские методы: `ConsumeFood/Water/Fuel` (bool), `AddFood/Water/Fuel`, `SetTemperature`, `BreakGenerator`, `RepairGenerator`, `SetGeneratorOn`. Валидация: не даёт уйти ниже 0, температура клампится 0–3. |
| **CharacterManager** | Управление списком персонажей, генерация, рационы еды/воды, UI-карточки. | `characters` (List<GeneratedCharacter>), `genCgaracter` (CharacterGenerator), `cardPrefab`, `cardContainer`, `initialCharacterCount`, `bioPanel` | `OnRationsChanged (Action)` | `CharacterGenerator`, `CharacterCardUI`, `CharacterBioPanelUI`, `DayCycleManager` (подписка `OnNightComplete`) | `GenerateInitialCharacters()` при старте. `CreateCharacter()` → генератор → карточка. `SetFoodRation/SetWaterRation`: проверяют `CanChangeRation` (жив, не в вылазке, в списке), списывают/возвращают ресурс через `ResourceManager`, инкрементят `consecutiveFoodDays/WaterDays`. `ResetDailyRations()` каждый день. `HasLivingCharacters()` — условие победы/поражения. |
| **DayCycleManager** | Главный игровой цикл: переход дня, ночной расчёт выживания, проверка победы/поражения. | `CurrentDay`, `expeditionManager`, `characterManager`, `electricalPanelController`, `VictoryDay = 10` | `OnDayChanged (Action<int>)`, `OnNightComplete (Action<NightReportData>)`, `OnDefeat (Action)`, `OnVictory (Action)` | `ResourceManager`, `CharacterManager`, `ExpeditionManager`, `ElectricalPanelController`, `EventManager` | `StartNextDay()`: 1) Потребление топлива/температура. 2) `UpdateSurvivalCounters`: голод/жажда/мороз → HP/мораль, смерть, исцеление. 3) `ApplyDeathMoralePenalty` (штраф за смерть других). 4) `ProcessMoraleDepartures` (уход при мораль < 0). 5) `ExpeditionManager.ProcessCompletedDay`. 6) `ElectricalPanelController.ProcessGeneratorNight`. 7) `EventManager.GetMorningLinesForDay`. 8) `IsDefeat()`/`IsVictory()` → события. |
| **ExpeditionManager** | Вылазки: запуск, кулдауны локаций, обработка ночи, риски смерти/травмы, награды. | `activeExpeditions` (List<ExpeditionRun>), `locationCooldowns`, `characterManager`, `referencePopulation`, `deathChanceIncreasePerMissingPerson`, `injuryChanceIncreasePerMissingPerson` | `OnExpeditionsChanged (Action)` | `CharacterManager`, `ExpeditionLocationData` (ScriptableObject), `DayCycleManager` (текущий день), `ResourceManager` (награды) | `TryStartExpedition`: проверки лимита (`MaxConcurrentExpeditions = Alive/2`), доступности локации (кулдаун), персонажа. `CompleteExpedition` (каждую ночь): `adjustedDeathChance = base + missingPeople * increase`; `adjustedInjuryChance` аналогично. `RollChance` → смерть/травма/награды (`GiveRewards` через `ResourceManager`). Кулдауны: случайные дни `minCooldownDays`–`maxCooldownDays`. |
| **ElectricalPanelController** | Генератор: поломка ночью, ремонт персонажем, модификаторы черт. | `resourceManager`, `characterManager`, `expeditionManager`, `baseBreakdownChancePercent`, `baseRepairSuccessChancePercent`, `baseRepairInjuryChancePercent`, `repairFailureDamageHP`, `breakdownChanceForOnePerson`, `breakdownReductionPerAdditionalPerson`, `repairSuccessBonusPerAdditionalPerson` | `OnResourcesChanged`, `OnGeneratorStatusChanged` | `ResourceManager`, `CharacterManager`, `ExpeditionManager`, `Trait.EffectType` | `ToggleGenerator()` — вкл/выкл с проверками. `SendCharacterForRepair` — ставит `pendingRepairCharacter`. `ProcessGeneratorNight` (вызывается `DayCycleManager`): 1) Если генератор работал — шанс поломки `CalculateBreakdownChance` (зависит от `aliveInBunker` + черт `ElectricityBreakChanceChangePoints`). 2) Если есть pending — `ResolvePendingRepair`: `successChance = base + (alive-1)*bonus`; при провале — шанс травмы (модифицируется чертами `ElectricityRepairInjuryChanceChangePoints/ReductionPercent`, `NoInjuryOnGeneratorRepair`). |
| **EventManager** | Записки на двери: запланированные/случайные, утренние строчки, применение опций. | `notes` (List<DoorNoteData>), `_activeNote`, `_noteResolvedToday` | (нет публичных событий, опрашивается `InteractDoor`/`InteractNote`) | `DayCycleManager` (`OnDayChanged`), `ResourceManager`, `CharacterManager`, `DoorNoteData` (Resources) | `OnNewDay`: сброс, поиск `Scheduled` (`triggerDay == day`) → иначе `Random` (`randomChance`). `HasActiveNote`/`GetActiveNote` для UI. `ApplyOption`: дельты ресурсов, мораль всем, HP урон случайному, шанс спавна персонажа. |

---

## 3. Данные (Data Models & ScriptableObjects)

| Тип | Поля | Назначение | Где создаётся/изменяется |
|-----|------|------------|---------------------------|
| **ResourceData** (Serializable) | `foodCans`, `waterBottles`, `fuelCans`, `currentDay`, `temperatureLevel`, `generatorBroken`, `generatorIsOn` | Сохраняемое состояние ресурсов. Хранится в `ResourceManager.resourceData`. | `ResourceManager.Awake` (рандомные стартовые), `DayCycleManager` (`currentDay`), `ResourceManager` методы потребления/добавления. |
| **GeneratedCharacter** (Serializable) | `gender`, `fullName`, `age`, `portrait`, `health` (0–100), `morale` (0–100), `daysWithoutFood`, `daysWithoutWater`, `receivesFoodToday`, `receivesWaterToday`, `consecutiveFoodDays`, `consecutiveWaterDays`, `isDead`, `isExiled`, `isSecretRevealed`, `traits` (List<Trait>), `description` | Полное состояние жителя. Мутирует ночью (`DayCycleManager`), при рационах (`CharacterManager`), в вылазках (`ExpeditionManager`), при ремонте (`ElectricalPanelController`). | `CharacterGenerator.Generate` (старт), `CharacterManager.CreateCharacter`, ночные расчёты, `ApplyOption` (`EventManager`). |
| **NightReportData** (Serializable) | `completedDay`, `newDay`, `fuelConsumed`, `generatorWorked`, `temperatureBefore`, `temperatureAfter`, `resourceChanges[]`, `survivalChanges[]`, `deaths[]`, `departures[]`, `expeditionReturns[]`, `secretReports[]`, `specialEvents[]` | Агрегат результатов ночи. Передаётся в `OnNightComplete` → `MorningReportPanel`, `BedSleepTrigger`, `CharacterManager.RemoveInactiveCharacterCards`. | `DayCycleManager.StartNextDay` (собирает всё), `ExpeditionManager.ProcessCompletedDay` (добавляет `expeditionReturns`), `ElectricalPanelController` (добавляет `specialEvents`). |
| **ExpeditionLocationData** (ScriptableObject) | `locationId`, `locationName`, `locationImage`, `description`, `riskDescription`, `durationDays`, `injuryChancePercent`, `deathChancePercent`, `minInjuryDamage`, `maxInjuryDamage`, `minCooldownDays`, `maxCooldownDays`, `possibleRewards` (List<ExpeditionRewardData>) | Определение локации вылазки: длительность, риски, кулдаун, возможные награды. | Создаётся в редакторе (`CreateAssetMenu`). Читается `ExpeditionManager`, `ExpeditionLocationCatalog`, UI (`ExpeditionLocationButtonUI`). |
| **ExpeditionRewardData** (Serializable) | `resourceType` (Food/Water/Fuel), `minAmount`, `maxAmount` | Награда за вылазку: тип ресурса и диапазон количества. | В `ExpeditionLocationData.possibleRewards`. Используется `ExpeditionManager.GiveRewards`. |
| **DoorNoteData** (ScriptableObject) | `spawnType` (Scheduled/Random), `triggerDay`, `randomChance`, `morningReportLine`, `options` (List<Option>), `Option`: `deltaFood`, `deltaWater`, `deltaFuel`, `deltaMorale`, `hpDamage`, `spawnCharacterChance` | Событие/записка на двери. Может давать ресурсы, мораль, урон, спавн жителя. | Редактор. `EventManager.LoadAll` + `OnNewDay` выбирает активную. `ApplyOption` применяет эффекты. |
| **CharaterObject** (ScriptableObject, предполагаемый) | `maleObliks[]`, `femaleObliks[]`, `namePool` (`maleNames`, `femaleNames`, `maleLastNames`, `femaleLastNames`), `minAge`, `maxAge`, `availableTraits[]` | Профиль генерации персонажей: портреты, имена, возраст, черты. | `CharacterGenerator.profile` (инспектор). `Generate()` берёт случайные элементы. |
| **Trait** (Serializable, предполагаемый) | `traitName`, `traitDesc`, `effects` (List<Effect>), `Effect`: `effectType` (EffectType), `value` | Черт персонажа с эффектами: модификаторы шансов поломки/ремонта генератора, урон, иммунитеты. | `CharaterObject.availableTraits`. Эффекты читаются `ElectricalPanelController.HasTraitEffect`/`CalculateBreakdownChance`/`ResolvePendingRepair`. |

---

## 4. Система взаимодействия (Interaction System)

| Компонент | Интерфейсы | Входы (инспектор/ссылки) | Логика `Interact()` | Highlight | Камера (Cinemachine) |
|-----------|------------|--------------------------|---------------------|-----------|----------------------|
| **Interactor** (на камере игрока) | — | `InteractorSource`, `mainCamera`, `InteractRange=5f` | Raycast от мыши. Если не над UI и LeftClick → `hit.collider.GetComponent<IInteractable>()?.Interact()`. Подсветка: `hit.collider.GetComponent<IHighlightable>()` → `Highlight(true/false)`. | Хранит `_currentHighlighted`, переключает при смене цели. | — |
| **InteractPC** (ПК, стол, монитор) | `IInteractable`, `IHighlightable` | `panelToOpen` (GameObject), `playerCamera`, `objCam` (CinemachineCamera), `highlightValue`/`normalValue`, `propertyName="_OutlineSize"` | `objCam.Priority=20` (переключение камеры). Ищет `IOpenablePanel` на `panelToOpen` → `Open()` или просто `SetActive(true)`. `_isInteracting=true`. | Outline material (2-й материал или 1-й) → `SetFloat(propertyName, value)`. Цвет обводки: чёрный/белый. | `objCam` приоритет 20 при взаимодействии, 0 в остальное время. Клавиша `N` → `outCam()` (сброс). |
| **InteractDoor** (Дверь с запиской) | `IInteractable`, `IHighlightable` | `noteObject`, `noteUI` (DoorNoteUI), `outlineMaterial`, `DayCycleManager.OnDayChanged` | Если `EventManager.HasActiveNote()` → `noteUI.Show(note)`. `_isInteracting=true`. | Аналогично через `outlineMaterial._OutlineSize`. | — |
| **InteractNote** (Сама записка) | `IInteractable`, `IHighlightable` | `noteUI` | Если `EventManager.HasActiveNote()` → `noteUI.Show(note)`. | Аналогично. | — |
| **InteractBed** (Кровать/Сон) | `IInteractable`, `IHighlightable` | `morningReportPanel`, `defeatScreenPanel`, `fadePanel` (CanvasGroup), `fadeDuration=1f` | `SleepSequence()`: отключает `PlayerMove`+`Interactor`, FadeIn (`CanvasGroup.alpha` 0→1), `DayCycleManager.StartNextDay()`, ждёт 0.3с → проверка Defeat/Victory → `MorningReportPanel.Show(pendingReport)` → FadeOut → возврат управления. | Outline `_OutlineSize` 1.1f/0f. | — |
| **BedSleepTrigger** (Альтернативный триггер сна на кровати) | — | Те же + `characterManager` | Почти идентичен `InteractBed`, но подписан на `DayCycleManager` события (`OnNightComplete`, `OnDefeat`, `OnVictory`) и хранит `_pendingReport` из события. Есть `ForceReset` (ContextMenu). | — | — |
| **InteractElectricalPanel** (Щиток) | `IInteractable`, `IHighlightable` | `panel` (ElectricalPanelUI) | `panel.Open()` → открывает UI генератора. | Outline `_OutlineSize` 1.1f/0f. | — |

---

## 5. UI-Система (Panels & Windows)

| Панель | Менеджер/данные | Входы (инспектор) | Публичные методы | События/колбэки | Анимации/специфика |
|--------|-----------------|-------------------|------------------|-----------------|-------------------|
| **CharacterCardUI** | `CharacterManager`, `GeneratedCharacter`, `CharacterBioPanelUI` | — (префаб) | `Setup(character, bioPanel)` | — | Drag (`CharacterCardDrag`), позиционирование в контейнере. |
| **CharacterBioPanelUI** | `GeneratedCharacter` | — | `Show(character)`, `ClosePanel()` | — | Детальная карточка жителя. |
| **RationMenuUI** | `CharacterManager`, `ResourceManager` | — | `Refresh()` | `CharacterManager.OnRationsChanged` → `Refresh` | Список жителей с переключателями еда/вода, счётчики ресурсов. |
| **MorningReportPanel** | `NightReportData`, `CommandantCommentSettings` | `titleText`, `bodyText`, `continueButton`, `bedMenuPanel`, `playerCamera`, `playerCameraBasePriority`, `commentSettings` | `Show(report, onContinue)`, `Hide()` | `continueButton.onClick` → `OnContinueClicked` → `Hide` + `bedMenuPanel.SetActive(false)` + `playerCamera.Priority=base` + `onContinue()` | Генерация текста `BuildReportText`: ресурсы, выживание, смерти, уходы, вылазки, особые события, комментарий коменданта (шаблоны). |
| **DefeatScreenPanel** | — | `animationImage`, `animationFrames[]`, `framesPerSecond`, `loopAnimation`, `titleText`, `subtitleText`, `restartButton`, `quitButton` | `Show()`, `ShowVictory()`, `ShowCustomDefeatReason()`, `ShowDefeatWithStory()`, `ShowVictoryWithStory()`, `Hide()`, `RestartGame()`, `QuitGame()` | `restartButton` → `SceneManager.LoadScene`, `quitButton` → `Application.Quit` | Sprite-анимация (Aseprite) в `Update`. Текст истории зависит от survivors/deaths/exiled. |
| **ElectricalPanelUI** | `ElectricalPanelController` | — | `Open()` | `ElectricalPanelController.OnGeneratorStatusChanged` → `Refresh` | Кнопки вкл/выкл, отправка на ремонт, показ статуса и доступных персонажей. |
| **ExpeditionPagesSwitcher** | — | `expeditionsPage`, `activeExpeditionsPage` | `ShowExpeditionsPage()`, `ShowActiveExpeditionsPage()` | — | Переключение между каталогом локаций и активными вылазками. |
| **ActiveExpeditionsUI** | `ExpeditionManager` | `expeditionManager`, `entriesContent`, `entryPrefab` (ActiveExpeditionEntryUI), `emptyText`, `countText` | `Refresh()` | `ExpeditionManager.OnExpeditionsChanged` → `Refresh` | Динамическое создание записей: имя персонажа, локация, дней осталось. |
| **ExpeditionLocationButtonUI** | `ExpeditionLocationData`, `ExpeditionManager` | — | `Setup(location, manager)` | — | Кнопка локации: название, риск, длительность, кулдаун, недоступность. |
| **ExpeditionLocationCatalog** | — | `locations` (List<ExpeditionLocationData>) | — | — | `ScriptableObject`-контейнер для списка локаций в инспекторе. |
| **CommandantCommentSettings** | — | `deathComments[]`, `hungryComments[]`, `thirstyComments[]`, `injuredComments[]`, `hungryAndThirstyComments[]`, `loneComments[]`, `calmComments[]` | — | — | Шаблоны комментариев коменданта для `MorningReportPanel.GetCommandantComment`. |

---

## 6. Игрок и камера

| Компонент | Роль | Ключевые поля | Взаимодействие с остальными |
|-----------|------|---------------|----------------------------|
| **PlayerMove** | Движение персонажа (`CharacterController`) | `speed`, `gravity`, `jumpHeight`, `cinemachineCamera` | Отключается `BedSleepTrigger`/`InteractBed` во время сна (`enabled=false`). |
| **CinemachineCursorLook** | Поворот камеры мышью | `cinemachineCamera`, `sensitivity` | На объекте «КамераИгрока» (`CinemachineCamera` + `CinemachineInputAxisController`). |
| **Interactor** (на КамераИгрока) | Луч взаимодействия | `InteractorSource`, `mainCamera`, `InteractRange` | Луч от центра экрана. Работает только когда не над UI (`EventSystem.IsPointerOverGameObject`). |
| **Quit** (на Main Camera) | Выход из игры | — | `Update`: Escape → `Application.Quit` / `EditorApplication.isPlaying=false`. |

---

## 7. Граф обмена данными и событий (Data Flow Diagram)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        PLAYER INPUT                                           │
│  Mouse/Keyboard → Interactor (Raycast) → IInteractable.Interact()             │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                     INTERACTABLE OBJECTS (Scene)                              │
│  InteractPC ──────→ opens PC UI (ExpeditionPagesSwitcher + panels)           │
│  InteractDoor ────→ opens DoorNoteUI (EventManager.HasActiveNote)            │
│  InteractNote ────→ opens DoorNoteUI                                         │
│  InteractBed ─────→ SleepSequence → DayCycleManager.StartNextDay()           │
│  BedSleepTrigger ─→ (alternative sleep trigger, event-subscribed)            │
│  InteractElectricalPanel → ElectricalPanelUI.Open()                          │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                    ┌─────────────────┼─────────────────┐
                    ▼                 ▼                 ▼
┌──────────────────────────┐ ┌──────────────────┐ ┌──────────────────┐
│   UI PANELS (read/write) │ │  MANAGERS        │ │  DATA MODELS     │
│  - RationMenuUI          │ │  (Singletons)    │ │  (Serializable)  │
│  - MorningReportPanel    │ │  ┌────────────┐  │ │  ┌────────────┐  │
│  - DefeatScreenPanel     │ │  │ResourceMgr │  │ │  │ResourceData│  │
│  - ElectricalPanelUI     │ │  │CharacterMgr│  │ │  │GenCharacter│  │
│  - Expedition UI         │ │  │DayCycleMgr │  │ │  │NightReport │  │
│  - DoorNoteUI            │ │  │ExpeditionM │  │ │  │ExpLocation │  │
│                          │ │  │ElectricalPC│  │ │  │DoorNote    │  │
│                          │ │  │EventManager│  │ │  │Trait/CharObj│ │
└──────────────────────────┘ └──────────────────┘ └──────────────────┘
         │                           │                       │
         │ SetFoodRation              │                       │
         ▼                           ▼                       ▼
   CharacterManager ◄────────── ResourceManager ◄────── ResourceData
      │ OnRationsChanged              │ OnResourcesChanged      │
      │                               │                         │
      ▼                               ▼                         ▼
 DayCycleManager ◄──────────────── ExpeditionManager ◄── ExpeditionLocationData
    │ OnNightComplete                      │ OnExpeditionsChanged      │
    │ OnDefeat/OnVictory                   │                           │
    ▼                                      ▼                           ▼
 BedSleepTrigger/MorningReportPanel  ActiveExpeditionsUI         ExpeditionRewardData
    │                                      │                           │
    ▼                                      ▼                           ▼
 NightReportData (aggregate) ──────────► UI Display ─────────────► Player sees
    │                                       │
    ├─ resourceChanges, survivalChanges ───┤
    ├─ deaths, departures ─────────────────┤
    ├─ expeditionReturns ──────────────────┤
    └─ specialEvents ──────────────────────┘
                                      │
                    ┌───────────────────┼───────────────────┐
                    ▼                   ▼                   ▼
           ElectricalPanelController  EventManager      CharacterGenerator
              (generator night)         (notes/day)         (new chars)
                    │                   │                   │
                    ▼                   ▼                   ▼
           NightReportData.specialEvents  MorningReportPanel  CharacterManager.characters
                    │                   │                   │
                    └───────────────────┴───────────────────┘
                                      │
                                      ▼
                          ┌───────────────────────┐
                          │   WIN/LOSS CHECK      │
                          │ DayCycleManager       │
                          │ IsDefeat() / IsVictory│
                          │ OnDefeat / OnVictory  │
                          └───────────────────────┘
                                      │
                                      ▼
                            DefeatScreenPanel.ShowDefeatWithStory
                            DefeatScreenPanel.ShowVictoryWithStory
```

---

## 8. Ключевые ветвления логики (Decision Points)

| Точка ветвления | Условия (if/else) | Результаты ветвей |
|-----------------|-------------------|-------------------|
| `ResourceManager.ConsumeFood/Water/Fuel` | `resource >= amount` | `true`: `resource -= amount`, `OnResourcesChanged`, return `true` / `false`: return `false` |
| `CharacterManager.SetFoodRation` | `CanChangeRation` (жив, не в вылазке, в списке) && (`receivesFood` ? `ResourceManager.ConsumeFood(1)` : `true`) | `true`: `consecutiveFoodDays++`, `receivesFoodToday=value`, `OnRationsChanged` / `false`: no-op |
| `DayCycleManager.StartNextDay` → Температура | `generatorWasOn` && `!GeneratorBroken` && `ConsumeFuel(1)` | `fuelConsumed=true`: `AddTemperature(1)` / `false`: `SetTemperature(temp-1)` |
| `DayCycleManager.UpdateSurvivalCounters` → Голод | `daysWithoutFood`: 1→мораль-5, 2→мораль-10, 3→HP-10/мораль-10, 4+→HP-15/мораль-15 → смерть при HP<=0 | Накопление `survivalChanges`, `deaths` |
| `DayCycleManager.UpdateSurvivalCounters` → Жажда | `daysWithoutWater`: 1→мораль-5, 2→HP-5/мораль-10, 3→HP-15/мораль-15, 4+→HP-20/мораль-15 → смерть | Аналогично |
| `DayCycleManager.UpdateSurvivalCounters` → Мороз | `TemperatureLevel <= 1`: 0→HP-30/мораль-10, 1→HP-15/мораль-10 → смерть | `survivalChanges` |
| `DayCycleManager.ApplyDeathMoralePenalty` | `report.deaths.Count > 0` && `aliveCount < 6` | `penalty = baseMoralePenaltyPerDeath * deaths.Count * (6-aliveCount)` → `morale -= penalty` |
| `DayCycleManager.ProcessMoraleDepartures` | `morale < 0` && не в вылазке | `isExiled=true`, `departures.Add`, `survivalChanges` |
| `ExpeditionManager.TryStartExpedition` | `ActiveExpeditionCount < MaxConcurrent` && `IsLocationAvailable` && character valid | `true`: добавлен в `activeExpeditions`, кулдаун локации, рационы сброшены, `OnExpeditionsChanged` / `false`: message |
| `ExpeditionManager.CompleteExpedition` (каждую ночь) | `RollChance(adjustedDeathChance)` → смерть / `RollChance(adjustedInjuryChance)` → травма + урон / иначе награды `GiveRewards` | `death`: `isDead=true`, `report.deaths` / `injury`: `health-=damage`, possible death / `rewards`: `ResourceManager.AddFood/Water/Fuel` |
| `ElectricalPanelController.CalculateBreakdownChance` | `aliveInBunker <= 1` → `breakdownChanceForOnePerson` / иначе `max(0, base - (alive-1)*reduction)` + traits.`ElectricityBreakChanceChangePoints` | `Clamp 0–100`, `RollChance` → `BreakGenerator()` |
| `ElectricalPanelController.ResolvePendingRepair` | `RollChance(successChance)` → `RepairGenerator()` / иначе: `injuryChance` (modified by traits) → `RollChance` → damage / no injury | `success`: `specialEvents` «починил» / `fail+injury`: HP damage, possible death / `fail`: `specialEvents` «не смог» |
| `EventManager.OnNewDay` | `Scheduled` note (`triggerDay==day`) → `_activeNote` / else `Random` note (`randomChance`) → `_activeNote` / else `null` | `HasActiveNote()` для UI |
| `EventManager.ApplyOption` | `Option.deltas` applied to `ResourceManager`, morale to all alive, `hpDamage` to random alive, `spawnCharacterChance` → `CreateCharacter` | Immediate resource/morale changes |
| `BedSleepTrigger/InteractBed.SleepSequence` | `DayCycleManager.IsDefeat()` → `DefeatScreen` / `IsVictory()` → `VictoryScreen` / else `MorningReportPanel` | `Defeat`: `ShowDefeatWithStory` / `Victory`: `ShowVictoryWithStory` / `Normal`: `Show(report)` |

---

## 9. Инициализация и порядок запуска (Bootstrap)

| Этап | Что происходит | Скрипты/объекты |
|------|----------------|-----------------|
| **Awake (Singletons)** | `ResourceManager`, `CharacterManager`, `DayCycleManager`, `ExpeditionManager`, `ElectricalPanelController`, `EventManager` устанавливают `Instance`, находят зависимости через `GetComponent`/`FindObjectOfType`. | Все Manager-скрипты на объекте `Manager` в сцене. |
| **Start** | `CharacterManager.GenerateInitialCharacters` → `CharacterGenerator.Generate` (5 шт). `EventManager` загружает `DoorNoteData` из `Resources`, `OnNewDay(CurrentDay)`. `BedSleepTrigger`/`InteractBed` подписываются на `DayCycleManager` события. `ActiveExpeditionsUI` подписывается на `ExpeditionManager`. `RationMenuUI` подписывается на `CharacterManager.OnRationsChanged`. | `CharacterManager`, `EventManager`, `BedSleepTrigger`, `ActiveExpeditionsUI`, `RationMenuUI` |
| **ResourceManager.Awake** | Рандомные стартовые ресурсы: еда 8–10, вода 8–10, топливо 4–6, температура 3, генератор цел/выкл. | `ResourceManager` |
| **DayCycleManager.Awake** | Синхронизация `CurrentDay` с `ResourceData.currentDay` (если >0). | `DayCycleManager` |

---

## 10. Сохранение/Загрузка (текущее состояние)

В коде **явной системы сохранения** (PlayerPrefs/JSON/файлы) **нет**. Состояние живёт в памяти в синглтонах и их data-объектах. При перезапуске сцены — полный ресет.

Для персистентности нужно добавить `SaveSystem` (сериализация):
- `ResourceData`
- `List<GeneratedCharacter>`
- `activeExpeditions`, `locationCooldowns`
- `CurrentDay`
- `pendingRepairCharacter`, `pendingRepairDaySent`
- `_activeNote`, `_noteResolvedToday`

---

## 11. Отсутствующие/нереализованные модули (Gaps)

- Save/Load система.
- Settings/Options меню (графика, звук, язык).
- Локализация (все строки hardcoded на русском).
- Unit/Integration тесты.
- Аудио-менеджер (SFX, музыка).
- Пуллинг объектов (`Instantiate`/`Destroy` карточек/записей каждый раз).
- Валидация данных (null-checks есть, но нет схемы валидации ScriptableObject).
- Конфиг сложности (баланс: стартовые ресурсы, шансы, `VictoryDay`).

---

## 12. Рекомендации по рефакторингу/архитектуре

1. Вынести интерфейсы (`IInteractable`, `IHighlightable`, `IOpenablePanel`) в отдельный Assembly Definition (`Core`).
2. Заменить прямые `FindObjectOfType` на dependency injection (Zenject/VContainer) или хотя бы `Register`/`Unregister` в статическом `ServiceLocator`.
3. Объединить `InteractBed` и `BedSleepTrigger` — дублирование логики сна.
4. Сделать `NightReportData` immutable (`record`/`readonly`) и собирать через `Builder` в `DayCycleManager`.
5. Вынести баланс (числа) в `ScriptableObject GameBalanceConfig`.
6. Добавить `ISerializationCallbackReceiver` для корректной сериализации списков черт/эффектов.
7. Рассмотреть ECS/DOTS для систем выживания (много похожих циклов по `characters`).