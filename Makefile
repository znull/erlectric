BUILD_CONFIG = Debug
ERLECTRIC_TEST_DLL = tests/Erlectric.Tests/bin/$(BUILD_CONFIG)/Erlectric.Tests.dll
NUNIT_OPTS = 
BUILD_OPTS = 

all: packages build test rel
	
build: packages
	xbuild src/Erlectric/Erlectric.csproj
	xbuild tests/Erlectric.Tests/Erlectric.Tests.csproj

test: build
	nunit3-console $(ERLECTRIC_TEST_DLL) $(NUNIT_OPTS)

packages:
	nuget restore -ConfigFile nuget.config

rel: all
	nuget pack Erlectric.nuspec 

clean:
	rm -rf src/*/bin
	rm -rf src/*/obj
	rm -rf tests/*/bin
	rm -rf tests/*/obj
	rm -rf packages
