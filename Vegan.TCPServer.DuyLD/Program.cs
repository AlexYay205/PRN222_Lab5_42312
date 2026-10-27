using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Vegan.TCPServer.DuyLD
{

    public class veganPlaceDeposit
    {
        public partial class VeganPlaceDuyLd
        {
            public long VeganPlaceDuyLdid { get; set; }

            public string CategoryName { get; set; } = string.Empty;

            public string CategoryDescription { get; set; } = string.Empty;

            public decimal? CategoryDisplayOrder { get; set; }

            public string PlaceName { get; set; } = string.Empty;

            public string Address { get; set; } = string.Empty;

            public string Phone { get; set; } = string.Empty;

            public string Website { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public decimal? Latitude { get; set; }

            public decimal? Longitude { get; set; }

            public decimal? Rating { get; set; }

            public DateTime CreatedAt { get; set; }

            public bool IsActive { get; set; }
        }

        public static class DataStore
        {
            public static List<VeganPlaceDuyLd> VeganPlaces = new List<VeganPlaceDuyLd>()
            {
                new VeganPlaceDuyLd()
                {
                    VeganPlaceDuyLdid = 1,
                    CategoryName = "Nha hang chay",
                    PlaceName = "Moc Xanh",
                    Address = "TP. Ho Chi Minh",
                    Rating = 4.5m,
                    CreatedAt = new DateTime(2026, 1, 1),
                    IsActive = true
                },
                new VeganPlaceDuyLd()
                {
                    VeganPlaceDuyLdid = 2,
                    CategoryName = "Quan an chay",
                    PlaceName = "La Non",
                    Address = "TP. Ho Chi Minh",
                    Rating = 4.2m,
                    CreatedAt = new DateTime(2026, 1, 1),
                    IsActive = true
                }
            };
        }

        internal class Program
        {
            public static async Task Main(string[] args)
            {
                Console.WriteLine("Vegan Place TCP SERVER");

                TcpListener? server = null;
                try
                {
                    IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                    int port = 5000;
                    server = new TcpListener(ipAddress, port);

                    server.Start();
                    Console.WriteLine("Server started at localhost:" + port.ToString());

                    while (true)
                    {
                        TcpClient client = await server.AcceptTcpClientAsync();
                        Console.WriteLine("Client connected");

                        _ = Task.Run(() => HandleClientAsync(client));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Server error: {ex.Message}");
                }
                finally
                {
                    server?.Stop();
                }
            }

            private static async Task HandleClientAsync(TcpClient client)
            {
                try
                {
                    using NetworkStream stream = client.GetStream();

                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        string jsonResponse = string.Empty;

                        var noItem = new
                        {
                            RequestType = request,
                            Status = "Not found",
                            Message = "No items in this request " + request
                        };

                        var items = DataStore.VeganPlaces
                            .Where(c => c.VeganPlaceDuyLdid.ToString() == request || request == "ALL" || request == "all").ToList();

                        var options = new JsonSerializerOptions { WriteIndented = true };

                        if (items != null && items.Count > 0)
                        {
                            jsonResponse = JsonSerializer.Serialize(items, options);
                        }
                        else
                        {
                            jsonResponse = JsonSerializer.Serialize(noItem, options);
                        }

                        byte[] responseBytes = Encoding.UTF8.GetBytes(jsonResponse);
                        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                        await stream.FlushAsync();

                        Console.WriteLine($"Sent message to client for request: {request}");
                        client.Close();
                        Console.WriteLine($"Client disconnect");
                    }
                    else
                    {
                        string error = JsonSerializer.Serialize(new { Error = "Invalid request" });
                        byte[] errorBytes = Encoding.UTF8.GetBytes(error);
                        await stream.WriteAsync(errorBytes, 0, errorBytes.Length);
                        await stream.FlushAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Client error: {ex.Message}");
                }
                finally
                {
                    client.Close();
                }
            }
        }
    }
}
