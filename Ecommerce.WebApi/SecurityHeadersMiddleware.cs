using Microsoft.Extensions.Primitives;//StringValues

public class SecurityHeaders
{
    private readonly RequestDelegate next;

    public SecurityHeaders(RequestDelegate next)
    {
        this.next = next;
    }
    public Task Invoke (HttpContext context)
    {
        context.Response.Headers.Add("super-secure", new StringValues("enable"));
        return next(context);
    }
}