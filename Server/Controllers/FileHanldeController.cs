using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using SmallEco.DTO;
using SmallEco.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace SmallEco.Server.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class FileHanldeController : ControllerBase
{
  //# for injection
  ILogger<FileHanldeController> _logger;

  public FileHanldeController(ILogger<FileHanldeController> logger)
  {
    _logger = logger;
  }

  [HttpPost("[action]")]
  [SwaggerResponse(200, type: typeof(byte[]))]
  public IActionResult DownloadFile(Guid id)
  {
    // 模擬邏輯失敗！
    if (id == Guid.Empty)
    {
      return BadRequest(new ErrMsg("模擬邏輯失敗！"));
    }

    FileInfo fi = new FileInfo(@"Template\消保1-1全行客訴案件統計暨同期分析比較.xlsx");

    if (System.IO.File.Exists(fi.FullName))
    {
      return File(System.IO.File.ReadAllBytes(fi.FullName), "application/octet-stream", fi.Name);
    }

    return NotFound();
  }

  [HttpPost("[action]")]
  [SwaggerResponse(200, type: typeof(string))]
  public IActionResult Echo()
  {
    return Ok($"echo at {DateTime.Now:HH:mm:ss.}");
  }
}
