# Diploma2025 — ML‑Agents автогонка

Проект Unity (2022.3.51f1) с автогонками и агентами ML‑Agents. Содержит обученные модели (ONNX), демонстрации для имитационного обучения и несколько сцен (меню и трассы).

## Быстрый старт
1) Откройте проект в Unity **2022.3.51f1**.
2) Убедитесь, что ML‑Agents подключён (см. «Зависимости»).
3) Откройте сцену `Assets/Scenes/Menu.unity` или `Assets/Scenes/Env_Alone.unity`.
4) Нажмите **Play**.

## Требования
- Unity **2022.3.51f1** (см. `ProjectSettings/ProjectVersion.txt`).
- Пакеты, перечисленные в `Packages/manifest.json`.
- ML‑Agents подключён локально:
  - `com.unity.ml-agents` и `com.unity.ml-agents.extensions` указывают на `file:C:/ml-agents/...`.
  - Если путь отличается, обновите `Packages/manifest.json` или создайте локальный каталог `C:/ml-agents`.

## Сцены
- `Assets/Scenes/Env_Alone.unity` — одиночная трасса (включена в сборку по умолчанию).
- `Assets/Scenes/Env_Duo.unity` — трасса с несколькими агентами.
- `Assets/Scenes/Menu.unity` — главное меню.

Список сцен для сборки хранится в `ProjectSettings/EditorBuildSettings.asset`.

## Управление
- **W/S** — газ/тормоз (ось `Vertical`).
- **A/D** — поворот (ось `Horizontal`).
- **Esc** — пауза (скрипт `PauseScript`).
- **E** — переключение камер (скрипт `CameraChange`).

> Управление через клавиатуру используется, если у агента включён флаг `isPlayer` в `CarControllerAgent`.

## Основные системы
### Вождение и физика
- Колёса и физика реализованы через `WheelCollider` и `Rigidbody`.
- Скорость и торможение управляются `CarController`.

### Контроль чекпоинтов
- `TrackCheckpoints` управляет последовательностью чекпоинтов и выдаёт события «правильный/неправильный».
- `CheckpointSingle` скрывает/показывает визуальный чекпоинт и вызывает трекер при въезде.

### Spline‑навигация
- `SplineCalculator` строит сглаженный сплайн по дочерним точкам объекта `SplinePoints`.
- `CarSplineStats` вычисляет расстояние до сплайна, прогресс, угол и локальную кривизну.
- `AIController` может выдавать базовые «руль/газ» по направлению к точке сплайна.

### ML‑Agents
- `CarControllerAgent` — агент ML‑Agents с наградами за скорость, прохождение чекпоинтов и штрафами за ошибки/столкновения.
- Наблюдения включают данные сплайна, дистанцию до чекпоинта, направление и скорость.
- Обученные модели в формате **ONNX** лежат в `Assets/Brains/`, а также в корне `Assets/` и `Assets/Scenes/`.

## Среда ML‑Agents (конфигурация проекта)
### Версии и протокол
- Unity ML‑Agents (Unity package): **2.3.0-exp.3** (см. `Assets/ML-Agents/Timers/*.json`).
- Communication protocol version: **1.5.0** (см. `Assets/ML-Agents/Timers/*.json`).
- Unity Editor: **2022.3.51f1** (см. `ProjectSettings/ProjectVersion.txt` и метаданные таймеров).

### Поведение агента
- Класс агента: `CarControllerAgent` (наследуется от `Agent`).
- Режим игрока: при `isPlayer = true` управление берётся с осей `Vertical/Horizontal`, иначе используется `AIController`.
- Действия: **Continuous** (2 значения)
  - `actions[0]` — газ/тормоз (forward)
  - `actions[1]` — поворот (turn)
- Наблюдения (CollectObservations):
  - расстояние до сплайна
  - прогресс по сплайну
  - угол к направлению сплайна
  - локальная кривизна
  - расстояние до следующего чекпоинта
  - скалярное произведение направления автомобиля и чекпоинта
  - модуль скорости `rb.velocity.magnitude`

### Награды и штрафы
Основные механики наград в `CarControllerAgent`:
- +10 и рост за каждый правильный чекпоинт (и +1 сек к таймеру).
- -2 за неправильный чекпоинт.
- +0.005 при близости к сплайну и скорости >= 20 км/ч (движение вперёд).
- -0.001 за шаг (per‑step penalty).
- -0.001 если скорость ниже 0.5.
- -10 за столкновения со стеной или игроком.
- Завершение эпизода при:
  - истечении таймера (штраф -20),
  - достижении всех чекпоинтов (бонус +5000),
  - превышении лимита шагов `maxSteps` (штраф -100).

### Сенсоры и визуальные наблюдения
- Скрипт `AddCameraSensor` добавляет `CameraSensorComponent`:
  - размер 84×84, `ObservationStacks = 1`, `CompressionType = PNG`.
- Скрипт `DebugCameraSensor` выводит состояние камеры и сенсора.

### Демонстрации и модели
- Демонстрации: `Assets/Demonstrations/*.demo`.
- Модели поведения (ONNX): `Assets/Brains/` и копии в `Assets/` и `Assets/Scenes/`.
- Основная модель: `Assets/Brains/CarDriverBehavior.onnx`.

### Таймеры производительности ML‑Agents
Логи таймеров находятся в `Assets/ML-Agents/Timers/`:
- `SampleScene_timers.json`
- `Menu_timers.json`
- `Env_Duo_timers.json`
- `Env_Alone_timers.json`
Таймеры фиксируют версию ML‑Agents, протокол и метрики выполнения для каждой сцены.

### UI и меню
- `MenuScript` переключает сцены и завершает игру.
- `PauseScript` управляет паузой и анимациями меню.
- `PositionCounter` считает позицию игрока среди агентов.

## Скрипты
Файлы в `Assets/Scripts/`:

- `AddCameraSensor.cs` — добавляет `CameraSensorComponent` на объект камеры (84×84, PNG компрессия).
- `AgentSpawner.cs` — создание префаба агента и смена материала.
- `AIController.cs` — простое следование по сплайну (руль/газ).
- `CameraChange.cs` — переключение между основной и дополнительной камерой по **E**.
- `CarController.cs` — физика автомобиля, передачи, тормоз/газ и поворот.
- `CarControllerAgent.cs` — агент ML‑Agents, награды/наблюдения/эпизоды.
- `CarSplineStats.cs` — метрики движения относительно сплайна.
- `Checkpoint.cs` — заготовка (пустой MonoBehaviour).
- `CheckpointSingle.cs` — единичный чекпоинт (Trigger).
- `DebugCameraSensor.cs` — проверка подключения камеры к сенсору.
- `FollowPlayer.cs` — камера следует за объектом игрока.
- `PauseScript.cs` — пауза/продолжение/выход/в меню.
- `Player.cs` — маркер игрока (пустой MonoBehaviour).
- `PositionCounter.cs` — расчёт позиции игрока среди агентов.
- `PrefabSpawner.cs` — выбор префаба и материала автомобиля.
- `SplineCalculator.cs` — генерация сплайна и визуализация линией.
- `TrackCheckpoints.cs` — логика прохождения чекпоинтов.
- `VehicleMoving.cs` — простое движение вперёд с заданной скоростью.
- `Wall.cs` — маркер стены (пустой MonoBehaviour).

Также присутствует `Assets/UI/MenuScript.cs` для логики меню.

## Ассеты и данные
- **ONNX‑модели**: `Assets/Brains/` и корень `Assets/` (несколько версий моделей поведения).
- **Демонстрации**: `Assets/Demonstrations/*.demo` (для обучения на демонстрациях).
- **Префабы**: `Assets/Prefabs/` (машины, окружение, точки спавна, трассы).
- **Материалы**: `Assets/Materials/`.
- **UI**: `Assets/UI/` (кнопки, фоновые изображения, анимации).
- **3D‑контент**: `Assets/Race Car Package/` (FBX, текстуры, материалы).
- **TextMesh Pro**: `Assets/TextMesh Pro/`.

## Зависимости (пакеты Unity)
Список подключён в `Packages/manifest.json`, ключевые:
- `com.unity.ml-agents` и `com.unity.ml-agents.extensions` (локальные пакеты).
- `com.unity.textmeshpro`, `com.unity.ugui`, `com.unity.visualscripting`.

## Запуск обучения (общие шаги)
1) Установите ML‑Agents (Python) вне Unity по инструкции от Unity.
2) Убедитесь, что в сцене назначена нужная модель/behavior.
3) Запускайте тренировку командой `mlagents-learn` (конфиг и параметры — по вашей конфигурации).

> В проекте уже присутствуют ONNX‑модели и demo‑файлы, их можно использовать для инференса/дообучения.

## Известные особенности
- В `Packages/manifest.json` используются локальные пути к ML‑Agents (`C:/ml-agents/...`). При переносе проекта их нужно изменить.
- Часть скриптов (например `Checkpoint.cs`, `Player.cs`, `Wall.cs`) — маркеры/заготовки без логики.

## Структура проекта (основное)
- `Assets/Scenes/` — сцены.
- `Assets/Scripts/` — логика игры, ML‑Agents и утилиты.
- `Assets/Prefabs/` — префабы машин и окружения.
- `Assets/Demonstrations/` — демонстрации ML‑Agents.
- `Assets/Brains/` — ONNX‑модели.
- `Assets/UI/` — интерфейс и анимации.
- `Packages/` — зависимости Unity.
- `ProjectSettings/` — настройки проекта.

---
Если нужно, могу дополнить README скриншотами сцен, описанием UI‑экранов или инструкцией по сборке под конкретную платформу.
