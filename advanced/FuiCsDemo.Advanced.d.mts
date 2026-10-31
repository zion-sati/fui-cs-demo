export interface NetWasmPagePlatformHooks {
  now?: () => number;
  random?: (bytes: Uint8Array) => void;
}
export interface NetWasmPageOpenOptions {
  environment?: Record<string, string>;
  modules?: Record<string, object>;
  onNotification?: (value: { operation: string; arguments: unknown[] }) => void | Promise<void>;
  platform?: NetWasmPagePlatformHooks;
  stderr?: (line: string) => void;
  stdout?: (line: string) => void;
}
export interface NetWasmCallOptions {
  signal?: AbortSignal;
}
export interface NetWasmPageModule {
  "__disposeApp"(): void;
  "__flushRenders"(): void;
  "__fui_can_show_context_menu"(handle: bigint): boolean;
  "__fui_capture_persisted_ui_state"(): void;
  "__fui_get_gesture_intent"(handle: bigint): number;
  "__fui_get_long_press_minimum_duration_ms"(handle: bigint): number;
  "__fui_get_long_press_movement_tolerance"(handle: bigint): number;
  "__fui_hide_active_context_menu"(): void;
  "__fui_long_press_continues_pointer_events"(handle: bigint): boolean;
  "__fui_needs_animation_frame"(): boolean;
  "__fui_on_context_menu"(handle: bigint, x: number, y: number): void;
  "__fui_on_cross_selection_changed"(handle: bigint, text: string): void;
  "__fui_on_fetch_complete"(requestId: number, ok: boolean, status: number, payload: Uint8Array): void;
  "__fui_on_fetch_error"(requestId: number, message: string): void;
  "__fui_on_focus_changed"(handle: bigint, focused: boolean): void;
  "__fui_on_font_loaded"(fontId: number): void;
  "__fui_on_frame"(timestampMs: number): void;
  "__fui_on_gesture_event"(handle: bigint, phase: number, kind: number, x: number, y: number, deltaX: number, deltaY: number, scale: number, pointerCount: number): boolean;
  "__fui_on_key_event"(eventType: number, key: string, modifiers: number): number;
  "__fui_on_long_press_event"(handle: bigint, x: number, y: number, pointerId: number, pointerType: number, modifiers: number, durationMs: number): boolean;
  "__fui_on_pointer_event_with_metadata"(eventType: number, handle: bigint, x: number, y: number, modifiers: number, pointerId: number, pointerType: number, button: number, buttons: number, pressure: number, width: number, height: number, clickCount: number, isPrimary: boolean, tangentialPressure: number, tiltX: number, tiltY: number, twist: number): boolean;
  "__fui_on_route_changed"(route: string): void;
  "__fui_on_scroll"(handle: bigint, offsetX: number, offsetY: number, contentWidth: number, contentHeight: number, viewportWidth: number, viewportHeight: number): void;
  "__fui_on_selection_changed"(handle: bigint, start: number, end: number): void;
  "__fui_on_svg_failed"(svgId: number, error: string): void;
  "__fui_on_svg_loaded"(svgId: number, width: number, height: number): void;
  "__fui_on_system_accent_color_changed"(color: number): void;
  "__fui_on_system_dark_mode_changed"(isDark: boolean): void;
  "__fui_on_text_changed"(handle: bigint, text: string): void;
  "__fui_on_text_replaced"(handle: bigint, start: number, end: number, text: string): void;
  "__fui_on_texture_failed"(textureId: number, error: string): void;
  "__fui_on_texture_loaded"(textureId: number, width: number, height: number): void;
  "__fui_on_viewport_changed"(width: number, height: number): void;
  "__fui_on_wheel_event"(handle: bigint, x: number, y: number, deltaX: number, deltaY: number, deltaMode: number, modifiers: number): number;
  "__fui_on_worker_message"(workerId: number, kind: number, text: string): void;
  "__fui_resolve_gesture_owner"(handle: bigint): bigint;
  "__fui_resolve_long_press_owner"(handle: bigint): bigint;
  "__fui_restore_persisted_ui_state"(): void;
  "__runApp"(): void;
  "fui_dispatch_custom_draw"(handle: bigint, canvas: number): void;
  close(): Promise<void>;
  readonly failure: Promise<unknown>;
}
export declare function open(options?: NetWasmPageOpenOptions): Promise<NetWasmPageModule>;
