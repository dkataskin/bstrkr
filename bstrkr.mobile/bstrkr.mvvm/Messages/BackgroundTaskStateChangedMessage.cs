using Cirrious.MvvmCross.Plugins.Messenger;

namespace bstrkr.mvvm.messages
{
    public class BackgroundTaskStateChangedMessage : MvxMessage
    {
        public BackgroundTaskStateChangedMessage(object sender, string taskId, BackgroundTaskState state) : base(sender)
        {
            this.TaskId = taskId;
            this.TaskState = state;
        }

        public string TaskId { get; private set; }

        public BackgroundTaskState TaskState { get; private set; }
    }
}