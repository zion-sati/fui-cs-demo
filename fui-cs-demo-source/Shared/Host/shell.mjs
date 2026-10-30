// The demo shell the routes share: a clock that ticks once a second, the accent color, and dark mode, which follows
// the browser's color scheme. Every route bundles its own copy of this module, so the clock lives on globalThis and
// keeps counting across route swaps.
const shell = (globalThis.fuiCsDemoShell ??= {
  tick: 0,
  tickListeners: new Set(),
  darkModeQuery: matchMedia('(prefers-color-scheme: dark)'),
});
shell.clock ??= setInterval(() => {
  shell.tick += 1;
  for (const listener of shell.tickListeners) listener(shell.tick);
}, 1000);

export function accentColorHex() {
  return '#2563eb';
}

export function isDarkMode() {
  return shell.darkModeQuery.matches;
}

export function onClockTickChanged(callback) {
  shell.tickListeners.add(callback);
  callback(shell.tick);
  return { dispose() { shell.tickListeners.delete(callback); } };
}

export function onDarkModeChanged(callback) {
  const listener = (event) => callback(event.matches);
  shell.darkModeQuery.addEventListener('change', listener);
  callback(shell.darkModeQuery.matches);
  return { dispose() { shell.darkModeQuery.removeEventListener('change', listener); } };
}
