# Product Requirements Document (PRD)

- [x] Summarize the product purpose, scope, and intended usage inferred from the codebase.
- [x] Enumerate the major features, modules, and user workflows exposed by the library and samples.
- [x] Capture the main data types, public APIs, and UI surfaces a junior developer would need to implement against.
- [x] Record explicit behavioral rules, edge cases, and quality constraints visible in source and tests.
- [x] Flag missing context, inferred assumptions, and areas that require reviewer confirmation.

## 1. **Project Overview**

OpenTUI is a .NET 10 terminal UI library centered on a cell-based rendering engine. It lets developers build terminal applications either imperatively in C# or declaratively through Razor components. The codebase includes a low-level frame buffer, ANSI rendering, flex-style layout, interactive renderable widgets, a rope-backed editing model, input parsing for keyboard and mouse events, and sample applications that demonstrate expected usage.

The product context inferred from the source is:
- A reusable core package (`OpenTui.Core`) for terminal rendering and interactive widgets.
- A Razor integration package (`OpenTui.Razor`) for component-based TUI composition.
- A test suite that defines required runtime behavior and edge-case handling.
- Sample apps that act as reference user flows and visual acceptance examples.

Note: The repository does not include product-management artifacts, roadmap, personas, or business goals. This overview is derived from implementation, tests, and samples only.

## 2. **Feature List**

- **ANSI color and terminal control**: Provides packed color types, text attributes, and escape-sequence helpers for terminal output.
- **Cell buffer drawing engine**: Maintains a 2D grid of cells for text, borders, fills, and framebuffer blitting.
- **Diff-based renderer**: Emits only changed terminal cells to reduce redraw cost.
- **CLI application host**: Runs render and input loops, handles terminal setup/teardown, resize events, focus, and raw input.
- **Flex layout system**: Calculates row/column layout using flex-style sizing, alignment, padding, margin, and absolute offsets.
- **Renderable component tree**: Supplies a base renderable model with parenting, z-ordering, visibility, focus, and event emission.
- **Text storage and styling**: Stores styled chunks for read-only text presentation.
- **Rope-backed edit buffer**: Supports cursor movement, insertion, deletion, line operations, undo/redo, and change events.
- **Keyboard and mouse input parsing**: Normalizes escape sequences and platform input into `KeyEvent` and `MouseEvent` objects.
- **Interactive input widget**: Single-line editable field with placeholder, cursor, max-length, and change/input events.
- **Interactive textarea widget**: Multi-line text editor backed by `EditBuffer`, including cursor and editing shortcuts.
- **Selection list widget**: Supports option navigation, selection change events, confirmation, and optional descriptions/scroll indicators.
- **Slider widget**: Supports horizontal or vertical sliders with keyboard and mouse control.
- **Tab selector widget**: Supports left/right tab switching and change events.
- **Scrollable container and scrollbar**: Supports wheel/drag/keyboard scrolling and optional sticky behavior.
- **Display widgets**: Includes box, text, line numbers, markdown, code, diff, framebuffer, ASCII font, and console overlay renderables.
- **Plugin registry**: Supports named plugin registration and lifecycle hooks.
- **Razor hosting and components**: Exposes host-builder/service-registration APIs and Razor wrappers for renderables.

## 3. **Functional Requirements**

### 3.1 Core rendering and buffer behavior

- The system must represent each terminal position as a cell containing codepoint, foreground color, background color, and text attributes.
- The buffer must support drawing ASCII, Unicode, CJK, and emoji content, including wide characters occupying two columns.
- Wide-character continuation cells must not be re-emitted as visible characters when serializing a buffer.
- Buffer creation must reject non-positive dimensions.
- Out-of-bounds reads must return `null`; out-of-bounds writes must not crash.
- Clearing a buffer must fill all cells with the requested background color.
- Resizing a buffer must preserve overlapping existing content and update width/height.
- Drawing a child buffer into a parent buffer must support clipped blitting and optional alpha-aware background blending.
- Drawing boxes must support single, double, rounded, and heavy border styles; optional fill; optional top and bottom titles; and left/center/right title alignment.
- Disposed buffers must reject further use with `ObjectDisposedException`.

### 3.2 Terminal rendering and host loop

- The renderer must maintain current and next frame buffers and only write changed cells.
- The renderer must support alternate-screen mode, cursor visibility/positioning, clear/home, and reset on shutdown.
- The CLI host must initialize terminal state for interactive mode, enable mouse tracking, and restore the terminal on destroy/dispose.
- The CLI host must expose the root render tree, current terminal size, current focus, and a global key-input emitter.
- The CLI host must request rerenders on demand and on terminal resize.
- The CLI host must route keyboard events to the focused renderable unless default handling is prevented.
- Ctrl+C must destroy the host when `ExitOnCtrlC` is enabled.
- Mouse wheel events must be routed to the renderable under the cursor, falling back to the focused renderable if needed.
- Left-click must focus the nearest focusable target in the clicked parent chain.
- Clicking empty space must blur the current focus.
- The host must support a test mode that avoids terminal side effects.

### 3.3 Layout and renderable tree

- Every renderable must expose flex-related sizing and positioning through its layout node.
- Renderables must support parent/child composition, visibility toggling, z-index ordering, and focus/blur events.
- Children must render depth-first in ascending z-index order.
- Screen coordinates must be computed from parent screen offsets plus local computed coordinates.
- Layout calculation must handle nested padding, percent-based sizing, auto sizing, and computed output dimensions.
- Absolute positioning inputs (`Top`, `Left`, `Right`, `Bottom`, `Position`) must be honored where set.

### 3.4 Text and editing behavior

- The edit buffer must store text in a rope-backed structure and expose line/offset/cursor conversion helpers.
- The edit buffer must support setting full text, getting full text, getting line count, and slicing by offsets or row/column pairs.
- Cursor movement must clamp at document boundaries and preserve a valid row/column/offset state.
- Insert, delete, newline, clear, delete-range, and delete-line operations must mutate the buffer and raise `TextChanged`.
- Undo and redo must preserve both text state and cursor state.
- Undo/redo availability must be queryable.
- Clearing history must remove undo/redo availability.
- Disposed edit buffers must reject further use.
- Styled text storage must preserve chunk-level foreground, background, attributes, and optional link metadata.

### 3.5 Input normalization

- Raw terminal input must be normalized into structured key and mouse events.
- Key events must preserve modifier state and normalized names such as printable characters, arrow keys, and Ctrl combinations.
- Mouse events must preserve coordinates, button identity, pressed state, and modifiers.
- The parser must handle CSI/SS3 escapes, UTF-8 input, Alt combinations, and SGR mouse sequences.

### 3.6 Widget requirements

#### Box
- Must render an optional border and filled background.
- Must support focus-aware border color changes when configured.
- Must support titles on top and bottom borders.

#### Text
- Must render string content with optional foreground, background, alignment, wrapping, and text attributes.

#### Input
- Must be focusable.
- Must support placeholder rendering when empty.
- Must keep the cursor visible by horizontally scrolling the visible substring.
- Must support left/right/home/end/backspace/delete/return and printable character insertion.
- Must enforce `MaxLength` both on direct assignment and typed input.
- Must emit `input` on live edits, `change` on blur and Enter, and `enter` on Enter.

#### Textarea
- Must provide multi-line editing behavior backed by `EditBuffer`.
- Must support cursor display, scrolling, placeholder-like empty behavior, and standard editing shortcuts.
- Must emit input-style change notifications when content changes.

#### Select
- Must be focusable.
- Must support up/down navigation bounded to the available options.
- Must keep the selected option visible when scrolling.
- Must emit `selectionChanged` when navigation changes selection.
- Must emit `itemSelected` with the selected option on Enter.
- Must support optional description display and scroll indicators.

#### Slider
- Must be focusable.
- Must support horizontal and vertical orientation.
- Must clamp value to `[Min, Max]`.
- Must honor `Step` snapping for mouse-driven updates.
- Must emit `valueChanged` only when the effective value changes.
- Must respond to arrow keys and left-click track interaction.

#### TabSelect
- Must support left/right switching across tabs.
- Must track and expose the active tab index.
- Must emit a tab-changed event when selection changes.

#### ScrollBox and ScrollBar
- Must support scroll offsets, mouse wheel scrolling, keyboard scrolling, and scrollbar dragging.
- Must optionally display vertical and horizontal scrollbars.
- Must track content size relative to viewport size.

#### Code
- Must render a syntax-highlighted code viewer.
- Must support optional line numbers.
- Must tokenize at least comments, strings, numbers, identifiers, and known keywords.
- Must clip output to the available width and height.
- Note: Highlighting is heuristic and appears optimized for C# plus some JS/TS keywords, not full language grammars.

#### Markdown
- Must render a defined markdown subset: H1-H3 headings, blockquotes, horizontal rules, bullet lists, numbered lists, fenced code blocks, bold, italic, inline code, and links.
- Must clip text to visible width.
- Note: The code does not show full Markdown compliance beyond this subset.

#### Diff
- Must render a side-by-side or line-oriented diff view based on old/new text.
- Must support optional line numbers.
- Note: Exact visual conventions are inferred from the dedicated renderable and sample, but not fully specified in tests.

#### FrameBuffer
- Must provide a drawable pixel-like region supporting set-pixel, lines, fills, and circles.

#### ASCII Font
- Must render large-text output using named font presets.
- Must support selection highlighting and mapping display columns back to text indices.

#### LineNumbers
- Must render a gutter given total line count and starting line.

#### ConsoleOverlay
- Must render a bounded overlay log panel in one of four screen corners.
- Must support info/warn/error style line insertion and clearing.

### 3.7 Plugins and events

- The event emitter must support persistent listeners, one-time listeners, explicit removal, and remove-all behavior.
- The plugin registry must support named registration, unregistration, existence checks, lookup, and enumeration.
- Plugins must provide a name plus register/unregister lifecycle methods.

### 3.8 Razor hosting

- The Razor package must allow host configuration via dependency injection and generic-host builder extensions.
- A hosted service must mount the root component and start the TUI runtime.
- Razor renderable wrappers must mirror the imperative component surface closely enough for nested UI composition.
- Host shutdown in testing mode must complete promptly.
- Service-registration detection must work without forcing construction of registered services.

## 4. **Data Structures**

Note: The repository is implemented in C#, not TypeScript. This section maps the requested “interfaces/types” inventory to the main C# classes, structs, records, and enums exposed by the source.

### 4.1 Foundational types

- **`Rgba`**: Packed color value with byte/float accessors, alpha, color intent (`Rgb`, `Indexed`, `Default`), and optional ANSI slot metadata.
- **`Cell`**: Terminal cell with `Codepoint`, `Fg`, `Bg`, and `Attributes`.
- **`TextAttributes`**: Bit flags for bold, dim, italic, underline, blink, inverse, hidden, and strikethrough.
- **`ColorIntent`**: Distinguishes raw RGB colors from indexed and terminal-default colors.

### 4.2 Layout model

- **`FlexNode`**
  - Layout inputs: `FlexDirection`, `AlignItems`, `JustifyContent`, `FlexGrow`, `FlexShrink`, `FlexBasis`
  - Size constraints: `Width`, `Height`, `MinWidth`, `MaxWidth`, `MinHeight`, `MaxHeight`
  - Spacing: `PaddingTop/Right/Bottom/Left`, `MarginTop/Right/Bottom/Left`
  - Positioning: `Position`, `Top`, `Left`, `Right`, `Bottom`
  - Computed outputs: `ComputedX`, `ComputedY`, `ComputedWidth`, `ComputedHeight`
  - Relationship: parent/child layout tree used by `Renderable`
- **`LayoutDimension`**
  - Represents `Fixed`, `Percent`, or `Auto` sizing
  - Parsed from numeric values, `"50%"`, or `"auto"`
  - Relationship: stored inside `FlexNode` for width/height-like fields

### 4.3 Text model

- **`Rope`**: Immutable-style rope data structure returning new rope instances on insert/delete/replace operations.
- **`LogicalCursor`**: Tracks `Row`, `Col`, and `Offset` together.
- **`EditBuffer`**
  - Holds the active `Rope`
  - Maintains current `LogicalCursor`
  - Maintains undo and redo stacks of `(Rope, LogicalCursor)` snapshots
  - Emits `TextChanged`
- **`StyledChunk`**
  - `Text`
  - Optional `Fg` and `Bg`
  - `Attributes`
  - Optional `Link`
- **`StyledText`**
  - Relationship: ordered collection of `StyledChunk` items for display-oriented text content

### 4.4 Input model

- **`KeyEvent`**
  - `Name`, `Key`
  - Modifier flags: `Ctrl`, `Alt`, `Shift`, `Meta`
  - Optional `Char`
  - `DefaultPrevented`
- **`MouseEvent`**
  - `X`, `Y`
  - `Button`
  - `Pressed`
  - Modifier flags
- **`MouseButton`**
  - `None`, `Left`, `Middle`, `Right`, `WheelUp`, `WheelDown`

### 4.5 UI model

- **`Renderable`**
  - Identity/state: `Id`, `Visible`, `Opacity`, `ZIndex`, `Focusable`, `Focused`
  - Layout relationship: owns `LayoutNode`
  - Tree relationship: `Parent`, child collection via `Add`, `Remove`, `GetChildren`
  - Runtime positioning: `X`, `Y`, `ComputedWidth`, `ComputedHeight`, `ScreenX`, `ScreenY`
- **Widget-specific models**
  - `InputOptions`: placeholder, cursor colors, fg/bg, max length, width/height, initial value
  - `SelectOption`: `Name`, optional `Description`, optional `Value`
  - `BoxOptions`, `TextOptions`, `TextareaOptions`, `CliRendererConfig`, `OpenTuiRazorOptions`, `StyleDefinition`

### 4.6 Razor/hosting model

- **`OpenTuiAppContext`**: Wraps renderer/runtime state for Razor-hosted apps.
- **`RenderableComponentBase<T>`**: Base component that manages renderable creation, parameter updates, and tree registration.
- **`ContainerRenderableComponentBase<T>`**: Adds nested `ChildContent` support.

## 5. **API Interfaces**

Note: There are no HTTP endpoints or external service APIs in the repository. The public interfaces are C# library APIs.

### 5.1 Low-level drawing API

```csharp
CellBuffer Create(int width, int height, string? id = null, bool respectAlpha = false)
void Clear(Rgba? bg = null)
Cell? GetCell(int x, int y)
void SetCell(int x, int y, int codepoint, Rgba fg, Rgba bg, TextAttributes attrs = TextAttributes.None)
void DrawText(string text, int x, int y, Rgba fg, Rgba? bg = null, TextAttributes attrs = TextAttributes.None)
void DrawBox(int x, int y, int width, int height, Rgba borderColor, Rgba bg, ...)
void FillRect(int x, int y, int w, int h, Rgba bg)
void DrawFrameBuffer(int destX, int destY, CellBuffer src, int srcX = 0, int srcY = 0, int? srcWidth = null, int? srcHeight = null)
void Resize(int width, int height)
List<(int Width, int Codepoint)> EncodeUnicode(string text)
byte[] GetRealCharBytes(bool addLineBreaks = false)
static int RuneWidth(Rune rune)
```

**Usage example**
```csharp
using var buf = CellBuffer.Create(60, 20);
buf.Clear(Rgba.FromInts(0, 0, 0));
buf.DrawBox(0, 0, 60, 20, Rgba.FromInts(0, 200, 255), Rgba.FromInts(0, 0, 0), fill: true, title: " Demo ");
buf.DrawText("Hello from OpenTUI", 2, 2, Rgba.FromInts(255, 255, 255));
```

### 5.2 Color and ANSI API

```csharp
Rgba FromValues(float r, float g, float b, float a = 1f)
Rgba FromInts(int r, int g, int b, int a = 255)
Rgba FromIndex(int index, Rgba? snapshot = null)
Rgba FromHex(string hex)
Rgba FromCss(string nameOrHex)
Rgba DefaultForeground(Rgba? snapshot = null)
Rgba DefaultBackground(Rgba? snapshot = null)
Rgba BlendOver(Rgba dst)
```

### 5.3 Renderer and application host API

```csharp
Renderer(int width, int height, TextWriter? output = null, bool testing = false)
void Initialize()
void Shutdown()
CellBuffer GetFrameBuffer()
void SetCursorPosition(int x, int y, bool visible = true)
void Resize(int width, int height)
void Render()

CliRenderer(CliRendererConfig? config = null)
void Start()
void Stop()
void Destroy()
void RequestRender()
void SetBackgroundColor(string color)
Renderable? CurrentFocus { get; }
RootRenderable Root { get; }
KeyHandler KeyInput { get; }
```

**Usage example**
```csharp
var renderer = new CliRenderer(new CliRendererConfig { TargetFps = 30, ExitOnCtrlC = true });
renderer.Root.Add(new BoxRenderable(renderer));
renderer.Start();
```

### 5.4 Layout and render tree API

```csharp
static void FlexLayout.Calculate(FlexNode root, int containerWidth, int containerHeight)

void Add(Renderable child)
void Remove(string id)
List<Renderable> GetChildren()
void Focus()
void Blur()
void RequestRender()
virtual void HandleKey(KeyEvent key)
virtual void HandleMouse(MouseEvent mouse)
```

### 5.5 Text and editor API

```csharp
EditBuffer Create()
void SetText(string text)
string GetText()
int GetLineCount()
string GetTextRange(int startOffset, int endOffset)
string GetTextRangeByCoords(int startRow, int startCol, int endRow, int endCol)
LogicalCursor GetCursorPosition()
void SetCursor(int row, int col)
void SetCursorByOffset(int offset)
void MoveCursorLeft()
void MoveCursorRight()
void MoveCursorUp()
void MoveCursorDown()
void GotoLine(int line)
LogicalCursor GetEol()
LogicalCursor GetNextWordBoundary()
LogicalCursor GetPrevWordBoundary()
void InsertChar(string ch)
void InsertText(string text)
void DeleteChar()
void DeleteCharBackward()
void DeleteRange(int startLine, int startCol, int endLine, int endCol)
void DeleteLine()
void NewLine()
void Clear()
bool CanUndo()
string? Undo()
bool CanRedo()
string? Redo()
void ClearHistory()
event EventHandler<string>? TextChanged
```

**Usage example**
```csharp
using var editor = EditBuffer.Create();
editor.SetText("Hello");
editor.SetCursor(0, 5);
editor.InsertChar("!");
editor.Undo();
```

### 5.6 Event and plugin API

```csharp
void On(string eventName, Action<object?> handler)
void Off(string eventName, Action<object?> handler)
void Once(string eventName, Action<object?> handler)
void Emit(string eventName, object? data = null)
void RemoveAllListeners(string? eventName = null)

interface IPlugin
{
    string Name { get; }
    void Register(PluginRegistry registry);
    void Unregister(PluginRegistry registry);
}

void Register(IPlugin plugin)
void Unregister(string name)
IPlugin? Get(string name)
bool Has(string name)
IReadOnlyDictionary<string, IPlugin> All { get; }
```

### 5.7 Razor hosting API

```csharp
IServiceCollection AddOpenTuiRazor(this IServiceCollection services)
IHostBuilder UseOpenTuiRazor<TComponent>(this IHostBuilder builder) where TComponent : IComponent
```

**Usage example**
```csharp
var host = Host.CreateDefaultBuilder(args)
    .UseOpenTuiRazor<MyRootComponent>()
    .Build();

await host.RunAsync();
```

### 5.8 Widget event interface summary

- `InputRenderable`: emits `input`, `change`, `enter`
- `TextareaRenderable`: emits input-related change events
- `SelectRenderable`: emits `selectionChanged`, `itemSelected`
- `SliderRenderable`: emits `valueChanged`
- `TabSelectRenderable`: emits tab-change events
- `Renderable`: emits `focused`, `blurred`, `destroyed`

Note: Some widget property sets are large and are primarily exposed as public properties rather than dedicated methods. Implementation should preserve those direct-set configuration surfaces.

## 6. **User Interface (UI) Outline**

OpenTUI is a library rather than a single end-user application, so the codebase exposes reusable UI primitives instead of fixed screens. The samples define the expected screen patterns.

### 6.1 Base screen structure

- A full-screen root renderable sized to the terminal.
- Nested boxes and containers arranged using flex layout.
- Draw order controlled by z-index.
- Focus ring and cursor state used to indicate active interactive elements.

### 6.2 Common visual patterns

- **Boxed layout**
  - Rounded/single/double/heavy border
  - Optional title on the top border
  - Optional bottom title/footer
  - Filled or transparent interior
- **Text panels**
  - Plain text
  - Styled/attributed text
  - Line-number gutters
- **Editing surfaces**
  - Single-line input with visible caret
  - Multi-line editor/textarea with scrolling
- **Choice controls**
  - Vertical select list with highlighted current row
  - Horizontal or vertical slider with track and thumb
  - Tab strip with active/inactive styling
- **Scrollable content**
  - Scrollbox viewport with optional scrollbars
  - Wheel scrolling and drag interactions
- **Content viewers**
  - Syntax-highlighted code area
  - Markdown viewer with headings, blockquotes, lists, inline code, and fenced code blocks
  - Diff viewer for old/new text comparison
- **Special renderers**
  - Pixel-like framebuffer canvas
  - Large ASCII font banner text
  - Corner console overlay for logs

### 6.3 Textual mockups

**Box + text**
```text
┌ Demo ──────────────────────────────┐
│ Hello from OpenTUI                 │
│                                    │
└────────────────────────────────────┘
```

**Select list**
```text
▶ First option
  Second option
  Third option                    █
```

**Input**
```text
[ type here... ]
        ^
```

**Slider**
```text
────●────────
```

**Console overlay**
```text
┌ Logs ─────────────┐
│ INFO  Started     │
│ WARN  Slow frame  │
└───────────────────┘
```

### 6.4 Razor UI composition

- Razor components mirror the imperative renderables, enabling markup like nested `<Box>`, `<Text>`, `<Textarea>`, `<Select>`, and related components.
- The Razor samples indicate that UI should be composable through child content and host startup should feel similar to ASP.NET Core component apps.

Note: The repository does not include screenshots or formal design specs, so the UI outline is inferred from renderable behavior and sample names/content.

## 7. **Non-Functional Requirements**

- **Platform/runtime**
  - Target framework is .NET 10.
  - The codebase is expected to run in terminal environments and contains both POSIX and Windows-specific raw-mode handling.
- **Performance**
  - Rendering should minimize terminal output by diffing current and next frames.
  - Layout and rendering must be efficient enough for an interactive target frame rate.
  - Buffer operations should avoid unnecessary full-string or full-frame churn where the implementation already uses ropes and diff buffers.
- **Unicode correctness**
  - The library must preserve UTF-8 output and handle wide characters such as CJK glyphs and emoji.
- **Reliability**
  - Terminal state must be restored on shutdown/dispose.
  - Disposed core objects must fail fast on later use rather than silently corrupting state.
  - Resize handling must update runtime dimensions without crashing interactive apps.
  - Host shutdown must complete promptly in testing mode.
- **Testability**
  - Multiple components include explicit `Testing` flags to suppress terminal side effects.
  - The repository includes an automated xUnit suite that should remain green.
- **Extensibility**
  - Event emitter and plugin registry patterns suggest the library should remain easy to extend without modifying core internals for every new feature.
- **Security**
  - No authentication, authorization, or network surfaces are present in the code inspected.
  - Input handling operates on raw terminal input, so implementations should avoid introducing unsafe parsing, unbounded memory growth, or failure to restore terminal state.
- **Scalability**
  - The current code is designed for single-process terminal applications rather than distributed or multi-user systems.

## 8. **Assumptions and Uncertainties**

- The repository is C#, not TypeScript, so the requested TypeScript data-model section has been adapted to C# types.
- No explicit product personas, acceptance criteria, or business requirements are present; all requirements are inferred from code, tests, and samples.
- Some renderables, especially `Diff`, `FrameBuffer`, `Textarea`, `TabSelect`, and `ASCII Font`, are only partially specified by the inspected tests, so parts of their behavior are inferred from source structure and sample naming.
- The markdown renderer clearly implements a subset of Markdown rather than a complete specification; any full-Markdown requirement would need confirmation.
- The syntax highlighter uses heuristic tokenization and a fixed keyword list, so “multi-language support” should be interpreted narrowly unless expanded later.
- Razor wrappers appear intended to mirror imperative renderables, but the exact parity of every parameter/event should be reviewed in the component wrappers during implementation work.
- Visual styling expectations are inferred from color constants and character choices in code, not from product design documents.
- There are no HTTP APIs, persistence schemas, or external integration contracts in the inspected codebase; if future work requires them, they must be specified separately.
- These uncertainties should be reviewed before using this PRD as the sole specification for major new features.
