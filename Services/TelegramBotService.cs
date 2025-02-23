using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ContaBle.Data;
using ContaBle.Models;
using Microsoft.AspNetCore.Identity;

namespace ContaBle.Services
{
    public class TelegramBotService : IHostedService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TelegramBotService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly long _botOwnerId = 1218100348; // 🔹 Reemplaza con TU ID de Telegram

        public TelegramBotService(IServiceScopeFactory scopeFactory, ILogger<TelegramBotService> logger)
        {
            _botClient = new TelegramBotClient("7329892510:AAGQ6UEV8P5Dk82nTulYSkEb7rmpD09glAo"); // 🔹 Usa tu token real
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // Permite recibir todos los tipos de actualizaciones
            };

            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );

            _logger.LogInformation("✅ Bot de Telegram iniciado con Long Polling");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("⛔ Bot de Telegram detenido.");
            return Task.CompletedTask;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text.Trim();
                var username = update.Message.Chat.Username ?? "Desconocido";
                var userId = update.Message?.From?.Id ?? 0;

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                    // Comando para dar la bienvenida
                    if (messageText.Equals("/inicio", StringComparison.OrdinalIgnoreCase))
                    {
                        string welcomeMessage = "¡Bienvenido a ContaBle! Para continuar, utiliza el comando /verificar <código> con el código que se te ha asignado.";
                        await botClient.SendMessage(chatId, welcomeMessage, cancellationToken: cancellationToken);
                        return;
                    }

                    // Comando de verificación
                    if (messageText.StartsWith("/verificar", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = messageText.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length < 2)
                        {
                            await botClient.SendMessage(chatId, "⚠️ *Uso correcto:* `/verificar CÓDIGO`", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                            return;
                        }

                        var verificationCode = parts[1];

                        // Buscar al usuario con ese código (usamos el ID, que es el código de verificación)
                        var user = await userManager.FindByIdAsync(verificationCode);
                        if (user == null)
                        {
                            await botClient.SendMessage(chatId, "❌ *Código de verificación inválido.*", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                            return;
                        }

                        // Marcar como verificado y asociar el chat de Telegram
                        user.TelegramVerified = true;
                        user.TelegramChatId = chatId;
                        await userManager.UpdateAsync(user);

                        await botClient.SendMessage(chatId,
                            "✅ *¡Cuenta verificada con éxito!*\n\n🎉 Ahora puedes iniciar sesión en *ContaBle* sin problemas.\n\n📌 Si necesitas ayuda, contacta con soporte.",
                            parseMode: ParseMode.Markdown,
                            cancellationToken: cancellationToken);
                        return;
                    }

                    // Comando para ver usuarios (solo el dueño del bot)
                    if (messageText.Equals("/usuarios", StringComparison.OrdinalIgnoreCase))
                    {
                        if (update.Message?.From?.Id != _botOwnerId)
                        {
                            await botClient.SendMessage(chatId, "❌ No tienes permisos para ver esta información.", cancellationToken: cancellationToken);
                            return;
                        }

                        var users = await dbContext.Users.Select(u => u.NormalizedUserName).ToListAsync(cancellationToken);
                        if (users.Count == 0)
                        {
                            await botClient.SendMessage(chatId, "📌 *No hay usuarios registrados aún.*", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                        }
                        else
                        {
                            string userList = "📋 *Usuarios Registrados:*\n\n" + string.Join("\n", users);
                            await botClient.SendMessage(chatId, userList, parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                        }
                        return;
                    }

                    // Si no se reconoce ningún comando, se envía un mensaje genérico.
                    await botClient.SendMessage(chatId, "Comando no reconocido.", cancellationToken: cancellationToken);
                }
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "❌ Error en el bot");
            return Task.CompletedTask;
        }
    }
}
