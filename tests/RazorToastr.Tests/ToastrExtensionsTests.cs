using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NUnit.Framework;

namespace RazorToastr.Tests;

[TestFixture]
public class ToastrExtensionsTests
{
    private ITempDataDictionary _tempData = null!;

    [SetUp]
    public void SetUp() => _tempData = TestDoubles.TempData();

    [Test]
    public void AddToast_StoresTheMessageUnderTheExpectedKey()
    {
        _tempData.AddToast(ToastLevel.Success, "Saved");

        Assert.That(_tempData.Peek(ToastrExtensions.TempDataKey), Is.Not.Null);
    }

    [Test]
    public void AddToast_KeepsEveryMessageWhenCalledRepeatedly()
    {
        _tempData.AddToast(ToastLevel.Success, "First");
        _tempData.AddToast(ToastLevel.Error, "Second");
        _tempData.AddToast(ToastLevel.Info, "Third");

        var consumed = _tempData.ConsumeToasts();

        Assert.That(consumed.Select(m => m.Message), Is.EqualTo(new[] { "First", "Second", "Third" }));
    }

    [Test]
    public void AddToast_PreservesLevelAndTitle()
    {
        _tempData.AddToast(ToastLevel.Warning, "Body text", "Heading");

        var consumed = _tempData.ConsumeToasts();

        Assert.That(consumed, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(consumed[0].Level, Is.EqualTo(ToastLevel.Warning));
            Assert.That(consumed[0].Message, Is.EqualTo("Body text"));
            Assert.That(consumed[0].Title, Is.EqualTo("Heading"));
        });
    }

    [Test]
    public void AddToast_LeavesTitleNullWhenOmitted()
    {
        _tempData.AddToast(ToastLevel.Info, "No heading");

        Assert.That(_tempData.ConsumeToasts()[0].Title, Is.Null);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("\t")]
    public void AddToast_RejectsABlankMessage(string message)
    {
        Assert.Throws<ArgumentException>(() => _tempData.AddToast(ToastLevel.Info, message));
    }

    [Test]
    public void AddToast_RejectsANullMessage()
    {
        Assert.Throws<ArgumentNullException>(() => _tempData.AddToast(ToastLevel.Info, null!));
    }

    [Test]
    public void ConsumeToasts_ReturnsAnEmptyListWhenNothingWasQueued()
    {
        Assert.That(_tempData.ConsumeToasts(), Is.Empty);
    }

    [Test]
    public void ConsumeToasts_ClearsTheQueueSoAToastIsShownOnce()
    {
        _tempData.AddToast(ToastLevel.Success, "Only once");

        var first = _tempData.ConsumeToasts();
        var second = _tempData.ConsumeToasts();

        Assert.Multiple(() =>
        {
            Assert.That(first, Has.Count.EqualTo(1));
            Assert.That(second, Is.Empty);
            Assert.That(_tempData.Peek(ToastrExtensions.TempDataKey), Is.Null);
        });
    }

    [Test]
    public void ConsumeToasts_TreatsACorruptPayloadAsAnEmptyQueue()
    {
        // A tampered or truncated TempData cookie must cost a lost toast, not a 500 on the page.
        _tempData[ToastrExtensions.TempDataKey] = "{ this is not the json we wrote";

        Assert.That(_tempData.ConsumeToasts(), Is.Empty);
    }

    [Test]
    public void SerialisedLevels_UseTheToastrFunctionNames()
    {
        // The level travels to razor-toastr.js as a string and is used to pick a toastr
        // function, so these names are a wire contract rather than an implementation detail.
        _tempData.AddToast(ToastLevel.Success, "a");
        _tempData.AddToast(ToastLevel.Info, "b");
        _tempData.AddToast(ToastLevel.Warning, "c");
        _tempData.AddToast(ToastLevel.Error, "d");

        var json = (string)_tempData.Peek(ToastrExtensions.TempDataKey)!;
        var levels = JsonDocument.Parse(json).RootElement
            .EnumerateArray()
            .Select(e => e.GetProperty("level").GetString())
            .ToArray();

        Assert.That(levels, Is.EqualTo(new[] { "success", "info", "warning", "error" }));
    }

    [Test]
    public void Serialisation_EscapesCharactersThatCouldEscapeAnHtmlAttribute()
    {
        _tempData.AddToast(ToastLevel.Error, "\"><script>alert(1)</script>");

        var json = (string)_tempData.Peek(ToastrExtensions.TempDataKey)!;

        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Not.Contain("<script>"));
            Assert.That(json, Does.Not.Contain("\"><"));
        });
    }

    [Test]
    public void RoundTrip_PreservesAMessageContainingMarkupAndQuotes()
    {
        const string hostile = "<b>bold</b> & \"quoted\" 'single' </div>";
        _tempData.AddToast(ToastLevel.Info, hostile);

        Assert.That(_tempData.ConsumeToasts()[0].Message, Is.EqualTo(hostile));
    }
}
