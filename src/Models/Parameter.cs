namespace XANDcalc;

//Универсальный класс для любого параметра (входного или выходного)
class Parameter
{
    public string Name { get; }
    public double Value { get; set; }
    public string Unit { get; }
    public string Tag { get; } // "all", "t1", "t2", "out" (для фильтрации в циклах ввода)

    public Parameter(string name, double value, string unit, string tag)
    {
        Name = name;
        Value = value;
        Unit = unit;
        Tag = tag;
    }
}