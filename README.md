# StruLog

- It has the necessary functions
- if you are satisfied with the default capabilities, use it via nuget.
- Using in Production began from 10/2020.
- .Net Standard
- More in /docs
- Give feedback

# Start guide

1. Edit json-config file (example in docs/).
2. `StruLog.StruLogProvider.Init(configPath, inProjectDir)`. inProjectDir=false (default value) if configPath is full, true if config file placed in runtime directory - then configPath is name, not full path.
3. Each logger-object binds to monitored class `logger = LoggersFactory.GetLogger<ClassName>()` or `logger = LoggersFactory.GetLogger(typeof(ClassName))`[for static ClassName class]. You can define logger object as static readonly field for example.
4. Logger-object types:

   - StruLog.Logger
   - ILogger (Microsoft.Extensions.Logging).

## Telegram tuning (alerts only, not logs storage)

1. Create your telegramBot with name “MyCompanyAlerts” for example. You will receive BOT_TOKEN, enter this to config (stores/telegram/token). Run the project.
2. if telegram/chats[] is null or empty, StruLog will wait any input from different Telegram users and write their chatId to config. Read the console for correct actions and finish procedure. After this StruLog will can to send logs to telegram chats.
3. You can connect >1 projects and >1 users to your bot.
4. If you want connect new consumers (add to existing consumers) to TelegramBot, call `.Init()`(item 2) with `options.AddTelegramConsumers = true`.
5. Attention! Speed control (config/telegram/sendingPeriod) required, because telegram handles messages too slow (1 post per 1 sec on 1 user and it's max mean speed). Logger handler set speed by 'sendingPeriod' field (=1000ms by default), but queue may overflow anyway. You must tighten Speed control and increase 'sendingPeriod' if >1 projects sending to bot (for example, for 2 projects on 1 bot you can set 'sendingPeriod'=2000ms in theory). If messages stop coming: stop execute and increase 'sendingPeriod' (likely required waiting that telegram server will drop temporary limitation on sending).
6. Intensity control (config/telegram/intensivity) ignores logs (removes from queue without handling) after limit was reached, so you will receive fresh logs when the limitation be dropped.

## Config content

#### time

- `UTC`
- `LOCAL`

#### projectName

Field with name of current project. Deliberately didn’t make automatic definition of this name that users can select more comfortable name.
You can use {projectName} selector in mongo/collectionName and file/path.

#### outputPattern (selectors)

- `msg`
- `excMsg`
- `excClassLine` – class and row where exception throwed (0 and 1 stack frames)
- `excStackTrace`
- `innerExc-i` – logger returns messages and stacktraces from i-th number included exceptions (inner exception objects); the value 'i' affects the logging speed, i:1,99
- `time`
- `logLevel`
- `obj`
- `loggerName` – monitored class full name (with namespace)
- `loggerName-i` – (doesn't work with Mongo) allows cut over fullname(`MyCode.ChuckMustFly.FlyEngine`) and returns the i-th segment from the right (i=1 => `FlyEngine`, i=2 => `ChuckMustFly.FlyEngine`), i:1,9.

You can add any char to the output pattern.

Example for file: `{time} | {logLevel} | {loggerName-2} | {msg} {obj} {excMsg} {excStackTrace} {innerExc-5}`

#### file/path

Selectors:

- `y` – year,
- `m` – month,
- `d` – day,
- `project` – runtime-directory of project.

Example: `{project}/../../Logs/{y}/{m}/{projectName}-{d}.log`

#### insideLoggingStore

Store (=storage) which using for StruLog events output.

## ILogger support

1. Log()-argument `eventId` converts to object with `eventId` and `eventName`
2. Not tested in ASP.NET.
3. Log()-argument `args` don't work (it's useless, you can use `$"la {arg1} na-na {arg2}"`).
4. `BeginScope(...)`, `IsEnabled(...)` are not implemented.
5. Logging ways:
   - you can write extension methods for ILogger - as in the type 'Logger', use information about ILogger-native methods and rewrites their when the logger lib changes.
   - ILogger-native methods:
     - only text: `LogDebug(...)`, `Log(...)` etc
     - with custom object, without text: `Log<TState>(...)`, where `formatter`-argument is null
     - with text based on custom object, but without object publishing: `Log<TState>(...)`, where `formatter`-argument is not null.

## Log entries post-processing

- LogDataModel is a public type. Use it to fetch log entries from Mongo.

# How it work

- 1 logger for 1 class.
- You can call out each Log() from >1 threads.

### Output queue for each logs storage

- Work with each logs storage based on background thread.
- Each such thread gets logEntry from himself queue and handles it.
- Log() call adds logInfo to queue of each enabled storage.
- Queues conception helps to amortize problems with access to storage (ex: Mongo connection fail) and non-constant logging intensity.
- When occupied queue capacity will be too big, logger notifies about it.

### Your tests

You can made your tests, clone repo, set Release config, call imitation methods and run StruLogDemo with different storages on long period. If logger can't provides required performance, queue occupied capacity will be constantly increase and you will see alert from StruLog in logs.
