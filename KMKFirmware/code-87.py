import board
import digitalio
import time

from kmk.kmk_keyboard import KMKKeyboard
from kmk.keys import KC
from kmk.matrix import DiodeOrientation

print("KMK Starting")

led = digitalio.DigitalInOut(board.LED)
led.direction = digitalio.Direction.OUTPUT
for x in range(3):
    led.value = True
    time.sleep(0.2)
    led.value = False
    time.sleep(0.2)
led.value = True

keyboard = KMKKeyboard()

# 18 pins: columns
keyboard.col_pins = \
    (board.GP0, board.GP1, board.GP2, board.GP3, 
     board.GP4, board.GP5, board.GP6, board.GP7, 
     board.GP8, board.GP9, board.GP10, board.GP11, 
     board.GP12, board.GP13, board.GP14, board.GP16, 
     board.GP17, board.GP18)

# 6 pins: rows
keyboard.row_pins = \
    (board.GP19, board.GP20, board.GP21, board.GP22, board.GP26, board.GP27)

keyboard.diode_orientation = DiodeOrientation.COL2ROW

XXX = KC.TRNS

keyboard.keymap = [
    # Qwerty (6 rows + 18 columns = 24 pins)
    #    1         2         3         4       5        6       7        8        9       10       11       12        13        14        15      16       17       18
    # ,--------------------------------------------------------------------------------------------------------------------------------------------------------------------.
    # | ESC     |         |  F1    |  F2   |  F3    |  F4   |        |  F5    |  F6    |  F7    |  F8    |  F9     | F10    |  F11     | F12    | Print  | Scroll | Break  |
    # |--------------------------------------------------------------------------------------------------------------------------------+--------|-----------------+--------|
    # | `       |    1    |   2    |   3   |   4    |   5   |   6    |   7    |   8    |   9    |   0    |   -     |   =    |          | Bksp   | Insert | Home   | PageUp |
    # |---------+---------+--------+-------+--------+-------+--------+--------+--------+--------+--------+---------+--------+----------+--------|--------+--------+--------|
    # | Tab     |         |   Q    |   W   |   E    |   R   |   T    |   Y    |   U    |   I    |   O    |   P     |   [    |   ]      |   \    | Delete | End    | PageDn |
    # |---------+---------+--------+-------+--------+-------+--------+--------+--------+--------+--------+---------+--------+----------+--------|--------+--------+--------|
    # | Caps    |         |   A    |   S   |   D    |   F   |   G    |   H    |   J    |   K    |   L    |   ;     |   '    | Enter    |        |        |        |        |
    # |---------+---------+--------+-------+--------+-------+--------+--------+--------+--------+--------+---------+--------+----------+--------|--------+--------+--------|
    # | Shift   |         |   Z    |   X   |   C    |   V   |   B    |   N    |   M    |   ,    |   .    |   /     |        | Shift    |        |        |   UP   |        |
    # |---------+---------+--------+-------+--------+-------+--------+--------+--------+--------+--------+---------+--------+----------+--------|--------+--------+--------|
    # | Ctrl    |  GUI    |  Alt   |       |        |       | Space  |        |        |        | Alt    | GUI     |        | Fn       |  Ctrl  |  LEFT  |  DOWN  | RIGHT  |
    # `--------------------------------------------------------------------------------------------------------------------------------------------------------------------,
    [
        KC.GESC,  XXX,     KC.F1,   KC.F2,   KC.F3,   KC.F4,  XXX,    KC.F5,   KC.F6,   KC.F7,   KC.F8,   KC.F9,   KC.F10,   KC.F11,   KC.F12,    KC.PSCR, KC.SLCK, KC.BRK,
        KC.GRV,   KC.N1,   KC.N2,   KC.N3,   KC.N4,   KC.N5,  KC.N6,  KC.N7,   KC.N8,   KC.N9,   KC.N0,   KC.MINS, KC.EQUAL, XXX,      KC.BSPC,   KC.INS,  KC.HOME, KC.PGUP,  
        KC.TAB,   XXX,     KC.Q,    KC.W,    KC.E,    KC.R,   KC.T,   KC.Y,    KC.U,    KC.I,    KC.O,    KC.P,    KC.LBRC,  KC.RBRC,  KC.BSLASH, KC.DEL,  KC.END,  KC.PGDN,  
        KC.CAPS,  XXX,     KC.A,    KC.S,    KC.D,    KC.F,   KC.G,   KC.H,    KC.J,    KC.K,    KC.L,    KC.SCLN, KC.QUOT,  KC.ENTER, XXX,       XXX,     XXX,     XXX,
        KC.LSFT,  XXX,     KC.Z,    KC.X,    KC.C,    KC.V,   KC.B,   KC.N,    KC.M,    KC.COMM, KC.DOT,  KC.SLSH, XXX,      KC.RSFT,  XXX,       XXX,     KC.UP,   XXX,
        KC.LCTRL, KC.LGUI, KC.LALT, XXX,     XXX,     XXX,    KC.SPC, XXX,     XXX,     XXX,     KC.RALT, KC.RGUI, XXX,      XXX,      KC.RCTRL,  KC.LEFT, KC.DOWN, KC.RGHT,
    ],
]

if __name__ == "__main__":
    keyboard.go()
