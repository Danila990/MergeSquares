using System;

namespace GameStats
{
    [Serializable]
    public struct GameStatContainer
    {
        public EGameStatType type;
        public int value;

        public GameStatContainer(EGameStatType type, int value = 0)
        {
            this.type = type;
            this.value = value;
        }
    }
}
