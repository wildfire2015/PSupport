@echo off
echo ��������м��ļ������Ե�......
del /f /s /q *.pdb
del /f /s /q *.obj
del /f /s /q *.ilk
del /f /s /q *.pch
del /f /s /q *.idb
del /f /s /q *.sbr
del /f /s /q *.user
del /f /s /q *.cache
del /f /s /q *.apk
rd /s /q %~dp0"GameTrunk/Library"
rd /s /q %~dp0"GameTrunk/Temp"
rd /s /q %~dp0"GameTrunk/PackOut"
rd /s /q %~dp0"GameTrunk/TempPrefab"
rd /s /q %~dp0"GameTrunk/Assets/StreamingAssets"
rd /s /q %~dp0"GameTrunk/Assets/StreamingAssetsURL"
rd /s /q %~dp0"NativeCode/Editor/bin"
rd /s /q %~dp0"NativeCode/Editor/obj"
rd /s /q %~dp0"NativeCode/Support/bin"
rd /s /q %~dp0"NativeCode/Support/obj"
echo ���ϵͳ�������!
echo ���ڿ�����ִ���ļ�

echo. & pause