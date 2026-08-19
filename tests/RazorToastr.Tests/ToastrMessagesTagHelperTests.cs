using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NUnit.Framework;

namespace RazorToastr.Tests;

[TestFixture]
public class ToastrMessagesTagHelperTests
{
    private ITempDataDictionary _tempData = null!;
    private ToastrMessagesTagHelper _tagHelper = null!;

    [SetUp]
    public void SetUp()
    {
        _tempData = TestDoubles.TempData();
        _tagHelper = new ToastrMessagesTagHelper { ViewContext = TestDoubles.ViewContext(_tempData) };
    }

    [Test]
    public void Process_EmitsNoMarkupWhenNothingIsQueued()
    {
        var (context, output) = TestDoubles.TagHelperCall();

        _tagHelper.Process(context, output);

        Assert.That(output.RenderToHtml(), Is.Empty);
    }

    [Test]
    public void Process_EmitsAHiddenDataCarryingDiv()
    {
        _tempData.AddToast(ToastLevel.Success, "Saved");
        var (context, output) = TestDoubles.TagHelperCall();

        _tagHelper.Process(context, output);
        var html = output.RenderToHtml();

        Assert.Multiple(() =>
        {
            Assert.That(html, Does.StartWith("<div "));
            Assert.That(html, Does.Contain($"id=\"{ToastrMessagesTagHelper.ElementId}\""));
            Assert.That(html, Does.Contain(ToastrMessagesTagHelper.DataAttribute));
            Assert.That(html, Does.Contain("hidden"));
        });
    }

    [Test]
    public void Process_EmitsNoScriptElementOrInlineHandler()
    {
        // The reason this tag helper exists: a strict CSP without unsafe-inline must not block
        // the toasts, which requires the rendered markup to carry no executable code at all.
        _tempData.AddToast(ToastLevel.Error, "Failed");
        var (context, output) = TestDoubles.TagHelperCall();

        _tagHelper.Process(context, output);
        var html = output.RenderToHtml();

        Assert.Multiple(() =>
        {
            Assert.That(html, Does.Not.Contain("<script"));
            Assert.That(html, Does.Not.Contain("javascript:"));
            // The emitted element carries data and nothing else: no event handler attribute
            // could smuggle executable code past a script-src directive.
            Assert.That(output.Attributes.Select(a => a.Name), Is.EquivalentTo(
                new[] { "id", ToastrMessagesTagHelper.DataAttribute, "hidden" }));
        });
    }

    [Test]
    public void Process_ClearsTheQueueSoAReloadShowsNothing()
    {
        _tempData.AddToast(ToastLevel.Info, "Once");

        var (firstContext, firstOutput) = TestDoubles.TagHelperCall();
        _tagHelper.Process(firstContext, firstOutput);

        var (secondContext, secondOutput) = TestDoubles.TagHelperCall();
        _tagHelper.Process(secondContext, secondOutput);

        Assert.Multiple(() =>
        {
            Assert.That(firstOutput.RenderToHtml(), Is.Not.Empty);
            Assert.That(secondOutput.RenderToHtml(), Is.Empty);
        });
    }

    [Test]
    public void Process_RendersEveryQueuedMessage()
    {
        _tempData.AddToast(ToastLevel.Success, "First");
        _tempData.AddToast(ToastLevel.Error, "Second");
        var (context, output) = TestDoubles.TagHelperCall();

        _tagHelper.Process(context, output);
        var html = output.RenderToHtml();

        Assert.Multiple(() =>
        {
            Assert.That(html, Does.Contain("First"));
            Assert.That(html, Does.Contain("Second"));
        });
    }

    [Test]
    public void Process_NeutralisesAMessageCraftedToBreakOutOfTheAttribute()
    {
        // Worth pinning down: admin toasts routinely echo user-supplied data such as an animal
        // name or an abuse report, so a stored payload can reach this attribute.
        _tempData.AddToast(ToastLevel.Error, "\"><script>alert(1)</script><div x=\"");
        var (context, output) = TestDoubles.TagHelperCall();

        _tagHelper.Process(context, output);
        var html = output.RenderToHtml();

        Assert.Multiple(() =>
        {
            Assert.That(html, Does.Not.Contain("<script"));
            // The only angle brackets in the output are the two the tag helper opened and closed
            // itself. Anything the payload contributed came through escaped, so it stayed inside
            // the attribute value instead of becoming markup.
            Assert.That(html.Count(c => c == '<'), Is.EqualTo(2), html);
            Assert.That(html, Does.StartWith($"<div id=\"{ToastrMessagesTagHelper.ElementId}\""));
            Assert.That(html, Does.EndWith("</div>"));
        });
    }
}
