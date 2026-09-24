namespace XANDcalc;


class XAND {

    public const string Ext = ".xand";   // содержимое — текст, расширение — ярлык

    public static bool logo = true;   

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

    static void RegisterXand()
    {
        if (!OperatingSystem.IsWindows()) return;          // на Linux/Mac реестра нет — молча выходим
        try
        {
            string exe = Environment.ProcessPath!;         // путь к нашему собственному exe
            string progId = "XANDcalc.Project";
            using var classes = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Classes", writable: true);
            if (classes == null) return;

            using (var ext = classes.CreateSubKey(".xand"))            // .xand -> ProgID
                ext.SetValue("", progId);

            using (var prog = classes.CreateSubKey(progId))
            {
                prog.SetValue("", "XANDcalc Project File");            // описание типа
                using var icon = prog.CreateSubKey("DefaultIcon");
                icon.SetValue("", $"\"{exe}\",0");                     // иконка = нулевая из самого exe
                using var cmd = prog.CreateSubKey(@"shell\open\command");
                cmd.SetValue("", $"\"{exe}\" \"%1\"");                 // команда запуска с путём к файлу
            }
        }
        catch { /* нет доступа к реестру — не смертельно, живём без ассоциации */ }
    }

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
            OpenGate();
        }
        catch (System.IO.FileNotFoundException) { "File not found in XANDprojects.".Print(Red); }
        catch (System.Exception e) { $"Load failed: {e.Message}".Print(Red); }
    }

    static string? PickSavePath()
    {
#if WIN_DIALOGS
        System.IO.Directory.CreateDirectory(ProjectFile.Folder);
        using var dlg = new System.Windows.Forms.SaveFileDialog
        {
            Title = "Save XANDcalc project",
            Filter = $"XANDcalc project (*{ProjectFile.Ext})|*{ProjectFile.Ext}",
            InitialDirectory = ProjectFile.Folder,
            DefaultExt = "xand",
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
            Filter = $"XANDcalc project (*{ProjectFile.Ext};*.txt)|*{ProjectFile.Ext};*.txt|All files (*.*)|*.*",
            InitialDirectory = ProjectFile.Folder,
            DefaultExt = "xand",
        };
        return dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dlg.FileName : null;
#else
        return AskPathConsole("Load");
#endif
    }

    static string? AskPathConsole(string action)
    {
        $"{action}: file name in XANDprojects (empty = cancel): ".Print(line: false);
        string s = Console.ReadLine()?.Trim() ?? "";
        if (s == "") return null;
        if (!s.Contains('.')) s += ProjectFile.Ext;
        return System.IO.Path.Combine(ProjectFile.Folder, s);
    }

    public static void OpenGate()
    {
        switch (Vars.Gate)
        {
            case "NOT": NOTgate.NOTchoice(); break;
            default: $"This build doesn't know gate '{Vars.Gate}' yet.".Print(Red); break;
        }
    }

    [System.STAThread]
    static void Main(string[] args)
    {
        Console.InputEncoding = System.Text.Encoding.UTF8;
        NOTgate.DeclareNOT();

        RegisterXand();

        if (args.Length > 0 && System.IO.File.Exists(args[0]))
        {
            ProjectFile.Load(args[0]);
            OpenGate();
        }

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
            "2. Save project".Print();
            "3. Load project".Print();

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

                case "2": case "save":
                    Console.Clear();
                    LogoMain();
                    "\n".Print();
                    SaveProject();
                    break;

                case "3": case "load": 
                    Console.Clear();
                    LogoMain();
                    "\n".Print();
                    LoadProject();
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
    
  
}