# Global Exception Handling

- A production-shaped global exception handler built from scratch, using ASP.NET Core's
  IExceptionHandler (introduced in .NET 8), to understand what the framework actually
  does internally before relying on it.

## The problem this solves

- Unhandled exceptions may leak sensitive information such as connection-strings, stack trace etc.
  Global exception handler catches every exception through the pipeline and returns rather a generic written ProblemDetails(JSON) or a specific one
  thus guarantees no sensitive information leaks. 

## Usage

[Registration snippet from Program.cs — AddExceptionHandler x2, AddProblemDetails, UseExceptionHandler]

-  Registers the handler classes as available services. On its own this does
   nothing to the pipeline — it just makes them available for injection.
   Order matters: AppExceptionHandler first, so specific exceptions get
   specific status codes; FallbackExceptionHandler last, since it accepts
   every exception unconditionally.
   
builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddExceptionHandler<FallbackExceptionHandler>();
 
- Enables IProblemDetailsService, used by both handlers to write responses.

builder.Services.AddProblemDetails();
 
- Inserts ExceptionHandlerMiddleware into the pipeline. This is what actually
  wraps everything downstream in a try/catch and invokes the registered
  IExceptionHandler services above, in order, when something throws.
  
app.UseExceptionHandler();

## Architecture

- Two handlers run in a chain, tried in registration order:

1. **AppExceptionHandler** — recognizes anything inheriting from `AppException`.
   Returns false for anything else, passing it to the next handler(**FallbackExceptionHandler**).
2. **FallbackExceptionHandler** — catches everything else. Always returns true.
   Never exposes the real exception message to the client.

## Adding a new expected exception type

1. Create a class inheriting AppException, implement StatusCode.
2. Throw it from your code.
3. AppExceptionHandler picks it up automatically — no other changes needed.

## Design notes

- `AppException` exists to give every expected application error a common type
  and a shared `StatusCode`, so `AppExceptionHandler` can handle any current or
  future subtype with one type check, instead of switching on each concrete
  exception type individually.
- Registration order determines chain position: `AppExceptionHandler` must run
  first, since `FallbackExceptionHandler` accepts every exception unconditionally
  — if it ran first, nothing else in the chain would ever get a turn.
- `ArgumentException` is deliberately left unmapped. It represents a programming
  mistake (bad input passed by code), not a user-facing validation failure, so
  it correctly falls through to the generic 500 rather than getting its own
  friendly status code.
- Client-caused failures log at `Warning` (`AppExceptionHandler`); unexpected
  ones log at `Error` (`FallbackExceptionHandler`) — kept separate on purpose,
  so production alerting can page on real failures without noise from routine,
  expected errors like "not found."