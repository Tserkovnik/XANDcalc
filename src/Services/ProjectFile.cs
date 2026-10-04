

namespace XANDcalc;
using System.Globalization;
using System.IO;
using System.Text;

static class ProjectFile
{
    public const string Ext = ".xand";

    public static void OpenGate()
    {
        switch (Vars.Gate)
        {
            case "NOT": CalcNOT(); break;
            case "NOR": CalcNOR(); break;
            case "NAND": CalcNAND(); break;
            default: $"This build doesn't know gate '{Vars.Gate}' yet.".Print(Red); break;
        }
    }

    // Папка всех проектов: Документы/XANDprojects
    public static string Folder =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "XANDprojects");

    public static void Save(string path)
    {
        Directory.CreateDirectory(Folder);
        var sb = new StringBuilder();
        sb.AppendLine("# XANDcalc project");
        sb.AppendLine("# version 2");
        sb.AppendLine($"Gate = {Vars.Gate}");
        sb.AppendLine($"# saved {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"VinAuto = {Vars.VinAuto}");
        foreach (var p in Vars.Order)
        {
            if (p.Kind != "in") continue;

            //если мы сохраняем NOT, у него всегда только 1 транзистор
            if (Vars.Gate == "NOT" && p.Tag.StartsWith("t") && p.Tag != "t1") continue;

            //если сохраняем NOR или NAND, пишем только те транзисторы, индексы которых не превышают инпуты
            if ((Vars.Gate == "NOR" || Vars.Gate == "NAND") && p.Tag.StartsWith("t"))
            {
                int tIndex = int.Parse(p.Tag[1..]);
                if (tIndex > (int)Vars.NumIn) continue;
            }

            sb.AppendLine($"{p.Name} = {p.Value.ToString("R", CultureInfo.InvariantCulture)}");
        }

        sb.AppendLine("# --- results at save time (info only) ---");
        foreach (var p in Vars.Order)
            if (p.Kind == "out")
                sb.AppendLine($"# {p.Name} = {p.Value.ToString("R", CultureInfo.InvariantCulture)} {p.Unit}");
        File.WriteAllText(path, sb.ToString());
    }

    public static int Load(string path)
    {
        var lines = File.ReadAllLines(path);

        // проход 1: узнать, какому гейту принадлежит файл, и объявить его параметры
        foreach (var raw in lines)
        {
            var (name, val) = Split(raw);
            if (name == "Gate") { Vars.Gate = val; break; }
        }
        DeclareForGate(Vars.Gate);

        // проход 2: значения
        int n = 0;
        foreach (var raw in lines)
        {
            var (name, val) = Split(raw);
            if (name == "VinAuto") { if (bool.TryParse(val, out bool b)) Vars.VinAuto = b; continue; }
            if (name == "Gate") continue;
            if (!Vars.Has(name)) continue;              // неизвестное имя -> файл из новой версии
            if (Vars.Get(name).Kind != "in") continue;  // расчётные из файла не перезаписываем
            bool ok = double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out double v)
                   || double.TryParse(val, NumberStyles.Float, CultureInfo.CurrentCulture, out v);
            if (ok) 
            {
                Vars.Set(name, v); n++;
                
                if (name == "Inputs") 
                {
                    DeclareForGate(Vars.Gate);
                }
            }
        }
        return n;
    }

    // Разбор строки "name = value"; для комментариев/пустых/мусора — пустое имя
    static (string, string) Split(string raw)
    {
        string line = raw.Trim();
        int eq = line.IndexOf('=');
        if (line.Length == 0 || line.StartsWith("#") || eq < 0) return ("", "");
        return (line[..eq].Trim(), line[(eq + 1)..].Trim());
    }

    static void DeclareForGate(string gate)
    {
        switch (gate)
        {
            case "NOT": NOTgate.DeclareNOT(); break;
            case "NOR": NORgate.DeclareNOR(); break;   
            case "NAND": NANDgate.DeclareNAND(); break; 
        }
    }
}