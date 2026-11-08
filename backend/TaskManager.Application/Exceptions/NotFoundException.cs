namespace TaskManager.Application.Exceptions;

// Mapped to HTTP 404.
public class NotFoundException(string message) : Exception(message);
