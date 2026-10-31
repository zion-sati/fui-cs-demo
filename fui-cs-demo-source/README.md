# FUI-CS Demo

The source of the [FUI-CS demo](https://fui-cs-demo.effindom.dev/): a routed
FuiCs application, made from the `fui-routed-app` template and built only from
the FuiCs packages on nuget.org. Each route is a C# application of its own,
compiled to WebAssembly with NetWasm; one shell loads the route the address
names and swaps routes in place as links are followed.

## Build it

Install the .NET SDK that `global.json` names, then publish the site and serve
`published/` at the root of any static file server:

```bash
dotnet publish -c Release -o published
```

Nothing else is needed: no Node, no npm. NuGet restores FuiCs, the EffinDOM
runtime it carries, and the NetWasm SDK.

## What's here

The same four routes as the FUI-AS and FUI-RS demos, in C#:

- `FuiCsDemo.csproj` and `Home/`: the home route, at `/`: a 100,000-item virtual
  list beside common controls, images, a nested scroll box and control
  foundations. Publishing this project publishes every route listed in its
  `FuiCsRoute` items, each into the folder its path names.
- `Routes/TextFonts/`: a sign-in form, a text area reconfigured live, rich text
  and app-authored DejaVu fonts, at `/text-fonts/`.
- `Routes/Advanced/`: transitions, smooth scrolling, a prime search in a Web
  Worker (`Routes/AdvancedWorker/`) driving a progress bar, real fetch requests
  and drag-and-drop reordering, at `/advanced/`.
- `Routes/ImmediateDrawing/`: charts, gauges and a paint canvas drawn each
  frame, at `/immediate-drawing/`.
- `Shared/`: the demo's design system, which every route builds on, including
  its compact layout for phones and narrow windows, and the demo shell:
  `Host/shell.mjs`, the JavaScript module the routes call through `[JSImport]`
  (`Host/DemoShell.cs`) for the clock.
- `assets/fonts/`: DejaVu Sans with its license, which the text and fonts route
  loads as an app-authored font.

## Learn more

- [FuiCs](https://github.com/zion-sati/fui-cs): the SDK, its guides and the
  templates.
- [Host services and events](https://github.com/zion-sati/fui-cs/blob/main/docs/v2/fui-cs/HOST_SERVICES_AND_WORKERS.md):
  how the routes reach the page's JavaScript and run workers.
- [EffinDOM](https://github.com/zion-sati/EffinDOM): the runtime.
