namespace XANDcalc;

static class Vars
{
    // --- Именное хранилище: единственный источник правды ---
    static readonly Dictionary<string, Parameter> byName = new();
    public static readonly List<Parameter> Order = new();  // порядок объявления = порядок ввода и сейва

    // Объявить параметр; повторное объявление НЕ сбрасывает значение
    public static Parameter Declare(string name, double def, string unit, string tag, string kind = "in")
    {
        if (byName.TryGetValue(name, out var p)) return p;
        p = new Parameter(name, def, unit, tag, kind);
        byName[name] = p;
        Order.Add(p);
        return p;
    }

    public static double Val(string name) => byName[name].Value;
    public static void   Set(string name, double v) => byName[name].Value = v;
    public static bool   Has(string name) => byName.ContainsKey(name);
    public static Parameter Get(string name) => byName[name];

    // --- Универсальные входы ---
    public static double Vcc   { get => Val("Vcc");   set => Set("Vcc", value); }
    public static double FOreq { get => Val("Required Fan-Out"); set => Set("Required Fan-Out", value); }
    public static double Vin   { get => Val("Vin");   set => Set("Vin", value); }

    // --- Выходы гейта: набор фиксирован для любого гейта ---
    public static double VinCalc  { get => Val("V_in_calc");  set => Set("V_in_calc", value); }
    public static double Voh      { get => Val("V_oh");       set => Set("V_oh", value); }
    public static double Vol      { get => Val("V_ol");       set => Set("V_ol", value); }
    public static double Vih      { get => Val("V_ih");       set => Set("V_ih", value); }
    public static double Ioh      { get => Val("I_oh");       set => Set("I_oh", value); }
    public static double Iol      { get => Val("I_ol");       set => Set("I_ol", value); }
    public static double IolSpare { get => Val("I_ol_spare"); set => Set("I_ol_spare", value); }
    public static double Iih      { get => Val("I_ih");       set => Set("I_ih", value); }
    public static int    FO       { get => (int)Val("Fan-Out"); set => Set("Fan-Out", value); }
    public static double NmL      { get => Val("NmL");        set => Set("NmL", value); }
    public static double NmH      { get => Val("NmH");        set => Set("NmH", value); }

    // --- Состояние программы ---
    public static string Gate = "NOT";
    public static bool VinAuto = true;

    // --- Константы схемотехники (мод) ---
    public const double Vt = 0.025;
    public const double Is = 5.47e-12;
    public const double Rs = 0.1;
    public const double Vil = 0.5;
    public const double Iil = 15e-9;

    // таблица сравнения — пока не трогаем
    public static List<CalculationRow> ResultsTable = new();
}