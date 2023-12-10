# -*- encoding: utf-8 -*-

from PyQt6.QtCore import QDir, QFileInfo, QStandardPaths

import mobase

from ..basic_game import BasicGame


class IllusionHoneyComeGame(BasicGame):
    Name = "Honey Come"
    Author = "TokcDK<Denis K>"
    Version = "0.1.0"

    GameName = "HoneyCome"
    GameShortName = "honeycome"
    GameNexusName = ""
    GameNexusId = 0
    GameSteamId = 0
    GameBinary = "Data/HoneyCome.exe"
    GameDataPath = ""

    def executables(self):
        return [
            mobase.ExecutableInfo(
                "HoneyCome",
                QFileInfo(self.gameDirectory(), "Data/HoneyCome.exe"),
            ),
            mobase.ExecutableInfo(
                "Settings",
                QFileInfo(self.gameDirectory(), "Data/InitSetting.exe"),
            ),
            mobase.ExecutableInfo(
                "StudioNEOV2",
                QFileInfo(self.gameDirectory(), "Data/StudioNEOV2.exe"),
            ),
        ]

    def dataDirectory(self):
        return QDir(self._gamePath+"/Data")
