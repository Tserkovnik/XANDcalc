namespace XANDcalc;

//Универсальный класс для любого параметра (входного или выходного)
class Parameter
{
    public string Name { get; }
    public double Value { get; set; }
    public string Unit { get; }
    public string Tag { get; }    // "all", "t1", "t2"... — группировка ввода
    public string Kind { get; }   // "in" — вводит пользователь, "out" — считает программа
    public Parameter(string name, double value, string unit, string tag, string kind = "in")
    { Name = name; Value = value; Unit = unit; Tag = tag; Kind = kind; }
}