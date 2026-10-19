using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TechNova.RMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        // Generic convenience overload for plain results
        public IActionResult HandleResult<T>(T result)
        {
            return Ok(result);
        }

        // Handle a ServiceResult (contains StatusCode and optional data/error)
        public IActionResult HandleResult<T>(Models.ServiceResult<T> result)
        {
            if (result == null)
                return NotFound();

            var code = result.StatusCode;
            switch (code)
            {
                case System.Net.HttpStatusCode.OK:
                    return Ok(result.Data);
                case System.Net.HttpStatusCode.Created:
                    return StatusCode(StatusCodes.Status201Created, result.Data);
                case System.Net.HttpStatusCode.NoContent:
                    return NoContent();
                case System.Net.HttpStatusCode.BadRequest:
                    return BadRequest(result.Message ?? (object?)result.Data);
                case System.Net.HttpStatusCode.Unauthorized:
                    return Unauthorized();
                case System.Net.HttpStatusCode.Forbidden:
                    return Forbid();
                case System.Net.HttpStatusCode.NotFound:
                    return NotFound(result.Message ?? (object?)result.Data);
                case System.Net.HttpStatusCode.Conflict:
                    return Conflict(result.Message ?? (object?)result.Data);
                default:
                    return StatusCode((int)code, result.Message ?? (object?)result.Data);
            }
        }
    }
}
