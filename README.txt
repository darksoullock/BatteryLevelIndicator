This app is a battery indicator, which uses only voltage for calculations. Useful, if your standard battery indicator doesn't work (mine shows 0% all the time).

Tuning parameters for your device:
	'charged' variable -- voltage for fully charged cell of the battery. At this voltage app will show 100% charge. Usually 4.2 or 4.1.
	'discharged' variable -- cutoff voltage. Recommend to set it slightly higher than the voltage at which device switch off.

Should work correctly for 1 and 3 cell battery (3.7 and 11.1 v). If you have another configuration, replace first if statement in 'GetLevelByVoltage' with 'v/=N' where N is amount of cells.

It assumes that discharge curve is a straight line. Though it's not true, it is close enough.

When started, it shows (clickable) tray icon, which changes color depending on charge: green-yellow-red-black.

Note: The battery charge percentage is not precise and depend on many factors (like screen brightness, cpu load, if charger connected, etc).