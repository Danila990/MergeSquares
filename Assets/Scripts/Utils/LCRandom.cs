namespace Utils {
	// ReSharper disable once InconsistentNaming
	// ReSharper disable SuggestVarOrType_BuiltInTypes

	public class LCRandom {
		private static readonly uint M = 2147483647;
		private static readonly uint Q = 127773;
		private static readonly int A = 16807;
		private static readonly int R = 2836;

		public uint seed;

		public LCRandom( int seed ) {
			if ( seed > 0 ) {
				this.seed = (uint)seed;
			}
			else {
				this.seed = (uint)( seed & 0x7FFFFFFFL ) | 0x80000000;
			}
		}

		public LCRandom( uint seed = 0 ) {
			this.seed = seed;
		}

		public int Random() {
			return ( NextByte() | ( NextByte() << 8 ) | ( NextByte() << 16 ) | ( NextByte() << 24 ) ) & 0x7FFFFFFF;
		}

		/**
     * @param int low lower bound inclusive
     * @param int hi upper bound exclusive
     * @return int random value in range [low, hi)
     */
		public int Range32( int low, int hi ) {
			int z = hi - low;
			int v = Random();
			return v % z + low;
		}

		private int NextByte() {
			var hi = seed / Q;
			var low = seed % Q;
			var test = A * low - R * hi;
			if ( test > 0 ) {
				seed = (uint)test;
			}
			else {
				seed = (uint)( M + test );
			}

			int x = 0;

			x ^= (int)( seed & 0xFF );
			x ^= (int)( ( seed >> 8 ) & 0xFF );
			x ^= (int)( ( seed >> 16 ) & 0xFF );
			x ^= (int)( ( seed >> 24 ) & 0xFF );

			return x;
		}
	}
}