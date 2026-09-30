namespace XANDcalc;

// Биомы: base-температуры из определений биомов Minecraft,
// окружение — по формуле мода (ThermalBehaviour.getAmbientTemperature)
static class Biomes
{
    public static double Ambient(double baseTemp) => 13.65 * baseTemp + 7.1;

    public static readonly (string Name, double Base)[] Common =
    {
        ("Snowy Plains", 0.0),
        ("Taiga",        0.25),
        ("Ocean",        0.5),
        ("Forest",       0.7),
        ("Plains",       0.8),
        ("Jungle",       0.95),
        ("Savanna",      1.2),
        ("Desert",       2.0),
    };

    public static void PrintReference()
    {
        "\n=== Biome ambient temperatures ===".Print(Cyan);
        foreach (var (name, b) in Common)
            $"{name,-14} base {b,4:0.00}  ->  T_amb {Ambient(b),5:0.0} C".Print();
        "\nFormula: T_amb = 13.65 * base + 7.1".Print(DarkGray);
    }

    // Экран биома ДО основного ввода: строитель тычет номер, электронщик скипает,
    // кастом — в градусах, потому что считать должна прога, а не человек
    public static bool ChooseBiome()
    {
        while (true)
        {
            Console.Clear();
            LogoMain();
            $"\n[Biome] now: T_amb = {Tamb:0.0} C".Print(Cyan);
            for (int i = 0; i < Biomes.Common.Length; i++)
            {
                var (name, b) = Biomes.Common[i];
                $"{i + 1,2}. {name,-14} base {b:0.00} -> {Biomes.Ambient(b),5:0.0} C".Print();
            }
            " c. Custom T_amb (C)".Print();
            "\nBiome [Enter/0 = skip, x = menu]: ".Print(line: false);
            string s = Console.ReadLine()?.Trim().ToLower() ?? "";
            if (s == "" || s == "0") return true;                 // скип: оставляем текущий
            if (s == "x" || s == "q" || s == "ч") return false;   // в меню без расчёта
            if (s == "c" || s == "с")
            {
                var r = ReadNumber("Custom T_amb [C]: ", out double v);
                if (r == ReadResult.Number) { Tamb = v; return true; }
                if (r == ReadResult.Back) { Tamb = 0; return true; }
                if (r == ReadResult.Exit) return true;
                continue;                                          // мусор -> перерисовать
            }
            if (int.TryParse(s, out int n) && n >= 1 && n <= Biomes.Common.Length)
            { Tamb = Biomes.Ambient(Biomes.Common[n - 1].Base); return true; }
            "Invalid input, try again".Print(Yellow);
        }
    }
}