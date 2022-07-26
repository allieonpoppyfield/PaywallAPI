using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace PaywallAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HotlinkController : ControllerBase
{
    [HttpPost("GenerateHotlinkPost")]
    [Consumes("application/json")]
    public async Task<IActionResult> GenerateHotlinkPost([FromBody] HotlinkRequest request)
    {
        try
        {
            string result = GetNewHotlink(request.password, request.url, request.ipAddress, request.validTime);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("GenerateHotlinkGet")]
    public async Task<IActionResult> GenerateHotlinkGet([FromQuery] string password, string url, string ipAddress, int validTime)
    {
        try
        {
            string result = GetNewHotlink(password, url, ipAddress, validTime);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private string GetNewHotlink(string password, string url, string ipAddress, int validTime)
    {
        string result = null;
        DateTime cur_date = DateTime.Now;
        TimeZone localzone = TimeZone.CurrentTimeZone;
        DateTime localTime = localzone.ToUniversalTime(cur_date);
        string date_time = localTime.ToString(new CultureInfo("en-us"));
        Int32 Valid = validTime;
        string to_be_hashed = ipAddress + password + date_time + Valid.ToString();
        byte[] to_be_hashed_byte_array = new byte[to_be_hashed.Length];
        int i = 0;
        foreach (char cur_char in to_be_hashed)
        {
            to_be_hashed_byte_array[i++] = (byte)cur_char;
        }
        byte[] hash = (new MD5CryptoServiceProvider()).ComputeHash(to_be_hashed_byte_array);
        string md5_signature = Convert.ToBase64String(hash);
        string signature = "server_time=" + date_time + "&hash_value=" + md5_signature + "&validminutes=" + Valid.ToString();
        string base64urlsignature = Convert.ToBase64String(Encoding.UTF8.GetBytes(signature));
        result = url + "?wmsAuthSign=" + base64urlsignature;
        return result;
    }
}
public record HotlinkRequest(string password, string url, string ipAddress, int validTime);