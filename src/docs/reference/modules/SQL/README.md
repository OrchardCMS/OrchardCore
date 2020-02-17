# Content Fields Indexing (`OrchardCore.ContentFields.Indexing.SQL`)

## Purpose

This module provides database indexing for content fields.

## Available Tables

* Note that types listed are SQL Server data types.

    *SQLite doesn't have a length limit on text fields.*

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
| **Url** | **nvarchar(4000)** | **false** | **false** |
| **Text** | **nvarchar(4000)** | **false** | **false** |

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
| **Text** | **nvarchar(4000)** | **false** | **false** |
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

From a Razor template.

```html
@using OrchardCore.ContentManagement
@using OrchardCore.ContentFields.Indexing
@inject ISession Session

@{
    var contentItems = await Session.Query<ContentItem, TextFieldIndex>(x => x.ContentType == "Acme" && x.ContentField == "Test").ListAsync();
}
```

From Liquid you will require to create a SQL Query in Orchard Core to retrieve these records first. Name it "AllCountries" for the current example and **don't select** the option "Return Documents" on the Query.

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