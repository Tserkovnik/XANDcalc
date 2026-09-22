namespace XANDcalc;

class Validation
{
    // Хард: значение бессмысленно -> переспросить
    public static string? HardLimit(string name, double v)
    {
        if ((name == "kSat1" || name == "kSat2") && v <= 1)
            return "k <= 1: no base overdrive — no saturation, the transistor is not a switch. Enter k > 1";
        if (name == "Required Fan-Out" && v < 1)
            return "Fan-Out < 1 makes no sense. Enter a natural number";
        return null;
    }

    // Софт: допустимо, но подозрительно -> принять с предупреждением
    public static string? SoftWarn(string name, double v)
    {
        if ((name == "Beta1" || name == "Beta2") && (v < 5 || v > 100))
            return "Note: Create: Power Grid clamps gain to 5..100";
        if ((name == "Rc1" || name == "Rc2") && v < 100)
            return "Warning: very small Rc — huge current and power";
        if (name == "Vin" && v > Vcc)
            return "Warning: input high is above supply Vcc — check your level shifting";
        return null;
    }
}