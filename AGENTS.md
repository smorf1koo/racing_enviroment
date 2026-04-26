# AGENTS.md

Diploma2025 — симуляция автогонок с обучением с подкреплением на Unity ML-Agents и gRPC-интерфейсом для внешних RL-клиентов.

Этот файл — инструкция для Claude Code / OpenAI Codex CLI. Агентов нет. Ты делаешь всё сам.

---

## Первый запуск — ознакомление с проектом

Прочитай параллельно:

1. `README.md` — что за проект и как запустить
2. `Assets/Proto/unity_racing.proto` — gRPC-интерфейс: сервис, сообщения, типы
3. `Assets/Scripts/CarControllerAgent.cs` — главный ML-агент: наблюдения, действия, награды
4. `Assets/Scripts/UnityRacingGrpcServer.cs` — gRPC-сервер: Reset/Step, поток
5. `git log --oneline -5` — последние изменения
6. `git status -s` — незакоммиченные изменения

Запомни контекст. Не печатай результат.

---

## Стек

- **Unity 2022.3.51f1** — игровой движок
- **C#** — язык скриптов
- **Unity ML-Agents 2.3.0-exp.3** — фреймворк обучения агентов (локальный путь в `Packages/manifest.json`)
- **gRPC / Grpc.Core + Google.Protobuf** — удалённый интерфейс управления (NuGet for Unity)
- **Protobuf 3** — сериализация сообщений (`Assets/Proto/unity_racing.proto`)
- **ONNX** — формат обученных моделей (`Assets/Brains/`)

---

## Структура проекта

```
Assets/
├── Scripts/                    # Все C# скрипты
│   ├── CarControllerAgent.cs   # ML-агент (наблюдения, действия, награды)
│   ├── CarController.cs        # Физика машины (WheelCollider, Rigidbody)
│   ├── CarSplineStats.cs       # Метрики машины относительно сплайна трека
│   ├── SplineCalculator.cs     # Генерация сплайна из точек трека
│   ├── TrackCheckpoints.cs     # Управление чекпоинтами
│   ├── AIController.cs         # Простой автопилот по сплайну
│   ├── UnityRacingGrpcServer.cs  # gRPC-сервер (порт 50051)
│   ├── UnityMainThreadDispatcher.cs  # Маршалинг gRPC → main thread
│   └── UnityRacing/            # Сгенерированный protobuf-код
│       ├── UnityRacing.cs      # Сообщения protobuf
│       └── UnityRacingGrpc.cs  # gRPC-сервис
├── Proto/
│   └── unity_racing.proto      # Определение gRPC-сервиса и сообщений
├── Scenes/
│   ├── Menu.unity              # Главное меню
│   ├── Env_Alone.unity         # Один агент
│   └── Env_Duo.unity           # Два агента
├── Brains/                     # ONNX-модели (CarDriverBehavior.onnx)
├── Demonstrations/             # .demo файлы для imitation learning
├── Plugins/                    # DLL: Grpc.Core, Google.Protobuf
└── UI/                         # UI-префабы, MenuScript.cs

Packages/
└── manifest.json               # Unity пакеты (ml-agents, NuGet for Unity)

_proto_gen/
└── ProtoGen.csproj             # Проект для кодогенерации protobuf
```

---

## Ключевые константы

| Параметр | Значение |
|----------|----------|
| gRPC порт | 50051 |
| Вектор наблюдений | 7 float: distance_to_spline, progress, angle, curvature, dist_checkpoint, direction_dot, speed |
| Вектор действий | 2 float: forward ∈ [-1,1], turn ∈ [-1,1] |
| maxSteps | 3000 |
| Таймер эпизода | 30 секунд |
| Camera sensor | 84×84, PNG |

### Награды

| Событие | Награда |
|---------|---------|
| Правильный чекпоинт | +10 + (checksOver / 5) |
| Неправильный чекпоинт | -2 |
| Близко к сплайну + speed ≥ 20 км/ч | +0.005 / кадр |
| Базальный штраф за шаг | -0.001 |
| Стоит (speed < 0.5 м/с) | -0.001 |
| Столкновение со стеной / игроком | -10 |
| Все чекпоинты пройдены | +5000 |
| Таймер истёк | -20 |
| maxSteps превышен | -100 |

---

## Правила кода

| Правило | Детали |
|---------|--------|
| Null-safety | Нет `!` (null-forgiving). Используй `?.`, проверки на null, `TryGetComponent` |
| Именование | Классы / компоненты — PascalCase; поля — camelCase; константы — UPPER_SNAKE_CASE |
| SerializeField | Используй `[SerializeField]` вместо public-полей где не нужен внешний доступ |
| Кодогенерация | Не редактируй `Assets/Scripts/UnityRacing/*.cs` вручную — они генерируются из `.proto` |
| Протобуф | Изменения начинаются с `.proto` файла → потом кодогенерация |
| Thread safety | gRPC-коллбеки выполняются не в main thread → всегда диспатч через `UnityMainThreadDispatcher` |
| ML-Agents API | Не угадывай методы — читай файлы агента и документацию ML-Agents |
| Язык docs | Все комментарии и документация — на **русском**. Commit messages — на **английском** |

---

## Команды workflow

### /start — ознакомление с проектом

Когда начинаешь работу с нуля или после перерыва.

**Шаги:**

1. Прочитай параллельно: `README.md`, `Assets/Proto/unity_racing.proto`, `Assets/Scripts/CarControllerAgent.cs`, `Assets/Scripts/UnityRacingGrpcServer.cs`
2. `git log --oneline -5` и `git status -s`
3. Подтверди одной строкой, например: «Прочитал. Unity 2022.3, ML-Agents 2.3, gRPC порт 50051, вектор наблюдений 7 float.»

---

### /research \<что\>

Исследуй кодовую базу перед изменениями.

**Шаги:**

1. Сформулируй что именно затрагивает задача
2. Найди и прочитай ВСЕ затрагиваемые файлы (Glob + Read параллельно)
3. Для каждого файла: путь, ключевые методы с номерами строк, нужно ли менять
4. Запиши: текущий data flow, gRPC-контракт (если меняется), точки интеграции между скриптами
5. Составь таблицу «Связанные файлы»:

| Файл | Действие | Описание |
|------|----------|----------|
| `Assets/Scripts/Foo.cs` | MODIFY | Добавить обработку X |
| `Assets/Scripts/Bar.cs` | CREATE | Новый компонент |
| `Assets/Proto/unity_racing.proto` | MODIFY / НЕ МЕНЯЕТСЯ | ... |

6. Покажи результат пользователю

---

### /design \<что\>

Спроектируй решение перед реализацией.

**Шаги:**

1. Прочитай результаты `/research` (или выполни его сначала)
2. Опиши секции:
   - **Контекст** — откуда задача, зачем
   - **Решение** — выбранный подход и почему
   - **Альтернативы** — таблица отклонённых вариантов с причинами
   - **Затрагиваемые компоненты** — какие скрипты создаются / меняются
   - **Data Flow** — как данные проходят (gRPC → сервер → Main Thread → Agent → ответ)
   - **Изменения proto** — если меняется `.proto`, опиши новые типы/методы
   - **Риски** — thread safety, ML-Agents lifecycle, backward compatibility gRPC

---

### /plan \<что\>

Разбей реализацию на шаги.

**Шаги:**

1. Прочитай результат `/design`
2. Разбей на фазы **снизу вверх**: proto → сгенерированный код → низкоуровневые скрипты → агент → сцена → интеграция
3. Для каждой фазы:
   - **File Ownership** — точные пути файлов (CREATE / MODIFY)
   - **Interface Contract** — публичные методы / сигнатуры, gRPC-сообщения
   - **Depends on** — от каких фаз зависит
   - **Verification** — как проверить (сборка в Unity, запуск сцены, grpc-тест)
4. Покажи план пользователю и жди подтверждения

---

### /implement \<что\>

Реализуй код по плану.

**Шаги:**

1. Прочитай план (или выполни `/plan` сначала)
2. Прочитай ВСЕ файлы из File Ownership **перед тем как писать**
3. Если меняется `.proto`:
   - Внеси изменения в `Assets/Proto/unity_racing.proto`
   - Обнови сгенерированный код в `Assets/Scripts/UnityRacing/` согласно новым типам
   - **Никогда не редактируй** сгенерированные файлы вручную — только через регенерацию
4. Реализуй файлы по фазам
5. После каждого файла проверь:
   - Нет ли прямых вызовов Unity API из gRPC-потока (нужен `UnityMainThreadDispatcher`)
   - Нет ли null-разыменований без проверок
   - Именование соответствует правилам

**Правила кода:**

- Нет `!` (null-forgiving operator)
- Нет магических чисел — только именованные константы или `[SerializeField]`
- gRPC-коллбеки → только через `UnityMainThreadDispatcher.Enqueue(...)`
- Не трогай `Assets/Scripts/UnityRacing/*.cs` напрямую

---

### /fix \<описание бага\>

Исправь баг.

**Шаги:**

1. Прочитай все скрипты, которые могут быть причастны
2. Сформулируй точные шаги воспроизведения, expected vs actual поведение
3. **Сформулируй 2–3 гипотезы** с evidence (`файл:строка`) для каждой — от наиболее вероятной к наименее
4. **Покажи root cause пользователю и жди подтверждения** перед правками
5. После подтверждения — исправь минимально необходимый код
6. Проверь: нет ли похожих мест в других скриптах с той же ошибкой
7. Опиши что исправил:

```
## Fix: Описание — YYYY-MM-DD

### Root cause
- `файл:строка` — механика бага

### Гипотезы
1. Гипотеза — ✓/✗
2. Гипотеза — ✓/✗

### Что исправлено
- `файл:строка` — изменение
```

---

### /review \<что\>

Самопроверка кода.

**Шаги:**

1. Прочитай все изменённые файлы
2. Проверь по чеклисту:
   - **Thread safety** — нет вызовов Unity API вне main thread
   - **Null safety** — нет `!`, есть проверки на null
   - **Proto consistency** — если изменён `.proto`, обновлён ли сгенерированный код?
   - **ML-Agents lifecycle** — `Initialize`, `OnEpisodeBegin`, `CollectObservations`, `OnActionReceived` вызываются правильно
   - **Reward logic** — нет случайных накоплений награды, нет двойного счёта
   - **Observation vector** — 7 значений, те же что в `CarSplineStats` и gRPC-ответе
   - **Соответствие плану** — все файлы из File Ownership реализованы
3. Выведи вердикт: **APPROVE** / **REQUEST CHANGES** с перечнем проблем

---

### /explain \<файл или компонент\>

Объясни как работает код.

**Шаги:**

1. Прочитай указанный файл
2. Прочитай связанные файлы (что он вызывает, кто его вызывает)
3. Объясни прогрессивно: общая картина → структура → data flow → детали реализации

---

### /proto \<что изменить\>

Измени gRPC-интерфейс.

**Шаги:**

1. Прочитай `Assets/Proto/unity_racing.proto`
2. Прочитай `Assets/Scripts/UnityRacingGrpcServer.cs` — текущая реализация сервиса
3. Внеси изменения в `.proto`
4. Обнови `Assets/Scripts/UnityRacing/UnityRacing.cs` и `UnityRacingGrpc.cs` — привести в соответствие с новыми типами
5. Обнови реализацию в `UnityRacingGrpcServer.cs`
6. Покажи итоговый diff и жди подтверждения

---

### /obs \<что изменить\>

Измени пространство наблюдений агента.

**Шаги:**

1. Прочитай `Assets/Scripts/CarControllerAgent.cs` — `CollectObservations`, `GetObservationVector`
2. Прочитай `Assets/Scripts/CarSplineStats.cs` — источник большинства метрик
3. Прочитай `Assets/Proto/unity_racing.proto` — `ResetResponse` / `StepResponse`
4. Сформулируй изменение: что добавляется/убирается, новый размер вектора
5. **Покажи пользователю план и жди подтверждения** — изменение размера вектора ломает обученные модели
6. После подтверждения: обнови `CarSplineStats`, `CarControllerAgent`, `.proto`, сгенерированный код

---

### /reward \<что изменить\>

Измени функцию награды.

**Шаги:**

1. Прочитай `Assets/Scripts/CarControllerAgent.cs` — все вызовы `AddReward` / `SetReward`
2. Составь текущую таблицу наград (событие → значение → строка кода)
3. Опиши предлагаемое изменение и ожидаемый эффект на обучение
4. **Покажи пользователю и жди подтверждения** — изменения наград требуют переобучения
5. После подтверждения — внеси минимальные изменения

---

### /gcm

Сформируй commit message по staged/unstaged файлам.

**Шаги:**

1. `git status -s`
2. `git diff --cached --stat` (или `git diff --stat` если не staged)
3. `git log --oneline -5` — стиль предыдущих коммитов
4. Сформулируй message в формате: `type(scope): description`
5. Типы: `feat`, `fix`, `refactor`, `chore`, `docs`
6. Покажи пользователю. **НЕ делай git add и git commit — только покажи message**

---

## Git — правила

- **НИКОГДА не делай `git add`, `git commit`, `git push`** — пользователь коммитит сам
- **Не делай деструктивные операции** без подтверждения:
  - `git reset --hard`
  - `git checkout -- <file>` / `git restore`
  - `git clean -f`
  - `git push --force`
  - `git branch -D`
- Если нужна деструктивная операция — опиши что и зачем, жди подтверждения

### Формат commit messages

```
feat(grpc): add Step method with truncated flag
fix(agent): correct reward accumulation on wrong checkpoint
refactor(spline): extract curvature calculation to CarSplineStats
chore(proto): regenerate C# code after proto update
docs(readme): add gRPC client usage example
```

---

## Если что-то идёт не так

| Проблема | Что делать |
|----------|-----------|
| Не компилируется в Unity | Проверь все `using` директивы, namespace, типы из protobuf |
| gRPC-вызов вешает Unity | Убедись, что все обращения к Unity API идут через `UnityMainThreadDispatcher` |
| Сгенерированный код не совпадает с `.proto` | Не редактируй `UnityRacing.cs` / `UnityRacingGrpc.cs` вручную, синхронизируй по `.proto` |
| Не знаешь ML-Agents API | Читай `Assets/Scripts/CarControllerAgent.cs` как референс, не угадывай |
| Размер вектора наблюдений изменился | Обязательно обнови и `.proto`, и `CarControllerAgent`, и `CarSplineStats` вместе |
| Не знаешь как устроена часть кода | Сначала прочитай файл, потом пиши. Никогда не угадывай содержимое |
| Нужно проверить gRPC вручную | Используй `grpcurl` или Python-клиент с `grpcio`: `localhost:50051` |
