# -*- encoding: utf-8 -*-

from PyQt5.QtCore import QDir, QFileInfo, QStandardPaths

import mobase

from ..basic_game import BasicGame


class IllusionCom3d2Game(BasicGame):
    Name = "Custom order maid 3d2"
    Author = "TokcDK<Denis K>"
    Version = "0.1.0"

    GameName = "Com3d2"
    GameShortName = "com3d2"
    GameNexusName = ""
    GameNexusId = 0
    GameSteamId = 0
    GameBinary = "Data/COM3D2x64.exe"
    GameDataPath = ""

    def executables(self):
        return [
            mobase.ExecutableInfo(
                "Com3d2",
                QFileInfo(self.gameDirectory(), "Data/COM3D2x64.exe"),
            )
        ]

    def dataDirectory(self):
        return QDir(self._gamePath+"/Data")
