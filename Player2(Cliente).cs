//Código exemplo
public void Print(bool showShips)
{
    Console.Write("   ");
    for (int c = 0; c < 10; c++) Console.Write($"{c} ");
    Console.WriteLine();
    for (int r = 0; r < 10; r++)
    {
        Console.Write($"{(char)('A' + r)}  ");
        for (int c = 0; c < 10; c++)
        {
            char cell = grid[r, c];
            Console.Write(!showShips && cell=='*' ? "~ " : $"{cell} ");
        }
        Console.WriteLine();
    }
}

public void PlaceShipsRandomly(int n)
{
    var rnd = new Random();
    int placed = 0;
    while (placed < n)
    {
        int r = rnd.Next(10), c = rnd.Next(10);
        if (grid[r,c] == '~')
        {
            grid[r,c] = '*';
            placed++;
        }
    }
}

//fim código exemplo