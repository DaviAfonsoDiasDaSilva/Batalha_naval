using System;
using System.Net.Sockets;
using System.Text;
using System.Net;

public class Player2Client
{
    private TcpClient client;
    private NetworkStream stream;

    public void ConnectToServer(string serverIp, int serverPort)
    {
        try
        {
            client = new TcpClient();
            client.Connect(serverIp, serverPort);
            stream = client.GetStream();
            Console.WriteLine($"Conectado ao servidor {serverIp}:{serverPort}");
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Erro ao conectar: {ex.Message}");
            throw; // Re-lança a exceção para ser tratada no Main
        }
    }

    public void Send(string message)
    {
        if (stream == null)
        {
            Console.WriteLine("Erro: Não conectado ao servidor.");
            return;
        }

        try
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            stream.Write(data, 0, data.Length);
            Console.WriteLine($"Enviado para o servidor: {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar: {ex.Message}");
            throw;
        }
    }

    public string Receive()
    {
        if (stream == null)
        {
            Console.WriteLine("Erro: Não conectado ao servidor.");
            return null;
        }

        try
        {
            byte[] buffer = new byte[1024]; // Buffer maior
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            if (bytesRead == 0)
            {
                Console.WriteLine("Servidor desconectou.");
                return null; // Indica desconexão
            }

            return Encoding.ASCII.GetString(buffer, 0, bytesRead);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao receber: {ex.Message}");
            throw;
        }
    }

    public void CloseConnection()
    {
        stream?.Close();
        client?.Close();
        Console.WriteLine("Conexão fechada.");
    }

    // NOVO MÉTODO: Para receber o input do usuário e validar a coordenada de ataque
    public string GetAttackCoordinate()
    {
        string coordInput;
        while (true)
        {
            Console.Write("Digite a coordenada para atacar (ex: A5): ");
            coordInput = Console.ReadLine().ToUpper(); // Converte para maiúsculas

            if (string.IsNullOrWhiteSpace(coordInput))
            {
                Console.WriteLine("A coordenada não pode ser vazia. Tente novamente.");
                continue;
            }

            if (coordInput.ToLower() == "sair")
            {
                return "ENCERRAR_CONEXAO"; // Retorna um comando especial para encerrar
            }

            try
            {
                // Tenta parsear a coordenada usando o método estático da classe Board
                // A classe Board deve estar acessível (no mesmo arquivo ou referenciada)
                Board.ParseCoordinate(coordInput);
                return coordInput; // Retorna a coordenada válida
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex.Message); // Exibe a mensagem de erro da validação
            }
        }
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
