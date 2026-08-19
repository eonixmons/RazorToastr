using System.Text.Json.Serialization;

namespace RazorToastr;

/// <summary>
/// A single queued toast. Instances are serialised into TempData by
/// <see cref="ToastrExtensions"/> and read back on the next request by the
/// <c>&lt;toastr-messages /&gt;</c> tag helper, which is why the JSON property names are
/// pinned explicitly: they are a contract with <c>razor-toastr.js</c>.
/// </summary>
/// <param name="Level">Severity, deciding which toastr function renders the message.</param>
/// <param name="Message">Body text. Rendered as text by toastr, never as markup.</param>
/// <param name="Title">Optional heading shown above the body.</param>
public sealed record ToastMessage(
    [property: JsonPropertyName("level")] ToastLevel Level,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("title")] string? Title = null);
