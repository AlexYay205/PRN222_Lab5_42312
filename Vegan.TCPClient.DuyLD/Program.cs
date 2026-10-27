using System.Net.Sockets;
using System.Text;

Console.WriteLine("Vegan Place TCP CLIENT");

Console.OutputEncoding = Encoding.UTF8;

while (true)
{
    Console.Write("Enter place ID or ALL (type EXIT to quit): ");
    string? command = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(command))
    {
        Console.WriteLine("Command must not be empty.");
        Console.WriteLine();
        continue;
    }

    command = command.Trim();
    if (command.Equals("EXIT", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    int port = 5000;

    try
    {
        using TcpClient client = new();
        await client.ConnectAsync("127.0.0.1", port);
        Console.WriteLine("Connect to Server:" + port.ToString() + " and send the request: " + command);

        using NetworkStream stream = client.GetStream();

        byte[] requestBytes = Encoding.UTF8.GetBytes(command);
        await stream.WriteAsync(requestBytes, 0, requestBytes.Length);
        await stream.FlushAsync();

        byte[] buffer = new byte[1024];
        StringBuilder responseBuilder = new();

        while (true)
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                break;
            }

            responseBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
        }

        Console.WriteLine("Server response:");
        Console.WriteLine(responseBuilder.ToString());

        client.Close();
        Console.WriteLine("Disconnect to Server");

    }
    catch (SocketException)
    {
        Console.WriteLine("Cannot connect to server at 127.0.0.1:" + port.ToString());
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error: {ex.Message}");
    }

    Console.WriteLine();
}
