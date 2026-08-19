using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace RazorToastr;

/// <summary>
/// Queues toasts from a page handler or a controller action. Messages are stored in
/// TempData, so they survive the redirect of a post-redirect-get and are rendered by the
/// <c>&lt;toastr-messages /&gt;</c> tag helper on the next request.
/// </summary>
public static class ToastrExtensions
{
    /// <summary>TempData entry holding the queue, as a JSON array.</summary>
    internal const string TempDataKey = "RazorToastr.Messages";

    /// <summary>
    /// Serialiser shared by the queueing and rendering sides.
    /// </summary>
    /// <remarks>
    /// The default <see cref="System.Text.Encodings.Web.JavaScriptEncoder"/> is deliberate:
    /// it escapes <c>&lt;</c>, <c>&gt;</c>, <c>&amp;</c>, <c>'</c> and <c>"</c> as <c>\uXXXX</c>,
    /// which is the first of the two layers keeping a hostile message from breaking out of the
    /// HTML attribute it is written into. Never swap it for
    /// <c>JavaScriptEncoder.UnsafeRelaxedJsonEscaping</c>.
    /// </remarks>
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    /// <summary>Queues a success toast, rendered by <c>toastr.success</c>.</summary>
    /// <param name="page">The page queueing the toast.</param>
    /// <param name="message">Body text. Displayed as text, never interpreted as markup.</param>
    /// <param name="title">Optional heading shown above the body.</param>
    public static void ToastSuccess(this PageModel page, string message, string? title = null)
        => page.AddToast(ToastLevel.Success, message, title);

    /// <summary>Queues an informational toast, rendered by <c>toastr.info</c>.</summary>
    /// <param name="page">The page queueing the toast.</param>
    /// <param name="message">Body text. Displayed as text, never interpreted as markup.</param>
    /// <param name="title">Optional heading shown above the body.</param>
    public static void ToastInfo(this PageModel page, string message, string? title = null)
        => page.AddToast(ToastLevel.Info, message, title);

    /// <summary>Queues a warning toast, rendered by <c>toastr.warning</c>.</summary>
    /// <param name="page">The page queueing the toast.</param>
    /// <param name="message">Body text. Displayed as text, never interpreted as markup.</param>
    /// <param name="title">Optional heading shown above the body.</param>
    public static void ToastWarning(this PageModel page, string message, string? title = null)
        => page.AddToast(ToastLevel.Warning, message, title);

    /// <summary>Queues an error toast, rendered by <c>toastr.error</c>.</summary>
    /// <param name="page">The page queueing the toast.</param>
    /// <param name="message">Body text. Displayed as text, never interpreted as markup.</param>
    /// <param name="title">Optional heading shown above the body.</param>
    public static void ToastError(this PageModel page, string message, string? title = null)
        => page.AddToast(ToastLevel.Error, message, title);

    /// <summary>Queues a toast of the given severity.</summary>
    /// <param name="page">The page queueing the toast.</param>
    /// <param name="level">Severity, deciding which toastr function renders the message.</param>
    /// <param name="message">Body text. Displayed as text, never interpreted as markup.</param>
    /// <param name="title">Optional heading shown above the body.</param>
    /// <exception cref="ArgumentNullException"><paramref name="page"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="message"/> is null or blank.</exception>
    public static void AddToast(this PageModel page, ToastLevel level, string message, string? title = null)
    {
        ArgumentNullException.ThrowIfNull(page);
        page.TempData.AddToast(level, message, title);
    }

    /// <summary>Queues a success toast, rendered by <c>toastr.success</c>.</summary>
    /// <param name="controller">The controller queueing the toast.</param>
    /// <param name="message">Body text. Displayed as text, never interpreted as markup.</param>
    /// <param name="title">Optional heading shown above the body.</param>
    public static void ToastSuccess(this Controller controller, string message, string? title = null)
        => controller.AddToast(ToastLevel.Success, message, title);

    /// <summary>Queues an informational toast, rendered by <c>toastr.info</c>.</summary>
    /// <param name="controller">The controller queueing the toast.</param>
    /// <param name="message">Body text. Displayed as text, never interpreted as markup.</param>
    /// <param name="title">Optional heading shown above the body.</param>
    public static void ToastInfo(this Controller controller, string message, string? title = null)
        => controller.AddToast(ToastLevel.Info, message, title);

    /// <summary>Queues a warning toast, rendered by <c>toastr.warning</c>.</summary>
    /// <param name="controller">The controller queueing the toast.</param>
    /// <param name="message">Body text. Displayed as text, never interpreted as markup.</param>
    /// <param name="title">Optional heading shown above the body.</param>
    public static void ToastWarning(this Controller controller, string message, string? title = null)
        => controller.AddToast(ToastLevel.Warning, message, title);

    /// <summary>Queues an error toast, rendered by <c>toastr.error</c>.</summary>
    /// <param name="controller">The controller queueing the toast.</param>
    /// <param name="message">Body text. Displayed as text, never interpreted as markup.</param>
    /// <param name="title">Optional heading shown above the body.</param>
    public static void ToastError(this Controller controller, string message, string? title = null)
        => controller.AddToast(ToastLevel.Error, message, title);

    /// <summary>Queues a toast of the given severity.</summary>
    /// <param name="controller">The controller queueing the toast.</param>
    /// <param name="level">Severity, deciding which toastr function renders the message.</param>
    /// <param name="message">Body text. Displayed as text, never interpreted as markup.</param>
    /// <param name="title">Optional heading shown above the body.</param>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="message"/> is null or blank.</exception>
    public static void AddToast(this Controller controller, ToastLevel level, string message, string? title = null)
    {
        ArgumentNullException.ThrowIfNull(controller);
        controller.TempData.AddToast(level, message, title);
    }

    /// <summary>
    /// Queues a toast straight onto a TempData dictionary. Use this from a filter, a middleware
    /// or anywhere else without a <see cref="PageModel"/> or <see cref="Controller"/> at hand.
    /// </summary>
    /// <param name="tempData">Dictionary carrying the queue to the next request.</param>
    /// <param name="level">Severity, deciding which toastr function renders the message.</param>
    /// <param name="message">Body text. Displayed as text, never interpreted as markup.</param>
    /// <param name="title">Optional heading shown above the body.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tempData"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="message"/> is null or blank.</exception>
    public static void AddToast(
        this ITempDataDictionary tempData,
        ToastLevel level,
        string message,
        string? title = null)
    {
        ArgumentNullException.ThrowIfNull(tempData);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        // Peek rather than the indexer: reading through the indexer marks the entry as consumed,
        // which would drop the queue if nothing else wrote to TempData afterwards.
        var queued = Deserialize(tempData.Peek(TempDataKey) as string);
        queued.Add(new ToastMessage(level, message, title));
        tempData[TempDataKey] = JsonSerializer.Serialize(queued, JsonOptions);
    }

    /// <summary>
    /// Returns the queued toasts and clears the queue, so a toast is shown exactly once.
    /// </summary>
    internal static IReadOnlyList<ToastMessage> ConsumeToasts(this ITempDataDictionary tempData)
    {
        ArgumentNullException.ThrowIfNull(tempData);

        var queued = Deserialize(tempData.Peek(TempDataKey) as string);
        tempData.Remove(TempDataKey);
        return queued;
    }

    /// <summary>
    /// Reads a queue back from its JSON form. A payload we cannot parse is treated as an empty
    /// queue: a corrupt or hand-edited TempData cookie should cost the user a lost toast, not a
    /// 500 on an otherwise healthy page.
    /// </summary>
    private static List<ToastMessage> Deserialize(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<ToastMessage>>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
