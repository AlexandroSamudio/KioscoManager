namespace API.Validators;

public record ValidationProblemDetailsDto
{
    public required string Type { get; init; }
    public required string Title { get; init; }
    public required int Status { get; init; }
    public required string Detail { get; init; }
    public required string Instance { get; init; }
    public required Dictionary<string, string[]> Errors { get; init; }
}


