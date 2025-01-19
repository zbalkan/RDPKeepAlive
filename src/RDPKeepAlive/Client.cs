namespace RDPKeepAlive
{
    internal readonly struct Client
    {
        public string ClassName { get; }

        public string WindowTitle { get; }

        public Client(string className, string windowTitle) : this()
        {
            ClassName = className;
            WindowTitle = windowTitle;
        }
    }
}