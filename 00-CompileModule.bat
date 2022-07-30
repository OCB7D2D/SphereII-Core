@echo off

call MC7D2D SCore.dll -recurse:Harmony\*.cs -recurse:Scripts\*.cs ^
/reference:"%PATH_7D2D_MANAGED%/Assembly-CSharp.dll" && ^
echo Successfully compiled SCore.dll

pause