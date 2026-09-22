namespace XANDcalc;

static class ProjectFile
{

#region Для сейвов и чтения

    public static void SaveProject()
    {
        try
        {
            string? path = PickSavePath();
            if (path == null) { "Cancelled.".Print(Yellow); return; }
            ProjectFile.Save(path);
            $"Saved to {path}".Print(Green);
        }
        catch (System.Exception e) { $"Save failed: {e.Message}".Print(Red); }
    }

    public static void LoadProject()
    {
        try
        {
            string? path = PickLoadPath();
            if (path == null) { "Cancelled.".Print(Yellow); return; }
            int n = ProjectFile.Load(path);
            $"Loaded {n} values from {path}".Print(Green);
            switch (Vars.Gate)
            {
                case "NOT": CalcNOT(); break;
                default: $"This build doesn't know gate '{Vars.Gate}' yet.".Print(Red); break;
            }
        }
        catch (System.IO.FileNotFoundException) { "File not found in XANDprojects.".Print(Red); }
        catch (System.Exception e) { $"Load failed: {e.Message}".Print(Red); }
    }

    static string? PickSavePath()
    {
    #if WIN_DIALOGS
        using var dlg = new System.Windows.Forms.SaveFileDialog
        {
            Title = "Save XANDcalc project",
            Filter = "XANDcalc project (*.txt)|*.txt",
            InitialDirectory = ProjectFile.Folder,
            DefaultExt = "txt",
            FileName = "project",
        };
        return dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dlg.FileName : null;
    #else
        return AskPathConsole("Save");
    #endif
    }

    static string? PickLoadPath()
    {
    #if WIN_DIALOGS
        System.IO.Directory.CreateDirectory(ProjectFile.Folder);
        using var dlg = new System.Windows.Forms.OpenFileDialog
        {
            Title = "Load XANDcalc project",
            Filter = "XANDcalc project (*.txt)|*.txt",
            InitialDirectory = ProjectFile.Folder,
            DefaultExt = "txt",
        };
        return dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dlg.FileName : null;
    #else
        return AskPathConsole("Load");
    #endif
    }

    public static string? AskPathConsole(string action)
    {
        $"{action}: file name in XANDprojects (empty = cancel): ".Print(line: false);
        string s = Console.ReadLine()?.Trim() ?? "";
        if (s == "") return null;
        if (!s.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) s += ".txt";
        return System.IO.Path.Combine(ProjectFile.Folder, s);
    }

#endregion

#region Класс для txtшек

    // Папка всех проектов: Документы/XANDprojects
    public static string Folder =>
        System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "XANDprojects");

    // Из имени, введённого пользователем, делаем полный путь
    
    /*
    public static string Resolve(string name)
    {
        if (!name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) name += ".txt";
        if (System.IO.Path.IsPathRooted(name)) return name;   // абсолютный путь — не спорим
        return System.IO.Path.Combine(Folder, name);
    }
    */

    public static void Save(string path)
    {
        System.IO.Directory.CreateDirectory(Folder);   // создать, если нет
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# XANDcalc project");
        sb.AppendLine("# version 1");
        sb.AppendLine($"Gate = {Vars.Gate}");
        sb.AppendLine($"# saved {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"VinAuto = {VinAuto}");
        foreach (var p in Vars.Parameters)
            sb.AppendLine($"{p.Name} = {p.Value.ToString("R", System.Globalization.CultureInfo.InvariantCulture)}");
        sb.AppendLine("# --- results at save time (info only) ---");
        foreach (var o in Vars.Outputs)
            sb.AppendLine($"# {o.Name} = {o.Value.ToString("R", System.Globalization.CultureInfo.InvariantCulture)} {o.Unit}");
        System.IO.File.WriteAllText(path, sb.ToString());
    }

    public static int Load(string path)   // возвращает, сколько значений применилось
    {
        int n = 0;
        foreach (var raw in System.IO.File.ReadAllLines(path))
        {
            string line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#")) continue;
            int eq = line.IndexOf('=');
            if (eq < 0) continue;
            string name = line[..eq].Trim();
            string val  = line[(eq + 1)..].Trim();

            if (name == "VinAuto") { if (bool.TryParse(val, out bool b)) VinAuto = b; continue; }
            if (name == "Gate") { Vars.Gate = val; continue; }

            var p = Array.Find(Vars.Parameters, x => x.Name == name);
            if (p == null) continue;                       // имя из новой версии — пропускаем
            double v;
            bool ok = double.TryParse(val, System.Globalization.NumberStyles.Float,
                         System.Globalization.CultureInfo.InvariantCulture, out v)
                   || double.TryParse(val, System.Globalization.NumberStyles.Float,
                         System.Globalization.CultureInfo.CurrentCulture, out v);
            if (ok) { p.Value = v; n++; }
        }
        return n;
    }
}

#endregion