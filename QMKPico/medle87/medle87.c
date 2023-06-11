
#include "medle87.h"

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

static bool rsft_down = false;
static bool rctl_down = false;

bool process_record_user(uint16_t keycode, keyrecord_t *record) {
  switch (keycode) {

    // Custom user keycode 1
    case KC_USR1:
      if (record->event.pressed) {
	toggle_led();
      } 
      return false; // Skip all further processing of this key

    // Right shift keycode
    case KC_RSFT:
      if (record->event.pressed) {
        rsft_down = true;
      } else {
        rsft_down = false;  
        if(rctl_down) toggle_led();
      }
      break; 

    // Right ctrl keycode
    case KC_RCTL:
      if (record->event.pressed) {
        rctl_down = true;
      } else {
        rctl_down = false; 
        if(rsft_down) toggle_led();
      }
      break;
  }

  // Process key normally
  return true;
}



