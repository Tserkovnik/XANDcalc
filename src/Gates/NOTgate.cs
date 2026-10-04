namespace XANDcalc;

class NOTgate
{

    // --- Объявление параметров: гейт сам говорит, что ему нужно ---
    // Вызывается при входе в режим и при загрузке; повторно — не сбрасывает значения
    public static void DeclareTransistor(int i)
    {
        Vars.Declare($"Beta{i}", 100, "Natural number", $"t{i}");
        Vars.Declare($"kSat{i}", 2, "Natural number", $"t{i}");
        // расчётные:
        Vars.Declare($"Rb{i}", 0, "Ohm", $"t{i}", "out");
        Vars.Declare($"Vbe{i}", 0, "V", $"t{i}", "out");
        Vars.Declare($"Vcesat{i}", 0, "V", $"t{i}", "out");
        Vars.Declare($"Icsat{i}", 0, "A", $"t{i}", "out");
        Vars.Declare($"Ibsat{i}", 0, "A", $"t{i}", "out");

        Vars.Declare($"Prb{i}", 0, "W", $"t{i}", "out");   // базовый резистор
        Vars.Declare($"Ptr{i}", 0, "W", $"t{i}", "out");   // сам транзистор
    }

    public static void DeclareNOT()
    {
        Vars.Declare("Vcc", 5.0, "V", "all");
        Vars.Declare("Required Fan-Out", 8, "Natural number", "all");
        Vars.Declare("Inputs", 1, "Natural number", "n"); // G A Y ? ? ?
        Vars.Declare("Vin", 5.0, "V", "all");
        Vars.Declare("Rc", 470.0, "Ohm", "all");
        Vars.Declare("T_amb", 18.0, "C", "biome");   // расчётный биом по умолчанию = равнины
        DeclareTransistor(1);
        // выходы гейта целиком; имена прежние — сейвы и привычки выживают
        Vars.Declare("V_in_calc", 0, "V", "all", "out");
        Vars.Declare("V_oh", 0, "V", "all", "out");
        Vars.Declare("V_ol", 0, "V", "all", "out");
        Vars.Declare("V_ih", 0, "V", "all", "out");
        Vars.Declare("V_il", 0, "V", "all", "out");
        Vars.Declare("I_oh", 0, "A", "all", "out");
        Vars.Declare("I_ol", 0, "A", "all", "out");
        Vars.Declare("I_ol_spare", 0, "A", "all", "out");
        Vars.Declare("I_ih", 0, "A", "all", "out");
        Vars.Declare("Fan-Out", 0, "Natural number", "all", "out");
        Vars.Declare("NmL", 0, "V", "all", "out");
        Vars.Declare("NmH", 0, "V", "all", "out");
        Vars.Declare("P_rc", 0, "W", "all", "out");
        
    }

    // Точка нагрузки: формулы те же, транзистор берём через обёртку
    static (double Voh, double NmH, double Ioh, double Iih) LoadPoint(int n)
    {
        var t1 = new Transistor(1);
        double voh = (t1.Rb * Vcc + n * Rc * t1.Vbe) / (t1.Rb + n * Rc);
        return (voh, voh - Vih, (Vcc - voh) / Rc, (voh - t1.Vbe) / (t1.Rb + Rc));
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

    public static void PrintThermalTable()
    {
        var t1 = new Transistor(1);
        const int L = 10;
        const int W = 14;
        var cols = ThermalColumns(Tamb);   // сосед снизу, выбранный, сосед сверху
        void Border(char left, char mid, char right)
        {
            Console.Write(left + new string('─', L));
            foreach (var _ in cols) Console.Write(mid + new string('─', W));
            Console.WriteLine(right);
        }
        void Cell(double t)
        {
            Console.Write("│");
            $"{t:F3} C".PadLeft(W).Print(t >= Tover ? Red : t >= Trated ? Yellow : null, false);
        }
        "\n=== Thermal table (steady, worst case) ===".Print(Cyan);
        Border('┌', '┬', '┐');
        Console.Write("│" + "biome".PadLeft(L));
        foreach (var c in cols) Console.Write("│" + c.Head.PadLeft(W));
        Console.WriteLine("│");
        Border('├', '┼', '┤');
        Console.Write("│" + "T_amb".PadRight(L));
        foreach (var c in cols) Console.Write("│" + $"{c.T:F3} C".PadLeft(W));
        Console.WriteLine("│");
        Console.Write("│" + "T_Rc".PadRight(L));
        foreach (var c in cols) Cell(c.T + Prc / DRes);
        Console.WriteLine("│");
        Console.Write("│" + "T_Rb".PadRight(L));
        foreach (var c in cols) Cell(c.T + t1.Prb / DRes);
        Console.WriteLine("│");
        Console.Write("│" + "T_tr".PadRight(L));
        foreach (var c in cols) Cell(c.T + t1.Ptr / DTr);
        Console.WriteLine("│");
        Border('└', '┴', '┘');
    }

    // Три колонки вокруг выбранного T_amb: ближайший снизу, сам выбранный (*), ближайший сверху
    public static (string Head, double T)[] ThermalColumns(double selected)
    {
        var all = new List<(string Head, double T)>();
        int center = -1;
        foreach (var (name, b) in Biomes.Common)
        {
            double a = Biomes.Ambient(b);
            if (center < 0 && Math.Abs(a - selected) < 0.05)
            { all.Add((name + " *", a)); center = all.Count - 1; }   // совпало с биомом — он и есть центр
            else all.Add((name, a));
        }
        if (center < 0)                                               // кастомное значение
        {
            all.Add(("yours *", selected));
            all.Sort((x, y) => x.T.CompareTo(y.T));
            center = all.FindIndex(c => c.Head == "yours *");
        }
        int lo = Math.Max(0, Math.Min(center - 1, all.Count - 3));   // окно из 3 с центром в выбранном, у краёв сдвигается
        return new[] { all[lo], all[lo + 1], all[lo + 2] };
    }


    public static void NOTchoice()
    {
        logo = true;
        Gate = "NOT";

        DeclareNOT();
        "\n[NOR] [x=menu]".Print();
        bool flag = true;
        
        while (flag)
            switch(ReadNumber($"Number of inputs (now {NumIn}) (N=1 for NOT) [0=exit]: ", out double n))
            {
                case ReadResult.Number:
                    if (n < 1 || n != Math.Floor(n)) { "Whole number >= 1".Print(Yellow); break; }
                    NumIn = n;
                    flag = false;
                    break;
                    
                case ReadResult.Back: case ReadResult.Exit:
                    return;

                case ReadResult.Auto: case ReadResult.Retry:
                    break;

                case ReadResult.Skip:
                    flag = false;
                    break;
            }

        if (!ChooseBiome()) return;

        Console.Clear();
        if (NumIn == 1) Console.WriteLine("\n[NOT] [x=menu]"); else $"\n[wired NOR{NumIn}] [x=menu]".Print();

        var filteredParams = Vars.Order.Where(p => p.Kind == "in" && (p.Tag == "all" || p.Tag == "t1")).ToList();

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
        var t1 = new Transistor(1);

        //РАСЧЁТЫ!

        //модель транзистора (Ebers-Moll)
        if (t1.k <= 1) { "k must be > 1 for saturation!".Print(Red); return; }

        t1.Vcesat = Vt * Math.Log((t1.Beta / t1.BetaR + t1.k * (1 + 1 / t1.BetaR)) / (t1.k - 1));

        t1.Icsat = (Vcc - t1.Vcesat) / Rc;

        double Ie = t1.Icsat * (1 + 1 / t1.Beta);
        t1.Vbe = Vt * Math.Log(t1.Icsat / Is + 1) + Ie * Rs;   // Шокли + омическая добавка

        //№3 Ток базы, мин
        t1.Ibsat = t1.Icsat/t1.Beta*t1.k;

        Vol = t1.Vcesat;

        //№4 Базовый резистор
        VinCalc = Vin * 0.9;

        int n = (int)Vars.Val("Inputs");
        double vEffective = VinCalc - n * t1.Vbe + (n - 1) * Vol;
        t1.Rb = vEffective / t1.Ibsat - Rc * FOreq;

        //ПАРАМЕТРЫ гейта под нагрузкой

        Voh = (t1.Rb*Vcc + FOreq*Rc*t1.Vbe)/(t1.Rb+FOreq*Rc);

        Vil = Vt * Math.Log(Vt / (Rc * Is));  //зависит от Rc

        Ioh = (Vcc - Voh)/Rc;//sourse current
        Iih = (Voh - t1.Vbe) / (t1.Rb + Rc);

        Iol = (Vcc - t1.Vcesat) / Rc;      //РЕАЛЬНЫЙ ток стока в "0" (= Icsat)
        IolSpare = t1.Icsat * (t1.k - 1);  //ЗАПАС: сколько ещё можно слить, оставаясь в насыщении
    

        Vih = n * t1.Vbe - (n - 1) * Vol + t1.Ibsat * t1.Rb;

        FO = (int)(Ioh/Iih);

        NmL = Vil - Vol;
        NmH = Voh - Vih;

        double ibMaxOne = (Vcc - t1.Vbe) / (t1.Rb + Rc); 
        Prc = t1.Icsat * t1.Icsat * Rc;
        t1.Prb = ibMaxOne * ibMaxOne * t1.Rb;

        //транзистор греется от суммарного тока всех n входов
        t1.Ptr = t1.Vcesat * t1.Icsat + t1.Vbe * (n * ibMaxOne); 



        NOTres();
    }

    public static void NOTres()
    {
        logo = true;
        var t1 = new Transistor(1);

        //ВИЗУАЛ ВИЗУАЛ ВИЗУАЛ
        Console.Clear();
        "The project author is not a professional (yet), so there may be errors.\n".Print(DarkGray);
        LogoMain();

        Console.Write("\n\n");

        int n = (int)Vars.Val("Inputs");
        if (n == 1) "[NOT gate under load]\n".Print(); 
        else $"[Wired NOR gate ({n} inputs) under load]\n".Print();

        $"Rc = {Rc:N0} Ohm".Print();
        $"Rb = {t1.Rb:N0} Ohm\n".Print(t1.Rb < 0 ? Red : null);

        $"Vcc = {Vcc:0.00}V".Print(Cyan);
        $"Beta = {t1.Beta}".Print();
        $"k = {t1.k}\n".Print();

        $"V_in = {Vin:0.00}V".Print(Cyan);
        $"V_in calc = {VinCalc:0.00}V (-10%)\n".Print(DarkGray);

        $"V_be = {t1.Vbe:0.00}V".Print();
        $"V_ce(sat) = {t1.Vcesat:0.00}V\n".Print();

        $"I_c = {t1.Icsat.FormatCurrent()}".Print();
        $"I_b = {t1.Ibsat.FormatCurrent()}\n".Print();

        PrintLoadTable();

        PrintThermalTable();

        double Pgate = Prc + t1.Prb + t1.Ptr;
        $"\nP_Rc = {Prc.FormatPower()}".Print();
        $"P_Rb = {t1.Prb.FormatPower()}".Print();
        $"P_tr = {t1.Ptr.FormatPower()}".Print();
        $"P_gate = {Pgate.FormatPower()}".Print();

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