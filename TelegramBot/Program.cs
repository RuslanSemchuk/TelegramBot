using NCalc;
using System;
using System.Linq.Expressions;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

var botClient = new TelegramBotClient("6677557956:AAGJ2o55sJJxO_YtNZK61vJ1NB4KlOqxVWo");

using CancellationTokenSource cts = new();

ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>()
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

cts.Cancel();

static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    string calculateCommandId = "/calculate";
    string helpCommandId = "/help";

    InlineKeyboardMarkup messageButtons =
       new(
           new[]
           {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "/help", callbackData: helpCommandId),
                    InlineKeyboardButton.WithCallbackData(text: "/calculate", callbackData: calculateCommandId)

                }
           }
       )
    ;

    if (update.CallbackQuery is not null)
    {
        var callbackChatId = update.CallbackQuery.Message.Chat.Id;
        Console.WriteLine(update.CallbackQuery.Data);
        if (update.CallbackQuery.Data != helpCommandId)
        {
            Console.WriteLine($"Do help");

            await botClient.SendTextMessageAsync(
                chatId: callbackChatId,
                text: "This bot can help evaluate expressions. To start, click 'calculate' then enter your equation.",
                cancellationToken: cancellationToken
            );

        }
        if (update.CallbackQuery.Data != calculateCommandId)
        {
            Console.WriteLine($"Do calculate");

            await botClient.SendTextMessageAsync(
                chatId: callbackChatId,
                text: "Enter what you want to calculate.",
                cancellationToken: cancellationToken
            );
        }
    }

    if (update.Message is not null)
    {
        Message message = update.Message;
        var chatId = update.Message.Chat.Id;
        if (message.Text.StartsWith(""))
        {
            await botClient.SendTextMessageAsync(
                  chatId: chatId,
                  text: "Choose what you want to do:",
                  replyMarkup: messageButtons,
                  cancellationToken: cancellationToken
              );
        }

    }
}

async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
}



static double CalculateExpression(string expression)
{
    try
    {
        NCalc.Expression expr = new NCalc.Expression(expression);
        object result = expr.Evaluate();

        if (result is int intValue)
        {
            return (double)intValue;
        }
        else if (result is double doubleValue)
        {
            return doubleValue;
        }
        else
        {
            throw new Exception("Unable to calculate the result.");
        }
    }
    catch (Exception ex)
    {
        throw new Exception("Calculation error: " + ex.Message);
    }
}

static bool IsUnsafeExpression(string expression)
{
    return expression.Contains("DELETE", StringComparison.OrdinalIgnoreCase);
}
