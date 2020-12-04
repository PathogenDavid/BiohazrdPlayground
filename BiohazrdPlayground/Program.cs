using Biohazrd;
using Biohazrd.CSharp;
using Biohazrd.OutputGeneration;
using Biohazrd.Transformation;
using Biohazrd.Transformation.Common;
using Biohazrd.Utilities;
using BiohazrdPlayground;
using System;
using System.Collections.Immutable;
using System.IO;

#pragma warning disable CS8321 // Local function is declared but never used

//Test<RemoveExplicitBitFieldPaddingFieldsTransformation>();
//Test<AddBaseVTableAliasTransformation>();
//Test<AddBaseVTableAliasTransformation>("AddBaseVTableAliasTransformation2.h");
//Test<ConstOverloadRenameTransformation>();
//Test<MakeEverythingPublicTransformation>();
//Test<RemoveRemainingTypedefsTransformation>(dumpTreeForOutputLibrary: true);
//Test<LiftAnonymousUnionFieldsTransformation>(doTypeReductionFirst: true);
//Test<CSharpBuiltinTypeTransformation>();
//TestWithFactory(() => new KludgeUnknownClangTypesIntoBuiltinTypesTransformation(true));
//Test<WrapNonBlittableTypesWhereNecessaryTransformation>(doTypeReductionFirst: true, customTransformations: l => new RemoveRemainingTypedefsTransformation().Transform(l));
//Test<DeduplicateNamesTransformation>();
//Test<MoveLooseDeclarationsIntoTypesTransformation>();
//Test<MoveLooseDeclarationsIntoTypesTransformation>(fileName: "AssociateAutomatically.h");
ShowTypes("TypeReferences.h");

void Test<TTransformation>(string? fileName = null, bool dumpTreeForOutputLibrary = false, bool doTypeReductionFirst = false, Func<TranslatedLibrary, TranslatedLibrary>? customTransformations = null)
    where TTransformation : RawTransformationBase, new()
    => TestWithFactory<TTransformation>(() => new TTransformation(), fileName, dumpTreeForOutputLibrary, doTypeReductionFirst, customTransformations);

void TestWithFactory<TTransformation>(Func<TTransformation> factory, string? fileName = null, bool dumpTreeForOutputLibrary = false, bool doTypeReductionFirst = false, Func<TranslatedLibrary, TranslatedLibrary>? customTransformations = null)
    where TTransformation : RawTransformationBase
{
    if (fileName is null)
    { fileName = $"{typeof(TTransformation).Name}.h"; }

    Console.WriteLine("==============================================================================");
    Console.WriteLine($"{typeof(TTransformation).Name} transforming {fileName}");
    Console.WriteLine("==============================================================================");

    TranslatedLibraryBuilder libraryBuilder = new();
    libraryBuilder.AddCommandLineArgument("--language=c++");
    libraryBuilder.AddCommandLineArgument("--std=c++17");
    //libraryBuilder.AddCommandLineArgument("--target=x86_64-pc-linux");

    libraryBuilder.AddFile($"TestHeaders/{fileName}");

    TranslatedLibrary library = libraryBuilder.Create();
    library = new BrokenDeclarationExtractor().Transform(library);
    library = new DllImportNameTransformation().Transform(library); // This just replaces TODO.dll with something more senisble.

    new LibraryPrinter(typeof(TTransformation)).Visit(library);

    Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
    ImmutableArray<TranslationDiagnostic> generationDiagnostics = GenerateOutputAndPrint(library, $"Output_{fileName}", out _);

    Console.WriteLine("-------------------------------- Diagnostics ---------------------------------");
    DiagnosticWriter diagnostics = new();
    diagnostics.AddFrom(library);
    diagnostics.AddCategory("Transformed Generation Diagnostics", generationDiagnostics);
    diagnostics.WriteOutDiagnostics(null, true);

    if (customTransformations is not null)
    { library = customTransformations.Invoke(library); }

    if (doTypeReductionFirst)
    {
        CSharpTypeReductionTransformation typeReductionTransformation = new();
        int iterations = 0;
        do
        {
            library = typeReductionTransformation.Transform(library);
            iterations++;
        } while (typeReductionTransformation.ConstantArrayTypesCreated > 0);

        library = new CSharpBuiltinTypeTransformation().Transform(library);
    }

    if (customTransformations is not null || doTypeReductionFirst)
    {
        Console.WriteLine();
        Console.WriteLine("----------------------------- Partial Transform ------------------------------");
        new LibraryPrinter(typeof(TTransformation)).Visit(library);
        Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        GenerateOutputAndPrint(library, $"Output_{fileName}_partiallyTransformed", out _);
    }

    Console.WriteLine();
    Console.WriteLine("--------------------------------- Transform ----------------------------------");

    library = factory().Transform(library);
    new LibraryPrinter(typeof(TTransformation)).Visit(library);

    Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
    ImmutableArray<TranslationDiagnostic> transformedGenerationDiagnostics = GenerateOutputAndPrint(library, $"Output_{fileName}_transformed", out TranslatedLibrary outputLibrary);

    if (dumpTreeForOutputLibrary)
    {
        Console.WriteLine("----------------------------------- Output -----------------------------------");
        new LibraryPrinter(typeof(TTransformation)).Visit(outputLibrary);
    }

    Console.WriteLine();
    Console.WriteLine("-------------------------------- Diagnostics ---------------------------------");
    DiagnosticWriter transformedDiagnostics = new();
    transformedDiagnostics.AddFrom(library);
    transformedDiagnostics.AddCategory("Transformed Generation Diagnostics", transformedGenerationDiagnostics);
    transformedDiagnostics.WriteOutDiagnostics(null, true);
}

void ShowTypes(string fileName)
{
    Console.WriteLine("==============================================================================");
    Console.WriteLine($"Showing types for {fileName}");
    Console.WriteLine("==============================================================================");

    TranslatedLibraryBuilder libraryBuilder = new();
    libraryBuilder.AddCommandLineArgument("--language=c++");
    libraryBuilder.AddCommandLineArgument("--std=c++17");
    //libraryBuilder.AddCommandLineArgument("--target=x86_64-pc-linux");

    libraryBuilder.AddFile($"TestHeaders/{fileName}");

    TranslatedLibrary library = libraryBuilder.Create();
    library = new BrokenDeclarationExtractor().Transform(library);
    library = new DllImportNameTransformation().Transform(library); // This just replaces TODO.dll with something more senisble.

    library = new RemoveRemainingTypedefsTransformation().Transform(library);

    CSharpTypeReductionTransformation typeReductionTransformation = new();
    int iterations = 0;
    do
    {
        library = typeReductionTransformation.Transform(library);
        iterations++;
    } while (typeReductionTransformation.ConstantArrayTypesCreated > 0);

    library = new CSharpBuiltinTypeTransformation().Transform(library);

    new LibraryPrinter(typeof(ShowTypesMarkerDummy)).Visit(library);

    Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
    ImmutableArray<TranslationDiagnostic> generationDiagnostics = GenerateOutputAndPrint(library, $"Output_{fileName}_transformed", out TranslatedLibrary outputLibrary);

    Console.WriteLine();
    Console.WriteLine("-------------------------------- Diagnostics ---------------------------------");
    DiagnosticWriter transformedDiagnostics = new();
    transformedDiagnostics.AddFrom(library);
    transformedDiagnostics.AddCategory("Generation Diagnostics", generationDiagnostics);
    transformedDiagnostics.WriteOutDiagnostics(null, true);
}

ImmutableArray<TranslationDiagnostic> GenerateOutputAndPrint(TranslatedLibrary library, string libraryDescription, out TranslatedLibrary actualLibraryUsed, bool dumpClangInfo = false)
{
    CSharpTypeReductionTransformation typeReductionTransformation = new();
    int iterations = 0;
    do
    {
        library = typeReductionTransformation.Transform(library);
        iterations++;
    } while (typeReductionTransformation.ConstantArrayTypesCreated > 0);

    library = new CSharpBuiltinTypeTransformation().Transform(library);

    library = new CSharpTranslationVerifier().Transform(library);

    actualLibraryUsed = library;

    using OutputSession outputSession = new()
    {
        AutoRenameConflictingFiles = false,
        BaseOutputDirectory = "temp"
    };

    outputSession.BaseOutputDirectory = Path.Combine("temp", libraryDescription);
    string outputFile = Path.Combine(outputSession.BaseOutputDirectory, "TranslatedLibrary.cs");
    if (File.Exists(outputFile))
    { File.Delete(outputFile); }

    ImmutableArray<TranslationDiagnostic> diagnostics = CSharpLibraryGenerator.Generate
    (
        CSharpGenerationOptions.Default with { DumpClangInfo = dumpClangInfo },
        outputSession,
        library,
        LibraryTranslationMode.OneFile
    );

    outputSession.Dispose();
    Console.WriteLine(File.ReadAllText(outputFile));

    return diagnostics;
}

class LibraryPrinter : DeclarationVisitor
{
    private string Indent = "";
    private Type TransformationType;

    public LibraryPrinter(Type transformationType)
        => TransformationType = transformationType;

    private void WriteLine(string s)
        => Console.WriteLine($"{Indent}{s}");

    private void VisitChildren(VisitorContext context, TranslatedDeclaration declaration)
    {
        string oldIndent = Indent;
        Indent += "    ";

        if (TransformationType == typeof(ShowTypesMarkerDummy))
        { VisitTypes(declaration); }

        base.VisitDeclaration(context, declaration);
        Indent = oldIndent;
    }

    private string GetBaseline(VisitorContext context, TranslatedDeclaration declaration)
    {
        string result = $"{declaration.GetType().Name} {declaration.Name}";

        if (TransformationType == typeof(MakeEverythingPublicTransformation))
        { result = $"{declaration.Accessibility} {result}"; }

        if (TransformationType == typeof(DeduplicateNamesTransformation) && declaration.IsUnnamed)
        { result += $" (IsUnnamed)"; }

        return result;
    }

    protected override void VisitDeclaration(VisitorContext context, TranslatedDeclaration declaration)
    {
        WriteLine(GetBaseline(context, declaration));
        VisitChildren(context, declaration);
    }

    protected override void VisitTypedef(VisitorContext context, TranslatedTypedef declaration)
    {
        WriteLine($"{GetBaseline(context, declaration)} -> {declaration.UnderlyingType}");
        VisitChildren(context, declaration);
    }

    protected override void VisitFunction(VisitorContext context, TranslatedFunction declaration)
    {
        if (TransformationType == typeof(ConstOverloadRenameTransformation))
        {
            WriteLine($"{GetBaseline(context, declaration)}{(declaration.IsConst ? " (IsConst)" : "")}");
            VisitChildren(context, declaration);
            return;
        }
        else if (TransformationType == typeof(RemoveRemainingTypedefsTransformation))
        {
            WriteLine($"{GetBaseline(context, declaration)} -> {declaration.ReturnType}");
            VisitChildren(context, declaration);
            return;
        }

        base.VisitFunction(context, declaration);
    }

    protected override void VisitField(VisitorContext context, TranslatedField declaration)
    {
        WriteLine($"{GetBaseline(context, declaration)} @ {declaration.Offset}");
        VisitChildren(context, declaration);
    }

    protected override void VisitBitField(VisitorContext context, TranslatedBitField declaration)
    {
        WriteLine($"{GetBaseline(context, declaration)} @ {declaration.Offset} (BitOffset = {declaration.BitOffset}, BitWidth = {declaration.BitWidth})");
        VisitChildren(context, declaration);
    }

    //TODO: It'd be nice if we had a TypeReferenceVisitor helper.
    private void VisitTypes(TranslatedDeclaration declaration)
    {
        switch (declaration)
        {
            case TranslatedEnum translatedEnum:
                VisitType(translatedEnum.UnderlyingType, nameof(translatedEnum.UnderlyingType));
                return;
            case TranslatedFunction function:
                VisitType(function.ReturnType, nameof(function.ReturnType));
                return;
            case TranslatedParameter parameter:
                VisitType(parameter.Type);
                return;
            case TranslatedStaticField staticField:
                VisitType(staticField.Type);
                return;
            case TranslatedBaseField baseField:
                VisitType(baseField.Type);
                return;
            case TranslatedNormalField normalField:
                VisitType(normalField.Type);
                return;
            case TranslatedTypedef typedef:
                VisitType(typedef.UnderlyingType, nameof(typedef.UnderlyingType));
                return;
            case TranslatedVTableEntry vTableEntry:
                VisitType(vTableEntry.Type);
                return;
        }
    }

    private void VisitType(TypeReference typeReference, string label = "Type")
    {
        string line = $"{label}: {typeReference.GetType().Name} {typeReference}";

        switch (typeReference)
        {
            case PointerTypeReference pointerType:
                if (pointerType.WasReference)
                { line += " (WasReference)"; }
                break;
            case FunctionPointerTypeReference functionPointerType:
                line += $" ({functionPointerType.CallingConvention})";
                break;
        }

        WriteLine(line);

        string oldIndent = Indent;
        Indent += "    ";
        switch (typeReference)
        {
            case FunctionPointerTypeReference functionPointer:
            {
                VisitType(functionPointer.ReturnType, nameof(functionPointer.ReturnType));

                for (int i = 0; i < functionPointer.ParameterTypes.Length; i++)
                { VisitType(functionPointer.ParameterTypes[0], $"{nameof(functionPointer.ParameterTypes)}[{i}]"); }
                break;
            }
            case PointerTypeReference pointerType:
                VisitType(pointerType.Inner, nameof(pointerType.Inner));
                break;
        }
        Indent = oldIndent;
    }
}

class ShowTypesMarkerDummy { }
