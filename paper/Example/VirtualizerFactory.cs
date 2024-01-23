using Example.Definitions;
using Example.NativeMetamodel;
using OutSystems.Model.PatternRecognizer.PatternItems;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Virtualizer = OutSystems.Model.PatternRecognizer.PatternRecognizer<Example.NativeMetamodel.ModelObject, Example.VirtualMetamodel.ModelObject, Example.Definitions.VirtualizationContext>;
using static Example.Definitions.Config;

namespace Example;

public class VirtualizerFactory {

    private const string GetLabelFunction = "GetLabelFor";

    /// <summary>
    /// Virtual widget definitions.
    /// 
    /// In this example we are using hard-coded definitions. In reality, these objects are dynamically created from a definitions
    /// file that is fetched at runtime from the pattern repository.
    /// 
    /// The definitions below correspond to the paper's example.
    /// </summary>
    private static VirtualWidgetDefinition[] VirtualWidgetDefinitions = new[] {
        // text input
        new VirtualWidgetDefinition("TextInput", new Property[] { new("Expression", "Variable", null), new("string", "Label", $"{GetLabelFunction}(Variable)") }, new[] {
            new VirtualWidgetPattern(1, new PropertyBinding[] { new("Variable", "i.Variable"), new("Label", "t.Value") },
                new PatternItem[] {
                    new VirtualWidgetPatternItem<Label>(
                        new [] { new VirtualWidgetPatternItem<Text>(captureName: "t") }),
                    new VirtualWidgetPatternItem<TextArea>(captureName: "i")
                })
        }, HandleTextInput),

        // bool input
        new VirtualWidgetDefinition("BooleanInput", new Property[] { new("Expression", "Variable", null), new("string", "Label", $"{GetLabelFunction}(Variable)") }, new[] {
            new VirtualWidgetPattern(1, new PropertyBinding[] { new("Variable", "i.Variable"), new("Label", "t.Value") },
                new PatternItem[] {
                    new VirtualWidgetPatternItem<Label>(
                        new [] { new VirtualWidgetPatternItem<Text>(captureName: "t") }),
                    new VirtualWidgetPatternItem<Checkbox>(captureName : "i")
                }),

            new VirtualWidgetPattern(2, new PropertyBinding[] { new("Variable", "i.Variable"), new("Label", "t.Value") },
                new PatternItem[] {
                    new VirtualWidgetPatternItem<Label>(
                        new [] { new VirtualWidgetPatternItem<Text>(captureName: "t") }),
                    new VirtualWidgetPatternItem<Switch>(captureName : "i")
                }),

            new VirtualWidgetPattern(3, new PropertyBinding[] { new("Variable", "i.Variable"), new("Label", "t.Value") },
                new PatternItem[] {
                    new VirtualWidgetPatternItem<Label>(
                        new [] { new VirtualWidgetPatternItem<Text>(captureName: "t") }),
                    new VirtualWidgetPatternItem<ButtonGroup>(
                        new[] {
                            new VirtualWidgetPatternItem<ButtonGroupItem>(
                                new[] {
                                    new VirtualWidgetPatternItem<Text>(defaultValues: new[] { new PropertyValue("Value", "Yes") })
                                },
                                condition: new PropertyValueCondition<ButtonGroupItem, string>(i => i.Value, nameof(ButtonGroupItem.Value), "true")),
                            new VirtualWidgetPatternItem<ButtonGroupItem>(
                                new[] {
                                    new VirtualWidgetPatternItem<Text>(defaultValues: new[] { new PropertyValue("Value", "No") })
                                },
                                condition: new PropertyValueCondition<ButtonGroupItem, string>(i => i.Value, nameof(ButtonGroupItem.Value), "false"))
                        },
                        captureName: "i")
                }),

        }, HandleBooleanInput),

        // enum input
        new VirtualWidgetDefinition("EnumInput", new Property[] {new("Expression", "Variable", null), new("string", "Label", $"{GetLabelFunction}(Variable)") }, new[] {
            new VirtualWidgetPattern(1, new PropertyBinding[] { new("Variable", "i.Variable"), new("Label", "t.Value") },
                new PatternItem[] {
                    new VirtualWidgetPatternItem<Label>(
                        new [] { new VirtualWidgetPatternItem<Text>(captureName: "t") }),
                    new VirtualWidgetPatternItem<ButtonGroup>(
                        new[] {
                            new VirtualWidgetPatternItem<ButtonGroupItem>(
                                new[] {
                                    new VirtualWidgetPatternItem < Text > ()
                                },
                                repeats: true)
                        },
                        captureName: "i")
                })
        }, HandleEnumInput),
    };

    private static Lazy<IEnumerable<ParserPattern>> Patterns = new(() => {
        var basePatterns = new[] {
            new ParserPattern(ParserPatternKind.Widget, "", WidgetNonTerminal, new [] { new NonTerminalItem(VirtualWidgetNonTerminal) }, passThrough: true),
            new ParserPattern(ParserPatternKind.Widget, "", WidgetNonTerminal, new [] { new NonTerminalItem(NativeWidgetNonTerminal) }, passThrough: true)
        };

        var virtualWidgetPatterns = VirtualWidgetDefinitions.SelectMany(d => d.Convert());

        // native patterns. These are to be auto-generated from the native metamodel
        var nativeWidgetPatterns = new[] {
            new ParserPattern(ParserPatternKind.NativeWidget, "ButtonGroup", NativeWidgetNonTerminal, new[] {
                new ModelItem(nameof(ButtonGroup), new[] { new NonTerminalItem(WidgetNonTerminal, Quantifier.ZeroOrMore, nameof(ButtonGroup.Items)) })
            }, NativeWidgetHandler<VirtualMetamodel.ButtonGroup>),

            new ParserPattern(ParserPatternKind.NativeWidget, "ButtonGroupItem", NativeWidgetNonTerminal, new[] {
                new ModelItem(nameof(ButtonGroupItem), new[] { new NonTerminalItem(WidgetNonTerminal, Quantifier.ZeroOrMore, nameof(ButtonGroupItem.TextWidgets)) })
            }, NativeWidgetHandler<VirtualMetamodel.ButtonGroupItem>),

            new ParserPattern(ParserPatternKind.NativeWidget, "Checkbox", NativeWidgetNonTerminal, new[] { new ModelItem(nameof(Checkbox)) }, NativeWidgetHandler<VirtualMetamodel.Checkbox>),

            new ParserPattern(ParserPatternKind.NativeWidget, "Container", NativeWidgetNonTerminal, new[] {
                new ModelItem(nameof(Container), new[] { new NonTerminalItem(WidgetNonTerminal, Quantifier.ZeroOrMore, nameof(Container.Widgets)) })
            }, NativeWidgetHandler<VirtualMetamodel.Container>),

            new ParserPattern(ParserPatternKind.Form, "", "Form", new [] {
                new ModelItem(nameof(Form), new[] { new NonTerminalItem(WidgetNonTerminal, Quantifier.ZeroOrMore, nameof(Form.Widgets)) })
            }, NativeWidgetHandler<VirtualMetamodel.Form>),

            new ParserPattern(ParserPatternKind.NativeWidget, "Label", NativeWidgetNonTerminal, new[] {
                new ModelItem(nameof(Label), new[] { new NonTerminalItem(WidgetNonTerminal, Quantifier.ZeroOrMore, nameof(Label.Widgets)) })
            }, NativeWidgetHandler<VirtualMetamodel.Label>),

            new ParserPattern(ParserPatternKind.NativeWidget, "Switch", NativeWidgetNonTerminal, new[] { new ModelItem(nameof(Switch)) }, NativeWidgetHandler<VirtualMetamodel.Switch>),

            new ParserPattern(ParserPatternKind.NativeWidget, "Text", NativeWidgetNonTerminal, new[] { new ModelItem(nameof(Text)) }, NativeWidgetHandler<VirtualMetamodel.Text>),

            new ParserPattern(ParserPatternKind.NativeWidget, "TextArea", NativeWidgetNonTerminal, new[] { new ModelItem(nameof(TextArea)) }, NativeWidgetHandler<VirtualMetamodel.TextArea>),
        };

        return basePatterns.Concat(virtualWidgetPatterns).Concat(nativeWidgetPatterns).ToList();
    });

    private static VirtualMetamodel.ModelObject? DefaultPatternHandler(
        Virtualizer.Pattern pattern,
        VirtualizationContext context,
        Virtualizer.ItemData[] topLevelItemsData,
        Func<string, Virtualizer.ItemData> getCapturedData) {
        return null;
    }

    private static int GetId(string patternName) => int.Parse(patternName.Substring(patternName.IndexOf('.') + 1));

    // In our example, and for the sake of simplicity, we have defined specific classes for each virtual widget.
    // In reality the virtual widget classes are not known a priori - we dynamically fetch definitions from the patterns
    // repository - and have a single "VirtualWidget" class that is used to represent any virtual widget instance.
    // Consequently, instead of having separate "Handle<WidgetName>" handlers for each virtual widget we would
    // have a single "HandleVirtualWidget" method.

    private static VirtualMetamodel.ModelObject? HandleTextInput(
        Virtualizer.Pattern pattern,
        VirtualizationContext context,
        Virtualizer.ItemData[] topLevelItemsData,
        Func<string, Virtualizer.ItemData> getCapturedData) {
        var textData = (Virtualizer.ModelItemData)getCapturedData("t");
        var text = (Text)textData.Object;
        var inputData = (Virtualizer.ModelItemData)getCapturedData("i");
        var input = (Input)inputData.Object;
        return new VirtualMetamodel.TextInput(context.NextId, input.Variable, text.Value, GetId(pattern.Name));
    }

    private static VirtualMetamodel.ModelObject? HandleBooleanInput(
        Virtualizer.Pattern pattern,
        VirtualizationContext context,
        Virtualizer.ItemData[] topLevelItemsData,
        Func<string, Virtualizer.ItemData> getCapturedData) {
        var textData = (Virtualizer.ModelItemData)getCapturedData("t");
        var text = (Text)textData.Object;
        var inputData = (Virtualizer.ModelItemData)getCapturedData("i");
        var input = (Input)inputData.Object;
        return new VirtualMetamodel.BooleanInput(context.NextId, input.Variable, text.Value, GetId(pattern.Name));
    }

    private static VirtualMetamodel.ModelObject? HandleEnumInput(
        Virtualizer.Pattern pattern,
        VirtualizationContext context,
        Virtualizer.ItemData[] topLevelItemsData,
        Func<string, Virtualizer.ItemData> getCapturedData) {
        var textData = (Virtualizer.ModelItemData)getCapturedData("t");
        var text = (Text)textData.Object;
        var inputData = (Virtualizer.ModelItemData)getCapturedData("i");
        var input = (Input)inputData.Object;
        return new VirtualMetamodel.EnumInput(context.NextId, input.Variable, text.Value, GetId(pattern.Name));
    }

    private static TVirtual? NativeWidgetHandler<TVirtual>(
        Virtualizer.Pattern pattern,
        VirtualizationContext context,
        Virtualizer.ItemData[] topLevelItemsData,
        Func<string, Virtualizer.ItemData> getCapturedData)
        where TVirtual : VirtualMetamodel.ModelObject {
        var nativeWidget = ((Virtualizer.ModelItemData)topLevelItemsData[0]).Object;
        var nativeType = nativeWidget.GetType();

        object? GetParameterValue(ParameterInfo param) {
            if (param.Name == null) {
                return null;
            }

            if (param.Name == nameof(VirtualMetamodel.ModelObject.Id)) {
                return nativeWidget.Id;
            }

            // as described in the metamodel classes, by convention the array fields represent child elements,
            // and those are virtualized recursively
            if (param.ParameterType.IsArray) {
                var listItem = (Virtualizer.ListData)getCapturedData(param.Name);
                var childItems = listItem.Items.Cast<Virtualizer.MatchedPatternData>().Select(i => i.Result).ToArray();
                var arrayConstructor = param.ParameterType.GetConstructors().Single();
                var children = (Array)arrayConstructor.Invoke(new object[] { childItems.Length });
                childItems.CopyTo(children, 0);
                return children;
            }

            return nativeType.GetProperty(param.Name)!.GetValue(nativeWidget);
        }

        var constructor = typeof(TVirtual).GetConstructors().Single();
        var arguments = constructor.GetParameters().Select(GetParameterValue).ToArray();
        return (TVirtual)constructor.Invoke(arguments);
    }

    /// <summary>
    /// Creates a new virtualizer using the hard-coded definitions contained in this class.
    /// For the real use case we are fetching the definitions from the patterns repository.
    /// </summary>
    /// <param name="tracer"></param>
    /// <returns></returns>
    public static Virtualizer CreateVirtualizer(Tracer tracer) => new("Form", Patterns.Value, Tokenizer.Instance, DefaultPatternHandler, tracer);

    public static void PrintGrammar(StreamWriter fileWriter) {
        using var grammarWriter = new IndentedTextWriter(fileWriter);
        Patterns.Value.PrintGrammar(grammarWriter);
    }

    internal static void PrintPatterns(IndentedTextWriter writer) {
        Patterns.Value.Print(writer);
    }
}
