using Hangfire;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.PostgreSql;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using System.Transactions;
class ResetRangeAttribute : JobFilterAttribute, IClientFilter, IServerFilter, IApplyStateFilter
{
    public void OnCreated(CreatedContext filterContext)
    {
        Console.WriteLine("ResetRangeAttribute OnCreated");
        var values =  new Dictionary<string, string>() { {"Timestamp", DateTimeOffset.UtcNow.ToString("o")} };
        filterContext.Connection.SetRangeInHash("foo", values);
        // throw new NotImplementedException();
    }

    public void OnCreating(CreatingContext filterContext)
    {
        // throw new NotImplementedException();
    }

    public void OnPerformed(PerformedContext filterContext)
    {
        // throw new NotImplementedException();
    }

    public void OnPerforming(PerformingContext filterContext)
    {
        // throw new NotImplementedException();
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        // throw new NotImplementedException();
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        // throw new NotImplementedException();
    }
}

class SomeInternalLogic {
    [ResetRangeAttribute]
    public string DoAThing() {
        return "";
    }
}

class Program
{
    static void Main(string[] args)
    {
        var conn = new Npgsql.NpgsqlConnectionStringBuilder() {
            Username = "postgres",
            Password = "postgres",
            Database = "postgres",
            Host = "localhost",
            Port = 5434
        };
        GlobalConfiguration.Configuration
            .UseColouredConsoleLogProvider()
            .UsePostgreSqlStorage(conn.ToString());
        var options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted};
        using(var ts = new TransactionScope(TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled))
        {
            using (var server = new BackgroundJobServer())
            {
                BackgroundJob.Enqueue<SomeInternalLogic>((logic) => logic.DoAThing());
                Console.ReadLine();
            }
        }
        
    }
}
