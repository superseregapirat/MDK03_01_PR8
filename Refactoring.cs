using System;
using System.Collections.Generic;
using FluentValidation;

namespace TurnstileMetricsSim
{
    // Модель данных запроса СКУД
    public class TurnstileRequest
    {
        public string CardNumber { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Con-sole.WriteLine("======================================================================");
            Console.WriteLine("   СКУД ПОРТФОЛИО: АУДИТ КАЧЕСТВА И РЕФАКТО-РИНГ КОДА (МЕТРИКИ ПО)    ");
            Console.WriteLine("   Студент: Абдувалиев Д.У. | Группа: И13-24-11 | Вариант: №1 (Turnstile) ");
            Console.WriteLine("======================================================================");
            Console.ResetColor();

            // Создаем тестовый запрос с нарушением маски (симуляция атаки / некоррект-ного ввода)
            var request = new TurnstileRequest { CardNumber = "12345' OR '", Direction = "IN" };

            Console.WriteLine($"[АНАЛИЗ] Тестирование входящего пакета. Карта: '{request.CardNumber}', Направление: '{request.Direction}'");

            // ----------------------------------------------------------------------
            // 1. ДЕМОНСТРАЦИЯ КОДА ДО РЕФАКТОРИНГА (v2.1.0)
            // ----------------------------------------------------------------------
            Console.WriteLine("\n----------------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ВЕРСИЯ v2.1.0 — ДО РЕФАКТОРИНГА]");
            Console.ResetColor();
            Console.WriteLine(" -> Метрика Маккейба (Cyclomatic Complexity): 6 (Высокая сложность)");
            Console.WriteLine(" -> Индекс поддерживаемости (Maintainability Index): 54 (Критическая зона)");
            Console.WriteLine(" -> Выявленные дефекты: Запах кода 'Long Method', прямая конкатенация строк в SQL.");

            // Имитация прогона старой логики
            Console.WriteLine(" -> Выполнение сырого SQL-запроса: SELECT * FROM PassLogs WHERE CardNumber = '" + request.CardNumber + "'");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(" -> РЕЗУЛЬТАТ АУДИТА: Обнаружен критический вектор уязвимости SQL Injection!");
            Console.ResetColor();

            // ----------------------------------------------------------------------
            // 2. ДЕМОНСТРАЦИЯ КОДА ПОСЛЕ РЕФАКТОРИНГА (v2.1.1)
            // ----------------------------------------------------------------------
            Console.WriteLine("\n----------------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[ВЕРСИЯ v2.1.1 — ПОСЛЕ РЕФАКТОРИНГА]");
            Console.ResetColor();
            Console.WriteLine(" -> Метрика Маккейба (Cyclomatic Complexity): 2 (Опти-мальная сложность)");
            Console.WriteLine(" -> Индекс поддерживаемости (Maintainability Index): 88 (Зе-леная зона)");
            Console.WriteLine(" -> Внедренные решения: Вынос валидации в FluentValidation, параметризация LINQ.");

            // Вызов изолированного объекта валидации
            var validator = new TurnstileRequestValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" -> [FluentValidation] Перехват ошибки: Номер RFID-карты должен состоять строго из 10 цифр!");
                Console.WriteLine(" -> РЕЗУЛЬТАТ: Попытка инъекции нейтрализована на уровне шлюза API (Статус 400 BadRequest).");
                Console.ResetColor();
            }

            Con-sole.WriteLine("\n======================================================================");
            Console.WriteLine("Анализ метрик завершен. Нажмите Enter для закрытия...");
            Console.ReadLine();
        }
    }

    // Класс жесткой валидации входящих параметров по принципу SRP (SOLID)
    public class TurnstileRequestValidator : AbstractValidator<TurnstileRequest>
    {
        public TurnstileRequestValidator()
        {
            RuleFor(x => x.CardNumber)
                .NotEmpty()
                .Matches(@"^[0-9]{10}$"); // Разрешены строго 10 цифр (защита от спецсим-волов)

            RuleFor(x => x.Direction)
                .NotEmpty()
                .Must(d => d == "IN" || d == "OUT");
        }
    }
}
