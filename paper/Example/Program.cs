using Example.Definitions;
using Example.NativeMetamodel;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Example;

public partial class Program {

    private static void PrintGrammar() {
        using var fileWriter = GetStreamWriter("grammar.txt");
        VirtualizerFactory.PrintGrammar(fileWriter);
    }

    private static void PrintModel(object model, string filename) {
        using var fileWriter = GetStreamWriter(filename);
        using var modelWriter = new IndentedTextWriter(fileWriter, "\t");
        model.Print(modelWriter);
    }

    private static void PrintParser(Tracer parserTracer) {
        File.WriteAllText(GetFileFullPath("parser.txt"), parserTracer.ToString());
    }

    private static StreamWriter GetStreamWriter(string filename) =>
        new StreamWriter(GetFileFullPath(filename));

    private static string GetFileFullPath(string filename) =>
        Path.Combine(GetOutputDirectory(), filename);

    private static string GetOutputDirectory([CallerFilePath] string filePath = "") =>
        Path.Combine(Path.GetDirectoryName(filePath)!, "output");

    private static Form CreateForm() {
        var ta = new TextArea("ta", "Request.Description");
        var t1 = new Text("t1", "Request description", "bold");
        var l1 = new Label("l1", ta, t1);
        var c1 = new Container("c1", l1, ta);

        var t3 = new Text("t3", "Yes");
        var g1 = new ButtonGroupItem("g1", "true", t3);

        var t4 = new Text("t4", "No");
        var g2 = new ButtonGroupItem("g2", "false", t4);

        var bg = new ButtonGroup("bg", "Request.Approved", g1, g2);
        var t2 = new Text("t2", "Approved", "bold");
        var l2 = new Label("l2", bg, t2);
        var c2 = new Container("c2", l2, bg);

        var form = new Form("f", c1, c2);

        return form;
    }

    public static void Main() {

        // In this example we create a "native" model (check CreateForm() function above) that
        // is then virtualized.
        //
        // The VirtualizerFactory class creates the virtualizer instance using, for simplicity,
        // a set of hard-coded patterns corresponding to the ones presented in the paper.
        // Note that in our actual use the patterns are dynamically fetched from the patterns repository.

        // When running the example several different files are created in the "output" folder
        // - model.xml: serialized representation of the native model setup by the CreateForm() function
        // - virtual-model.xml: serialized representation of the virtual model obtained by running the virtualizer
        //   on the native model
        // - grammar.txt: textual representation of the grammar corresponding to the virtual widget patterns
        // - parser.txt: detailed information about the generated parser and its execution on the input native model
        // - log.txt: additional information about the virtualization process

        using var logFile = new StreamWriter(GetFileFullPath("log.txt"));
        using var writer = new IndentedTextWriter(logFile);

        writer.WriteLine("-- Native model ---");
        var form = CreateForm();
        form.Print(writer);
        writer.WriteLine();
        PrintModel(form, "model.xml");

        writer.WriteLine("-- Patterns ---");
        VirtualizerFactory.PrintPatterns(writer);
        writer.WriteLine();
        PrintGrammar();

        var tracer = new Tracer();
        var virtualizer = VirtualizerFactory.CreateVirtualizer(tracer);

        writer.WriteLine("-- Virtualizer ---");
        var auxTracer = new Tracer();
        virtualizer.Print(auxTracer, fullDetails: false);
        writer.WriteLine(auxTracer);

        // the virtualization process occurs here: the call to virtualizer.Parse will return the virtual model
        // corresponding to the provided native model
        var context = new VirtualizationContext();
        var virtualForm = virtualizer.Parse(new[] { form }, context, CancellationToken.None)?.FirstOrDefault();
        PrintParser(tracer);

        writer.WriteLine("-- Virtual model ---");
        if (virtualForm == null) {
            writer.WriteLine("Parse failed (check parser.txt file):");
        } else {
            virtualForm.Print(writer);
            PrintModel(virtualForm, "virtual-model.xml");
        }
        writer.WriteLine();
    }
}