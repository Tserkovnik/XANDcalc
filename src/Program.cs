using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static System.ConsoleColor;

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
}



struct CALCres
{
    public double Icsat;
    public double Ibsat;
    public double Vin;
    public double Rb;
    public double VohFO;
    public double VolFO;
    public double VihFO;
    public double IohFO;
    public double IolFO;
    public double IihFO;
    public int FO;
}

class Parameter
{
    public string Name;
    public double Value;
    public double Default;

    public Parameter(string name, double value)
    {
        Name = name;
        Value = value;
    }
}


class XANDcalc {
    enum ReadResult { Number, Back, Exit, Retry, Skip }

    //переменые
    static bool logo = true;

    

    //параметры
    static Parameter[] parameters = [
        new Parameter("Vcc", 5.0),
        new Parameter("Rc", 470),
        new Parameter("Beta", 100),
        new Parameter("kSat", 2),
        new Parameter("Required Fan-Out", 8),
    ];

    static Parameter Vcc => parameters[0];
    static Parameter Rc => parameters[1];
    static Parameter hFE => parameters[2];
    static Parameter kSat => parameters[3];
    static Parameter FOreq => parameters[4];

    

    //Константы
    const double Vbe   = 0.7;
    const double Vcesat= 0.2;
    const double Vil = 0.5;
    const double Iil = 15e-9;

    static string GetUnit(string name)
    {
        switch (name)
        {
            case "Vcc":  return "V";
            case "Rc": case "Rb":   return "Ohm";
            case "Beta": case "kSat":  case "Required Fan-Out": return "Natural number";
            default:     return "";
        }
    }

    static void ClearLastLine()
    {
        // курсор сдвиг вверх
        Console.CursorTop--; 
        Console.CursorLeft = 0;
        
        // пробелы вместо текста 
        // и возвращаем курсор в начало этой строки
        Console.Write(new string(' ', Console.BufferWidth-1));
        Console.CursorLeft = 0;
    }

    static ReadResult ReadNumber(string prompt, out double value)
    {
        value = 0;                                   // на всякий случай
        Console.Write(prompt);                       // печатаем подсказку
        string s = Console.ReadLine()?.Trim().ToLower() ?? "";

        if (s == "0")  return ReadResult.Back;       // код "назад"
        if (s == "x" || s == "q" || s == "ч") return ReadResult.Exit; // код "выход"
        if (s == "") return ReadResult.Skip;

        if (double.TryParse(s, out value))           // получилось число?
        {
            if (value > 0) return ReadResult.Number; // да и >0 -> код Number, число в value
            Console.WriteLine("Value must be positive");
            return ReadResult.Retry;                 // число, но плохое -> "переспроси"
        }

        if (s.Length<5) Console.WriteLine("Invalid input, try again"); else Console.WriteLine("You are idiot, try again");          // вообще не число
        return ReadResult.Retry;
    }

    

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
                Console.WriteLine(@"__    __       __       ___    ___ ______       
\ \  / /      /  \      |  \   | | | ___ \      
 \ \/ /      / /\ \     | \ \  | | | |  \ \     
  \  /      / /  \ \    | |\ \ | | | |  | |     
  /  \     / /____\ \   | | \ \| | | |  | |     
 / /\ \   / /------\ \  | |  \ \ | | |__/ /     
/_/  \_\ /_/        \_\ |_|   \__| |_____/  CALC");
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
                    NOTcalc();
                    break;
            }
        }
    }

    static void NOTcalc()
    {
        logo = true;
        Console.Clear();
        Console.WriteLine("\n[NOT] [x=menu]");

        int step = 0;
        while (step < parameters.Length)
        {
            var p = parameters[step];
            string back = step == 0 ? "0=exit" : "0=back";
            string prompt = $"{p.Name} [{GetUnit(p.Name)}] (now {p.Value})  [{back}]: ";

            switch (ReadNumber(prompt, out double v))
            {
                case ReadResult.Number:
                    p.Value = v;
                    step++;
                    break;
                case ReadResult.Back:
                    if (step == 0) return;   //выход из режима
                    step--;
                    ClearLastLine();
                    ClearLastLine();
                    break;
                case ReadResult.Exit:
                    return;
                case ReadResult.Retry:
                    break;                   // просто переспросить
                case ReadResult.Skip:
                    step++;
                    break;
            }
        }

        CalculateNOT(Vcc, Rc, hFE, kSat, FOreq);   // сюда расчёт

    }

    static void CalculateNOT(Parameter Vcc, Parameter Rc, Parameter hFE, Parameter kSat, Parameter FOreq)
    {

        //РАСЧЁТЫ!

        //№1 у нас по сути уже есть параметры, в том числе выбранный Rc

        //№2 Ток коллектора
        double Icsat = (Vcc.Value - Vcesat)/Rc.Value;

        //№3 Ток базы, мин
        double Ibsat = Icsat/hFE.Value*kSat.Value;

        //№4 Базовый резистор
        double Vin = Vcc.Value * 0.9; //просто чтобы брать не макс значение.
        double Rb = (Vin - Vbe)/Ibsat - Rc.Value*FOreq.Value;

        //ПАРАМЕТРЫ гейта под нагрузкой

        double VohFO = (Rb*Vcc.Value + FOreq.Value*Rc.Value*Vbe)/(Rb+FOreq.Value*Rc.Value);
        double VolFO = Vcesat;

        double VihFO = Vbe + Ibsat*Rb;

        double IohFO = (Vcc.Value - VohFO)/Rc.Value;//sourse current
        double IolFO = (Vcc.Value - Vcesat)/Rc.Value; //sink current

        double IihFO = (VohFO - Vbe)/(Rb+Rc.Value);

        int FO = (int)(IohFO/IihFO);

        CALCres res = new CALCres
        {
            Icsat = Icsat,
            Ibsat = Ibsat,
            Vin = Vin,
            Rb = Rb,
            
            VohFO = VohFO,
            VolFO = VolFO,
            VihFO = VihFO,

            IohFO = IohFO,
            IolFO = IolFO,
            IihFO = IihFO,
            FO = FO,
        };

        NOTres(res);
    }

    static void NOTres(CALCres res)
    {
        //ВИЗУАЛ ВИЗУАЛ ВИЗУАЛ
        Console.Clear();
        "The project author is not a professional (yet), so there may be errors.\n".Print(DarkGray);
        @"__    __       __       ___    ___ ______       
\ \  / /      /  \      |  \   | | | ___ \      
 \ \/ /      / /\ \     | \ \  | | | |  \ \     
  \  /      / /  \ \    | |\ \ | | | |  | |     
  /  \     / /____\ \   | | \ \| | | |  | |     
 / /\ \   / /------\ \  | |  \ \ | | |__/ /     
/_/  \_\ /_/        \_\ |_|   \__| |_____/  CALC".Print();

        Console.Write("\n\n");

        "[NOT gate under load]\n".Print();

        $"Rc = {Rc.Value:N0} Ohm".Print();
        $"Rb = {res.Rb:N0} Ohm\n".Print(res.Rb < 0 ? Red : null);

        $"Vcc = {Vcc.Value:0.00}V".Print(Cyan);
        $"Beta = {hFE.Value}".Print();
        $"k = {kSat.Value}\n".Print();

        $"I_c = {res.Icsat.FormatCurrent()}".Print();
        $"I_b = {res.Ibsat.FormatCurrent()}\n".Print();

        $"Under load:".Print();
        $"I_oh/source current = {res.IohFO.FormatCurrent()}".Print();
        $"I_ol/sink current = {res.IolFO.FormatCurrent()}".Print();
        $"I_ih = {res.IihFO.FormatCurrent()}".Print();
        $"I_il = {Iil.FormatCurrent()}\n".Print();

        $"V_in = {res.Vin:0.00}V".Print();
        $"V_oh = {res.VohFO:0.00}V".Print(res.VihFO >= res.VohFO ? Red : null);
        $"V_ol = {res.VolFO:0.00}V".Print(res.VolFO >= Vil ? Red : null);
        $"V_ih = {res.VihFO:0.00}V".Print(res.VihFO >= res.VohFO ? Red : null);
        $"V_il = {Vil:0.00}V\n".Print(res.VolFO >= Vil ? Red : null);

        $"Fan-Out = {res.FO}".Print(res.FO >= FOreq.Value ? Green : Red);

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




        
    }
    
  
}