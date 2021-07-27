# Goblinfactory.JsonQuery

Cross platform dotnet tool for querying json files and first super hacky support for js files with embedded json. First language supported in JmesPath.

## install

## usage

> jsq {filename} {jmespath query expression} [outputfile]

example

> jsq sample.js "lists[].{sort:pos, inactive:closed }" lists.json
