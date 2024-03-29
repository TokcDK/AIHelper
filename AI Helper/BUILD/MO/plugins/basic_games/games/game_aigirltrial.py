# -*- encoding: utf-8 -*-

from PyQt6.QtCore import QDir, QFileInfo, QStandardPaths

import mobase

from ..basic_game import BasicGame


class IllusionAIGirlTrialGame(BasicGame):
    Name = "Illusion AIGirl Trial"
    Author = "TokcDK<Denis K>"
    Version = "0.1.0"

    GameName = "AIGirlTrial"
    GameShortName = "aigirltrial"
    GameNexusName = ""
    GameNexusId = 0
    GameSteamId = 0
    GameBinary = "Data/AI-SyoujyoTrial.exe"
    GameDataPath = ""

    def executables(self):
        return [
            mobase.ExecutableInfo(
                "Editor",
                QFileInfo(self.gameDirectory(), "Data/AI-SyoujyoTrial.exe"),
            ),
            mobase.ExecutableInfo(
                "Settings",
                QFileInfo(self.gameDirectory(), "Data/InitSetting.exe"),
            )
        ]

    def dataDirectory(self):
        return QDir(self._gamePath+"/Data")
