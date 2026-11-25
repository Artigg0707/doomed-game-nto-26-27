# Архитектура игры «Doomed Game» — полная диаграмма Mermaid

> Источник: анализ всех `.cs`-скриптов в `Assets/Scripts/`, сцены `Assets/Scenes/GameScene 1.unity`, ассетов `Assets/ScriptableObjects/` и документации `Assets/GameDataSchema.md`, `Assets/GameAlgorithm_Flowchart.md`.
> Диаграмма отражает реальные зависимости между классами, ссылки в инспекторе сцены и события между менеджерами.

```mermaid
flowchart TB
    %% ==================== ВНЕШНИЙ АКТЕР ====================
    PLAYER((Игрок<br/>мышь / клавиатура))

    %% ==================== СЦЕНА: GameScene 1 ====================
    subgraph Scene["Сцена GameScene 1"]

        %% --- Менеджеры (シングルトны на GameObject «Manager») ---
        subgraph ManagerGO["GameObject «Manager» (синглтоны)"]
            DCM[DayCycleManager]
            RM[ResourceManager]
            CM[CharacterManager]
            CG[CharacterGenerator]
            EM[ExpeditionManager]
            EPC[ElectricalPanelController]
            EV[EventManager]
        end

        %% --- Система ввода ---
        subgraph InputSys["Ввод и взаимодействие"]
            INT[Interactor]
            PM[PlayerMove]
            CL[CinemachineCursorLook]
            QU[Quit]
        end

        %% --- Объекты мира ---
        subgraph WorldObjs["Объекты мира (IInteractable / IHighlightable)"]
            Door[Door / InteractDoor]
            Bed[Bed / InteractBed + BedSleepTrigger]
            RB[Ration Board / InteractRationBoard]
            Elec[Электропанель / InteractElectricalPanel]
            PC[PC / InteractPC]
            Note[Note / InteractNote]
            BST[BedSleepTrigger]
        end

        %% --- UI Canvas ---
        subgraph UICanvas["Canvas — пользовательский интерфейс"]

            subgraph MainUI["Основные панели"]
                MRP[MorningReportPanel]
                EXP[ExpeditionsTabUI]
                AEU[ActiveExpeditionsUI]
                EPUI[ElectricalPanelUI]
                RAT[RationMenuUI]
                DSP[DefeatScreenPanel]
                CBP[CharacterBioPanelUI]
                DNU[DoorNoteUI]
            end

            subgraph UIElements["Элементы UI (карточки, кнопки, списки)"]
                CCU[CharacterCardUI + CharacterCardDrag]
                EXCCU[ExpeditionCharacterCardUI]
                RCCU[RepairCharacterCardUI]
                RRCU[RationCharacterCardUI]
                AEUEntry[ActiveExpeditionEntryUI]
                EPS[ExpeditionPagesSwitcher]
                ELB[ExpeditionLocationButtonUI]
            end
        end
    end

    %% ==================== ДАННЫЕ (ScriptableObjects + модели) ====================
    subgraph Data["Данные — ScriptableObjects и модели"]
        RD[ResourceData]
        NRD[NightReportData]
        DND[DoorNoteData SO]
        ELD[ExpeditionLocationData SO]
        ELC[ExpeditionLocationCatalog SO]
        CO[CharaterObject SO]
        NP[NamePool SO]
        TRT[Trait SO]
        ST[Stati SO — не используется]
        HSD[HiddenStatDefinition SO — не используется]
        DIS[disease SO — не используется]
        CCS[CommandantCommentSettings SO]
    end

    %% ==================== СВЯЗИ: Игрок ↔ Ввод ====================
    PLAYER -->|клик, нажатие клавиш| INT
    PLAYER -->|движение мыши| CL
    PLAYER -->|Esc| QU

    CL -->|поворот камеры| PLAYER
    QU -->|Application.Quit()| PLAYER

    %% ==================== СВЯЗИ: Ввод → Объекты мира ====================
    INT -->|IInteractable.Interact()| Door
    INT -->|IInteractable.Interact()| Bed
    INT -->|IInteractable.Interact()| RB
    INT -->|IInteractable.Interact()| Elec
    INT -->|IInteractable.Interact()| PC
    INT -->|IInteractable.Interact()| Note

    %% ==================== СВЯЗИ: Объекты мира → Менеджеры / UI ====================
    Door -->|EventManager.HasActiveNote()| EV
    Door -->|DoorNoteUI.Show()| DNU

    Bed -->|BedSleepTrigger.OnSleepButtonClicked()| BST
    BST -->|DayCycleManager.StartNextDay()| DCM
    Bed -->|OnNightComplete / OnDefeat / OnVictory| DCM
    Bed -->|MorningReportPanel.Show()| MRP
    Bed -->|DefeatScreenPanel.Show()| DSP

    RB -->|RationMenuUI.Open()| RAT
    Elec -->|ElectricalPanelUI.Open()| EPUI
    PC -->|IOpenablePanel.Open()| UICanvas

    %% ==================== СВЯЗИ: Менеджеры ↔ Данные ====================
    DCM -->|ResourceManager.Instance| RM
    DCM -->|CharacterManager.Instance| CM
    DCM -->|ExpeditionManager.ProcessCompletedDay(report)| EM
    DCM -->|ElectricalPanelController.ProcessGeneratorNight(report)| EPC
    DCM -->|OnDayChanged → EventManager.OnNewDay| EV
    DCM -->|создаёт NightReportData| NRD

    CM -->|ResourceManager.Instance.Consume/AddFood/Water| RM
    CM -->|CharacterGenerator.Generate()| CG
    CM -->|OnRationsChanged → UI| RAT
    CM -->|создаёт GeneratedCharacter| CG

    CG -->|CharaterObject профиль| CO
    CG -->|NamePool имена| NP
    CG -->|Trait черты| TRT

    EM -->|CharacterManager.characters| CM
    EM -->|ResourceManager.AddFood/Water/Fuel (награды)| RM
    EM -->|DayCycleManager.Instance.CurrentDay| DCM
    EM -->|ExpeditionLocationData (SO)| ELD
    EM -->|OnExpeditionsChanged → UI| EXP
    EM -->|OnExpeditionsChanged → UI| AEU

    EPC -->|ResourceManager| RM
    EPC -->|CharacterManager| CM
    EPC -->|ExpeditionManager.IsCharacterOnExpedition| EM
    EPC -->|DayCycleManager.Instance.CurrentDay| DCM
    EPC -->|ProcessGeneratorNight(report)| DCM
    EPC -->|Trait.EffectType.*| TRT
    EPC -->|OnGeneratorStatusChanged → UI| EPUI

    EV -->|DayCycleManager.OnDayChanged| DCM
    EV -->|DoorNoteData (SO) Resources.LoadAll| DND
    EV -->|ApplyOption → ResourceManager| RM
    EV -->|ApplyOption → CharacterManager (мораль, здоровье, новый житель)| CM
    EV -->|GetMorningLinesForDay → DayCycleManager| DCM

    %% ==================== СВЯЗИ: UI → Менеджеры ====================
    RAT -->|CharacterManager| CM
    RAT -->|ResourceManager.Instance| RM
    RAT -->|ExpeditionManager| EM
    RAT -->|RationCharacterCardUI.Setup()| RRCU

    EXP -->|ExpeditionLocationCatalog| ELC
    EXP -->|ExpeditionManager.TryStartExpedition()| EM
    EXP -->|CharacterManager| CM
    EXP -->|ExpeditionLocationButtonUI| ELB
    EXP -->|ExpeditionCharacterCardUI| EXCCU

    AEU -->|ExpeditionManager.OnExpeditionsChanged| EM
    AEU -->|ActiveExpeditionEntryUI| AEUEntry

    EPUI -->|ElectricalPanelController| EPC
    EPUI -->|CharacterManager| CM
    EPUI -->|ExpeditionManager| EM
    EPUI -->|RepairCharacterCardUI| RCCU

    MRP -->|NightReportData| NRD
    MRP -->|CommandantCommentSettings| CCS
    MRP -->|CinemachineCamera приоритет| CL

    DSP -->|SceneManager.LoadScene (перезапуск)| Scene
    DSP -->|Application.Quit / EditorApplication.isPlaying = false| QU

    CBP -->|GeneratedCharacter| CM

    DNU -->|DoorNoteData| DND
    DNU -->|EventManager.ApplyOption| EV

    %% ==================== СВЯЗИ: Данные между собой ====================
    NRD -.->|содержит изменения| RD
    DND -.->|опции меняют| RD
    ELD -.->|входит в| ELC
    CO -.->|содержит| NP
    CO -.->|содержит| TRT
    CO -.->|содержит (закомментировано)| ST

    %% ==================== СВЯЗИ: UI-элементы ↔ Панели ====================
    RAT -->|RationCharacterCardUI| RRCU
    EXP -->|ExpeditionCharacterCardUI| EXCCU
    EXP -->|ExpeditionLocationButtonUI| ELB
    AEU -->|ActiveExpeditionEntryUI| AEUEntry
    EPUI -->|RepairCharacterCardUI| RCCU

    %% ==================== Стили классов ====================
    classDef singleton fill:#e1f5fe,stroke:#01579b,stroke-width:2px;
    classDef ui fill:#f3e5f5,stroke:#4a148c,stroke-width:2px;
    classDef data fill:#fff3e0,stroke:#e65100,stroke-width:2px;
    classDef world fill:#e8f5e9,stroke:#1b5e20,stroke-width:2px;
    classDef input fill:#ffebee,stroke:#b71c1c,stroke-width:2px;
    classDef unused fill:#f5f5f5,stroke:#9e9e9e,stroke-width:1px,stroke-dasharray: 5 5;

    class DCM,RM,CM,CG,EM,EPC,EV singleton;
    class MRP,EXP,AEU,EPUI,RAT,DSP,CBP,DNU,CCU,EXCCU,RCCU,RRCU,AEUEntry,EPS,ELB ui;
    class RD,NRD,DND,ELD,ELC,CO,NP,TRT,CCS data;
    class ST,HSD,DIS unused;
    class Door,Bed,RB,Elec,PC,Note,BST world;
    class INT,PM,CL,QU input;

    click DCM href "#" "DayCycleManager.cs"
    click RM href "#" "ResourceManager.cs"
    click CM href "#" "CharacterManager.cs"
    click CG href "#" "CharacterGenerator.cs"
    click EM href "#" "ExpeditionManager.cs"
    click EPC href "#" "ElectricalPanelController.cs"
    click EV href "#" "EventManager.cs"
    click INT href "#" "Interactor.cs"
    click Door href "#" "InteractDoor.cs"
    click Bed href "#" "InteractBed.cs"
    click RB href "#" "InteractRationBoard.cs"
    click Elec href "#" "InteractElectricalPanel.cs"
    click PC href "#" "InteractPC.cs"
    click MRP href "#" "MorningReportPanel.cs"
    click RAT href "#" "RationMenuUI.cs"
    click EXP href "#" "ExpeditionsTabUI.cs"
    click AEU href "#" "ActiveExpeditionsUI.cs"
    click EPUI href "#" "ElectricalPanelUI.cs"
    click DNU href "#" "DoorNoteUI.cs"
    click CO href "#" "CharaterObject.cs"
    click TRT href "#" "Trait.cs"
```

## Описание слоёв

| Слой | Что входит | Ключевые файлы |
|------|-----------|----------------|
| **Игрок** | Внешний пользователь (мышь, клавиатура) | — |
| **Ввод (Input)** | `Interactor` — raycast-взаимодействие, `PlayerMove` — перемещение (отключён), `CinemachineCursorLook` — камера, `Quit` — выход | `Interactor.cs`, `PlayerMove.cs`, `CinemachineCursorLook.cs`, `Quit.cs` |
| **Менеджеры (シングルトны на GameObject «Manager»)** | `DayCycleManager`, `ResourceManager`, `CharacterManager`, `CharacterGenerator`, `ExpeditionManager`, `ElectricalPanelController`, `EventManager` | Соответствующие `.cs` в `Assets/Scripts/` |
| **Объекты мира** | Дверь, кровать, рационная доска, электропанель, ПК, записка — все реализуют `IInteractable` / `IHighlightable` | `InteractDoor.cs`, `InteractBed.cs`, `BedSleepTrigger.cs`, `InteractRationBoard.cs`, `InteractElectricalPanel.cs`, `InteractPC.cs`, `InteractNote.cs` |
| **UI** | Панели меню, карточки персонажей, списки вылазок, экран поражения | `*UI.cs` в `Assets/Scripts/UI/`, `Assets/Scripts/Character/`, `Assets/Scripts/Door/` |
| **Данные (SO + модели)** | ScriptableObjects: локации вылазок, черты, записи у двери, каталог, пул имён, настройки комментариев; модели: `ResourceData`, `NightReportData`, `GeneratedCharacter` | `*.cs` в `Assets/ScriptableObjects/`, `Assets/Scripts/Core/`, `Assets/Scripts/Resources/` |

## Что отличается от краткой версии

1. **Добавлены все объекты мира** — дверь, кровать, рационная доска, электропанель, ПК, записка, кнопка сна (`BedSleepTrigger`).
2. **Добавлена система ввода** — `Interactor`, `PlayerMove`, `CinemachineCursorLook`, `Quit`.
3. **Полный состав UI** — все 8 панелей + 7 элементов UI (карточки, кнопки, переключатель страниц).
4. **Полный состав данных** — `ResourceData`, `ExpeditionLocationCatalog`, `NamePool`, `CommandantCommentSettings`, а также неиспользуемые заготовки (`Stati`, `HiddenStatDefinition`, `disease`).
5. **Все связи менеджеров** — включая `ElectricalPanelController → CharacterManager`, `EventManager → DayCycleManager`, `CharacterGenerator → NamePool/Trait` и др.
6. **Стили и кликабельные ссылки** на исходные `.cs`-файлы.

## Где создан

`D:\Documents\GITHUB2.0\doomed game\Assets\GameArchitecture_Complete.md`
