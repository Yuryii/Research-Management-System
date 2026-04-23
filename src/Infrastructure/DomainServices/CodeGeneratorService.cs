using RMS.Domain.Interfaces;

namespace RMS.Infrastructure.DomainServices;

internal class CodeGeneratorService : ICodeGeneratorService
{
    public string GenerateApplicationCode(string title)
    {
        var prefix = new string(title
            .Where(char.IsLetterOrDigit)
            .Take(3)
            .Select(char.ToUpperInvariant)
            .ToArray());

        return $"{prefix}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
    }
}
