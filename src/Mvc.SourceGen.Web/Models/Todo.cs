namespace Mvc.SourceGen.Web.Models;

using System.Diagnostics.CodeAnalysis;

public record Todo(int Id, string Title, string Description, TodoType type);

public enum TodoType
{
    Private,
    Public
}


public class ParsableTodo : IParsable<ParsableTodo>
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TodoType Type { get; set; }

    public static ParsableTodo Parse(string s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out ParsableTodo result)
    {
        throw new NotImplementedException();
    }
}