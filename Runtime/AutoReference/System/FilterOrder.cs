// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

namespace Teo.AutoReference.System {
    public static class FilterOrder {
        public const int First = int.MinValue; // = 2147483648
        public const int PreProcess = -400;
        public const int PreFilter = -300;
        public const int Filter = -200;
        public const int PostFilter = -100;
        public const int Default = 0;
        public const int PreSort = 100;
        public const int Sort = 200;
        public const int PostSort = 300;
        public const int PostProcess = 400;
        public const int Last = int.MaxValue; // = 2147483647
    }
}
