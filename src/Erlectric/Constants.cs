namespace Erlectric {
	public class Constants {
		public const byte FORMAT_VERSION = 131;
		public const byte FLOAT_EXT_BYTES = 31;

		public const byte NEW_FLOAT_EXT		= (byte)'F';	// 70  [Float64:IEEE float]
		public const byte BIT_BINARY_EXT	= (byte)'M';	// 77  [UInt32:Len, UInt8:Bits, Len:Data]
		public const byte SMALL_INTEGER_EXT	= (byte)'a';	// 97  [UInt8:Int]
		public const byte INTEGER_EXT		= (byte)'b';	// 98  [Int32:Int]
		public const byte FLOAT_EXT		= (byte)'c';	// 99  [31:Float String] Float in string format (formatted "%.20e", sscanf "%lf"). Superseded by NEW_FLOAT_EXT
		public const byte ATOM_EXT		= (byte)'d';	// 100 [UInt16:Len, Len:AtomName] max Len is 255
		public const byte REFERENCE_EXT		= (byte)'e';	// 101 [atom:Node, UInt32:ID, UInt8:Creation]
		public const byte PORT_EXT		= (byte)'f';	// 102 [atom:Node, UInt32:ID, UInt8:Creation]
		public const byte PID_EXT		= (byte)'g';	// 103 [atom:Node, UInt32:ID, UInt32:Serial, UInt8:Creation]
		public const byte SMALL_TUPLE_EXT	= (byte)'h';	// 104 [UInt8:Arity, N:Elements]
		public const byte LARGE_TUPLE_EXT	= (byte)'i';	// 105 [UInt32:Arity, N:Elements]
		public const byte NIL_EXT		= (byte)'j';	// 106 empty list
		public const byte STRING_EXT		= (byte)'k';	// 107 [UInt32:Len, Len:Characters]
		public const byte LIST_EXT		= (byte)'l';	// 108 [UInt32:Len, Elements, Tail]
		public const byte BINARY_EXT		= (byte)'m';	// 109 [UInt32:Len, Len:Data]
		public const byte SMALL_BIG_EXT		= (byte)'n';	// 110 [UInt8:n, UInt8:Sign, n:nums]
		public const byte LARGE_BIG_EXT		= (byte)'o';	// 111 [UInt32:n, UInt8:Sign, n:nums]
		public const byte NEW_FUN_EXT		= (byte)'p';	// 112 [UInt32:Size, UInt8:Arity, 16*Uint6-MD5:Uniq, UInt32:Index, UInt32:NumFree, atom:Module, int:OldIndex, int:OldUniq, pid:Pid, NunFree*ext:FreeVars]
		public const byte EXPORT_EXT		= (byte)'q';	// 113 [atom:Module, atom:Function, smallint:Arity]
		public const byte NEW_REFERENCE_EXT	= (byte)'r';	// 114 [UInt16:Len, atom:Node, UInt8:Creation, Len*UInt32:ID]
		public const byte SMALL_ATOM_EXT	= (byte)'s';	// 115 [UInt8:Len, Len:AtomName]
		public const byte FUN_EXT		= (byte)'u';	// 117 [UInt4:NumFree, pid:Pid, atom:Module, int:Index, int:Uniq, NumFree*ext:FreeVars]
		public const byte COMPRESSED		= (byte)'P';	// 80  [UInt4:UncompressedSize, N:ZlibCompressedData]
	}
}
