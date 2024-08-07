using System;
using System.Collections.Generic;
using System.Linq;
using OutSystems.Model.PatternRecognizer.PatternItems;
using OutSystems.Model.Tests.Shared.Metamodel;
using Pattern = OutSystems.Model.PatternRecognizer.PatternRecognizer<OutSystems.Model.Tests.Shared.Metamodel.IBaseObject, OutSystems.Model.PatternRecognizer.Tests.VirtualWidget, int>.Pattern;
using PatternParser = OutSystems.Model.PatternRecognizer.Tests.PatternParser<OutSystems.Model.Tests.Shared.Metamodel.IBaseObject, OutSystems.Model.PatternRecognizer.Tests.VirtualWidget, int>;
using Virtualizer = OutSystems.Model.PatternRecognizer.PatternRecognizer<OutSystems.Model.Tests.Shared.Metamodel.IBaseObject, OutSystems.Model.PatternRecognizer.Tests.VirtualWidget, int>;

namespace OutSystems.Model.PatternRecognizer.Tests;

internal partial class BasePatternRecognizerTest {

    public record TestCase(
        string Name,
        string StartSymbol,
        Func<IEnumerable<IScreen>>? ScreenConstructors,
        string Definitions,
        Dictionary<string, Virtualizer.PatternHandler>? Handlers = null,
        Dictionary<string, PropertyValueCondition>? Conditions = null,
        params string[] NonTerminals) {

        public override string ToString() => Name;

        public Pattern[] Patterns =>
            PatternParser.Parse(Definitions, Handlers, Conditions, NonTerminals).ToArray();
    }

    protected static IEnumerable<TestCase> TestCases {
        get {
            var textCondition = new PropertyValueCondition<string>(typeof(ITextWidget), "Text", ": ");

            ///////////////////////////////////////////////////////////////////////////////////////
            yield return new("Basic", "VirtualWidget",
                () => {
                    var screen = ModelFactory.CreateScreen();
                    var textWidget = screen.CreateWidget<ITextWidget>();
                    textWidget.Text = "Hello";

                    return new[] { screen };
                },
                """
                Pattern Text.1 : VirtualWidget
                    ITextWidget
                """);

            ///////////////////////////////////////////////////////////////////////////////////////
            yield return new("TextAndExpression", "VirtualWidget",
                () => {
                    var screen1 = ModelFactory.CreateScreen();
                    var textWidget = screen1.CreateWidget<ITextWidget>();
                    textWidget.Text = "Hello";

                    var screen2 = ModelFactory.CreateScreen();
                    textWidget = screen2.CreateWidget<ITextWidget>();
                    textWidget.Text = "Some field";
                    var exprWidget = screen2.CreateWidget<IExpressionWidget>();
                    exprWidget.Value = "123";

                    return new[] { screen1, screen2 };
                },
                """
                Pattern Text.1 : VirtualWidget
                    ITextWidget

                Pattern Expr.1 : VirtualWidget
                    ITextWidget
                    IExpressionWidget
                """);

            ///////////////////////////////////////////////////////////////////////////////////////
            yield return new("Capture", "VirtualWidget",
                () => {
                    var screen1 = ModelFactory.CreateScreen();
                    var expr = screen1.CreateWidget<IExpressionWidget>();
                    expr.Value = "000";

                    var screen2 = ModelFactory.CreateScreen();
                    var textWidget = screen2.CreateWidget<ITextWidget>();
                    textWidget.Text = "T2";
                    expr = screen2.CreateWidget<IExpressionWidget>();
                    expr.Value = "123";

                    var screen3 = ModelFactory.CreateScreen();
                    textWidget = screen3.CreateWidget<ITextWidget>();
                    textWidget.Text = "T3";
                    textWidget = screen3.CreateWidget<ITextWidget>();
                    textWidget.Text = ": ";
                    expr = screen3.CreateWidget<IExpressionWidget>();
                    expr.Value = "456";

                    var screen4 = ModelFactory.CreateScreen();
                    var labelWidget = screen4.CreateWidget<ILabelWidget>();
                    textWidget = labelWidget.Content.CreateWidget<ITextWidget>();
                    textWidget.Text = "T4";
                    expr = screen4.CreateWidget<IExpressionWidget>();
                    expr.Value = "789";

                    return new[] { screen1, screen2, screen3, screen4 };
                },
                """
                Pattern Expression.1 : VirtualWidget
                    w1=IExpressionWidget

                Pattern Expression.2 : VirtualWidget
                    w1=ITextWidget
                    w2=IExpressionWidget

                Pattern Expression.3 : VirtualWidget
                    w1=ITextWidget
                    ITextWidget [textCondition]
                    w2=IExpressionWidget

                Pattern Expression.4 : VirtualWidget
                    ILabelWidget
                    |   Content
                    |   |   w1=ITextWidget
                    w2=IExpressionWidget
                """,
                Conditions: new() {
                    { "textCondition", textCondition }
                });

            ///////////////////////////////////////////////////////////////////////////////////////
            yield return new("Repetition", "VirtualWidget",
                () => {
                    var screens = new List<IScreen>();
                    for (var i = 0; i <= 3; i++) {
                        var screen = ModelFactory.CreateScreen();
                        for (var j = 1; j <= i; j++) {
                            var textWidget = screen.CreateWidget<ITextWidget>();
                            textWidget.Text = $"Text #{j}";
                        }
                        screens.Add(screen);
                    }
                    return screens;
                },
                """
                Pattern Texts.1 : VirtualWidget
                    ITextWidget*
                """);


            ///////////////////////////////////////////////////////////////////////////////////////
            yield return new("RulesPriority", "Screen",
                () => {
                    var screens = new List<IScreen>();
                    for (var i = 1; i <= 3; i++) {
                        var screen = ModelFactory.CreateScreen();
                        for (var j = 1; j <= i; j++) {
                            var textWidget = screen.CreateWidget<ITextWidget>();
                            textWidget.Text = $"Text #{j}";
                        }
                        screens.Add(screen);
                    }
                    return screens;
                },
                """
                Pattern Screen! : Screen
                    VirtualWidget*
                
                Pattern VirtualWidget.1! : VirtualWidget
                    NativeWidget
                                
                Pattern Text.1 : VirtualWidget
                    ITextWidget
                
                Pattern Expression.1 : VirtualWidget
                    ITextWidget
                    ITextWidget
                    IExpressionWidget
                                
                Pattern Native.Text : NativeWidget
                    ITextWidget                
                """,
                Handlers: null,
                Conditions: null,
                "Screen", "VirtualWidget", "NativeWidget");

            ///////////////////////////////////////////////////////////////////////////////////////
            yield return new("MultipleWidgets", "Screen",
                () => {
                    var screens = new List<IScreen>();
                    for (var i = 0; i <= 3; i++) {
                        var screen = ModelFactory.CreateScreen();
                        for (var j = 1; j <= i; j++) {
                            var textWidget = screen.CreateWidget<ITextWidget>();
                            textWidget.Text = $"Text #{j}";
                        }
                        screens.Add(screen);
                    }
                    return screens;
                },
                """
                Pattern Screen! : Screen
                    VirtualWidget*

                Pattern Text.1 : VirtualWidget
                    ITextWidget
                """, 
                Handlers: null, Conditions: null, "VirtualWidget");

            ///////////////////////////////////////////////////////////////////////////////////////
            yield return new("SingleRecursion", "VirtualWidget",
                () => {
                    var screens = new List<IScreen>();

                    for (var i = 0; i <= 3; i++) {
                        var screen = ModelFactory.CreateScreen();
                        screens.Add(screen);

                        var list = screen.CreateWidget<IListWidget>();
                        for (var j = 0; j < i; j++) {
                            var text = list.ListItem.CreateWidget<ITextWidget>();
                            text.Text = $"T_{j}";
                        }
                    }

                    return screens;
                },
                """
                Pattern Text.1 : VirtualWidget
                    ITextWidget

                Pattern List.1 : VirtualWidget
                    IListWidget
                    |   ListItem
                    |   |   Child=VirtualWidget*
                """);

            ///////////////////////////////////////////////////////////////////////////////////////
            yield return new("RepeatedParts", "VirtualWidget",
                () => {
                    var screen = ModelFactory.CreateScreen();
                    var ifWidget = screen.CreateWidget<IIfWidget>();
                    ifWidget.TrueBranch.CreateWidget<ITextWidget>().Text = "Text in true branch";
                    ifWidget.FalseBranch.CreateWidget<ITextWidget>().Text = "Text #1 in false branch";
                    ifWidget.FalseBranch.CreateWidget<ITextWidget>().Text = "Text #2 in false branch";

                    return new[] { screen };
                },
                """
                Pattern Text.1 : VirtualWidget
                    ITextWidget

                Pattern If.1 : VirtualWidget
                    IIfWidget
                    |   TrueBranch
                    |   |   ChildTrue=VirtualWidget*
                    |   FalseBranch
                    |   |   ChildFalse=VirtualWidget*
                """);

            ///////////////////////////////////////////////////////////////////////////////////////
            yield return new("NonCapturedRepeatedParts", "VirtualWidget",
                () => {
                    var screen = ModelFactory.CreateScreen();
                    var ifWidget = screen.CreateWidget<IIfWidget>();
                    ifWidget.TrueBranch.CreateWidget<ITextWidget>().Text = "Text in true branch";
                    ifWidget.FalseBranch.CreateWidget<ITextWidget>().Text = "Text #1 in false branch";
                    ifWidget.FalseBranch.CreateWidget<ITextWidget>().Text = "Text #2 in false branch";

                    return new[] { screen };
                },
                """
                Pattern Text.1 : VirtualWidget
                    ITextWidget

                Pattern If.1 : VirtualWidget
                    IIfWidget
                    |   TrueBranch
                    |   |   VirtualWidget*
                    |   FalseBranch
                    |   |   VirtualWidget*
                """);

            ///////////////////////////////////////////////////////////////////////////////////////
            yield return new("Priority", "VirtualWidget",
                () => {
                    var screens = new List<IScreen>();

                    void CreateScreen(int numberOfWidgetsInHeader, Action<IHeaderCellWidget, int> createWidgetInHeaderCell) {
                        var screen = ModelFactory.CreateScreen();
                        var table = screen.CreateWidget<ITableWidget>();

                        var headerCell = table.HeaderRow.CreateWidget<IHeaderCellWidget>();
                        for (var j = 1; j <= numberOfWidgetsInHeader; j++) {
                            createWidgetInHeaderCell(headerCell, j);
                        }

                        var dataCell = table.DataRow.CreateWidget<IDataCellWidget>();
                        dataCell.Content.CreateWidget<IExpressionWidget>();

                        screens.Add(screen);
                    }

                    for (var i = 0; i <= 2; i++) {
                        CreateScreen(i, (h, i) => h.Content.CreateWidget<ITextWidget>().Text = $"Text #{i}");
                    }
                    for (var i = 0; i <= 2; i++) {
                        CreateScreen(i, (h, i) => h.Content.CreateWidget<IIconWidget>());
                    }

                    return screens;
                },
                """
                Pattern LabelWithFallback.1! : LabelWithFallback
                    LabelWidget

                Pattern LabelWithFallback.2! : LabelWithFallback
                    VirtualWidget

                Pattern Label.1 : LabelWidget
                    ITextWidget

                Pattern VirtualWidget.1! : VirtualWidget
                    NativeWidget

                Pattern Expr.1 : VirtualWidget
                    IExpressionWidget

                Pattern Expr.2 : VirtualWidget
                    ITextWidget
                    IExpressionWidget

                Pattern Table.1 : VirtualWidget
                    ITableWidget
                    |   HeaderRow
                    |   |   CHeader=IHeaderCellWidget*
                    |   |   |   Content
                    |   |   |   |   LabelWithFallback*
                    |   DataRow
                    |   |   CData=IDataCellWidget*
                    |   |   |   Content
                    |   |   |   |   VirtualWidget*

                Pattern Native.Text : NativeWidget
                    ITextWidget

                Pattern Native.Icon : NativeWidget
                    IIconWidget

                Pattern Native.Table : NativeWidget
                    ITableWidget
                    |   HeaderRow
                    |   |   IHeaderCellWidget*
                    |   |   |   Content
                    |   |   |   |   VirtualWidget*
                    |   DataRow
                    |   |   IDataCellWidget*
                    |   |   |   Content
                    |   |   |   |   VirtualWidget*
                """, 
                Handlers: new() {
                    { "Table.1",  (p, c, d, g) => CreateVirtualWidget(p, d, g, new[] { "CHeader", "CData" })}
                },
                Conditions: null,
                "LabelWidget", "VirtualWidget", "NativeWidget");

            ///////////////////////////////////////////////////////////////////////////////////////
            yield return new("DoubleRecursion", "VirtualWidget",
                () => {
                    var screens = new List<IScreen>();

                    for (var i = 0; i <= 3; i++) {
                        var screen = ModelFactory.CreateScreen();
                        screens.Add(screen);

                        var table = screen.CreateWidget<ITableWidget>();
                        for (var j = 0; j < i; j++) {
                            var headerCell = table.HeaderRow.CreateWidget<IHeaderCellWidget>();
                            var text = headerCell.Content.CreateWidget<ITextWidget>();
                            text.Text = $"T_{j}";

                            var dataCell = table.DataRow.CreateWidget<IDataCellWidget>();
                            var expr = dataCell.Content.CreateWidget<IExpressionWidget>();
                            expr.Value = $"{j}";
                        }
                    }

                    return screens;
                },
                """
                Pattern Text.1 : VirtualWidget
                    ITextWidget

                Pattern Expr.1 : VirtualWidget
                    IExpressionWidget

                Pattern Table.1 : VirtualWidget
                    ITableWidget
                    |   HeaderRow
                    |   |   CHeader=IHeaderCellWidget*
                    |   |   |   Content
                    |   |   |   |   VirtualWidget*
                    |   DataRow
                    |   |   CData=IDataCellWidget*
                    |   |   |   Content
                    |   |   |   |   VirtualWidget*
                """,
                Handlers: new() {
                    { "Table.1", (p, c, d, g) => CreateVirtualWidget(p, d, g, new[] { "CHeader", "CData" }) }
                });

            ///////////////////////////////////////////////////////////////////////////////////////
            yield return new("Advanced", "VirtualWidget",
                null,
                """
                Pattern LabelWidget! : VirtualWidget
                    LabelWidget

                Pattern DataWidget! : VirtualWidget
                    DataWidget

                Pattern Label.1 : LabelWidget
                    ITextWidget

                Pattern Label.2 : LabelWidget
                    ILabelWidget
                    |   Content
                    |   |   ITextWidget

                Pattern Expression.1 : DataWidget
                    IExpressionWidget

                Pattern Expression.2 : DataWidget
                    ITextWidget
                    IExpressionWidget

                Pattern Expression.3 : DataWidget
                    ITextWidget
                    ITextWidget [textCondition]
                    IExpressionWidget

                Pattern Expression.4 : DataWidget
                    ILabelWidget
                    |   content
                    |   |   ITextWidget
                    IExpressionWidget

                Pattern List.1 : VirtualWidget
                    IListWidget
                    |   ListItem
                    |   |   VirtualWidget*

                Pattern List.2 : VirtualWidget
                    ITextWidget
                    IListWidget
                    |   ListItem
                    |   |   VirtualWidget*

                Pattern Table.1 : VirtualWidget
                    HeaderRow
                    |   HeaderCell*
                    |   |   Content
                    |   |   |   LabelWidget
                    DataRow
                    |   RowCell*
                    |   |   Content
                    |   |   |   DataWidget

                Pattern Table.2 : VirtualWidget
                    ITextWidget
                    HeaderRow
                    |   HeaderCell*
                    |   |   Content
                    |   |   |   LabelWidget
                    DataRow
                    |   RowCell*
                    |   |   Content
                    |   |   |   DataWidget

                Pattern Table.3 : VirtualWidget
                    ITextWidget
                    HeaderRow
                    |   HeaderCell*
                    |   |   Content
                    |   |   |   VirtualWidget*
                    DataRow
                    |   RowCell*
                    |   |   Content
                    |   |   |   VirtualWidget*
                """,
                Handlers: null,
                Conditions: new() {
                    { "textCondition", textCondition }
                },
                "LabelWidget", "DataWidget");

            ///////////////////////////////////////////////////////////////////////////////////////
            yield return new("CapturedDoubleRecursion", "VirtualWidget",
                () => {
                    var screens = new List<IScreen>();

                    for (var i = 0; i <= 3; i++) {
                        var screen = ModelFactory.CreateScreen();
                        screens.Add(screen);

                        var table = screen.CreateWidget<ITableWidget>();
                        for (var j = 0; j < i; j++) {
                            var headerCell = table.HeaderRow.CreateWidget<IHeaderCellWidget>();
                            var text = headerCell.Content.CreateWidget<ITextWidget>();
                            text.Text = $"T_{j}";

                            var dataCell = table.DataRow.CreateWidget<IDataCellWidget>();
                            var expr = dataCell.Content.CreateWidget<IExpressionWidget>();
                            expr.Value = $"{j}";
                        }
                    }

                    return screens;
                },
                """
                Pattern Text.1 : VirtualWidget
                    ITextWidget

                Pattern Expr.1 : VirtualWidget
                    IExpressionWidget

                Pattern Table.1 : VirtualWidget
                    w1=ITableWidget
                    |   w2=HeaderRow
                    |   |   w3=IHeaderCellWidget*
                    |   |   |   w4=Content
                    |   |   |   |   CHeader=VirtualWidget*
                    |   w5=DataRow
                    |   |   w6=IDataCellWidget*
                    |   |   |   w7=Content
                    |   |   |   |   CData=VirtualWidget*
                """,
                Handlers: new() {
                    { "Table.1", (p, c, d, g) => CreateVirtualWidget(p, d, g, new[] { "CHeader", "CData" }) }
                });
        }
    }

    protected static VirtualWidget? CreateVirtualWidget(
        Pattern pattern,
        Virtualizer.ItemData[] topLevelItemsData,
        Func<string, Virtualizer.ItemData> getItemData,
        string[] childrenToZip) {

        var nativeWidgets = topLevelItemsData.SelectMany(i => i switch {
            Virtualizer.ModelItemData m => new[] { m.Object },
            Virtualizer.ListData l => l.Items.OfType<Virtualizer.ModelItemData>().Select(i => i.Object),
            _ => Enumerable.Empty<IBaseObject>()
        }).Cast<IWidget>().ToArray();

        var captures = pattern.AllItems.
            Select(i => i.CaptureName).
            OfType<string>();

        VirtualWidget[] children;
        if (childrenToZip.Length == 2) {
            // in all tests that supply a childrenToZip value, the children are inside a double recursion,
            // which means that for each provided child we get a ListData, which itselt then will contain
            // another ListData
            var childrenLists = childrenToZip.Select(n => getItemData(n)).
                Cast<Virtualizer.ListData>().Select(l => l.Items.OfType<Virtualizer.ListData>().ToList()).ToList();
            if (childrenLists[0].Count != childrenLists[1].Count) {
                // TODO: what should we do? Is it enough to give up and try other virtualization rule?
                return null;
            }

            children = childrenLists[0].Zip(childrenLists[1]).Select(c => {
                var subchildren = GetVirtualWidgets(c.First).Concat(GetVirtualWidgets(c.Second)).ToArray();
                return new VirtualWidget("Group", Array.Empty<IWidget>(), subchildren);
            }).ToArray();
        } else {
            // in the tests, by convention the children are the non-terminals with a capture name starting with "C"
            children = captures.Where(n => n.StartsWith("C")).
                SelectMany(n => GetVirtualWidgets(getItemData(n))).ToArray();
        }

        var capturesData = captures.Select(n => (n, (object)getItemData(n).ToString())).ToArray();

        return new VirtualWidget(pattern.Name, nativeWidgets, children, capturesData);
    }

    private static IEnumerable<VirtualWidget> GetVirtualWidgets(Virtualizer.ItemData item) => item switch {
        Virtualizer.MatchedPatternData p => new[] { p.Result },
        Virtualizer.ListData l => l.Items.OfType<Virtualizer.MatchedPatternData>().Select(p => p.Result),
        _ => Enumerable.Empty<VirtualWidget>()
    };
}
