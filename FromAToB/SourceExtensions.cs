namespace FromAToB
{
    public static class SourceExtensions
    {
        public static IAndSource And(this ISource @source) =>
            new AndSource(@source);
    }
}