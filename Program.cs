using Hangfire;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.PostgreSql;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using System.Transactions;
class ResetRangeAttribute : JobFilterAttribute, IClientFilter
{

    private void TransactionScopeError (CreatedContext filterContext, IEnumerable<KeyValuePair<string, string>> values) {
        // The transaction specified for TransactionScope has a different IsolationLevel than the value requested for the scope
        filterContext.Connection.SetRangeInHash("foo", values);
    }

    private void ForeignKeyError (CreatedContext filterContext, IEnumerable<KeyValuePair<string, string>> values) {
        // insert or update on table "state" violates foreign key constraint "state_jobid_fkey
        var options2 = new TransactionOptions() { IsolationLevel = IsolationLevel.Unspecified};
        using(var t2s = new TransactionScope(TransactionScopeOption.Suppress, options2, TransactionScopeAsyncFlowOption.Suppress)) {
            filterContext.Connection.SetRangeInHash("foo", values);
        }
    }


    public void OnCreated(CreatedContext filterContext)
    {
        var values =  new Dictionary<string, string>() { {"Timestamp", DateTimeOffset.UtcNow.ToString("o")} };

        // this.TransactionScopeError(filterContext, values);
        this.ForeignKeyError(filterContext,values);
    }

    public void OnCreating(CreatingContext filterContext)
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
