
using CanteenBackend.Models;
public class FAQ
{
  public int Id { get; set; }

  public required string  Question {get; set;}
  public required string Answer{get; set;}

  public bool isDeleted{get;set;}

  public DateTime createdAt{get; set;}

}