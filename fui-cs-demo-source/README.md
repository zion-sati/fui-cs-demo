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

- `FuiCsDemo.csproj` and `Dashboard/`: the dashboard, the home route, at `/`.
  Publishing this project publishes every route listed in its `FuiCsRoute`
  items, each into the folder its path names.
- `Routes/AdvancedControls/`: text areas, transitions, drag-and-drop reorder,
  fetch, custom fonts and a worker sample, at `/advanced-controls/`. Its
  worker, `Routes/AdvancedControlsWorker/`, runs a prime search in a Web Worker.
- `Routes/TemplatedControls/`: design-system templates and per-control
  overrides, at `/templated-controls/`.
- `Routes/ImmediateDrawing/`: charts, gauges and a paint canvas drawn each
  frame, at `/immediate-drawing/`.
- `Shared/`: the demo's design system and route page, which every route builds
  on, and the demo shell: `Host/shell.mjs`, the JavaScript module the routes
  call through `[JSImport]` (`Host/DemoShell.cs`) for the shell clock, the
  accent color and dark mode.
- `assets/`: the dashboard's textures, and DejaVu Sans with its license, which
  the advanced controls route loads as an app-authored font.

## Learn more

- [FuiCs](https://github.com/zion-sati/fui-cs): the SDK, its guides and the
  templates.
- [Host services and events](https://github.com/zion-sati/fui-cs/blob/main/docs/v2/fui-cs/HOST_SERVICES_AND_WORKERS.md):
  how the routes reach the page's JavaScript and run workers.
- [EffinDOM](https://github.com/zion-sati/EffinDOM): the runtime.
