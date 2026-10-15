namespace Nexo.NET
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            _ = NexoSignature.Id;

            var chatClient = new ChatClient();
            Application.Run(new SalaChat(chatClient));
        }
    }
}