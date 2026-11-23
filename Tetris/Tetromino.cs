namespace Tetris;

public class Tetromino
{
    public bool[,] Shape { get; private set; }
    public int X { get; set; }
    public int Y { get; set; }

    // Человекочитаемое имя фигуры — пригодится для отладки.
    public string Name { get; private set; } = "?";

    public Tetromino(bool[,] shape, string name = "?")
    {
        Shape = shape;
        Name = name;
    }

    public Tetromino Clone()
    {
        int rows = Shape.GetLength(0);
        int cols = Shape.GetLength(1);
        bool[,] copy = new bool[rows, cols];
        Array.Copy(Shape, copy, Shape.Length);
        return new Tetromino(copy, Name) { X = X, Y = Y };
    }

    public void Rotate()
    {
        int rows = Shape.GetLength(0);
        int cols = Shape.GetLength(1);
        bool[,] rotated = new bool[cols, rows];

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                rotated[c, rows - 1 - r] = Shape[r, c];

        Shape = rotated;
    }

    public static Tetromino CreateRandom(Random rnd)
    {
        // Теперь к каждой форме привязано имя.
        (string name, bool[,] shape)[] shapes =
        {
            ("I", new bool[,] { {true,true,true,true} }),
            ("O", new bool[,] { {true,true}, {true,true} }),
            ("T", new bool[,] { {true,true,true}, {false,true,false} }),
            ("S", new bool[,] { {false,true,true}, {true,true,false} }),
            ("Z", new bool[,] { {true,true,false}, {false,true,true} }),
            ("L", new bool[,] { {true,false,false}, {true,true,true} }),
            ("J", new bool[,] { {false,false,true}, {true,true,true} })
        };

        var pick = shapes[rnd.Next(shapes.Length)];
        var t = new Tetromino(pick.shape, pick.name);
        t.X = 3;
        t.Y = 0;
        return t;
    }
}