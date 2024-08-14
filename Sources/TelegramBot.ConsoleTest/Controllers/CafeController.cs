using Telegram.Bot;
using TelegramBot.Builders;
using TelegramBot.Attributes;
using TelegramBot.Controllers;
using TelegramBot.Abstractions;

namespace TelegramBot.ConsoleTest.Controllers
{
    public class CafeController(ITelegramBotClient telegramBotClient) : BotControllerBase
    {
        const long adminId = 123;

        [TextCommand("/burgers")]
        public IActionResult HandleBurgers()
        {
            KeyboardBuilder builder = new();
            builder
                .WithColumns(2)
                .AddButton("🍔", "/burgers/1/drink/false")
                .AddButton("🍔🍔", "/burgers/2/drink/false")
                .AddButton("🍔🥤", "/burgers/1/drink/true")
                .AddButton("🍔🍔🥤", "/burgers/2/drink/true");
            return Inline("Choose your order:", builder.Build());
        }

        [InlineCommand("/burgers/{burgers}/drink/{drink}")]
        public async Task<IActionResult> HandleBurgersAsync(int burgers, bool drink)
        {
            const int burgerPrice = 5;
            const int drinkPrice = 2;
            int cookTime = Random.Shared.Next(18, 35);
            int totalPrice = burgers * burgerPrice + (drink ? drinkPrice : 0);

            string text = "🍽🍽🍽🍽🍽🍽🍽🍽🍽🍽🍽🍽\n" +
                "Vadim's Burgers got your order!\n\n" +
                "Your order:\n" +
                $"🍔 {burgers}x Vadim's Burger\n" +
                (drink ? "🥤 1x Drink\n" : "No drink\n\n") +
                $"Total: ${totalPrice}.00\n\n" +
                "The best burgers will be ready in " + cookTime + " minutes\n" +
                "🍽🍽🍽🍽🍽🍽🍽🍽🍽🍽🍽🍽";

            string sender = Update.Message?.From?.Username ?? "Anonymous";
            string orderText = $"Order from {sender}:\n" +
                $"🍔 {burgers}x Vadim's Burger\n" +
                (drink ? "🥤 1x Drink\n" : "No drink\n") +
            $"Total: ${totalPrice}.00\n" +
                "Send /burgersdone when you are ready to pick up";

            await telegramBotClient.SendTextMessageAsync(adminId, orderText);
            SetValue("customerId", User.Id);
            await telegramBotClient.DeleteMessageAsync(User.Id, Update.CallbackQuery!.Message!.MessageId);
            return Text(text);
        }

        [TextCommand("/burgersdone")]
        public async Task<IActionResult> HandleBurgersDoneAsync()
        {
            const string text = "🍽🍔🥤🍔🥤🍔🍽\n\n" +
                "Your order is ready!\n" +
                "Please pick up your order at the living room.\n" +
                "Enjoy your meal!\n\n" +
                "🍽🍔🥤🍔🥤🍔🍽";
            long customerId = GetValue<long>("customerId");
            if (customerId == 0)
            {
                return Text("You have no active orders");
            }
            await telegramBotClient.SendTextMessageAsync(customerId, text);
            return Text(text);
        }
    }
}
