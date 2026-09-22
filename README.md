<div align="center">

# 💻 XANDcalc

**A console engineering calculator for resistor–transistor logic (RTL).**
**Консольный инженерный калькулятор резисторно-транзисторной логики (RTL).**

[![Release](https://img.shields.io/github/v/release/Tserkovnik/XANDcalc)](https://github.com/Tserkovnik/XANDcalc/releases/latest)
[![License](https://img.shields.io/github/license/Tserkovnik/XANDcalc)](LICENSE)

[English](#english) | [Русский](#russian)

</div>

---

<a name="english"></a>
## English

XANDcalc is a console engineering calculator for **resistor–transistor logic (RTL)**.
It was originally created for the *Create: Power Grid* mod (Minecraft), but it is
fully applicable to real-world electronics as well. The program computes the base
resistor (R_b) a gate needs, together with all of its electrical parameters.

### Inputs

| Symbol  | Meaning                            |
|---------|------------------------------------|
| `Vcc`   | Supply voltage                     |
| `Rc`    | Collector resistor                 |
| `Beta`   | Transistor DC current gain (beta)  |
| `k`     | Base overdrive / saturation factor |
| `FOreq` | Required fan-out                   |

### Current features

- **NOT gate (RTL) calculation:**
  - collector & base saturation currents (I_C, I_B);
  - base resistor R_b selection for the required fan-out;
  - logic levels: V_OH, V_OL, V_IH, V_IL;
  - sink/source and other currents: I_OH, I_OL, I_IH, I_IL;
  - noise margins NM_H / NM_L;
  - fan-out check against the required value.
- **Load table** — the same gate at N = 0 (voltmeter), N = 1 and N = FO_req:
  watch V_OH and the margins degrade as the load grows; marginal cells are
  highlighted in color.
- **Smart output** — automatic current formatting (A → mA → µA → nA → pA);
  red/green highlighting for impossible and healthy operating points.
- **Convenient recalculation** — values entered earlier in the session become
  the defaults; press `Enter` to keep them.

### The math inside

```text
I_C(sat) = (V_CC − V_CE(sat)) / R_C
I_B(req) = I_C(sat) · k_sat / β
R_b      = (V_in − V_BE) / I_B(req) − N · R_C
V_OH(N)  = (R_b·V_CC + N·R_C·V_BE) / (R_b + N·R_C)
V_IH     = V_BE + I_B(req) · R_b
NM_H     = V_OH − V_IH        NM_L = V_IL − V_OL
FO       = floor( I_OH / I_IH )
```


### Controls

| Input      | Meaning                       |
|------------|-------------------------------|
| `<number>` | set the parameter             |
| `0`        | one step back (exit at first) |
| `x` / `q`  | exit to the main menu         |
| `Enter`    | skip, keep the current value  |
| `a`        | auto Vin                      |

### How to run

**Release build (Windows x64):**
1. Download `XANDcalc.exe` from [Releases](https://github.com/Tserkovnik/XANDcalc/releases/latest).
2. Run it. No installation, no dependencies (self-contained).

**From source:**
```bash
git clone https://github.com/Tserkovnik/XANDcalc.git
cd XANDcalc/src
dotnet run
```
Requires .NET 10+.

### Roadmap

- [x] NOT gate
- [ ] Save / load projects to/from `.txt`
- [ ] Power consumption
- [ ] NOR gate
- [ ] NAND gate
- [ ] Arbitrary gates with series/parallel NPN connections
- [ ] hFE variation with temperature and other conditions
- [ ] More features
- [ ] PNP ???

**More ideas than plans:**
- [ ] Comparison table of several configurations
- [ ] Rounding results to real E24 nominals

### Contributing

The project is open to suggestions! Want to add AND / OR / NAND / NOR gates,
improve the visuals, or refine the calculation model? Open an issue or a pull request.

### Note
All code comments will remain in Russian for now.

### License

GPL-3.0 — see [LICENSE](LICENSE).

---

<a name="russian"></a>
## Русский

XANDcalc — консольный инженерный калькулятор **резисторно-транзисторной логики (RTL)**.
Первоначально создан для мода *Create: Power Grid* (Minecraft), но также полностью
применим для расчёта реальной электроники. Программа рассчитывает необходимый
базовый резистор (R_b) гейта и все его электрические параметры.

### Входные данные

| Обозначение | Значение                                        |
|-------------|-------------------------------------------------|
| `Vcc`       | Напряжение питания                              |
| `Rc`        | Коллекторный резистор                           |
| `Beta`       | Коэффициент усиления транзистора по току (beta) |
| `k`         | Коэффициент перегрузки/насыщения базы           |
| `FOreq`     | Требуемый fan-out (разветвление по выходу)      |

### Текущие возможности

- **Расчёт NOT-гейта (RTL):**
  - токи насыщения коллектора и базы (I_C, I_B);
  - подбор базового резистора R_b под требуемый fan-out;
  - логические уровни: V_OH, V_OL, V_IH, V_IL;
  - sink/source и прочие токи: I_OH, I_OL, I_IH, I_IL;
  - запасы помехоустойчивости NM_H / NM_L;
  - проверка fan-out относительно требуемого.
- **Таблица нагрузок** — один и тот же гейт при N = 0 (вольтметр), N = 1 и
  N = FO_req: видно, как деградируют V_OH и запасы с ростом нагрузки;
  плохие ячейки подсвечиваются цветом.
- **Умный вывод** — автоформат токов (А → мА → мкА → нА → пА); красный/зелёный
  для невозможных и здоровых режимов.
- **Удобный перерасчёт** — значения, введённые ранее в течение сеанса, становятся
  значениями по умолчанию: нажмите `Enter`, чтобы оставить их.

### Математика внутри

```text
I_C(sat) = (V_CC − V_CE(sat)) / R_C
I_B(req) = I_C(sat) · k_sat / β
R_b      = (V_in − V_BE) / I_B(req) − N · R_C
V_OH(N)  = (R_b·V_CC + N·R_C·V_BE) / (R_b + N·R_C)
V_IH     = V_BE + I_B(req) · R_b
NM_H     = V_OH − V_IH        NM_L = V_IL − V_OL
FO       = floor( I_OH / I_IH )
```


### Управление

| Ввод            | Значение                              |
|-----------------|---------------------------------------|
| `<число>`       | установить параметр                   |
| `0`             | шаг назад (на первом — выход)         |
| `x` / `q` / `ч` | выход в главное меню                  |
| `Enter`         | пропустить, оставить текущее значение |
| `a`             | автоматический Vin                    |

### Как запустить

**Релизная сборка (Windows x64):**
1. Скачать `XANDcalc.exe` из [Releases](https://github.com/Tserkovnik/XANDcalc/releases/latest).
2. Запустить. Установка и зависимости не нужны (self-contained).

**Из исходников:**
```bash
git clone https://github.com/Tserkovnik/XANDcalc.git
cd XANDcalc/src
dotnet run
```
Нужен .NET 10+.

### Планы

- [x] NOT гейт
- [ ] Сохранение / загрузка проектов в/из `.txt`
- [ ] Энергопотребление
- [ ] NOR гейт
- [ ] NAND гейт
- [ ] Расчёт произвольных гейтов с последовательными и параллельными соединениями NPN
- [ ] Расчёт изменения hFE в зависимости от температуры и прочих условий
- [ ] Больше возможностей
- [ ] PNP ???

**Больше идеи, чем планы:**
- [ ] Таблица сравнения нескольких конфигураций
- [ ] Округление результатов до реальных номиналов E24

### Содействие

Проект открыт предложениям! Хотите добавить AND / OR / NAND / NOR гейты, улучшить
визуал или уточнить модель расчёта? Открывайте issue и pull request.

### Примечание
Все комментарии к коду пока что останутся на русском.

### Лицензия

GPL-3.0 — см. [LICENSE](LICENSE).