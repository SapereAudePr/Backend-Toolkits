namespace MiddlewareDemo;

public class NoEntityFoundException(string message) : AppException($"{message}")
{
    public override int StatusCode => StatusCodes.Status404NotFound;
}