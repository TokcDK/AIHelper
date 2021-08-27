# -*- encoding: utf-8 -*-

from PyQt5.QtCore import QDir, QFileInfo, QStandardPaths

import mobase

from ..basic_game import BasicGame


class IllusionKoikatuGame(BasicGame):
    Name = "Illusion Koikatsu Sunshine!"
    Author = "TokcDK<Denis K>"
    Version = "0.1.0"

    GameName = "Koikatu Sunshine"
    GameShortName = "koikatusunshine"
    GameNexusName = ""
    GameNexusId = 0
    GameSteamId = 0
    GameBinary = "Data/KoikatuSunshine.exe"
    GameDataPath = ""

    def executables(self):
        return [
            mobase.ExecutableInfo(
                "Koikatu Sunshine",
                QFileInfo(self.gameDirectory(), "Data/KoikatuSunshine.exe"),
            ),
            mobase.ExecutableInfo(
                "Settings",
                QFileInfo(self.gameDirectory(), "Data/InitSetting.exe"),
            ),
            mobase.ExecutableInfo(
                "Koikatu Sunshine VR",
                QFileInfo(self.gameDirectory(), "Data/KoikatuSunshineVR.exe"),
            ),
            mobase.ExecutableInfo(
                "Studio",
                QFileInfo(self.gameDirectory(), "Data/CharaStudio.exe"),
            )
        ]

    def dataDirectory(self):
        return QDir(self._gamePath+"/Data")
