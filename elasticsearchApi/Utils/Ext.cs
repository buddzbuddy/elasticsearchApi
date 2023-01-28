namespace elasticsearchApi.Utils
{
    public static class MyExtensions
{
    /// <summary>
    ///     A string extension method that query if '@this' is empty.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>true if empty, false if not.</returns>
    public static bool IsEmpty(this string @this)
    {
        return string.IsNullOrEmpty(@this);
    }
    public static bool IsNullOrEmpty(this object @this)
    {
        return @this == null || string.IsNullOrEmpty(@this.ToString());
    }
}
}