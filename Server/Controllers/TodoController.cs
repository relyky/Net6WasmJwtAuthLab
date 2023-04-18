using Microsoft.AspNetCore.Mvc;
using SmallEco.DTO;
using Swashbuckle.AspNetCore.Annotations;

namespace SmallEco.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TodoController : ControllerBase
{
  List<TodoDto> _simsTodoRepo = new()
  {
      new() { Sn = 1, Description = "今天天氣真好", Done = false, CreateDtm = DateTime.Now.AddDays(-3) },
      new() { Sn = 2, Description = "下午去看電影", Done = false, CreateDtm = DateTime.Now.AddDays(-2) },
      new() { Sn = 3, Description = "晚上吃大餐", Done = false, CreateDtm = DateTime.Now.AddDays(-1) }
  };

  //# for injection
  ILogger<TodoController> _logger;

  public TodoController(ILogger<TodoController> logger)
  { 
    _logger = logger;
  }

  [HttpPost("[action]")]
  [SwaggerResponse(200, type: typeof(List<TodoDto>))]
  [SwaggerResponse(400, type: typeof(ErrMsg))]
  public IActionResult QryDataList(TodoQryAgs args)
  {
    if(args.Msg == "測試邏輯失敗")
    {
      return BadRequest(new ErrMsg("這是邏輯失敗！"));
    }

    return Ok(_simsTodoRepo);
  }

  //// GET api/<TodoController>/5
  //[HttpGet("{id}")]
  //public string Get(int id)
  //{
  //  return "value";
  //}

  //// POST api/<TodoController>
  //[HttpPost]
  //public void Post([FromBody] string value)
  //{
  //}

  //// PUT api/<TodoController>/5
  //[HttpPut("{id}")]
  //public void Put(int id, [FromBody] string value)
  //{
  //}

  //// DELETE api/<TodoController>/5
  //[HttpDelete("{id}")]
  //public void Delete(int id)
  //{
  //}
}
