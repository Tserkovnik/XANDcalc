namespace XANDcalc;

class NOTgate
{
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




    public static void NOTchoice()
    {
        Gate = "NOT";
        logo = true;

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
                    string? err = HardLimit(p.Name, v);
                    if (err != null) { err.Print(Red); break; }   // переспрос, step не двигаем
                    string? warn = SoftWarn(p.Name, v);
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

    public static void CalcNOT()
    {
        logo = true;

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

    public static void NOTres()
    {
        logo = true;

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
            "\nSave project and exit to menu? (y/n): ".Print(line: false); 
            string ans = Console.ReadLine()?.Trim().ToLower() ?? "";

            if (ans == "n") 
            {
                break;
            }

            if (ans == "y") {
                Console.Clear();
                LogoMain();
                "\n".Print();
                SaveProject();
                break;
            }

            "Invalid choice. Type 'y' for Yes.".Print(ConsoleColor.Yellow);
        }


    }
}