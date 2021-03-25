# Spatial (OrchardCore.Spatial)

This modules provides a GeoPoint content type which can be used to give a geographic position to content.

## Lucene Geo Queries
See https://www.elastic.co/guide/en/elasticsearch/reference/current/geo-queries.html for details.

### Geo Bounding Box

A query allowing to filter hits based on a point location using a bounding box.

Assuming a BlogPost content item has a `GeoPointField` named Location with the value `[Lat:-33, Long:138]`

```
{
    "query": {
        "bool" : {
            "must" : {
                "match_all" : {}
            },
            "filter" : {
                "geo_bounding_box" : {
                    "BlogPost.Location" : {
                        "top_left" : {
                            "lat" : -32,
                            "lon" : 137
                        },
                        "bottom_right" : {
                            "lat" : -34,
                            "lon" : 139
                        }
                    }
                }
            }
        }
    }
}

```

### Geo Distance

Filters documents that include only hits that exist within a specific distance from a geo point.

Assuming a BlogPost content item has a `GeoPointField` named Location with the value `[Lat:-33, Long:138]`

```
{
    "query": {
        "bool" : {
            "must" : {
                "match_all" : {}
            },
            "filter" : {
                "geo_distance" : {
                    "distance" : "200km",
                    "BlogPost.Location" : {
                        "lat" : -34,
                        "lon" : 138
                    }
                }
            }
        }
    }
}

```

Note: a 200km radius equates to approximately 1.7986 degrees of arc from the geo point centre. So searching
at `[-34.8, 138]` should be greater than 200km from the content location and not return it as a result. 
