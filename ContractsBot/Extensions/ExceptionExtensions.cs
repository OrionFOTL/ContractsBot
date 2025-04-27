namespace ContractsBot.Extensions;

public static class ExceptionExtensions
{
    public static IEnumerable<string> GetAllMessages(this Exception exception)
    {
        var currentException = exception;
        do
        {
            yield return currentException.Message;
            currentException = currentException.InnerException;
        }
        while (currentException != null);
    }
}