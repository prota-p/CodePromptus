using System.Collections.Generic;

namespace CodePromptus.App.Services;

public interface IGitIgnoreRecommenderService
{
    IEnumerable<GitIgnoreRecommendation> RecommendTemplatesFromFolder(string root, int limit=3);
}