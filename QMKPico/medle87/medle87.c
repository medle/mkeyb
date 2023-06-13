//
// Medle87 QMK keyboard firmware
// SL (13.06.2023)
//
#include "medle87.h"
#include "deferred_exec.h"

#define LED_PIN GP15

static void set_led(bool on) {
  writePin(LED_PIN, on);
}

static bool read_led(void) {
  return readPin(LED_PIN);
}

static void toggle_led(void) {
  set_led(!read_led());
}

static void repeat_led_flashes(int n, int millis) {
  for(int i = 0; i < n; i++) {
    set_led(true);
    wait_ms(millis);
    set_led(false); 
    if(i < n - 1) wait_ms(millis);
  }
}

static void play_welcome_flashes(void) {
  repeat_led_flashes(2, 100);
}

//
// Happens at the end of the QMK firmware's startup process. 
// This is where you'd want to put "customization" code, for the most part.
//
void keyboard_post_init_user(void) {
  // to enable QMK debugging add "CONSOLE_ENABLE = yes" to rules.mk
  // see debug output in "qmk console"
  //debug_enable=true;
  //debug_keyboard=true;
  //debug_matrix=true;
  //debug_mouse=true;

  setPinOutput(LED_PIN);
  play_welcome_flashes();
}

#define GENERATE_ALT_SHIFT
#ifdef GENERATE_ALT_SHIFT
//
// Generates Left-Alt-Shift sequence as if user has pressed 
// a change kanguage keyboard shortcut.
//
static bool generate_alt_shift(void) {
  if (get_mods() != 0) return false;
  register_code(KC_LALT);
  register_code(KC_LSFT);
  unregister_code(KC_LALT);
  unregister_code(KC_LSFT);
  return true; 
}

static bool alt_shift_pending = false;
#endif

//
// These function is called by QMK every time a key is pressed or released.
// See %HOME%/qmk_firmware/docs/feature_advanced_keycodes.md
//
bool process_record_user(uint16_t keycode, keyrecord_t *record) {
  switch (keycode) {

    // Custom user keycode 1 toggles LED
    case KC_USR1:
      if (record->event.pressed) {
	toggle_led();
      } 
      return false; // Skip all further processing of this key

    // When right shift is released and right ctrl is held, toggle LED
    case KC_RSFT:
      if (!record->event.pressed) {
        if (get_mods() & MOD_BIT(KC_RCTL)) {
          toggle_led();
#ifdef GENERATE_ALT_SHIFT
          alt_shift_pending = true; // schedule job after mod change is finished
#endif
        }  
      }
      break; 

    // When right ctrl is released and right shift is held, toggle LED
    case KC_RCTL:
      if (!record->event.pressed) {
        if (get_mods() & MOD_BIT(KC_RSFT)) {
          toggle_led();
#ifdef GENERATE_ALT_SHIFT
          alt_shift_pending = true; // schedule job after mod change is finished
#endif
        }
      }
      break;
  }

  // Process key normally
  return true;
}

//
// This function gets called by QMK at the end of all QMK 
// processing, before starting the next iteration.
//
void housekeeping_task_user(void) {
#ifdef GENERATE_ALT_SHIFT
  if (alt_shift_pending) {
    if (generate_alt_shift()) alt_shift_pending = false; 
  }
#endif
}


