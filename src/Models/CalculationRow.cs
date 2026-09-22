namespace XANDcalc;

//Класс-контейнер для одной строки сравнительной таблицы результатов (по Fan-Out)
class CalculationRow
{
    public int CurrentFO { get; set; } // Для какого Fan-Out сделан этот конкретный расчет
    
    // Внутри каждой строки таблицы лежит свой независимый набор выходных параметров
    public Parameter[] OutputParams { get; }

    public CalculationRow(int currentFO, double rb, double voh, double vol, double vih)
    {
        CurrentFO = currentFO;
        OutputParams = [
            new Parameter("Rb", rb, "Ohm", "out"),
            new Parameter("V_oh", voh, "V", "out"),
            new Parameter("V_ol", vol, "V", "out"),
            new Parameter("V_ih", vih, "V", "out")
        ];
    }

    // Удобный поиск значения внутри строки таблицы по имени параметра
    public double GetVal(string name) => 
        Array.Find(OutputParams, p => p.Name == name)?.Value ?? 0;
}