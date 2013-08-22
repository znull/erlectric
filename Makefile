BUILD_CONFIG = Debug
ERLECTRIC_TEST_DLL = tests/Erlectric.Tests/bin/$(BUILD_CONFIG)/Erlectric.Tests.dll
NUNIT_OPTS = -noresult -nologo -stoponerror -labels

test:
	xbuild src/Erlectric/Erlectric.csproj
	xbuild tests/Erlectric.Tests/Erlectric.Tests.csproj

	nunit-console $(ERLECTRIC_TEST_DLL) $(NUNIT_OPTS)

clean:
	rm -rf src/*/bin
	rm -rf src/*/obj
	rm -rf tests/*/bin
	rm -rf tests/*/obj
