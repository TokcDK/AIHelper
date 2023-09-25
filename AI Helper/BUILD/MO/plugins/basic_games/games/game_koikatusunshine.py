# -*- encoding: utf-8 -*-

from PyQt5.QtCore import QDir, QFileInfo, QStandardPaths

import mobase

from ..basic_game import BasicGame


class IllusionKoikatsuSunshineGame(BasicGame):
    Name = "Illusion Koikatsu Sunshine!"
    Author = "TokcDK<Denis K>"
    Version = "0.1.0"

    GameName = "KoikatsuSunshine"
    GameShortName = "koikatsusunshine"
    GameNexusName = ""
    GameNexusId = 0
    GameSteamId = 0
    GameBinary = "Data/KoikatsuSunshine.exe"
    GameDataPath = ""

    def executables(self):
        return [
            mobase.ExecutableInfo(
                "Koikatu_Sunshine",
                QFileInfo(self.gameDirectory(), "Data/KoikatsuSunshine.exe"),
            ),
            mobase.ExecutableInfo(
                "Settings",
                QFileInfo(self.gameDirectory(), "Data/InitSetting.exe"),
            ),
            mobase.ExecutableInfo(
                "Koikatu_Sunshine_VR",
                QFileInfo(self.gameDirectory(), "Data/KoikatsuSunshineVR.exe"),
            ),
            mobase.ExecutableInfo(
                "Studio",
                QFileInfo(self.gameDirectory(), "Data/CharaStudio.exe"),
            )
        ]

    def dataDirectory(self):
        return QDir(self._gamePath+"/Data")
