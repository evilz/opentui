using Microsoft.Extensions.Hosting;
using OpenTui.Razor.Hosting;
using OpenTui.Razor.Samples.Components;

var sample = args.Length > 0 ? args[0].ToLowerInvariant() : "layout";

var builder = Host.CreateDefaultBuilder(args);

var hostBuilder = sample switch
{
    "layout" => builder.UseOpenTuiRazor<LayoutSample>(),
    "styled" => builder.UseOpenTuiRazor<StyledSample>(),
    "editor" => builder.UseOpenTuiRazor<EditorSample>(),
    "scroll" => builder.UseOpenTuiRazor<ScrollSample>(),
    "input" => builder.UseOpenTuiRazor<InputSample>(),
    "keypress" => builder.UseOpenTuiRazor<KeypressSample>(),
    "ascii" => builder.UseOpenTuiRazor<AsciiSample>(),
    "framebuffer" => builder.UseOpenTuiRazor<FrameBufferSample>(),
    "code" => builder.UseOpenTuiRazor<CodeSample>(),
    "markdown" => builder.UseOpenTuiRazor<MarkdownSample>(),
    "diff" => builder.UseOpenTuiRazor<DiffSample>(),
    "select" => builder.UseOpenTuiRazor<SelectSample>(),
    "slider" => builder.UseOpenTuiRazor<SliderSample>(),
    "tabs" => builder.UseOpenTuiRazor<TabsSample>(),
    "console" => builder.UseOpenTuiRazor<ConsoleSample>(),
    _ => throw new InvalidOperationException($"Unknown sample: '{sample}'. Available samples: layout, styled, editor, scroll, input, keypress, ascii, framebuffer, code, markdown, diff, select, slider, tabs, console")
};

var host = hostBuilder.Build();
await host.RunAsync();
