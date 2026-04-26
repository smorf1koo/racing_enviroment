# Diploma2025 — RL-автогонка (gRPC)

Проект Unity (2022.3.51f1) с автогонками и внешним RL-обучением через gRPC. Агент управляется Python-клиентом, который получает наблюдения и отправляет действия по протоколу gRPC.

## Быстрый старт

### Требования
- Unity **2022.3.51f1**
- NuGetForUnity (подключён в `Packages/manifest.json`)

### Первый запуск после клонирования

1. Откройте проект в Unity **2022.3.51f1**.
2. Unity автоматически установит пакеты из `manifest.json` (включая NuGetForUnity).
3. После загрузки откройте **NuGet → Restore Packages** (или дождитесь автоматического восстановления).
   NuGetForUnity установит `Google.Protobuf 3.21.12` и `Grpc.Core 2.46.6` по `Assets/packages.config`.
4. Откройте сцену `Assets/Scenes/Env_Alone.unity`.
5. Нажмите **Play**.

> Если пакеты NuGet не восстановились автоматически: **NuGet → Manage NuGet Packages → Restore**.

### Запуск с gRPC (внешнее RL-обучение)

1. Запустите сцену в Unity (Play).
2. gRPC-сервер стартует на порту **50051** (`UnityRacingGrpcServer`).
3. Подключитесь Python-клиентом и используйте RPC `Reset` / `Step`.

## Архитектура

```
Python RL-клиент  ←— gRPC (порт 50051) —→  Unity (CarControllerAgent)
     │                                           │
     │  StepRequest(action[2])                   │  ApplyAction(fwd, turn)
     │  StepResponse(obs[7], reward, done)       │  GetObservationVector()
     │  ResetRequest(seed)                       │  OnEpisodeBegin()
     │  ResetResponse(obs[7])                    │
```

### Протокол (unity_racing.proto)

```protobuf
service UnityRacingService {
  rpc Reset(ResetRequest) returns (ResetResponse);
  rpc Step(StepRequest) returns (StepResponse);
}
```

- **Действия**: 2 continuous float — `[forward, turn]`, каждый в `[-1, 1]`
- **Наблюдения**: 7 float — расстояние до сплайна, прогресс, угол к сплайну, кривизна, расстояние до чекпоинта, dot направлений, скорость

## Управление

- **W/S** — газ/тормоз
- **A/D** — поворот
- **Esc** — пауза
- **E** — переключение камер

> Ручное управление работает при `isPlayer = true` в `CarControllerAgent`. При `isPlayer = false` агент управляется через `AIController` (автопилот по сплайну).

## Награды и штрафы

| Событие | Награда |
|---------|---------|
| Правильный чекпоинт | +10 + checksOver/5 |
| Неправильный чекпоинт | -2 |
| Близко к сплайну (< 2м) | +0.005/кадр |
| Скорость >= 20 км/ч + вперёд | +0.005/кадр |
| Движение вперёд (action) | +0.01/шаг |
| Штраф за шаг | -0.001/шаг |
| Медленное движение (< 0.5 м/с) | -0.001/кадр |
| Столкновение со стеной | -10 |
| Столкновение с игроком | -10 |
| Контакт со стеной (continuous) | -0.1/кадр |
| Таймер истёк (30с) | -20, конец эпизода |
| Все чекпоинты пройдены | +5000, конец эпизода |
| Превышен maxSteps (3000) | -100, конец эпизода |

## Сцены

- `Assets/Scenes/Env_Alone.unity` — одиночная трасса
- `Assets/Scenes/Env_Duo.unity` — несколько агентов
- `Assets/Scenes/Menu.unity` — главное меню

## Зависимости

### Unity-пакеты (Packages/manifest.json)
- `com.github-glitchenzo.nugetforunity` — менеджер NuGet-пакетов
- `com.unity.textmeshpro`, `com.unity.ugui`, `com.unity.visualscripting`

### NuGet-пакеты (Assets/packages.config)
- `Google.Protobuf 3.21.12`
- `Grpc.Core 2.46.6`

## Структура проекта

```
Assets/
├── Scripts/              # Логика игры и gRPC-сервер
│   ├── CarControllerAgent.cs     # Агент: награды, наблюдения, эпизоды
│   ├── UnityRacingGrpcServer.cs  # gRPC-сервер (Reset/Step)
│   ├── CarController.cs          # Физика автомобиля
│   ├── AIController.cs           # Автопилот по сплайну
│   ├── TrackCheckpoints.cs       # Система чекпоинтов
│   ├── CarSplineStats.cs         # Метрики относительно сплайна
│   ├── SplineCalculator.cs       # Генерация сплайна
│   └── UnityRacing/              # Сгенерированный protobuf/gRPC код
├── Proto/                # .proto определения
├── Scenes/               # Сцены Unity
├── Prefabs/              # Префабы машин и окружения
└── Plugins/              # DLL (устанавливаются NuGet)
Packages/                 # Unity-пакеты
_proto_gen/               # .NET проект для генерации protobuf-кода
```
