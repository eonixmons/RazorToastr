using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;

namespace RazorToastr.Tests;

/// <summary>
/// Minimal scaffolding to exercise the queueing and rendering sides without an ASP.NET Core host.
/// </summary>
internal static class TestDoubles
{
    /// <summary>Creates an empty TempData dictionary backed by a no-op provider.</summary>
    internal static TempDataDictionary TempData()
        => new(new DefaultHttpContext(), new NoOpTempDataProvider());

    /// <summary>Creates a view context exposing <paramref name="tempData"/> to a tag helper.</summary>
    internal static ViewContext ViewContext(ITempDataDictionary tempData)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());

        var viewData = new ViewDataDictionary(
            new EmptyModelMetadataProvider(),
            new ModelStateDictionary());

        return new ViewContext(
            actionContext,
            new StubView(),
            viewData,
            tempData,
            TextWriter.Null,
            new HtmlHelperOptions());
    }

    /// <summary>Creates the context and output pair a tag helper's Process method expects.</summary>
    internal static (TagHelperContext Context, TagHelperOutput Output) TagHelperCall()
    {
        var context = new TagHelperContext(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "toastr-messages",
            new TagHelperAttributeList(),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        return (context, output);
    }

    /// <summary>
    /// Renders a tag helper's output to HTML exactly as Razor would, so tests can assert on the
    /// markup that actually reaches the browser rather than on in-memory attribute values.
    /// </summary>
    internal static string RenderToHtml(this TagHelperOutput output)
    {
        using var writer = new StringWriter();
        output.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    private sealed class NoOpTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object?> LoadTempData(HttpContext context)
            => new Dictionary<string, object?>();

        public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
        {
            // Nothing to persist: tests inspect the dictionary directly.
        }
    }

    private sealed class StubView : IView
    {
        public string Path => "/stub";

        public Task RenderAsync(ViewContext context) => Task.CompletedTask;
    }
}
