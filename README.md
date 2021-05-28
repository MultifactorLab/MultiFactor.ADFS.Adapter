[![Build status](https://ci.appveyor.com/api/projects/status/2q0mv1isiddp18ay?svg=true)](https://ci.appveyor.com/project/MultifactorLab/multifactor-adfs-adapter)

# MultiFactor.ADFS.Adapter

Двухфакторная аутентификация для ADFS.

## Установка

1. Скачайте и распакуйте архив на сервер с ADFS.
2. Отредактируйте файл конфигурации MultiFactor.ADFS.Adapter.dll.config: пропишите ключи доступа из личного кабинета Мультифактора
3. Запустите PowerShell скрипт install.ps1 с правами администратора.
4. Зайдите в консоль управления ADFS, в разделе "Authentication methods" -> "Multi-factor Authentication Methods" включите метод MultiFactor.

## Удаление

1. Отключите в ADFS метод MultiFactor.
2. Запустите PowerShell скрипт uninstall.ps1 с правами администратора.
