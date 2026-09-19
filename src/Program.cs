namespace System;
using static System.ConsoleColor;
using static Vars; // чтоб писать удобнее, vars БОЛЬШЕ НЕТ
using System.Linq;  // для .Where()
using static ConsoleExtensions;

enum ReadResult { Number, Back, Exit, Retry, Skip, Auto }

#region ЦЕНТРАЛЬНАЯ БАЗА ПАРАМЕТРОВ, КОНСТАНТ И РЕЗУЛЬТАТОВ

// 1. Универсальный класс для любого параметра (входного или выходного)
class Parameter
{
    public string Name { get; }
    public double Value { get; set; }
    public string Unit { get; }
    public string Tag { get; } // "all", "t1", "t2", "out" (для фильтрации в циклах ввода)

    public Parameter(string name, double value, string unit, string tag)
    {
        Name = name;
        Value = value;
        Unit = unit;
        Tag = tag;
    }
}

// 2. Класс-контейнер для одной строки сравнительной таблицы результатов (по Fan-Out)
class CalculationRow
{
    public int CurrentFO { get; set; } // Для какого Fan-Out сделан этот конкретный расчет
    
    // Внутри каждой строки таблицы лежит свой независимый набор выходных параметров
    public Parameter[] OutputParams { get; }

    public CalculationRow(int currentFO, double rb, double voh, double vol, double vih)
    {
        CurrentFO = currentFO;
        OutputParams = [
            new Parameter("Rb", rb, "Ohm", "out"),
            new Parameter("V_oh", voh, "V", "out"),
            new Parameter("V_ol", vol, "V", "out"),
            new Parameter("V_ih", vih, "V", "out")
        ];
    }

    // Удобный поиск значения внутри строки таблицы по имени параметра
    public double GetVal(string name) => 
        Array.Find(OutputParams, p => p.Name == name)?.Value ?? 0;
}

// 3. Единое централизованное хранилище всей программы
static class Vars
{
    // ВХОДНЫЕ ПАРАМЕТРЫ (Вводит пользователь. Значения сохраняются между заходами в меню)
    public static readonly Parameter[] Parameters = [
        new Parameter("Vcc", 5.0, "V", "all"),
        new Parameter("Required Fan-Out", 8, "Natural number", "all"),
        new Parameter("Vin", 5.0, "V", "all"),
        
        // Транзистор 1
        new Parameter("Rc1", 470, "Ohm", "t1"),
        new Parameter("Beta1", 100, "Natural number", "t1"),
        new Parameter("kSat1", 2, "Natural number", "t1"),

        // Транзистор 2
        new Parameter("Rc2", 1000, "Ohm", "t2"),
        new Parameter("Beta2", 150, "Natural number", "t2"),
        new Parameter("kSat2", 1.5, "Natural number", "t2")
    ];

    // ХРАНИЛИЩЕ ДЛЯ ТАБЛИЦЫ РЕЗУЛЬТАТОВ (Сюда складываем строки расчетов для разных FO)
    public static System.Collections.Generic.List<CalculationRow> ResultsTable = new();
    public static bool VinAuto = true;

    #region ПСЕВДОНИМЫ ДЛЯ ПОЛНЫХ ОБЪЕКТОВ (чтобы удобно брать .Name и .Unit при выводе)
    public static Parameter P_Vcc   => Parameters[0];
    public static Parameter P_FOreq => Parameters[1];
    public static Parameter P_Vin => Parameters[2];
    public static Parameter P_Rc1   => Parameters[3];
    public static Parameter P_Beta1 => Parameters[4];
    public static Parameter P_kSat1 => Parameters[5];
    public static Parameter P_Rc2   => Parameters[6];
    public static Parameter P_Beta2 => Parameters[7];
    public static Parameter P_kSat2 => Parameters[8];
    #endregion

    #region ПСЕВДОНИМЫ ДЛЯ МАТЕМАТИКИ (Чтобы в формулах работать просто со значениями типа double)
    public static double Vcc    { get => Parameters[0].Value; set => Parameters[0].Value = value; }
    public static double FOreq  { get => Parameters[1].Value; set => Parameters[1].Value = value; }
    public static double Vin   { get => Parameters[2].Value; set => Parameters[2].Value = value; }
    public static double Rc1    { get => Parameters[3].Value; set => Parameters[3].Value = value; }
    public static double Beta1  { get => Parameters[4].Value; set => Parameters[4].Value = value; }
    public static double kSat1  { get => Parameters[5].Value; set => Parameters[5].Value = value; }
    public static double Rc2    { get => Parameters[6].Value; set => Parameters[6].Value = value; }
    public static double Beta2  { get => Parameters[7].Value; set => Parameters[7].Value = value; }
    public static double kSat2  { get => Parameters[8].Value; set => Parameters[8].Value = value; }
    #endregion

    // РЕЗУЛЬТАТЫ (пишет Calculate, читает отрисовка)
    public static readonly Parameter[] Outputs = [
        new Parameter("Rb",      0, "Ohm", "out"),
        new Parameter("V_oh",    0, "V",   "out"),
        new Parameter("V_ol",    0, "V",   "out"),
        new Parameter("V_ih",    0, "V",   "out"),
        new Parameter("I_c",     0, "A",   "out"),
        new Parameter("I_b",     0, "A",   "out"),
        new Parameter("V_in_calc", 0, "V", "out"),
        new Parameter("I_oh",    0, "A",   "out"),
        new Parameter("I_ol",    0, "A",   "out"),
        new Parameter("I_ol_spare", 0, "A", "out"),
        new Parameter("I_ih",    0, "A",   "out"),
        new Parameter("Fan-Out", 0, "",    "out"),
        new Parameter("NmL",     0, "V",   "out"),
        new Parameter("NmH",     0, "V",   "out"),
        new Parameter("V_be",    0, "V",   "out"),
        new Parameter("V_ce_sat", 0, "V",  "out"),
        new Parameter("V_in_calc", 0, "V", "out"),
    ];

    #region ПСЕВДОНИМЫ ДЛЯ РЕЗУЛЬТАТОВ
    public static double Rb    { get => Outputs[0].Value;  set => Outputs[0].Value = value; }
    public static double Voh   { get => Outputs[1].Value;  set => Outputs[1].Value = value; }
    public static double Vol   { get => Outputs[2].Value;  set => Outputs[2].Value = value; }
    public static double Vih   { get => Outputs[3].Value;  set => Outputs[3].Value = value; }
    public static double Icsat { get => Outputs[4].Value;  set => Outputs[4].Value = value; }
    public static double Ibsat { get => Outputs[5].Value;  set => Outputs[5].Value = value; }
    public static double VinCalc { get => Outputs[6].Value; set => Outputs[6].Value = value; }
    public static double Ioh   { get => Outputs[7].Value;  set => Outputs[7].Value = value; }
    public static double Iol   { get => Outputs[8].Value;  set => Outputs[8].Value = value; }
    public static double Iih   { get => Outputs[9].Value;  set => Outputs[9].Value = value; }
    public static int    FO    { get => (int)Outputs[10].Value; set => Outputs[10].Value = value; }
    public static double NmL   { get => Outputs[11].Value; set => Outputs[11].Value = value; }
    public static double NmH   { get => Outputs[12].Value; set => Outputs[12].Value = value; }
    public static double Vbe      { get => Outputs[13].Value; set => Outputs[13].Value = value; }
    public static double Vcesat   { get => Outputs[14].Value; set => Outputs[14].Value = value; }
    public static double IolSpare { get => Outputs[15].Value; set => Outputs[15].Value = value; }

    #endregion

    #region КОНСТАНТЫ СХЕМОТЕХНИКИ
    public const double Vt = 0.025;      // тепловое напряжение
    public const double Is = 5.47e-12;   // ток насыщения
    public const double Rs = 0.1;        // омическое сопротивление выводов
    public const double Vil    = 0.5;
    public const double Iil    = 15e-9;
    #endregion
}
#endregion

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

    public static void LogoMain()
    {
                        Console.WriteLine(@"__    __       __       ___    ___ ______       
\ \  / /      /  \      |  \   | | | ___ \      
 \ \/ /      / /\ \     | \ \  | | | |  \ \     
  \  /      / /  \ \    | |\ \ | | | |  | |     
  /  \     / /____\ \   | | \ \| | | |  | |     
 / /\ \   / /------\ \  | |  \ \ | | |__/ /     
/_/  \_\ /_/        \_\ |_|   \__| |_____/  CALC");
    }

    static (double Voh, double NmH, double Ioh, double Iih) LoadPoint(int n)
    {
        double voh = (Rb * Vcc + n * Rc1 * Vbe) / (Rb + n * Rc1);
        return (voh, voh - Vih, (Vcc - voh) / Rc1, (voh - Vbe) / (Rb + Rc1));
    }

    public static void PrintLoadTable()
    {
        const int L = 10;   // ширина колонки-подписи
        const int W = 14;   // ширина колонки данных

        var pts = new[] { LoadPoint(0), LoadPoint(1), LoadPoint((int)FOreq) };
        string[] heads = { "N=0", "N=1", $"N={FOreq:F0}" };

        // локальная функция: рисует линию рамки
        void Border(char left, char mid, char right)
        {
            Console.Write(left + new string('─', L));
            foreach (var _ in pts) Console.Write(mid + new string('─', W));
            Console.WriteLine(right);
        }

        "\n=== Load table ===".Print(Cyan);

        Border('┌', '┬', '┐');
        Console.Write("│" + "FO = N".PadLeft(L));
        foreach (var h in heads) Console.Write("│" + h.PadLeft(W));
        Console.WriteLine("│");
        Border('├', '┼', '┤');

        Console.Write("│" + "Voh".PadRight(L));
        foreach (var t in pts) Console.Write("│" + $"{t.Voh:F2} V".PadLeft(W));
        Console.WriteLine("│");

        Console.Write("│" + "NM_H".PadRight(L));
        foreach (var t in pts)
        {
            Console.Write("│");
            $"{t.NmH:F2} V".PadLeft(W).Print(t.NmH < 0.3 ? Red : null, false);
        }
        Console.WriteLine("│");

        Console.Write("│" + "Ioh".PadRight(L));
        foreach (var t in pts) Console.Write("│" + t.Ioh.FormatCurrent().PadLeft(W));
        Console.WriteLine("│");

        Console.Write("│" + "Iih".PadRight(L));
        foreach (var t in pts) Console.Write("│" + t.Iih.FormatCurrent().PadLeft(W));
        Console.WriteLine("│");

        Border('└', '┴', '┘');
    }

    // Хард: значение бессмысленно -> переспросить
    public static string HardLimit(string name, double v)
    {
        if ((name == "kSat1" || name == "kSat2") && v <= 1)
            return "k <= 1: no base overdrive — no saturation, the transistor is not a switch. Enter k > 1";
        if (name == "Required Fan-Out" && v < 1)
            return "Fan-Out < 1 makes no sense. Enter a natural number";
        return null;
    }

    // Софт: допустимо, но подозрительно -> принять с предупреждением
    public static string SoftWarn(string name, double v)
    {
        if ((name == "Beta1" || name == "Beta2") && (v < 5 || v > 100))
            return "Note: Create: Power Grid clamps gain to 5..100";
        if ((name == "Rc1" || name == "Rc2") && v < 100)
            return "Warning: very small Rc — huge current and power";
        if (name == "Vin" && v > Vcc)
            return "Warning: input high is above supply Vcc — check your level shifting";
        return null;
    }

}
#endregion



class XANDcalc {

    static bool logo = true;   

    static void Main()
    {
        Console.InputEncoding = System.Text.Encoding.UTF8;
        //Заставка
        Console.WriteLine();

        bool run = true;
        while (run)
        {
            if (logo) {
                Console.Clear();
                LogoMain();
            }

            logo = false;
            Console.WriteLine("\n0. Exit/Stop");
            Console.WriteLine("1. NOT gate");

            Console.Write("\nInput: ");

            string? Choice = Console.ReadLine()?.ToLower();

            switch (Choice)
            {
                default: 
                    Console.WriteLine("\nInvalid input, try again");
                    break;

                case "xand": 
                    Console.WriteLine("\nYes, this is my program!");
                    break;

                case "tserkovnik": case "tser":
                    Console.WriteLine("\nCool guy");
                    break;

                case "0": case "exit": case "stop": case "break":
                    Console.WriteLine("\nGoodbye\n");
                    run = false;
                    break;
                
                case "1": case "not": 
                    NOTchoice();
                    break;

                case "echo":
                    Console.WriteLine();
                    Console.WriteLine(@"""ECHO"" is a beautiful song; I really love listening to it. 
However, did you know that its author released a ranobe (light novel) based on it? 
I haven't read it myself yet, but I hope I'll be able to do so soon. 

People who know English will probably find it easy to read since 
it's available in the original Japanese, as well as English and Vietnamese translations 
(though I don't actually speak English myself).");
                    break;

                case "qwen":
                    "\nThank you\n".Print();
                    break;

            }
        }
    }

    static void NOTchoice()
    {
        Console.Clear();
        Console.WriteLine("\n[NOT] [x=menu]");

        var filteredParams = Vars.Parameters.Where(p => p.Tag == "all" || p.Tag == "t1").ToList();

        int step = 0;
        while (step < filteredParams.Count)
        {
            var p = filteredParams[step];
            string back = step == 0 ? "0=exit" : "0=back";
            string now = (p.Name == "Vin" && VinAuto) ? $"{p.Value}, auto" : $"{p.Value}";
            string prompt = $"{p.Name} [{p.Unit}] (now {now})  [{back}]: ";

            switch (ReadNumber(prompt, out double v))
            {
                case ReadResult.Number:
                {
                    string err = HardLimit(p.Name, v);
                    if (err != null) { err.Print(Red); break; }   // переспрос, step не двигаем
                    string warn = SoftWarn(p.Name, v);
                    if (warn != null) warn.Print(Yellow);
                    if (p.Name == "Vcc" && VinAuto) Vin = v;      // авто включено — Vin едет за Vcc
                    if (p.Name == "Vin") VinAuto = false;         // взяли вручную — авто выкл
                    p.Value = v;
                    step++;
                    break;
                }
                case ReadResult.Auto:
                    if (p.Name == "Vin") { VinAuto = true; Vin = Vcc; step++; }
                    else "auto is available for Vin only".Print(Yellow);
                    break;
                case ReadResult.Back:
                    if (step == 0) return;
                    step--;
                    ClearLastLine();
                    ClearLastLine();
                    break;
                case ReadResult.Exit:
                    return;
                case ReadResult.Retry:
                    break;
                case ReadResult.Skip:
                    step++;
                    break;
            }
        }

        CalcNOT();   // сюда расчёт

    }

    static void CalcNOT()
    {

        //РАСЧЁТЫ!

        //модель транзистора (Ebers-Moll)
        if (kSat1 <= 1) { "k must be > 1 for saturation!".Print(Red); return; }

        double betaR = Math.Max(0.5, Beta1 * 0.1);   // обратный β

        Vcesat = Vt * Math.Log((Beta1 / betaR + kSat1 * (1 + 1 / betaR)) / (kSat1 - 1));

        Icsat = (Vcc - Vcesat) / Rc1;

        double Ie = Icsat * (1 + 1 / Beta1);
        Vbe = Vt * Math.Log(Icsat / Is + 1) + Ie * Rs;   // Шокли + омическая добавка

        //№3 Ток базы, мин
        Ibsat = Icsat/Beta1*kSat1;

        //№4 Базовый резистор
        VinCalc = Vin * 0.9;
        Rb = (VinCalc - Vbe)/Ibsat - Rc1*FOreq;

        //ПАРАМЕТРЫ гейта под нагрузкой

        Voh = (Rb*Vcc + FOreq*Rc1*Vbe)/(Rb+FOreq*Rc1);
        Vol = Vcesat;

        Vih = Vbe + Ibsat*Rb;

        Ioh = (Vcc - Voh)/Rc1;//sourse current

        Iol = (Vcc - Vcesat) / Rc1;      // РЕАЛЬНЫЙ ток стока в "0" (= Icsat)
        IolSpare = Icsat * (kSat1 - 1);  // ЗАПАС: сколько ещё можно слить, оставаясь в насыщении
    

        Iih = (Voh - Vbe)/(Rb+Rc1);

        FO = (int)(Ioh/Iih);

        NmL = Vil - Vol;
        NmH = Voh - Vih;

        NOTres();
    }

    static void NOTres()
    {
        //ВИЗУАЛ ВИЗУАЛ ВИЗУАЛ
        Console.Clear();
        "The project author is not a professional (yet), so there may be errors.\n".Print(DarkGray);
        LogoMain();

        Console.Write("\n\n");

        "[NOT gate under load]\n".Print();

        $"Rc = {Rc1:N0} Ohm".Print();
        $"Rb = {Rb:N0} Ohm\n".Print(Rb < 0 ? Red : null);

        $"Vcc = {Vcc:0.00}V".Print(Cyan);
        $"Beta = {Beta1}".Print();
        $"k = {kSat1}\n".Print();

        $"V_in = {Vin:0.00}V".Print(Cyan);
        $"V_in calc = {VinCalc:0.00}V (-10%)\n".Print(DarkGray);

        $"V_be = {Vbe:0.00}V".Print();
        $"V_ce(sat) = {Vcesat:0.00}V\n".Print();

        $"I_c = {Icsat.FormatCurrent()}".Print();
        $"I_b = {Ibsat.FormatCurrent()}\n".Print();

        PrintLoadTable();

        $"\nI_ol = {Iol.FormatCurrent()}".Print();
        $"I_ol spare = {IolSpare.FormatCurrent()}".Print();
        $"I_il = {Iil.FormatCurrent()}\n".Print();

        $"V_ol = {Vol:0.00}V".Print(Vol >= Vil ? Red : null);
        $"V_ih = {Vih:0.00}V".Print(Vih >= Voh ? Red : null);
        $"V_il = {Vil:0.00}V\n".Print(Vol >= Vil ? Red : null);

        $"Nm_l = {NmL:0.00}V".Print(NmL < 0.0 ? Red : null);

        $"Fan-Out = {FO}".Print(FO >= FOreq ? Green : Red);

        $"\n\n".Print();

        while (true)
        {
            "\nReturn to the main menu? (y): ".Print(line: false); 
            string ans = Console.ReadLine()?.Trim().ToLower() ?? "";

            if (ans == "y" || ans == "д") 
            {
                break;
            }
            "Invalid choice. Type 'y' for Yes.".Print(ConsoleColor.Yellow);
        }




        logo = true;
    }
    
  
}