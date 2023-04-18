using Microsoft.Extensions.Logging;
namespace SmallEco.DTO;

public class ErrMsg
{
  public ErrMsg()
  {
    Message = String.Empty;
    Severity = LogLevel.None;
  }

  public ErrMsg(string message, LogLevel severity = LogLevel.Error)
  {
    Message = message;
    Severity = severity;
  }

  public string Message { get; set; }

  public LogLevel Severity { get; set; }
}