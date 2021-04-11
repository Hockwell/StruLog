# StruLog
- Has necessary functions
- if you satisfied default capabilities, use it through nuget.
- Using in Production began from 10/2020.
- .Net Standard
- More in /docs
- Leave feedback

# Start guide
1. Edit json-config file (example in docs/).
2. `StruLog.StruLogProvider.Init(configPath)`
3. (static readonly field on each logged class) `logger = LoggersFactory.GetLogger<ClassName>()` or `logger = LoggersFactory.GetLogger(typeof(ClassName))`[for static ClassName]

## Telegram tuning (alerts only, not logs storage)
1. Create your telegramBot with name “MyCompanyAlerts” for example. You will receive BOT_TOKEN, print it to config (stores/telegram/token).
2. if telegram/chats[] is null or empty TelegramStore will wait any input from different Telegram users (accounts) and write their chatId to config. After this procedure StruLog will can send logs for you.
3. You can connect >1 projects and >1 users to your bot.
4. If you want connect new consumers (add to existing consumers) to TelegramBot, call .Init() with options.AddTelegramConsumers = true.
5. Attention! Intensivity control (see config) required, because telegram handles messages too slow (1 post per 3 sec in mean, and it's max universal speed). Logger handler set speed by 'sendingPeriod' field (=3000ms by default), but queue may overflow anyway. You must tighten your intensivity control if >1 projects sending to bot (for example, for 2 projects on 1 bot you can set 'sendingPeriod'=6000ms in theory).
6. Intensivity control ignores logs (removes from queue without handling) after limit was reached, so you will receive fresh logs when the limitation be dropped.

# Basic details
- 1 logger for 1 class.
- You can call out Log() functions from >1 threads.

### Output queue for each logs store
- Work with each logs store based on demon-thread.
- Each demon gets logEntry from himself queue and handles it. 
- Log() call adds logInfo to stores queues. 
- Queues conception help amortizate store access problems (#Mongo connection fail) and unconstant logging perfomance.
- When occupied queue capacity will be too big, logger notifies about it.

### HighLoad tests
You can made your tests, clone rep, call imitation methods and run StruLogDemo with different stores on long period. If logger can't provides required performance, occupied queue capacity will be constantly increase. You can print queues occupation to console for example.
