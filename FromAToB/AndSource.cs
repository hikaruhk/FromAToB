namespace FromAToB
{
    public class AndSource : IAndSource
    {
        public AndSource(ISource source)
        {
            GetParent = source;
        }

        public ISource GetParent { get; }
    }
}