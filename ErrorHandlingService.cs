
public class ErrorHandlingService
{
    private readonly ILogger<ErrorHandlingService> _logger;
    private readonly List<string> _messages = new List<string>();

    public event Action OnMessageAdded;

    public ErrorHandlingService(ILogger<ErrorHandlingService> logger)
    {
        _logger = logger;
    }

    public void LogError(Exception ex, string customMessage = null)
    {
        string errorMessage = customMessage ?? "An error occurred";
        _logger.LogError(ex, errorMessage);
        _messages.Add($"Error: {errorMessage}");
        OnMessageAdded?.Invoke();
    }

    public void LogInfo(string message)
    {
        _logger.LogInformation(message);
        _messages.Add($"Info: {message}");
        OnMessageAdded?.Invoke();
    }

    public IEnumerable<string> GetMessages()
    {
        return _messages;
    }

    public void ClearMessages()
    {
        _messages.Clear();
        OnMessageAdded?.Invoke();
    }
}