PROJ=./LoremipsumSharp.Collections.sln

clean:
	rm -rf ./dist

build:clean
	dotnet build $(PROJ) -c Release

publish:build
	dotnet publish -c Release $(PROJ) 
	
pack:publish
	dotnet pack -c Release $(PROJ) -o ./dist

push:pack
	$(foreach file, $(wildcard ./dist/*.nupkg), dotnet nuget push $(file) -k $(NUGET_APIKEY) -s http://nuget.dev.yunexpress.com/v3/index.json;)
	rm -rf ./dist

test1:
	echo $(NUGET_APIKEY)