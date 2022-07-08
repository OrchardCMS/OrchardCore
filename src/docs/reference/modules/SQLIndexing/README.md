# SQL Indexing

## Content Item Indexing

Here are some SQL tables that you can query and their columns.

### **ContentItemIndex**

| Name | Type | Non-Null | Primary Key |
| --- | --- | --- | --- |
| `Id` | `int` | `true` | `true` |
| `DocumentId` | `int` | `false` | `false` |
| `ContentItemId` | `nvarchar(26)` | `false` | `false` |
| `Published` | `bit` | `false` | `false` |
| `Latest` | `bit` | `false` | `false` |
| `ModifiedUtc` | `datetime` | `false` | `false` |
| `PublishedUtc` | `datetime` | `false` | `false` |
| `CreatedUtc` | `datetime` | `false` | `false` |
| `Owner` | `nvarchar(255)` | `false` | `false` |
| `Author` | `nvarchar(255)` | `false` | `false` |
| `DisplayText` | `nvarchar(255)` | `false` | `false` |

### **LocalizedContentItemIndex**

| Name | Type | Non-Null | Primary Key |
| --- | --- | --- | --- |
| `Id` | `int` | `true` | `true` |
| `DocumentId` | `int` | `false` | `false` |
| `ContentItemId` | `nvarchar(26)` | `false` | `false` |
| `Published` | `bit` | `false` | `false` |
| `Latest` | `bit` | `false` | `false` |
| **LocalizationSet** | **nvarchar** | **false** | **false** |
| **Culture** | **nvarchar** | **false** | **false** |

## Content Fields Indexing

The `OrchardCore.ContentFields.Indexing.SQL` module provides database indexing for content fields.

* Note that the listed types are SQL Server data types.

    *SQLite doesn't have a length limit on text fields.*

## Available Content Fields Tables

### **BooleanFieldIndex**

| Name | Type | Non-Null | Primary Key |
| --- | --- | --- | --- |
| `Id` | `int` | `true` | `true` |
| `DocumentId` | `int` | `false` | `false` |
| `ContentItemId` | `nvarchar(26)` | `false` | `false` |
| `ContentItemVersionId` | `nvarchar(26)` | `false` | `false` |
| `ContentType` | `nvarchar(255)` | `false` | `false` |
| `ContentPart` | `nvarchar(255)` | `false` | `false` |
| `ContentField` | `nvarchar(255)` | `false` | `false` |
| `Published` | `bit` | `false` | `false` |
| `Latest` | `bit` | `false` | `false` |
| **Boolean** | **bit** | **false** | **false** |

### **ContentPickerFieldIndex**

| Name | Type | Non-Null | Primary Key |
| --- | --- | --- | --- |
| `Id` | `int` | `true` | `true` |
| `DocumentId` | `int` | `false` | `false` |
| `ContentItemId` | `nvarchar(26)` | `false` | `false` |
| `ContentItemVersionId` | `nvarchar(26)` | `false` | `false` |
| `ContentType` | `nvarchar(255)` | `false` | `false` |
| `ContentPart` | `nvarchar(255)` | `false` | `false` |
| `ContentField` | `nvarchar(255)` | `false` | `false` |
| `Published` | `bit` | `false` | `false` |
| `Latest` | `bit` | `false` | `false` |
| **SelectedContentItemId** | **nvarchar(26)** | **false** | **false** |

### **DateFieldIndex**

| Name | Type | Non-Null | Primary Key |
| --- | --- | --- | --- |
| `Id` | `int` | `true` | `true` |
| `DocumentId` | `int` | `false` | `false` |
| `ContentItemId` | `nvarchar(26)` | `false` | `false` |
| `ContentItemVersionId` | `nvarchar(26)` | `false` | `false` |
| `ContentType` | `nvarchar(255)` | `false` | `false` |
| `ContentPart` | `nvarchar(255)` | `false` | `false` |
| `ContentField` | `nvarchar(255)` | `false` | `false` |
| `Published` | `bit` | `false` | `false` |
| `Latest` | `bit` | `false` | `false` |
| **Date** | **datetime** | **false** | **false** |

### **DateTimeFieldIndex**

| Name | Type | Non-Null | Primary Key |
| --- | --- | --- | --- |
| `Id` | `int` | `true` | `true` |
| `DocumentId` | `int` | `false` | `false` |
| `ContentItemId` | `nvarchar(26)` | `false` | `false` |
| `ContentItemVersionId` | `nvarchar(26)` | `false` | `false` |
| `ContentType` | `nvarchar(255)` | `false` | `false` |
| `ContentPart` | `nvarchar(255)` | `false` | `false` |
| `ContentField` | `nvarchar(255)` | `false` | `false` |
| `Published` | `bit` | `false` | `false` |
| `Latest` | `bit` | `false` | `false` |
| **DateTime** | **datetime** | **false** | **false** |

### **HtmlFieldIndex**

| Name | Type | Non-Null | Primary Key |
| --- | --- | --- | --- |
| `Id` | `int` | `true` | `true` |
| `DocumentId` | `int` | `false` | `false` |
| `ContentItemId` | `nvarchar(26)` | `false` | `false` |
| `ContentItemVersionId` | `nvarchar(26)` | `false` | `false` |
| `ContentType` | `nvarchar(255)` | `false` | `false` |
| `ContentPart` | `nvarchar(255)` | `false` | `false` |
| `ContentField` | `nvarchar(255)` | `false` | `false` |
| `Published` | `bit` | `false` | `false` |
| `Latest` | `bit` | `false` | `false` |
| **Html** | **nvarchar(max)** | **false** | **false** |

### **LinkFieldIndex**

| Name | Type | Non-Null | Primary Key |
| --- | --- | --- | --- |
| `Id` | `int` | `true` | `true` |
| `DocumentId` | `int` | `false` | `false` |
| `ContentItemId` | `nvarchar(26)` | `false` | `false` |
| `ContentItemVersionId` | `nvarchar(26)` | `false` | `false` |
| `ContentType` | `nvarchar(255)` | `false` | `false` |
| `ContentPart` | `nvarchar(255)` | `false` | `false` |
| `ContentField` | `nvarchar(255)` | `false` | `false` |
| `Published` | `bit` | `false` | `false` |
| `Latest` | `bit` | `false` | `false` |
| **Url** | **nvarchar(766)** | **false** | **false** |
| **BigUrl** | **nvarchar(max)** | **false** | **false** |
| **Text** | **nvarchar(766)** | **false** | **false** |
| **BigText** | **nvarchar(max)** | **false** | **false** |

### **MultiTextFieldIndex**

| Name | Type | Non-Null | Primary Key |
| --- | --- | --- | --- |
| `Id` | `Int` | `true` | `true` |
| `DocumentId` | `int` | `false` | `false` |
| `ContentItemId` | `nvarchar(26)` | `false` | `false` |
| `ContentItemVersionId` | `nvarchar(26)` | `false` | `false` |
| `ContentType` | `nvarchar(255)` | `false` | `false` |
| `ContentPart` | `nvarchar(255)` | `false` | `false` |
| `ContentField` | `nvarchar(255)` | `false` | `false` |
| `Published` | `bit` | `false` | `false` |
| `Latest` | `bit` | `false` | `false` |
| **Value** | **nvarchar(766)** | **false** | **false** |
| **BigValue** | **nvarchar(max)** | **false** | **false** |

### **NumericFieldIndex**

| Name | Type | Non-Null | Primary Key |
| --- | --- | --- | --- |
| `Id` | `int` | `true` | `true` |
| `DocumentId` | `int` | `false` | `false` |
| `ContentItemId` | `nvarchar(26)` | `false` | `false` |
| `ContentItemVersionId` | `nvarchar(26)` | `false` | `false` |
| `ContentType` | `nvarchar(255)` | `false` | `false` |
| `ContentPart` | `nvarchar(255)` | `false` | `false` |
| `ContentField` | `nvarchar(255)` | `false` | `false` |
| `Published` | `bit` | `false` | `false` |
| `Latest` | `bit` | `false` | `false` |
| **Numeric** | **decimal(19,5)** | **false** | **false** |

### **TextFieldIndex**

| Name | Type | Non-Null | Primary Key |
| --- | --- | --- | --- |
| `Id` | `Int` | `true` | `true` |
| `DocumentId` | `int` | `false` | `false` |
| `ContentItemId` | `nvarchar(26)` | `false` | `false` |
| `ContentItemVersionId` | `nvarchar(26)` | `false` | `false` |
| `ContentType` | `nvarchar(255)` | `false` | `false` |
| `ContentPart` | `nvarchar(255)` | `false` | `false` |
| `ContentField` | `nvarchar(255)` | `false` | `false` |
| `Published` | `bit` | `false` | `false` |
| `Latest` | `bit` | `false` | `false` |
| **Text** | **nvarchar(766)** | **false** | **false** |
| **BigText** | **nvarchar(max)** | **false** | **false** |

### **TimeFieldIndex**

| Name | Type | Non-Null | Primary Key |
| --- | --- | --- | --- |
| `Id` | `Int` | `true` | `true` |
| `DocumentId` | `int` | `false` | `false` |
| `ContentItemId` | `nvarchar(26)` | `false` | `false` |
| `ContentItemVersionId` | `nvarchar(26)` | `false` | `false` |
| `ContentType` | `nvarchar(255)` | `false` | `false` |
| `ContentPart` | `nvarchar(255)` | `false` | `false` |
| `ContentField` | `nvarchar(255)` | `false` | `false` |
| `Published` | `bit` | `false` | `false` |
| `Latest` | `bit` | `false` | `false` |
| **Time** | **datetime** | **false** | **false** |

### **UserPickerFieldIndex**

| Name | Type | Non-Null | Primary Key |
| --- | --- | --- | --- |
| `Id` | `Int` | `true` | `true` |
| `DocumentId` | `int` | `false` | `false` |
| `ContentItemId` | `nvarchar(26)` | `false` | `false` |
| `ContentItemVersionId` | `nvarchar(26)` | `false` | `false` |
| `ContentType` | `nvarchar(255)` | `false` | `false` |
| `ContentPart` | `nvarchar(255)` | `false` | `false` |
| `ContentField` | `nvarchar(255)` | `false` | `false` |
| `Published` | `bit` | `false` | `false` |
| `Latest` | `bit` | `false` | `false` |
| **SelectedUserId** | **string** | **false** | **false** |

## Usage

Please look at each index tables to see which fields are available to query on. The following examples are for the TextFieldIndex only.

From a class.

```csharp
using OrchardCore.ContentManagement;
using OrchardCore.ContentFields.Indexing

public class MyClass(){
    private readonly ISession _session;

        public MyClass(ISession session)
        {
            _session = session;
        }

        public async Task<IEnumerable<ContentItem>> GetTextFieldIndexRecords(string contentType, string contentField){
            return await _session.Query<ContentItem, TextFieldIndex>(x => x.ContentType == contentType && x.ContentField == contentField).ListAsync();
        }
}
```

From a Razor template:

```html
@using OrchardCore.ContentManagement
@using OrchardCore.ContentFields.Indexing
@inject ISession Session

@{
    var contentItems = await Session.Query<ContentItem, TextFieldIndex>(x => x.ContentType == "Acme" && x.ContentField == "Test").ListAsync();
}
```

From Liquid, you will be required to create a SQL Query in Orchard Core to retrieve these records first. Name it "AllCountries" for the current example and **don't select** the option "Return Documents" on the Query:

```SQL
SELECT * FROM TextFieldIndex
WHERE ContentType = 'Acme' AND ContentField = 'Country'
```

In our Liquid template we will now retrieve these records.

```liquid
{% assign allCountries = Queries.AllCountries | query %}
{% for country in allCountries %}
{{ country.Text }}
{% endfor %}
```

Please note that Datetimes are stored as UTC so a conversion with the current request culture will be required.
