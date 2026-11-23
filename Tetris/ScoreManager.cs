using System.Text.Json; // для сериализации списка рекордов в JSON

namespace Tetris;

/// <summary>
/// Одна запись таблицы рекордов.
/// </summary>
public class ScoreEntry
{
    public int Score { get; set; }
    public string Date { get; set; } = ""; // дата в ISO-формате (yyyy-MM-dd HH:mm)
}

/// <summary>
/// Класс для чтения/записи таблицы рекордов в файл.
/// Хранит не больше MaxRecords записей, отсортированных по убыванию счёта.
/// Файл — простой JSON рядом с исполняемым файлом (scores.json).
/// </summary>
public class ScoreManager
{
    public const int MaxRecords = 10; // сколько рекордов хранить
    private readonly string _path;    // путь к файлу рекордов

    public ScoreManager(string path = "scores.json")
    {
        _path = path;
    }

    /// <summary>
    /// Загрузить таблицу рекордов. Если файла нет или он битый — вернуть пустой список.
    /// </summary>
    public List<ScoreEntry> Load()
    {
        try
        {
            if (!File.Exists(_path)) return new List<ScoreEntry>();
            string json = File.ReadAllText(_path);
            var list = JsonSerializer.Deserialize<List<ScoreEntry>>(json);
            return list ?? new List<ScoreEntry>();
        }
        catch
        {
            // Любая ошибка чтения — считаем, что рекордов нет.
            // Так игра точно не упадёт из-за повреждённого файла.
            return new List<ScoreEntry>();
        }
    }

    /// <summary>
    /// Добавить результат в таблицу. Если он попадает в топ — сохраняет файл.
    /// Возвращает обновлённый список и место (1-based), либо 0, если не попал в топ.
    /// </summary>
    public (List<ScoreEntry> list, int rank) AddScore(int score)
    {
        var list = Load();

        var entry = new ScoreEntry
        {
            Score = score,
            Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
        };

        list.Add(entry);

        // Сортируем по убыванию очков, при равенстве — свежая запись выше.
        list = list.OrderByDescending(s => s.Score).ThenByDescending(s => s.Date).ToList();

        // Обрезаем до MaxRecords.
        if (list.Count > MaxRecords)
            list = list.Take(MaxRecords).ToList();

        Save(list);

        // Определяем место этой записи в топе.
        int rank = list.FindIndex(s => ReferenceEquals(s, entry) || (s.Score == entry.Score && s.Date == entry.Date)) + 1;
        return (list, rank);
    }

    /// <summary>
    /// Сохранить список рекордов на диск.
    /// </summary>
    public void Save(List<ScoreEntry> list)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_path, JsonSerializer.Serialize(list, options));
        }
        catch
        {
            // Не смогли сохранить — не критично, игра продолжается.
        }
    }

    /// <summary>
    /// Отрисовать таблицу рекордов на экране.
    /// </summary>
    public void ShowTable()
    {
        Console.Clear();
        Console.WriteLine("=== ТАБЛИЦА РЕКОРДОВ ===");
        Console.WriteLine();

        var list = Load();
        if (list.Count == 0)
        {
            Console.WriteLine("Пока пусто. Сыграйте партию!");
        }
        else
        {
            Console.WriteLine("{0,-4} {1,-8} {2}", "#", "Очки", "Дата");
            Console.WriteLine(new string('-', 30));
            for (int i = 0; i < list.Count; i++)
                Console.WriteLine("{0,-4} {1,-8} {2}", i + 1, list[i].Score, list[i].Date);
        }

        Console.WriteLine();
        Console.WriteLine("Нажмите любую клавишу для возврата в меню...");
        Console.ReadKey(true);
    }
}