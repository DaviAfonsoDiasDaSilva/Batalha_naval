using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Player1Server
{
    private TcpListener listener;
    private TcpClient client;
    private NetworkStream stream;
    private Board gameBoard;
    public int shipsSunk = 0;

    public Player1Server()
    {
        gameBoard = new Board();
    }

    public void StartServer(int port)
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Aguardando Player2 na porta {port}...");
        client = listener.AcceptTcpClient();
        stream = client.GetStream();
        Console.WriteLine("Player2 conectado!");
        Console.WriteLine("[DEBUG] Enviando mensagem inicial para o Player2.");
        Send("Bem-vindo ao jogo de Batalha Naval!");
    }

    public void SetupGame()
    {
        Console.WriteLine("Escolha o modo de posicionamento dos navios:");
        Console.WriteLine("1. Posicionamento Aleatório");
        Console.WriteLine("2. Posicionamento Manual");
        Console.Write("Digite sua escolha (1 ou 2): ");
        string choice = Console.ReadLine();

        if (choice == "1")
        {
            gameBoard.PlaceShipsRandomly(10);
        }
        else if (choice == "2")
        {
            gameBoard.PlaceShipsManually(10);
        }
        else
        {
            Console.WriteLine("Escolha inválida. Posicionando navios aleatoriamente.");
            gameBoard.PlaceShipsRandomly(10);
        }

        Console.WriteLine("\nTabuleiro com navios posicionados:");
        gameBoard.Print(true);
    }

    public void Send(string msg)
    {
        byte[] data = Encoding.ASCII.GetBytes(msg);
        stream.Write(data, 0, data.Length);
    }

    public string Receive()
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                Console.WriteLine("Servidor desconectou.");
                return null;
            }
            return Encoding.ASCII.GetString(buffer, 0, bytesRead);
        }
        catch (IOException ex)
        {
            Console.WriteLine("Timeout ao aguardar mensagem do servidor.");
            return null;
        }
    }

    public void HandleAttack(string coord)
    {
        Console.WriteLine($"[DEBUG] Processando ataque na coordenada: {coord}"); // Mensagem de debug
        try
        {
            var (row, col) = Board.ParseCoordinate(coord);
            if (gameBoard.IsShip(row, col))
            {
                gameBoard.MarkHit(row, col);
                shipsSunk++;
                Console.WriteLine("[DEBUG] Navio atingido!"); // Mensagem de debug

                if (shipsSunk == 10)
                {
                    Console.WriteLine("[DEBUG] Todos os navios foram afundados!"); // Mensagem de debug
                    Send("WIN");
                }
                else
                {
                    Send("HIT");
                }
            }
            else
            {
                Console.WriteLine("[DEBUG] Ataque errou o alvo."); // Mensagem de debug
                gameBoard.MarkMiss(row, col);
                Send("MISS");
            }
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"[DEBUG] Coordenada inválida recebida: {coord}. Erro: {ex.Message}"); // Mensagem de debug
            Send("INVALID");
        }

        Console.WriteLine("\nTabuleiro após o ataque:");
        gameBoard.Print(true);
    }

    public void StopServer()
    {
        stream?.Close();
        client?.Close();
        listener?.Stop();
    }
        public class Board
{
    private char[,] grid = new char[10, 10];

    public Board()
    {
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
                grid[r, c] = '~';
    }

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
                Console.Write(!showShips && cell == '*' ? "~ " : $"{cell} ");
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
            if (grid[r, c] == '~')
            {
                grid[r, c] = '*';
                placed++;
            }
        }
    }

    public void PlaceShipsManually(int n)
    {
        for (int i = 0; i < n; i++)
        {
            bool placed = false;
            while (!placed)
            {
                Console.Write($"Digite a coordenada para o navio {i + 1} (ex: A5): ");
                string coord = Console.ReadLine().ToUpper();
                try
                {
                    var (row, col) = ParseCoordinate(coord);
                    if (grid[row, col] == '~')
                    {
                        grid[row, col] = '*';
                        placed = true;
                    }
                    else
                    {
                        Console.WriteLine("Já existe um navio nessa coordenada. Tente novamente.");
                    }
                }
                catch (FormatException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }

    public bool IsShip(int r, int c)
    {
        return grid[r, c] == '*';
    }

    public void MarkHit(int r, int c)
    {
        grid[r, c] = 'X';
    }

    public void MarkMiss(int r, int c)
    {
        grid[r, c] = 'O';
    }

    public bool AreAllShipsSunk()
    {
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
                if (grid[r, c] == '*')
                    return false;
        return true;
    }

    public static (int row, int col) ParseCoordinate(string coord)
    {
        if (string.IsNullOrEmpty(coord) || coord.Length < 2 || coord.Length > 3)
            throw new FormatException("Coordenada inválida: Formato incorreto.");

        int row = coord[0] - 'A';
        if (row < 0 || row > 9)
            throw new FormatException("Coordenada inválida: Linha fora do intervalo (A-J).");

        if (!int.TryParse(coord.Substring(1), out int col) || col < 0 || col > 9)
            throw new FormatException("Coordenada inválida: Coluna fora do intervalo (0-9).");

        return (row, col);
    }
}
}