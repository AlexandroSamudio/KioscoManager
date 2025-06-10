namespace API.Entities;

public class CodigoInvitacion
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public DateTime ExpirationDate { get; set; }
    public bool IsUsed { get; set; } = false;
    public int KioscoId { get; set; }
    public Kiosco? Kiosco { get; set; }

}
