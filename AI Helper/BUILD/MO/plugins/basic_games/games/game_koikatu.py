# -*- encoding: utf-8 -*-

from PyQt6.QtCore import QDir, QFileInfo, QStandardPaths

import mobase

from ..basic_game import BasicGame


class IllusionKoikatsuGame(BasicGame):
    Name = "Illusion Koikatsu!"
    Author = "TokcDK<Denis K>"
    Version = "0.1.0"

    GameName = "Koikatu"
    GameShortName = "koikatu"
    GameNexusName = ""
    GameNexusId = 0
    GameSteamId = 0
    GameBinary = "Data/Koikatu.exe"
    GameDataPath = ""

    def executables(self):
        return [
            mobase.ExecutableInfo(
                "Koikatu",
                QFileInfo(self.gameDirectory(), "Data/Koikatu.exe"),
            ),
            mobase.ExecutableInfo(
                "Settings",
                QFileInfo(self.gameDirectory(), "Data/InitSetting.exe"),
            ),
            mobase.ExecutableInfo(
                "Koikatu_VR",
                QFileInfo(self.gameDirectory(), "Data/KoikatuVR.exe"),
            ),
            mobase.ExecutableInfo(
                "Studio",
                QFileInfo(self.gameDirectory(), "Data/CharaStudio.exe"),
            )
        ]

    def dataDirectory(self):
        return QDir(self._gamePath+"/Data")
