# Capgemini ADCenter Network Code Challenge
## Gene Code Processing

### introduction

This is a possible solution for the first Code Challenge by the ADCenter Network from Capgemini. 
This one is written in F# and not intended to be a reference solution nor a showcase of proper "best practices".

### The challenge

A detailed description of the Challenge [can be found here](doc/ADCenter_Network_Code_Challenge_Processing_mRNA_Sequences.pdf), while the rules [are detailed here](doc/Code_Challenge_Rules_of_Engagement.pdf).

### Installation .NET

This projects needs .NET 5.0+ in order to be compiled and run successfully. You can find instructions on how to install for you platform here:
https://dotnet.microsoft.com/download/dotnet/5.0

### Compile and run 

In the root of the solution directory the project can be build by running the command:

    dotnet build

All unit-tests can be run with:

    dotnet test

The executable can be run with

     dotnet run --project .\src\App\App.fsproj .\data\refMrna.fa.corrected.txt

Note that the sample file is _refMrna.fa.corrected.txt_ which is valid. The original file _refMrna.fa.txt_ can be used but contains (intentionally) coding errors

### Why F#

For Fun ("and Profit"?). F# is together with TypeScript and Rust my favourite programming language triplet _du-jour_. Although in case of F# that fondness has not been so short-lived. I´ve liked the language a lot from when i first encountered it (2008 I think).   

Good resources to start are the [F# home page](https://fsharp.org/), the [docs at Microsoft](https://docs.microsoft.com/en-us/dotnet/fsharp/)  and the phenomenally rich site [F# for Fun and Profit](https://fsharpforfunandprofit.com/).

### Build & Test status

![Main build and test](https://github.com/ivanderk/docs/actions/workflows/dotnet.yml/badge.svg)

### License 

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

Copyright 2021 Iwan van der Kleijn

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
