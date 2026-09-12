using System;

namespace Game.Input.Structure
{
    [Serializable]
    public struct ScriptedBoostEvent
    {
        public float time;
        public int level;

        public ScriptedBoostEvent(float time, int level)
        {
            this.time = time;
            this.level = level;
        }
    }
}
