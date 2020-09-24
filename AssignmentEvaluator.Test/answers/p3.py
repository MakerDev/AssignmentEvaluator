# 4
print(65.2, "kg =>", format(65.2 / 0.45359, '.1f'), "pounds")
print(175.3, "cm =>", format(175.3 / 30.48, '.1f'), "feet")
print(36.6, "C =>", format(36.6 * 9 / 5 + 32, '.1f'), "F")

print("{} kg => {:.1f} pounds".format(65.2, 65.2 / 0.45359))
print("{} cm => {:.1f} feet".format(175.3, 175.3 / 30.48))
print("{} C => {:.1f} F".format(36.6, 36.6 * 9 / 5 + 32))
