
10.06.2023

87 key medle87 default layout:
c:\users\sl\qmk_firmware\layouts\default\tkl_ansi

Нашел простой пример на RP2040:
c:\users\sl\qmk_firmware\keyboards\an_achronism\tetromino
c:\users\sl\qmk_firmware\keyboards\dnworks\sbl

https://docs.qmk.fm/#/getting_started_build_tools
https://docs.qmk.fm/#/newbs_building_firmware

Start QMK MSYS...
qmk config user.keyboard=medle87
qmk config user.keymap=default
qmk compile
cd qmk_firmware
make medle87:default:all

To flash new firmware, press App+Esc then copy 
%$HOME%\qmk_firmware\medle87_default.uf2 file to bootloader disk

https://docs.qmk.fm/#/keymap
https://docs.qmk.fm/#/configurator_default_keymaps
https://github.com/qmk/qmk_firmware/blob/master/docs/custom_quantum_functions.md

# Medle87 matrix (diode_direction: COL2ROW)
# Qwerty (6 rows + 18 columns = 24 pins)
#    0         1         2         3       4        5         6        7        8       9        10       11        12        13        14      15       16       17
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

Enter the bootloader in 3 ways:

* **Bootmagic reset**: Hold down the key at (0,0) in the matrix (usually the top left key or Escape) and plug in the keyboard
* **Physical reset button**: Short the 'USB_BOOT' button and plug in keyboard or briefly short the `RESET` and `GND` pads on the SWD header twice
* **Keycode in layout**: Press the key mapped to `QK_BOOT` if it is available



