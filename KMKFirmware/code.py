#
# SL Keyboard Firmware (KMK extension)
# Version 29.04.2022
#

import board
import digitalio
import time
import kmk.handlers.stock as handlers

import supervisor
import sys

from kmk.kmk_keyboard import KMKKeyboard
from kmk.keys import (KC, make_mod_key)
from kmk.matrix import DiodeOrientation
from kmk.modules import Module

#
# KMK Keyboard class custom plugin module.
#
class SLModule(Module):
    def during_bootup(self, keyboard):
        return

    def before_matrix_scan(self, keyboard):
        return

    def after_matrix_scan(self, keyboard):
        return

    def before_hid_send(self, keyboard):
        # if while waiting for the release of an x key we get any other key pressed
        # discard the waiting so x-key sequence will be aborted
        if keyboard.waiting_x_release and keyboard.x_pressed_len < len(keyboard.keys_pressed):
            keyboard.waiting_x_release = False
            log(f'discard wait')
        return

    def after_hid_send(self, keyboard):
        if keyboard.just_switched:
            keyboard.just_switched = False
            log('just switched')
            send_backup_switch(keyboard)
        return

    def on_powersave_enable(self, keyboard):
        return

    def on_powersave_disable(self, keyboard):
        return

    def process_and_send_hid(self, keyboard, key, pressed):
        keyboard.process_key(key, pressed)
        keyboard._send_hid()

    # This is to emulate left alt/shift sequence.
    def send_backup_switch(self, keyboard):
        oldkeys_pressed = keyboard.keys_pressed
        keyboard.keys_pressed = set()
        self.process_key_and_send_hid(keyboard, KC.LALT, True)
        self.process_key_and_send_hid(keyboard, KC.LSFT, True)
        self.process_key_and_send_hid(keyboard, KC.LALT, False)
        self.process_key_and_send_hid(keyboard, KC.LSFT, False)
        keyboard.keys_pressed = oldkeys_pressed


#
# Custom KMK Keyboard class definition.
#
class SLKeyboard(KMKKeyboard):

    # light management
    light_pin = None

    # special overloaded key codes
    _RCTRLX = None  # right ctrl key
    _RSFTX = None   # right shift key
    _RALTX = None   # right alt key

    # special module to detect all the keypresses
    sl_module = SLModule()

    waiting_x_release = False
    just_switched = False
    x_pressed_len = 0

    def __init__(self):
        self.modules.append(self.sl_module)
        self.setup_lights()
        self.setup_keys()
        return

    def setup_lights(self):
        self.light_pin = digitalio.DigitalInOut(board.GP28)
        self.light_pin.direction = digitalio.Direction.OUTPUT

    def turn_light(self, on):
        self.light_pin.value = on

    def is_light_on(self):
        return self.light_pin.value

    def toggle_light(self):
        self.turn_light(not self.is_light_on())

    def welcome_flashes(self):
        for x in range(2):
            self.turn_light(True)
            time.sleep(0.2)
            self.turn_light(False)
            time.sleep(0.2)

    def setup_keys(self):
        self.debug_enabled = False

        # 18 pins: columns
        self.col_pins = \
            (board.GP0, board.GP1, board.GP2, board.GP3,
             board.GP4, board.GP5, board.GP6, board.GP7,
             board.GP8, board.GP9, board.GP10, board.GP11,
             board.GP12, board.GP13, board.GP14, board.GP16,
             board.GP17, board.GP18)

        # 6 pins: rows (GP19 seems to always read true, so changed to GP15)
        self.row_pins = \
            (board.GP15, board.GP20, board.GP21, board.GP22, board.GP26, board.GP27)

        self.diode_orientation = DiodeOrientation.COL2ROW

        XXX = KC.TRANSPARENT

        self._RCTRLX = make_mod_key(code=KC.RIGHT_CONTROL.code, names=('RCTRLX',),
                                    on_press=on_x_key_pressed,
                                    on_release=on_x_key_released)
        rctrlx = self._RCTRLX

        self._RSFTX = make_mod_key(code=KC.RIGHT_SHIFT.code, names=('RSFTX',),
                                   on_press=on_x_key_pressed,
                                   on_release=on_x_key_released)
        rsftx = self._RSFTX

        self._RALTX = make_mod_key(code=KC.RIGHT_ALT.code, names=('RALTX',),
                                   on_press=on_x_key_pressed,
                                   on_release=on_x_key_released)
        raltx = self._RALTX

        self.keymap = [
            # Qwerty (6 rows + 18 columns = 24 pins)
            #    1         2         3         4       5        6         7        8        9       10       11       12        13        14        15      16       17       18
            # ,--------------------------------------------------------------------------------------------------------------------------------------------------------------------.
            # | ESC     |         |  F1    |  F2   |  F3    |  F4   |          |  F5    |  F6    |  F7    |  F8  |  F9     | F10    |  F11     | F12    | Print  | Scroll | Break  |
            # |--------------------------------------------------------------------------------------------------------------------------------+--------|-----------------+--------|
            # | `       |    1    |   2    |   3   |   4    |   5   |   6      |   7    |   8    |   9    |   0  |   -     |   =    |          | Bksp   | Insert | Home   | PageUp |
            # |---------+---------+--------+-------+--------+-------+----------+--------+--------+--------+------+---------+--------+----------+--------|--------+--------+--------|
            # | Tab     |         |   Q    |   W   |   E    |   R   |   T      |   Y    |   U    |   I    |   O  |   P     |   [    |   ]      |   \    | Delete | End    | PageDn |
            # |---------+---------+--------+-------+--------+-------+----------+--------+--------+--------+------+---------+--------+----------+--------|--------+--------+--------|
            # | Caps    |         |   A    |   S   |   D    |   F   |   G      |   H    |   J    |   K    |   L  |   ;     |   '    | Enter    |        |        |        |        |
            # |---------+---------+--------+-------+--------+-------+----------+--------+--------+--------+------+---------+--------+----------+--------|--------+--------+--------|
            # | Shift   |         |   Z    |   X   |   C    |   V   |   B      |   N    |   M    |   ,    |   .  |   /     |        | Shift    |        |        |   UP   |        |
            # |---------+---------+--------+-------+--------+-------+----------+--------+--------+--------+------+---------+--------+----------+--------|--------+--------+--------|
            # | Ctrl    |  GUI    |  Alt   |       |        |       | Space    |        |        |        | Alt  | GUI     |        | Fn       |  Ctrl  |  LEFT  |  DOWN  | RIGHT  |
            # `--------------------------------------------------------------------------------------------------------------------------------------------------------------------,
            [
                KC.ESC,   XXX,     KC.F1,   KC.F2,   KC.F3,   KC.F4,  XXX,    KC.F5,   KC.F6,   KC.F7,   KC.F8,  KC.F9,   KC.F10,   KC.F11,   KC.F12,    KC.PSCR, KC.SLCK, KC.BRK,
                KC.GRV,   KC.N1,   KC.N2,   KC.N3,   KC.N4,   KC.N5,  KC.N6,  KC.N7,   KC.N8,   KC.N9,   KC.N0,  KC.MINS, KC.EQUAL, XXX,      KC.BSPC,   KC.INS,  KC.HOME, KC.PGUP,
                KC.TAB,   XXX,     KC.Q,    KC.W,    KC.E,    KC.R,   KC.T,   KC.Y,    KC.U,    KC.I,    KC.O,   KC.P,    KC.LBRC,  KC.RBRC,  KC.BSLASH, KC.DEL,  KC.END,  KC.PGDN,
                KC.CAPS,  XXX,     KC.A,    KC.S,    KC.D,    KC.F,   KC.G,   KC.H,    KC.J,    KC.K,    KC.L,   KC.SCLN, KC.QUOT,  KC.ENTER, XXX,       XXX,     XXX,     XXX,
                KC.LSFT,  XXX,     KC.Z,    KC.X,    KC.C,    KC.V,   KC.B,   KC.N,    KC.M,    KC.COMM, KC.DOT, KC.SLSH, XXX,      rsftx,    XXX,       XXX,     KC.UP,   XXX,
                KC.LCTRL, KC.LGUI, KC.LALT, XXX,     XXX,     XXX,    KC.SPC, XXX,     XXX,     XXX,     raltx,  KC.RGUI, XXX,      XXX,      rctrlx,    KC.LEFT, KC.DOWN, KC.RGHT,
            ],
        ]

    def maybe_perform_switch_before_x_release(self):
        log(f'before x release {self.waiting_x_release}')
        if self.is_right_shift_and_control_pressed() and self.waiting_x_release:
            self.toggle_light()
            self.waiting_x_release = False
            self.just_switched = True

    def is_pressed(self, key):
        return (key in self.keys_pressed)

    def is_right_shift_and_control_pressed(self):
        return (self.is_pressed(self._RSFTX) and self.is_pressed(self._RCTRLX))

    def get_key_name(self, key):
        if key == self._RCTRLX: return 'RCTRLX'
        if key == self._RALTX: return 'RALTX'
        if key == self._RSFTX: return 'RSHTX'
        return '?'

    def on_x_key_pressed(self, key):
        log(f'{self.get_key_name(key)}=on')
        self.keys_pressed.add(key)
        self.hid_pending = True
        if self.is_right_shift_and_control_pressed():
            log("both pressed");
            self.waiting_x_release = True
            self.x_pressed_len = len(self.keys_pressed)

    def on_x_key_released(self, key):
        log(f'{self.get_key_name(key)}=off')
        self.maybe_perform_switch_before_x_release()
        self.keys_pressed.discard(key)
        self.hid_pending = True

#
# Custom key handlers.
#
def on_x_key_pressed(key, keyboard, KC, coord_int=None, coord_raw=None, *args, **kwargs):
    keyboard.on_x_key_pressed(key)
    return keyboard

def on_x_key_released(key, keyboard, KC, coord_int=None, coord_raw=None, *args, **kwargs):
    keyboard.on_x_key_released(key)
    return keyboard

#
# Debug printing.
#
def log(message):
    _log_enabled = False
    if _log_enabled: print(message)

#
# Program start.
#
if __name__ == "__main__":
    log("Keyboard setup...")
    keyboard = SLKeyboard()
    keyboard.welcome_flashes()
    log("Keyboard go...")
    keyboard.go()

