#
# SL Keyboard Firmware (KMK extension)
# Version 04.04.2022
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

class SLKeyboard(KMKKeyboard):

    # light management
    light_pin = None

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

    # special overloaded key codes
    _RCTRLX = None  # right ctrl key
    _RSFTX = None   # right shift key
    _RALTX = None   # right alt key

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

    # Experimental method of receiving bytes from the user OS.
    def check_serial_input(self):
        while supervisor.runtime.serial_bytes_available:
            value = sys.stdin.read(1)
            log(f'Received: {value}')

    def get_key_name(self, key):
        if key == self._RCTRLX: return 'RCTRLX'
        if key == self._RALTX: return 'RALTX'
        if key == self._RSFTX: return 'RSHTX'
        return '?'

    def is_pressed(self, key):
        return (key in self.keys_pressed)

    def send_press_release(self, key1, key2):
        self.keys_pressed.clear()
        self.keys_pressed.add(key1)
        self.keys_pressed.add(key2)
        self._send_hid()
        self.keys_pressed.clear()
        self._send_hid()

    def perform_switch(self):
        log('Switch')
        self.toggle_light()
        # simulate the ctrl+shift, then alt+shift
        self.send_press_release(self._RCTRLX, self._RSFTX)
        self.send_press_release(self._RALTX, self._RSFTX)

    def maybe_perform_switch(self):
        if self.is_pressed(self._RSFTX):
           if ((self.is_pressed(self._RCTRLX) and not self.is_pressed(self._RALTX)) or
               (self.is_pressed(self._RALTX) and not self.is_pressed(self._RCTRLX))):
                self.perform_switch()

    def maybe_perform_switch_before_release(self):
        if self.is_pressed(self._RSFTX) and self.is_pressed(self._RCTRLX):
            self.toggle_light()

    def on_x_key_pressed(self, key):
        log(f'{self.get_key_name(key)}=on')
        self.keys_pressed.add(key)
        self.hid_pending = True
        #self.maybe_perform_switch()

    def on_x_key_released(self, key):
        log(f'{self.get_key_name(key)}=off')
        # turn the easy mode for debugging (04/04/2022)
        self.maybe_perform_switch_before_release()
        self.keys_pressed.discard(key)
        self.hid_pending = True

def on_x_key_pressed(key, keyboard, KC, coord_int=None, coord_raw=None, *args, **kwargs):
    keyboard.on_x_key_pressed(key)
    return keyboard

def on_x_key_released(key, keyboard, KC, coord_int=None, coord_raw=None, *args, **kwargs):
    keyboard.on_x_key_released(key)
    return keyboard

# Debug printing.
def log(message):
    _log_enabled = False
    if _log_enabled: print(message)

# Program start.
if __name__ == "__main__":
    log("Keyboard setup...")
    keyboard = SLKeyboard()
    keyboard.setup_lights()
    keyboard.setup_keys()
    keyboard.welcome_flashes()
    log("Keyboard go...")
    keyboard.go()

