public class MahaiDto
{
    public int Id { get; set; }
    public int MahaiZenbakia { get; set; }
    public int PertsonaMax { get; set; }
    public bool Occupied { get; set; }
    public int? ErreserbaId { get; set; }
    public int? PertsonaKopurua { get; set; }
}

public class MahaiCreateDto
{
    public int MahaiZenbakia { get; set; }
    public int PertsonaMax { get; set; }
}

public class MahaiUpdateDto
{
    public int Id { get; set; }
    public int MahaiZenbakia { get; set; }
    public int PertsonaMax { get; set; }
}