using System.Data;
using OrchardCore.Alias.Models;
using OrchardCore.ArchiveLater.Models;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTransfer;
using OrchardCore.ContentTransfer.Handlers;
using OrchardCore.ContentTransfer.Handlers.Fields;
using OrchardCore.ContentTransfer.Services;
using OrchardCore.Html.Models;
using OrchardCore.Liquid.Models;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Models;
using OrchardCore.Media.Fields;
using OrchardCore.PublishLater.Models;
using OrchardCore.Taxonomies.Fields;

namespace OrchardCore.Tests.Modules.OrchardCore.ContentTransfer;

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

        Assert.True(contentItem.TryGet<HtmlBodyPart>(out var part));
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

        Assert.True(contentItem.TryGet<HtmlBodyPart>(out var part));
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

        Assert.False(contentItem.TryGet<HtmlBodyPart>(out _));
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
            ContentPart = contentItem.GetOrCreate<HtmlBodyPart>(),
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
            ContentPart = contentItem.GetOrCreate<HtmlBodyPart>(),
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

        Assert.True(contentItem.TryGet<AutoroutePart>(out var part));
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

        Assert.False(contentItem.TryGet<AutoroutePart>(out _));
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

        Assert.False(contentItem.TryGet<AutoroutePart>(out _));
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
            ContentPart = contentItem.GetOrCreate<AutoroutePart>(),
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
            ContentPart = contentItem.GetOrCreate<AutoroutePart>(),
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

public class MarkdownBodyPartContentImportHandlerTests
{
    private readonly MarkdownBodyPartContentImportHandler _handler;

    public MarkdownBodyPartContentImportHandlerTests()
    {
        _handler = new MarkdownBodyPartContentImportHandler(Mock.Of<IStringLocalizer<MarkdownBodyPartContentImportHandler>>());
    }

    [Fact]
    public void GetColumns_ReturnsSingleMarkdownColumn()
    {
        var context = CreatePartContext("MarkdownBodyPart");

        var columns = _handler.GetColumns(context);

        var column = Assert.Single(columns);
        Assert.Equal("MarkdownBodyPart_Markdown", column.Name);
    }

    [Fact]
    public async Task ImportAsync_SetsMarkdownOnPart()
    {
        var contentItem = new ContentItem { ContentType = "Article" };
        var dataTable = new DataTable();
        dataTable.Columns.Add("MarkdownBodyPart_Markdown");
        var row = dataTable.NewRow();
        row["MarkdownBodyPart_Markdown"] = "## Hello World";
        dataTable.Rows.Add(row);

        var context = new ContentPartImportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("MarkdownBodyPart"),
            ContentItem = contentItem,
            Columns = dataTable.Columns,
            Row = row,
        };

        await _handler.ImportAsync(context);

        Assert.True(contentItem.TryGet<MarkdownBodyPart>(out var part));
        Assert.Equal("## Hello World", part.Markdown);
    }

    [Fact]
    public async Task ExportAsync_WritesMarkdownToRow()
    {
        var contentItem = new ContentItem { ContentType = "Article" };
        contentItem.Alter<MarkdownBodyPart>(p => p.Markdown = "## Exported");

        _handler.GetColumns(CreatePartContext("MarkdownBodyPart"));

        var dataTable = new DataTable();
        dataTable.Columns.Add("MarkdownBodyPart_Markdown");
        var row = dataTable.NewRow();

        var exportContext = new ContentPartExportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("MarkdownBodyPart"),
            ContentPart = contentItem.GetOrCreate<MarkdownBodyPart>(),
            ContentItem = contentItem,
            Row = row,
        };

        await _handler.ExportAsync(exportContext);

        Assert.Equal("## Exported", row["MarkdownBodyPart_Markdown"]?.ToString());
    }

    private static ImportContentPartContext CreatePartContext(string partName)
        => new()
        {
            ContentTypePartDefinition = CreateTypePartDefinition(partName),
        };

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

public class AliasPartContentImportHandlerTests
{
    private readonly AliasPartContentImportHandler _handler;

    public AliasPartContentImportHandlerTests()
    {
        _handler = new AliasPartContentImportHandler(Mock.Of<IStringLocalizer<AliasPartContentImportHandler>>());
    }

    [Fact]
    public async Task ImportAsync_SetsAliasOnPart()
    {
        var contentItem = new ContentItem { ContentType = "Article" };
        var dataTable = new DataTable();
        dataTable.Columns.Add("AliasPart_Alias");
        var row = dataTable.NewRow();
        row["AliasPart_Alias"] = "my-alias";
        dataTable.Rows.Add(row);

        var context = new ContentPartImportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("AliasPart"),
            ContentItem = contentItem,
            Columns = dataTable.Columns,
            Row = row,
        };

        await _handler.ImportAsync(context);

        Assert.True(contentItem.TryGet<AliasPart>(out var part));
        Assert.Equal("my-alias", part.Alias);
    }

    [Fact]
    public async Task ExportAsync_WritesAliasToRow()
    {
        var contentItem = new ContentItem { ContentType = "Article" };
        contentItem.Alter<AliasPart>(p => p.Alias = "exported-alias");

        _handler.GetColumns(CreatePartContext("AliasPart"));

        var dataTable = new DataTable();
        dataTable.Columns.Add("AliasPart_Alias");
        var row = dataTable.NewRow();

        var exportContext = new ContentPartExportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("AliasPart"),
            ContentPart = contentItem.GetOrCreate<AliasPart>(),
            ContentItem = contentItem,
            Row = row,
        };

        await _handler.ExportAsync(exportContext);

        Assert.Equal("exported-alias", row["AliasPart_Alias"]?.ToString());
    }

    private static ImportContentPartContext CreatePartContext(string partName)
        => new()
        {
            ContentTypePartDefinition = CreateTypePartDefinition(partName),
        };

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

public class PublishLaterPartContentImportHandlerTests
{
    private readonly PublishLaterPartContentImportHandler _handler;

    public PublishLaterPartContentImportHandlerTests()
    {
        _handler = new PublishLaterPartContentImportHandler(Mock.Of<IStringLocalizer<PublishLaterPartContentImportHandler>>());
    }

    [Fact]
    public async Task ImportAsync_SetsScheduledPublishUtcOnPart()
    {
        var contentItem = new ContentItem { ContentType = "Article" };
        var dataTable = new DataTable();
        dataTable.Columns.Add("PublishLaterPart_ScheduledPublishUtc");
        var row = dataTable.NewRow();
        row["PublishLaterPart_ScheduledPublishUtc"] = "2026-03-23T10:30:00Z";
        dataTable.Rows.Add(row);

        var context = new ContentPartImportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("PublishLaterPart"),
            ContentItem = contentItem,
            Columns = dataTable.Columns,
            Row = row,
        };

        await _handler.ImportAsync(context);

        Assert.True(contentItem.TryGet<PublishLaterPart>(out var part));
        Assert.NotNull(part.ScheduledPublishUtc);
    }

    [Fact]
    public async Task ExportAsync_WritesScheduledPublishUtcToRow()
    {
        var contentItem = new ContentItem { ContentType = "Article" };
        var value = new DateTime(2026, 3, 23, 10, 30, 0, DateTimeKind.Utc);
        contentItem.Alter<PublishLaterPart>(p => p.ScheduledPublishUtc = value);

        _handler.GetColumns(CreatePartContext("PublishLaterPart"));

        var dataTable = new DataTable();
        dataTable.Columns.Add("PublishLaterPart_ScheduledPublishUtc");
        var row = dataTable.NewRow();

        var exportContext = new ContentPartExportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("PublishLaterPart"),
            ContentPart = contentItem.GetOrCreate<PublishLaterPart>(),
            ContentItem = contentItem,
            Row = row,
        };

        await _handler.ExportAsync(exportContext);

        Assert.Equal(value.ToString(), row["PublishLaterPart_ScheduledPublishUtc"]?.ToString());
    }

    private static ImportContentPartContext CreatePartContext(string partName)
        => new()
        {
            ContentTypePartDefinition = CreateTypePartDefinition(partName),
        };

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

public class MarkdownFieldImportHandlerTests
{
    private readonly MarkdownFieldImportHandler _handler;

    public MarkdownFieldImportHandlerTests()
    {
        _handler = new MarkdownFieldImportHandler(Mock.Of<IStringLocalizer<MarkdownFieldImportHandler>>());
    }

    [Fact]
    public async Task ImportAsync_SetsMarkdownFieldValue()
    {
        var part = new ContentPart();
        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_MyField_Markdown");
        var row = dataTable.NewRow();
        row["MyPart_MyField_Markdown"] = "## Markdown";
        dataTable.Rows.Add(row);

        await _handler.ImportAsync(CreateFieldImportContext("MyPart", "MyField", part, dataTable, row));

        var field = part.Get<MarkdownField>("MyField");
        Assert.NotNull(field);
        Assert.Equal("## Markdown", field.Markdown);
    }

    [Fact]
    public async Task ExportAsync_ReturnsMarkdownFieldValue()
    {
        var part = new ContentPart();
        part.Alter<MarkdownField>("MyField", f => f.Markdown = "## Exported Markdown");

        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_MyField_Markdown");
        var row = dataTable.NewRow();

        await _handler.ExportAsync(CreateFieldExportContext("MyPart", "MyField", part, row));

        Assert.Equal("## Exported Markdown", row["MyPart_MyField_Markdown"]?.ToString());
    }

    private static ContentFieldImportMapContext CreateFieldImportContext(string partName, string fieldName, ContentPart part, DataTable dataTable, DataRow row)
        => new()
        {
            PartName = partName,
            ContentPartFieldDefinition = new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(MarkdownField)), fieldName, []),
            ContentPart = part,
            ContentItem = new ContentItem(),
            Columns = dataTable.Columns,
            Row = row,
        };

    private static ContentFieldExportMapContext CreateFieldExportContext(string partName, string fieldName, ContentPart part, DataRow row)
        => new()
        {
            PartName = partName,
            ContentPartFieldDefinition = new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(MarkdownField)), fieldName, []),
            ContentPart = part,
            ContentField = part.Get<MarkdownField>(fieldName) ?? new MarkdownField(),
            ContentItem = new ContentItem(),
            Row = row,
        };
}

public class HtmlFieldImportHandlerTests
{
    private readonly HtmlFieldImportHandler _handler;

    public HtmlFieldImportHandlerTests()
    {
        _handler = new HtmlFieldImportHandler(Mock.Of<IStringLocalizer<HtmlFieldImportHandler>>());
    }

    [Fact]
    public async Task ImportAsync_SetsHtmlFieldValue()
    {
        var part = new ContentPart();
        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_MyField_Html");
        var row = dataTable.NewRow();
        row["MyPart_MyField_Html"] = "<p>Hello</p>";
        dataTable.Rows.Add(row);

        await _handler.ImportAsync(CreateFieldImportContext("MyPart", "MyField", part, dataTable, row));

        var field = part.Get<HtmlField>("MyField");
        Assert.NotNull(field);
        Assert.Equal("<p>Hello</p>", field.Html);
    }

    [Fact]
    public async Task ExportAsync_ReturnsHtmlFieldValue()
    {
        var part = new ContentPart();
        part.Alter<HtmlField>("MyField", f => f.Html = "<p>Exported</p>");

        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_MyField_Html");
        var row = dataTable.NewRow();

        await _handler.ExportAsync(CreateFieldExportContext("MyPart", "MyField", part, row));

        Assert.Equal("<p>Exported</p>", row["MyPart_MyField_Html"]?.ToString());
    }

    private static ContentFieldImportMapContext CreateFieldImportContext(string partName, string fieldName, ContentPart part, DataTable dataTable, DataRow row)
        => new()
        {
            PartName = partName,
            ContentPartFieldDefinition = new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(HtmlField)), fieldName, []),
            ContentPart = part,
            ContentItem = new ContentItem(),
            Columns = dataTable.Columns,
            Row = row,
        };

    private static ContentFieldExportMapContext CreateFieldExportContext(string partName, string fieldName, ContentPart part, DataRow row)
        => new()
        {
            PartName = partName,
            ContentPartFieldDefinition = new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(HtmlField)), fieldName, []),
            ContentPart = part,
            ContentField = part.Get<HtmlField>(fieldName) ?? new HtmlField(),
            ContentItem = new ContentItem(),
            Row = row,
        };
}

public class MultiTextFieldImportHandlerTests
{
    private readonly MultiTextFieldImportHandler _handler;

    public MultiTextFieldImportHandlerTests()
    {
        _handler = new MultiTextFieldImportHandler(Mock.Of<IStringLocalizer<MultiTextFieldImportHandler>>());
    }

    [Fact]
    public async Task ImportAsync_SplitsPipeSeparatedValues()
    {
        var part = new ContentPart();
        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_MyField_Values");
        var row = dataTable.NewRow();
        row["MyPart_MyField_Values"] = "One|Two|Three";
        dataTable.Rows.Add(row);

        await _handler.ImportAsync(CreateFieldImportContext("MyPart", "MyField", part, dataTable, row));

        var field = part.Get<MultiTextField>("MyField");
        Assert.NotNull(field);
        Assert.Equal(["One", "Two", "Three"], field.Values);
    }

    [Fact]
    public async Task ExportAsync_JoinsValuesWithPipe()
    {
        var part = new ContentPart();
        part.Alter<MultiTextField>("MyField", f => f.Values = ["One", "Two"]);

        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_MyField_Values");
        var row = dataTable.NewRow();

        await _handler.ExportAsync(CreateFieldExportContext("MyPart", "MyField", part, row));

        Assert.Equal("One|Two", row["MyPart_MyField_Values"]?.ToString());
    }

    private static ContentFieldImportMapContext CreateFieldImportContext(string partName, string fieldName, ContentPart part, DataTable dataTable, DataRow row)
        => new()
        {
            PartName = partName,
            ContentPartFieldDefinition = new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(MultiTextField)), fieldName, []),
            ContentPart = part,
            ContentItem = new ContentItem(),
            Columns = dataTable.Columns,
            Row = row,
        };

    private static ContentFieldExportMapContext CreateFieldExportContext(string partName, string fieldName, ContentPart part, DataRow row)
        => new()
        {
            PartName = partName,
            ContentPartFieldDefinition = new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(MultiTextField)), fieldName, []),
            ContentPart = part,
            ContentField = part.Get<MultiTextField>(fieldName) ?? new MultiTextField(),
            ContentItem = new ContentItem(),
            Row = row,
        };
}

public class MediaFieldImportHandlerTests
{
    private readonly MediaFieldImportHandler _handler;

    public MediaFieldImportHandlerTests()
    {
        _handler = new MediaFieldImportHandler(Mock.Of<IStringLocalizer<MediaFieldImportHandler>>());
    }

    [Fact]
    public void GetColumns_ReturnsExportOnlyPathsColumn()
    {
        var context = new ImportContentFieldContext
        {
            PartName = "GalleryPart",
            ContentPartFieldDefinition = new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(MediaField)), "Images", []),
            ContentPart = new ContentPart(),
            ContentItem = new ContentItem(),
        };

        var column = Assert.Single(_handler.GetColumns(context));
        Assert.Equal("GalleryPart_Images_Paths", column.Name);
        Assert.Equal(ImportColumnType.ExportOnly, column.Type);
    }

    [Fact]
    public async Task ImportAsync_DoesNothing()
    {
        var part = new ContentPart();
        var dataTable = new DataTable();
        dataTable.Columns.Add("GalleryPart_Images_Paths");
        var row = dataTable.NewRow();
        row["GalleryPart_Images_Paths"] = "media/a.jpg,media/b.jpg";
        dataTable.Rows.Add(row);

        await _handler.ImportAsync(new ContentFieldImportMapContext
        {
            PartName = "GalleryPart",
            ContentPartFieldDefinition = new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(MediaField)), "Images", []),
            ContentPart = part,
            ContentItem = new ContentItem(),
            Columns = dataTable.Columns,
            Row = row,
        });

        Assert.Null(part.Get<MediaField>("Images"));
    }

    [Fact]
    public async Task ExportAsync_JoinsMediaPathsWithComma()
    {
        var part = new ContentPart();
        part.Alter<MediaField>("Images", field => field.Paths = ["media/a.jpg", "media/b.jpg"]);

        var dataTable = new DataTable();
        dataTable.Columns.Add("GalleryPart_Images_Paths");
        var row = dataTable.NewRow();

        await _handler.ExportAsync(new ContentFieldExportMapContext
        {
            PartName = "GalleryPart",
            ContentPartFieldDefinition = new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(MediaField)), "Images", []),
            ContentPart = part,
            ContentField = part.Get<MediaField>("Images"),
            ContentItem = new ContentItem(),
            Row = row,
        });

        Assert.Equal("media/a.jpg,media/b.jpg", row["GalleryPart_Images_Paths"]?.ToString());
    }
}

public class ArchiveLaterPartContentImportHandlerTests
{
    private readonly ArchiveLaterPartContentImportHandler _handler;

    public ArchiveLaterPartContentImportHandlerTests()
    {
        _handler = new ArchiveLaterPartContentImportHandler(Mock.Of<IStringLocalizer<ArchiveLaterPartContentImportHandler>>());
    }

    [Fact]
    public async Task ImportAsync_SetsScheduledArchiveUtcOnPart()
    {
        var contentItem = new ContentItem { ContentType = "Article" };
        var dataTable = new DataTable();
        dataTable.Columns.Add("ArchiveLaterPart_ScheduledArchiveUtc");
        var row = dataTable.NewRow();
        row["ArchiveLaterPart_ScheduledArchiveUtc"] = "2026-03-24T10:30:00Z";
        dataTable.Rows.Add(row);

        await _handler.ImportAsync(new ContentPartImportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("ArchiveLaterPart"),
            ContentItem = contentItem,
            Columns = dataTable.Columns,
            Row = row,
        });

        Assert.True(contentItem.TryGet<ArchiveLaterPart>(out var part));
        Assert.NotNull(part.ScheduledArchiveUtc);
    }

    [Fact]
    public async Task ExportAsync_WritesScheduledArchiveUtcToRow()
    {
        var value = new DateTime(2026, 3, 24, 10, 30, 0, DateTimeKind.Utc);
        var contentItem = new ContentItem { ContentType = "Article" };
        contentItem.Alter<ArchiveLaterPart>(p => p.ScheduledArchiveUtc = value);

        _handler.GetColumns(new ImportContentPartContext { ContentTypePartDefinition = CreateTypePartDefinition("ArchiveLaterPart") });

        var dataTable = new DataTable();
        dataTable.Columns.Add("ArchiveLaterPart_ScheduledArchiveUtc");
        var row = dataTable.NewRow();

        await _handler.ExportAsync(new ContentPartExportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("ArchiveLaterPart"),
            ContentPart = contentItem.GetOrCreate<ArchiveLaterPart>(),
            ContentItem = contentItem,
            Row = row,
        });

        Assert.Equal(value.ToString(), row["ArchiveLaterPart_ScheduledArchiveUtc"]?.ToString());
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

public class LiquidPartContentImportHandlerTests
{
    private readonly LiquidPartContentImportHandler _handler;

    public LiquidPartContentImportHandlerTests()
    {
        _handler = new LiquidPartContentImportHandler(Mock.Of<IStringLocalizer<LiquidPartContentImportHandler>>());
    }

    [Fact]
    public async Task ImportAsync_SetsLiquidOnPart()
    {
        var contentItem = new ContentItem { ContentType = "Widget" };
        var dataTable = new DataTable();
        dataTable.Columns.Add("LiquidPart_Liquid");
        var row = dataTable.NewRow();
        row["LiquidPart_Liquid"] = "{{ Model.ContentItem.DisplayText }}";
        dataTable.Rows.Add(row);

        await _handler.ImportAsync(new ContentPartImportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("LiquidPart"),
            ContentItem = contentItem,
            Columns = dataTable.Columns,
            Row = row,
        });

        Assert.True(contentItem.TryGet<LiquidPart>(out var part));
        Assert.Equal("{{ Model.ContentItem.DisplayText }}", part.Liquid);
    }

    [Fact]
    public async Task ExportAsync_WritesLiquidToRow()
    {
        var contentItem = new ContentItem { ContentType = "Widget" };
        contentItem.Alter<LiquidPart>(p => p.Liquid = "{{ Model.ContentItem.DisplayText }}");

        _handler.GetColumns(new ImportContentPartContext { ContentTypePartDefinition = CreateTypePartDefinition("LiquidPart") });

        var dataTable = new DataTable();
        dataTable.Columns.Add("LiquidPart_Liquid");
        var row = dataTable.NewRow();

        await _handler.ExportAsync(new ContentPartExportMapContext
        {
            ContentTypePartDefinition = CreateTypePartDefinition("LiquidPart"),
            ContentPart = contentItem.GetOrCreate<LiquidPart>(),
            ContentItem = contentItem,
            Row = row,
        });

        Assert.Equal("{{ Model.ContentItem.DisplayText }}", row["LiquidPart_Liquid"]?.ToString());
    }

    private static ContentTypePartDefinition CreateTypePartDefinition(string partName)
    {
        var contentTypeDefinition = new ContentTypeDefinition("Widget", "Widget");
        var contentPartDefinition = new ContentPartDefinition(partName);

        return new ContentTypePartDefinition(partName, contentPartDefinition, [])
        {
            ContentTypeDefinition = contentTypeDefinition,
        };
    }
}

public class LinkFieldImportHandlerTests
{
    private readonly LinkFieldImportHandler _handler;

    public LinkFieldImportHandlerTests()
    {
        _handler = new LinkFieldImportHandler(Mock.Of<IStringLocalizer<LinkFieldImportHandler>>());
    }

    [Fact]
    public async Task ImportAsync_SetsAllLinkProperties()
    {
        var part = new ContentPart();
        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_MyField_Url");
        dataTable.Columns.Add("MyPart_MyField_Text");
        dataTable.Columns.Add("MyPart_MyField_Target");
        var row = dataTable.NewRow();
        row["MyPart_MyField_Url"] = "https://example.com";
        row["MyPart_MyField_Text"] = "Example";
        row["MyPart_MyField_Target"] = "_blank";
        dataTable.Rows.Add(row);

        await _handler.ImportAsync(new ContentFieldImportMapContext
        {
            PartName = "MyPart",
            ContentPartFieldDefinition = new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(LinkField)), "MyField", []),
            ContentPart = part,
            ContentItem = new ContentItem(),
            Columns = dataTable.Columns,
            Row = row,
        });

        var field = part.Get<LinkField>("MyField");
        Assert.NotNull(field);
        Assert.Equal("https://example.com", field.Url);
        Assert.Equal("Example", field.Text);
        Assert.Equal("_blank", field.Target);
    }

    [Fact]
    public async Task ExportAsync_WritesAllLinkProperties()
    {
        var part = new ContentPart();
        part.Alter<LinkField>("MyField", field =>
        {
            field.Url = "https://example.com";
            field.Text = "Example";
            field.Target = "_blank";
        });

        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_MyField_Url");
        dataTable.Columns.Add("MyPart_MyField_Text");
        dataTable.Columns.Add("MyPart_MyField_Target");
        var row = dataTable.NewRow();

        await _handler.ExportAsync(new ContentFieldExportMapContext
        {
            PartName = "MyPart",
            ContentPartFieldDefinition = new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(LinkField)), "MyField", []),
            ContentPart = part,
            ContentField = part.Get<LinkField>("MyField"),
            ContentItem = new ContentItem(),
            Row = row,
        });

        Assert.Equal("https://example.com", row["MyPart_MyField_Url"]?.ToString());
        Assert.Equal("Example", row["MyPart_MyField_Text"]?.ToString());
        Assert.Equal("_blank", row["MyPart_MyField_Target"]?.ToString());
    }
}

public class YoutubeFieldImportHandlerTests
{
    private readonly YoutubeFieldImportHandler _handler;

    public YoutubeFieldImportHandlerTests()
    {
        _handler = new YoutubeFieldImportHandler(Mock.Of<IStringLocalizer<YoutubeFieldImportHandler>>());
    }

    [Fact]
    public async Task ImportAsync_SetsYoutubeAddresses()
    {
        var part = new ContentPart();
        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_Video_RawAddress");
        dataTable.Columns.Add("MyPart_Video_EmbeddedAddress");
        var row = dataTable.NewRow();
        row["MyPart_Video_RawAddress"] = "https://youtu.be/test";
        row["MyPart_Video_EmbeddedAddress"] = "https://www.youtube.com/embed/test";
        dataTable.Rows.Add(row);

        await _handler.ImportAsync(new ContentFieldImportMapContext
        {
            PartName = "MyPart",
            ContentPartFieldDefinition = new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(YoutubeField)), "Video", []),
            ContentPart = part,
            ContentItem = new ContentItem(),
            Columns = dataTable.Columns,
            Row = row,
        });

        var field = part.Get<YoutubeField>("Video");
        Assert.NotNull(field);
        Assert.Equal("https://youtu.be/test", field.RawAddress);
        Assert.Equal("https://www.youtube.com/embed/test", field.EmbeddedAddress);
    }

    [Fact]
    public async Task ExportAsync_WritesYoutubeAddresses()
    {
        var part = new ContentPart();
        part.Alter<YoutubeField>("Video", field =>
        {
            field.RawAddress = "https://youtu.be/test";
            field.EmbeddedAddress = "https://www.youtube.com/embed/test";
        });

        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_Video_RawAddress");
        dataTable.Columns.Add("MyPart_Video_EmbeddedAddress");
        var row = dataTable.NewRow();

        await _handler.ExportAsync(new ContentFieldExportMapContext
        {
            PartName = "MyPart",
            ContentPartFieldDefinition = new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(YoutubeField)), "Video", []),
            ContentPart = part,
            ContentField = part.Get<YoutubeField>("Video"),
            ContentItem = new ContentItem(),
            Row = row,
        });

        Assert.Equal("https://youtu.be/test", row["MyPart_Video_RawAddress"]?.ToString());
        Assert.Equal("https://www.youtube.com/embed/test", row["MyPart_Video_EmbeddedAddress"]?.ToString());
    }
}

public class UserPickerFieldImportHandlerTests
{
    private readonly UserPickerFieldImportHandler _handler;

    public UserPickerFieldImportHandlerTests()
    {
        _handler = new UserPickerFieldImportHandler(Mock.Of<IStringLocalizer<UserPickerFieldImportHandler>>());
    }

    [Fact]
    public async Task ExportAsync_JoinsUserIdsWithComma()
    {
        var part = new ContentPart();
        part.Alter<UserPickerField>("Authors", field => field.UserIds = ["user1", "user2"]);

        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_Authors_UserIds");
        var row = dataTable.NewRow();

        await _handler.ExportAsync(new ContentFieldExportMapContext
        {
            PartName = "MyPart",
            ContentPartFieldDefinition = new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(UserPickerField)), "Authors", []),
            ContentPart = part,
            ContentField = part.Get<UserPickerField>("Authors"),
            ContentItem = new ContentItem(),
            Row = row,
        });

        Assert.Equal("user1,user2", row["MyPart_Authors_UserIds"]?.ToString());
    }
}

public class TaxonomyFieldImportHandlerTests
{
    private readonly TaxonomyFieldImportHandler _handler;

    public TaxonomyFieldImportHandlerTests()
    {
        _handler = new TaxonomyFieldImportHandler(Mock.Of<IStringLocalizer<TaxonomyFieldImportHandler>>());
    }

    [Fact]
    public async Task ExportAsync_WritesTaxonomyAndTermIds()
    {
        var part = new ContentPart();
        part.Alter<TaxonomyField>("Categories", field =>
        {
            field.TaxonomyContentItemId = "taxonomy-id";
            field.TermContentItemIds = ["term1", "term2"];
        });

        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_Categories_TaxonomyContentItemId");
        dataTable.Columns.Add("MyPart_Categories_TermContentItemIds");
        var row = dataTable.NewRow();

        await _handler.ExportAsync(new ContentFieldExportMapContext
        {
            PartName = "MyPart",
            ContentPartFieldDefinition = new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(TaxonomyField)), "Categories", []),
            ContentPart = part,
            ContentField = part.Get<TaxonomyField>("Categories"),
            ContentItem = new ContentItem(),
            Row = row,
        });

        Assert.Equal("taxonomy-id", row["MyPart_Categories_TaxonomyContentItemId"]?.ToString());
        Assert.Equal("term1,term2", row["MyPart_Categories_TermContentItemIds"]?.ToString());
    }
}

public class LocalizationSetContentPickerFieldImportHandlerTests
{
    private readonly LocalizationSetContentPickerFieldImportHandler _handler;

    public LocalizationSetContentPickerFieldImportHandlerTests()
    {
        _handler = new LocalizationSetContentPickerFieldImportHandler(Mock.Of<IStringLocalizer<LocalizationSetContentPickerFieldImportHandler>>());
    }

    [Fact]
    public async Task ExportAsync_JoinsLocalizationSetsWithComma()
    {
        var part = new ContentPart();
        part.Alter<LocalizationSetContentPickerField>("LocalizedItems", field => field.LocalizationSets = ["set1", "set2"]);

        var dataTable = new DataTable();
        dataTable.Columns.Add("MyPart_LocalizedItems_LocalizationSets");
        var row = dataTable.NewRow();

        await _handler.ExportAsync(new ContentFieldExportMapContext
        {
            PartName = "MyPart",
            ContentPartFieldDefinition = new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(LocalizationSetContentPickerField)), "LocalizedItems", []),
            ContentPart = part,
            ContentField = part.Get<LocalizationSetContentPickerField>("LocalizedItems"),
            ContentItem = new ContentItem(),
            Row = row,
        });

        Assert.Equal("set1,set2", row["MyPart_LocalizedItems_LocalizationSets"]?.ToString());
    }
}
