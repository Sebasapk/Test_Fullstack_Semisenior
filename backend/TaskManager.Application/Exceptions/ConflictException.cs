namespace TaskManager.Application.Exceptions;

// Mapped to HTTP 409.
public class ConflictException(string message) : Exception(message);
