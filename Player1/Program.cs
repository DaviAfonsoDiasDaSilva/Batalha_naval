using System;
using System.Net.Sockets;

public class Program
{
    public static void Main(string[] args)
    {
        Player1Server player1 = new Player1Server();
        int serverPort = 12345;

        try
        {
            Console.WriteLine("Iniciando Servidor do Player 1...");
            player1.StartServer(serverPort);
            player1.SetupGame();

            while (true)
            {
                string attackCoord = player1.Receive();
                if (string.IsNullOrEmpty(attackCoord))
                {
                    Console.WriteLine("Player 2 desconectou. Fim de jogo.");
                    break;
                }

                Console.WriteLine($"Ataque recebido do Player 2: {attackCoord}");
                player1.HandleAttack(attackCoord);

                if (player1.shipsSunk == 10)
                {
                    Console.WriteLine("Todos os navios afundados! Você venceu!");
                    break;
                }
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Erro de Socket: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
        finally
        {
            player1.StopServer();
            Console.WriteLine("Servidor encerrado.");
        }
    }
}