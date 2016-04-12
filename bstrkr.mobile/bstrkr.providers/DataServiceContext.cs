namespace bstrkr.providers
{
    public class DataServiceContext
    {
        public DataServiceContext(string currentUICultureThreeLetterISOName)
        {
            this.CurrentUIThreeLetterISOName = currentUICultureThreeLetterISOName;
        }

        public string CurrentUIThreeLetterISOName { get; private set; }
    }
}