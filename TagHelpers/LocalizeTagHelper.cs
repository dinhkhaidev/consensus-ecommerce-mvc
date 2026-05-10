using Microsoft.AspNetCore.Razor.TagHelpers;
using WebActionResults.Services;

namespace WebActionResults.TagHelpers;

[HtmlTargetElement("localize")]
public class LocalizeTagHelper : TagHelper
{
    private readonly ILocalizationService _localizationService;

    public LocalizeTagHelper(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    [HtmlAttributeName("key")]
    public string? Key { get; set; }

    [HtmlAttributeName("args")]
    public string? Args { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;

        var text = !string.IsNullOrEmpty(Key) ? _localizationService.Get(Key) : string.Empty;

        if (!string.IsNullOrEmpty(Args))
        {
            var args = Args.Split(',');
            text = string.Format(text, args);
        }

        output.Content.SetContent(text);
    }
}