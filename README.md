# LightInvestTest

Тестовое задание на C# / ASP.NET Core с использованием SignalR.

Проект реализует упрощённую систему управления торговыми ордерами:

* создание ордеров через REST API;
* хранение активных ордеров в памяти приложения;
* отправку обновлений через SignalR;
* отправку начального списка активных ордеров при подключении клиента;
* автоматическую отмену ордеров через заданное время;
* потокобезопасное хранение ордеров с использованием `ConcurrentDictionary`.

## Технологии

* C#
* .NET / ASP.NET Core
* ASP.NET Core Web API
* SignalR
* `BackgroundService`
* `ConcurrentDictionary`
* Postman для проверки REST API
* Rider для разработки и запуска приложения

## Структура

Основные компоненты проекта:

* `OrdersController` — REST API для создания ордеров.
* `OrderService` — создание, хранение и отмена ордеров.
* `OrdersHub` — SignalR Hub для работы с клиентскими подключениями.
* `OrderExpirationBackgroundService` — фоновый сервис, автоматически отменяющий старые ордера.
* `OrderRequest` — DTO для создания ордера.
* `Order` — модель ордера.
* `IOrderClient` — контракт методов, доступных SignalR-клиенту.

## Запуск

Открыть проект в Rider и запустить ASP.NET Core приложение.

После запуска API доступен по адресу:

```text
http://localhost:5111
```

Порт может отличаться в зависимости от настроек запуска приложения.

## REST API

### Создание ордера

**POST**

```text
/api/orders
```

Пример запроса:

```json
{
  "symbol": "AAPL",
  "price": 195.50,
  "volume": 10,
  "userId": "user1"
}
```

Пример через Postman:

```text
POST http://localhost:5111/api/orders
Content-Type: application/json
```

Тело запроса:

```json
{
  "symbol": "AAPL",
  "price": 195.50,
  "volume": 10,
  "userId": "user1"
}
```

При успешном создании API возвращает созданный ордер.

Ордер создаётся с:

```text
IsActive = true
CreatedAt = UTC time
```

После создания всем SignalR-подключениям соответствующего пользователя отправляется событие:

```text
ReceiveOrderUpdate
```

## Проверка валидации

При создании ордера выполняется проверка входных данных.

Например, следующий запрос должен вернуть `400 Bad Request`:

```json
{
  "symbol": "",
  "price": -10,
  "volume": 0,
  "userId": ""
}
```

Проверяются:

* наличие `UserId`;
* наличие `Symbol`;
* `Price > 0`;
* `Volume > 0`.

## SignalR

SignalR Hub доступен по адресу:

```text
/hub/orders
```

Полный адрес:

```text
http://localhost:5111/hub/orders
```

Для тестового сценария идентификатор пользователя передаётся через query string:

```text
/hub/orders?userId=user1
```

После подключения клиент добавляется в SignalR-группу соответствующего пользователя.

Например:

```text
/hub/orders?userId=user1
```

подключает клиента к группе:

```text
user1
```

### Начальные ордера

При подключении клиент получает список всех активных ордеров пользователя через:

```text
ReceiveInitialOrders
```

Например:

```json
[
  {
    "id": "d5727e9d-059a-4d71-bf90-536319f2ab50",
    "symbol": "AAPL",
    "price": 195.50,
    "volume": 10,
    "userId": "user1",
    "createdAt": "...",
    "isActive": true
  }
]
```

### Обновление ордера

После создания или автоматической отмены ордера клиент получает:

```text
ReceiveOrderUpdate
```

с актуальным состоянием ордера.

## Автоматическая отмена

Фоновый сервис `OrderExpirationBackgroundService` проверяет активные ордера каждую секунду.

Текущий срок жизни ордера:

```text
30 секунд
```

После истечения срока:

```text
IsActive = false
```

После этого пользователю отправляется:

```text
ReceiveOrderUpdate
```

с обновлённым ордером.

### Проверка

1. Подключить SignalR-клиента как `user1`.
2. Создать ордер через `POST /api/orders`.
3. Убедиться, что получено событие `ReceiveOrderUpdate` с `isActive: true`.
4. Подождать около 30 секунд.
5. Убедиться, что получено повторное событие `ReceiveOrderUpdate` с `isActive: false`.

## Пример сценария проверки

### Шаг 1. Подключение SignalR

Подключиться к:

```text
ws://localhost:5111/hub/orders?userId=user1
```

После установления соединения клиент должен получить:

```text
ReceiveInitialOrders
```

с текущими активными ордерами пользователя.

### Шаг 2. Создание ордера

Отправить через Postman:

```http
POST http://localhost:5111/api/orders
```

```json
{
  "symbol": "AAPL",
  "price": 195.50,
  "volume": 10,
  "userId": "user1"
}
```

Ожидаемый результат:

```text
HTTP 200 OK
```

и получение SignalR-события:

```text
ReceiveOrderUpdate
```

с:

```text
isActive: true
```

### Шаг 3. Автоматическая отмена

Через примерно 30 секунд фоновый сервис обнаружит истёкший ордер.

Ожидаемый результат:

```text
ReceiveOrderUpdate
```

с:

```text
isActive: false
```

## Хранение данных

Для хранения ордеров используется:

```csharp
ConcurrentDictionary<Guid, Order>
```

Это позволяет безопасно работать с коллекцией из нескольких потоков.

Данные хранятся только в памяти приложения и будут потеряны после перезапуска приложения.

Для production-системы вместо in-memory storage потребовалось бы использовать постоянное хранилище, например базу данных или Redis.

## Примечание по идентификации пользователя

В рамках тестового задания `userId` передаётся через query string SignalR-подключения.

В production-приложении идентификатор пользователя следует получать из аутентифицированного пользователя (`Claims` / `Context.UserIdentifier`), а не доверять значению из query string.

## Проверка

REST API проверялся с помощью **Postman**.

Приложение запускалось и отлаживалось в **JetBrains Rider**.

SignalR-соединение проверялось отдельно с помощью WebSocket/SignalR-клиента.
