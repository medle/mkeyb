
FN = KC.MO(1)
XXXXXXX = KC.TRNS

keyboard.keymap = [
    # Qwerty (6 rows + 18 columns = 24 pins)
    #    1         2         3         4       5        6       7        8        9       10       11       12        13        14        15      16       17
    # ,--------------------------------------------------------------------------------------------------------------------------------|--------------------------.
    # | ESC     |  XXXX   | F1     |  F2   |  F3    |  F4   |  F5    |  F6    |  F7    |  F8    |  F9    | F10     | F11    | F12      | Print  | Scroll | Break  |
    # |--------------------------------------------------------------------------------------------------------------------------------|--------------------------|
    # | `       |   1     |   2    |   3   |   4    |   5   |   6    |   7    |   8    |   9    |   0    |   -     |   =    | Bksp     | Insert | Home   | PageUp |
    # |---------+---------+--------+-------+--------+-------+--------+--------+--------+--------+--------+---------+--------+----------|--------+--------+--------|
    # | Tab     |   Q     |   W    |   E   |   R    |   T   |   Y    |   U    |   I    |   O    |   P    |   [     |   ]    |   \      | Delete | End    | PageDn |
    # |---------+---------+--------+-------+--------+-------+--------+--------+--------+--------+--------+---------+--------+----------|--------+--------+--------|
    # | Caps    |   A     |   S    |   D   |   F    |   G   |   H    |   J    |   K    |   L    |   ;    |   '     |XXXXXX  | Enter    | XXXXXX | XXXXXX | XXXXXX |
    # |---------+---------+--------+-------+--------+-------+--------+--------+--------+--------+--------+---------+--------+----------|--------+--------+--------|
    # | Shift   |   Z     |   X    |   C   |   V    |   B   |   N    |   M    |   ,    |   .    |   /    |XXXXXX   |XXXXXX  | Shift    | XXXXXX |   UP   | XXXXXX |
    # |---------+---------+--------+-------+--------+-------+--------+--------+--------+--------+--------+---------+--------+----------|--------+--------+--------|
    # | Ctrl    | GUI     |  Alt   |XXXXXX |XXXXXX  | Space |XXXXXX  |XXXXXX  |XXXXXX  | Alt    | GUI    | Fn      |XXXXXX  | Ctrl     |  LEFT  |  DOWN  | RIGHT  |
    # `--------------------------------------------------------------------------------------------------------------------------------|--------+--------+--------,
    [
        KC.GESC,  XXXXXXX, KC.F1,   KC.F2,   KC.F3,   KC.F4,  KC.F5,   KC.F6,   KC.F7,   KC.F8,   KC.F9,   KC.F10,  KC.F11,   KC.F12,    KC.PSCR, KC.SLCK, KC.BRK,
        KC.GRV,   KC.N1,   KC.N2,   KC.N3,   KC.N4,   KC.N5,  KC.N6,   KC.N7,   KC.N8,   KC.N9,   KC.N0,   KC.MINS, KC.EQUAL, KC.BSPC,   KC.INS,  KC.HOME, KC.PGUP,  
        KC.TAB,   KC.Q,    KC.W,    KC.E,    KC.R,    KC.T,   KC.Y,    KC.U,    KC.I,    KC.O,    KC.P,    KC.LBRC, KC.RBRC,  KC.BSLASH, KC.DEL,  KC.END,  KC.PGDN,  
        KC.CAPS,  KC.A,    KC.S,    KC.D,    KC.F,    KC.G,   KC.H,    KC.J,    KC.K,    KC.L,    KC.SCLN, KC.QUOT, XXXXXXX,  KC.ENTER,  XXXXXXX, XXXXXXX, XXXXXXX,
        KC.LSFT,  KC.Z,    KC.X,    KC.C,    KC.V,    KC.B,   KC.N,    KC.M,    KC.COMM, KC.DOT,  KC.SLSH, XXXXXXX, XXXXXXX,  KC.RSFT,   XXXXXXX, KC.UP,   XXXXXXX,
        KC.LCTRL, KC.LGUI, KC.LALT, XXXXXXX, XXXXXXX, KC.SPC, XXXXXXX, XXXXXXX, XXXXXXX, KC.RALT, KC.RGUI, FN,      XXXXXXX,  KC.RCTRL   KC.LEFT, KC.DOWN, KC.RGHT,
    ],
]
