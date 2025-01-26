namespace Test.Common
{
    public class Command
    {
        public string Name { get; set; }
        public Func<Task> Action { get; set; }
    }
}
