using System;
using System.IO;
using System.Threading;
using Microsoft.Data.Sqlite;

namespace TurnstileOfflineBuffer
{
    class Program
    {
        // Имя локального файла базы данных на физическом контроллере турникета
        private static readonly string DbFilename = "local_turnstile_buffer.db";

        static void Main(string[] args)
        {
            // Настройка внешнего вида консоли и вывод подписи студента
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Con-sole.WriteLine("======================================================================");
            Console.WriteLine("   СКУД ПОРТФОЛИО: МОДУЛЬ АВТОНОМНОЙ БУФЕ-РИЗАЦИИ ТРАНЗАКЦИЙ (SQLITE)  ");
            Console.WriteLine("   Студент: Абдувалиев Д.У. | Группа: И13-24-11 | Вариант: №1 (Turnstile) ");
            Con-sole.WriteLine("======================================================================");
            Console.ResetColor();

            // 1. ИНИЦИАЛИЗАЦИЯ ЛОКАЛЬНОГО ХРАНИЛИЩА (Аналог EF Core Mi-grations в SQLite)
            InitializeLocalDatabase();

            // Эмуляция считывания RFID-метки студента на проходной
            string mockCardUid = "04A1B2C3D4E5F6";
            string mockDirection = "IN";

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[АВАРИЯ] Центральный сервер СКУД не отвечает (Код ошибки: Network 503).");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[АВТОНОМИЯ] Запуск локального контура СКУД. Реги-страция карты: {mockCardUid}");
            Console.ResetColor();

            // 2. ФИКСАЦИЯ СОБЫТИЯ В БУФЕР (SQLite Data Write)
            SaveLogToLocalBuffer(mockCardUid, mockDirection);

            // 3. ИМИТАЦИЯ ПОЛИТИКИ ОТКАЗОУСТОЙЧИВОСТИ POLLY (Pol-ly.RetryPolicy)
            Console.WriteLine("\n[POLLY] Ожидание восстановления сети. Инициализация Retry Policy...");
            bool networkRestored = false;

            // Цикл имитирует экспоненциальный откат (Exponential Backoff) повторных попыток Polly
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                Console.Write($" -> Попытка ресинхронизации #{attempt} с центральным об-лаком кампуса... ");
                Thread.Sleep(1000); // Симуляция сетевой задержки

                if (attempt == 3) // На 3 попытке симулируем успешное поднятие сети
                {
                    networkRestored = true;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("УСПЕШНО!");
                    Console.ResetColor();
                    break;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ОТКЛОНЕНО (Таймаут).");
                    Console.ResetColor();
                }
            }

            // 4. СИНХРОНИЗАЦИЯ И ОЧИСТКА БУФЕРА (Паттерн Transaction Flush)
            if (networkRestored)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[СИНХРОНИЗАЦИЯ] Канал связи восстановлен.");
                Console.ResetColor();
                UploadAndClearBuffer();
            }

            Con-sole.WriteLine("\n======================================================================");
            Console.WriteLine("Программа завершила работу. Нажмите Enter для закры-тия...");
            Console.ReadLine();
        }

        /// <summary>
        /// Создание таблицы буферизации в локальной SQLite БД при ее отсутствии
        /// </summary>
        private static void InitializeLocalDatabase()
        {
            using (var connection = new SqliteConnection($"Data Source={DbFilename}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS OfflineBuffer (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CardUid TEXT NOT NULL,
                        Direction TEXT NOT NULL,
                        Timestamp TEXT NOT NULL
                    );";
                command.ExecuteNonQuery();
            }
        }


        private static void SaveLogToLocalBuffer(string cardUid, string direction)
        {
            using (var connection = new SqliteConnection($"Data Source={DbFilename}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO OfflineBuffer (CardUid, Direction, Timestamp) VALUES ($uid, $dir, $time)";
                command.Parameters.AddWithValue("$uid", cardUid);
                command.Parameters.AddWithValue("$dir", direction);
                command.Parameters.AddWithValue("$time", DateTime.UtcNow.ToString("o"));
                command.ExecuteNonQuery();
            }
            Console.WriteLine(" -> Событие успешно записано в локальную БД 'lo-cal_turnstile_buffer.db'.");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" -> РЕЗУЛЬТАТ: Проход разрешен. Данные сохранены ло-кально.");
            Console.ResetColor();
        }


        private static void UploadAndClearBuffer()
        {
            using (var connection = new SqliteConnection($"Data Source={DbFilename}"))
            {
                connection.Open();

                // Подсчет накопленных записей в буфере
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT COUNT(*) FROM OfflineBuffer";
                long count = (long)(selectCmd.ExecuteScalar() ?? 0L);

                Console.WriteLine($" -> Извлечено пакетов из SQLite буфера: {count} шт.");
                Console.WriteLine(" -> Все транзакции успешно реплицированы в централь-ную СУБД PostgreSQL.");

                // Полная очистка локальной таблицы буфера после синхронизации
                var deleteCmd = connection.CreateCommand();
                deleteCmd.CommandText = "DELETE FROM OfflineBuffer";
                deleteCmd.ExecuteNonQuery();

                Console.WriteLine(" -> Локальная таблица буфера SQLite успешно очищена (память освобождена).");
            }
        }
    }
}
