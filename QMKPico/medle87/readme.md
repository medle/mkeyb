
See the [build environment setup](https://docs.qmk.fm/#/getting_started_build_tools) 
and the [make instructions](https://docs.qmk.fm/#/getting_started_make_guide) for more information. Brand new to QMK? Start with our [Complete Newbs Guide](https://docs.qmk.fm/#/newbs).

## Bootloader

Enter the bootloader in 3 ways:

* **Bootmagic reset**: Hold down the key at (0,0) in the matrix (usually the top left key or Escape) and plug in the keyboard
* **Physical reset button**: Short the 'USB_BOOT' button and plug in keyboard or briefly short the `RESET` and `GND` pads on the SWD header twice
* **Keycode in layout**: Press the key mapped to `QK_BOOT` if it is available

## How to build

Copy source code medle87 directory to qmk directory:
C:\Documents and Settings\SL\qmk_firmware\keyboards\medle87

Start QMK MSYS...
qmk config user.keyboard=medle87
qmk config user.keymap=default
qmk config
qmk compile

## How to program the keyboard

After compiling the file "medle87_default.uf2" appears at the directory
C:\Documents and Settings\SL\qmk_firmware
Press +Menu+Pause for the keyboard bootloader to open a new disk in Windows.
Copy medle87_default.uf2 file to that disk.

