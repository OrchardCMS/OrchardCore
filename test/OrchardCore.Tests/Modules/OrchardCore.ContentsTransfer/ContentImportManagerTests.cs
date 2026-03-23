using System.Data;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentsTransfer;
using OrchardCore.ContentsTransfer.Handlers;
using OrchardCore.ContentsTransfer.Handlers.Fields;
using OrchardCore.ContentsTransfer.Services;
using OrchardCore.Html.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.ContentsTransfer;

public class ContentImportManagerTests
{
    private readonly Mock<IContentImportHandlerResolver> _handlerResolver;
    private readonly Mock<ITypeActivatorFactory<ContentPart>> _partFactory;
    private readonly Mock<ITypeActivatorFactory<ContentField>> _fieldFactory;
    private readonly Mock<IContentManager> _contentManager;
    private readonly Mock<ILogger<ContentImportManager>> _logger;
    private readonly ContentImportManager _manager;

    public ContentImportManagerTests()
    {
        _handlerResolver = new Mock<IContentImportHandlerResolver>();
        _partFactory = new Mock<ITypeActivatorFactory<ContentPart>>();
        _fieldFactory = new Mock<ITypeActivatorFactory<ContentField>>();
        _contentManager = new Mock<IContentManager>();
        _logger = new Mock<ILogger<ContentImportManager>>();

        _handlerResolver.Setup(x => x.GetPartHandlers(It.IsAny<string>()))
            .Returns([]);
        _handlerResolver.Setup(x => x.GetFieldHandlers(It.IsAny<string>()))
            .Returns([]);

        _manager = new ContentImportManager(
            _handlerResolver.Object,
            _partFactory.Object,
            _fieldFactory.Object,
            [],
            _contentManager.Object,
            _logger.Object);
    }

    [Fact]
    public async Task GetColumnsAsync_WithNoParts_ReturnsEmpty()
    {
        var contentTypeDefinition = new ContentTypeDefinition("TestType", "Test Type");

        _contentManager.Setup(x => x.NewAsync(It.IsAny<string>()))
            .ReturnsAsync(new ContentItem { ContentType = "TestType" });

        var context = new ImportContentContext
        {
            ContentItem = new ContentItem { ContentType = "TestType" },
            ContentTypeDefinition = contentTypeDefinition,
        };

        var columns = await _manager.GetColumnsAsync(context);

        Assert.NotNull(columns);
        Assert.Empty(columns);
    }

    [Fact]
    public async Task GetColumnsAsync_WithContentImportHandler_ReturnsHandlerColumns()
    {
        var handler = new Mock<IContentImportHandler>();
        handler.Setup(x => x.GetColumns(It.IsAny<ImportContentContext>()))
            .Returns(new[] { new ImportColumn { Name = "TestColumn" } });

        var manager = new ContentImportManager(
            _handlerResolver.Object,
            _partFactory.Object,
            _fieldFactory.Object,
            [handler.Object],
            _contentManager.Object,
            _logger.Object);

        _contentManager.Setup(x => x.NewAsync(It.IsAny<string>()))
            .ReturnsAsync(new ContentItem { ContentType = "TestType" });

        var context = new ImportContentContext
        {
            ContentItem = new ContentItem { ContentType = "TestType" },
            ContentTypeDefinition = new ContentTypeDefinition("TestType", "Test Type"),
        };

        var columns = await manager.GetColumnsAsync(context);

        Assert.Single(columns);
        Assert.Equal("TestColumn", columns.First().Name);
    }

    [Fact]
    public async Task ImportAsync_InvokesContentImportHandlers()
    {
        var handler = new Mock<IContentImportHandler>();
        handler.Setup(x => x.ImportAsync(It.IsAny<ContentImportContext>()))
            .Returns(Task.CompletedTask);

        var manager = new ContentImportManager(
            _handlerResolver.Object,
            _partFactory.Object,
            _fieldFactory.Object,
            [handler.Object],
            _contentManager.Object,
            _logger.Object);

        var dataTable = new DataTable();
        dataTable.Columns.Add("Title");
        var row = dataTable.NewRow();
        row["Title"] = "Test Title";
        dataTable.Rows.Add(row);

        var context = new ContentImportContext
        {
            ContentItem = new ContentItem { ContentType = "TestType" },
            ContentTypeDefinition = new ContentTypeDefinition("TestType", "Test Type"),
            Columns = dataTable.Columns,
            Row = row,
        };

        await manager.ImportAsync(context);

        handler.Verify(x => x.ImportAsync(It.IsAny<ContentImportContext>()), Times.Once);
    }

    [Fact]
    public async Task ExportAsync_InvokesContentExportHandlers()
    {
        var handler = new Mock<IContentImportHandler>();
        handler.Setup(x => x.ExportAsync(It.IsAny<ContentExportContext>()))
            .Returns(Task.CompletedTask);

        var manager = new ContentImportManager(
            _handlerResolver.Object,
            _partFactory.Object,
            _fieldFactory.Object,
            [handler.Object],
            _contentManager.Object,
            _logger.Object);

        var dataTable = new DataTable();
        dataTable.Columns.Add("Title");
        var row = dataTable.NewRow();

        var context = new ContentExportContext
        {
            ContentItem = new ContentItem { ContentType = "TestType" },
            ContentTypeDefinition = new ContentTypeDefinition("TestType", "Test Type"),
            Row = row,
        };

        await manager.ExportAsync(context);

        handler.Verify(x => x.ExportAsync(It.IsAny<ContentExportContext>()), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_ThrowsOnNullContext()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _manager.ImportAsync(null));
    }

    [Fact]
    public async Task ExportAsync_ThrowsOnNullContext()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _manager.ExportAsync(null));
    }

    [Fact]
    public async Task GetColumnsAsync_ThrowsOnNullContext()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _manager.GetColumnsAsync(null));
    }
}

public class CommonContentImportHandlerTests
{
    private readonly CommonContentImportHandler _handler;
    private readonly Mock<IContentItemIdGenerator> _idGenerator;

    public CommonContentImportHandlerTests()
    {
        _idGenerator = new Mock<IContentItemIdGenerator>();
        _idGenerator.Setup(x => x.GenerateUniqueId(It.IsAny<ContentItem>()))
            .Returns("abcdefghijklmnopqrstuvwxyz");

        _handler = new CommonContentImportHandler(
            Mock.Of<IStringLocalizer<TitlePartContentImportHandler>>(),
            _idGenerator.Object);
    }

    [Fact]
    public void GetColumns_ReturnsContentItemIdColumn()
    {
        var context = new ImportContentContext
        {
            ContentItem = new ContentItem(),
            ContentTypeDefinition = new ContentTypeDefinition("Test", "Test"),
        };

        var columns = _handler.GetColumns(context);

        Assert.Contains(columns, c => c.Name == nameof(ContentItem.ContentItemId));
    }

    [Fact]
    public void GetColumns_ReturnsCreatedUtcAsExportOnly()
    {
        var context = new ImportContentContext
        {
            ContentItem = new ContentItem(),
            ContentTypeDefinition = new ContentTypeDefinition("Test", "Test"),
        };

        var columns = _handler.GetColumns(context);

        var createdUtcCol = columns.FirstOrDefault(c => c.Name == nameof(ContentItem.CreatedUtc));
        Assert.NotNull(createdUtcCol);
        Assert.Equal(ImportColumnType.ExportOnly, createdUtcCol.Type);
    }

    [Fact]
    public void GetColumns_ReturnsModifiedUtcAsExportOnly()
    {
        var context = new ImportContentContext
        {
            ContentItem = new ContentItem(),
            ContentTypeDefinition = new ContentTypeDefinition("Test", "Test"),
        };

        var columns = _handler.GetColumns(context);

        var modifiedUtcCol = columns.FirstOrDefault(c => c.Name == nameof(ContentItem.ModifiedUtc));
        Assert.NotNull(modifiedUtcCol);
        Assert.Equal(ImportColumnType.ExportOnly, modifiedUtcCol.Type);
    }

    [Fact]
    public async Task ImportAsync_SetsContentItemId_WhenValidLength()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add(nameof(ContentItem.ContentItemId));
        var row = dataTable.NewRow();
        row[nameof(ContentItem.ContentItemId)] = "abcdefghijklmnopqrstuvwxyz";
        dataTable.Rows.Add(row);

        var contentItem = new ContentItem();

        var context = new ContentImportContext
        {
            ContentItem = contentItem,
            ContentTypeDefinition = new ContentTypeDefinition("Test", "Test"),
            Columns = dataTable.Columns,
            Row = row,
        };

        await _handler.ImportAsync(context);

        Assert.Equal("abcdefghijklmnopqrstuvwxyz", contentItem.ContentItemId);
    }

    [Fact]
    public async Task ImportAsync_DoesNotSetContentItemId_WhenEmpty()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add(nameof(ContentItem.ContentItemId));
        var row = dataTable.NewRow();
        row[nameof(ContentItem.ContentItemId)] = "";
        dataTable.Rows.Add(row);

        var contentItem = new ContentItem();

        var context = new ContentImportContext
        {
            ContentItem = contentItem,
            ContentTypeDefinition = new ContentTypeDefinition("Test", "Test"),
            Columns = dataTable.Columns,
            Row = row,
        };

        await _handler.ImportAsync(context);

        Assert.Null(contentItem.ContentItemId);
    }

    [Fact]
    public async Task ImportAsync_DoesNotSetContentItemId_WhenWrongLength()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add(nameof(ContentItem.ContentItemId));
        var row = dataTable.NewRow();
        row[nameof(ContentItem.ContentItemId)] = "short";
        dataTable.Rows.Add(row);

        var contentItem = new ContentItem();

        var context = new ContentImportContext
        {
            ContentItem = contentItem,
            ContentTypeDefinition = new ContentTypeDefinition("Test", "Test"),
            Columns = dataTable.Columns,
            Row = row,
        };

        await _handler.ImportAsync(context);

        Assert.Null(contentItem.ContentItemId);
    }

    [Fact]
    public async Task ExportAsync_SetsContentItemIdAndDates()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add(nameof(ContentItem.ContentItemId));
        dataTable.Columns.Add(nameof(ContentItem.CreatedUtc));
        dataTable.Columns.Add(nameof(ContentItem.ModifiedUtc));
        var row = dataTable.NewRow();

        var contentItem = new ContentItem
        {
            ContentItemId = "test-id-123456789012345",
            CreatedUtc = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            ModifiedUtc = new DateTime(2024, 6, 20, 14, 45, 0, DateTimeKind.Utc),
        };

        var context = new ContentExportContext
        {
            ContentItem = contentItem,
            ContentTypeDefinition = new ContentTypeDefinition("Test", "Test"),
            Row = row,
        };

        await _handler.ExportAsync(context);

        Assert.Equal("test-id-123456789012345", row[nameof(ContentItem.ContentItemId)].ToString());
        Assert.Equal(contentItem.CreatedUtc?.ToString(), row[nameof(ContentItem.CreatedUtc)]?.ToString());
        Assert.Equal(contentItem.ModifiedUtc?.ToString(), row[nameof(ContentItem.ModifiedUtc)]?.ToString());
    }

    [Fact]
    public async Task ImportAsync_ThrowsOnNullContext()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.ImportAsync(null));
    }

    [Fact]
    public async Task ExportAsync_ThrowsOnNullContext()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.ExportAsync(null));
    }
}

public class TextFieldImportHandlerTests
{
    private readonly TextFieldImportHandler _handler;

    public TextFieldImportHandlerTests()
    {
        _handler = new TextFieldImportHandler(Mock.Of<IStringLocalizer<TextFieldImportHandler>>());
    }

    [Fact]
    public void GetColumns_ReturnsSingleColumn()
    {
        var context = CreateFieldContext("MyPart", "MyField");

        var columns = _handler.GetColumns(context);

        Assert.Single(columns);
        Assert.Equal("MyPart_MyField_Text", columns.First().Name);
    }

    [Fact]
    public async Task ImportAsync_SetsTextFieldValue()
    {
        var part = new ContentPart();
        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_MyField_Text");
        var row = dataTable.NewRow();
        row["MyPart_MyField_Text"] = "Hello World";
        dataTable.Rows.Add(row);

        var context = CreateFieldImportContext("MyPart", "MyField", part, dataTable, row);

        await _handler.ImportAsync(context);

        var field = part.Get<TextField>("MyField");
        Assert.NotNull(field);
        Assert.Equal("Hello World", field.Text);
    }

    [Fact]
    public async Task ImportAsync_TrimsWhitespace()
    {
        var part = new ContentPart();
        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_MyField_Text");
        var row = dataTable.NewRow();
        row["MyPart_MyField_Text"] = "  Trimmed  ";
        dataTable.Rows.Add(row);

        var context = CreateFieldImportContext("MyPart", "MyField", part, dataTable, row);

        await _handler.ImportAsync(context);

        var field = part.Get<TextField>("MyField");
        Assert.Equal("Trimmed", field.Text);
    }

    [Fact]
    public async Task ExportAsync_ReturnsTextFieldValue()
    {
        var part = new ContentPart();
        part.Alter<TextField>("MyField", f => f.Text = "Exported Text");

        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_MyField_Text");
        var row = dataTable.NewRow();

        var context = CreateFieldExportContext("MyPart", "MyField", part, row);

        await _handler.ExportAsync(context);

        Assert.Equal("Exported Text", row["MyPart_MyField_Text"].ToString());
    }

    [Fact]
    public async Task ExportAsync_ReturnsNull_WhenFieldNotPresent()
    {
        var part = new ContentPart();

        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_MyField_Text");
        var row = dataTable.NewRow();

        var context = CreateFieldExportContext("MyPart", "MyField", part, row);

        await _handler.ExportAsync(context);

        Assert.True(row["MyPart_MyField_Text"] == null || row["MyPart_MyField_Text"] == DBNull.Value);
    }

    private static ImportContentFieldContext CreateFieldContext(string partName, string fieldName)
    {
        return new ImportContentFieldContext
        {
            PartName = partName,
            ContentPartFieldDefinition = CreateFieldDefinition(fieldName, nameof(TextField)),
            ContentPart = new ContentPart(),
            ContentItem = new ContentItem(),
        };
    }

    private static ContentFieldImportMapContext CreateFieldImportContext(
        string partName, string fieldName, ContentPart part, DataTable dataTable, DataRow row)
    {
        return new ContentFieldImportMapContext
        {
            PartName = partName,
            ContentPartFieldDefinition = CreateFieldDefinition(fieldName, nameof(TextField)),
            ContentPart = part,
            ContentItem = new ContentItem(),
            Columns = dataTable.Columns,
            Row = row,
        };
    }

    private static ContentFieldExportMapContext CreateFieldExportContext(
        string partName, string fieldName, ContentPart part, DataRow row)
    {
        return new ContentFieldExportMapContext
        {
            PartName = partName,
            ContentPartFieldDefinition = CreateFieldDefinition(fieldName, nameof(TextField)),
            ContentPart = part,
            ContentField = part.Get<TextField>(fieldName) ?? new TextField(),
            ContentItem = new ContentItem(),
            Row = row,
        };
    }

    private static ContentPartFieldDefinition CreateFieldDefinition(string fieldName, string fieldType)
    {
        return new ContentPartFieldDefinition(new ContentFieldDefinition(fieldType), fieldName, []);
    }
}

public class BooleanFieldImportHandlerTests
{
    private readonly BooleanFieldImportHandler _handler;

    public BooleanFieldImportHandlerTests()
    {
        _handler = new BooleanFieldImportHandler(Mock.Of<IStringLocalizer<BooleanFieldImportHandler>>());
    }

    [Theory]
    [InlineData("True", true)]
    [InlineData("true", true)]
    [InlineData("TRUE", true)]
    [InlineData("False", false)]
    [InlineData("false", false)]
    [InlineData("", false)]
    public async Task ImportAsync_ParsesBooleanCorrectly(string input, bool expected)
    {
        var part = new ContentPart();
        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_IsActive_Value");
        var row = dataTable.NewRow();
        row["MyPart_IsActive_Value"] = input;
        dataTable.Rows.Add(row);

        var context = new ContentFieldImportMapContext
        {
            PartName = "MyPart",
            ContentPartFieldDefinition = CreateFieldDefinition("IsActive", nameof(BooleanField)),
            ContentPart = part,
            ContentItem = new ContentItem(),
            Columns = dataTable.Columns,
            Row = row,
        };

        await _handler.ImportAsync(context);

        var field = part.Get<BooleanField>("IsActive");
        Assert.NotNull(field);
        Assert.Equal(expected, field.Value);
    }

    [Fact]
    public async Task ExportAsync_ReturnsBooleanValue()
    {
        var part = new ContentPart();
        part.Alter<BooleanField>("IsActive", f => f.Value = true);

        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_IsActive_Value");
        var row = dataTable.NewRow();

        var context = new ContentFieldExportMapContext
        {
            PartName = "MyPart",
            ContentPartFieldDefinition = CreateFieldDefinition("IsActive", nameof(BooleanField)),
            ContentPart = part,
            ContentField = part.Get<BooleanField>("IsActive"),
            ContentItem = new ContentItem(),
            Row = row,
        };

        await _handler.ExportAsync(context);

        Assert.Equal(true.ToString(), row["MyPart_IsActive_Value"]?.ToString());
    }

    [Fact]
    public void GetColumns_ReturnsValidValues()
    {
        var context = new ImportContentFieldContext
        {
            PartName = "MyPart",
            ContentPartFieldDefinition = CreateFieldDefinition("IsActive", nameof(BooleanField)),
            ContentPart = new ContentPart(),
            ContentItem = new ContentItem(),
        };

        var columns = _handler.GetColumns(context);

        var column = Assert.Single(columns);
        Assert.Contains("True", column.ValidValues);
        Assert.Contains("False", column.ValidValues);
    }

    private static ContentPartFieldDefinition CreateFieldDefinition(string fieldName, string fieldType)
    {
        return new ContentPartFieldDefinition(new ContentFieldDefinition(fieldType), fieldName, []);
    }
}

public class NumberFieldImportHandlerTests
{
    private readonly NumberFieldImportHandler _handler;

    public NumberFieldImportHandlerTests()
    {
        _handler = new NumberFieldImportHandler(Mock.Of<IStringLocalizer<NumberFieldImportHandler>>());
    }

    [Theory]
    [InlineData("42", 42)]
    [InlineData("3.14", 3.14)]
    [InlineData("-100", -100)]
    [InlineData("0", 0)]
    public async Task ImportAsync_ParsesNumberCorrectly(string input, double expected)
    {
        var part = new ContentPart();
        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_Price_Value");
        var row = dataTable.NewRow();
        row["MyPart_Price_Value"] = input;
        dataTable.Rows.Add(row);

        var context = new ContentFieldImportMapContext
        {
            PartName = "MyPart",
            ContentPartFieldDefinition = CreateFieldDefinition("Price", nameof(NumericField)),
            ContentPart = part,
            ContentItem = new ContentItem(),
            Columns = dataTable.Columns,
            Row = row,
        };

        await _handler.ImportAsync(context);

        var field = part.Get<NumericField>("Price");
        Assert.NotNull(field);
        Assert.Equal((decimal)expected, field.Value);
    }

    [Theory]
    [InlineData("not-a-number")]
    [InlineData("abc")]
    public async Task ImportAsync_DoesNotSetValue_ForInvalidInput(string input)
    {
        var part = new ContentPart();
        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_Price_Value");
        var row = dataTable.NewRow();
        row["MyPart_Price_Value"] = input;
        dataTable.Rows.Add(row);

        var context = new ContentFieldImportMapContext
        {
            PartName = "MyPart",
            ContentPartFieldDefinition = CreateFieldDefinition("Price", nameof(NumericField)),
            ContentPart = part,
            ContentItem = new ContentItem(),
            Columns = dataTable.Columns,
            Row = row,
        };

        await _handler.ImportAsync(context);

        var field = part.Get<NumericField>("Price");
        Assert.Null(field);
    }

    [Fact]
    public async Task ExportAsync_ReturnsNumericValue()
    {
        var part = new ContentPart();
        part.Alter<NumericField>("Price", f => f.Value = 99.99m);

        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_Price_Value");
        var row = dataTable.NewRow();

        var context = new ContentFieldExportMapContext
        {
            PartName = "MyPart",
            ContentPartFieldDefinition = CreateFieldDefinition("Price", nameof(NumericField)),
            ContentPart = part,
            ContentField = part.Get<NumericField>("Price"),
            ContentItem = new ContentItem(),
            Row = row,
        };

        await _handler.ExportAsync(context);

        Assert.Equal(99.99m.ToString(), row["MyPart_Price_Value"]?.ToString());
    }

    private static ContentPartFieldDefinition CreateFieldDefinition(string fieldName, string fieldType)
    {
        return new ContentPartFieldDefinition(new ContentFieldDefinition(fieldType), fieldName, []);
    }
}

public class DateFieldImportHandlerTests
{
    private readonly DateFieldImportHandler _handler;

    public DateFieldImportHandlerTests()
    {
        _handler = new DateFieldImportHandler(Mock.Of<IStringLocalizer<DateFieldImportHandler>>());
    }

    [Fact]
    public async Task ImportAsync_ParsesValidDate()
    {
        var part = new ContentPart();
        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_StartDate_Value");
        var row = dataTable.NewRow();
        row["MyPart_StartDate_Value"] = "2024-06-15";
        dataTable.Rows.Add(row);

        var context = new ContentFieldImportMapContext
        {
            PartName = "MyPart",
            ContentPartFieldDefinition = CreateFieldDefinition("StartDate", nameof(DateField)),
            ContentPart = part,
            ContentItem = new ContentItem(),
            Columns = dataTable.Columns,
            Row = row,
        };

        await _handler.ImportAsync(context);

        var field = part.Get<DateField>("StartDate");
        Assert.NotNull(field);
        Assert.Equal(new DateTime(2024, 6, 15), field.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-date")]
    public async Task ImportAsync_DoesNotSetValue_ForInvalidDate(string input)
    {
        var part = new ContentPart();
        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_StartDate_Value");
        var row = dataTable.NewRow();
        row["MyPart_StartDate_Value"] = input;
        dataTable.Rows.Add(row);

        var context = new ContentFieldImportMapContext
        {
            PartName = "MyPart",
            ContentPartFieldDefinition = CreateFieldDefinition("StartDate", nameof(DateField)),
            ContentPart = part,
            ContentItem = new ContentItem(),
            Columns = dataTable.Columns,
            Row = row,
        };

        await _handler.ImportAsync(context);

        var field = part.Get<DateField>("StartDate");
        Assert.Null(field);
    }

    [Fact]
    public async Task ExportAsync_ReturnsDateValue()
    {
        var part = new ContentPart();
        var date = new DateTime(2024, 1, 15);
        part.Alter<DateField>("StartDate", f => f.Value = date);

        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_StartDate_Value");
        var row = dataTable.NewRow();

        var context = new ContentFieldExportMapContext
        {
            PartName = "MyPart",
            ContentPartFieldDefinition = CreateFieldDefinition("StartDate", nameof(DateField)),
            ContentPart = part,
            ContentField = part.Get<DateField>("StartDate"),
            ContentItem = new ContentItem(),
            Row = row,
        };

        await _handler.ExportAsync(context);

        Assert.Equal(date.ToString(), row["MyPart_StartDate_Value"]?.ToString());
    }

    [Fact]
    public async Task ExportAsync_ReturnsNull_WhenFieldNotPresent()
    {
        var part = new ContentPart();

        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_StartDate_Value");
        var row = dataTable.NewRow();

        var context = new ContentFieldExportMapContext
        {
            PartName = "MyPart",
            ContentPartFieldDefinition = CreateFieldDefinition("StartDate", nameof(DateField)),
            ContentPart = part,
            ContentField = new DateField(),
            ContentItem = new ContentItem(),
            Row = row,
        };

        await _handler.ExportAsync(context);

        Assert.True(row["MyPart_StartDate_Value"] == null || row["MyPart_StartDate_Value"] == DBNull.Value);
    }

    private static ContentPartFieldDefinition CreateFieldDefinition(string fieldName, string fieldType)
    {
        return new ContentPartFieldDefinition(new ContentFieldDefinition(fieldType), fieldName, []);
    }
}

public class HtmlBodyPartContentImportHandlerTests
{
    private readonly HtmlBodyPartContentImportHandler _handler;

    public HtmlBodyPartContentImportHandlerTests()
    {
        _handler = new HtmlBodyPartContentImportHandler(Mock.Of<IStringLocalizer<HtmlBodyPartContentImportHandler>>());
    }

    [Fact]
    public void GetColumns_ReturnsSingleHtmlColumn()
    {
        var context = CreatePartContext("HtmlBodyPart");

        var columns = _handler.GetColumns(context);

        var column = Assert.Single(columns);
        Assert.Equal("HtmlBodyPart_Html", column.Name);
    }

    [Fact]
    public async Task ImportAsync_SetsHtmlOnPart()
    {
        var contentItem = new ContentItem { ContentType = "Article" };
        var dataTable = new DataTable();
        dataTable.Columns.Add("HtmlBodyPart_Html");
        var row = dataTable.NewRow();
        row["HtmlBodyPart_Html"] = "<p>Hello World</p>";
        dataTable.Rows.Add(row);

        var context = new ContentPartImportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("HtmlBodyPart"),
            ContentItem = contentItem,
            Columns = dataTable.Columns,
            Row = row,
        };

        await _handler.ImportAsync(context);

        var part = contentItem.As<HtmlBodyPart>();
        Assert.NotNull(part);
        Assert.Equal("<p>Hello World</p>", part.Html);
    }

    [Fact]
    public async Task ImportAsync_SetsEmptyString()
    {
        var contentItem = new ContentItem { ContentType = "Article" };
        var dataTable = new DataTable();
        dataTable.Columns.Add("HtmlBodyPart_Html");
        var row = dataTable.NewRow();
        row["HtmlBodyPart_Html"] = "";
        dataTable.Rows.Add(row);

        var context = new ContentPartImportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("HtmlBodyPart"),
            ContentItem = contentItem,
            Columns = dataTable.Columns,
            Row = row,
        };

        await _handler.ImportAsync(context);

        var part = contentItem.As<HtmlBodyPart>();
        Assert.NotNull(part);
        Assert.Equal(string.Empty, part.Html);
    }

    [Fact]
    public async Task ImportAsync_IgnoresUnmatchedColumn()
    {
        var contentItem = new ContentItem { ContentType = "Article" };
        var dataTable = new DataTable();
        dataTable.Columns.Add("WrongColumn");
        var row = dataTable.NewRow();
        row["WrongColumn"] = "<p>Should not be set</p>";
        dataTable.Rows.Add(row);

        var context = new ContentPartImportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("HtmlBodyPart"),
            ContentItem = contentItem,
            Columns = dataTable.Columns,
            Row = row,
        };

        await _handler.ImportAsync(context);

        var part = contentItem.As<HtmlBodyPart>();
        Assert.Null(part);
    }

    [Fact]
    public async Task ExportAsync_WritesHtmlToRow()
    {
        var contentItem = new ContentItem { ContentType = "Article" };
        contentItem.Alter<HtmlBodyPart>(p => p.Html = "<h1>Title</h1>");

        var context = CreatePartContext("HtmlBodyPart");
        _handler.GetColumns(context);

        var dataTable = new DataTable();
        dataTable.Columns.Add("HtmlBodyPart_Html");
        var row = dataTable.NewRow();

        var exportContext = new ContentPartExportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("HtmlBodyPart"),
            ContentPart = contentItem.As<HtmlBodyPart>(),
            ContentItem = contentItem,
            Row = row,
        };

        await _handler.ExportAsync(exportContext);

        Assert.Equal("<h1>Title</h1>", row["HtmlBodyPart_Html"]?.ToString());
    }

    [Fact]
    public async Task ExportAsync_WritesEmptyString_WhenHtmlIsNull()
    {
        var contentItem = new ContentItem { ContentType = "Article" };
        contentItem.Alter<HtmlBodyPart>(p => p.Html = null);

        var context = CreatePartContext("HtmlBodyPart");
        _handler.GetColumns(context);

        var dataTable = new DataTable();
        dataTable.Columns.Add("HtmlBodyPart_Html");
        var row = dataTable.NewRow();

        var exportContext = new ContentPartExportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("HtmlBodyPart"),
            ContentPart = contentItem.As<HtmlBodyPart>(),
            ContentItem = contentItem,
            Row = row,
        };

        await _handler.ExportAsync(exportContext);

        Assert.Equal(string.Empty, row["HtmlBodyPart_Html"]?.ToString());
    }

    private static ImportContentPartContext CreatePartContext(string partName)
    {
        return new ImportContentPartContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition(partName),
        };
    }

    private static ContentTypePartDefinition CreateTypePartDefinition(string partName)
    {
        var contentTypeDefinition = new ContentTypeDefinition("Article", "Article");
        var contentPartDefinition = new ContentPartDefinition(partName);

        return new ContentTypePartDefinition(partName, contentPartDefinition, [])
        {
            ContentTypeDefinition = contentTypeDefinition,
        };
    }
}

public class AutoroutePartContentImportHandlerTests
{
    private readonly AutoroutePartContentImportHandler _handler;

    public AutoroutePartContentImportHandlerTests()
    {
        _handler = new AutoroutePartContentImportHandler(Mock.Of<IStringLocalizer<AutoroutePartContentImportHandler>>());
    }

    [Fact]
    public void GetColumns_ReturnsSinglePathColumn()
    {
        var context = CreatePartContext("AutoroutePart");

        var columns = _handler.GetColumns(context);

        var column = Assert.Single(columns);
        Assert.Equal("AutoroutePart_Path", column.Name);
    }

    [Fact]
    public async Task ImportAsync_SetsPathOnPart()
    {
        var contentItem = new ContentItem { ContentType = "Article" };
        var dataTable = new DataTable();
        dataTable.Columns.Add("AutoroutePart_Path");
        var row = dataTable.NewRow();
        row["AutoroutePart_Path"] = "my-article-slug";
        dataTable.Rows.Add(row);

        var context = new ContentPartImportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("AutoroutePart"),
            ContentItem = contentItem,
            Columns = dataTable.Columns,
            Row = row,
        };

        await _handler.ImportAsync(context);

        var part = contentItem.As<AutoroutePart>();
        Assert.NotNull(part);
        Assert.Equal("my-article-slug", part.Path);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ImportAsync_IgnoresEmptyOrWhitespacePath(string input)
    {
        var contentItem = new ContentItem { ContentType = "Article" };
        var dataTable = new DataTable();
        dataTable.Columns.Add("AutoroutePart_Path");
        var row = dataTable.NewRow();
        row["AutoroutePart_Path"] = input;
        dataTable.Rows.Add(row);

        var context = new ContentPartImportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("AutoroutePart"),
            ContentItem = contentItem,
            Columns = dataTable.Columns,
            Row = row,
        };

        await _handler.ImportAsync(context);

        var part = contentItem.As<AutoroutePart>();
        Assert.Null(part);
    }

    [Fact]
    public async Task ImportAsync_IgnoresUnmatchedColumn()
    {
        var contentItem = new ContentItem { ContentType = "Article" };
        var dataTable = new DataTable();
        dataTable.Columns.Add("WrongColumn");
        var row = dataTable.NewRow();
        row["WrongColumn"] = "some-path";
        dataTable.Rows.Add(row);

        var context = new ContentPartImportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("AutoroutePart"),
            ContentItem = contentItem,
            Columns = dataTable.Columns,
            Row = row,
        };

        await _handler.ImportAsync(context);

        var part = contentItem.As<AutoroutePart>();
        Assert.Null(part);
    }

    [Fact]
    public async Task ExportAsync_WritesPathToRow()
    {
        var contentItem = new ContentItem { ContentType = "Article" };
        contentItem.Alter<AutoroutePart>(p => p.Path = "exported-article");

        var context = CreatePartContext("AutoroutePart");
        _handler.GetColumns(context);

        var dataTable = new DataTable();
        dataTable.Columns.Add("AutoroutePart_Path");
        var row = dataTable.NewRow();

        var exportContext = new ContentPartExportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("AutoroutePart"),
            ContentPart = contentItem.As<AutoroutePart>(),
            ContentItem = contentItem,
            Row = row,
        };

        await _handler.ExportAsync(exportContext);

        Assert.Equal("exported-article", row["AutoroutePart_Path"]?.ToString());
    }

    [Fact]
    public async Task ExportAsync_WritesEmptyString_WhenPathIsNull()
    {
        var contentItem = new ContentItem { ContentType = "Article" };
        contentItem.Alter<AutoroutePart>(p => p.Path = null);

        var context = CreatePartContext("AutoroutePart");
        _handler.GetColumns(context);

        var dataTable = new DataTable();
        dataTable.Columns.Add("AutoroutePart_Path");
        var row = dataTable.NewRow();

        var exportContext = new ContentPartExportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("AutoroutePart"),
            ContentPart = contentItem.As<AutoroutePart>(),
            ContentItem = contentItem,
            Row = row,
        };

        await _handler.ExportAsync(exportContext);

        Assert.Equal(string.Empty, row["AutoroutePart_Path"]?.ToString());
    }

    private static ImportContentPartContext CreatePartContext(string partName)
    {
        return new ImportContentPartContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition(partName),
        };
    }

    private static ContentTypePartDefinition CreateTypePartDefinition(string partName)
    {
        var contentTypeDefinition = new ContentTypeDefinition("Article", "Article");
        var contentPartDefinition = new ContentPartDefinition(partName);

        return new ContentTypePartDefinition(partName, contentPartDefinition, [])
        {
            ContentTypeDefinition = contentTypeDefinition,
        };
    }
}
