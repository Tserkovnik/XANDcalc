namespace XANDcalc;

// Именованный взгляд на транзистор №I. Своих данных не хранит:
// всё живёт в Vars, поэтому рассинхронизироваться с базой не может.
class Transistor
{
    public int I { get; }
    public Transistor(int i) { I = i; }

    // входы
    public double Rc   => Vars.Val($"Rc{I}");
    public double Beta => Vars.Val($"Beta{I}");
    public double k    => Vars.Val($"kSat{I}");

    // расчётные
    public double Rb     { get => Vars.Val($"Rb{I}");     set => Vars.Set($"Rb{I}", value); }
    public double Vbe    { get => Vars.Val($"Vbe{I}");    set => Vars.Set($"Vbe{I}", value); }
    public double Vcesat { get => Vars.Val($"Vcesat{I}"); set => Vars.Set($"Vcesat{I}", value); }
    public double Icsat  { get => Vars.Val($"Icsat{I}");  set => Vars.Set($"Icsat{I}", value); }
    public double Ibsat  { get => Vars.Val($"Ibsat{I}");  set => Vars.Set($"Ibsat{I}", value); }

    public double BetaR => Math.Max(0.5, Beta * 0.1);   // обратная β, как в моде
}