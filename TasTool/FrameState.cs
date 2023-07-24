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

        public bool GetInput(int input)
        {
            int bit = ConvertInputToBit(input);
            if (bit < 0)
                return false;

            return (_input & 1 << bit) > 0;
        }

        public void SetInput(int input, bool value)
        {
            int bit = ConvertInputToBit(input);
            if (bit < 0)
                return;

            _input |= (value ? 1 : 0) << bit;
        }

        private int ConvertInputToBit(int input)
        {
            return input switch
            {
                5 => 0,
                6 => 1,
                57 => 2,
                8 => 3,
                7 => 5,
                23 => 6,
                25 => 7,
                38 => 8,
                28 => 9,
                29 => 10,
                43 => 11,
                45 => 12,
                64 => 13,
                35 => 14,
                39 => 15,
                50 => 16,
                51 => 17,
                52 => 18,
                60 => 19,
                61 => 20,
                10 => 21,
                22 => 22,
                65 => 23,
                _ => -1
            };
        }
    }
}
