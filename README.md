# Pattern Recognizer
This repo contains a reference implementation for the Pattern Recognizer algorithm
presented in the Chameleon paper.

# Building
Run `dotnet build` in the root folder.

# Running tests
Run `dotnet test` in the root folder.

# Example
The [Example](./paper/Example) folder contains an example console app that
creates a virtualizer class using the paper's virtual widget definitions.

Run `dotnet run` in that folder to run the example.

# Source code

The source code is organized as follows:

## OutSystems.Model.Parser
The [Parser](./src/OutSystems.Model.Parser) project defines the [GeneralLRParser](./src/OutSystems.Model.Parser/GeneralLRParser.cs)
class which is a [GLR parser](https://en.wikipedia.org/wiki/GLR_parser). The parser
receives a set of low-level states and rules and provides a parse method receiving a token sequence.

## OutSystems.Model.ParserGenerator
The [ParserGenerator](./src/OutSystems.Model.ParserGenerator) project defines the 
[ParserGenerator](./src/OutSystems.Model.ParserGenerator/ParserGenerator.cs) class which allows to generate a 
[GeneralLRParser](./src/OutSystems.Model.Parser/GeneralLRParser.cs)
from a higher-level grammar specified as a set of rules.

## OutSystems.Model.PatternRecognizer
The [PatternRecognizer](./src/OutSystems.Model.PatternRecognizer) project defines the 
[PatternRecognizer](./src/OutSystems.Model.PatternRecognizer/PatternRecognizer.cs) class to create patter recognizers.
These are a specialization of the grammar expected by the [ParserGenerator](./src/OutSystems.Model.ParserGenerator/ParserGenerator.cs)
class that better fits or domain where inputs are models (i.e. trees of objects).


