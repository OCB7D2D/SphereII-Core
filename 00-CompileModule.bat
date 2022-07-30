@echo off

call MC7D2D SCore.dll -recurse:*.cs /reference:"%PATH_7D2D_MANAGED%/Assembly-CSharp.dll" && ^
echo Successfully compiled SCore.dll

pause