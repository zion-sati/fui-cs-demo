using FuiCs;
using FuiCsDemo.Shared;

namespace FuiCsDemo.TextFonts;

/// <summary>
/// The route's page: a sign-in form, the text area section, rich text spans recolored with the theme, and text in
/// DejaVu loaded by the app with a color emoji fallback.
/// </summary>
internal sealed class TextFontsPage : ManagedApplicationController
{
    private readonly DemoTextInput usernameInput = new DemoTextInput()
        .Placeholder("Username or email")
        .HostAutofill("username")
        .NodeId("public-demo-username");
    private readonly DemoTextInput passwordInput = new DemoTextInput()
        .Placeholder("Password")
        .Password()
        .HostAutofill("current-password")
        .NodeId("public-demo-current-password");
    private readonly DemoButton signInButton = new("Sign in", DemoButtonTone.Primary);
    private readonly DemoButton clearButton = new("Clear");
    private readonly DemoText signInStatusText = new(
        "Password managers should recognize this projected login form. No credentials are sent anywhere.",
        DemoTextStyle.BodySecondary);
    private readonly Form credentialsForm;
    private readonly TextAreaSection textArea = new();
    private readonly FontFace emojiFace = FontFace.Load(DemoRoutes.FontUrl("NotoColorEmoji.ttf"));
    private readonly FontFamily customFamily;
    private readonly FontStack bodyStack;
    private readonly RichText containerText;
    private readonly RichText helperText;
    private readonly SelectionArea root;

    public TextFontsPage()
    {
        Theme.UseSystem();
        bodyStack = FontStack.Load(DemoRoutes.FontUrl("DejaVuSans.ttf")).Fallback(emojiFace);
        var headingStack = FontStack.Load(DemoRoutes.FontUrl("DejaVuSans-Bold.ttf")).Fallback(emojiFace);
        customFamily = FontFamily.RegularBold(bodyStack, headingStack);
        containerText = new RichText().FontFamily(customFamily).FontWeight(FontWeight.Bold).FontSize(20.0f).LineHeight(28.0f).MaxLines(1).FillWidth();
        helperText = new RichText().FontFamily(customFamily).FontSize(18.0f).LineHeight(26.0f).MaxLines(1).FillWidth();
        usernameInput.SemanticLabel = "Username or email";
        passwordInput.SemanticLabel = "Password";
        signInButton.Click += _ => signInStatusText.Text = "Sign-in submitted locally. No credentials were transmitted.";
        clearButton.Click += _ =>
        {
            usernameInput.Text = "";
            passwordInput.Text = "";
            signInStatusText.Text = "Login fields cleared locally.";
        };
        credentialsForm = new Form { DefaultButton = signInButton, CancelButton = clearButton }
            .Child(new Grid()
                .Columns(GridLength.Pixels(180.0f), GridLength.Star(1.0f))
                .Rows(GridLength.Pixels(40.0f), GridLength.Pixels(12.0f), GridLength.Pixels(40.0f))
                .PlaceChild(FieldLabel("Username or email:   "), 0, 0, 1, 1)
                .PlaceChild(usernameInput, 0, 1, 1, 1)
                .PlaceChild(FieldLabel("Password:   "), 2, 0, 1, 1)
                .PlaceChild(passwordInput, 2, 1, 1, 1)
                .Width(500.0f)
                .MaxWidth(100.0f, Unit.Percent))
            .Child(Spacing.Vertical(14.0f))
            .Child(FlexBox.Row(signInButton, Spacing.Horizontal(12.0f), clearButton))
            .Child(Spacing.Vertical(10.0f))
            .Child(signInStatusText)
            .FillWidth();

        var page = new PageShell("Text & Fonts - FUI-CS Demo 🫶", DemoRoutes.TextFonts, CreateMainPanel());
        root = new SelectionArea().FillSize().Child(page);
        Application.OnUnmount(Theme.Bind(theme =>
        {
            root.Background = theme.Colors.Background;
            SyncRichText(theme);
        }).Dispose);
        credentialsForm.Activate();
        Application.OnUnmount(credentialsForm.Deactivate);
    }

    public override Node GetRoot() => root;

    private static DemoText FieldLabel(string text) => new DemoText(text, DemoTextStyle.Body)
        .VerticalAlign(TextVerticalAlign.Center)
        .TextAlign(TextAlign.Right);

    private Panel CreateMainPanel()
    {
        var textInputs = new Panel()
            .Child(new DemoText("Text Inputs", DemoTextStyle.Heading3))
            .Child(Spacing.Vertical(Spacing.TitleToSupportingGap))
            .Child(new DemoText(
                "This explicit Form projects username and password fields together so Proton Pass, browser autofill, and other password managers can recognize them. Enter submits; Escape clears.",
                DemoTextStyle.BodySecondary))
            .Child(Spacing.Vertical(Spacing.HeadingToBodyGap))
            .Child(credentialsForm)
            .Child(Spacing.Vertical(Spacing.PageSectionGap));
        foreach (var node in textArea.Build())
        {
            textInputs.Child(node);
        }
        textInputs.Child(Spacing.Vertical(Spacing.PageSectionGap));

        var richText = new Panel()
            .Child(new DemoText("Static Rich Text", DemoTextStyle.Heading3))
            .Child(Spacing.Vertical(Spacing.TitleToSupportingGap))
            .Child(containerText)
            .Child(Spacing.Vertical(10.0f))
            .Child(helperText)
            .Child(Spacing.Vertical(10.0f))
            .Child(new DemoText(
                "Use helper spans to compose inline styling, and use RichText container defaults when you want the same font family, weight, size, or color across the whole object.",
                DemoTextStyle.BodySecondary))
            .Child(Spacing.Vertical(Spacing.PageSectionGap))
            .Width(800.0f)
            .MaxWidth(100.0f, Unit.Percent);

        var customFonts = new Panel()
            .Child(new DemoText("App-authored Custom Fonts", DemoTextStyle.Heading3))
            .Child(Spacing.Vertical(Spacing.TitleToSupportingGap))
            .Child(new DemoText("Custom DejaVu FontStack sample 🌍", DemoTextStyle.Heading2).FontFamily(customFamily))
            .Child(Spacing.Vertical(10.0f))
            .Child(new DemoText(
                    "Load DejaVu Sans through FontStack.Load(...), use DejaVu Bold for heavier text, and keep color emoji fallback without dropping to bridge-specific APIs.",
                    DemoTextStyle.BodySecondary)
                .FontFamily(customFamily)
                .FontSize(16.0f))
            .Child(Spacing.Vertical(10.0f))
            .Child(new DemoText("Apply a stack directly: Text.FontStack(customBodyStack, 17) ✨", DemoTextStyle.BodySecondary).FontStack(bodyStack, 17.0f))
            .Child(Spacing.Vertical(8.0f))
            .Child(new DemoText("Bold family resolution stays intact: DejaVu Bold + emoji fallback 😄", DemoTextStyle.Body)
                .FontFamily(customFamily)
                .FontWeight(FontWeight.Bold)
                .FontSize(18.0f))
            .Width(800.0f)
            .MaxWidth(100.0f, Unit.Percent);

        return new Panel()
            .Child(new DemoText("Text & Fonts", DemoTextStyle.Heading2))
            .Child(Spacing.Vertical(Spacing.TitleToSupportingGap))
            .Child(new DemoText("Rich text, custom fonts, and text-input/textarea configuration samples.", DemoTextStyle.BodySecondary))
            .Child(Spacing.Vertical(Spacing.HeadingToBodyGap))
            .Child(new DemoText("Explore Typography APIs, RichText inline styling, and live TextArea control bindings.", DemoTextStyle.BodySecondary))
            .Child(Spacing.Vertical(Spacing.PageSectionGap))
            .Child(new DemoScrollBox("AdvancedControlsScrollBox")
                .FitWidthWhenCompact()
                .FillSize()
                .Child(FlexBox.Column(textInputs, richText, customFonts).FillWidth()))
            .FillSize();
    }

    private void SyncRichText(Theme theme)
    {
        var primary = theme.Colors.TextPrimary;
        containerText.SetSpans(
            new Span("Base family ").Underline().Color(primary),
            new Span("with ").Color(primary),
            new Span("CUSTOM OVERRIDE").FontFamily(customFamily).FontWeight(FontWeight.Bold).Strikethrough());
        var emojiBackground = Color.FromRgb(30, 41, 59);
        var emojiColor = Color.FromRgb(167, 243, 208);
        helperText.SetSpans(
            new Span("Rich ").Bold().Color(primary),
            new Span("text ").Italic().Color(Color.FromRgb(96, 165, 250)),
            new Span("underline ").Underline().Color(Color.FromRgb(251, 191, 36)),
            new Span("strike ").Strikethrough().Color(Color.FromRgb(248, 113, 113)),
            new Span("emoji- ").Background(emojiBackground).Color(emojiColor),
            new Span("😄").FontFamily(FontFamily.WithRegular(emojiFace)).Background(emojiBackground).Color(emojiColor),
            new Span(" ").Background(emojiBackground).Color(emojiColor),
            new Span("helpers").Bold().Italic().Underline().Strikethrough().Color(Color.FromRgb(203, 213, 225)));
    }
}
