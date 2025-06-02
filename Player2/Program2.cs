using System;
using System.Net.Sockets;
using System.Text; // Adicionado para Encoding

public class Program
{
    // Definição da classe Board (movida para dentro de Program.cs para garantir acessibilidade)
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

        public char GetCell(int r, int c)
        {
            return grid[r, c];
        }

        public void SetCell(int r, int c, char value)
        {
            grid[r, c] = value;
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

    public static void Main(string[] args)
    {
        Player2Client player2 = new Player2Client();
        string serverIp = "127.0.0.1"; // Endereço IP do servidor (Player 1)
        int serverPort = 12345;            // Porta do servidor
        Board attackBoard = new Board(); // Instanciar Board aqui

        try
        {
            Console.WriteLine("Iniciando Player 2 (Cliente)...");
            player2.ConnectToServer(serverIp, serverPort);

            Console.WriteLine("Tabuleiro de Ataque:");
            attackBoard.Print(false); // Exibe o tabuleiro de ataque (inicialmente em branco)

            // Exemplo de comunicação
            string initialMessage = player2.Receive();
            Console.WriteLine($"Recebido do servidor: {initialMessage}");

            player2.Send("Olá, Player 1! Player 2 conectado.");

            Console.WriteLine("\n--- Início do Jogo de Batalha Naval ---");
            while (true)
            {
                Console.Write("Digite a coordenada para atacar (ex: A5): ");
                string messageToSend = Console.ReadLine().ToUpper(); // Converte para maiúsculas para padronização

                if (string.IsNullOrWhiteSpace(messageToSend)) continue;

                if (messageToSend.ToLower() == "sair")
                {
                    player2.Send("ENCERRAR_CONEXAO"); // Envia um comando para o servidor, se necessário
                    break;
                }

                // Valida a coordenada antes de enviar
                try
                {
                    var (row, col) = Board.ParseCoordinate(messageToSend); // Chamar Board.ParseCoordinate
                    player2.Send(messageToSend); // Envia a coordenada de ataque

                    string receivedMessage = player2.Receive();
                    if (receivedMessage == null) // Servidor desconectou
                    {
                        Console.WriteLine("Conexão encerrada pelo servidor.");
                        break;
                    }
                    Console.WriteLine($"Resultado do ataque: {receivedMessage}");

                    // Atualiza o tabuleiro de ataque com base na resposta do servidor
                    if (receivedMessage == "HIT")
                    {
                        attackBoard.MarkHit(row, col);
                    }
                    else if (receivedMessage == "MISS")
                    {
                        attackBoard.MarkMiss(row, col);
                    }
                    else if (receivedMessage == "WIN")
                    {
                        attackBoard.MarkHit(row, col); // Marca o último acerto
                        Console.WriteLine("Você venceu! Parabéns!");
                        break;
                    }
                    else if (receivedMessage == "INVALID")
                    {
                        Console.WriteLine("Coordenada inválida enviada. Tente novamente.");
                    }

                    Console.WriteLine("Tabuleiro de Ataque Atualizado:");
                    attackBoard.Print(false); // Exibe o tabuleiro de ataque atualizado
                }
                catch (FormatException ex)
                {
                    Console.WriteLine(ex.Message); // Exibe a mensagem de erro da coordenada inválida
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ocorreu um erro durante o ataque: {ex.Message}");
                    break;
                }
            }
            Console.WriteLine("--- Fim do Jogo de Batalha Naval ---");
        }
        catch (SocketException sockEx)
        {
            Console.WriteLine($"Erro de Socket: {sockEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ocorreu um erro inesperado: {ex.Message}");
        }
        finally
        {
            player2.CloseConnection();
            Console.WriteLine("Aplicação do Player 2 (Cliente) encerrada. Pressione qualquer tecla para sair.");
            Console.ReadKey();
        }
    }
}
