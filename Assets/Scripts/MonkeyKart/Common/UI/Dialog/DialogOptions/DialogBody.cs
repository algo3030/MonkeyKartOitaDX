namespace MonkeyKart.Common.UI
{
    public abstract class DialogBody { }

    class NoneBody: DialogBody { }

    class MessageBody : DialogBody
    {
        public string Message { get; private set; }
        public MessageBody(string message)
        {
            Message = message;
        }
    }
}