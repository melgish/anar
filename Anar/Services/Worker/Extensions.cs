namespace Anar.Services.Worker;

internal static class Extensions
{
    // Add some conditional handling to PointData

    /// <summary>
    /// When predicate is true, apply action to T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj">PointData instance</param>
    /// <param name="predicate">Condition to test</param>
    /// <param name="action">Action to execute when true</param>
    /// <returns></returns>
    public static T When<T>(this T obj, Func<T, bool> predicate, Func<T, T> action) =>
        predicate(obj) ? action(obj) : obj;
}