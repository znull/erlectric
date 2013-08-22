# erlectric

Erlectric is a partial implementation of erlang's [External Term Format][etf] for C#. It serves as the backend for csharp-bert, but may be used independently. The name and inspiration come from the excellent [erlastic] module for python.

Many erlang-native types have been omitted because erlectric is intended for use with a [bert] encoder/decoder, but there's no reason the remaining types can't be added.

[bert]: http://bert-rpc.org/
[etf]: http://erlang.org/doc/apps/erts/erl_ext_dist.html
[erlastic]: https://github.com/samuel/python-erlastic

## string encoding

For compatibility with other non-erlang ETF libs (and common sense), C# strings are encoded to the ETF BINARY_EXT type, as a utf-8 encoded sequence of bytes.  The STRING_EXT type isn't used as you might expect because its ETF meaning is a list of bytes, which is only useful in erlang.  This means that a c# string becomes a byte[] after a round-trip through ETF. 
