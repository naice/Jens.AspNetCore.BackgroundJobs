using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

public static class Extensions 
{



    private static string GetLogInvocationsDump(IInvocationList list)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("#### DUMPING LOG CALLS BEGIN ####");
        var invocations = list.Where(inv => inv.Method.Name == "Log").Select(
            inv => $"LogLevel: {inv.Arguments[0]}, Message: {inv.Arguments[2].ToString()}"
        ).ToList();

        if (invocations.Count == 0)
            sb.AppendLine("NO INVOCATIONS ON LOG CALL.");
        else
            invocations.ForEach(inv => sb.AppendLine(inv));
        sb.AppendLine("#### DUMPING LOG CALLS END ####");
        return sb.ToString();
    }

    public static Mock<ILogger<T>> VerifyLogStartsWith<T>(this Mock<ILogger<T>> logger, LogLevel lvl, string message)
    {
        var any = logger.Invocations.Any(inv => 
            inv.Method.Name == "Log" &&
            (LogLevel)inv.Arguments[0] == lvl &&
            (inv.Arguments[2].ToString()?.StartsWith(message) ?? false)
        );
        
        Assert.True(any, 
            $"No Log invocation matches, {lvl} + StartsWith({message})" 
            + GetLogInvocationsDump(logger.Invocations));
        return logger;
    }
    public static Mock<ILogger<T>> VerifyLogEquals<T>(this Mock<ILogger<T>> logger, LogLevel lvl, string message)
    {
        var any = logger.Invocations.Any(inv => 
            inv.Method.Name == "Log" &&
            (LogLevel)inv.Arguments[0] == lvl &&
            inv.Arguments[2].ToString() == message
        );
        
        Assert.True(any, 
            $"No Log invocation matches, {lvl} + Equals({message})" 
            + GetLogInvocationsDump(logger.Invocations)
        );
        return logger;
    }
    public static Mock<ILogger<T>> VerifyLogContainsAll<T>(this Mock<ILogger<T>> logger, LogLevel lvl, params string[] args)
    {
        var any = logger.Invocations.Any(inv => {
            var match = inv.Method.Name == "Log" &&
            (LogLevel)inv.Arguments[0] == lvl;
            if (!match) return false;
            var message = inv.Arguments[2].ToString() ?? string.Empty;
            return args.All(x => message.Contains(x));
        });
        
        Assert.True(any, 
            $"No Log invocation matches, {lvl} + ContainsAll({string.Join(", ", args)})" 
            + GetLogInvocationsDump(logger.Invocations)
        );
        return logger;
    }

    public static T JsonClone<T>(this T a) 
        => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(a))!;
}