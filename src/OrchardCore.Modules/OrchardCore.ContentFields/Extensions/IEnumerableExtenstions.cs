using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GraphQLParser;

namespace OrchardCore.ContentFields.Extentions;

public static class EnumerableExtentions
{

    public static IEnumerable<TSource[]> Chunk<TSource>(this IEnumerable<TSource> source, int size, int overlap)
    {
        if (size <= overlap)
        {
            throw new ArgumentOutOfRangeException(nameof(overlap));
        }

        TSource[] chunk = new TSource[size];
        int sourceIndex = 0, chunkIndex = 0;
        while (sourceIndex < source.Count())
        {
            chunk[chunkIndex++] = source.ElementAt(sourceIndex++);

            // When you have reached the chunk size or the end of the source, yield chunk and reset
            if (chunkIndex == size || sourceIndex == source.Count())
            {
                yield return chunk;
                chunk = new TSource[size];
                chunkIndex = 0;

                // If there is still more, move source index back by overlap
                if (sourceIndex < source.Count())
                {
                    sourceIndex -= overlap;
                }
            }
        }
    }

}