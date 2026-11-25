# Блок-схема алгоритма игры «Doomed Game»

> Источник: анализ всех `.cs`-скриптов в `Assets/Scripts/` и сцены `Assets/Scenes/GameScene 1.unity`.
> Диаграмма отражает реальный порядок выполнения кода: инициализация → дневной цикл → сон → ночные расчёты → утренний отчёт → повтор.
>
> Открывайте в любом редакторе с поддержкой Mermaid (VS Code, Obsidian, GitHub, Notion).

```mermaid
flowchart TB
    %% ==================== СТАРТ ====================
    Start([Сцена GameScene 1 загружена]) --> Init["Инициализация менеджеров на GameObject «Manager»<br/>DayCycleManager, ResourceManager, CharacterManager, CharacterGenerator, ExpeditionManager"]
    
    Init --> GenChars["CharacterManager.Start()<br/>CharacterGenerator.Generate() × initialCharacterCount (5)<br/>Создание списка GeneratedCharacter"]
    
    GenChars --> InitRes["ResourceManager.Awake()<br/>Случайный старт:<br/>• Еда: 8–10 банок<br/>• Вода: 8–10 бутылок<br/>• Топливо: 4–6 канистр<br/>• Температура: 3/3<br/>• Генератор: не сломан, выключен"]

    %% ==================== ДНЕВНОЙ ЦИКЛ ====================
    InitRes --> DayLoop["🌞 ДНЕВНОЙ ЦИКЛ<br/>Игрок перемещается по бункеру<br/>PlayerMove (отключён во время сна)<br/>Камера: CinemachineCursorLook"]

    DayLoop --> Interact["Игрок наводит курсор на объект мира<br/>Interactor: raycast из камеры<br/>IHighlightable.Highlight(true/false) — обводка объекта"]

    Interact --> Click{"Игрок нажал ЛКМ (Input.GetMouseButtonDown)?"}
    Click -->|Нет| DayLoop

    Click -->|Да| InteractObj{"Какой объект мира?"}

    %% --- Ветка: Дверь / записка ---
    InteractObj -->|Дверь / записка| DoorInt["InteractDoor.Interact()<br/>Проверка: EventManager.Instance.HasActiveNote()<br/>Если да → DoorNoteUI.Show(DoorNoteData)<br/>Игрок выбирает опцию → EventManager.ApplyOption(option)<br/>• deltaFood/Water/Fuel → ResourceManager<br/>• deltaMorale → всем персонажам<br/>• hpDamage → случайному персонажу<br/>• spawnCharacterChance → CharacterManager.CreateCharacter()<br/>EventManager.ResolveNote()"]

    %% --- Ветка: Кровать / сон ---
    InteractObj -->|Кровать| BedInt["BedSleepTrigger.OnSleepButtonClicked()<br/>или InteractBed.Interact()<br/>Проверка: не идёт ли уже сон (_isSleeping)<br/>→ SleepSequence()"]

    BedInt --> SleepSeq["BedSleepTrigger.SleepSequence() / InteractBed.SleepSequence()<br/>1. Отключить PlayerMove.enabled = false<br/>2. Отключить Interactor.enabled = false<br/>3. Затемнение экрана: fadePanel.alpha 0 → 1<br/>4. DayCycleManager.StartNextDay()<br/>5. Ожидание 0.3 с<br/>6. Проверка поражения/победы<br/>7. MorningReportPanel.Show(report)<br/>8. Осветление экрана<br/>9. Включить PlayerMove, Interactor"]

    %% --- Ветка: Рационная доска ---
    InteractObj -->|Рационная доска| RationInt["InteractRationBoard.Interact()<br/>→ RationMenuUI.Open()"]

    RationInt --> RationAction{"Назначить еду/воду персонажу?"}
    RationAction -->|Да| RationCheck{"Ресурсы в ResourceManager достаточны?"}
    RationCheck -->|Да| RationApply["CharacterManager.SetFoodRation / SetWaterRation<br/>→ ResourceManager.ConsumeFood(1) / ConsumeWater(1)<br/>character.consecutiveFoodDays++ / consecutiveWaterDays++<br/>OnRationsChanged → RationMenuUI.RefreshResourceCounts() + карточки"]
    RationCheck -->|Нет| RationReject["UI-сообщение:<br/>«Недостаточно еды» / «Недостаточно воды»<br/>Toggle возвращается в предыдущее состояние"]
    RationAction -->|Нет| RationClose["RationMenuUI.ClosePanel()"]

    %% --- Ветка: Электропанель / генератор ---
    InteractObj -->|Электропанель| ElecInt["InteractElectricalPanel.Interact()<br/>→ ElectricalPanelUI.Open()<br/>В UI:<br/>• Показания температуры, топлива, статуса генератора<br/>• Кнопка включения/выключения генератора → ElectricalPanelController.ToggleGenerator()<br/>• Ремонт: выбор RepairCharacterCardUI → SendCharacterForRepair()<br/>  → ElectricalPanelController.pendingRepairCharacter (ремонт ночью)"]

    %% --- Ветка: ПК ---
    InteractObj -->|ПК| PCInt["InteractPC.Interact()<br/>• CinemachineCamera objCam.Priority = 20 (камера объекта)<br/>• IOpenablePanel.Open() — открыть привязанную панель<br/>• Клавиша N — выйти из режима объекта"]

    %% --- Ветка: Вылазки ---
    DayLoop --> ExpeditionAction["ExpeditionsTabUI<br/>1. Игрок выбирает локацию из ExpeditionLocationCatalog<br/>2. Игрок выбирает персонажа (ExpeditionCharacterCardUI)<br/>3. Нажатие «Отправить экспедицию»<br/>→ ExpeditionManager.TryStartExpedition(location, character)"]

    ExpeditionAction --> ExpCheck{"Проверки:<br/>• Активных вылазок < MaxConcurrentExpeditions<br/>• Локация не на cooldown и не в поездке<br/>• Персонаж жив, не изгнан, не в другой вылазке"}
    ExpCheck -->|Нет| ExpWarning["Предупреждение в UI:<br/>максимум вылазок / локация недоступна / персонаж недоступен"]
    ExpCheck -->|Да| ExpStart["ExpeditionRun добавляется в activeExpeditions<br/>• Персонажу отменяются пайки на сегодня (SetFoodRation(false), SetWaterRation(false))<br/>• Локации ставится cooldown на minCooldownDays–maxCooldownDays<br/>• OnExpeditionsChanged → ActiveExpeditionsUI обновляется"]

    %% ==================== ПЕРЕХОД К НОЧИ (StartNextDay) ====================
    SleepSeq --> StartNextDay["DayCycleManager.StartNextDay() — алгоритм ночи"]

    StartNextDay --> NightStep1["Шаг 1. Топливо и температура<br/>if (generatorIsOn && !generatorBroken):<br/>   ResourceManager.ConsumeFuel(1)<br/>   ResourceManager.AddTemperature(1)<br/>else:<br/>   ResourceManager.SetTemperature(temperature - 1)"]

    NightStep1 --> NightStep2["Шаг 2. Выживание персонажей (UpdateSurvivalCounters)<br/>Для каждого живого, не изгоревшего, не в поездке персонажа:<br/>• receivesFoodToday → daysWithoutFood = 0, foodSpent++<br/>  иначе daysWithoutFood++<br/>• receivesWaterToday → daysWithoutWater = 0, waterSpent++<br/>  иначе daysWithoutWater++<br/>Ветвления по дням без еды:<br/>  1 день → предупреждение<br/>  2 дня → мораль −5<br/>  3 дня → здоровье −10, мораль −10<br/>  ≥4 дня → здоровье −15, мораль −15, если HP ≤ 0 → смерть<br/>Ветвления по дням без воды:<br/>  1 день → мораль −5<br/>  2 дня → здоровье −5, мораль −10<br/>  3 дня → здоровье −15, мораль −15<br/>  ≥4 дня → здоровье −20, мораль −15, если HP ≤ 0 → muerte<br/>Холод (temp ≤ 1): здоровье −15/−30, мораль −10<br/>Восстановление: если еда и вода ≥2 дней подряд → HP +5, мораль +5"]

    NightStep2 --> NightStep3["Шаг 3. Сброс пайков<br/>CharacterManager.ResetDailyRations()<br/>Все receivesFoodToday = false, receivesWaterToday = false<br/>OnRationsChanged → UI"]

    NightStep3 --> NightStep4["Шаг 4. Экспедиции (ExpeditionManager.ProcessCompletedDay)<br/>Для каждой активной вылазки:<br/>• daysRemaining--<br/>• Если 0 → CompleteExpedition:<br/>  – Проверка смерти: шанс = deathChance + fehlende люди × 5%<br/>  – Если выжил: проверка травмы (injuryChance), урон HP<br/>  – Награды (еда/вода/топливо) → ResourceManager.Add*()<br/>• Удаление завершённых вылазок<br/>• OnExpeditionsChanged → UI"]

    NightStep4 --> NightStep5["Шаг 5. Генератор (ElectricalPanelController.ProcessGeneratorNight)<br/>Если генератор работал всю ночь:<br/>• Вероятность поломки = базовая шанс (зависит от числа людей в бункере)<br/>• Если сломался → ResourceManager.BreakGenerator()<br/>Если pendingRepairCharacter (персонаж отправлен на ремонт днём):<br/>• ResolvePendingRepair:<br/>  – Шанс успеха = базовый + (людей − 1) × бонус<br/>  – Успех → ResourceManager.RepairGenerator()<br/>  – Неудача → проверка травмы от ремонта, урон HP, возможная смерть"]

    NightStep5 --> NightStep6["Шаг 6. Мораль и уходы<br/>ApplyDeathMoralePenalty:<br/>• Штраф морали всем живым за каждую смерть<br/>• Множитель зависит от числа выживших<br/>ProcessMoraleDepartures:<br/>• Если мораль < 0 → isExiled = true, персонаж уходит из бункера"]

    NightStep6 --> NightStep7["Шаг 7. События дня<br/>OnDayChanged → EventManager.OnNewDay(day)<br/>• Запланированная записка (SpawnType.Scheduled, triggerDay == day)<br/>• Или случайная записка (SpawnType.Random, randomChance)<br/>EventManager.GetMorningLinesForDay() → строки для утреннего отчёта"]

    NightStep7 --> NightStep8["Шаг 8. Проверка конца игры<br/>IsDefeat(): все персонажи мертвы или ушли → OnDefeat()<br/>IsVictory(): день ≥ 10 и есть выжившие → OnVictory()"]

    NightStep8 --> CheckEnd{"Результат проверки?"}
    CheckEnd -->|Поражение| Defeat["OnDefeat → DefeatScreenPanel.ShowDefeatWithStory()<br/>• Дни survived, выжило, погибло, ушло<br/>• Причина поражения"]
    CheckEnd -->|Победа| Victory["OnVictory → DefeatScreenPanel.ShowVictoryWithStory()<br/>• Дни survived, статистика<br/>• Гуманитарный конвой"]
    CheckEnd -->|Продолжить| NightReport["Сбор NightReportData<br/>• resourceChanges: еда, вода, топливо, температура<br/>• survivalChanges: состояние каждого жителя<br/>• deaths: имена погибших<br/>• departures: имена ушедших<br/>• expeditionReturns: результаты вылазок<br/>• specialEvents: поломка генератора, ремонт, события записки"]

    %% ==================== УТРЕННИЙ ОТЧЁТ ====================
    NightReport --> Morning["MorningReportPanel.Show(report, OnSummaryContinued)<br/>Построение текста:<br/>• День, топливо, температура, запасы<br/>• Состояние жителей (голод, жажда, здоровье)<br/>• Смерти и уходы<br/>• Возвращения экспедиций<br/>• Особые события<br/>• Итоговая строка (GetSummaryLine)<br/>• Комментарий коменданта (CommandantCommentSettings):<br/>  – смерти → deathComments<br/>  – уходы → специальный текст<br/>  – голод ≥2 → hungryComments<br/>  – жажда ≥2 → thirstyComments<br/>  – раненые → injuredComments<br/>  – иначе → calmComments"]

    Morning --> FadeOut["BedSleepTrigger.FadeInRoutine()<br/>1. fadePanel.alpha 1 → 0<br/>2. MorningReportPanel.Hide()<br/>3. PlayerMove.enabled = true<br/>4. Interactor.enabled = true<br/>5. _isSleeping = false"]

    FadeOut --> DayLoop

    %% ==================== КОНЕЦ ИГРЫ ====================
    Defeat --> EndGame{"Рестарт или выход?"}
    Victory --> EndGame
    EndGame -->|Рестарт| Restart["DefeatScreenPanel.RestartGame()<br/>SceneManager.LoadScene(activeBuildIndex)"]
    EndGame -->|Выход| Quit["Application.Quit()<br/>в редакторе: EditorApplication.isPlaying = false"]
    Restart --> Start

    %% ==================== СТИЛИ ====================
    classDef process fill:#e1f5fe,stroke:#01579b,stroke-width:2px;
    classDef decision fill:#fff3e0,stroke:#e65100,stroke-width:2px;
    classDef ui fill:#f3e5f5,stroke:#4a148c,stroke-width:2px;
    classDef end fill:#ffebee,stroke:#b71c1c,stroke-width:2px;
    classDef data fill:#e8f5e9,stroke:#1b5e20,stroke-width:2px;

    class Start,Init,GenChars,InitRes,DayLoop,SleepSeq,StartNextDay,NightStep1,NightStep2,NightStep3,NightStep4,NightStep5,NightStep6,NightStep7,NightStep8,NightReport,Morning,FadeOut,Restart process;
    class Click,InteractObj,RationAction,RationCheck,ExpCheck,CheckEnd,EndGame decision;
    class DoorInt,BedInt,RationInt,ElecInt,PCInt,RationMenuUI,ExpeditionAction,RepairAction,RationApply,RationReject,RationClose,ExpWarning,ExpStart,Defeat,Victory,DoorNoteUI,ElectricalPanelUI ui;
    class Quit end;
```

## Назначение модулей, входы, результаты, обмен данными

| Модуль | Назначение | Входы | Результаты / выходы | Обмен данными |
|--------|-----------|-------|---------------------|---------------|
| **Interactor** | Обработка ввода игрока в мире (raycast) | Камера, позиция мыши, объекты с `IInteractable` | Вызов `Interact()`, подсветка `Highlight()` | Прямой вызов методов на объектах мира |
| **InteractDoor / DoorNoteUI** | Показ записи у двери и обработка опций | `EventManager.HasActiveNote()`, `DoorNoteData` | `EventManager.ApplyOption(option)` | События через `EventManager.Instance` |
| **InteractBed / BedSleepTrigger** | Запуск сна и перехода к следующему дню | `DayCycleManager.Instance`, `MorningReportPanel`, `DefeatScreenPanel` | `DayCycleManager.StartNextDay()` | События `OnNightComplete`, `OnDefeat`, `OnVictory` |
| **RationMenuUI** | Распределение пайков еды и воды | `CharacterManager`, `ResourceManager` | `SetFoodRation`, `SetWaterRation` | Событие `OnRationsChanged` |
| **ExpeditionsTabUI** | Отправка персонажей на вылазки | `ExpeditionManager`, `CharacterManager`, `ExpeditionLocationCatalog` | `TryStartExpedition()` | Событие `OnExpeditionsChanged` |
| **ExpeditionManager** | Управление вылазками, расчёт исходов | `DayCycleManager.Instance.CurrentDay`, `CharacterManager`, `ResourceManager` | `ProcessCompletedDay()`, `CompleteExpedition()` | Событие `OnExpeditionsChanged` |
| **ElectricalPanelController** | Генератор: включение, поломка, ремонт | `ResourceManager`, `CharacterManager`, `ExpeditionManager`, `DayCycleManager` | `ProcessGeneratorNight()`, `SendCharacterForRepair()` | События `OnResourcesChanged`, `OnGeneratorStatusChanged` |
| **EventManager** | Записки у двери, случайные события | `DayCycleManager.OnDayChanged`, `DoorNoteData` (Resources.LoadAll) | `ApplyOption(option)` меняет ресурсы, мораль, здоровье | Прямой вызов `ResourceManager`, `CharacterManager` |
| **DayCycleManager** | Оркестратор ночи: день → ночь → утро | Все менеджеры через singletons | `NightReportData`, `OnNightComplete`, `OnDefeat`, `OnVictory` | События, подписчики: `CharacterManager`, `BedSleepTrigger`, `InteractBed`, `EventManager` |
| **ResourceManager** | Центральное хранилище ресурсов | `ResourceData` (еда, вода, топливо, температура, генератор) | Методы `Consume*`, `Add*`, `SetTemperature`, `BreakGenerator`, `RepairGenerator` | Событие `OnResourcesChanged` |
| **CharacterManager** | Управление жителями | `CharacterGenerator`, `ResourceManager` | `CreateCharacter()`, `SetFoodRation`, `SetWaterRation`, `ResetDailyRations` | Событие `OnRationsChanged` |
| **MorningReportPanel** | Утренняя сводка игроку | `NightReportData`, `CommandantCommentSettings` | Текст отчёта, кнопка продолжения | Вызывается из `BedSleepTrigger` после сна |
| **DefeatScreenPanel** | Экран поражения/победы | Результат `IsDefeat()` / `IsVictory()` | Перезапуск сцены или выход | `SceneManager.LoadScene`, `Application.Quit` |

## Ключевые ветвления алгоритма

1. **Сон** — точка входа в ночные расчёты. Без сна день не сменяется.
2. **Проверка ресурсов при выдаче пайка** — если еды/воды нет, toggle сбрасывается, выводится сообщение.
3. **Доступность локации для вылазки** — проверяется cooldown, занятость локации, лимита одновременных вылазок.
4. **Исход вылазки** — ветвление: смерть (по шансу), травма (по шансу), успех с наградами.
5. **Ремонт генератора** — ветвление: успех (генератор чинится), неудача без травмы, неудача с травмой и возможной смертью.
6. **Конец игры** — `IsDefeat()` (все мертвы/ушли) или `IsVictory()` (день ≥ 10).
7. **Комментарий коменданта** — ветвление по наличию смертей, уходов, голода, жажды, ранений.

## Файл для ТЗ

`D:\Documents\GITHUB2.0\doomed game\Assets\GameAlgorithm_Flowchart.md`
