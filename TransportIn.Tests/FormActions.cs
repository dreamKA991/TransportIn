using AngleSharp.Dom;
using Bunit;

namespace TransportIn.Tests.Persistence;

internal static class FormActions
{
    public static IElement Field(this IRenderedFragment form, string placeholder, int index = 0) =>
        form.FindAll($"[placeholder='{placeholder}']")[index];

    public static void Fill(this IRenderedFragment form, string placeholder, string value, int index = 0) =>
        form.Field(placeholder, index).Change(value);

    public static void ClickButton(this IRenderedFragment form, string text) =>
        form.FindAll("button").Single(button => button.TextContent.Trim() == text).Click();

    public static IElement SelectWithOption(this IRenderedFragment form, string value) =>
        form.FindAll("select").Single(select =>
            select.QuerySelector($"option[value='{value}']") is not null);

    public static void CheckSavedFields(params (string Field, bool Saved)[] fields)
    {
        var missing = fields.Where(field => !field.Saved).Select(field => field.Field).ToList();
        Assert.True(missing.Count == 0,
            $"Поля відсутні або мають інше значення в PostgreSQL: {string.Join(", ", missing)}.");
    }
}
