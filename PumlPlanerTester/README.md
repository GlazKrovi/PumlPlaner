Pour générer les class utils de antlr4 :
cd .\PumlPlaner\
antlr4-tool -Dlanguage=CSharp -visitor .\Grammar\Pumlg.g4

Faut d'abord installer :
https://builds.dotnet.microsoft.com/dotnet/Runtime/8.0.19/dotnet-runtime-8.0.19-win-x64.exe
et
dotnet tool install --global Antlr4CodeGenerator.Tool

installer le dernier jdk en date, aller dans les var d'env du pc, ajouter le chemin du jdk au path et REDEMMARRER L4IDE (pas seulement le terminaazl !)

---

PumlgParser contient les règles de tokenisation. Exemple : si y a un token Class_declaration alors PumlgParser.Class_declarationContext


