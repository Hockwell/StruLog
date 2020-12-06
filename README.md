# StruLog
- Structural logging to console, file, db with multi-threading and output queues support. Это простой логгер с важными функциями.
- Пишите, если найдёте баги и узкие места. 
# Особенности
- Необходимые функции для современного логгера имеются
- Логгеры привязаны к классам и доступ к ним возможен из разных потоков
- Проект работает в production-е с октября 2020.

### Отличия от готовых логгеров

- Код открытый - меняйте на своё усмотрение (подробная документация и продуманная архитектура помогут в этом)

### Очереди на вывод и многопоточность

- Каждое хранилище логов работает со своим потоком: консоль, файл и Mongo.
- Каждый такой поток имеет очередь опред. размера (его можно менять), которую он обрабатывает. Логирующий поток кладёт в эти очереди логи.
- При сбое какого-либо хранилища (#потеря доступа к БД) очередь будет накапливать несохранённые логи и ожидать доступа, при заполнении очереди на опред. % будет сообщаться в заданное хранилище логов информация об этом, а если очередь уже заполнена полностью - будет показано время, как долго это длится.
- Логгер сам себя логирует и обо всех проблемах сообщает

### Конфиг

В коде только специфические настройки. Важные и зависящие от проекта настройки содержатся в json-конфиге.
