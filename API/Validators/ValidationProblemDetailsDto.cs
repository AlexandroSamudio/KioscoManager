namespace API.Validators;

public class ValidationProblemDetailsDto
{
    public required string Type { get; set; }
    public required string Title { get; set; }
    public required int Status { get; set; }
    public required string Detail { get; set; }
    public required string Instance { get; set; }
    public required Dictionary<string, string[]> Errors { get; set; }
}


