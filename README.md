# FUI-CS Demo

Live demo of FuiCs, the C# SDK for EffinDOM: retained-mode canvas UI on a
WebAssembly display server, with every route compiled from C# to WebAssembly
by NetWasm. No DOM. No CSS. No virtual DOM.

**[→ View the demo](https://fui-cs-demo.effindom.dev/)**

**[→ Demo source code](./fui-cs-demo-source/)**

This is a static GitHub Pages deployment of the FUI-CS demo app, built with
`dotnet publish` from the FuiCs packages on nuget.org. The built site (WASM,
JS, HTML) is served directly from the repo root.

## What's here

- **Dashboard**: layout, scrolling, a pooled list, input and pointer state,
  media and the shell clock
- **Advanced controls**: text areas, transitions, drag-and-drop reorder,
  fetch, app-authored fonts, and a ProgressBar driven by a Web Worker
- **Templated controls**: design-system templates and per-control overrides
- **Immediate-mode drawing**: charts, gauges, waveforms and a paint canvas
- **Dark & light themes**: following the system color scheme

## Repos

| Repo | Purpose |
|---|---|
| **[fui-cs-demo](https://github.com/zion-sati/fui-cs-demo)** | This repo: live demo site |
| **[fui-cs](https://github.com/zion-sati/fui-cs)** | C# SDK, controls and `dotnet new` templates |
| **[EffinDOM](https://github.com/zion-sati/EffinDOM)** | Runtime, engine, docs |
| **[NetWasm](https://github.com/zion-sati/NetWasm)** | The .NET to WebAssembly compiler |
