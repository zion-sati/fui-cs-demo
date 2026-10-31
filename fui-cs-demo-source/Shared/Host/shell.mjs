// The demo shell the routes share: the browser's clock, and a tick once a second with the time in Unix seconds.
// Every route bundles its own copy of this module, so the ticking timer lives on globalThis.
const shell = (globalThis.fuiCsDemoShell ??= { listeners: new Set() });

export function nowUnixSeconds() {
  return Math.floor(Date.now() / 1000);
}

shell.timer ??= setInterval(() => {
  const now = nowUnixSeconds();
  for (const listener of shell.listeners) listener(now);
}, 1000);

export function onClockTick(callback) {
  shell.listeners.add(callback);
  callback(nowUnixSeconds());
  return { dispose() { shell.listeners.delete(callback); } };
}
