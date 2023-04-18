namespace SmallEco.DTO;

public class TodoDto
{
  public int Sn { get; set; }

  public string Description { get; set; }

  public bool Done { get; set; }

  public System.DateTime CreateDtm { get; set; }
}

public class TodoQryAgs
{
  public int Amt { get; set; }

  public string Msg { get; set; }
}
