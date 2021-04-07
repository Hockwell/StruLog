# StruLog
- Structural logging to console, file, MongoDB, Telegram with multi-threading and output queues support.
- Has necessary functions
- if you satisfied default capabilities, use it through nuget.
- Production start: 10/2020.
- .Net Standard
- More in /docs
- Leave feedback

# Start guide
1. Edit config file (example in /docs).
2. `StruLog.StruLogProvider.Init(configPath)`
3. (static readonly field on each logged class) `logger = LoggersFactory.GetLogger<ClassName>()` or `logger = LoggersFactory.GetLogger(typeof(ClassName))`[for static ClassName]

## Telegram tuning
1. Create your telegramBot with name “MyCompanyAllerts” for example. You will receive BOT_TOKEN, print it to config (stores/telegram/token).
2. if telegram/chats[] is null or empty TelegramStore will wait any input from different users and write their chatId to config. After this procedure StruLog will can send logs for you.
3. You can connect >1 projects and >1 users to your bot.
4. If you want add new consumers, call .Init() with options.AddTelegramConsumers = true.

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
