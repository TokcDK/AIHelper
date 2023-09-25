# -*- encoding: utf-8 -*-

from PyQt5.QtCore import QDir, QFileInfo, QStandardPaths

import mobase

from ..basic_game import BasicGame


class IllusionHoneySelect2Game(BasicGame):
    Name = "Illusion Honey Select 2: Libido"
    Author = "TokcDK<Denis K>"
    Version = "0.1.0"

    GameName = "HoneySelect2"
    GameShortName = "honeyselect2"
    GameNexusName = ""
    GameNexusId = 0
    GameSteamId = 0
    GameBinary = "Data/HoneySelect2.exe"
    GameDataPath = ""

    def executables(self):
        return [
            mobase.ExecutableInfo(
                "HoneySelect_x64",
                QFileInfo(self.gameDirectory(), "Data/HoneySelect2.exe"),
            ),
            mobase.ExecutableInfo(
                "Settings",
                QFileInfo(self.gameDirectory(), "Data/InitSetting.exe"),
            ),
            mobase.ExecutableInfo(
                "Studio_NEO",
                QFileInfo(self.gameDirectory(), "Data/StudioNEOV2.exe"),
            ),
        ]

    def dataDirectory(self):
        return QDir(self._gamePath+"/Data")
