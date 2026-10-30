import { openPage } from "./netwasm.page.mjs";

const startup = Object.freeze({
  buildFingerprint: "295f362355ba0fb4437f4647f6d8e5fdf66145a1e2989359a92cffb1cd27d615",
  manifestSha256: "0094279a2820047f0fcaa653ce649954af9d4c28507f346bee694125705a9543",
});
const exports = Object.freeze(["__disposeApp","__flushRenders","__fui_can_show_context_menu","__fui_capture_persisted_ui_state","__fui_get_gesture_intent","__fui_get_long_press_minimum_duration_ms","__fui_get_long_press_movement_tolerance","__fui_hide_active_context_menu","__fui_long_press_continues_pointer_events","__fui_needs_animation_frame","__fui_on_context_menu","__fui_on_cross_selection_changed","__fui_on_fetch_complete","__fui_on_fetch_error","__fui_on_focus_changed","__fui_on_font_loaded","__fui_on_frame","__fui_on_gesture_event","__fui_on_key_event","__fui_on_long_press_event","__fui_on_pointer_event_with_metadata","__fui_on_route_changed","__fui_on_scroll","__fui_on_selection_changed","__fui_on_svg_failed","__fui_on_svg_loaded","__fui_on_system_accent_color_changed","__fui_on_system_dark_mode_changed","__fui_on_text_changed","__fui_on_text_replaced","__fui_on_texture_failed","__fui_on_texture_loaded","__fui_on_viewport_changed","__fui_on_wheel_event","__fui_on_worker_message","__fui_resolve_gesture_owner","__fui_resolve_long_press_owner","__fui_restore_persisted_ui_state","__runApp","fui_dispatch_custom_draw"]);
export function open(options) {
  return openPage(Object.freeze({ exports, options, startup }));
}
