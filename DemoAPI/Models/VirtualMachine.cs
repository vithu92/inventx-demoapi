public class VirtualMachine
{
    public int Id {get; set;}
    public string? Name {get; set;}
    public string Status { get; set; } = string.Empty;
    public int? ParentId {get; set;}
    public bool? IsStartable {get; set;}
    public string? Location {get; set;}
    public string? Owner {get; set;}
    public string? CreatedBy {get; set;}
    public List<string>? Tags {get; set;}
    public int Cpu {get; set;}
    public long Ram {get; set;}
    public DateTime CreatedAt {get; set;}

}