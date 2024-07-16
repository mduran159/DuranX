using System.Collections.Concurrent;

namespace YarpApiGateway.Middlewares
{
    public class ClientRateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        public ClientRateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string clientIp = context.Connection.RemoteIpAddress.ToString();

            // Obtener el semáforo para esta IP
            SemaphoreSlim semaphore = _semaphores.GetOrAdd(clientIp, new SemaphoreSlim(5));

            try
            {
                // Esperar para adquirir el semáforo con un timeout de 20 segundos
                var timeout = TimeSpan.FromSeconds(20);
                var acquireTask = semaphore.WaitAsync();
                var timeoutTask = Task.Delay(timeout);

                // Esperar la primera tarea que se complete: adquisición del semáforo o timeout
                var completedTask = await Task.WhenAny(acquireTask, timeoutTask);

                // Si la adquisición del semáforo completó primero, procesar la solicitud
                if (completedTask == acquireTask)
                {
                    await _next(context);
                }
                else
                {
                    // Si el timeout completó primero, devolver un error de timeout
                    context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
                    await context.Response.WriteAsync("Timeout waiting for semaphore.");
                }
            }
            finally
            {
                // Liberar el semáforo cuando se completa la solicitud
                semaphore.Release();
            }
        }
    }


}
