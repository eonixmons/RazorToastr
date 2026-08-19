namespace RazorToastr;

/// <summary>
/// Severity of a toast. Each value maps onto the toastr function of the same name
/// (<c>toastr.success</c>, <c>toastr.info</c>, …), so the names are part of the wire
/// format shared with <c>razor-toastr.js</c> and must not be renamed lightly.
/// </summary>
public enum ToastLevel
{
    /// <summary>Rendered by <c>toastr.success</c>. An operation completed.</summary>
    Success,

    /// <summary>Rendered by <c>toastr.info</c>. Neutral information.</summary>
    Info,

    /// <summary>Rendered by <c>toastr.warning</c>. Something needs attention but nothing failed.</summary>
    Warning,

    /// <summary>Rendered by <c>toastr.error</c>. An operation failed.</summary>
    Error,
}
