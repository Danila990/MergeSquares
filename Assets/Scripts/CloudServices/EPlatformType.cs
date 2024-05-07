using System;

namespace CloudServices
{
    [Serializable]
    public enum EPlatformType : byte
    {
        YANDEX = 0,
        VK = 1,
        CRAZY_GAMES = 2,
        GAME_DISTRIBUTION = 3,
        GAME_MONETIZE = 4,
        OK = 5,
        SMARTMARKET = 6,
        GAMEPIX = 7,
        POKI = 8,
        VK_PLAY = 9,
        None = 10,
        YANDEX_NAO = 100,
        CRAZY_GAMES_NAO = 101
    }
}