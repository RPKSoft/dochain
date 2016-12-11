# Quickstart

## Prerequisites
1. .NET Core https://www.microsoft.com/net/core
2. Solidity compiler to compile contracts: https://github.com/ethereum/solidity/releases (install manually and add path to PATH env variable)http://solidity.readthedocs.io/en/develop/index.html (or use Visual Studio Code with extension)
3. Ethereum client: https://geth.ethereum.org/downloads/ (install and add to PATH)
4. MsBuild (if you're on Windows you're good with Visual Studio 2015) to build Dochain.Contractshttps://github.com/Microsoft/msbuild
We want to use https://github.com/Nethereum/Nethereum

## Enlistment
	Start your favorite shell. 
	Cd to your sources directory (e.g. C:\git)
	git clone https://github.com/RPKSoft/dochain.git
	Start ethereum test network
	Start &lt;sources dir&gt;\dochain\testchain\startgeth.bat or startgeth.sh (depending on shell) and keep it running.

## Build, test and run from CLI
	Start Developer Command prompt for Visual Studio or whatever there is on your system allowing to run msbuild and dotnet.
	Cd &lt;sources dir&gt;/dochain/Dochain.Contracts
	msbuild Dochain.Contracts.solproj /fl
	Cd ../Dochain.Contracts.Tests
	dotnet restore
	dotnet build
	dotnet test
	(ethereum should be running while testing)
	Cd ../Dochain.Web
	dotnet restore
	dotnet build
	dotnet run
