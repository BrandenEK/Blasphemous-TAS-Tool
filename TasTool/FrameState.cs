using UnityEngine;

namespace TasTool
{
    [System.Serializable]
    public struct FrameState
    {
        //public Random.State rng;

        private int _input;

        public int Input => _input;

        public FrameState(int input)
        {
            _input = input;
        }

        public bool Attack
        {
            get => (_input & 1 << 0) > 0;
            set => _input |= (value ? 1 : 0) << 0;
        }

        public bool Jump
        {
            get => (_input & 1 << 1) > 0;
            set => _input |= (value ? 1 : 0) << 1;
        }

        public bool RangedAttack
        {
            get => (_input & 1 << 2) > 0;
            set => _input |= (value ? 1 : 0) << 2;
        }

        public bool Interact
        {
            get => (_input & 1 << 3) > 0;
            set => _input |= (value ? 1 : 0) << 3;
        }
    }
}
