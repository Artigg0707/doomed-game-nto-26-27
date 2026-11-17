# 📦 ResourceManager — менеджер ресурсов бункера

## Назначение
Единственный источник истины по запасам бункера: еда, вода, топливо, температура. Без этого скрипта игра не является выживанием.

## Поля (Singleton)
```csharp
public static ResourceManager Instance;
public int foodCans;      // банки еды (старт 8–10 по диздоку)
public int waterBottles;  // бутылки воды (старт 8–10)
public int fuelCans;      // канистры топлива/деталей (старт 4–6)
public int temperatureLevel; // 0..3 (0 Обледенение, 3 Тепло)
public bool generatorBroken; // генератор сломан
public event Action OnResourcesChanged; // для UI
```

## Методы
- `bool ConsumeFood(int count)` — вернуть false если не хватает
- `bool ConsumeWater(int count)`
- `bool ConsumeFuel(int count)`
- `void AddFood(int count)`, `void AddWater(int count)`, `void AddFuel(int count)` — после экспедиций
- `void SetTemperature(int level)` — clamping 0..3
- `void BreakGenerator()` / `void RepairGenerator()`

## Зависимости
- `CharacterManager` — при ночном расходе итерируем всех персонажей и списываем ресурсы
- `DayCycleManager` — вызывает расход ресурсов за ночь

## Критерии готовности
- [ ] Игра запускается со стартовыми ресурсами по диздоку
- [ ] Потратить/получить ресурсы можно без ошибок
- [ ] UI обновляется при изменении (событие OnResourcesChanged)
