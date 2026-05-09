using OpenTui.Core.Input;
using OpenTui.Core.Rendering;
using OpenTui.Core.Renderables;

namespace OpenTui.Samples;

internal static class NovaTuiShowcaseSample
{
    public static void Run()
    {
        var renderer = new CliRenderer(new CliRendererConfig
        {
            ExitOnCtrlC = true,
            TargetFps = 30,
            BackgroundColor = "#1b1c2a"
        });

        var root = new BoxRenderable(renderer, new BoxOptions
        {
            BackgroundColor = "#1b1c2a"
        });
        root.SetWidth("100%");
        root.SetHeight("100%");
        renderer.Root.Add(root);

        AddText(root, renderer, "╱╱╱╱╱", "#6057ff", 2, 1, 8);
        AddText(root, renderer, "╱╱╱╱╱", "#6057ff", 2, 2, 8);
        AddText(root, renderer, "╱╱╱╱╱", "#6057ff", 2, 3, 8);

        AddText(root, renderer, "NovaTUI™", "#ff5ef2", 10, 1, 14);

        var logo = new ASCIIFontRenderable(renderer)
        {
            Text = "NOVA",
            Font = "block",
            Color = "#c56aff",
            BackgroundColor = "#1b1c2a"
        };
        logo.Position = "absolute";
        logo.Left = 9;
        logo.Top = 2;
        logo.SetWidth(42);
        logo.SetHeight(4);
        root.Add(logo);

        AddText(root, renderer, "v1.0.0", "#6f6bff", 38, 1, 8);
        AddText(root, renderer, new string('╱', 60), "#6057ff", 50, 1, 62);
        AddText(root, renderer, "~", "#9ca3af", 2, 6, 2);

        AddText(root, renderer, "◇ phi4-mini-reasoning via Ollama", "#d1d5db", 2, 8, 40);

        AddText(root, renderer, "LSPs", "#6b7280", 2, 10, 12);
        AddText(root, renderer, "MCPs", "#6b7280", 30, 10, 12);
        AddText(root, renderer, "Skills", "#6b7280", 60, 10, 16);

        AddText(root, renderer, "None", "#6b7280", 2, 12, 12);
        AddText(root, renderer, "None", "#6b7280", 30, 12, 12);
        AddText(root, renderer, "● nova-config", "#d1d5db", 60, 12, 20);
        AddText(root, renderer, "● nova-hooks", "#d1d5db", 60, 13, 20);
        AddText(root, renderer, "● jq", "#d1d5db", 60, 14, 20);

        AddText(root, renderer, "●", "#14d8a6", 60, 12, 2);
        AddText(root, renderer, "●", "#14d8a6", 60, 13, 2);
        AddText(root, renderer, "●", "#14d8a6", 60, 14, 2);

        AddText(root, renderer, ">", "#2ef2d3", 3, 30, 2);
        AddText(root, renderer, "R", "#ff5ef2", 5, 30, 2, "#2a1930");
        AddText(root, renderer, "eady?", "#6b7280", 6, 30, 8);

        AddText(root, renderer, "::: ", "#14d8a6", 2, 31, 4);
        AddText(root, renderer, "::: ", "#14d8a6", 2, 32, 4);

        AddText(
            root,
            renderer,
            "/ or ctrl+p commands • ctrl+l models • ctrl+j newline • ctrl+c quit • ctrl+g more",
            "#6b7280",
            2,
            34,
            110);

        renderer.KeyInput.On("keypress", (KeyEvent key) =>
        {
            if (key.Name is "q" or "escape")
                renderer.Destroy();
        });

        renderer.Start();
    }

    private static void AddText(BoxRenderable root, CliRenderer renderer, string content, string fg, int left, int top, int width, string? bg = null)
    {
        var text = new TextRenderable(renderer, new TextOptions
        {
            Content = content,
            Fg = fg,
            Bg = bg ?? "#1b1c2a",
            Wrap = false,
            Height = 1
        });
        text.Position = "absolute";
        text.Left = left;
        text.Top = top;
        text.SetWidth(width);
        root.Add(text);
    }
}
