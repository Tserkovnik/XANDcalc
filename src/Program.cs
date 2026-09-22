namespace XANDcalc;


class XAND {

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

    [System.STAThread]
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