namespace XANDcalc;

class NANDgate
{
    public static void DeclareTransistor(int i)
    {
        Vars.Declare($"Beta{i}", 100, "Natural number", $"t{i}");
        Vars.Declare($"kSat{i}", 2, "Natural number", $"t{i}");
        Vars.Declare($"Rb{i}", 0, "Ohm", $"t{i}", "out");
        Vars.Declare($"Vbe{i}", 0, "V", $"t{i}", "out");
        Vars.Declare($"Vcesat{i}", 0, "V", $"t{i}", "out");
        Vars.Declare($"Icsat{i}", 0, "A", $"t{i}", "out");
        Vars.Declare($"Ibsat{i}", 0, "A", $"t{i}", "out");
        Vars.Declare($"Ve{i}", 0, "V", $"t{i}", "out");     // приподнятость эмиттера над землёй, вроде
        Vars.Declare($"Prb{i}", 0, "W", $"t{i}", "out");
        Vars.Declare($"Ptr{i}", 0, "W", $"t{i}", "out");
    }

    public static void DeclareGlobals()
    {
        Vars.Declare("Vcc", 5.0, "V", "all");
        Vars.Declare("Required Fan-Out", 8, "Natural number", "all");
        Vars.Declare("Vin", 5.0, "V", "all");
        Vars.Declare("Rc", 470.0, "Ohm", "all");
        Vars.Declare("T_amb", 18.0, "C", "biome");
        Vars.Declare("Inputs", 2, "Natural number", "n");
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

    public static void DeclareNAND()
    {
        DeclareGlobals();
        int n = (int)NumIn;
        for (int i = 1; i <= n; i++) DeclareTransistor(i);
    }

    static (double Voh, double NmH, double Ioh, double Iih) LoadPoint(int n)
    {
        var t1 = new Transistor(1);
        double rc = Vars.Val("Rc");
        double voh = (t1.Rb * Vcc + n * rc * t1.Vbe) / (t1.Rb + n * rc);
        return (voh, voh - Vih, (Vcc - voh) / rc, (voh - t1.Vbe) / (t1.Rb + rc));
    }

    public static void PrintLoadTable()
    {
        const int L = 10;
        const int W = 14;
        var pts = new[] { LoadPoint(0), LoadPoint(1), LoadPoint((int)FOreq) };
        string[] heads = { "N=0", "N=1", $"N={FOreq:F0}" };
        void Border(char left, char mid, char right)
        {
            Console.Write(left + new string('─', L));
            foreach (var _ in pts) Console.Write(mid + new string('─', W));
            Console.WriteLine(right);
        }
        "\n=== Load table ===".Print(Cyan);
        Border('┌', '┴', '┐');
        Console.Write("│" + "FO = N".PadLeft(L));
        foreach (var h in heads) Console.Write("│" + h.PadLeft(W));
        Console.WriteLine("│");
        Border('├', '┴', '┤');
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
        int n = (int)NumIn;
        double ptrMax = 0, prbMax = 0;
        for (int i = 1; i <= n; i++)
        {
            var t = new Transistor(i);
            ptrMax = Math.Max(ptrMax, t.Ptr);
            prbMax = Math.Max(prbMax, t.Prb);
        }
        double prc = Vars.Val("P_rc");
        const int L = 10;
        const int W = 14;
        var cols = ThermalColumns(Tamb);
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
        foreach (var c in cols) Cell(c.T + prc / DRes);
        Console.WriteLine("│");
        Console.Write("│" + "T_Rb".PadRight(L));
        foreach (var c in cols) Cell(c.T + prbMax / DRes);
        Console.WriteLine("│");
        Console.Write("│" + "T_tr".PadRight(L));
        foreach (var c in cols) Cell(c.T + ptrMax / DTr);
        Console.WriteLine("│");
        Border('└', '┴', '┘');
    }

    public static void NANDchoice()
    {
        logo = true;
        Gate = "NAND";
        DeclareGlobals();          // слот N существует ДО первого чтения NumIn
        "\n[NAND] [x=menu]".Print();
        bool flag = true;
        while (flag)
            switch (ReadNumber($"Number of inputs (now {NumIn}) [0=exit]: ", out double n))
            {
                case ReadResult.Number:
                    if (n < 1 || n != Math.Floor(n)) { "Whole number >= 1".Print(Yellow); break; }
                    NumIn = n;
                    flag = false;
                    break;
                case ReadResult.Skip:
                    flag = false;
                    break;
                case ReadResult.Back: case ReadResult.Exit:
                    return;
                case ReadResult.Auto: case ReadResult.Retry:
                    break;
            }
        DeclareNAND();
        if (!ChooseBiome()) return;
        Console.Clear();
        Console.WriteLine("\n[NAND] [x=menu]");
        var filteredParams = Vars.Order.Where(p => p.Kind == "in"
            && (p.Tag == "all" || (p.Tag.StartsWith("t") && int.Parse(p.Tag[1..]) <= (int)NumIn))).ToList();
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
                    if (err != null) { err.Print(Red); break; }
                    string? warn = SoftWarn(p.Name, v);
                    if (warn != null) warn.Print(Yellow);
                    if (p.Name == "Vcc" && VinAuto) Vin = v;
                    if (p.Name == "Vin") VinAuto = false;
                    p.Value = v;
                    if (p.Name == "Inputs") { DeclareNAND(); filteredParams = Vars.Order.Where(q => q.Kind == "in"
                        && (q.Tag == "all" || (q.Tag.StartsWith("t") && int.Parse(q.Tag[1..]) <= (int)NumIn))).ToList(); }
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
        CalcNAND();
    }

    public static void CalcNAND()
    {
        logo = true;
        int n = (int)NumIn;
        double rc = Vars.Val("Rc");
        VinCalc = Vin * 0.9;
        // проход 1: Vcesat каждого (только beta,k) и стопочный Vol
        double vol = 0;
        for (int i = 1; i <= n; i++)
        {
            var t = new Transistor(i);
            if (t.k <= 1) { $"k{i} must be > 1 for saturation!".Print(Red); return; }
            t.Vcesat = Vt * Math.Log((t.Beta / t.BetaR + t.k * (1 + 1 / t.BetaR)) / (t.k - 1));
            vol += t.Vcesat;
        }
        Vol = vol;                                   // серия: низ это СУММА Vcesat
        double I = (Vcc - Vol) / rc;                 // серийный ток: один на всех
        // суффиксные суммы: Ve,i эт сумма VceSat транзисторов НИЖЕ i
        double[] veSum = new double[n + 2];
        for (int i = n; i >= 1; i--) veSum[i] = veSum[i + 1] + new Transistor(i).Vcesat;
        // проход 2 сверху вниз: T1 коллектором к выходу, каждый нижний несёт +базовый ток верхнего
        double ic = I;  double Ic0 = 0;
        double kMin = double.MaxValue;
        double vihMax = 0;
        for (int i = 1; i <= n; i++)
        {
            var t = new Transistor(i);
            double ve = veSum[i + 1];
            t.Ve = ve;
            t.Icsat = ic;
            double Ie = ic * (1 + 1 / t.Beta);
            t.Vbe = Vt * Math.Log(ic / Is + 1) + Ie * Rs;
            t.Ibsat = ic / t.Beta * t.k;
            double head = VinCalc - ve - t.Vbe;      // голова на базовый привод после подъёма эмиттера
            if (head <= 0)
            { $"T{i}: stack too tall — no headroom (Vin_calc {VinCalc:0.00} V, V_e {ve:0.00} V, V_be {t.Vbe:0.00} V)".Print(Red); return; }
            t.Rb = head / t.Ibsat - rc * FOreq;
            t.Prb = t.Ibsat * t.Ibsat * t.Rb;
            t.Ptr = t.Vcesat * ic + t.Vbe * t.Ibsat;
            vihMax = Math.Max(vihMax, ve + t.Vbe + t.Ibsat * t.Rb);
            kMin = Math.Min(kMin, t.k);
            ic += t.Ibsat;                           // нижний несёт коллекторный + базовый

            if (t.Icsat > Ic0) Ic0 = t.Icsat;
        }
        // гейт целиком (нагрузки -- идентичные копии, представитель t1, онн худший)
        var t1 = new Transistor(1);
        Voh = (t1.Rb * Vcc + FOreq * rc * t1.Vbe) / (t1.Rb + FOreq * rc);
        Vih = vihMax;
        Vil = Vt * Math.Log(Vt / (rc * Is));
        Ioh = (Vcc - Voh) / rc;
        Iol = I;
        IolSpare = (kMin - 1) * I;                   // стопка сдаётся по слабейшему перегрузу
        Iih = (Voh - t1.Vbe) / (t1.Rb + rc);
        FO = (int)(Ioh / Iih);
        NmL = Vil - Vol;
        NmH = Voh - Vih;
        Prc = I*I * rc;
        NANDres(Ic0);
    }

    public static void NANDres(double Ic)
    {
        logo = true;
        int n = (int)NumIn;
        double rc = Vars.Val("Rc");
        Console.Clear();
        "The project author is not a professional (yet), so there may be errors.\n".Print(DarkGray);
        LogoMain();
        Console.Write("\n\n");
        $"[NAND gate, {n} inputs in series, under load]\n".Print();
        $"Rc = {rc:N0} Ohm  I_c = {Ic.FormatCurrent()}".Print();

        $"\nVcc = {Vcc:0.00}V".Print(Cyan);
        $"V_in = {Vin:0.00}V".Print(Cyan);
        $"V_in calc = {VinCalc:0.00}V (-10%)\n".Print(DarkGray);
        for (int i = 1; i <= n; i++)
        {
            var t = new Transistor(i);
            $"T{i}: Beta = {t.Beta}, k = {t.k}, V_e = {t.Ve:0.00}V, Rb = {t.Rb:N0} Ohm, V_be = {t.Vbe:0.00}V, V_ce(sat) = {t.Vcesat:0.00}V, I_c = {t.Icsat.FormatCurrent()}, I_b = {t.Ibsat.FormatCurrent()}, P_Rb = {t.Prb.FormatPower()}, P_tr = {t.Ptr.FormatPower()}".Print(t.Rb < 0 ? Red : null);
        }
        PrintLoadTable();
        PrintThermalTable();
        double tRc = Tamb + Vars.Val("P_rc") / DRes;
        $"\nP_Rc = {Vars.Val("P_rc").FormatPower()}".Print(tRc >= Tover ? Red : tRc >= Trated ? Yellow : null);
        var tw = new Transistor(1);
        double Pgate = Prc + tw.Prb + tw.Ptr;
        $"P_gate = {Pgate.FormatPower()}".Print();
        $"\nI_ol (series) = {Iol.FormatCurrent()}".Print();
        $"I_ol spare = {IolSpare.FormatCurrent()}".Print();
        $"I_il = {Iil.FormatCurrent()}\n".Print();
        $"V_ol (stacked) = {Vol:0.00}V".Print(Vol >= Vil ? Red : null);
        $"V_ih (worst) = {Vih:0.00}V".Print(Vih >= Voh ? Red : null);
        $"V_il = {Vil:0.00}V\n".Print();
        $"Nm_l = {NmL:0.00}V".Print(NmL < 0.0 ? Red : null);
        $"Fan-Out = {FO}".Print(FO >= FOreq ? Green : Red);
        $"\n\n".Print();
        while (true)
        {
            "\nSave project and exit to menu? (y/n): ".Print(line: false);
            string ans = Console.ReadLine()?.Trim().ToLower() ?? "";
            if (ans == "n") break;
            if (ans == "y")
            {
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