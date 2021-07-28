# StruLog
- Has necessary functions
- if you satisfied default capabilities, use it through nuget.
- Using in Production began from 10/2020.
- .Net Standard
- More in /docs
- Give feedback

# Start guide
1. Edit json-config file (example in docs/).
2. `StruLog.StruLogProvider.Init(configPath, inProjectDir)`. inProjectDir=false (default value) if configPath is full, true if config file placed in runtime directory, then configPath is name, not full path.
3. (static readonly field on each logged class) `logger = LoggersFactory.GetLogger<ClassName>()` or `logger = LoggersFactory.GetLogger(typeof(ClassName))`[for static ClassName]

## Telegram tuning (alerts only, not logs storage)
1. Create your telegramBot with name “MyCompanyAlerts” for example. You will receive BOT_TOKEN, enter this to config (stores/telegram/token). Run the project. 
2. if telegram/chats[] is null or empty, StruLog will wait any input from different Telegram users and write their chatId to config. Read the console for correct actions and finish procedure. After this StruLog will can to send logs to telegram chats.
3. You can connect >1 projects and >1 users to your bot.
4. If you want connect new consumers (add to existing consumers) to TelegramBot, call ```.Init()```(item 2) with ```options.AddTelegramConsumers = true```.
5. Attention! Speed control (config/telegram/sendingPeriod) required, because telegram handles messages too slow (1 post per 1 sec on 1 user and it's max mean speed). Logger handler set speed by 'sendingPeriod' field (=1000ms by default), but queue may overflow anyway. You must tighten Speed control and increase 'sendingPeriod' if >1 projects sending to bot (for example, for 2 projects on 1 bot you can set 'sendingPeriod'=2000ms in theory). If messages stop coming: stop execute and increase 'sendingPeriod' (likely required waiting that telegram server will drop temporary limitation on sending). 
6. Intensity control (config/telegram/intensivity) ignores logs (removes from queue without handling) after limit was reached, so you will receive fresh logs when the limitation be dropped.

## Config content
#### projectName
Field with name of current project. Deliberately didn’t make automatic definition of this name that users can select more comfortable name.
You can use {projectName} selector in mongo/collectionName and file/path.
#### file/path
Selectors:
- y – year,
- m – month,
- d – day,
- project – runtime-directory of project.

Example: "{project}/../../Logs/{y}/{m}/{projectName}-{d}.log"
#### insideLoggingStore
Store which using for StruLog events output.
# Basic details
- 1 logger for 1 class.
- You can call out Log() functions from >1 threads.

### Output queue for each logs store
- Work with each logs store based on demon-thread.
- Each demon gets logEntry from himself queue and handles it. 
- Log() call adds logInfo to stores queues. 
- Queues conception help amortize store access problems (#Mongo connection fail) and non-constant logging intensity.
- When occupied queue capacity will be too big, logger notifies about it.

### HighLoad tests
You can made your tests, clone repo, set Release config, call imitation methods and run StruLogDemo with different stores on long period. If logger can't provides required performance, queue occupied capacity will be constantly increase. You can print queues occupation to console for example.
