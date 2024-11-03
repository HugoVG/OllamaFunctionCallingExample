using Bogus.DataSets;
using Microsoft.Extensions.Logging;

namespace OllamaFunctionCalling.Services;

/// <summary>
/// In Memory collector. You can image your database or csv file here.
/// I just didn't want to set up EF-core just for this.
/// </summary>
public class MockDatabase(ILogger<MockDatabase> logger) : IDatabase
{
    private static readonly List<ProcessedSentiment> PostSentimentStatements = [];
    
    public bool AddSentiment(ProcessedSentiment statement)
    {
        PostSentimentStatements.Add(statement);
        logger.LogInformation("Added Sentiment {Id}: {Sentiment} {Statement}", statement.Id, statement.Sentiment, statement.Statement);
        return true;
    }

    public IEnumerable<DbStatement> GetStatementsToProcess(string product = "product")
    {
        while (true) yield return new DbStatement()
        {
            Id = Guid.NewGuid(),
            Statement = new Rant().Review(product)
        };
    }
    
    public List<ProcessedSentiment> GetSentiments()
    {
        return PostSentimentStatements;
    }
}

public interface IDatabase 
{
    bool AddSentiment(ProcessedSentiment statement);
    IEnumerable<DbStatement> GetStatementsToProcess(string product = "product");
    List<ProcessedSentiment> GetSentiments();
}

public class DbStatement
{
    public Guid Id { get; set; }

    public string Statement { get; set; }
}

public class ProcessedSentiment
{
    public Guid Id { get; set; }

    public string Statement { get; set; }

    public string? Sentiment { get; set; }
}
