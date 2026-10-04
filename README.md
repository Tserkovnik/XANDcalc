`[English](#english) | [Русский](#russian)`

<a name="english"></a>

English

XANDcalc is a console engineering calculator for resistor–transistor logic (RTL). It is designed for precise calculation of discrete semiconductor circuit parameters and in-game schematics for the Create: Power Grid modification (Minecraft). Based on the physical parameters of transistors, the calculator computes the required base resistor rating (Rb), builds static load tables and estimates the thermal balance of the dies.

## Input data

| Notation | Meaning |
| --- | --- |
| `Vcc` | Supply voltage |
| `Rc` | Collector resistor value |
| `Beta` | DC current gain of the transistor (β) |
| `k` | Base overdrive (saturation) factor |
| `FOreq` | Required fan-out |
| `Inputs` | Number of gate inputs |
| `T_amb` | Ambient temperature (per climate biome) |

## Current features

### NOT / Wired NOR hybrid calculation:

- Automatic selection of the `R_b` rating accounting for current distribution in a multi-input base;
- Determination of the actual static collector and base saturation currents `I_C`, `I_B`;
- Tracking of critical logic-level degradation and noise margins as the input count grows.

### Complex gate modelling (NAND / NOR):

- Layer-by-layer calculation of series transistor stacks with floating emitter potentials;
- Parallel cascades.

### Steady-state thermal analysis:

- Joule heating estimation for every resistor and semiconductor junction;
- Worst-case thermal scenario modelling (logic "1" applied to all inputs simultaneously).

### Integration tools:

- Interactive parameter degradation table for `N = 0`, `N = 1` and `N = FOreq`;
- Standalone project save/load system using `.xand` files with invariant culture.

> [!IMPORTANT]
> Logic levels and load capability are calculated under the convention of a fully homogeneous environment: the outputs and inputs of the gate under study are assumed to be connected to identical RTL gates.

## Mathematical framework

<details>
<summary>Expand the full summary of physical and thermal formulas</summary>

### 1. General constants and transistor equations (Ebers–Moll and Shockley models)

Collector–emitter saturation voltage `V_CE(sat)`, collector saturation current `I_C(sat)` and the minimum required base current `I_B(req)`:

$$
V_{CE(sat)} = V_t \cdot \ln\left(
\frac{
\frac{\beta}{\beta_R} + k \cdot \left(1 + \frac{1}{\beta_R}\right)
}{
k - 1
}
\right)
$$

$$
I_{C(sat)} = \frac{V_{CC} - V_{CE(sat)}}{R_C},
\quad
I_{B(req)} = \frac{I_{C(sat)} \cdot k}{\beta}
$$

Actual base–emitter junction voltage drop `V_BE` (Shockley equation including the drop across the bulk ohmic emitter resistance `R_s`):

$$
V_{BE} =
V_t \cdot \ln\left(\frac{I_{C(sat)}}{I_s} + 1\right)
+
I_{C(sat)} \cdot \left(1 + \frac{1}{\beta}\right) \cdot R_s
$$

---

### 2. NOT gate / resistor-based Wired NOR (single shared transistor)

Base resistor `R_b` calculation accounting for base shunting by `(n - 1)` closed neighbour inputs and the systematic stage load `FO_req`:

$$
V_{eff} = (V_{in} \cdot 0.9) - n \cdot V_{BE} + (n - 1) \cdot V_{OL}
$$

$$
R_b = \frac{V_{eff}}{I_{B(req)}} - R_C \cdot FO_{req}
$$

Input switching thresholds (`V_IH` worst-case leakage, cut-off threshold `V_IL`):

$$
V_{IH} = n \cdot V_{BE} - (n - 1) \cdot V_{OL} + I_{B(req)} \cdot R_b
$$

$$
V_{IL} = V_t \cdot \ln\left(\frac{V_t}{R_C \cdot I_s}\right)
$$

Output high level `V_OH` of the gate under an arbitrary static load of `N` elements and the sink current of a logic one `I_IH`:

$$
V_{OH}(N) =
\frac{R_b \cdot V_{CC} + N \cdot R_C \cdot V_{BE}}{R_b + N \cdot R_C},
\quad
I_{IH} = \frac{V_{OH} - V_{BE}}{R_b + R_C}
$$

---

### 3. NOR gate (shared `R_C`, parallel transistors)

Worst-case turn-on — exactly one transistor is open, single-handedly sinking the entire current of the shared collector resistor `R_C`:

$$
R_{b(i)} = \frac{(V_{in} \cdot 0.9) - V_{BE(i)}}{I_{B(req)(i)}} - R_C \cdot FO_{req}
$$

$$
V_{OH}(N) =
\frac{R_{b(i)} \cdot V_{CC} + N \cdot R_C \cdot V_{BE(i)}}{R_{b(i)} + N \cdot R_C},
\quad
I_{IH} =
\frac{V_{OH} - V_{BE(g)}}{R_{b(g)} + R_C}
$$

$$
V_{IH} =
\max_i
\left(
V_{BE(i)} + I_{B(req)(i)} \cdot R_{b(i)}
\right)
$$

---

### 4. NAND gate (transistor stack / series connection)

The emitter potential of each transistor in the stack `V_e(i)` is defined by the sum of `V_CE(sat)` of all transistors below it in the chain down to ground. The collector current of the lower transistors grows due to the summation of the base currents of the upper stages:

$$
V_{e(i)} =
\sum_{j=i+1}^{n} V_{CE(sat)(j)},
\quad
I_{C(sat)(i)} =
I_{C(sat)(top)} +
\sum_{j=1}^{i-1} I_{B(req)(j)}
$$

Individual `R_b(i)` calculation for each stack input accounting for the dynamic emitter lift:

$$
R_{b(i)} = \frac{(V_{in} \cdot 0.9) - V_{e(i)} - V_{BE(i)}}{I_{B(req)(i)}} - R_C \cdot FO_{req}
$$

Stage switching threshold `V_IH` and the `V_OH(N)` level referred to the bottom transistor:

$$
V_{IH} =
\max_i
\left(
V_{e(i)} + V_{BE(i)} + I_{B(req)(i)} \cdot R_{b(i)}
\right),
\quad
V_{OH}(N) =
\frac{
R_{b(1)} \cdot V_{CC} + N \cdot R_C \cdot V_{BE(1)}
}{
R_{b(1)} + N \cdot R_C
}
$$

---

### 5. Output currents, fan-out and noise margins

$$
I_{OH} = \frac{V_{CC} - V_{OH}}{R_C},
\quad
I_{OL} = \frac{V_{CC} - V_{OL}}{R_C},
\quad
I_{OL(spare)} = I_{C(sat)} \cdot (k_{min} - 1)
$$

$$
FO =
\left\lfloor
\frac{I_{OH}}{I_{IH}}
\right\rfloor,
\quad
NM_H = V_{OH} - V_{IH},
\quad
NM_L = V_{IL} - V_{OL}
$$

---

### 6. Steady-state thermal analysis (Worst Case Steady-State)

Heat dissipation on the resistors and transistor dies at maximum current load, and the resulting component temperatures:

$$
P_{Rc} = I_{OL}^2 \cdot R_C,
\quad
P_{Rb} = I_B^2 \cdot R_b
$$

$$
P_{tr(NOT)} =
V_{CE(sat)} \cdot I_{C(sat)} +
V_{BE} \cdot (n \cdot I_B),
\quad
P_{tr(NAND/NOR)} =
V_{CE(sat)} \cdot I_{C(sat)} +
V_{BE} \cdot I_B
$$

$$
T_{Rc} =
T_{amb} + \frac{P_{Rc}}{D_{Res}},
\quad
T_{Rb} =
T_{amb} + \frac{P_{Rb}}{D_{Res}},
\quad
T_{tr} =
T_{amb} + \frac{P_{tr}}{D_{Tr}}
$$

</details>

## Controls

| Input | Action |
| --- | --- |
| `<number>` | Set the value of the selected parameter |
| `0` | One step back through the input stages (on the first stage — exit) |
| `x` / `q` | Immediate return to the main menu |
| `Enter` | Skip the step, keeping the current default value |
| `a` | Toggle automatic `Vin` calculation mode |

## How to run

### Release build (Windows x64):

1. Go to the [Releases](https://github.com) section and download the `XANDcalc.exe` file.
2. Run it. The build is portable (self-contained) and requires no dependency or runtime installation.

### Building from source:

```bash
git clone https://github.com
cd XANDcalc/src
dotnet run -r win-x64
```

> [!NOTE]
> Compiling from source requires the .NET 10+ SDK to be installed.

## Roadmap

- [x] Basic NOT inverter calculation
- [x] Automatic saving and loading of `.xand` project files
- [x] Gate power consumption calculation
- [x] Multi-input NOR gate calculation (wired base and multi-transistor designs)
- [x] NAND gate calculation (series transistor stack)
- [x] Thermal operating mode modelling and biome dependence
- [ ] Rounding of calculated resistances to standard E24 nominal series
- [ ] Integration of a consolidated configuration comparison table into the console UI
- [ ] Adding mathematical models for PNP structures

## Contributing

The repository is open to pull requests and suggestions for refining the physical models. If you want to propose changes to the calculation algorithms or the console graphics, please open a new Issue.

## Note

All comments inside the program's source code remain strictly in Russian.

## AI usage

YES, AI was used in this project, but not for thoughtless "vibe coding". All code was reviewed by a human, and all code was integrated by a human.

This does not affect the operation of the program in any way.

## License

The project is distributed under the free GPL-3.0 license — see the [LICENSE](LICENSE) file for detailed terms.





<a name="russian"></a>

Русский

XANDcalc — консольный инженерный калькулятор резисторно-транзисторной логики (RTL). Программа предназначена для точного расчёта параметров дискретной полупроводниковой электроники и внутриигровых схем модификации Create: Power Grid (Minecraft). На основе физических параметров транзисторов калькулятор вычисляет необходимый номинал базового резистора (Rb), формирует таблицы статических нагрузок и оценивает тепловой баланс кристаллов.

## Входные данные

| Обозначение | Значение |
| --- | --- |
| `Vcc` | Напряжение питания схемы |
| `Rc` | Сопротивление коллекторного резистора |
| `Beta` | Статический коэффициент усиления транзистора по току (β) |
| `k` | Коэффициент перегрузки (насыщения) базы |
| `FOreq` | Требуемый коэффициент разветвления по выходу (fan-out) |
| `Inputs` | Количество входов логического элемента |
| `T_amb` | Температура окружающей среды (расчёт по климатическим биомам) |

## Текущие возможности

### Расчёт универсального гибрида NOT / Wired NOR:

- Автоматический подбор номинала `R_b` с учётом распределения токов в многовходовой базе;
- Определение реальных статических токов насыщения коллектора и базы `I_C`, `I_B`;
- Фиксация критического падения логических уровней и запасов помехоустойчивости при росте числа входов.

### Моделирование сложных вентилей (NAND / NOR):

- Послойный расчёт последовательных транзисторных структур с вычислением плавающих потенциалов эмиттеров;
- Параллельное объединение каскадов.

### Стационарный тепловой расчёт:

- Оценка рассеиваемой мощности на каждом резисторе и полупроводниковом переходе по закону Джоуля-Ленца;
- Моделирование худшего теплового сценария (одновременная подача логических «1» на все входы).

### Инструменты интеграции:

- Интерактивная таблица деградации параметров при `N = 0`, `N = 1` и `N = FOreq`;
- Автономная система сохранения и загрузки проектов в файлы `.xand` с использованием инвариантной культуры.

> [!IMPORTANT]
> Расчёт логических уровней и нагрузочной способности производится исходя из конвенции полной однородности среды: предполагается, что к выходам и входам исследуемого элемента подключены идентичные ему RTL-вентили.

## Математический аппарат

<details>
<summary>Развернуть полный свод физических и тепловых формул</summary>

### 1. Общие константы и уравнения транзистора (Модель Эберса-Молла и Шокли)

Напряжение насыщения коллектор-эмиттер `V_CE(sat)`, ток насыщения коллектора `I_C(sat)` и минимально необходимый ток базы `I_B(req)`:

$$
V_{CE(sat)} = V_t \cdot \ln\left(
  \frac{
    \frac{\beta}{\beta_R} + k \cdot \left(1 + \frac{1}{\beta_R}\right)
  }{
    k - 1
  }
\right)
$$

$$
I_{C(sat)} = \frac{V_{CC} - V_{CE(sat)}}{R_C},
\quad
I_{B(req)} = \frac{I_{C(sat)} \cdot k}{\beta}
$$

Реальное падение напряжения на переходе база-эмиттер `V_BE` (уравнение Шокли с учётом падения на объёмном омическом сопротивлении эмиттера `R_s`):

$$
V_{BE} =
V_t \cdot \ln\left(\frac{I_{C(sat)}}{I_s} + 1\right)
+
I_{C(sat)} \cdot \left(1 + \frac{1}{\beta}\right) \cdot R_s
$$

---

### 2. Вентиль NOT / Резисторный Wired NOR (один общий транзистор)

Расчёт базового резистора `R_b` с учётом шунтирования базы `(n - 1)` закрытыми входами соседей и системной нагрузки каскада `FO_req`:

$$
V_{eff} = (V_{in} \cdot 0.9) - n \cdot V_{BE} + (n - 1) \cdot V_{OL}
$$

$$
R_b = \frac{V_{eff}}{I_{B(req)}} - R_C \cdot FO_{req}
$$

Входные пороги переключения элементов (`V_IH` худшего случая утечки, порог запирания `V_IL`):

$$
V_{IH} = n \cdot V_{BE} - (n - 1) \cdot V_{OL} + I_{B(req)} \cdot R_b
$$

$$
V_{IL} = V_t \cdot \ln\left(\frac{V_t}{R_C \cdot I_s}\right)
$$

Выходной высокий уровень `V_OH` гейта под произвольной статической нагрузкой из `N` элементов и втекающий ток единицы `I_IH`:

$$
V_{OH}(N) =
\frac{R_b \cdot V_{CC} + N \cdot R_C \cdot V_{BE}}{R_b + N \cdot R_C},
\quad
I_{IH} = \frac{V_{OH} - V_{BE}}{R_b + R_C}
$$

---

### 3. Вентиль NOR (общий `R_C`, параллельные транзисторы)

Худший случай включения — открыт строго один транзистор, который в одиночку стягивает весь ток общего коллекторного резистора `R_C`:

$$
R_{b(i)} = \frac{(V_{in} \cdot 0.9) - V_{BE(i)}}{I_{B(req)(i)}} - R_C \cdot FO_{req}
$$

$$
V_{OH}(N) =
\frac{R_{b(i)} \cdot V_{CC} + N \cdot R_C \cdot V_{BE(i)}}{R_{b(i)} + N \cdot R_C},
\quad
I_{IH} =
\frac{V_{OH} - V_{BE(g)}}{R_{b(g)} + R_C}
$$

$$
V_{IH} =
\max_i
\left(
  V_{BE(i)} + I_{B(req)(i)} \cdot R_{b(i)}
\right)
$$

---

### 4. Вентиль NAND (транзисторный стек / последовательное соединение)

Потенциал эмиттера каждого транзистора в стеке `V_e(i)` определяется суммой `V_CE(sat)` всех транзисторов, находящихся ниже него по цепи до земли. Ток коллектора нижних транзисторов растёт из-за суммирования базовых токов верхних каскадов:

$$
V_{e(i)} =
\sum_{j=i+1}^{n} V_{CE(sat)(j)},
\quad
I_{C(sat)(i)} =
I_{C(sat)(top)} +
\sum_{j=1}^{i-1} I_{B(req)(j)}
$$

Расчёт индивидуального `R_b(i)` для каждого входа стека с учётом динамического подъёма эмиттеров:

$$
R_{b(i)} = \frac{(V_{in} \cdot 0.9) - V_{e(i)} - V_{BE(i)}}{I_{B(req)(i)}} - R_C \cdot FO_{req}
$$

Порог переключения каскада `V_IH` и уровень `V_OH(N)` по базовому транзистору:

$$
V_{IH} =
\max_i
\left(
  V_{e(i)} + V_{BE(i)} + I_{B(req)(i)} \cdot R_{b(i)}
\right),
\quad
V_{OH}(N) =
\frac{
  R_{b(1)} \cdot V_{CC} + N \cdot R_C \cdot V_{BE(1)}
}{
  R_{b(1)} + N \cdot R_C
}
$$

---

### 5. Выходные токи, коэффициент разветвления (Fan-Out) и запасы помехоустойчивости

$$
I_{OH} = \frac{V_{CC} - V_{OH}}{R_C},
\quad
I_{OL} = \frac{V_{CC} - V_{OL}}{R_C},
\quad
I_{OL(spare)} = I_{C(sat)} \cdot (k_{min} - 1)
$$

$$
FO =
\left\lfloor
  \frac{I_{OH}}{I_{IH}}
\right\rfloor,
\quad
NM_H = V_{OH} - V_{IH},
\quad
NM_L = V_{IL} - V_{OL}
$$

---

### 6. Стационарный тепловой расчёт (Worst Case Steady-State)

Тепловыделение на резисторах и кристаллах транзисторов при максимальной токовой нагрузке, а также результирующие температуры элементов:

$$
P_{Rc} = I_{OL}^2 \cdot R_C,
\quad
P_{Rb} = I_B^2 \cdot R_b
$$

$$
P_{tr(NOT)} =
V_{CE(sat)} \cdot I_{C(sat)} +
V_{BE} \cdot (n \cdot I_B),
\quad
P_{tr(NAND/NOR)} =
V_{CE(sat)} \cdot I_{C(sat)} +
V_{BE} \cdot I_B
$$

$$
T_{Rc} =
T_{amb} + \frac{P_{Rc}}{D_{Res}},
\quad
T_{Rb} =
T_{amb} + \frac{P_{Rb}}{D_{Res}},
\quad
T_{tr} =
T_{amb} + \frac{P_{tr}}{D_{Tr}}
$$

</details>

## Управление

| Ввод | Значение |
| --- | --- |
| `<число>` | Установить значение выбранного параметра |
| `0` | Шаг назад по этапам ввода (на первом этапе — выход) |
| `x` / `q` / `ч` | Немедленный возврат в главное меню программы |
| `Enter` | Пропустить шаг, сохранив текущее значение по умолчанию |
| `a` | Переключить расчёт напряжения `Vin` в автоматический режим |

## Как запустить

### Релизная сборка (Windows x64):

1. Перейдите в раздел [Releases](https://github.com) и скачайте файл `XANDcalc.exe`.
2. Запустите файл. Сборка является портативной (self-contained) и не требует установки зависимостей или среды выполнения.

### Сборка из исходного кода:

```bash
git clone https://github.com
cd XANDcalc/src
dotnet run -r win-x64
```
> [!NOTE]
> Для компиляции исходного кода требуется установленный SDK **.NET 10+**.

### План разработки (Roadmap)

- [x] Расчёт базового NOT-инвертора
- [x] Автоматическое сохранение и чтение проектов в файлы `.xand`
- [x] Расчёт потребляемой мощности элементов
- [x] Расчёт многовходовых вентилей NOR (Wired-база и многотранзисторные схемы)
- [x] Расчёт вентилей NAND (последовательный транзисторный стек)
- [x] Моделирование тепловых режимов работы и зависимости от биомов
- [ ] Округление расчётных сопротивлений до стандартных номинальных рядов E24
- [ ] Интеграция общей сравнительной таблицы конфигураций в консольный интерфейс
- [ ] Добавление математических моделей для PNP-структур

### Содействие

Репозиторий открыт для pull-request и предложений по уточнению физических моделей. Если вы хотите предложить изменения в алгоритмах расчёта или консольной графике, создайте новую задачу (Issue).

### Примечание
Все комментарии внутри исходного кода программы остаются строго на русском языке.

### Об использовании ИИ
ДА, в проекте использовался ИИ, но не для бездумного "вайбкодинга". Весь код перепроверен человеком, весь код встраивался человеком. 
Это никак не влияет на работу прогроммы.

### Лицензия

Проект распространяется под свободной лицензией **GPL-3.0** — подробные условия изложены в файле [LICENSE](LICENSE).
