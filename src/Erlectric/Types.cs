using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Erlectric {
	public class Atom : IComparable, IEquatable<Atom> {
		static Encoding latin1 = Encoding.GetEncoding("ISO-8859-1",
				new EncoderExceptionFallback(),
				new DecoderExceptionFallback());

		public readonly byte[] Name;

		public Atom(string name) {
			lock(latin1) {
				Name = latin1.GetBytes(name);
			}
		}

		internal Atom(byte[] bytes, int offset, int len) {
			Name = new byte[len];
			Buffer.BlockCopy(bytes, offset, Name, 0, len);
		}

		public override string ToString() {
			string name;
			lock(latin1) {
				name = latin1.GetString(Name);
			}
			return string.Format("Atom({0})", name);
		}

		public int CompareTo(object obj) {
			Atom other = obj as Atom;
			if(other == null) {
				return 1;
			} else {
				string thisName;
				string otherName;
				lock(latin1) {
					thisName = latin1.GetString(this.Name);
					otherName = latin1.GetString(other.Name);
				}
				return thisName.CompareTo(otherName);
			}
		}

		public bool Equals(Atom other) {
			if(other == null || this.Name.Length != other.Name.Length) {
				return false;
			} else {
				for(int i = 0; i < this.Name.Length; i++) {
					if(this.Name[i] != other.Name[i]) {
						return false;
					}
				}
			}
			return true;
		}

		public override bool Equals(object obj) {
			return this.Equals(obj as Atom);
		}

		public override int GetHashCode() {
			string name;
			lock(latin1) {
				name = latin1.GetString(Name);
			}
			return name.GetHashCode();
		}
	}

	public class ETFTuple : ArrayList {
		public ETFTuple() : base() {}
		public ETFTuple(ICollection coll) : base(coll) {}
		public ETFTuple(int len) : base(len) {}

		public override string ToString() {
			var s = "{}";
			if(Count > 0) {
				var sb = new StringBuilder("{ ");
				bool first = true;
				foreach(var e in this) {
					if(!first) {
						sb.Append(", ");
					}
					first = false;
					sb.Append(e == null ? "null" : e.ToString());
				}
				sb.Append(" }");
				s = sb.ToString();
			}
			return s;
		}
	}
}
