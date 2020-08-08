# -*- encoding: utf-8 -*-

from PyQt5.QtCore import QDir, QFileInfo, QStandardPaths

import mobase

from ..basic_game import BasicGame


class IllusionKoikatuGame(BasicGame):
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
                "Setting",
                QFileInfo(self.gameDirectory(), "Data/InitSetting.exe"),
            ),
            mobase.ExecutableInfo(
                "Koikatu VR",
                QFileInfo(self.gameDirectory(), "Data/KoikatuVR.exe"),
            ),
            mobase.ExecutableInfo(
                "Studio",
                QFileInfo(self.gameDirectory(), "Data/CharaStudio.exe"),
            )
        ]

    def dataDirectory(self):
        return QDir(self._gamePath+"/Data")
