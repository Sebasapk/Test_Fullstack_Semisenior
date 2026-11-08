namespace TaskManager.Application.Exceptions;

// Mapped to HTTP 400.
public class BusinessRuleException(string message) : Exception(message);
