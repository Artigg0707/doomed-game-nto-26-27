# Схема данных игры «Doomed Game»

> Источник: анализ всех `.cs`-скриптов и `.asset`-конфигов в `Assets/ScriptableObjects/`, `Assets/Scripts/Core/`, `Assets/Scripts/Resources/`.
> Показывает структуры данных, их поля и связи (кто читает, кто пишет, кто создаёт).
>
> Открывайте в любом редакторе с поддержкой Mermaid.

```mermaid
erDiagram
    %% ==================== ГЛАВНЫЕ СУЩНОСТИ ====================
    GameSession ||--|| ResourceData : "содержит"
    GameSession ||--|| NightReportData : "генерирует каждую ночь"
    GameSession }o--|| CharacterManager : "управляет"
    GameSession }o--|| ExpeditionManager : "управляет"

    %% Ресурсы
    ResourceData ||--|| NightReportData : "потребляется в"

    %% Персонажи
    CharacterManager }o--o{ GeneratedCharacter : "содержит список"
    GeneratedCharacter }o--|| Trait : "имеет черты (0 или больше)"
    GeneratedCharacter }o--|| NamePool : "имена берутся из"
    GeneratedCharacter }o--|| CharaterObject : "профиль генерации"

    %% Вылазки
    ExpeditionManager }o--o{ ExpeditionRun : "активные вылазки"
    ExpeditionRun }o--|| ExpeditionLocationData : "локация вылазки"
    ExpeditionRun }o--|| GeneratedCharacter : "персонаж в вылазке"
    ExpeditionLocationCatalog }o--o{ ExpeditionLocationData : "каталог локаций"
    ExpeditionLocationData }o--o{ ExpeditionRewardData : "возможные награды"

    %% Записки у двери
    EventManager }o--o{ DoorNoteData : "загружает и хранит"
    DoorNoteData }o--o{ DoorNoteData_Option : "варианты ответа"

    %% Генератор / ремонт
    ElectricalPanelController }o--|| ResourceManager : "управляет генератором"
    ElectricalPanelController }o--o{ GeneratedCharacter : "кандидаты на ремонт"

    %% События и отчёты
    NightReportData }o--|| ResourceData : "отражает изменения ресурсов"
    NightReportData }o--o{ GeneratedCharacter : "выживание, смерти, уходы"
    NightReportData }o--o{ ExpeditionRun : "результаты возвращений"

    %% Комментарии коменданта
    MorningReportPanel }o--|| CommandantCommentSettings : "шаблоны текста"

    %% ==================== ПОЛЯ СУЩНОСТЕЙ ====================

    ResourceData {
        int foodCans
        int waterBottles
        int fuelCans
        int currentDay
        int temperatureLevel
        bool generatorBroken
        bool generatorIsOn
    }

    GeneratedCharacter {
        Gender gender
        string fullName
        int age
        Sprite portrait
        int health 0-100
        int morale 0-100
        int daysWithoutFood
        int daysWithoutWater
        bool receivesFoodToday
        bool receivesWaterToday
        int consecutiveFoodDays
        int consecutiveWaterDays
        bool isDead
        bool isExiled
        bool isSecretRevealed
        List<Trait> traits
        string description
    }

    NightReportData {
        int completedDay
        int newDay
        bool fuelConsumed
        bool generatorWorked
        int temperatureBefore
        int temperatureAfter
        List<string> resourceChanges
        List<string> survivalChanges
        List<string> deaths
        List<string> departures
        List<string> expeditionReturns
        List<string> specialEvents
        List<string> morningReportLines
    }

    ExpeditionLocationData {
        string locationId
        string locationName
        Sprite locationImage
        string description
        string riskDescription
        int durationDays
        int injuryChancePercent 0-100
        int deathChancePercent 0-100
        int minInjuryDamage
        int maxInjuryDamage
        int minCooldownDays
        int maxCooldownDays
        List<ExpeditionRewardData> possibleRewards
    }

    ExpeditionRewardData {
        ExpeditionRewardType resourceType Food|Water|Fuel
        int minAmount
        int maxAmount
    }

    DoorNoteData {
        SpawnType spawnType Scheduled|Random
        string title
        string text
        int triggerDay
        int randomChance 0-100
        string morningReportLine
        List<Option> options
    }

    DoorNoteData_Option {
        string buttonText
        int deltaFood
        int deltaWater
        int deltaFuel
        int deltaMorale
        int hpDamage
        int spawnCharacterChance 0-100
    }

    Trait {
        string traitName
        string traitDesc
        Sprite traitSprite
        TraitCategory category Positive|Negative
        List<TraitEffect> effects
    }

    TraitEffect {
        EffectType effectType
        float value
        int locationNumber
    }

    CommandantCommentSettings {
        List<string> deathComments
        List<string> hungryComments
        List<string> thirstyComments
        List<string> injuredComments
        List<string> hungryAndThirstyComments
        List<string> calmComments
        List<string> loneComments
    }

    CharaterObject {
        List<Stati> availableStats
        List<Trait> availableTraits
        int minAge
        int maxAge
        List<Sprite> maleObliks
        List<Sprite> femaleObliks
        NamePool namePool
    }

    NamePool {
        List<string> maleNames
        List<string> maleLastNames
        List<string> femaleNames
        List<string> femaleLastNames
    }

    Stati {
        Sprite icon
        string statName
        string statDesc
        Color statColor
        float minValue
        float maxValue
        float minDefaultValue
        float maxDefaultValue
        float baseDecayPerDay
    }

    HiddenStatDefinition {
        string statName
        string statDesc
        float minValue
        float maxValue
        bool isMnozhitel
        Stati linkStat
    }

    disease {
        string diseaseName
        string symptoms
        Sprite icon
        List<StatEffect> statEffects
        List<CureRequirement> cureRequirements
        deathType DeathType Natural|Suicide|Freezing
        float baseDeathChance 0-1
        float deathChanceGrowth 0-0.1
        float maxDeathChance 0-1
    }

    %% ==================== ПОТОКИ ДАННЫХ ====================

    %% Ресурсы ↔ Менеджеры
    ResourceManager --> ResourceData : "читает/пишет поля"
    CharacterManager --> ResourceManager : "ConsumeFood/Water при выдаче пайка"
    ExpeditionManager --> ResourceManager : "AddFood/Water/Fuel при награде"
    EventManager --> ResourceManager : "ApplyOption меняет еду/воду/топливо"
    DayCycleManager --> ResourceManager : "ConsumeFuel, AddTemperature, SetTemperature"
    ElectricalPanelController --> ResourceManager : "BreakGenerator, RepairGenerator, SetGeneratorOn"

    %% Персонажи ↔ Менеджеры
    CharacterGenerator --> CharaterObject : "профиль (имена, портреты, черты, возраст)"
    CharacterGenerator --> NamePool : "генерация имени и фамилии"
    CharacterGenerator --> Trait : "случайная черта при создании"
    CharacterManager --> GeneratedCharacter : "список всех жителей"
    CharacterManager --> ResourceManager : "проверка ресурсов перед пайком"
    DayCycleManager --> CharacterManager : "UpdateSurvivalCounters, ResetDailyRations"
    EventManager --> CharacterManager : "изменение мораль/здоровье/новый житель"
    ElectricalPanelController --> CharacterManager : "выбор кандидата на ремонт"
    ExpeditionManager --> CharacterManager : "жив ли персонаж, не в поездке ли"

    %% Вылазки ↔ Данные
    ExpeditionManager --> ExpeditionLocationData : "риск, длительность, награды, cooldown"
    ExpeditionLocationCatalog --> ExpeditionLocationData : "список всех локаций"
    ExpeditionManager --> NightReportData : "expeditionReturns после завершения"

    %% Записки ↔ Данные
    EventManager --> DoorNoteData : "загружает через Resources.LoadAll"
    DoorNoteData --> DoorNoteData_Option : "варианты ответа с изменениями"
    DoorNoteData_Option --> ResourceManager : "deltaFood/Water/Fuel"
    DoorNoteData_Option --> CharacterManager : "deltaMorale, hpDamage, spawnCharacterChance"
    DoorNoteData --> NightReportData : "morningReportLine, specialEvents"

    %% Отчёт ↔ Данные
    DayCycleManager --> NightReportData : "собирает все поля за ночь"
    NightReportData --> MorningReportPanel : "источник текста отчёта"
    MorningReportPanel --> CommandantCommentSettings : "шаблон комментария"

    %% Генератор
    ElectricalPanelController --> NightReportData : "ProcessGeneratorNight добавляет specialEvents"
    ElectricalPanelController --> GeneratedCharacter : "требует персонажа для ремонта"

    classDef entity fill:#e1f5fe,stroke:#01579b,stroke-width:2px;
    classDef data fill:#fff3e0,stroke:#e65100,stroke-width:2px;
    classDef manager fill:#f3e5f5,stroke:#4a148c,stroke-width:2px;
    classDef unused fill:#f5f5f5,stroke:#9e9e9e,stroke-width:1px,stroke-dasharray: 5 5;

    class GameSession,ResourceManager,CharacterManager,CharacterGenerator,ExpeditionManager,ElectricalPanelController,EventManager,DayCycleManager,MorningReportPanel manager;
    class ResourceData,GeneratedCharacter,NightReportData,ExpeditionLocationData,ExpeditionRewardData,DoorNoteData,DoorNoteData_Option,Trait,TraitEffect,CommandantCommentSettings,CharaterObject,NamePool,Stati,HiddenStatDefinition,disease data;
```

## Словарик данных (назначение полей)

### ResourceData — состояние ресурсов бункера
| Поле | Назначение |
|------|-----------|
| `foodCans` | Количество банок еды на складе |
| `waterBottles` | Количество бутылок воды |
| `fuelCans` | Количество канистр топлива |
| `currentDay` | Текущий номер дня |
| `temperatureLevel` | Температура 0–3 (0 = критический холод, 3 = норма) |
| `generatorBroken` | Генератор сломан (нельзя включить до ремонта) |
| `generatorIsOn` | Генератор включён (расходует топливо ночью) |

### GeneratedCharacter — один житель бункера
| Поле | Назначение |
|------|-----------|
| `gender` | Мужчина/Женщина (для шаблонов описания) |
| `fullName` | Имя и фамилия |
| `age` | Возраст (из minAge–maxAge профиля) |
| `portrait` | Спрайт портрета (из профиля, без повторов) |
| `health` | Здоровье 0–100. При 0 → смерть |
| `morale` | Мораль 0–100. При <0 → уходит из бункера |
| `daysWithoutFood` | Счётчик дней без еды |
| `daysWithoutWater` | Счётчик дней без воды |
| `receivesFoodToday` | Выдана ли еда сегодня |
| `receivesWaterToday` | Выдана ли вода сегодня |
| `consecutiveFoodDays` | Дней подряд получает еду (для восстановления) |
| `consecutiveWaterDays` | Дней подряд получает воду |
| `isDead` | Персонаж погиб |
| `isExiled` | Персонаж ушёл из бункера |
| `traits` | Черты персонажа (из `Trait` SO) |
| `description` | Текстовое описание (генерируется шаблонами) |

### NightReportData — ночной отчёт (основная структура передачи данных между ночью и утром)
| Поле | Назначение |
|------|-----------|
| `completedDay` | Завершённый день |
| `newDay` | Новый день (наступает утром) |
| `fuelConsumed` | Был ли израсходован 1 канистр топлива |
| `generatorWorked` | Работал ли генератор всю ночь |
| `temperatureBefore` | Температура до ночи |
| `temperatureAfter` | Температура после ночи |
| `resourceChanges` | Текстовые изменения ресурсов для отчёта |
| `survivalChanges` | Состояние каждого жителя (голод, жажда, здоровье) |
| `deaths` | Имена погибших за ночь |
| `departures` | Имена ушедших из-за низкой морали |
| `expeditionReturns` | Результаты возвращения с вылазок |
| `specialEvents` | Особые события (поломка/ремонт генератора, записи) |

### ExpeditionLocationData — локация вылазки (ScriptableObject)
| Поле | Назначение |
|------|-----------|
| `locationName` | Название локации (отображается в UI) |
| `description` | Описание |
| `riskDescription` | Текст риска |
| `durationDays` | Сколько дней длится вылазка |
| `injuryChancePercent` | Базовый шанс травмы 0–100 |
| `deathChancePercent` | Базовый шанс смерти 0–100 |
| `minInjuryDamage / maxInjuryDamage` | Диапазон урона при травме |
| `minCooldownDays / maxCooldownDays` | Перезарядка локации после вылазки |
| `possibleRewards` | Список возможных наград (еда/вода/топливо) |

### DoorNoteData — записка у двери (ScriptableObject)
| Поле | Назначение |
|------|-----------|
| `spawnType` | `Scheduled` (по дню) или `Random` (по шансу) |
| `triggerDay` | День срабатывания (для запланированных) |
| `randomChance` | Шанс случайного появления 0–100 |
| `title` / `text` | Заголовок и текст записи |
| `morningReportLine` | Строка для утреннего отчёта |
| `options` | Варианты ответа игрока |

### DoorNoteData.Option — вариант ответа на записку
| Поле | Назначение |
|------|-----------|
| `buttonText` | Текст кнопки |
| `deltaFood / deltaWater / deltaFuel` | Изменение ресурсов (положительное = получить, отрицательное = потратить) |
| `deltaMorale` | Изменение морали всем живым персонажам |
| `hpDamage` | Урон здоровью случайному живому персонажу |
| `spawnCharacterChance` | Шанс 0–100 создать нового жителя |

## Неиспользуемые / нереализованные данные (в разработке)

| Сущность | Статус | Примечание |
|----------|--------|------------|
| `Stati` | Не используется в коде | Определение параметра с иконкой, цветом, распадом в день — заготовка |
| `HiddenStatDefinition` | Не используется | Скрытый параметр со ссылкой на `Stati` — заготовка |
| `disease` | Не используется | Болезнь с эффектами на параметры, лечением и типом смерти — заготовка |
| `CharaterObject.availableStats` | Закомментировано | Список `Stati` в профиле персонажа закомментирован |
| `disease.availableDiseases` | Закомментировано | В `CharaterObject` закомментировано |

## Файл для ТЗ

`D:\Documents\GITHUB2.0\doomed game\Assets\GameDataSchema.md`
