# -*- encoding: utf-8 -*-

from PyQt5.QtCore import QDir, QFileInfo, QStandardPaths

import mobase

from ..basic_game import BasicGame


class IllusionHoneySelectGame(BasicGame):
    Name = "Room Girl"
    Author = "TokcDK<Denis K>"
    Version = "0.1.0"

    GameName = "RoomGirl"
    GameShortName = "roomgirl"
    GameNexusName = ""
    GameNexusId = 0
    GameSteamId = 0
    GameBinary = "Data/RoomGirl.exe"
    GameDataPath = ""

    def executables(self):
        return [
            mobase.ExecutableInfo(
                "HoneySelect_x64",
                QFileInfo(self.gameDirectory(), "Data/RoomGirl.exe"),
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
