namespace Fortnite_Optimizer
{
    internal class PopUp
    {
        public static void Show(string content)
        {
            Notification.Content = content;
            new Notification().ShowDialog();
        }
    }
}