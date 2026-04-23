using RMS.Domain.Entities.Models;

namespace RMS.Domain.Interfaces;

public interface ICodeGeneratorService
{
    string GenerateApplicationCode(string title);
}
