# -*- encoding: utf-8 -*-

from PyQt6.QtCore import QDir, QFileInfo, QStandardPaths

import mobase

from ..basic_game import BasicGame


class IllusionHoneySelectGame(BasicGame):
    Name = "Illusion Honey Select"
    Author = "TokcDK<Denis K>"
    Version = "0.1.0"

    GameName = "HoneySelect"
    GameShortName = "honeyselect"
    GameNexusName = ""
    GameNexusId = 0
    GameSteamId = 0
    GameBinary = "Data/HoneySelect_64.exe"
    GameDataPath = ""

    def executables(self):
        return [
            mobase.ExecutableInfo(
                "HoneySelect_x64",
                QFileInfo(self.gameDirectory(), "Data/HoneySelect_64.exe"),
            ),
            mobase.ExecutableInfo(
                "HoneySelect_x32",
                QFileInfo(self.gameDirectory(), "Data/HoneySelect_32.exe"),
            ),
            mobase.ExecutableInfo(
                "Settings",
                QFileInfo(self.gameDirectory(), "Data/InitSetting.exe"),
            ),
            mobase.ExecutableInfo(
                "Studio_NEO",
                QFileInfo(self.gameDirectory(), "Data/StudioNEO_64.exe"),
            ),
            mobase.ExecutableInfo(
                "Studio",
                QFileInfo(self.gameDirectory(), "Data/HoneyStudio_64.exe"),
            ),
            mobase.ExecutableInfo(
                "Studio_NEO_x32",
                QFileInfo(self.gameDirectory(), "Data/StudioNEO_32.exe"),
            ),
            mobase.ExecutableInfo(
                "Studio_x32",
                QFileInfo(self.gameDirectory(), "Data/HoneyStudio_32.exe"),
            )
        ]

    def dataDirectory(self):
        return QDir(self._gamePath+"/Data")
