using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorToastr;

/// <summary>
/// Renders the toasts queued for this request. Place <c>&lt;toastr-messages /&gt;</c> once in
/// the layout, after toastr itself and after <c>razor-toastr.js</c>.
/// </summary>
/// <remarks>
/// The queue is written into a <c>data-</c> attribute rather than emitted as an inline
/// <c>&lt;script&gt;</c>. That is the whole point of this tag helper: a site running a strict
/// Content-Security-Policy — one without <c>unsafe-inline</c> in <c>script-src</c> — would see
/// inline toasts silently blocked. Here the only executable code is the packaged asset, served
/// from its own URL and covered by <c>script-src 'self'</c>.
/// </remarks>
[HtmlTargetElement(TagName, TagStructure = TagStructure.WithoutEndTag)]
public sealed class ToastrMessagesTagHelper : TagHelper
{
    /// <summary>Element that activates this tag helper.</summary>
    private const string TagName = "toastr-messages";

    /// <summary>
    /// <c>id</c> of the emitted element. Part of the contract with <c>razor-toastr.js</c>.
    /// </summary>
    internal const string ElementId = "razor-toastr";

    /// <summary>
    /// Attribute carrying the JSON queue. Part of the contract with <c>razor-toastr.js</c>.
    /// </summary>
    internal const string DataAttribute = "data-razor-toastr";

    /// <summary>Ambient view context, supplying the TempData holding the queue.</summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = default!;

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        var messages = ViewContext.TempData.ConsumeToasts();
        if (messages.Count == 0)
        {
            // Nothing queued: emit no markup at all rather than an empty placeholder.
            output.SuppressOutput();
            return;
        }

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("id", ElementId);

        // Second of the two escaping layers: SetAttribute HTML-encodes the value, on top of the
        // \uXXXX escaping already applied by the JSON serialiser.
        output.Attributes.SetAttribute(
            DataAttribute,
            JsonSerializer.Serialize(messages, ToastrExtensions.JsonOptions));

        // The element is a data carrier, never something the user should see.
        output.Attributes.SetAttribute("hidden", "hidden");
    }
}
