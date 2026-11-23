namespace Tetris;

/// <summary>
/// Класс игрового поля.
/// Хранит матрицу Cells[Height, Width]:
///   0 — пустая клетка,
///   1 — клетка, занятая «застывшим» блоком.
/// Текущая падающая фигура в Cells НЕ хранится — она рисуется поверх в методе Draw.
///
/// ВАЖНО: в консоли символ '█' по ширине уже, чем высота строки,
/// поэтому одна игровая клетка рисуется CellWidth символами подряд.
/// Так клетки выглядят квадратными.
/// </summary>
public class GameField
{
    // Размеры поля в игровых клетках.
    public const int Width = 10;
    public const int Height = 20;

    // Сколько символов консоли занимает одна игровая клетка по горизонтали.
    // 2 — компенсирует то, что строки в консоли выше, чем ширина символа.
    public const int CellWidth = 2;

    // Фиксированный бокс под «следующую» фигуру (в игровых клетках).
    private const int NextBoxSize = 4;

    // Отступ между полем и правой колонкой (в символах консоли).
    private const int RightGap = 3;

    public int[,] Cells { get; private set; }

    public GameField()
    {
        Cells = new int[Height, Width];
    }

    public void Clear()
    {
        Cells = new int[Height, Width];
    }

    public bool CanPlace(Tetromino piece, int x, int y)
    {
        int rows = piece.Shape.GetLength(0);
        int cols = piece.Shape.GetLength(1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (!piece.Shape[r, c]) continue;

                int fieldRow = y + r;
                int fieldCol = x + c;

                if (fieldCol < 0 || fieldCol >= Width) return false;
                if (fieldRow >= Height) return false;
                if (fieldRow >= 0 && Cells[fieldRow, fieldCol] != 0) return false;
            }
        }
        return true;
    }

    public void LockPiece(Tetromino piece)
    {
        int rows = piece.Shape.GetLength(0);
        int cols = piece.Shape.GetLength(1);

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (piece.Shape[r, c])
                {
                    int fr = piece.Y + r;
                    int fc = piece.X + c;
                    if (fr >= 0 && fr < Height && fc >= 0 && fc < Width)
                        Cells[fr, fc] = 1;
                }
    }

    public int ClearFullLines()
    {
        int cleared = 0;
        for (int r = Height - 1; r >= 0; r--)
        {
            bool full = true;
            for (int c = 0; c < Width; c++)
                if (Cells[r, c] == 0) { full = false; break; }

            if (full)
            {
                for (int rr = r; rr > 0; rr--)
                    for (int c = 0; c < Width; c++)
                        Cells[rr, c] = Cells[rr - 1, c];

                for (int c = 0; c < Width; c++)
                    Cells[0, c] = 0;

                cleared++;
                r++;
            }
        }
        return cleared;
    }

    /// <summary>
    /// Вспомогательный метод: рисует одну игровую клетку CellWidth символами.
    /// filled = true → блок, false → пусто.
    /// </summary>
    private static void WriteCell(bool filled)
    {
        Console.Write(filled
            ? new string('█', CellWidth)
            : new string(' ', CellWidth));
    }

    /// <summary>
    /// Отрисовка поля + падающей фигуры + блока «Следующая» справа.
    /// Каждая клетка занимает CellWidth символов, чтобы быть визуально квадратной.
    /// </summary>
    public void Draw(Tetromino? current, Tetromino? next)
    {
        Console.SetCursorPosition(0, 0);

        // Полная ширина поля в символах консоли.
        int fieldWidthChars = Width * CellWidth;

        // Ширина правой колонки в символах:
        //   NextBoxSize * CellWidth (бокс) + 1 (запасной пробел справа)
        int nextBoxChars = NextBoxSize * CellWidth;
        int rightWidth = nextBoxChars + 1;

        // ── Верхняя рамка + заголовок «Следующая:» ──
        Console.Write("+" + new string('-', fieldWidthChars) + "+");
        Console.Write(new string(' ', RightGap));
        if (next != null)
            Console.Write("Следующая:".PadRight(rightWidth));
        else
            Console.Write(new string(' ', rightWidth));
        Console.WriteLine();

        // Центрирование матрицы следующей фигуры внутри бокса
        // (в игровых клетках, не в символах).
        int nextPadLeft = 0;
        int nextPadTop = 0;
        int nextShapeRows = 0;
        int nextShapeCols = 0;

        if (next != null)
        {
            nextShapeRows = next.Shape.GetLength(0);
            nextShapeCols = next.Shape.GetLength(1);
            nextPadLeft = (NextBoxSize - nextShapeCols) / 2;
            nextPadTop = (NextBoxSize - nextShapeRows) / 2;
        }

        // ── Строки поля ──
        for (int r = 0; r < Height; r++)
        {
            // Левая стенка.
            Console.Write("|");

            // Поле + падающая фигура.
            for (int c = 0; c < Width; c++)
            {
                bool isPiece = false;
                if (current != null)
                {
                    int pr = r - current.Y;
                    int pc = c - current.X;
                    if (pr >= 0 && pr < current.Shape.GetLength(0) &&
                        pc >= 0 && pc < current.Shape.GetLength(1) &&
                        current.Shape[pr, pc])
                        isPiece = true;
                }

                // Каждая игровая клетка рисуется CellWidth символами.
                WriteCell(isPiece || Cells[r, c] != 0);
            }

            // Правая стенка.
            Console.Write("|");

            // Отступ до правой колонки.
            Console.Write(new string(' ', RightGap));

            // Правая колонка: бокс «Следующая».
            if (next != null)
            {
                int boxRow = r;

                if (boxRow < nextPadTop || boxRow >= nextPadTop + nextShapeRows)
                {
                    // Строка бокса вне матрицы фигуры — просто пустые клетки.
                    for (int i = 0; i < NextBoxSize; i++) WriteCell(false);
                }
                else
                {
                    int shapeRow = boxRow - nextPadTop;

                    // Левый паддинг.
                    for (int i = 0; i < nextPadLeft; i++) WriteCell(false);

                    // Сама строка матрицы.
                    for (int c = 0; c < nextShapeCols; c++)
                        WriteCell(next.Shape[shapeRow, c]);

                    // Правый паддинг.
                    int rightPad = NextBoxSize - nextPadLeft - nextShapeCols;
                    for (int i = 0; i < rightPad; i++) WriteCell(false);
                }

                // Запасной пробел, чтобы затирать остатки прошлого кадра.
                Console.Write(" ");
            }
            else
            {
                // Если следующей фигуры нет — затираем всю правую колонку.
                Console.Write(new string(' ', rightWidth));
            }

            Console.WriteLine();
        }

        // ── Нижняя рамка + добивка правой колонки ──
        Console.Write("+" + new string('-', fieldWidthChars) + "+");
        Console.Write(new string(' ', RightGap + rightWidth));
        Console.WriteLine();
    }
}