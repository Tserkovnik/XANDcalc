namespace XANDcalc;

enum ReadResult { Number, Back, Exit, Retry, Skip, Auto }

#region Вызываемые методы для удобства
static class ConsoleExtensions
{
    // Ключевое слово "this" позволяет вызывать метод через точку у любой строки
    // ConsoleColor? c = null делает цвет необязательным (по умолчанию обычный)
    public static void Print(this string text, ConsoleColor? c = null, bool line = true)
    {
        if (c != null) Console.ForegroundColor = c.Value;
        if (line) 
            Console.WriteLine(text);
        else 
            Console.Write(text);
        if (c != null) Console.ResetColor();
    }

    public static string FormatCurrent(this double amperes)
    {
        double absValue = Math.Abs(amperes);

        if (amperes >= 1e-1) return $"{amperes:F2} A";

        // Больше или равно 1 мАа
        if (absValue >= 1e-3) 
            return $"{amperes * 1e3:F2} mA";
            
        // Больше или равно 1 мкА
        if (absValue >= 1e-6) 
            return $"{amperes * 1e6:F2} uA";
            
        // Больше или равно 1 нА (нано)
        if (absValue >= 1e-9) 
            return $"{amperes * 1e9:F2} nA";
            
        // Всё что меньше  переводим в пико
        return $"{amperes * 1e12:F2} pA";
    }

     public static void ClearLastLine()
    {
        // курсор сдвиг вверх
        Console.CursorTop--; 
        Console.CursorLeft = 0;
        
        // пробелы вместо текста 
        // и возвращаем курсор в начало этой строки
        Console.Write(new string(' ', Console.BufferWidth-1));
        Console.CursorLeft = 0;
    }

    public static ReadResult ReadNumber(string prompt, out double value)
    {
        value = 0;                                   // на всякий случай
        Console.Write(prompt);                       // печатаем подсказку
        string s = Console.ReadLine()?.Trim().ToLower() ?? "";

        if (s == "0")  return ReadResult.Back;       // код "назад"
        if (s == "x" || s == "q" || s == "ч") return ReadResult.Exit; // код "выход"
        if (s == "") return ReadResult.Skip;
        if (s == "a" || s == "auto" || s == "ф") return ReadResult.Auto; 

        if (double.TryParse(s, out value))           // получилось число?
        {
            if (value > 0) return ReadResult.Number; // да и >0 -> код Number, число в value
            Console.WriteLine("Value must be positive");
            return ReadResult.Retry;                 // число, но плохое -> "переспроси"
        }

        if (s.Length<5) Console.WriteLine("Invalid input, try again"); else Console.WriteLine("You are idiot, try again");          // вообще не число
        return ReadResult.Retry;
    }

}
#endregion