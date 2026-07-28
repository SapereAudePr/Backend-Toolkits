namespace MiddlewareDemo;

public class NoEntityFoundException(string message) : Exception($"{message}")
{
}