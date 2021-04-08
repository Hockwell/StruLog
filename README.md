# StruLog
- Structural logging to console, file, MongoDB, Telegram[preview] with multi-threading and output queues support.
- Has necessary functions
- if you satisfied default capabilities, use it through nuget.
- Using in Production began 10/2020.
- .Net Standard
- More in /docs
- Leave feedback

# Start guide
1. Edit config file (example in /docs).
2. `StruLog.StruLogProvider.Init(configPath)`
3. (static readonly field on each logged class) `logger = LoggersFactory.GetLogger<ClassName>()` or `logger = LoggersFactory.GetLogger(typeof(ClassName))`[for static ClassName]

## Telegram tuning (alerts only, not logs storage)
1. Create your telegramBot with name “MyCompanyAllerts” for example. You will receive BOT_TOKEN, print it to config (stores/telegram/token).
2. if telegram/chats[] is null or empty TelegramStore will wait any input from different users and write their chatId to config. After this procedure StruLog will can send logs for you.
3. You can connect >1 projects and >1 users to your bot.
4. If you want add new consumers, call .Init() with options.AddTelegramConsumers = true.
5. Attention: intensivity control required, because telegram handles messages too slow (1 post per 3 sec in mean, and it's max speed, likely it's too fast for 24h). Logger handler set optimal speed (1 post per 3 sec), but queue may overflow.
6. Intensivity control ignore logs (removes from queue without handling), when set output limit, so you will be receive fresh logs, when the limitation drop.

# Basic details
- 1 logger for 1 class.
- You can call logger functions from >1 threads.

### Output queue for each logs store
- Work with each logs store based on demon-thread.
- Each demon gets logEntry from himself queue and handles it. 
- Log() call add logInfo to stores queues. 
- Queues conception help amortizate store access problems (#Mongo connection fail) and unconstant logging perfomance.
- When queue capacity will be too big, logger notify about it.

### Configuration
- In code: for logic tuning.
- In json: tuning for concrete project.

### HighLoad tests
You can made your tests, clone rep, call neccessary methods and run StruLogDemo with different stores on long period. If logger can't provides required perfomance, you will see warnings about >90% queue capacity and errors about max capacity for some kind of store.