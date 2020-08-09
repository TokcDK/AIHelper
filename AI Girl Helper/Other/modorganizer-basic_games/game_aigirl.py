# -*- encoding: utf-8 -*-

from PyQt5.QtCore import QDir, QFileInfo, QStandardPaths

import mobase

from ..basic_game import BasicGame


class IllusionAIGirlGame(BasicGame):
    Name = "Illusion AI-Girl"
    Author = "TokcDK<Denis K>"
    Version = "0.1.0"

    GameName = "AIGirl"
    GameShortName = "aigirl"
    GameNexusName = ""
    GameNexusId = 0
    GameSteamId = 0
    GameBinary = "Data/AI-Syoujyo.exe"
    GameDataPath = ""

    def executables(self):
        return [
            mobase.ExecutableInfo(
                "AI Girl",
                QFileInfo(self.gameDirectory(), "Data/AI-Syoujyo.exe"),
            ),
            mobase.ExecutableInfo(
                "Settings",
                QFileInfo(self.gameDirectory(), "Data/InitSetting.exe"),
            ),
            mobase.ExecutableInfo(
                "Studio",
                QFileInfo(self.gameDirectory(), "Data/StudioNEOV2.exe"),
            )
        ]

    def dataDirectory(self):
        return QDir(self._gamePath+"/Data")
