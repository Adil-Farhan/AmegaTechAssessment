using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using WebApp.Controllers;
using WebApp.Models;


namespace WebSocketsSample.Controllers
{
    public class WebSocketController : ControllerBase
    {
        private static readonly ConcurrentDictionary<Guid, (WebSocket WebSocket, string SubscribedInstrument)> _connectedClients = new();
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private static Timer _timer;
        private readonly Response _response;


        public WebSocketController(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _response = new Response();

            if (_timer == null)
            {
                _timer = new Timer(async _ => await BroadcastLatestPrices(), null, 0, 20000);
            }
        }

        [Route("/ws")]
        public async Task<IActionResult> Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var clientId = Guid.NewGuid();
                _connectedClients.TryAdd(clientId, (webSocket, null));

                await HandleClientConnection(clientId, webSocket);
                return Ok();
            }
            else
            {
                _response.Code = StatusCodes.Status400BadRequest;
                _response.Message = "This is Web Socket EndPoint Use Web Socket Client.";
                Log.Logger.Error(JsonConvert.SerializeObject(_response));
                return BadRequest(_response);
            }
        }

        private async Task HandleClientConnection(Guid clientId, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;

            try
            {
                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _connectedClients[clientId] = (webSocket, receivedMessage.Trim().ToLower());
                    }

                } while (!result.CloseStatus.HasValue);
            }
            finally
            {
                _connectedClients.TryRemove(clientId, out _);
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            }
        }

        private async Task BroadcastLatestPrices()
        {
            foreach (var clientPair in _connectedClients)
            {
                var clientId = clientPair.Key;
                var (webSocket, instrument) = clientPair.Value;

                if (webSocket.State == WebSocketState.Open && !string.IsNullOrWhiteSpace(instrument))
                {
                    string token = _configuration["TiingoApiToken"];
                    string url = $"https://api.tiingo.com/tiingo/crypto/prices?tickers={instrument}&token={token}";

                    HttpResponseMessage response = await _httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();
                        var cryptoPrices = JsonConvert.DeserializeObject<List<CryptoPriceResponce>>(responseData).FirstOrDefault();

                        if (cryptoPrices != null)
                        {
                            var latestPrice = cryptoPrices.PriceData.OrderByDescending(x => x.Date).FirstOrDefault();
                            if (latestPrice != null)
                            {
                                var priceMessage = JsonConvert.SerializeObject(latestPrice);
                                var messageBytes = Encoding.UTF8.GetBytes(priceMessage);
                                await webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                                _response.Code = StatusCodes.Status200OK;
                                _response.Message = $"Financial Instrument {instrument} Price Fetched Successfully.";
                                Log.Logger.Information(JsonConvert.SerializeObject(_response));
                            }
                        }
                        else
                        {
                            _response.Code = StatusCodes.Status200OK;
                            _response.Message = $"Financial Instrument {instrument} Price not Fetched Successfully.";
                            Log.Logger.Information(JsonConvert.SerializeObject(_response));
                            await SendErrorMessage(webSocket, JsonConvert.SerializeObject(_response));
                        }
                    }
                    else
                    {
                        _response.Code = StatusCodes.Status400BadRequest;
                        _response.Message = $"Financial Instrument {instrument} Price not Fetched Successfully.";
                        Log.Logger.Information(JsonConvert.SerializeObject(_response));
                        await SendErrorMessage(webSocket, JsonConvert.SerializeObject(_response));
                    }
                }
            }
        }

        private async Task SendErrorMessage(WebSocket webSocket, string errorMessage)
        {
            byte[] errorBytes = Encoding.UTF8.GetBytes(errorMessage);
            await webSocket.SendAsync(new ArraySegment<byte>(errorBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
