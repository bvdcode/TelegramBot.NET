using System.Net;
using System.Text;

namespace TelegramBot.Tests
{
    public class RecordingBotHttpHandler : HttpMessageHandler
    {
        public List<string> Methods { get; } = [];

        public List<string> Payloads { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            string method = request.RequestUri!.Segments.Last();
            Methods.Add(method);
            Payloads.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
            string response = method == "answerCallbackQuery"
                ? "{\"ok\":true,\"result\":true}"
                : "{\"ok\":true,\"result\":{\"message_id\":17,\"date\":1,\"chat\":{\"id\":1001,\"type\":\"private\"}}}";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response, Encoding.UTF8, "application/json"),
            };
        }
    }
}
