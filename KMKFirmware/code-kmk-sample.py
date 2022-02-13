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

keyboard.col_pins = (board.GP0,)
keyboard.row_pins = (board.GP1,)
keyboard.diode_orientation = DiodeOrientation.COL2ROW

keyboard.keymap = [
    [                    
        KC.A,
    ]
]

if __name__ == "__main__":
    keyboard.go()

