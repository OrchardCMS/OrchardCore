# Creating the pieces needed to query content based on assigned taxonomies

## What you will build

In this example, you will build a search tool to filter blog posts based on the assigned taxonomies. Of course, this does not need to be restricted to filtering blogs.

## What you will need

You will start from a new Orchard Core site built using the Blog recipe.

You will be using a Lucene query, made dynamic with a Liquid template.

You will use a Razor file to call this query.

## Setting things up

To make this sample a little more interesting, we're going to edit the BlogPost content type to allow multiple Category assignments.

Go to Content > Content Definition > Content Types and click to edit Blog Post. Then click Edit next to the "Category" Taxonomy field. Uncheck `Unique` and click `Save`. While we're here, since we want to make the 2 taxonomy fields (Category and Tags) searchable, you can do that now. Click to edit Category, then check the box for `Include this element in the index.`

Now do the same for the `Tags` field on the Blog Post content type.

Be sure to rebuild the index by navigating to `Search > Indexing > Lucene Indices` and then clicking "Rebuild."

Then navigate to Content > Content Types > Taxonomy and click to edit Categories. Then click Add Category to add "Politics". Then publish the taxonomy.

You will probably want to create a few more blog posts and assign various compbinations of Tags and Categories so you can try out your filter.

Start by adding a new Lucene query. Name it `GetBlogsByFilter.` You can leave the schema blank, leave the Index set to the default (`search`) and check the box to `Return Content Items.`

The query itself will use Liquid to:

-   check if we have a value for each of the filters
-   build the correct blocks based on the filter data

### A quick review

The Blog recipe creates 2 `Taxonomy` content types for us, `Categories` and `Tags.` What we are going to build will allow us to pass 0 or more Categories and 0 or more Tags and get back the set of BlogPosts that are connected to these filters.

### A filter object

A good way to model this filter in JSON is like this:

```
{
  categories: [...],
  tags: [...],
}
```

### A starting point query

```
{
	"size": 10,
	"query": {
 	  "term": { "Content.ContentItem.ContentType.keyword" : "BlogPost" }
	}
}
```

The query above will fetch us 10 BlogPosts, without using a filter. This is the query we will use if the filter we receive looks like this:

```
{
  categories: null,
  tags: null,
}
```

### A smarter query

To create that logic, we can do this:

```
{
	"size": 10,
	"query": {
{% if categories or tags %}

{% else %}
  	"term": { "Content.ContentItem.ContentType.keyword" : "BlogPost" }
{% endif %}
	}
}
```

Notice that the `if` fails, the `else` block runs and we get back the 10 BlogPosts.

### A functional query

To do the actual filter work, we'll make use of `bool`, `must` and `match`. `Match` will take care of allowing multiple terms to be joined together by an `OR`. And `Must` will manage the need for the 2 different taxonomies to be joined with an `AND`.

Consider this block:

```
"match": {
  "BlogPost.Category": {
    "query":"4..a 4..b",
    "operator": "or"
  }
}
```

It will bring us back BlogPost content items with the Category set to either "4..a" or "4..b".

To create the `AND` condition across taxonomy fields, we'll be wrapping the `match` blocks in a `must` block. We'll use Liquid to include the `match` block only when needed. And we'll also use Liquid to loop over the values being passed in the array.

```
"must": [
	{% if categories %}
		{
      "match":
      {
      	"BlogPost.Category":
        {
      	  "query":"{% for cat in categories %}{{cat}} {% endfor %}",
         	"operator": "or"
      	}
    	}
	  },
	{% endif %}
	{% if tags %}
		{
      "match":
      {
      	"BlogPost.Tags": {
      		"query":"{% for tag in tags %}{{tag}} {% endfor %}",
      		"operator": "or"
      	}
    	}
    },
	{% endif %}
```

Finally, gluing it all together into a usable query:

```
{
	"size": 10,
	"query": {
		{% if categories or tags %}
    "bool": {
			"must": [
				{% if categories %}
              		{
      					"match": {
      						"BlogPost.Category": {
      	  						"query": "{% for cat in categories %}{{cat}} {% endfor %}",
         						"operator": "or"
      						}
    					}
	  				},
				{% endif %}
				{% if tags %}
					{
      					"match": {
      						"BlogPost.Tags": {
      							"query":"{% for tag in tags %}{{tag}} {% endfor %}",
      							"operator": "or"
      						}
    					}
    				},
				{% endif %}
    		],
		}
    {% else %}
      "term": { "Content.ContentItem.ContentType.keyword" : "BlogPost" }
    {% endif %}
	}
}
```

## Using this Query in Code

This article is not intended to provide a complete implementation. However, as a simple example, you could do something like this in a View:

```
  var parameters = new Dictionary<string, object>();
  parameters.Add("categories", new string[] { "4pgm1krsy9nmyre41y5dj2p7df" });
  parameters.Add("tags", null);
  var posts = await Orchard.QueryAsync("GetBlogsByFilter", parameters);
  
  foreach (ContentItem post in posts)
  {
      await Orchard.DisplayAsync(post);
  }
```

## Summary

You just created the pieces needed to search through blogs to find only the ones with specific tags assigned.
