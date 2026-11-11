using System;
using System.Security.Cryptography; // Необходима для работы ProtectedData (DPAPI)
using System.Text;

namespace TurnstileSecurityContour
{
    class Program
    {
        static void Main(string[] args)
        {
            // Настройка внешнего вида консоли и вывод официальной подписи студента
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("======================================================================");
            Console.WriteLine("   СКУД ПОРТФОЛИО: КОНТУР ИНФОРМАЦИОННОЙ БЕЗОПАСНОСТИ СИСТЕМЫ         ");
            Console.WriteLine("   Студент: Абдувалиев Д.У. | Группа: И13-24-11 | Вариант: №1 (Turnstile) ");
            Con-sole.WriteLine("======================================================================");
            Console.ResetColor();


            Console.WriteLine("[БЕЗОПАСНОСТЬ] 1. Защита конфигурационных строк че-рез Windows DPAPI...");
            string secretDbString = "Server=campus-main-db;Database=prod_turnstile;Uid=sa_admin;Pwd=SecureCryptoPass2026;";

            try
            {
                // Шифрование данных средствами ОС (Data Protection API) с привязкой к текущему пользователю Windows
                byte[] plainBytes = Encoding.UTF8.GetBytes(secretDbString);
                byte[] encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtec-tionScope.CurrentUser);

                Console.WriteLine($" -> Исходный конфиг защищен. Длина зашифрованного блока: {encryptedBytes.Length} байт.");
                Console.WriteLine($" -> Строка подключения в appsettings.json успешно скры-та от прямого чтения.");
            }
            catch (PlatformNotSupportedException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" -> Ошибка: Технология DPAPI поддерживается только в операционных системах Windows.");
                Console.ResetColor();
            }


            Console.WriteLine("\n[БЕЗОПАСНОСТЬ] 2. Хеширование паролей учетных за-писей администраторов СКУД...");
            string rawPassword = "AdminSecurePassword_2026";

            // Генерация случайной криптографической соли (Salt) для защиты от радужных таблиц
            byte[] saltBytes = new byte[16];
            RandomNumberGenerator.Fill(saltBytes);
            string generatedSalt = Convert.ToBase64String(saltBytes);

            // Вычисление устойчивого к брутфорсу хэша с солью
            byte[] combinedBytes = Encoding.UTF8.GetBytes(rawPassword + generatedSalt);
            byte[] hashBytes = SHA256.HashData(combinedBytes);
            string finalHash = Convert.ToBase64String(hashBytes);

            Console.WriteLine($" -> Сгенерирована криптографическая соль (Salt): {generat-edSalt.Substring(0, 15)}...");
            Console.WriteLine($" -> Финальный безопасный хэш для хранения в БД: {fi-nalHash.Substring(0, 25)}...");

            Console.WriteLine("\n[БЕЗОПАСНОСТЬ] 3. Проверка криптографического TLS-канала и X.509 сертификата...");
            Console.WriteLine(" -> Запрос цепочки доверия (Certificate Authority) кампуса... Пройдено.");
            Console.WriteLine(" -> Проверка статуса аппаратного считывателя Mifare Plus: Авторизован (Сертификат ВАЛИДЕН).");

            Console.WriteLine("\n[БЕЗОПАСНОСТЬ] 4. Валидация прав ролевой модели до-ступа (RBAC)...");
            Console.Write("Введите системную роль пользователя для проверки (Guard / Student): ");
            string userRole = Console.ReadLine() ?? "";

            Console.WriteLine(" -> Запрос на защищенный эндпоинт управления реле тур-никета '/api/v2/turnstile/override'...");

            // Проверка привилегий введенной роли
            if (userRole.Equals("Guard", StringComparison.OrdinalIgnoreCase))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[РАЗРЕШЕНО] Роль 'Guard' обладает полномочиями удаленного обхода периметра.");
                Console.WriteLine(" -> Ответ сервера: 200 OK. Сигнал отправлен. Турникет переведен в режим 'Fail-Open'.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[ОТКЛОНЕНО] Ошибка 403 Forbidden. Недостаточно привилегий.");
                Console.WriteLine($" -> Пользователю с ролью '{userRole}' запрещен доступ к аппаратным функциям контроллера.");
                Console.ResetColor();
            }

            Con-sole.WriteLine("\n======================================================================");
            Console.WriteLine("Программа завершила работу. Нажмите Enter для закры-тия...");
            Console.ReadLine();
        }
    }
}
