using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using WebApp.Models;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinancialInstrumentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly Response _response;

        public FinancialInstrumentController(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _response = new Response();         
        }

        [HttpGet("GetFinancialInstrument")]
        public async Task<IActionResult> GetCryptoData()
        {
            try
            {
                string token = _configuration["TiingoApiToken"];
                string url = $"https://api.tiingo.com/tiingo/crypto?token={token}";

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    var cryptos = JsonConvert.DeserializeObject<List<Crypto>>(responseData);
                    _response.Code = StatusCodes.Status200OK;
                    _response.Message = "Financial Instruments Fetched Successfully.";
                    _response.Data = cryptos;
                    Log.Logger.Information(JsonConvert.SerializeObject(_response));
                    return Ok(_response);
                }
                else
                {
                    _response.Code = StatusCodes.Status400BadRequest;
                    _response.Message = "Financial Instruments not Fetched Successfully.";
                    Log.Logger.Information(JsonConvert.SerializeObject(_response));
                    return Ok(_response);
                }
            }
            catch (Exception ex)
            {
                _response.Code = StatusCodes.Status500InternalServerError;
                _response.Message = ex.Message;
                Log.Logger.Error(JsonConvert.SerializeObject(_response));
                return BadRequest(_response);
            }
            
            
        }

        [HttpGet("GetPrices")]
        public async Task<IActionResult> GetCryptoPrices(string instrument)
        {
            try
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
                        _response.Code = StatusCodes.Status200OK;
                        _response.Message = $"Financial Instrument {instrument} Price Fetched Successfully.";
                        _response.Data = latestPrice;
                        Log.Logger.Information(JsonConvert.SerializeObject(_response));
                        return Ok(_response);
                    }
                    else
                    {
                        _response.Code = StatusCodes.Status200OK;
                        _response.Message = $"Financial Instrument {instrument} Price not Fetched Successfully.";
                        Log.Logger.Information(JsonConvert.SerializeObject(_response));
                        return Ok(_response);
                    }
                }
                else
                {
                    _response.Code = StatusCodes.Status400BadRequest;
                    _response.Message = $"Financial Instrument {instrument} Price not Fetched Successfully.";
                    Log.Logger.Information(JsonConvert.SerializeObject(_response));
                    return Ok(_response);
                }
            }
            catch (Exception ex)
            {
                _response.Code = StatusCodes.Status500InternalServerError;
                _response.Message = ex.Message;
                Log.Logger.Error(JsonConvert.SerializeObject(_response));
                return BadRequest(_response);
            }
        }
    }
}
