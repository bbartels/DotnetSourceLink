using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace DotnetSourceLink.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SourceLinkController : ControllerBase
    {
        private readonly static Dictionary<string, (string, ushort, ushort)[]> _locationDictionary = new Dictionary<string, (string, ushort, ushort)[]>();
        private readonly RepositoryManager _manager;

        public SourceLinkController(RepositoryManager manager) => _manager = manager;

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            if (_locationDictionary.ContainsKey(id)) { return Ok(_locationDictionary[id].Select(x => new { github = x.Item1, start = x.Item2, end = x.Item3 })); }

            var result = _manager.Get(id);
            if (result.Item1 != null)
            {
                var res = result.Item1.Select(x => new { github = x.File.ToString(), start = x.StartLineNumber, end = x.EndLineNumber });
                _locationDictionary.Add(id, res.Select(x => (x.github, x.start, x.end)).ToArray());
                return Ok(res);
            }
            return BadRequest(result.message);
        }
    }
}
