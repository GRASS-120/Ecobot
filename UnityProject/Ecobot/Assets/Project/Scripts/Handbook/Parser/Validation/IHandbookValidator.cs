using System.Threading;
using System.Threading.Tasks;

namespace Handbook.Parser.Validation
{
    public interface IHandbookValidator
    {
        Task<HandbookValidationReport> ValidateAllAsync(IHandbookRepository repo, CancellationToken ct = default);
        Task<HandbookValidationReport> ValidatePageAsync(IHandbookRepository repo, string pageId, CancellationToken ct = default);
    }
}