using System.Diagnostics;

namespace Tetris;

public class TetrisGame
{
    private GameField _field = new();
    private Tetromino? _current;
    private Tetromino? _next;
    private readonly Random _rnd = new();
    private int _score;
    private int _best;
    private int _fallDelay = 500;

    // Менеджер рекордов и текущий «лучший» счёт.
    private readonly ScoreManager _scores = new();

    /// <summary>
    /// Точка входа: главное меню. Пока пользователь не выберет «Выход» —
    /// крутимся здесь, вызывая либо игру, либо таблицу рекордов.
    /// </summary>
    public void Run()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== ТЕТРИС ===");
            Console.WriteLine();
            Console.WriteLine("1. Новая игра");
            Console.WriteLine("2. Таблица рекордов");
            Console.WriteLine("3. Выход");
            Console.WriteLine();
            Console.Write("Выбор: ");

            var key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    PlaySession();
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    _scores.ShowTable();
                    break;
                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                case ConsoleKey.Escape:
                    return;
            }
        }
    }

    /// <summary>
    /// Одна «сессия» — может включать несколько партий подряд,
    /// пока игрок нажимает Y после проигрыша. Счёт в рекорды пишется за каждую партию.
    /// </summary>
    private void PlaySession()
    {
        // Загружаем лучший результат один раз в начале сессии.
        var table = _scores.Load();
        _best = table.Count > 0 ? table[0].Score : 0;

        while (true)
        {
            StartNewGame();
            GameLoop(); // играем до проигрыша или Esc

            // Игрок вышел по Esc — без записи в рекорды.
            bool abortedByEsc = _current != null && !_field.CanPlace(_current, _current.X, _current.Y);

            // Записываем результат, если партия завершилась проигрышем.
            if (!abortedByEsc)
            {
                var (list, rank) = _scores.AddScore(_score);
                _best = list.Count > 0 ? list[0].Score : _best;

                Console.SetCursorPosition(0, GameField.Height + 4);
                Console.WriteLine($"Игра окончена! Счёт: {_score}. Рекорд: {_best}.   ");
                if (rank > 0)
                    Console.WriteLine($"Ваш результат попал в топ на {rank} место!   ");
                else
                    Console.WriteLine("В топ не попали, но попробуйте ещё!   ");
            }
            else
            {
                Console.SetCursorPosition(0, GameField.Height + 4);
                Console.WriteLine($"Вы вышли из партии. Счёт: {_score}.   ");
            }

            Console.Write("Начать заново? (y/n): ");
            var key = Console.ReadKey(true).Key;
            if (key != ConsoleKey.Y) break;
        }
    }

    private void StartNewGame()
    {
        _field = new GameField();
        _score = 0;
        _current = Tetromino.CreateRandom(_rnd);
        _next = Tetromino.CreateRandom(_rnd);
    }

    private void GameLoop()
    {
        Console.CursorVisible = false;
        Stopwatch sw = Stopwatch.StartNew();
        long lastFall = 0;

        while (true)
        {
            // 1. Автопадение
            if (sw.ElapsedMilliseconds - lastFall >= _fallDelay)
            {
                lastFall = sw.ElapsedMilliseconds;

                if (!TryMove(0, 1))
                {
                    _field.LockPiece(_current!);
                    int cleared = _field.ClearFullLines();
                    _score += cleared * 100;

                    _current = _next;
                    _next = Tetromino.CreateRandom(_rnd);
                    _current!.X = 3;
                    _current.Y = 0;

                    // Конец игры — новая фигура не помещается.
                    if (!_field.CanPlace(_current, _current.X, _current.Y))
                        return;
                }
            }

            // 2. Управление
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.LeftArrow: TryMove(-1, 0); break;
                    case ConsoleKey.RightArrow: TryMove(1, 0); break;
                    case ConsoleKey.DownArrow: TryMove(0, 1); break;
                    case ConsoleKey.UpArrow: TryRotate(); break;
                    case ConsoleKey.Spacebar:
                        while (TryMove(0, 1)) { }
                        break;
                    case ConsoleKey.Escape:
                        return;
                }
            }

            // 3. Отрисовка
            _field.Draw(_current, _next);
            DrawHud();

            Thread.Sleep(20);
        }
    }

    /// <summary>
    /// Отрисовка счёта/рекорда/подсказок под полем.
    /// Использует SetCursorPosition, чтобы не «прыгать» кадром.
    /// </summary>
    private void DrawHud()
    {
        int baseRow = GameField.Height + 2;

        Console.SetCursorPosition(0, baseRow);
        Console.WriteLine($"Счёт: {_score}   ");
        Console.SetCursorPosition(0, baseRow + 1);
        Console.WriteLine($"Рекорд: {_best}   ");
        Console.SetCursorPosition(0, baseRow + 2);
        Console.WriteLine("← → движение  ↑ поворот  ↓ вниз  Пробел сброс  Esc выход   ");
    }

    private bool TryMove(int dx, int dy)
    {
        if (_field.CanPlace(_current!, _current!.X + dx, _current.Y + dy))
        {
            _current.X += dx;
            _current.Y += dy;
            return true;
        }
        return false;
    }

    private void TryRotate()
    {
        var clone = _current!.Clone();
        clone.Rotate();
        if (_field.CanPlace(clone, clone.X, clone.Y))
            _current = clone;
    }
}