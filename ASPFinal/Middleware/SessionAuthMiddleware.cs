using ASPFinal.Data;
using ASPFinal.Data.Entity;
using System.Security.Claims;

namespace ASPFinal.Middleware
{
    public class SessionAuthMiddleware
    {
        /* Для утворення ланцюга кожна ланка (об'єкт Middleware) отримує посилання на наступну ланку.
         * Це посилання передається через конструктор і має бути збережене у об'єкті
         */
        private readonly RequestDelegate _next;

        /* Middleware існує протягом всієх роботи програми (Singleton), тому інжекція сервісів через конструктор не здійснюється */
        public SessionAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /* Обов'язковий для Middleware метод InvokeAsync або Invoke (старий підхід)
         * Перший параметр - завжди HttpContext context,
         * за потреби, наступні параметри - інжекційні
         */
        public async Task InvokeAsync(HttpContext context, ILogger<SessionAuthMiddleware> logger, DataContext dataContext)
        {
            //logger.LogInformation("SessionAuthMiddleware works");
            
            // Перевіряємо наявність у сесії "AuthUserId" (встановлюється при автентифікації у UserController::AuthUser() )
            string? userId = context.Session.GetString("authUserId");
            if(userId is not null)
            {
                try
                {
                    User? authUser = dataContext.Users.Find(Guid.Parse(userId));
                    if (authUser is not null) 
                    {
                        context.Items.Add("AuthUser", authUser);
                        /* Передача відомостей про користувача шляхом посилання на об'єкт-сутність (Entity) підвищує зчеплення (залежність від реалізацій),
                         * а також поширює відомості про "технічну" сутність User, потрібну для ORM, на весь проєкт, де потрібна авторизація.
                         * Для уніфікації існує механізм "тверджень" (Claims).
                         * При автентифікації користувачу задаються певні Claims, а при авторизації перевіряється наявність потрібних з них (наприклад: вік, стать, номер телефону тощо)
                         */
                        Claim[] claims = new Claim[]
                        {
                            new Claim(ClaimTypes.Sid, userId),
                            new Claim(ClaimTypes.Name, authUser.Name),
                            new Claim(ClaimTypes.NameIdentifier, authUser.Login),
                            new Claim(ClaimTypes.Email, authUser.Email),
                            new Claim(ClaimTypes.UserData, authUser.Avatar ?? String.Empty),
                            new Claim("IsConfirmed", authUser.EmailCode ?? String.Empty)
                        };
                        /* Створюємо власника (Principal) із даними твердженнями */
                        var principal = new ClaimsPrincipal(
                            new ClaimsIdentity(
                                claims,
                                nameof(SessionAuthMiddleware)));
                        /* У HttpContext є вбудоване поле User з типом Principal */
                        /* Встановлення його дозволить задіяти ASP механізми авторизації */
                        context.User = principal;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "SessionAuthMiddleware");
                }
            }

            /* Для продовження ланцюга метод має викликати натсупну ланку Middleware */
            await _next(context);
        }
    }
    /* Включення класу SessionAuthMiddleware у ланцюг Middleware здійснюється у Phjgram.cs командою:
     * app.UseMiddleware<SessionAuthMiddleware>();
     * Традиційно Middleware забезпечують розширеннями (extensions) для вживання UseXxxxx формалізму
     */
    public static class SessionAuthMiddlewareExtension
    {
        public static IApplicationBuilder UseSessionAuth(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SessionAuthMiddleware>();
        }
    }
}
/*
 Middleware - ПЗ проміжного рівня
 */
