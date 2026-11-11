using System;
using System.Security.Cryptography;
using System.Text;

namespace TurnstileAutoUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            // Очистка консоли и вывод официальной подписи разработчика
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Con-sole.WriteLine("======================================================================");
            Console.WriteLine("   СКУД ПОРТФОЛИО: МОДУЛЬ АВТОМАТИЧЕСКОГО ОБНОВЛЕНИЯ (AUTO-UPDATER)   ");
            Console.WriteLine("   Студент: Абдувалиев Д.У. | Группа: И13-24-11 | Вариант: №1 (Turnstile) ");
            Con-sole.WriteLine("======================================================================");
            Console.ResetColor();

            // 1. Инициализация семантических версий (SemVer: Major.Minor.Patch)
            string currentVersion = "2.1.0"; // Текущая версия на турникете кампуса
            string serverVersion = "2.1.1"; // Доступная версия на сервере дистрибуции (.NET 8 Patch)

            Console.WriteLine($"[ПРОВЕРКА] Чтение локального манифеста... Текущая версия СКУД: v{currentVersion}");
            Console.WriteLine($"[ЗАПРОС] Проверка обновлений на сервере... Доступная версия: v{serverVersion}");

            // Сравнение версий по правилам семантического версионирования
            if (IsUpdateAvailable(currentVersion, serverVersion))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n[УВЕДОМЛЕНИЕ] Обнаружена более новая сборка v{serverVersion}. Инициализация загрузки...");
                Console.ResetColor();

                // Имитируем скачивание бинарного пакета обновления (сборки Turnstile API)
                string downloadedPackageData = "Cam-pus_Smart_Systems_TurnstileAPI_Core_Patch_v2.1.1_Compiled_Data";

                // Эталонный хэш SHA-256, который мы «получили» из безопасного манифе-ста сервера
                string expectedServerHash = "8bc4928d3bd6b68e989be50be57cf062bb023fa8d9e6005c93c4e3412cb59bf4";

                Console.WriteLine(" -> Скачивание пакета обновления .NET 8 завершено.");
                Console.WriteLine(" -> Запуск криптографической проверки целостности дан-ных...");

                // 2. Вычисление хэш-суммы SHA-256 для скачанного файла
                string calculatedHash = CalculateSHA256(downloadedPackageData);

                Console.WriteLine($" -> Ожидаемый хэш (Манифест): {expectedServerHash}");
                Console.WriteLine($" -> Фактический хэш (Файл):   {calculatedHash}");

                // 3. Верификация хэшей (Защита от повреждения или подмены кода)
                if (calculatedHash.Equals(expectedServerHash, StringCompari-son.OrdinalIgnoreCase))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n[УСПЕХ] Контрольные суммы полностью СОВПА-ДАЮТ.");
                    Console.WriteLine($"[СТАТУС] Пакет безопасен. Ядро СКУД успешно об-новлено до версии v{serverVersion}!");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[КРИТИЧЕСКАЯ ОШИБКА] Контрольные суммы НЕ СОВПАДАЮТ!");
                    Console.WriteLine("[БЛОКИРОВКА] Пакет поврежден или подменен зло-умышленником. Обновление отменено.");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[СТАТУС] Установлена актуальная версия ПО СКУД. Обновление не требуется.");
                Console.ResetColor();
            }
            Console.WriteLine("\n======================================================================");
            Console.WriteLine("Программа завершила работу. Нажмите Enter для закры-тия...");
            Console.ReadLine();
        }

        static bool IsUpdateAvailable(string current, string server)
        {
            Version currentVer = new Version(current);
            Version serverVer = new Version(server);
            return serverVer > currentVer;
        }

        static string CalculateSHA256(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = SHA256.HashData(inputBytes);

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2")); // Перевод байтов в шестнадцатеричную строку
            }
            return sb.ToString();
        }
    }
}
