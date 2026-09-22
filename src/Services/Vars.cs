namespace XANDcalc;

#region ЦЕНТРАЛЬНАЯ БАЗА ПАРАМЕТРОВ, КОНСТАНТ И РЕЗУЛЬТАТОВ

//Единое централизованное хранилище всей программы
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

    //другое
    public static string Gate = "NOT";
}
#endregion