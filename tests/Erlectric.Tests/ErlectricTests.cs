using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Erlectric
{
	using NUnit.Framework;

	[TestFixture]
	public class ErlectricTests
	{
		[Test]
		public void TestHeterogenousList() {
			var objects = new ArrayList() {
				new ArrayList(),
				new ArrayList() {
					Byte.MinValue,			// 0
					(byte)27,			// 27
					Byte.MaxValue,			// 255
					UInt16.MinValue,		// 0
					(ushort)17,			// 17
					(ushort)Byte.MaxValue,		// 255
				},
				new ArrayList() {
					(ushort)256,			// 256
					(ushort)(Byte.MaxValue + 1),	// 256
					UInt16.MaxValue,		// 65535
					UInt32.MinValue,		// 0
					(uint)UInt16.MaxValue,		// 65535
					(uint)(UInt16.MaxValue + 1),	// 65536
					987654U,			// 976654
					-1,				// -1
					-255,				// -255
					-256,				// -256
					Int32.MinValue,			// -2147483648
					Int32.MaxValue,			//  2147483647
				},
				new ArrayList() {
					(long)Int32.MaxValue + 1,	// 2147483648
					(ulong)Int32.MaxValue + 1U,	// 2147483648
					UInt32.MaxValue,		// 4294967295
					(ulong)UInt32.MaxValue + 1,	// 4294967296
					Int64.MaxValue,			//  9223372036854775807
					(ulong)Int64.MaxValue + 1,	//  9223372036854775808
					Int64.MinValue + 1,		// -9223372036854775807
					Int64.MinValue,			// -9223372036854775808
					UInt64.MinValue,		// 0
					UInt64.MaxValue,		// 18446744073709551615
				},
			};
			TestRoundtrip(objects);
		}

		[Test]
		public void TestNull() {
			Assert.Throws<NotSupportedException>( () => { ETFCodec.Encode(null); } );
		}

		[Test]
		public void TestBool() {
			Assert.Throws<NotSupportedException>( () => { ETFCodec.Encode(true); } );
			Assert.Throws<NotSupportedException>( () => { ETFCodec.Encode(false); } );
		}

		[Test]
		public void TestEmptyList() {
			Assert.That(ETFCodec.Encode(new ArrayList()), Is.EqualTo(new byte[] { Constants.FORMAT_VERSION, Constants.NIL_EXT }));
		}

		[Test]
		public void TestAtom() {
			TestRoundtrip(new ETFTuple() {
				new Atom("foo"),
				new Atom(""),
			});
			Assert.Throws<EncoderFallbackException> ( () => { new Atom("unicode: ກ ຜ ໄ ໓"); });

			string longstr = "";
			for(int i = 0; i < 333; i++) {
				longstr += "x";
			}
			TestRoundtrip(new Atom(longstr));
		}

		[Test]
		public void TestAtomHashing() {
			var a = new Atom("a");
			var b1 = new Atom("b");
			var b2 = new Atom("b");
			var c1 = new Atom("ccccccccccccccccccccccccccccccccccc");
			var c2 = new Atom("ccccccccccccccccccccccccccccccccccc");
			Assert.That(a, Is.EqualTo(a));
			Assert.That(a, Is.Not.EqualTo(b1));
			Assert.That(b1, Is.EqualTo(b2));
			Assert.That(b1.GetHashCode(), Is.EqualTo(b2.GetHashCode()));
			Assert.That(c1.GetHashCode(), Is.EqualTo(c2.GetHashCode()));
		}

		[Test]
		public void TestBinary() {
			TestRoundtrip(new byte[0]);
			TestRoundtrip(new byte[] { 0, 1, 2, 3 });
		}

		[Test, Sequential]
		public void TestString([Values("nonempty string", "unicode: ກ ຜ ໄ ໓", "")] string s) {
			Assert.That(Roundtrip(s), Is.EqualTo(ETFCodec.ToBytes(s)));
		}

		[Test]
		public void TestInt() {
			TestRoundtrip(Byte.MinValue);				// 0
			TestRoundtrip((byte)27);				// 27
			TestRoundtrip(Byte.MaxValue);				// 255
			TestRoundtrip(UInt16.MinValue, typeof(byte));		// 0
			TestRoundtrip((ushort)17, typeof(byte));		// 17
			TestRoundtrip((ushort)Byte.MaxValue, typeof(byte));	// 255
			TestRoundtrip((ushort)256, typeof(int));		// 256
			TestRoundtrip(UInt16.MaxValue, typeof(int));		// 65535
			TestRoundtrip(UInt32.MinValue, typeof(byte));		// 0
			TestRoundtrip((uint)UInt16.MaxValue, typeof(int));	// 65535
			TestRoundtrip((uint)(UInt16.MaxValue + 1), typeof(int));// 65536
			TestRoundtrip(987654U, typeof(int));			// 976654
			TestRoundtrip(-1);					// -1
			TestRoundtrip(-255);					// -255
			TestRoundtrip(-256);					// -256
			TestRoundtrip(Int32.MinValue);				// -2147483648
			TestRoundtrip(Int32.MaxValue);				//  2147483647
			TestRoundtrip((long)Int32.MaxValue + 1, typeof(ulong));	// 2147483648
			TestRoundtrip((ulong)Int32.MaxValue + 1U);		// 2147483648
			TestRoundtrip(UInt32.MaxValue, typeof(ulong));		// 4294967295
			TestRoundtrip((ulong)UInt32.MaxValue + 1);		// 4294967296
			TestRoundtrip(Int64.MaxValue, typeof(ulong));		//  9223372036854775807
			TestRoundtrip((ulong)Int64.MaxValue + 1);		//  9223372036854775808
			TestRoundtrip(Int64.MinValue + 1);			// -9223372036854775807
			TestRoundtrip(Int64.MinValue);				// -9223372036854775808
			TestRoundtrip(UInt64.MinValue, typeof(byte));		// 0
			TestRoundtrip(UInt64.MaxValue);				// 18446744073709551615
		}

		[Test]
		public void TestFloat() {
			var inputs = new Dictionary<string, double> { 
				{ "     7.75000000000000000000e+00",	 7.75 },
				{ "    -7.75000000000000000000e+00",	-7.75 },
				{ "     8.00000000170000036009e-28",	 0.00000000000000000000000000080000000017 },
				{ "    -9.99999999999999929757e+44",	 -999999999999999999999999999999999999999999999.99 },
			};
			foreach(var entry in inputs)
			{
				var num = Encoding.ASCII.GetBytes(entry.Key);
				var enc = new byte[ num.Length + 2 ];
				enc[0] = Constants.FORMAT_VERSION;
				enc[1] = Constants.FLOAT_EXT;
				Array.Copy(num, 0, enc, 2, num.Length);
				Assert.That(ETFCodec.Decode(enc), Is.EqualTo(entry.Value));
			}

			TestRoundtrip(new ArrayList() {
					1.234,
					1.234f,
					-4.567,
					-4.567f,
					Double.MinValue,
					Double.MaxValue,
					Double.Epsilon,
					Double.NaN,
					Double.NegativeInfinity,
					Double.PositiveInfinity,
					});
		}

		[Test]
		public void TestTuple() {
			TestRoundtrip(new ETFTuple());
			TestRoundtrip(new ETFTuple() { 7 });
			TestRoundtrip(new ArrayList() { new ETFTuple() { 7 }, 17});
			TestRoundtrip(new ETFTuple(Enumerable.Range(0, 300).ToList()));

			Assert.That(new ETFTuple().ToString(), Is.EqualTo("{}"));
			Assert.That((new ETFTuple() { 12 }).ToString(), Is.EqualTo("{ 12 }"));
			Assert.That((new ETFTuple() { "x", "y" }).ToString(), Is.EqualTo("{ x, y }"));
		}

		[Test]
		public void Test_SMALL_BIG_EXT() {
			var enc1 = new byte[] { Constants.FORMAT_VERSION, Constants.SMALL_BIG_EXT, 1, 0, 1, };
			Assert.That(ETFCodec.Decode(enc1), Is.EqualTo(1));

			var enc2 = new byte[] { Constants.FORMAT_VERSION, Constants.SMALL_BIG_EXT,
				8, 0,
				255, 255, 255, 255,
				255, 255, 255, 255,
			};
			Assert.That(ETFCodec.Decode(enc2), Is.EqualTo(UInt64.MaxValue));

			var enc3 = new byte[] { Constants.FORMAT_VERSION, Constants.SMALL_BIG_EXT,
				8, 1,
				0, 0, 0, 0,
				0, 0, 0, 128,
			};
			Assert.That(ETFCodec.Decode(enc3), Is.EqualTo(Int64.MinValue));

			var enc4 = new byte[] { Constants.FORMAT_VERSION, Constants.SMALL_BIG_EXT,
				8, 1,
				1, 0, 0, 0,
				0, 0, 0, 128,
			};
			Assert.Throws<NotSupportedException> ( () => { ETFCodec.Decode(enc4); } );
		}

		[Test]
		public void Test_LARGE_BIG_EXT() {
			var enc1 = new byte[] { Constants.FORMAT_VERSION, Constants.LARGE_BIG_EXT,
				0, 0, 0, 1,
				0,
				1,
			};
			Assert.That(ETFCodec.Decode(enc1), Is.EqualTo(1));

			var enc2 = new byte[] { Constants.FORMAT_VERSION, Constants.LARGE_BIG_EXT,
				0, 0, 0, 8,
				0,
				255, 255, 255, 255,
				255, 255, 255, 255,
			};
			Assert.That(ETFCodec.Decode(enc2), Is.EqualTo(UInt64.MaxValue));

			var enc3 = new byte[] { Constants.FORMAT_VERSION, Constants.LARGE_BIG_EXT,
				0, 0, 0, 8,
				1,
				0, 0, 0, 0,
				0, 0, 0, 128,
			};
			Assert.That(ETFCodec.Decode(enc3), Is.EqualTo(Int64.MinValue));

			var enc4 = new byte[] { Constants.FORMAT_VERSION, Constants.LARGE_BIG_EXT,
				0, 0, 0, 8,
				1,
				1, 0, 0, 0,
				0, 0, 0, 128,
			};
			Assert.Throws<NotSupportedException> ( () => { ETFCodec.Decode(enc4); } );
		}

		object Roundtrip(object obj) {
			return ETFCodec.Decode(ETFCodec.Encode(obj));
		}

		public void TestRoundtrip(object obj) {
			TestRoundtrip(obj, obj == null ? null : obj.GetType());
		}

		public void TestRoundtrip(object obj, Type expectType) {
			var encoded1 = ETFCodec.Encode(obj);
			var encoded2 = ETFCodec.Encode(obj);
			var reencoded = ETFCodec.Decode(encoded1);
			Assert.That(reencoded, Is.EqualTo(obj));

			// verify that decoding doesn't alter the encoded buffer
			Assert.That(encoded1, Is.EqualTo(encoded2));

			// verify that types remain what we expect
			if(expectType != null) {
				Assert.That(reencoded, Is.InstanceOf(expectType));
			}
		}
	}
}
