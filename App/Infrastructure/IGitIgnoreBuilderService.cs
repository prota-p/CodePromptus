using System.Threading.Tasks;

namespace CodePromptus.App.Infrastructure;
public interface IGitignoreBuilderService
{
    Task<PathIgnorePredicate> CreateIsIgnoredDelegateAsync(string gitignorePath);
}