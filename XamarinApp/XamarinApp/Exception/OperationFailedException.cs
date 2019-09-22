namespace XamarinApp.Exception
{
    public class OperationFailedException : System.Exception
    {
        public OperationFailedException(string message = null) : base(message)
        {
        }
    }
}
