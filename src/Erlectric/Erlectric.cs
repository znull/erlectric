using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Erlectric {
	public class ETFCodec {
		public static byte[] Encode(object obj, int? compress = null) {
			if(compress != null) {
				throw new NotSupportedException("compression not yet supported");
			}

			var parts = new List<byte[]>();
			EncodePart(obj, parts);
			var encoded = new byte[1 + parts.Sum(e => e.Length)];
			int i = 0;
			encoded[i++] = Constants.FORMAT_VERSION;

			foreach(var part in parts) {
				Buffer.BlockCopy(part, 0, encoded, i, part.Length);
				i += part.Length;
			}
			return encoded;
		}

		public static byte[] ToBytes(string s) {
			return Encoding.UTF8.GetBytes(s);
		}

		public static string ToUTF8(object bytes) {
			return ToString(bytes, Encoding.UTF8);
		}

		public static string ToASCII(object bytes) {
			return ToString(bytes, Encoding.ASCII);
		}

		public static string ToString(object bytes, Encoding enc) {
			return enc.GetString((byte[])bytes);
		}

		internal static void EncodePart(object obj, List<byte[]> parts) {
			switch(Convert.GetTypeCode(obj)) {
				case TypeCode.String:
					AddByte(parts, Constants.BINARY_EXT);
					var bytes = Encoding.UTF8.GetBytes((string)obj);
					parts.Add(UIntAsBigEndian((uint)bytes.Length));
					parts.Add(bytes);
					break;

				case TypeCode.SByte:
				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
					EncodePart(Convert.ToInt64(obj), parts);
					break;

				case TypeCode.UInt64:
					EncodePart((ulong)obj, parts);
					break;

				case TypeCode.Single:
				case TypeCode.Double:
					AddByte(parts, Constants.NEW_FLOAT_EXT);
					var dbytes = BitConverter.GetBytes(Convert.ToDouble(obj));
					FlipBigEndian(dbytes);
					parts.Add(dbytes);
					break;

				case TypeCode.Empty:
					throw new NotSupportedException("null");

				default:
					EncodeOther(obj, parts);
					break;
			}
		}

		internal static void EncodePart(ulong u, List<byte[]> parts) {
			if(u <= Byte.MaxValue) {
				parts.Add(new byte[] { Constants.SMALL_INTEGER_EXT, (byte)u });
			} else if (u <= Int32.MaxValue) {
				EncodeInt((int)u, parts);
			} else {
				EncodeBig(u, 0, parts);
			}
		}

		internal static void EncodePart(long i, List<byte[]> parts) {
			if(i >= Int32.MinValue) {
				if(i < 0) {
					EncodeInt((int)i, parts);
				} else if(i <= Byte.MaxValue) {
					parts.Add(new byte[] { Constants.SMALL_INTEGER_EXT, (byte)i });
				} else if (i <= Int32.MaxValue) {
					EncodeInt((int)i, parts);
				} else {
					EncodeBig((ulong)i, 0, parts);
				}
			} else {
				if(i == Int64.MinValue) {
					parts.Add(new byte[] { Constants.SMALL_BIG_EXT, 8, 1, 0, 0, 0, 0, 0, 0, 0, 128 });
				} else {
					EncodeBig((ulong)-i, 1, parts);
				}
			}
		}

		internal static void EncodeOther(object obj, List<byte[]> parts) {
			byte[] b = obj as byte[];
			if(b != null) {
				AddByte(parts, Constants.BINARY_EXT);
				parts.Add(UIntAsBigEndian((uint)b.Length));
				parts.Add(b);
				return;
			}

			ETFTuple t = obj as ETFTuple;
			if(t != null) {
				if(t.Count <= Byte.MaxValue) {
					parts.Add(new byte[] { Constants.SMALL_TUPLE_EXT, (byte)t.Count });
				} else {
					AddByte(parts, Constants.LARGE_TUPLE_EXT);
					parts.Add(UIntAsBigEndian((uint)t.Count));
				}
				foreach(var e in t) {
					EncodePart(e, parts);
				}
				return;
			}

			Atom a = obj as Atom;
			if(a != null) {
				if(a.Name.Length <= Byte.MaxValue) {
					parts.Add(new byte[] { Constants.SMALL_ATOM_EXT, (byte)a.Name.Length });
				} else {
					AddByte(parts, Constants.ATOM_EXT);
					parts.Add(UshortAsBigEndian((ushort)a.Name.Length));
				}
				parts.Add(a.Name);
				return;
			}

			IList list = obj as IList;
			if(list != null) {
				if(list.Count > 0) {
					AddByte(parts, Constants.LIST_EXT);
					parts.Add(IntAsBigEndian(list.Count));
					foreach(var e in list) {
						EncodePart(e, parts);
					}
				}
				AddByte(parts, Constants.NIL_EXT);
				return;
			}

			throw new NotSupportedException(string.Format("{0}: {1}", obj.GetType(), obj.ToString()));
		}

		internal static void EncodeInt(int i, List<byte[]> parts) {
			AddByte(parts, Constants.INTEGER_EXT);
			parts.Add(IntAsBigEndian(i));
		}

		internal static void EncodeBig(ulong u, byte sign, List<byte[]> parts) {
			var bytes = new List<byte>();
			while(u > 0) {
				bytes.Add((byte)u);
				u >>= 8;
			}
			if(bytes.Count <= Byte.MaxValue) {
				AddByte(parts, Constants.SMALL_BIG_EXT);
				AddByte(parts, (byte)bytes.Count);
			} else {
				AddByte(parts, Constants.LARGE_BIG_EXT);
				parts.Add(IntAsBigEndian(bytes.Count));
			}
			AddByte(parts, sign);
			parts.Add(bytes.ToArray());
		}

		internal static void AddByte(List<byte[]> parts, byte b) {
			parts.Add(new byte[] { b });
		}

		internal static byte[] IntAsBigEndian(int i) {
			return FlipBigEndian(BitConverter.GetBytes(i));
		}

		internal static byte[] UIntAsBigEndian(uint i) {
			return FlipBigEndian(BitConverter.GetBytes(i));
		}

		internal static byte[] UshortAsBigEndian(ushort u) {
			return FlipBigEndian(BitConverter.GetBytes(u));
		}

		internal static byte[] FlipBigEndian(byte[] array) {
			if(BitConverter.IsLittleEndian) {
				Array.Reverse(array);
			}
			return array;
		}

		public static object Decode(byte[] encoded, int offset = 0) {
			var version = encoded[offset++];
			if(version != Constants.FORMAT_VERSION) {
				throw new EncodingError(string.Format("Invalid version number {0} != {1}", version, Constants.FORMAT_VERSION));
			}
			return DecodePart(encoded, ref offset);
		}

		internal static object DecodePart(byte[] encoded, ref int offset) {
			byte encoding_type = encoded[offset++];
			switch(encoding_type) {
				case Constants.SMALL_INTEGER_EXT:
					return encoded[offset++];

				case Constants.INTEGER_EXT:
					return DecodeInt(encoded, ref offset);

				case Constants.STRING_EXT:
					ushort slen = DecodeUshort(encoded, ref offset);
					var cl = new List<byte>(slen);
					for(var i = 0; i < slen; i++) {
						cl.Add(encoded[offset + i]);
					}
					offset += slen;
					return cl;

				case Constants.BINARY_EXT:
					int blen = (int)DecodeUint(encoded, ref offset);
					byte[] b = new byte[blen];
					Buffer.BlockCopy(encoded, offset, b, 0, blen);
					offset += blen;
					return b;

				case Constants.LIST_EXT:
					uint llen = DecodeUint(encoded, ref offset);
					var list = new ArrayList((int)llen);
					DecodeList(encoded, ref offset, list, llen, true);
					return list;

				case Constants.SMALL_TUPLE_EXT:
					var stlen = encoded[offset++];
					var st = new ETFTuple(stlen);
					DecodeList(encoded, ref offset, st, stlen);
					return st;

				case Constants.LARGE_TUPLE_EXT:
					uint ltlen = DecodeUint(encoded, ref offset);
					var lt = new ETFTuple((int)ltlen);
					DecodeList(encoded, ref offset, lt, ltlen);
					return lt;

				case Constants.SMALL_ATOM_EXT:
					var salen = encoded[offset++];
					offset += salen;
					return new Atom(encoded, offset - salen, salen);

				case Constants.ATOM_EXT:
					var alen = DecodeUshort(encoded, ref offset);
					offset += alen;
					return new Atom(encoded, offset - alen, alen);

				case Constants.SMALL_BIG_EXT:
					return DecodeBig(encoded, ref offset, encoded[offset++]);

				case Constants.LARGE_BIG_EXT:
					int size = (int)DecodeUint(encoded, ref offset);
					return DecodeBig(encoded, ref offset, size);

				case Constants.NEW_FLOAT_EXT:
					return DecodeDouble(encoded, ref offset);

				case Constants.FLOAT_EXT:
					double dret;
					string ds = Encoding.ASCII.GetString(encoded, offset, Constants.FLOAT_EXT_BYTES);
					offset += Constants.FLOAT_EXT_BYTES;
					NumberStyles style =
						  NumberStyles.AllowDecimalPoint
						| NumberStyles.AllowExponent
						| NumberStyles.AllowLeadingSign
						| NumberStyles.AllowLeadingWhite;
					if(Double.TryParse(ds, style, NumberFormatInfo.InvariantInfo, out dret)) {
						return dret;
					} else {
						throw new EncodingError(string.Format("invalid float encoding: \"{0}\"", ds));
					}

				case Constants.NIL_EXT:
					return new ArrayList();

				default:
					throw new EncodingError(string.Format("invalid encoding type: {0}", encoding_type));
			}
		}

		internal static ArrayList DecodeList(byte[] encoded, ref int offset, ArrayList list, uint len, bool terminator = false) {
			while(len-- > 0) {
				list.Add(DecodePart(encoded, ref offset));
			}
			if(terminator && encoded[offset++] != Constants.NIL_EXT) {
				throw new NotSupportedException("Improper lists not supported");
			}
			return list;
		}

		internal static int DecodeInt(byte[] encoded, ref int offset) {
			var bytes = Read4HostEndian(encoded, ref offset);
			return BitConverter.ToInt32(bytes, 0);
		}

		internal static uint DecodeUint(byte[] encoded, ref int offset) {
			var bytes = Read4HostEndian(encoded, ref offset);
			return BitConverter.ToUInt32(bytes, 0);
		}

		internal static ushort DecodeUshort(byte[] encoded, ref int offset) {
			var bytes = Read2HostEndian(encoded, ref offset);
			return BitConverter.ToUInt16(bytes, 0);
		}

		internal static double DecodeDouble(byte[] encoded, ref int offset) {
			var bytes = new byte[8];
			Buffer.BlockCopy(encoded, offset, bytes, 0, 8);
			offset += 8;
			FlipBigEndian(bytes);
			return BitConverter.ToDouble(bytes, 0);
		}

		internal static byte[] Read2HostEndian(byte[] encoded, ref int offset) {
			byte[] val;
			if(BitConverter.IsLittleEndian) {
				val = new byte[] { encoded[offset+1], encoded[offset] };
			} else {
				val = new byte[] { encoded[offset], encoded[offset+1] };
			}
			offset += 2;
			return val;
		}

		internal static byte[] Read4HostEndian(byte[] encoded, ref int offset) {
			byte[] val;
			if(BitConverter.IsLittleEndian) {
				val = new byte[] { encoded[offset+3], encoded[offset+2], encoded[offset+1], encoded[offset] };
			} else {
				val = new byte[] { encoded[offset], encoded[offset+1], encoded[offset+2], encoded[offset+3] };
			}
			offset += 4;
			return val;
		}

		internal static object DecodeBig(byte[] encoded, ref int offset, int size) {
			bool negative = 0x01 == encoded[offset++];
			if(size > 8) {
				throw new NotSupportedException(string.Format("integer too big to decode ({0} bytes)", size));
			}
			var buf = new byte[8];
			Buffer.BlockCopy(encoded, offset, buf, 0, size);
			if(!BitConverter.IsLittleEndian) {
				Array.Reverse(buf);
			}
			offset += size;
			ulong magnitude = BitConverter.ToUInt64(buf, 0);
			object result = magnitude;
			if(negative) {
				if(magnitude > ((ulong)Int64.MaxValue) + 1UL) {
					throw new NotSupportedException(string.Format("negative integer too big to convert"));
				}
				result = -(long)magnitude;
			}
			return result;
		}
	}

	public class EncodingError : ApplicationException {
		public EncodingError(string msg) : base(msg) {}
	}
}
