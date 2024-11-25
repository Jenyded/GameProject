namespace Windows
{
    public class WindowsResolver
    {
        public WindowsResolver()
        {
        }

        public InfoWindow.Model GetInfoWindowModel(string message)
        {
            return new(message);
        }
    }
}